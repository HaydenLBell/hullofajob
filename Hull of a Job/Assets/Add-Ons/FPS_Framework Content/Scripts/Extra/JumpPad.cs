using UnityEngine;

namespace cowsins
{
    public class JumpPad : MonoBehaviour
    {
        public enum ForceModeType
        {
            PlayerOrientation,
            PlayerMovementDirection,
            JumpPadLocalAxis
        }

        [SerializeField] private ForceModeType forceModeType = ForceModeType.PlayerOrientation;

        [SerializeField] private float verticalForceMagnitude = 200f, horizontalForceMagnitude = 200;

        [SerializeField] private AudioClip jumpSound;



        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player") || !other.TryGetComponent<Rigidbody>(out Rigidbody playerRigidbody)) return;

            playerRigidbody.linearVelocity = Vector3.zero;

            if (other.TryGetComponent<PlayerMovement>(out PlayerMovement playerMovement))
            {
                playerMovement.movementContext.HasJumped = true;
            }

            other.TryGetComponent<IPlayerMovementStateProvider>(out IPlayerMovementStateProvider player);

            Vector3 hForceDirection = Vector3.zero;
            Vector3 vForceDirection = Vector3.zero;

            switch (forceModeType)
            {
                case ForceModeType.PlayerOrientation:
                    hForceDirection = player.Orientation.Forward;
                    vForceDirection = other.transform.up;
                    break;
                case ForceModeType.PlayerMovementDirection:
                    hForceDirection = playerRigidbody.linearVelocity.normalized;
                    vForceDirection = other.transform.up;
                    break;
                case ForceModeType.JumpPadLocalAxis:
                    hForceDirection = transform.right;
                    vForceDirection = transform.up;
                    break;
                default:
                    vForceDirection = other.transform.up;
                    break;
            }

            playerRigidbody.AddForce(vForceDirection * verticalForceMagnitude, ForceMode.Impulse);
            playerRigidbody.AddForce(hForceDirection * horizontalForceMagnitude, ForceMode.Impulse);

            if (jumpSound != null)
            {
                SoundManager.Instance.PlaySoundAtPosition(jumpSound, transform.position, 0f, 0f, false);
            }
        }
    }
}