# -*- coding: utf-8 -*-
from __future__ import division
import math
import torch
from torch import nn
from torch.nn import functional as F


# Factorised NoisyLinear layer with bias
class NoisyLinear(nn.Module):
    def __init__(self, in_features, out_features, std_init=0.5):
        super(NoisyLinear, self).__init__()
        self.in_features = in_features
        self.out_features = out_features
        self.std_init = std_init
        self.weight_mu = nn.Parameter(torch.empty(out_features, in_features))
        self.weight_sigma = nn.Parameter(torch.empty(out_features, in_features))
        self.register_buffer('weight_epsilon', torch.empty(out_features, in_features))
        self.bias_mu = nn.Parameter(torch.empty(out_features))
        self.bias_sigma = nn.Parameter(torch.empty(out_features))
        self.register_buffer('bias_epsilon', torch.empty(out_features))
        self.reset_parameters()
        self.reset_noise()

    def reset_parameters(self):
        mu_range = 1 / math.sqrt(self.in_features)
        self.weight_mu.data.uniform_(-mu_range, mu_range)
        self.weight_sigma.data.fill_(self.std_init / math.sqrt(self.in_features))
        self.bias_mu.data.uniform_(-mu_range, mu_range)
        self.bias_sigma.data.fill_(self.std_init / math.sqrt(self.out_features))

    def _scale_noise(self, size):
        x = torch.randn(size)
        return x.sign().mul_(x.abs().sqrt_())

    def reset_noise(self):
        epsilon_in = self._scale_noise(self.in_features)
        epsilon_out = self._scale_noise(self.out_features)
        self.weight_epsilon.copy_(epsilon_out.ger(epsilon_in))
        self.bias_epsilon.copy_(epsilon_out)

    def forward(self, input):
        if self.training:
            return F.linear(input, self.weight_mu + self.weight_sigma * self.weight_epsilon, self.bias_mu + self.bias_sigma * self.bias_epsilon)
        else:
            return F.linear(input, self.weight_mu, self.bias_mu)
        
class Encoder(nn.Module):
    def __init__(self):
        super(Encoder, self).__init__()
        
        # Encoder with modified output to (1, 192)
        self.encoder = nn.Sequential(
            nn.Linear(4 * 9 * 29, 128),  # Input layer to hidden layer
            nn.ReLU(True),
            nn.Linear(128, 192)      # Hidden layer to desired output size
        )

    def forward(self, x):
        x = self.encoder(x)
        return x
    
    def save_model(self, path):
        torch.save(self.state_dict(), path)
        print(f"Model saved to {path}")

    def load_model(self, path):
        self.load_state_dict(torch.load(path))
        print(f"Model loaded from {path}")

class DQN(nn.Module):
    def __init__(self, args, action_space):
        super(DQN, self).__init__()
        self.atoms = args['atoms']
        self.action_space = action_space
        self.encoder = Encoder()
        if args['architecture'] == 'canonical':
            self.convs = nn.Sequential(nn.Conv2d(args['history_length'], 32, 8, stride=4, padding=0), nn.ReLU(),
                                    nn.Conv2d(32, 64, 4, stride=2, padding=0), nn.ReLU(),
                                    nn.Conv2d(64, 64, 3, stride=1, padding=0), nn.ReLU())
            self.conv_output_size = 768
        elif args['architecture'] == 'data-efficient':
            self.convs = nn.Sequential(nn.Conv2d(args['history_length'], 32, 5, stride=5, padding=0), nn.ReLU(),
                                    nn.Conv2d(32, 64, 5, stride=5, padding=0), nn.ReLU())
            self.conv_output_size = 192 * 2
        self.fc_h_v = NoisyLinear(self.conv_output_size, args['hidden_size'], std_init=args['noisy_std'])
        self.fc_h_a = NoisyLinear(self.conv_output_size, args['hidden_size'], std_init=args['noisy_std'])
        self.fc_z_v = NoisyLinear(args['hidden_size'], self.atoms, std_init=args['noisy_std'])
        self.fc_z_a = NoisyLinear(args['hidden_size'], action_space * self.atoms, std_init=args['noisy_std'])

    def forward(self, x, y, log=False):
        x = self.convs(x)
        x = x.view(-1, self.conv_output_size // 2)
        y = y.flatten(start_dim=1)
        y = self.encoder(y)
        x = torch.cat((x, y), dim=1)
        v = self.fc_z_v(F.relu(self.fc_h_v(x)))  # Value stream
        a = self.fc_z_a(F.relu(self.fc_h_a(x)))  # Advantage stream
        v, a = v.view(-1, 1, self.atoms), a.view(-1, self.action_space, self.atoms)
        q = v + a - a.mean(1, keepdim=True)  # Combine streams
        if log:  # Use log softmax for numerical stability
            q = F.log_softmax(q, dim=2)  # Log probabilities with action over second dimension
        else:
            q = F.softmax(q, dim=2)  # Probabilities with action over second dimension
        return q

    def reset_noise(self):
        for name, module in self.named_children():
            if 'fc' in name:
                module.reset_noise()
