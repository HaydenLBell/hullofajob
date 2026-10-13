using System.Collections;
using UnityEngine;

namespace cowsins
{
    /// <summary>
    /// Contains a collection of useful UI Effects, from Sway, to Tilt or Jump Motion.
    /// This component can be found on "UIEffects" Object, inside PlayerUI Prefab.
    /// </summary>
    public class UIEffects : MonoBehaviour
    {
        [Title("References"), SerializeField] private PlayerDependencies playerDependencies;
        [Tooltip("Assign all the UI containers to apply effects to"), SerializeField] private RectTransform[] effectContainers;

        [SerializeField, Title("UI SWAY"), Header("Position")] private float amount = 0.02f;
        [SerializeField] private float maxAmount = 0.06f;
        [SerializeField] private float smoothAmount = 6f;

        [SerializeField, Header("Tilt")] private float tiltAmount = 4f;
        [SerializeField] private float maxTiltAmount = 5f;
        [SerializeField] private float smoothTiltAmount = 12f;

        [SerializeField, Title("JUMP MOTION", upMargin = 10)] private AnimationCurve jumpMotion;
        [SerializeField] private AnimationCurve groundedMotion;
        [SerializeField] private float distance;
        [SerializeField] private float rotationAmount;
        [SerializeField, Min(1)] private float evaluationSpeed;

        private struct ContainerData
        {
            public RectTransform Transform;
            public Vector3 InitialPosition;
            public Quaternion InitialRotation;
        }
        private ContainerData[] containersData;

        private float InputX;
        private float InputY;

        private Vector3 swayPositionOffset = Vector3.zero;
        private Quaternion swayRotationOffset = Quaternion.identity;

        private Vector3 jumpPositionOffset = Vector3.zero;
        private Quaternion jumpRotationOffset = Quaternion.identity;

        private Coroutine jumpMotionCoroutine;

        private IPlayerControlProvider playerControl; // Reference to PlayerControl.cs ( IPlayerControlProvider is implemented in PlayerControl.cs )
        private InputManager inputManager;

        private void Start()
        {
            if (effectContainers != null)
            {
                containersData = new ContainerData[effectContainers.Length];
                for (int i = 0; i < effectContainers.Length; i++)
                {
                    if (effectContainers[i] != null)
                    {
                        containersData[i] = new ContainerData
                        {
                            Transform = effectContainers[i],
                            InitialPosition = effectContainers[i].localPosition,
                            InitialRotation = effectContainers[i].localRotation
                        };
                    }
                }
            }

            playerControl = playerDependencies.PlayerControl;
            inputManager = playerDependencies.InputManager;
            playerEvents = playerDependencies.PlayerMovementEvents;

            // Listen to PlayerMovement events when Jumping and landing to handle Jump Motion accordingly.
            playerEvents.Events.OnJump.AddListener(OnJump);
            playerEvents.Events.OnLand.AddListener(OnLand);
        }

        private IPlayerMovementEventsProvider playerEvents;

        private void OnDestroy()
        {
            if (playerEvents != null && playerEvents.Events != null)
            {
                playerEvents.Events.OnJump.RemoveListener(OnJump);
                playerEvents.Events.OnLand.RemoveListener(OnLand);
            }
        }

        private void Update()
        {
            if (playerControl == null || inputManager == null) return;

            if (playerControl.IsControllable)
            {
                SimpleSway();
            }

            // Apply combined transforms here
            ApplyCombinedTransform();
        }

        private void SimpleSway()
        {
            CalculateSway();
            CalculateSwayOffsets();
        }

        private void CalculateSway()
        {
            InputX = -inputManager.MouseX / 10f - 2f * inputManager.ControllerX;
            InputY = -inputManager.MouseY / 10f - 2f * inputManager.ControllerY;
        }

        private void CalculateSwayOffsets()
        {
            float moveX = Mathf.Clamp(InputX * amount, -maxAmount, maxAmount);
            float moveY = Mathf.Clamp(InputY * amount, -1f, 1f);
            Vector3 targetPos = new Vector3(moveX, moveY, 0f);

            swayPositionOffset = Vector3.Lerp(swayPositionOffset, targetPos, Time.deltaTime * smoothAmount);

            float tiltX = Mathf.Clamp(InputX * tiltAmount, -maxTiltAmount, maxTiltAmount);
            Quaternion targetRot = Quaternion.Euler(0f, 0f, tiltX);

            swayRotationOffset = Quaternion.Slerp(swayRotationOffset, targetRot, Time.deltaTime * smoothTiltAmount);
        }

        private void ApplyCombinedTransform()
        {
            if (containersData == null) return;

            for (int i = 0; i < containersData.Length; i++)
            {
                if (containersData[i].Transform == null) continue;

                Vector3 combinedPosition = containersData[i].InitialPosition + swayPositionOffset + jumpPositionOffset;
                Quaternion combinedRotation = containersData[i].InitialRotation * swayRotationOffset * jumpRotationOffset;

                containersData[i].Transform.localPosition = combinedPosition;
                containersData[i].Transform.localRotation = combinedRotation;
            }
        }

        private void OnJump()
        {
            if (!isActiveAndEnabled) return;
            if (jumpMotionCoroutine != null) StopCoroutine(jumpMotionCoroutine);
            jumpMotionCoroutine = StartCoroutine(ApplyMotion(jumpMotion));
        }

        private void OnLand()
        {
            if (!isActiveAndEnabled) return;
            if (jumpMotionCoroutine != null) StopCoroutine(jumpMotionCoroutine);
            jumpMotionCoroutine = StartCoroutine(ApplyMotion(groundedMotion));
        }

        private IEnumerator ApplyMotion(AnimationCurve motionCurve)
        {
            float motion = 0f;

            while (motion < 1f)
            {
                motion += Time.deltaTime * evaluationSpeed;
                float evaluatedMotion = motionCurve.Evaluate(motion);

                // Update the Jump Offsets
                jumpPositionOffset = new Vector3(0f, evaluatedMotion * distance, 0f);
                jumpRotationOffset = Quaternion.Euler(evaluatedMotion * rotationAmount, 0f, 0f);

                yield return null;
            }

            // Reset Jump Offsets after motion ends to avoid getting stuck
            jumpPositionOffset = Vector3.zero;
            jumpRotationOffset = Quaternion.identity;
        }
    }
}
