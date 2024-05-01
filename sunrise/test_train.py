from mlagents_envs.environment import UnityEnvironment
from mlagents_envs.envs.unity_gym_env import UnityToGymWrapper
from mlagents_envs.envs.unity_aec_env import UnityAECEnv
from mlagents_envs.envs.unity_pettingzoo_base_env import UnityPettingzooBaseEnv
from mlagents_envs.logging_util import set_log_level
import logging
import matplotlib.pyplot as plt
from agent import Agent
from memory import ReplayMemory
import numpy as np
import os
import torch
import datetime
import pickle
import bz2
from tqdm import trange
# from test import ensemble_test
import argparse

set_log_level(logging.CRITICAL)
try:
    env.close()
except:
    pass

env = UnityEnvironment(file_name='./Build/darwin_souls.x86_64', no_graphics=False)
aec = UnityAECEnv(env)

num_cycles = 5

save_one = False
count = 0

aec.reset()
obs, _, _, _ = aec.last()
print(obs)
for agent in aec.agent_iter(aec.num_agents * num_cycles):
    # print(type(aec.action_space(agent).sample()))
    obs, reward, done, info = aec.last()
    # print(type(reward))
    # print(obs)
    if isinstance(obs, dict) and 'action_mask' in obs:
        for i, val in obs.items():
            # print(f'{i}: {len(val)}')
            for j in val:
                # print(j.shape)
                if save_one and count == 1:
                    continue
                if len(j.shape) == 3 and j.shape[1] == 45 and j.shape[2] == 80:
                    plt.imsave(fname=f'frame_{count}.png', arr=j[0], cmap='gray', pil_kwargs={'compress_level':0})
                    count += 1
                else:
                    ...
                    # print(j)

        # print(aec.observation_spaces[agent].sample())
        action_mask = obs['action_mask']
    if done:
        # print('we done here')
        action = None
    else:
        action = aec.action_spaces[agent].sample() # randomly choose an action for example
        # print(action)
    aec.step(action)

aec.close()
