from mlagents_envs.environment import UnityEnvironment
from mlagents_envs.envs.unity_gym_env import UnityToGymWrapper
from mlagents_envs.envs.unity_aec_env import UnityAECEnv
from mlagents_envs.envs.unity_pettingzoo_base_env import UnityPettingzooBaseEnv
from mlagents_envs.logging_util import set_log_level
import logging
import matplotlib.pyplot as plt
from encoder import Autoencoder, Encoder
import numpy as np
import os
import torch
import datetime
import pickle
import bz2
from tqdm import trange, tqdm
import argparse
import cvxpy as cp

set_log_level(logging.CRITICAL)
try:
    env.close()
    aec.close()
except:
    pass
env = UnityEnvironment(file_name='./Build/darwin_souls_sensor.x86_64', no_graphics=True)
aec = UnityAECEnv(env)

aec.reset()
with tqdm(aec.agent_iter(aec.num_agents * 10), postfix={'agent':'initial'}) as iter:
    for agent in iter:
        prev_observe, reward, done, info = aec.last()
        print(info)
        if isinstance(prev_observe, dict) and 'action_mask' in prev_observe:
            action_mask = prev_observe['action_mask']
            (state, skill) = prev_observe['observation']
        else:
            (state, skill) = prev_observe
        if done:
            action = None
        else:
            action = aec.action_spaces[agent].sample() # randomly choose an action for example
        print(agent, aec.agent_selection)
        aec.step(action)
        iter.set_postfix(agent=f'{agent}')
# # Note that hyperparameters may originally be reported in ATARI game frames instead of agent steps
# parser = argparse.ArgumentParser(description='Encoder')
# parser.add_argument('--id', type=str, default='encoder', help='Experiment ID')
# parser.add_argument('--disable-cuda', action='store_true', help='Disable CUDA')
# # parser.add_argument('--game', type=str, default='space_invaders', choices=atari_py.list_games(), help='ATARI game')
# parser.add_argument('--T-max', type=int, default=int(50e3), metavar='STEPS', help='Number of training steps (4x number of frames)')
# parser.add_argument('--enc-model', type=str, metavar='PARAMS', help='Pretrained model (state dict)')
# parser.add_argument('--dec-model', type=str, metavar='decPARAMS', help='Pretrained model (state dict)')
# parser.add_argument('--learning-rate', type=float, default=3e-4, metavar='η', help='Learning rate')
# parser.add_argument('--adam-eps', type=float, default=1.5e-4, metavar='ε', help='Adam epsilon')
# parser.add_argument('--batch-size', type=int, default=32, metavar='SIZE', help='Batch size')
# parser.add_argument('--evaluate', action='store_true', help='Evaluate only')
# # TODO: Note that DeepMind's evaluation method is running the latest agent for 500K frames ever every 1M steps
# parser.add_argument('--evaluation-size', type=int, default=500, metavar='N', help='Number of transitions to use for validating Q')
# # parser.add_argument('--render', action='store_true', help='Display screen (testing only)')
# parser.add_argument('--enable-cudnn', action='store_true', help='Enable cuDNN (faster but nondeterministic)')
# parser.add_argument('--checkpoint-interval', default=5e3, help='How often to checkpoint the model, defaults to 0 (never checkpoint)')
# parser.add_argument('--memory', help='Path to save/load the memory from')
# parser.add_argument('--seed', type=int, default=123)

# # Setup
# args = vars(parser.parse_args())
# np.random.seed(args['seed'])
# torch.manual_seed(np.random.randint(1, 10000))
# if torch.cuda.is_available() and not args['disable_cuda']:
#     args['device'] = torch.device('cuda')
#     torch.cuda.manual_seed(np.random.randint(1, 10000))
#     torch.backends.cudnn.enabled = args['enable_cudnn']
# else:
#     args['device'] = torch.device('cpu')


# results_dir = os.path.join('./results/encoders')
# if not os.path.exists(results_dir):
#     os.makedirs(results_dir)

# # Simple ISO 8601 timestamped logger
# def log(s):
#     print('[' + str(datetime.now().strftime('%Y-%m-%dT%H:%M:%S')) + '] ' + s)

# # Environment
# action_space = aec.action_space(aec.agent_selection).n

# ae = Autoencoder().to(args['device'])
# if args['enc_model']:
#     ae.encoder.load_model(args['enc_model'])
# if args['enc_model']:
#     ae.decoder.load_model(args['dec_model'])
# optimizer = torch.optim.Adam(ae.parameters(), lr=args['learning_rate'])
# criterion = torch.nn.MSELoss()


# if args['evaluate']:
#     ae.eval()
#     T, done = 0, True
#     losses = []
#     with torch.no_grad():
#         while T < args['evaluation_size']:
#             if done:
#                 aec.reset()
#                 state, _, _, _ = aec.last()
#                 state = torch.Tensor(state['observation'][1] if isinstance(state, dict) else state[1]).to(args['device'])
#                 state = state.view(state.size(0), -1)
#                 output = ae(state)
#                 loss = criterion(output, state)
#                 losses.append(loss.item())
#                 done = False
                
#             aec.step(np.random.randint(0, action_space))
#             next_state, _, done, _ = aec.last()
#             next_state = torch.Tensor(next_state['observation'][1] if isinstance(next_state, dict) else next_state[1]).to(args['device'])
#             state = next_state
#             state = state.view(state.size(0), -1)
#             output = ae(state)
#             loss = criterion(output, state)
#             losses.append(loss.item())
#             # print(f'henlo {T}')
#             T += 1
#     print(f'avg loss: {sum(losses) / len(losses)}')
# else:
#     # Training loop
#     T, done = 0, True
#     with trange(1, args['T_max'] + 1, postfix={'suffix':'initial'}) as bar:
#         for T in bar:
#             if done:
#                 aec.reset()
#                 (state, reward, _, info), done = aec.last(), False
#                 state = torch.Tensor(state['observation'][1] if isinstance(state, dict) else state[1]).to(args['device'])
#                 state = state.view(state.size(0), -1)
#                 output = ae(state)
#                 loss = criterion(output, state)
#                 bar.set_postfix(suffix=f'loss: {loss.item()}')
#                 optimizer.zero_grad()
#                 loss.backward()
#                 optimizer.step()
                

#             aec.step(np.random.randint(0, action_space))  # Choose an action greedily (with noisy weights)
#             next_state, reward, done, info = aec.last()
#             next_state = torch.Tensor(next_state['observation'][1] if isinstance(next_state, dict) else next_state[1]).to(args['device'])
#             state = next_state
#             state = state.view(state.size(0), -1)
#             output = ae(state)
#             loss = criterion(output, state)
#             bar.set_postfix(suffix=f'loss: {loss.item()}')
#             optimizer.zero_grad()
#             loss.backward()
#             optimizer.step()

#             if T % args['checkpoint_interval'] == 0:
#                 ae.encoder.save_model(f"results/encoders/encoder_{T / args['checkpoint_interval']}.pth")
#                 ae.decoder.save_model(f"results/encoders/decoder_{T / args['checkpoint_interval']}.pth")

aec.close()