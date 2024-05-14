# -*- coding: utf-8 -*-
from __future__ import division
import os
import plotly
from mlagents_envs.envs.unity_aec_env import UnityAECEnv
from plotly.graph_objs import Scatter
from plotly.graph_objs.scatter import Line
import torch
import numpy as np

# from env import Env

# Test DQN
def test(env: UnityAECEnv, args, T, dqn, val_mem, metrics, results_dir, evaluate=False):
    metrics['steps'].append(T)
    T_rewards, T_Qs = [], []

    # Test performance over several episodes
    done = True
    for _ in range(args['evaluation_episodes']):
        while True:
            if done:
                env.reset()
                state, reward_sum, done, _ = env.last()
                state = torch.Tensor(state['observation'][0] if isinstance(state, dict) else state).to(args['device'])
                reward_sum = 0
                done = False
            action = dqn.act_e_greedy(state)  # Choose an action ε-greedily
            env.step(action)
            state, reward, done, _ = env.last()  # Step
            state = torch.Tensor(state['observation'][0] if isinstance(state, dict) else state).to(args['device'])
            reward_sum += reward
            # if args['render']:
            #     env.render()
            if done:
                T_rewards.append(reward_sum)
                break
    # env.close()
    env.reset()

    # Test Q-values over validation memory
    for state in val_mem:  # Iterate over valid states
        T_Qs.append(dqn.evaluate_q(state))

    avg_reward, avg_Q = sum(T_rewards) / len(T_rewards), sum(T_Qs) / len(T_Qs)
    if not evaluate:
        # Save model parameters if improved
        if avg_reward > metrics['best_avg_reward']:
            metrics['best_avg_reward'] = avg_reward
            dqn.save(results_dir)

        # Append to results and save metrics
        metrics['rewards'].append(T_rewards)
        metrics['Qs'].append(T_Qs)
        torch.save(metrics, os.path.join(results_dir, 'metrics.pth'))

        # Plot
        _plot_line(metrics['steps'], metrics['rewards'], 'Reward', path=results_dir)
        _plot_line(metrics['steps'], metrics['Qs'], 'Q', path=results_dir)

    # Return average reward and Q-value
    return avg_reward, avg_Q

def ensemble_test(env: UnityAECEnv, args, T, dqn, val_mem, metrics, results_dir, num_ensemble, evaluate=False):
    metrics['steps'].append(T)
    T_rewards, T_Qs = [], []
    action_space = env.action_space(env.agent_selection).n
        
    # Test performance over several episodes
    done = True
    for _ in range(args['evaluation_episodes']):
        while True:
            if done:
                env.reset()
                state, reward_sum, done, _ = env.last()
                state = torch.Tensor(state['observation'][0] if isinstance(state, dict) else state).to(args['device'])
                reward_sum = 0
                done = False
            q_tot = 0
            for en_index in range(num_ensemble):
                if en_index == 0:
                    q_tot = dqn[en_index].ensemble_q(state)
                else:
                    q_tot += dqn[en_index].ensemble_q(state)
            action = q_tot.argmax(1).item()
                    
            env.step(action)
            state, reward, done, _ = env.last()  # Step
            state = torch.Tensor(state['observation'][0] if isinstance(state, dict) else state).to(args['device'])
            reward_sum += reward
            # if args['rsender']:
                # env.render()
            if done:
                T_rewards.append(reward_sum)
                break
    # env.close()
    env.reset()

    # Test Q-values over validation memory
    for state in val_mem:  # Iterate over valid states
        for en_index in range(num_ensemble):
            T_Qs.append(dqn[en_index].evaluate_q(state))
            
    avg_reward, avg_Q = sum(T_rewards) / len(T_rewards), sum(T_Qs) / len(T_Qs)
    if not evaluate:
        # Save model parameters if improved
        if avg_reward > metrics['best_avg_reward']:
            metrics['best_avg_reward'] = avg_reward
            for en_index in range(num_ensemble):
                dqn[en_index].save(results_dir, name='%dth_model.pth'%(en_index))
                
        # Append to results and save metrics
        metrics['rewards'].append(T_rewards)
        metrics['Qs'].append(T_Qs)
        
        torch.save(metrics, os.path.join(results_dir, 'metrics.pth'))
        # Plot
        _plot_line(metrics['steps'], metrics['rewards'], 'Reward', path=results_dir)
        _plot_line(metrics['steps'], metrics['Qs'], 'Q', path=results_dir)
            
    # Return average reward and Q-value
    return avg_reward, avg_Q


# Plots min, max and mean + standard deviation bars of a population over time
def _plot_line(xs, ys_population, title, path=''):
    max_colour, mean_colour, std_colour, transparent = 'rgb(0, 132, 180)', 'rgb(0, 172, 237)', 'rgba(29, 202, 255, 0.2)', 'rgba(0, 0, 0, 0)'

    ys = torch.tensor(ys_population, dtype=torch.float32)
    ys_min, ys_max, ys_mean, ys_std = ys.min(1)[0].squeeze(), ys.max(1)[0].squeeze(), ys.mean(1).squeeze(), ys.std(1).squeeze()
    ys_upper, ys_lower = ys_mean + ys_std, ys_mean - ys_std

    trace_max = Scatter(x=xs, y=ys_max.numpy(), line=Line(color=max_colour, dash='dash'), name='Max')
    trace_upper = Scatter(x=xs, y=ys_upper.numpy(), line=Line(color=transparent), name='+1 Std. Dev.', showlegend=False)
    trace_mean = Scatter(x=xs, y=ys_mean.numpy(), fill='tonexty', fillcolor=std_colour, line=Line(color=mean_colour), name='Mean')
    trace_lower = Scatter(x=xs, y=ys_lower.numpy(), fill='tonexty', fillcolor=std_colour, line=Line(color=transparent), name='-1 Std. Dev.', showlegend=False)
    trace_min = Scatter(x=xs, y=ys_min.numpy(), line=Line(color=max_colour, dash='dash'), name='Min')

    plotly.offline.plot({
        'data': [trace_upper, trace_mean, trace_lower, trace_min, trace_max],
        'layout': dict(title=title, xaxis={'title': 'Step'}, yaxis={'title': title})
    }, filename=os.path.join(path, title + '.html'), auto_open=False)