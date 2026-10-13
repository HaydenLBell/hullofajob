using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Text.RegularExpressions;

namespace cowsins
{
    // Handles interaction HUD elements, like prompts, Progress Bars, Forbidden Interaction...
    public class InteractionHUDModule : MonoBehaviour, IHUDModule
    {
        [Title("UI Settings")]
        [Tooltip("Attach the UI you want to use as your interaction UI"), SerializeField] private GameObject interactUI;
        [Tooltip("UI attached to the weapon, used for reloading or interaction background"), SerializeField] private RectTransform interactUIBackground;
        
        [Title("Audio")]
        [SerializeField] private AudioClip allowedInteractionSFX;
        
        [Title("Progress & Forbidden UI")]
        [Tooltip("Attach the UI you want to use as your progress display. NOTE: MUST BE AN IMAGE"), SerializeField] private Image interactUIProgressDisplay;
        [Tooltip("Attach the UI you want to use as your forbidden interaction UI"), SerializeField] private GameObject forbiddenInteractionUI;
        [Tooltip("Text that displays the action"), SerializeField] private TextMeshProUGUI interactText;

        private IInteractEventsProvider interactEvents;

        public void Initialize(PlayerDependencies dependencies)
        {
            interactEvents = dependencies.InteractEvents;

            if (interactUI != null) interactUI.SetActive(false);

            // Subscribing to Interaction Events
            interactEvents.Events.OnAllowedInteraction.AddListener(AllowedInteraction);
            interactEvents.Events.OnForbiddenInteraction.AddListener(ForbiddenInteraction);
            interactEvents.Events.OnDisableInteraction.AddListener(DisableInteractionUI);
            interactEvents.Events.OnInteractionProgressChanged.AddListener(InteractionProgressUpdate);
            interactEvents.Events.OnFinishInteraction.AddListener(FinishInteraction);
            interactEvents.Events.OnFinishInteraction.AddListener(DisableInteractionUI);
        }

        private void OnDestroy()
        {
            if (interactEvents == null) return;

            interactEvents.Events.OnAllowedInteraction.RemoveListener(AllowedInteraction);
            interactEvents.Events.OnForbiddenInteraction.RemoveListener(ForbiddenInteraction);
            interactEvents.Events.OnDisableInteraction.RemoveListener(DisableInteractionUI);
            interactEvents.Events.OnInteractionProgressChanged.RemoveListener(InteractionProgressUpdate);
            interactEvents.Events.OnFinishInteraction.RemoveListener(FinishInteraction);
            interactEvents.Events.OnFinishInteraction.RemoveListener(DisableInteractionUI);
        }

        private void AllowedInteraction(string displayText)
        {
            if (forbiddenInteractionUI != null) forbiddenInteractionUI.SetActive(false);

            if (interactUI != null)
            {
                interactUI.gameObject.SetActive(true);
                if (interactText != null) interactText.text = displayText;
                interactUI.GetComponent<Animation>().Play();

                // Adjust the width of the background based on the length of the displayText
                if (interactUIBackground != null)
                {
                    string plainText = Regex.Replace(displayText, "<.*?>", string.Empty);
                    float textLength = plainText.Length;
                    interactUIBackground.sizeDelta = new Vector2(100 + textLength * 10, interactUIBackground.sizeDelta.y);
                }
            }

            if (allowedInteractionSFX != null && SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySound(allowedInteractionSFX, 0, 0, false);
            }
        }

        private void ForbiddenInteraction()
        {
            if (forbiddenInteractionUI != null) forbiddenInteractionUI.SetActive(true);
            if (interactUI != null) interactUI.gameObject.SetActive(false);
        }

        private void DisableInteractionUI()
        {
            if (forbiddenInteractionUI != null) forbiddenInteractionUI.SetActive(false);
            if (interactUI != null) interactUI.gameObject.SetActive(false);
        }

        private void InteractionProgressUpdate(float value)
        {
            if (interactUIProgressDisplay != null)
            {
                interactUIProgressDisplay.gameObject.SetActive(true);
                interactUIProgressDisplay.fillAmount = value;
            }
        }

        private void FinishInteraction()
        {
            if (interactUIProgressDisplay != null) interactUIProgressDisplay.gameObject.SetActive(false);
        }
    }
}

#if UNITY_EDITOR
namespace cowsins { [UnityEditor.CustomEditor(typeof(InteractionHUDModule))] public class InteractionHUDModuleEditor : HUDModuleEditorBase { } }
#endif

