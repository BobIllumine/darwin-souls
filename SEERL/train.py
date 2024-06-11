from mlagents_envs.envs.unity_aec_env import UnityAECEnv
from agent import Agent
from memory import ReplayMemory
import numpy as np
import os
import torch
from datetime import datetime
import pickle
import bz2
from tqdm import tqdm
from test import ensemble_test
import cvxpy as cp
import copy
import torch.nn.functional as F


def KL(state, skill, policy_1, policy_2):
    # Ensure the distributions are normalized (sum to 1)
    p = policy_1(state, skill)
    q = policy_2(state, skill)

    p = p.flatten(start_dim=1)
    q = q.flatten(start_dim=1)

    p = F.softmax(p, dim=-1)
    q = F.softmax(q, dim=-1)
    # Avoid division by zero and log of zero by adding a small epsilon
    epsilon = 1e-10
    p = p + epsilon
    q = q + epsilon
    
    p = p / p.sum(dim=-1, keepdim=True)
    q = q / q.sum(dim=-1, keepdim=True)
    # Calculate KL divergence
    kl_div = F.kl_div(p, q, reduction="batchmean", log_target=True)
    return kl_div

# Simple ISO 8601 timestamped logger
def log(s):
    print('[' + str(datetime.now().strftime('%Y-%m-%dT%H:%M:%S')) + '] ' + s)

def create_exp_folder(args):
    # exp name
    exp_name = args['id'] + '_' + str(args['num_ensemble']) + '/'
    exp_name += '/Beta_' + str(args['beta'])
    exp_name += '/seed_' + str(args['seed']) + '/'

    results_dir = os.path.join('./results', exp_name)
    if not os.path.exists(results_dir):
        os.makedirs(results_dir)
    
    return results_dir

def load_memory(memory_path, disable_bzip):
    if disable_bzip:
        with open(memory_path, 'rb') as pickle_file:
            return pickle.load(pickle_file)
    else:
        with bz2.open(memory_path, 'rb') as zipped_pickle_file:
            return pickle.load(zipped_pickle_file)


def save_memory(memory, memory_path, disable_bzip):
    if disable_bzip:
        with open(memory_path, 'wb') as pickle_file:
            pickle.dump(memory, pickle_file)
    else:
        with bz2.open(memory_path, 'wb') as zipped_pickle_file:
            pickle.dump(memory, zipped_pickle_file)

def setup_torch(args):
    np.random.seed(args['seed'])
    torch.manual_seed(np.random.randint(1, 10000))
    if torch.cuda.is_available() and not args['disable_cuda']:
        args['device'] = torch.device('cuda')
        torch.cuda.manual_seed(np.random.randint(1, 10000))
        torch.backends.cudnn.enabled = args['enable_cudnn']
    else:
        args['device'] = torch.device('cpu')

def construct_val_mem(aec, args):
    val_mem = { agent : ReplayMemory(args, args['evaluation_size']) for agent in aec.agents }
    done = True
    for agent in aec.agent_iter(aec.num_agents * args['evaluation_size']):
        if done:
            aec.reset()
            state, _, _, _ = aec.last()
            (state, skill) = state['observation'] if isinstance(state, dict) else state
            state = torch.Tensor(state).to(args['device']).flip(0)
            skill = torch.Tensor(skill).to(args['device']).flip(0)
            done = False
            
        aec.step(np.random.randint(0, aec.action_space(aec.agent_selection).n))
        next_state, _, done, _ = aec.last()
        (next_state, next_skill) = next_state['observation'] if isinstance(next_state, dict) else next_state
        next_state = torch.Tensor(next_state).to(args['device']).flip(0)
        next_skill = torch.Tensor(next_skill).to(args['device']).flip(0)
        val_mem[agent].append(state, skill, None, None, done)
        state = next_state
        skill = next_skill
    return val_mem


def start(aec, args):

    results_dir = create_exp_folder(args)
    
    metrics = {agent : {'steps': [], 'rewards': [], 'Qs': [], 'best_avg_reward': -float('inf')} for agent in aec.agents }

    setup_torch(args)
    # Agent
    dqn_list = { agent : [] for agent in aec.agents }
    for agent in aec.agents:
        for _ in range(args['num_ensemble']):
            dqn = Agent(args, aec, agent)
            dqn_list[agent].append(dqn)

    batch_list = { agent: [] for agent in aec.agents }
    policy_list = { agent: [] for agent in aec.agents }
    beta = args['beta']

    # If a model is provided, and evaluate is fale, presumably we want to resume, so try to load memory
    if args['model'] is not None and not args['evaluate']:
        if not args['memory']:
            raise ValueError('Cannot resume training without memory save path. Aborting...')
        elif not os.path.exists(args['memory']):
            raise ValueError('Could not find memory file at {path}. Aborting...'.format(path=args['memory']))
        mem = { agent : load_memory(args['memory'], args['disable_bzip_memory']) for agent in aec.agents }
    else:
        mem = { agent : ReplayMemory(args, args['memory_capacity']) for agent in aec.agents }

    priority_weight_increase = (1 - args['priority_weight']) / (args['T_max'] - args['learn_start'])

    # Construct validation memory
    val_mem = construct_val_mem(aec, args)

    if args['evaluate']:
        for agent in aec.agents:
            for en_index in range(args['num_ensemble']):
                dqn_list[agent][en_index].eval()
            # # KM: test code
            # avg_reward, avg_Q = ensemble_test(aec, agent, args, 0, dqn_list[agent], val_mem[agent], metrics, results_dir, 
            #                                 num_ensemble=args['num_ensemble'], evaluate=True)  # Test
            # print('Avg. reward: ' + str(avg_reward) + ' | Avg. Q: ' + str(avg_Q) + ' | Agent: ' + agent)
    else:
        # Training loop
        for agent in aec.agents:
            for en_index in range(args['num_ensemble']):
                dqn_list[agent][en_index].train()
        T, done = { agent: 0 for agent in aec.agents }, True
        selected_en_index = np.random.randint(args['num_ensemble'])
        prev_agent = None
        with tqdm(aec.agent_iter(int(aec.num_agents * args['T_max'] * 1.2)), postfix={'prev_agent': 'initial', 'T' : 0}) as iter:
            for agent in iter:
                if done:
                    aec.reset()
                    (state, _, _, info), done = aec.last(), False
                    (state, skill) = state['observation'] if isinstance(state, dict) else state
                    state = torch.Tensor(state).to(args['device']).flip(0)
                    skill = torch.Tensor(skill).to(args['device']).flip(0)
                    prev_agent = agent
                    selected_en_index = np.random.randint(args['num_ensemble'])

                iter.set_postfix(prev_agent=prev_agent.split('?')[0], T=T[prev_agent])
                if T[prev_agent] % args['replay_frequency'] == 0:
                    for en_index in range(args['num_ensemble']):
                        dqn_list[prev_agent][en_index].reset_noise()  # Draw a new set of noisy weights

                action = dqn_list[prev_agent][selected_en_index].act(state, skill)  # Choose an action greedily (with noisy weights)
                aec.step(action)
                next_state, reward, done, info = aec.last()
                (next_state, next_skill) = next_state['observation'] if isinstance(next_state, dict) else next_state
                next_state = torch.Tensor(next_state).to(args['device']).flip(0) # Step
                next_skill = torch.Tensor(next_skill).to(args['device']).flip(0) # Step

                if args['reward_clip'] > 0:
                    reward = max(min(reward, args['reward_clip']), -args['reward_clip'])  # Clip rewards
                mem[prev_agent].append(state, skill, action, reward, done)  # Append transition to memory

                # Train and test
                if T[prev_agent] >= args['learn_start']:
                    mem[prev_agent].priority_weight = min(mem[prev_agent].priority_weight + priority_weight_increase, 1)  # Anneal importance sampling weight β to 1
                    if T[prev_agent] % args['replay_frequency'] == 0:
                        loss, batch = dqn_list[prev_agent][selected_en_index].learn(mem[prev_agent])  # Train with n-step distributional double-Q learning
                        
                        if T[prev_agent] % args['save_state_every'] == 0:
                            batch_list[prev_agent].append(batch)

                        if T[prev_agent] % (np.ceil(args['evaluation_interval'] / args['num_policy'])) == 0:
                            policy_list[prev_agent].append(copy.deepcopy(dqn_list[prev_agent][selected_en_index]))
                            # print(len(policy_list), len(ps_list), len(state_list), len(loss_list))
                        
                    if T[prev_agent] % args['evaluation_interval'] == 0 and T[prev_agent] != args['learn_start']:
                        # TODO: Optimization framework here
                        for policy in policy_list[prev_agent]:
                            policy.eval()
                        # Calculate L(s, a) and KL divergences
                        state_list = [batch[1] for batch in batch_list[prev_agent]]
                        skill_list = [batch[2] for batch in batch_list[prev_agent]]
                        weight_list = [batch[-1] for batch in batch_list[prev_agent]]
                        # Convert state_probabilities to a numpy array

                        num_policies = len(policy_list[prev_agent])
                        num_states = len(state_list)
                        B = np.zeros((num_policies, num_states))
                        for s in range(num_states):
                            for i in range(num_policies):
                                kl_sum = 0
                                for k in range(num_policies):
                                    if i != k:
                                        kl_sum += KL(state_list[s], skill_list[s], policy_list[prev_agent][i], policy_list[prev_agent][k])
                                l_s = (weight_list[s] * policy_list[prev_agent][i].loss(*batch_list[prev_agent][s])[0]).mean()
                                B[i, s] = l_s - (beta / (num_policies - 1)) * kl_sum

                        probabilities = mem[prev_agent].get_batched_state_probabilities(state_list, skill_list).detach().cpu().numpy()
                        # Calculate the matrix B_ij using PyTorch
                        B_matrix = np.zeros((num_policies, num_policies))
                        for i in range(num_policies):
                            for j in range(num_policies):
                                B_matrix[i, j] = (probabilities * B[i] * B[j]).sum()
                        # Ensure B_matrix is symmetric
                        B_matrix = (B_matrix + B_matrix.T) / 2

                        # Add a small positive value to the diagonal elements to ensure positive semi-definiteness
                        epsilon = 1e-5
                        B_matrix += epsilon * np.eye(num_policies)

                        # Define the optimization variables
                        w = cp.Variable(num_policies)

                        # Define the objective function
                        objective = cp.Minimize(cp.quad_form(w, B_matrix))

                        # Define the constraints
                        constraints = [cp.sum(w) == 1, w >= 0]

                        problem = cp.Problem(objective, constraints)
                        problem.solve()
                        selection = [policy_list[prev_agent][i] for i in w.value.argsort()[-args['num_ensemble']:]]
                        # Don't change anything after this comment
                        for policy in selection:
                            policy.eval()  # Set DQN (online network) to evaluation mode
                        avg_reward, avg_Q = ensemble_test(aec, prev_agent, args, T[prev_agent], selection, val_mem[prev_agent], metrics, results_dir, args['num_ensemble'])  # Test
                        log_str = f"T = {T[prev_agent]}/{args['T_max']} | Avg. reward: {avg_reward} | Avg. Q: {avg_Q} | Agent: {prev_agent.split('?')[0]}"
                        log(log_str)
                        
                        for en_index in range(args['num_ensemble']):
                            dqn_list[prev_agent][en_index].train()  # Set DQN (online network) back to training mode

                        # If memory path provided, save it
                        if args['memory'] is not None:
                            save_memory(mem[prev_agent], args['memory'], args['disable_bzip_memory'])
                        for policy in policy_list[prev_agent]:
                            del policy
                        for batch in batch_list[prev_agent]:
                            del batch

                        policy_list[prev_agent].clear()
                        batch_list[prev_agent].clear()

                    # Update target network
                    if T[prev_agent] % args['target_update'] == 0:
                        for en_index in range(args['num_ensemble']):
                            dqn_list[prev_agent][en_index].update_target_net()

                    # Checkpoint the network
                    if (args['checkpoint_interval'] != 0) and (T[prev_agent] % args['checkpoint_interval'] == 0):
                        dqn_list[prev_agent][selected_en_index].save(results_dir, f'{prev_agent.split("?")[0]}_{selected_en_index}_checkpoint.pth')

                state = next_state
                skill = next_skill
                T[prev_agent] += 1
                prev_agent = agent

                count = 0
                for i in aec.agents:
                    if T[i] >= args['T_max']:
                        count += 1
                if count == aec.num_agents:
                    break

    aec.close()