/// <summary>
/// This script belongs to cowsins™ as a part of the cowsins´ FPS Engine. All rights reserved. 
/// </summary>
using UnityEngine;
using System.Collections;
namespace cowsins
{
    /// <summary>
    /// Inheriting from Interactable, this means you can interact with the door
    /// Keep in mind that this is highly subject to change on future updates
    /// </summary>
    [RequireComponent(typeof(BoxCollider))] // Require a trigger collider to detect side
    public class DoorInteractable : Interactable
    {
        [SerializeField, Title("DOOR INTERACTABLE", upMargin: 8, divider: true)] private string openInteractionText;

        [SerializeField] private string closeInteractionText;

        [SerializeField] private string lockedInteractionText;

        [SerializeField, SaveField] private bool isLocked;

        /// <summary>Exposed for network sync components to check lock state.</summary>
        public bool IsLocked => isLocked;

        [Tooltip("The pivot point for the door"), SerializeField]
        private Transform doorPivot;

        [Tooltip("The Vector3 to add to the door on opened"), SerializeField] private Vector3 offsetPosition;

        [Tooltip("How much you want to rotate the door"), SerializeField]
        private float openedDoorRotation;

        [Tooltip("rotation speed"), SerializeField]
        private float speed;

        [SerializeField] private AudioClip openDoorSFX, closeDoorSFX, lockedDoorSFX;

        public AudioClip OpenDoorSFX => openDoorSFX;
        public AudioClip CloseDoorSFX => closeDoorSFX;

        private Quaternion closedRot;

        private Vector3 initialLocalPos;

        [SaveField] private int side;

        private Coroutine doorCoroutine;

        private void Awake()
        {
            initialLocalPos = doorPivot.localPosition;
            closedRot = doorPivot.localRotation;
        }

        private void Start()
        {
            interactText = isLocked ? lockedInteractionText : openInteractionText;
        }
 
        /// <summary>
        /// Check for interaction. Overriding from Interactable.cs
        /// </summary>
        public override void Interact(Transform player)
        {
            // Check if its locked
            if (isLocked)
            {
                SoundManager.Instance.PlaySound(lockedDoorSFX, 0, .1f, true);
                return;
            }
            // Change state
            alreadyInteracted = !alreadyInteracted;

            // Display appropriate UI
            interactText = isLocked ? lockedInteractionText :
                (alreadyInteracted ? closeInteractionText : openInteractionText);

            if (alreadyInteracted) SoundManager.Instance.PlaySound(openDoorSFX, 0, .1f, true);
            else SoundManager.Instance.PlaySound(closeDoorSFX, 0, .1f, true);

            IPlayerMovementStateProvider playerProvider = player.GetComponent<IPlayerMovementStateProvider>();
            // Checking the side where we are opening the door from;
            side = (Vector3.Dot(transform.right, playerProvider.Orientation.Forward) > 0) ? 1 : -1;

            // Stop any running coroutines before starting a new one
            if (doorCoroutine != null)
                StopCoroutine(doorCoroutine);

            // Start coroutine for opening/closing the door
            doorCoroutine = StartCoroutine(HandleDoorMovement());

            interactableEvents.OnInteract?.Invoke();
        }

        /// <summary>
        /// Applies an open/close state change and animates the door pivot.
        /// Safe to call from any peer — each peer runs the coroutine locally.
        /// Called by MultiplayerDoorSync when a SyncVar update is received.
        /// </summary>
        /// <param name="open">True to open; false to close.</param>
        /// <param name="playerSide">Which side the door swings toward (1 or -1).</param>
        /// <param name="playAudio">Pass false for silent late-join state restoration.</param>
        public void SetNetworkState(bool open, int playerSide, bool playAudio = true)
        {
            if (alreadyInteracted == open) return;
            alreadyInteracted = open;
            side = playerSide;
            interactText = isLocked ? lockedInteractionText
                : (alreadyInteracted ? closeInteractionText : openInteractionText);

            if (playAudio)
            {
                if (alreadyInteracted) SoundManager.Instance.PlaySound(openDoorSFX, 0, .1f, true);
                else SoundManager.Instance.PlaySound(closeDoorSFX, 0, .1f, true);
            }

            if (doorCoroutine != null) StopCoroutine(doorCoroutine);
            doorCoroutine = StartCoroutine(HandleDoorMovement());
        }

        protected IEnumerator HandleDoorMovement()
        {
            Vector3 targetLocalPos = alreadyInteracted ? initialLocalPos + offsetPosition : initialLocalPos;
            Quaternion targetRot = alreadyInteracted
                ? closedRot * Quaternion.Euler(0, openedDoorRotation * side, 0)
                : closedRot;

            // Smoothly move and rotate the door until it reaches the target position and rotation
            while (Vector3.Distance(doorPivot.localPosition, targetLocalPos) > 0.01f || Quaternion.Angle(doorPivot.localRotation, targetRot) > 0.1f)
            {
                doorPivot.localPosition = Vector3.Lerp(doorPivot.localPosition, targetLocalPos, Time.deltaTime * speed);
                doorPivot.localRotation = Quaternion.Lerp(doorPivot.localRotation, targetRot, Time.deltaTime * speed);
                yield return null;
            }

            // Ensure the door is exactly at the target position and rotation
            doorPivot.localPosition = targetLocalPos;
            doorPivot.localRotation = targetRot;
        }

        // Immediately snaps the door to its closed position without any lerp
        public void SnapToClosed()
        {
            if (doorCoroutine != null)
            {
                StopCoroutine(doorCoroutine);
                doorCoroutine = null;
            }

            alreadyInteracted = false;
            side = 1;
            interactText = isLocked ? lockedInteractionText : openInteractionText;
            doorPivot.localPosition = initialLocalPos;
            doorPivot.localRotation = closedRot;
        }

        public void Lock()
        {
            isLocked = true;
            interactText = lockedInteractionText;
        }

        public void UnLock()
        {
            isLocked = false;
            interactText = openInteractionText;
        }

        public void ToggleLock()
        {
            isLocked = !isLocked;
            interactText = isLocked ? lockedInteractionText : openInteractionText;
        }

#if SAVE_LOAD_ADD_ON
        // Open or close the door based on whether it is interacted or not
        // InteractedState is called after loading
        public override void LoadedState()
        {
            if (alreadyInteracted)
            {
                doorPivot.localPosition = initialLocalPos + offsetPosition;
                doorPivot.localRotation = closedRot * Quaternion.Euler(0, openedDoorRotation * side, 0);
            }
            else
            {
                doorPivot.localPosition = initialLocalPos;
                doorPivot.localRotation = closedRot;
            }
        }
#endif
    }
}
