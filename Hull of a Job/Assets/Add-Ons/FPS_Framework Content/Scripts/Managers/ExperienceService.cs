using UnityEngine;

namespace cowsins
{
    [System.Serializable]
    public class ExperienceService
    {
        private ProgressionManager manager;
        
        public int playerLevel { get; private set; }
        private float totalExperience;

        public float TotalExperience => totalExperience;

        public ExperienceService(ProgressionManager manager)
        {
            this.manager = manager;
        }

        // Add experience to the player
        public void AddExperience(float amount)
        {
            // Increase the players total experience
            totalExperience += amount;

            // Check if the player has leveled up
            CheckForLevelUp();
            manager.Events.OnExperienceCollected?.Invoke(true);
        }

        // remove experience from the player
        public void RemoveExperience(float amount)
        {
            // Reduce the player's total experience.
            totalExperience = Mathf.Max(totalExperience - amount, 0);

            // Check if the player has leveled down.
            CheckForLevelDown();
            manager.Events.OnExperienceCollected?.Invoke(true);
        }

        public void ResetExperience()
        {
            totalExperience = 0;
            manager.Events.OnExperienceCollected?.Invoke(false);
        }

        // Set the exact experience count
        public void SetExperience(float newAmount)
        {
            totalExperience = Mathf.Max(0, newAmount);
            CheckForLevelUp();
            CheckForLevelDown();
            manager.Events.OnExperienceCollected?.Invoke(true);
        }

        // check if the player has leveled up.
        private void CheckForLevelUp()
        {
            // While the player's level is less than the maximum level and their total experience is greater than the experience required for the next level, increase the player's level.
            while (playerLevel < manager.ExperienceRequirements.Length - 1 && totalExperience >= manager.ExperienceRequirements[playerLevel])
            {
                playerLevel++;
            }
        }

        // check if the player has leveled down.
        private void CheckForLevelDown()
        {
            // While the player's level is greater than the minimum level and their total experience is less than the experience required for the current level, decrease the player's level.
            while (playerLevel > 0 && totalExperience < manager.ExperienceRequirements[playerLevel])
            {
                playerLevel--;
            }
        }

        // get the player's level.
        public int GetPlayerLevel()
        {
            // Return the player's level plus one, since the level array starts at zero.
            return playerLevel + 1;
        }

        // get the player's current experience.
        public float GetCurrentExperience()
        {
            // Calculate the player's current experience by subtracting the experience required for the previous level from their total experience.
            float previousLevelExperience = playerLevel > 0 ? manager.ExperienceRequirements[playerLevel - 1] : 0;
            return totalExperience - previousLevelExperience;
        }
    }
}
