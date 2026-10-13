using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace cowsins
{
    // Handles progression HUD elements such as Experience Bar, Levels and Coins
    public class ProgressionHUDModule : MonoBehaviour, IHUDModule
    {
        [Title("Experience")]
        [SerializeField] private Image xpImage;
        [SerializeField] private TextMeshProUGUI currentLevel, nextLevel;
        [SerializeField] private float lerpXpSpeed;
        [Tooltip("Color of screen flash effect when XP is collected"), SerializeField] private Color xpCollectColor = new Color(0, 0, 1, 0.2f); // Example default

        [Title("Coins")]
        [SerializeField] private GameObject coinsUI;
        [SerializeField] private TextMeshProUGUI coinsText;
        [Tooltip("Color of screen flash effect when coins are collected"), SerializeField] private Color coinCollectColor = new Color(1, 1, 0, 0.2f); // Example default

        private PlayerDependencies dependencies;
        
        [Title("Screen Flash")]
        [SerializeField] private ScreenFlashService flashService;

        public void Initialize(PlayerDependencies dependencies)
        {
            this.dependencies = dependencies;

            if (dependencies.ProgressionManager != null)
            {
                dependencies.ProgressionManager.Events.OnCoinsChange += UpdateCoins;
                dependencies.ProgressionManager.Events.OnExperienceCollected += UpdateXP;
                UpdateXP(false);
            }
        }

        private void OnDestroy()
        {
            if (dependencies != null && dependencies.ProgressionManager != null)
            {
                dependencies.ProgressionManager.Events.OnCoinsChange -= UpdateCoins;
                dependencies.ProgressionManager.Events.OnExperienceCollected -= UpdateXP;
            }
        }

        private void UpdateCoinsPanel() => flashService?.Flash(coinCollectColor);

        private void UpdateXPPanel() => flashService?.Flash(xpCollectColor);

        public void UpdateXP(bool updatePanel)
        {
            if (dependencies.ProgressionManager == null) return;

            int playerLevel = dependencies.ProgressionManager.experienceService.playerLevel;

            if (currentLevel != null) currentLevel.text = (playerLevel + 1).ToString();
            if (nextLevel != null) nextLevel.text = (playerLevel + 2).ToString();

            // Stop the previous fill coroutine to prevent overlap
            StopAllCoroutines(); 
            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(FillExperienceBar());
            }
            else if (xpImage != null)
            {
                xpImage.fillAmount = dependencies.ProgressionManager.experienceService.GetCurrentExperience() / dependencies.ProgressionManager.ExperienceRequirements[dependencies.ProgressionManager.experienceService.playerLevel];
            }

            if (updatePanel) UpdateXPPanel();
        }

        private void UpdateCoins(int amount, bool updateCoinsPanel)
        {
            if (dependencies.ProgressionManager == null) return;

            if (coinsText != null) coinsText.text = dependencies.ProgressionManager.coinService.coins.ToString();
            if (updateCoinsPanel) UpdateCoinsPanel();
        }

        private IEnumerator FillExperienceBar()
        {
            if (xpImage == null || dependencies.ProgressionManager == null) yield break;

            float targetXp = dependencies.ProgressionManager.experienceService.GetCurrentExperience() / dependencies.ProgressionManager.ExperienceRequirements[dependencies.ProgressionManager.experienceService.playerLevel];
            xpImage.fillAmount = 0; 
            while (xpImage.fillAmount < targetXp)
            {
                xpImage.fillAmount = Mathf.Lerp(xpImage.fillAmount, targetXp, lerpXpSpeed * Time.deltaTime);
                yield return null;
            }
        }
    }
}

#if UNITY_EDITOR
namespace cowsins { [UnityEditor.CustomEditor(typeof(ProgressionHUDModule))] public class ProgressionHUDModuleEditor : HUDModuleEditorBase { } }
#endif

