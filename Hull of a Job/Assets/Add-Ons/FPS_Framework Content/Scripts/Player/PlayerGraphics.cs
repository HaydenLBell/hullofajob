using UnityEngine;
namespace cowsins
{
    public class PlayerGraphics : MonoBehaviour
    {
        [SerializeField] private PlayerDependencies playerDependencies;
        [SerializeField] private float crouchYOffset = 0.25f;

        private Transform playerTransform;
        private IPlayerMovementStateProvider playerMovementStateProvider; // IPlayerMovementStateProvider is implemented in PlayerMovement.cs

        private void Start()
        {
            playerTransform = playerDependencies.transform;
            playerMovementStateProvider = playerDependencies.PlayerMovementState;
        }
        private void Update()
        {
            if (playerMovementStateProvider == null) return;
            Vector3 pos = playerTransform.position;
            if (playerMovementStateProvider.IsCrouching)
                pos.y += crouchYOffset;
            transform.position = pos;
            transform.rotation = playerMovementStateProvider.Orientation.Rotation;
        }
    }
}
