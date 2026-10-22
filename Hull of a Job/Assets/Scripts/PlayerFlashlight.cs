using UnityEngine;

namespace cowsins
{
    public class PlayerFlashlight : MonoBehaviour
    {
        [SerializeField] private InputManager inputManager;
        [SerializeField] private Light lightSource;
        [SerializeField] private AudioClip turnOnSFX, turnOffSFX;

        public bool TurnedOn => lightSource != null && lightSource.enabled;

        private void Awake()
        {
            if (lightSource != null) lightSource.enabled = false;
        }

        private void OnEnable()
        {
            if (inputManager != null) inputManager.OnMelee += Toggle;
        }

        private void OnDisable()
        {
            if (inputManager != null) inputManager.OnMelee -= Toggle;
        }

        private void Toggle()
        {
            if (lightSource == null) return;

            lightSource.enabled = !lightSource.enabled;

            AudioClip clip = lightSource.enabled ? turnOnSFX : turnOffSFX;
            if (clip != null) SoundManager.Instance.PlaySound(clip, 0, 0, true, 0.5f);
        }
    }
}