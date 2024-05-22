import torch
import torch.nn as nn
import torch.optim as optim
from torch.optim.lr_scheduler import _LRScheduler

class EnsembleCosineAnnealingLR(_LRScheduler):
    def __init__(self, optimizer, init_lr, T_max, num_models, last_epoch=0):
        self.init_lr = init_lr
        self.T_max = T_max
        self.num_models = num_models
        super(EnsembleCosineAnnealingLR, self).__init__(optimizer, last_epoch)

    def get_lr(self):
        # Calculate the current learning rate
        epochs = torch.ceil(self.T_max / self.num_models)
        return [
            (self.init_lr / 2) * (torch.cos(torch.pi * ((self.last_epoch - 1) % epochs) / epochs) + 1)
            for base_lr in self.base_lrs
        ]