from agent import Agent
from memory import ReplayMemory
import numpy as np
import os
import torch
from datetime import datetime
import pickle
import bz2
from tqdm import tqdm
from test import test

def create_exp_folder(args):
    exp_name = args['id'] + '/'
    exp_name += '/seed_' + str(args['seed']) + '/'

    results_dir = os.path.join('./results', exp_name)
    if not os.path.exists(results_dir):
        os.makedirs(results_dir)
    return results_dir

def setup_torch(args):
    np.random.seed(args['seed'])
    torch.manual_seed(np.random.randint(1, 10000))
    if torch.cuda.is_available() and not args['disable_cuda']:
        args['device'] = torch.device('cuda')
        torch.cuda.manual_seed(np.random.randint(1, 10000))
        torch.backends.cudnn.enabled = args['enable_cudnn']
    else:
        args['device'] = torch.device('cpu')

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

# Simple ISO 8601 timestamped logger
def log(s):
    print('[' + str(datetime.now().strftime('%Y-%m-%dT%H:%M:%S')) + '] ' + s)

def start(aec, args):
    # exp name
    results_dir = create_exp_folder(args)

    metrics = { agent : {'steps': [], 'rewards': [], 'Qs': [], 'best_avg_reward': -float('inf')} for agent in aec.agents }
    
    setup_torch(args)

    # Agent
    dqn_list = { agent: None for agent in aec.agents }

    for agent in aec.agents:
        dqn_list[agent] = Agent(args, aec, agent)

    # If a model is provided, and evaluate is false, presumably we want to resume, so try to load memory
    if args['model'] is not None and not args['evaluate']:
        if not args['memory']:
            raise ValueError('Cannot resume training without memory save path. Aborting...')
        elif not os.path.exists(args['memory']):
            raise ValueError('Could not find memory file at {path}. Aborting...'.format(path=args['memory']))
        mem = {agent : load_memory(args['memory'], args['disable_bzip_memory']) for agent in aec.agents}
    else:
        mem = { agent : ReplayMemory(args, args['memory_capacity']) for agent in aec.agents }

    priority_weight_increase = (1 - args['priority_weight']) / (args['T_max'] - args['learn_start'])

    # Construct validation memory
    val_mem = construct_val_mem(aec, args)

    if args['evaluate']:
        for agent in aec.agents:
                dqn_list[agent].eval()
        # KM: test code
        avg_reward, avg_Q = test(aec, agent, args, 0, dqn_list[agent], val_mem[agent], metrics, results_dir, evaluate=True)  # Test
        print('Avg. reward: ' + str(avg_reward) + ' | Avg. Q: ' + str(avg_Q) + ' | Agent: ' + agent)
    else:
        # Training loop
        for agent in aec.agents:
            dqn_list[agent].train()
        T, done = { agent: 0 for agent in aec.agents }, True
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

                iter.set_postfix(prev_agent=prev_agent.split('?')[0], T=T[prev_agent])
                if T[prev_agent] % args['replay_frequency'] == 0:
                        dqn_list[prev_agent].reset_noise()  # Draw a new set of noisy weights
                
                action = dqn_list[agent].act(state, skill)
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
                        dqn_list[prev_agent].learn(mem[prev_agent])
                    
                    if T[prev_agent] % args['evaluation_interval'] == 0 and T[prev_agent] != args['learn_start']:
                        dqn_list[prev_agent].eval()  # Set DQN (online network) to evaluation mode
                        avg_reward, avg_Q = test(aec, prev_agent, args, T[prev_agent], dqn_list[prev_agent], val_mem[prev_agent], metrics, results_dir)  # Test
                        log(f'T[{prev_agent.split("?")[0]}] = ' + str(T[prev_agent]) + ' / ' + str(args['T_max']) + ' | Avg. reward: ' + str(avg_reward) + ' | Avg. Q: ' + str(avg_Q))
                        dqn_list[prev_agent].train()  # Set DQN (online network) back to training mode
                        # If memory path provided, save it
                        if args['memory'] is not None:
                            save_memory(mem[prev_agent], args['memory'], args['disable_bzip_memory'])

                    # Update target network
                    if T[prev_agent] % args['target_update'] == 0:
                        dqn_list[prev_agent].update_target_net()

                    # Checkpoint the network
                    if (args['checkpoint_interval'] != 0) and T[prev_agent] % args['checkpoint_interval'] == 0:
                        dqn_list[prev_agent].save(results_dir, f'{prev_agent.split("?")[0]}_checkpoint.pth')
                            
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