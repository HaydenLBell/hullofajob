using UnityEngine;
using UnityEngine.UI;

namespace cowsins
{
    public class StaminaHUDModule : MonoBehaviour, IHUDModule
    {
        [Tooltip("Our Slider UI Object. Stamina will be shown here.")]
        [SerializeField] private Slider staminaSlider;

        [Tooltip("If Stamina bar is filled up entirely, disable it. It will reappear as soon as you lose stamina as well")]
        [SerializeField] private bool staminaSliderFadesOutIfFull;

        private PlayerDependencies dependencies;

        public void Initialize(PlayerDependencies dependencies)
        {
            this.dependencies = dependencies;

            if (dependencies.PlayerMovementState != null && !dependencies.PlayerMovementState.UsesStamina)
            {
                if (staminaSlider != null) staminaSlider.gameObject.SetActive(false);
                return;
            }

            if (dependencies.PlayerMovementEvents != null)
                dependencies.PlayerMovementEvents.Events.OnStaminaChanged.AddListener(UpdateStaminaUI);
        }

        private void OnDestroy()
        {
            if (dependencies != null && dependencies.PlayerMovementEvents != null)
                dependencies.PlayerMovementEvents.Events.OnStaminaChanged.RemoveListener(UpdateStaminaUI);
        }

        private void UpdateStaminaUI(float currentStamina, float maxStamina)
        {
            if (staminaSlider == null) return;

            staminaSlider.maxValue = maxStamina;
            staminaSlider.value = currentStamina;

            if (staminaSliderFadesOutIfFull)
            {
                bool isFull = Mathf.Approximately(currentStamina, maxStamina) || (currentStamina >= maxStamina - 0.01f);
                staminaSlider.gameObject.SetActive(!isFull);
            }
        }
    }
}

#if UNITY_EDITOR
namespace cowsins { [UnityEditor.CustomEditor(typeof(StaminaHUDModule))] public class StaminaHUDModuleEditor : HUDModuleEditorBase { } }
#endif
