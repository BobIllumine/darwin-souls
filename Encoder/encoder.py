import torch
import torch.nn as nn
import torch.optim as optim
from torch.utils.data import DataLoader, TensorDataset

class Encoder(nn.Module):
    def __init__(self):
        super(Encoder, self).__init__()
        
        # Encoder with modified output to (1, 192)
        self.encoder = nn.Sequential(
            nn.Linear(9 * 29, 128),  # Input layer to hidden layer
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

class Decoder(nn.Module):
    def __init__(self):
        super(Decoder, self).__init__()
        
        # Encoder with modified output to (1, 192)
        self.decoder = nn.Sequential(
            nn.Linear(192, 128),     # Adjusted to match encoder output size
            nn.ReLU(True),
            nn.Linear(128, 9 * 29),  # Hidden layer to output layer
            nn.Sigmoid()     # Hidden layer to desired output size
        )

    def forward(self, x):
        x = self.decoder(x)
        return x
    
    def save_model(self, path):
        torch.save(self.state_dict(), path)
        print(f"Model saved to {path}")

    def load_model(self, path):
        self.load_state_dict(torch.load(path))
        print(f"Model loaded from {path}") 

class Autoencoder(nn.Module):
    def __init__(self):
        super(Autoencoder, self).__init__()
        
        # Use the Encoder class
        self.encoder = Encoder()
        
        # Decoder
        self.decoder = Decoder()

    def forward(self, x):
        x = self.encoder(x)
        x = self.decoder(x)
        return x

