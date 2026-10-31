using UnityEngine;
using hb.framework;

namespace hb
{
    public class HoldSpin : MonoBehaviour
    {
        [Tooltip("The interactable the player holds (e.g. the wheel's MessInteractable).")]
        [SerializeField] private Interactable interactable;

        [Tooltip("The object that visually spins. Leave empty to spin this object.")]
        [SerializeField] private Transform wheel;

        [Tooltip("Local axis the wheel spins around.")]
        [SerializeField] private Vector3 spinAxis = Vector3.forward;

        [Tooltip("Degrees per second while holding.")]
        [SerializeField] private float spinSpeed = 180f;

        [Tooltip("Degrees per second when unwinding after letting go.")]
        [SerializeField] private float returnSpeed = 900f;

        private PlayerDependencies player;
        private Quaternion startRotation;
        private float currentAngle = 0f;

        private void Start()
        {
            if (wheel == null)
            {
                wheel = transform;
            }

            startRotation = wheel.localRotation;
            player = FindFirstObjectByType<PlayerDependencies>();
        }

        private void Update()
        {
            if (interactable == null)
            {
                return;
            }

            // Once the interaction has completed, leave the wheel where it is
            if (interactable.alreadyInteracted)
            {
                return;
            }

            if (IsBeingHeld())
            {
                currentAngle = currentAngle + spinSpeed * Time.deltaTime;
            }
            else
            {
                // Already at the start, so nothing to do this frame
                if (currentAngle == 0f)
                {
                    return;
                }

                currentAngle = Mathf.MoveTowards(currentAngle, 0f, returnSpeed * Time.deltaTime);
            }

            wheel.localRotation = startRotation * Quaternion.AngleAxis(currentAngle, spinAxis);
        }

        private bool IsBeingHeld()
        {
            if (interactable.IsHighlighted == false)
            {
                return false;
            }

            if (player == null || player.InputManager == null)
            {
                return false;
            }

            // Check this matches the interact property name in your InputManager
            return player.InputManager.Interacting;
        }
    }
}