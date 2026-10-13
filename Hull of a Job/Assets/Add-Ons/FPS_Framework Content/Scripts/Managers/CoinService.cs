using UnityEngine;

namespace cowsins
{
    [System.Serializable]
    public class CoinService
    {
        private ProgressionManager manager;

        public int coins { get; private set; } // access the coin count

        public CoinService(ProgressionManager manager)
        {
            this.manager = manager;
            this.coins = manager.InitialCoins;
        }

        public void AddCoins(int amount) => AddCoins(amount, false);

        // Add coins to the total count
        public void AddCoins(int amount, bool updateCoinsPanel)
        {
            coins += Mathf.Abs(amount); // Add positive amount of coins
            manager.Events.OnCoinsChange?.Invoke(coins, updateCoinsPanel);
        }

        // Remove coins from the total count
        public void RemoveCoins(int amount, bool updateCoinsPanel)
        {
            coins -= Mathf.Abs(amount); // Reduce amount of coins

            if (coins <= 0) // Ensure coins dont go negative
            {
                coins = 0; // Set coins to zero if they go below
            }
            manager.Events.OnCoinsChange?.Invoke(coins, updateCoinsPanel);
        }

        // Check if there are enough coins
        public bool CheckIfEnoughCoins(int amount)
        {
            return coins >= amount; // Return whether there are enough coins
        }

        // Purchase action, checking and reducing coins if enough
        public bool CheckIfEnoughCoinsAndPurchase(int amount, bool updateCoinsPanel)
        {
            if (coins >= amount) // Check if enough coins are available
            {
                RemoveCoins(amount, updateCoinsPanel); // Reduce coins from the total
                return true; // Purchase successful
            }
            return false; // Not enough coins for purchase
        }

        // Reset the coin count to initial coins
        public void ResetCoins()
        {
            coins = manager.InitialCoins; // Set coins to initial coins
            manager.Events.OnCoinsChange?.Invoke(coins, false);
        }

        // Set the exact coin count
        public void SetCoins(int newAmount)
        {
            coins = Mathf.Max(0, newAmount);
            manager.Events.OnCoinsChange?.Invoke(coins, true);
        }
    }
}
