// This script belongs to cowsins as a part of the cowsins FPS Engine. All rights reserved. 

using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace cowsins
{
    // HUD module responsible for displaying player health and shield
    public class HealthHUDModule : MonoBehaviour, IHUDModule
    {
        [Title("Display Mode")]
        [Tooltip("Use image bars to display player statistics."), SerializeField]
        private bool barHealthDisplay;

        [Tooltip("Use text to display player statistics."), SerializeField]
        private bool numericHealthDisplay;

        [Tooltip("If true, displays total health (health + shield) and hides shield UI elements"), SerializeField]
        private bool displayTotalHealth;

        [Title("Bar Display")]
        [Tooltip("Slider that will display the health on screen"), SerializeField]
        private Slider healthSlider;

        [Tooltip("Slider that will display the shield on screen"), SerializeField]
        private Slider shieldSlider;

        [Title("Numeric Display")]
        [SerializeField, Tooltip("UI Element ( TMPro text ) that displays current health.")]
        private TextMeshProUGUI healthTextDisplay;

        [SerializeField, Tooltip("UI Element ( TMPro text ) that displays current shield.")]
        private TextMeshProUGUI shieldTextDisplay;

        [SerializeField, Tooltip("UI Element ( TMPro text ) that displays maximum health.")]
        private TextMeshProUGUI maxHealthDisplay;

        [SerializeField, Tooltip("UI Element ( TMPro text ) that displays maximum shield.")]
        private TextMeshProUGUI maxShieldDisplay;

        [Title("Screen Flash")]
        [SerializeField] private ScreenFlashService screenFlash;

        [Tooltip("Color of screen flash when taking damage"), SerializeField]
        private Color damageColor;

        [Tooltip("Color of screen flash when healing"), SerializeField]
        private Color healColor;

        private IPlayerStatsEventsProvider playerStatsEvents;
        private Action<float, float> healthDisplayMethod;

        public void Initialize(PlayerDependencies dependencies)
        {
            playerStatsEvents = dependencies.PlayerStatsEvents;

            if (barHealthDisplay) healthDisplayMethod += BarHealthDisplayMethod;
            if (numericHealthDisplay) healthDisplayMethod += NumericHealthDisplayMethod;

            playerStatsEvents.Events.OnHealthChanged.AddListener(UpdateHealthUI);
            playerStatsEvents.Events.OnInitializeHealth.AddListener(InitializeHealthUI);

            var playerStats = dependencies.PlayerStats;
            if (playerStats != null && playerStats.MaxHealth > 0)
                InitializeHealthUI(playerStats);
        }

        private void OnDestroy()
        {
            if (playerStatsEvents?.Events == null) return;

            playerStatsEvents.Events.OnHealthChanged.RemoveListener(UpdateHealthUI);
            playerStatsEvents.Events.OnInitializeHealth.RemoveListener(InitializeHealthUI);
        }

        private void UpdateHealthUI(float health, float shield, bool damaged)
        {
            if (!gameObject.activeInHierarchy) return;

            healthDisplayMethod?.Invoke(health, shield);

            Color colorSelected = damaged ? damageColor : healColor;
            if (screenFlash != null) screenFlash.Flash(colorSelected);
        }

        private void InitializeHealthUI(IPlayerStatsProvider playerStats)
        {
            if (healthSlider != null) healthSlider.maxValue = playerStats.MaxHealth;
            if (shieldSlider != null) shieldSlider.maxValue = playerStats.MaxShield;

            if (displayTotalHealth && numericHealthDisplay)
            {
                if (shieldTextDisplay != null) shieldTextDisplay.gameObject.SetActive(false);
                if (maxShieldDisplay != null) maxShieldDisplay.gameObject.SetActive(false);
            }

            if (maxHealthDisplay != null) 
                maxHealthDisplay.text = displayTotalHealth ? (playerStats.MaxHealth + playerStats.MaxShield).ToString("F0") : playerStats.MaxHealth.ToString("F0");

            if (maxShieldDisplay != null && !displayTotalHealth) 
                maxShieldDisplay.text = playerStats.MaxShield.ToString("F0");
            
            healthDisplayMethod?.Invoke(playerStats.Health, playerStats.Shield);

            if (playerStats.MaxShield == 0 && shieldSlider != null) shieldSlider.gameObject.SetActive(false);
        }

        private void BarHealthDisplayMethod(float health, float shield)
        {
            if (healthSlider != null)
                healthSlider.value = health;

            if (shieldSlider != null)
                shieldSlider.value = shield;
        }

        private void NumericHealthDisplayMethod(float health, float shield)
        {
            if (displayTotalHealth)
            {
                float total = health + shield;
                if (healthTextDisplay != null) healthTextDisplay.text = total > 0 && total <= 1 ? 1.ToString("F0") : total.ToString("F0");
            }
            else
            {
                if (healthTextDisplay != null) healthTextDisplay.text = health > 0 && health <= 1 ? 1.ToString("F0") : health.ToString("F0");
                if (shieldTextDisplay != null) shieldTextDisplay.text = shield.ToString("F0");
            }
        }
    }
}

#if UNITY_EDITOR
namespace cowsins 
{
    [UnityEditor.CustomEditor(typeof(HealthHUDModule))]
    public class HealthHUDModuleEditor : HUDModuleEditorBase 
    {
        public override void DrawModuleProperties()
        {
            serializedObject.Update();

            UnityEditor.EditorGUILayout.Space();
            UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("barHealthDisplay"));
            UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("numericHealthDisplay"));
            UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("displayTotalHealth"));

            if (serializedObject.FindProperty("barHealthDisplay").boolValue)
            {
                UnityEditor.EditorGUILayout.Space();
                UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("healthSlider"));
                UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("shieldSlider"));
            }

            if (serializedObject.FindProperty("numericHealthDisplay").boolValue)
            {
                UnityEditor.EditorGUILayout.Space();
                UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("healthTextDisplay"));
                UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("shieldTextDisplay"));
                UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("maxHealthDisplay"));
                UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("maxShieldDisplay"));
            }

            UnityEditor.EditorGUILayout.Space();
            UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("screenFlash"));
            UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("damageColor"));
            UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("healColor"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
