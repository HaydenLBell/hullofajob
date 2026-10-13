using UnityEngine;
using System;

namespace cowsins
{
    public class ProgressionEvents
    {
        public Action<int, bool> OnCoinsChange;
        public Action<bool> OnExperienceCollected;
    }

    public class ProgressionManager : MonoBehaviour
    {
        [Tooltip("Flag to indicate if the game uses coins.")]
        [SerializeField] private bool useCoins;
        public bool UseCoins => useCoins;

        [Tooltip("Initial amount of coins the player starts with.")]
        [SerializeField] private int initialCoins = 0;
        public int InitialCoins => initialCoins;

        [Tooltip("Flag to indicate if the game uses experience.")]
        [SerializeField] private bool useExperience;
        public bool UseExperience => useExperience;

        [SerializeField] private int[] experienceRequirements;
        public int[] ExperienceRequirements => experienceRequirements;


        public CoinService coinService { get; private set; }
        public ExperienceService experienceService { get; private set; }

        public ProgressionEvents Events { get; private set; } = new ProgressionEvents();

        private void Awake()
        {
            coinService = new CoinService(this);
            experienceService = new ExperienceService(this);
        }

        private void Start()
        {
            Events.OnCoinsChange?.Invoke(coinService.coins, false);
        }
    }
}
