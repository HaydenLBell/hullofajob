using System.Collections;
using UnityEngine;

namespace cowsins
{
    [System.Flags]
    public enum TransformProperties
    {
        None = 0,
        Position = 1,
        Rotation = 2,
        Scale = 4
    }

    public class FixFurniture : MonoBehaviour
    {
        [Header("What to change")]
        [SerializeField] private TransformProperties propertiesToChange = TransformProperties.Rotation;

        [Header("Target (local space, same values as the Transform shows)")]
        [SerializeField] private Vector3 targetPosition;
        [SerializeField] private Vector3 targetRotation;
        [SerializeField] private Vector3 targetScale = Vector3.one;

        [Tooltip("How long the movement takes, in seconds.")]
        [SerializeField] private float duration = 0.35f;

        [Header("Prompt Text (optional)")]
        [Tooltip("The interactable whose prompt text should change. Leave empty to skip.")]
        [SerializeField] private Interactable interactable;

        [Tooltip("Prompt while in the original state, e.g. 'Close Drawer'.")]
        [SerializeField] private string originalStateText = "";

        [Tooltip("Prompt while in the target state, e.g. 'Open Drawer'.")]
        [SerializeField] private string targetStateText = "";

        // The original state is read from where the object is placed in the scene
        private Vector3 originalPosition;
        private Quaternion originalRotation;
        private Vector3 originalScale;

        private Quaternion targetRotationQuaternion;

        // Worked out once in Awake so the coroutine doesn't need to check the enum
        private bool changePosition;
        private bool changeRotation;
        private bool changeScale;

        private bool isAtTarget = false;
        private Coroutine moveRoutine;

        private void Awake()
        {
            originalPosition = transform.localPosition;
            originalRotation = transform.localRotation;
            originalScale = transform.localScale;

            targetRotationQuaternion = Quaternion.Euler(targetRotation);

            changePosition = (propertiesToChange & TransformProperties.Position) != 0;
            changeRotation = (propertiesToChange & TransformProperties.Rotation) != 0;
            changeScale = (propertiesToChange & TransformProperties.Scale) != 0;

            UpdatePromptText();
        }

        // Hook this up to the Interactable's OnInteract event
        public void Toggle()
        {
            Debug.Log("Toggle called. isAtTarget = " + isAtTarget, this);

            if (isAtTarget)
            {
                MoveToOriginal();
            }
            else
            {
                MoveToTarget();
            }
        }

        public void MoveToTarget()
        {
            isAtTarget = true;
            StartMove(targetPosition, targetRotationQuaternion, targetScale);
        }

        public void MoveToOriginal()
        {
            isAtTarget = false;
            StartMove(originalPosition, originalRotation, originalScale);
        }

        private void StartMove(Vector3 endPosition, Quaternion endRotation, Vector3 endScale)
        {
            // Nothing selected, so there's nothing to animate
            if (changePosition == false && changeRotation == false && changeScale == false)
            {
                UpdatePromptText();
                return;
            }

            // If it's already moving, stop and continue from where it is now
            if (moveRoutine != null)
            {
                StopCoroutine(moveRoutine);
            }

            UpdatePromptText();
            moveRoutine = StartCoroutine(MoveRoutine(endPosition, endRotation, endScale));
        }

        private IEnumerator MoveRoutine(Vector3 endPosition, Quaternion endRotation, Vector3 endScale)
        {
            Vector3 startPosition = transform.localPosition;
            Quaternion startRotation = transform.localRotation;
            Vector3 startScale = transform.localScale;

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed = elapsed + Time.deltaTime;

                float progress = Mathf.Clamp01(elapsed / duration);

                // Ease in and out so it starts and stops gently
                float eased = Mathf.SmoothStep(0f, 1f, progress);

                ApplyTransform(startPosition, startRotation, startScale, endPosition, endRotation, endScale, eased);

                yield return null;
            }

            // Snap exactly to the end so there's no tiny drift
            ApplyTransform(startPosition, startRotation, startScale, endPosition, endRotation, endScale, 1f);

            moveRoutine = null;
        }

        // Only touches the properties that were selected
        private void ApplyTransform(
            Vector3 startPosition, Quaternion startRotation, Vector3 startScale,
            Vector3 endPosition, Quaternion endRotation, Vector3 endScale,
            float amount)
        {
            if (changePosition)
            {
                transform.localPosition = Vector3.Lerp(startPosition, endPosition, amount);
            }

            if (changeRotation)
            {
                transform.localRotation = Quaternion.Slerp(startRotation, endRotation, amount);
            }

            if (changeScale)
            {
                transform.localScale = Vector3.Lerp(startScale, endScale, amount);
            }
        }

        private void UpdatePromptText()
        {
            if (interactable == null)
            {
                return;
            }

            if (isAtTarget)
            {
                if (targetStateText != "")
                {
                    interactable.interactText = targetStateText;
                }
            }
            else
            {
                if (originalStateText != "")
                {
                    interactable.interactText = originalStateText;
                }
            }
        }

        // ---------- Editor helpers ----------

        // Runs when the component is first added: copies the current
        // transform into the targets so nothing starts at zero
        private void Reset()
        {
            CaptureTargetFromCurrent();
        }

        // Right-click the component header to use this
        [ContextMenu("Set Target From Current Transform")]
        private void CaptureTargetFromCurrent()
        {
            targetPosition = transform.localPosition;
            targetRotation = transform.localEulerAngles;
            targetScale = transform.localScale;
        }
    }
}