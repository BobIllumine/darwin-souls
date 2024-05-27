import torch
import torch.nn as nn
import torch.optim as optim
from torch.optim.lr_scheduler import _LRScheduler
import math

class EnsembleCosineAnnealingLR(_LRScheduler):
    def __init__(self, optimizer, init_lr, T_max, num_models, last_epoch=0):
        self.init_lr = init_lr
        self.T_max = T_max
        self.num_models = num_models

        # Set the initial learning rate for each parameter group
        for param_group in optimizer.param_groups:
            if 'initial_lr' not in param_group:
                param_group['initial_lr'] = init_lr

        super(EnsembleCosineAnnealingLR, self).__init__(optimizer, last_epoch)

    def get_lr(self):
        # Calculate the current learning rate
        epochs = math.ceil(self.T_max / self.num_models)
        return [
            (self.init_lr / 2) * (math.cos(math.pi * ((self.last_epoch - 1) % epochs) / epochs) + 1)
            for base_lr in self.base_lrs
        ]