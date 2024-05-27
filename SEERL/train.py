from mlagents_envs.environment import UnityEnvironment
from mlagents_envs.envs.unity_gym_env import UnityToGymWrapper
from mlagents_envs.envs.unity_aec_env import UnityAECEnv
from mlagents_envs.envs.unity_pettingzoo_base_env import UnityPettingzooBaseEnv
from mlagents_envs.logging_util import set_log_level
import logging
import matplotlib.pyplot as plt
from agent import Agent, DQNPolicy
from memory import ReplayMemory
import numpy as np
import os
import torch
import datetime
import pickle
import bz2
from tqdm import trange
from test import ensemble_test
import argparse
import cvxpy as cp

set_log_level(logging.CRITICAL)
try:
    env.close()
except:
    pass

def KL(state, policy_1: DQNPolicy, policy_2: DQNPolicy):
    # Ensure the distributions are normalized (sum to 1)
    p = policy_1(state)
    q = policy_2(state)

    p = p / p.sum()
    q = q / q.sum()
    
    # Avoid division by zero and log of zero by adding a small epsilon
    epsilon = 1e-10
    p = p + epsilon
    q = q + epsilon
    
    # Calculate KL divergence
    kl_div = (p * torch.log(p / q)).sum()
    return kl_div


env = UnityEnvironment(file_name='./Build/darwin_souls.x86_64', no_graphics=True)
aec = UnityAECEnv(env)

# Note that hyperparameters may originally be reported in ATARI game frames instead of agent steps
parser = argparse.ArgumentParser(description='Rainbow')
parser.add_argument('--id', type=str, default='boot_rainbow', help='Experiment ID')
parser.add_argument('--seed', type=int, default=123, help='Random seed')
parser.add_argument('--disable-cuda', action='store_true', help='Disable CUDA')
# parser.add_argument('--game', type=str, default='space_invaders', choices=atari_py.list_games(), help='ATARI game')
parser.add_argument('--T-max', type=int, default=int(50e3), metavar='STEPS', help='Number of training steps (4x number of frames)')
parser.add_argument('--max-episode-length', type=int, default=int(108e3), metavar='LENGTH', help='Max episode length in game frames (0 to disable)')
parser.add_argument('--history-length', type=int, default=4, metavar='T', help='Number of consecutive states processed')
parser.add_argument('--architecture', type=str, default='canonical', choices=['canonical', 'data-efficient'], metavar='ARCH', help='Network architecture')
parser.add_argument('--hidden-size', type=int, default=512, metavar='SIZE', help='Network hidden size')
parser.add_argument('--noisy-std', type=float, default=0.1, metavar='σ', help='Initial standard deviation of noisy linear layers')
parser.add_argument('--atoms', type=int, default=51, metavar='C', help='Discretised size of value distribution')
parser.add_argument('--V-min', type=float, default=-10, metavar='V', help='Minimum of value distribution support')
parser.add_argument('--V-max', type=float, default=10, metavar='V', help='Maximum of value distribution support')
parser.add_argument('--model', type=str, metavar='PARAMS', help='Pretrained model (state dict)')
parser.add_argument('--memory-capacity', type=int, default=int(1e6), metavar='CAPACITY', help='Experience replay memory capacity')
parser.add_argument('--replay-frequency', type=int, default=4, metavar='k', help='Frequency of sampling from memory')
parser.add_argument('--priority-exponent', type=float, default=0.5, metavar='ω', help='Prioritised experience replay exponent (originally denoted α)')
parser.add_argument('--priority-weight', type=float, default=0.4, metavar='β', help='Initial prioritised experience replay importance sampling weight')
parser.add_argument('--multi-step', type=int, default=3, metavar='n', help='Number of steps for multi-step return')
parser.add_argument('--discount', type=float, default=0.99, metavar='γ', help='Discount factor')
parser.add_argument('--target-update', type=int, default=int(8e3), metavar='τ', help='Number of steps after which to update target network')
parser.add_argument('--reward-clip', type=int, default=1, metavar='VALUE', help='Reward clipping (0 to disable)')
parser.add_argument('--learning-rate', type=float, default=0.0000625, metavar='η', help='Learning rate')
parser.add_argument('--adam-eps', type=float, default=1.5e-4, metavar='ε', help='Adam epsilon')
parser.add_argument('--batch-size', type=int, default=32, metavar='SIZE', help='Batch size')
parser.add_argument('--learn-start', type=int, default=int(1e3), metavar='STEPS', help='Number of steps before starting training') 
parser.add_argument('--evaluate', action='store_true', help='Evaluate only')
parser.add_argument('--evaluation-interval', type=int, default=2e3, metavar='STEPS', help='Number of training steps between evaluations')
parser.add_argument('--evaluation-episodes', type=int, default=10, metavar='N', help='Number of evaluation episodes to average over')
# TODO: Note that DeepMind's evaluation method is running the latest agent for 500K frames ever every 1M steps
parser.add_argument('--evaluation-size', type=int, default=500, metavar='N', help='Number of transitions to use for validating Q')
# parser.add_argument('--render', action='store_true', help='Display screen (testing only)')
parser.add_argument('--enable-cudnn', action='store_true', help='Enable cuDNN (faster but nondeterministic)')
parser.add_argument('--checkpoint-interval', default=10e3, help='How often to checkpoint the model, defaults to 0 (never checkpoint)')
parser.add_argument('--memory', help='Path to save/load the memory from')
parser.add_argument('--disable-bzip-memory', action='store_true', help='Don\'t zip the memory file. Not recommended (zipping is a bit slower and much, much smaller)')
# ensemble
parser.add_argument('--num-policy', type=int, default=200, help='Number of policies')
parser.add_argument('--num-ensemble', type=int, default=3, metavar='N', help='Number of ensembles')
parser.add_argument('--beta-mean', type=float, default=1.0, help='mean of bernoulli')
parser.add_argument('--temperature', type=float, default=0.0, help='temperature for CF')
parser.add_argument('--ucb-infer', type=float, default=0.0, help='coeff for UCB infer')
parser.add_argument('--ucb-train', type=float, default=0.0, help='coeff for UCB train')
parser.add_argument('--beta', type=float, default=1.5, help='mean of bernoulli')

# Setup
args = vars(parser.parse_args())


# print(' ' * 26 + 'Options')
# for k, v in vars(args).items():
#     print(' ' * 26 + k + ': ' + str(v))
    
# exp name
exp_name = args['id'] + '_' + str(args['num_ensemble']) + '/'
exp_name += '/Beta_' + str(args['beta_mean']) + '_T_' + str(args['temperature'])
exp_name +='_UCB_I_' + str(args['ucb_infer']) + '_UCB_T_' + str(args['ucb_train']) + '/'
exp_name += '/seed_' + str(args['seed']) + '/'

results_dir = os.path.join('./results', exp_name)
if not os.path.exists(results_dir):
    os.makedirs(results_dir)

metrics = {'steps': [], 'rewards': [], 'Qs': [], 'best_avg_reward': -float('inf')}
np.random.seed(args['seed'])
torch.manual_seed(np.random.randint(1, 10000))
if torch.cuda.is_available() and not args['disable_cuda']:
    args['device'] = torch.device('cuda')
    torch.cuda.manual_seed(np.random.randint(1, 10000))
    torch.backends.cudnn.enabled = args['enable_cudnn']
else:
    args['device'] = torch.device('cpu')

# Simple ISO 8601 timestamped logger
def log(s):
    print('[' + str(datetime.now().strftime('%Y-%m-%dT%H:%M:%S')) + '] ' + s)

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

# Environment
action_space = aec.action_space(aec.agent_selection).n

# Agent
dqn_list = []
for _ in range(args['num_ensemble']):
    dqn = Agent(args, aec)
    dqn_list.append(dqn)

state_list = []
policy_list = []
loss_list = []
ps_list = []
beta = args['beta']


# If a model is provided, and evaluate is fale, presumably we want to resume, so try to load memory
if args['model'] is not None and not args['evaluate']:
    if not args['memory']:
        raise ValueError('Cannot resume training without memory save path. Aborting...')
    elif not os.path.exists(args['memory']):
        raise ValueError('Could not find memory file at {path}. Aborting...'.format(path=args['memory']))
    mem = load_memory(args['memory'], args['disable_bzip_memory'])
else:
    mem = ReplayMemory(args, args['memory_capacity'])

priority_weight_increase = (1 - args['priority_weight']) / (args['T_max'] - args['learn_start'])

# Construct validation memory
val_mem = ReplayMemory(args, args['evaluation_size'])
T, done = 0, True
while T < args['evaluation_size']:
    if done:
        aec.reset()
        state, _, _, _ = aec.last()
        state = torch.Tensor(state['observation'][0] if isinstance(state, dict) else state).to(args['device'])
        done = False
        
    aec.step(np.random.randint(0, action_space))
    next_state, _, done, _ = aec.last()
    
    next_state = torch.Tensor(next_state['observation'][0] if isinstance(next_state, dict) else next_state).to(args['device'])
    val_mem.append(state, None, None, done)
    state = next_state
    # print(f'henlo {T}')
    T += 1

if args['evaluate']:
    for en_index in range(args['num_ensemble']):
        dqn_list[en_index].eval()
    
    # KM: test code
    avg_reward, avg_Q = ensemble_test(aec, args, 0, dqn_list, val_mem, metrics, results_dir, 
                                      num_ensemble=args['num_ensemble'], evaluate=True)  # Test
    print('Avg. reward: ' + str(avg_reward) + ' | Avg. Q: ' + str(avg_Q))

else:
    # Training loop
    for en_index in range(args['num_ensemble']):
        dqn_list[en_index].train()
    T, done = 0, True
    selected_en_index = np.random.randint(args['num_ensemble'])

    for T in trange(1, args['T_max'] + 1):
        if done:
            aec.reset()
            (state, _, _, _), done = aec.last(), False
            state = torch.Tensor(state['observation'][0] if isinstance(state, dict) else next_state).to(args['device'])
            selected_en_index = np.random.randint(args['num_ensemble'])

        if T % args['replay_frequency'] == 0:
            dqn_list[selected_en_index].reset_noise()  # Draw a new set of noisy weights

        action = dqn_list[selected_en_index].act(state)  # Choose an action greedily (with noisy weights)
        aec.step(action)
        next_state, reward, done, _ = aec.last()  # Step
        next_state = torch.Tensor(next_state['observation'][0] if isinstance(next_state, dict) else next_state).to(args['device'])
        if args['reward_clip'] > 0:
            reward = max(min(reward, args['reward_clip']), -args['reward_clip'])  # Clip rewards
        mem.append(state, action, reward, done)  # Append transition to memory
        
        # Train and test
        if T >= args['learn_start']:
            mem.priority_weight = min(mem.priority_weight + priority_weight_increase, 1)  # Anneal importance sampling weight β to 1
            if T % args['replay_frequency'] == 0:
                loss, states = dqn_list[selected_en_index].learn(mem)  # Train with n-step distributional double-Q learning
                
                if T % (np.ceil(args['T_max'] / args['num_policy'])) == 0:
                    policy_list.append(dqn_list[selected_en_index].policy)
                    ps_list.append(loss[1])
                    state_list.append(states)
                    loss_list.append(loss[0])
                    # print(len(policy_list), len(ps_list), len(state_list), len(loss_list))
                
            if T % args['evaluation_interval'] == 0:
                state_list = []
                W = cp.Variable(len(policy_list))
                # Objective
                objective_terms = []
                for s in range(len(state_list)):
                    B = []
                    for i in range(policy_list):
                        kl_sum = cp.sum([KL(state_list[s], policy_list[i], policy_list[k]) for k in range(policy_list) if k != i])
                        B.append(loss_list[s, i] - beta / (len(policy_list) - 1) * kl_sum)
                    objective_terms.append(ps_list[s] * cp.square(cp.sum(cp.multiply(W, B))))

                objective = cp.Minimize(sum(objective_terms))
                # Constraints
                constraints = [
                    cp.sum(W) == 1,  # Sum of weights must be 1
                    W >= 0           # Weights must be non-negative
                ]
                problem = cp.Problem(objective, constraints)
                problem.solve()
                prob_dist = W.value
                print(prob_dist.argsort())
                selection = [policy_list[i] for i in prob_dist.argsort()[-args['num_ensemble']:]]
                # TODO: select 
                for en_index in range(args['num_ensemble']):
                    dqn_list[en_index].eval()  # Set DQN (online network) to evaluation mode
                # avg_reward, avg_Q = ensemble_test(args, T, dqn_list, val_mem, metrics, results_dir)  # Test
                log('T = ' + str(T) + ' / ' + str(args['T_max']) + ' | Avg. reward: ' + str(avg_reward) + ' | Avg. Q: ' + str(avg_Q))
                
                for en_index in range(args['num_ensemble']):
                    dqn_list[en_index].train()  # Set DQN (online network) back to training mode

                # If memory path provided, save it
                if args['memory'] is not None:
                    save_memory(mem, args['memory'], args['disable_bzip_memory'])

            # Update target network
            if T % args['target_update'] == 0:
                dqn.update_target_net()

            # Checkpoint the network
            if (args['checkpoint_interval'] != 0) and (T % args['checkpoint_interval'] == 0):
                dqn_list[selected_en_index].save(results_dir, 'checkpoint.pth')

        state = next_state

aec.close()