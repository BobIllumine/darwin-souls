# -*- coding: utf-8 -*-
from __future__ import division
from collections import namedtuple
import numpy as np
import torch


Transition = namedtuple('Transition', ('timestep', 'state', 'skill', 'action', 'reward', 'nonterminal'))
blank_trans = Transition(0, torch.zeros(45, 80, dtype=torch.uint8, device=torch.device('cuda')), torch.zeros(9, 29, dtype=torch.int16, device=torch.device('cuda')), None, 0, False)


# Segment tree data structure where parent node values are sum/max of children node values
class SegmentTree():
    def __init__(self, size):
        self.index = 0
        self.size = size
        self.full = False  # Used to track actual capacity
        self.sum_tree = np.zeros((2 * size - 1, ), dtype=np.float32)  # Initialise fixed size tree with all (priority) zeros
        self.data = np.array([None] * size)  # Wrap-around cyclic buffer
        self.max = 1  # Initial max value to return (1 = 1^ω)

    # Propagates value up tree given a tree index
    def _propagate(self, index, value):
        parent = (index - 1) // 2
        left, right = 2 * parent + 1, 2 * parent + 2
        self.sum_tree[parent] = self.sum_tree[left] + self.sum_tree[right]
        if parent != 0:
            self._propagate(parent, value)

    # Updates value given a tree index
    def update(self, index, value):
        self.sum_tree[index] = value  # Set new value
        self._propagate(index, value)  # Propagate value
        self.max = max(value, self.max)

    def append(self, data, value):
        self.data[self.index] = data  # Store data in underlying data structure
        self.update(self.index + self.size - 1, value)  # Update tree
        self.index = (self.index + 1) % self.size  # Update index
        self.full = self.full or self.index == 0  # Save when capacity reached
        self.max = max(value, self.max)

    # Searches for the location of a value in sum tree
    def _retrieve(self, index, value):
        left, right = 2 * index + 1, 2 * index + 2
        if left >= len(self.sum_tree):
            return index
        elif value <= self.sum_tree[left]:
            return self._retrieve(left, value)
        else:
            return self._retrieve(right, value - self.sum_tree[left])

    # Searches for a value in sum tree and returns value, data index and tree index
    def find(self, value):
        index = self._retrieve(0, value)  # Search for index of item from root
        data_index = index - self.size + 1
        return (self.sum_tree[index], data_index, index)  # Return value, data index, tree index

    # Returns data given a data index
    def get(self, data_index):
        return self.data[data_index % self.size]

    def total(self):
        return self.sum_tree[0]

class ReplayMemory():
    def __init__(self, args, capacity):
        self.device = args['device']
        self.capacity = capacity
        self.history = args['history_length']
        self.discount = args['discount']
        self.n = args['multi_step']
        self.priority_weight = args['priority_weight']  # Initial importance sampling weight β, annealed to 1 over course of training
        self.priority_exponent = args['priority_exponent']
        self.t = 0  # Internal episode timestep counter
        self.transitions = SegmentTree(capacity)  # Store transitions in a wrap-around cyclic buffer within a sum tree for querying priorities

    # Adds state and action at time t, reward and terminal at time t + 1
    def append(self, state, skill, action, reward, terminal):
        state = state[-1].mul(255).to(dtype=torch.uint8, device=self.device)  # Only store last frame and discretise to save memory
        skill = skill[-1].mul(65535).to(dtype=torch.int16, device=self.device)  # Only store last frame and discretise to save memory
        self.transitions.append(Transition(self.t, state, skill, action, reward, not terminal), self.transitions.max)  # Store new transition with maximum priority
        self.t = 0 if terminal else self.t + 1  # Start new episodes with t = 0

    # Returns a transition with blank states where appropriate
    def _get_transition(self, idx):
        transition = np.array([None] * (self.history + self.n))
        transition[self.history - 1] = self.transitions.get(idx)
        for t in range(self.history - 2, -1, -1):  # e.g. 2 1 0
            if transition[t + 1].timestep == 0:
                transition[t] = blank_trans  # If future frame has timestep 0
            else:
                transition[t] = self.transitions.get(idx - self.history + 1 + t)
        for t in range(self.history, self.history + self.n):  # e.g. 4 5 6
            if transition[t - 1].nonterminal:
                transition[t] = self.transitions.get(idx - self.history + 1 + t)
            else:
                transition[t] = blank_trans  # If prev (next) frame is terminal
        return transition

    # Returns a valid sample from a segment
    def _get_sample_from_segment(self, segment, i):
        valid = False
        while not valid:
            sample = np.random.uniform(i * segment, (i + 1) * segment)  # Uniformly sample an element from within a segment
            prob, idx, tree_idx = self.transitions.find(sample)  # Retrieve sample from tree with un-normalised probability
            # Resample if transition straddled current index or probablity 0
            if (self.transitions.index - idx) % self.capacity > self.n and (idx - self.transitions.index) % self.capacity >= self.history and prob != 0:
                valid = True  # Note that conditions are valid but extra conservative around buffer index 0

        # Retrieve all required transition data (from t - h to t + n)
        transition = self._get_transition(idx)
        # Create un-discretised state and nth next state
        state = torch.stack([trans.state for trans in transition[:self.history]]).to(device=self.device).to(dtype=torch.float32).div_(255)
        skill = torch.stack([trans.skill for trans in transition[:self.history]]).to(device=self.device).to(dtype=torch.float32).div_(255)
        next_state = torch.stack([trans.state for trans in transition[self.n:self.n + self.history]]).to(device=self.device).to(dtype=torch.float32).div_(255)
        next_skill = torch.stack([trans.skill for trans in transition[self.n:self.n + self.history]]).to(device=self.device).to(dtype=torch.float32).div_(255)
        # Discrete action to be used as index
        action = torch.tensor([transition[self.history - 1].action], dtype=torch.int64, device=self.device)
        # Calculate truncated n-step discounted return R^n = Σ_k=0->n-1 (γ^k)R_t+k+1 (note that invalid nth next states have reward 0)
        R = torch.tensor([sum(self.discount ** n * transition[self.history + n - 1].reward for n in range(self.n))], dtype=torch.float32, device=self.device)
        # Mask for non-terminal nth next states
        nonterminal = torch.tensor([transition[self.history + self.n - 1].nonterminal], dtype=torch.float32, device=self.device)

        return prob, idx, tree_idx, state, skill, action, R, next_state, next_skill, nonterminal

    def sample(self, batch_size):
        p_total = self.transitions.total()  # Retrieve sum of all priorities (used to create a normalised probability distribution)
        segment = p_total / batch_size  # Batch size number of segments, based on sum over all probabilities
        batch = [self._get_sample_from_segment(segment, i) for i in range(batch_size)]  # Get batch of valid samples
        probs, idxs, tree_idxs, states, skills, actions, returns, next_states, next_skills, nonterminals = zip(*batch)
        states, next_states, = torch.stack(states), torch.stack(next_states)
        skills, next_skills = torch.stack(skills), torch.stack(next_skills)
        actions, returns, nonterminals = torch.cat(actions), torch.cat(returns), torch.stack(nonterminals)
        probs = np.array(probs, dtype=np.float32) / p_total  # Calculate normalised probabilities
        capacity = self.capacity if self.transitions.full else self.transitions.index
        weights = (capacity * probs) ** -self.priority_weight  # Compute importance-sampling weights w
        weights = torch.tensor(weights / weights.max(), dtype=torch.float32, device=self.device)  # Normalise by max importance-sampling weight from batch
        return tree_idxs, states, skills, actions, returns, next_states, next_skills, nonterminals, weights


    def update_priorities(self, idxs, priorities):
        priorities = np.power(priorities, self.priority_exponent)
        [self.transitions.update(idx, priority) for idx, priority in zip(idxs, priorities)]

    # Set up internal state for iterator
    def __iter__(self):
        self.current_idx = 0
        return self

    # Return valid states for validation
    def __next__(self):
        if self.current_idx == self.capacity:
            raise StopIteration
        # Create stack of states
        state_stack = [None] * self.history
        skill_stack = [None] * self.history
        state_stack[-1] = self.transitions.data[self.current_idx].state
        skill_stack[-1] = self.transitions.data[self.current_idx].skill
        prev_timestep = self.transitions.data[self.current_idx].timestep
        for t in reversed(range(self.history - 1)):
            if prev_timestep == 0:
                state_stack[t] = blank_trans.state  # If future frame has timestep 0
                skill_stack[t] = blank_trans.skill  # If future frame has timestep 0
            else:
                state_stack[t] = self.transitions.data[self.current_idx + t - self.history + 1].state
                skill_stack[t] = self.transitions.data[self.current_idx + t - self.history + 1].skill
                prev_timestep -= 1
        state = torch.stack(state_stack, 0).to(dtype=torch.float32, device=self.device).div_(255)  # Agent will turn into batch
        skill = torch.stack(skill_stack, 0).to(dtype=torch.float32, device=self.device).div_(65535)  # Agent will turn into batch
        self.current_idx += 1
        return state, skill
    
    def get_priorities(self):
        # Extract the priorities from the leaf nodes of the segment tree
        priorities = self.transitions.sum_tree[self.transitions.size - 1:]
        return priorities

    def get_probability_distribution(self):
        # Normalize the priorities to form a probability distribution
        priorities = self.get_priorities()
        probabilities = priorities / np.sum(priorities)
        return probabilities

    def get_batched_state_probabilities(self, states, skills):
        """
        Get the probabilities for a batch of states and skills.

        Parameters:
        states (torch.Tensor): Batch of states.
        skills (torch.Tensor): Batch of skills.

        Returns:
        torch.Tensor: Batch of probabilities corresponding to each state and skill pair.
        """
        # Convert states and skills to a comparable format
        states_flat = [tuple(state.cpu().numpy().flatten()) for state in states]
        skills_flat = [tuple(skill.cpu().numpy().flatten()) for skill in skills]
        
        # Get the list of states and corresponding probabilities
        probabilities = self.get_probability_distribution()
        states_list = []
        skills_list = []

        for i in range(len(probabilities)):
            value, data_idx, idx = self.transitions.find(i)
            item = self.transitions.get(data_idx)
            if item is not None:
                states_list.append(item.state)
                skills_list.append(item.skill)

        # Convert states and skills in the transitions to comparable format
        states_list_flat = [tuple(s.cpu().numpy().flatten()) for s in states_list]
        skills_list_flat = [tuple(sk.cpu().numpy().flatten()) for sk in skills_list]

        # Find the indices of the given states and skills
        state_probabilities = []
        for state_flat, skill_flat in zip(states_flat, skills_flat):
            probability = 0.0
            for idx, (s, sk) in enumerate(zip(states_list_flat, skills_list_flat)):
                if s == state_flat and sk == skill_flat:
                    probability = probabilities[idx]
                    break
            state_probabilities.append(probability)

        state_probabilities = torch.tensor(state_probabilities).to(self.device)
        remaining = 1 - state_probabilities.sum().item()
        zeros = state_probabilities.size(0) - state_probabilities.count_nonzero().item()
        state_probabilities[state_probabilities == 0] = remaining / zeros
        state_probabilities /= state_probabilities.sum()
        return state_probabilities

    next = __next__  # Alias __next__ for Python 2 compatibility