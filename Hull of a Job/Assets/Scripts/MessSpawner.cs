using System.Collections;
using UnityEngine;
using hb.framework;

namespace hb
{
    public class MessSpawner : MonoBehaviour
    {
        [Tooltip("The mess in the scene to bring back, e.g. the puddle under the leak.")]
        [SerializeField] private MessInteractable mess;

        [Tooltip("Seconds after cleaning before the mess comes back.")]
        [SerializeField] private float respawnDelay = 3f;

        [SerializeField] private bool canSpawn = true;

        private Coroutine respawnRoutine;

        private void OnEnable()
        {
            if (mess != null)
            {
                mess.interactableEvents.OnInteract.AddListener(OnMessCleaned);
            }
        }

        private void OnDisable()
        {
            if (mess != null)
            {
                mess.interactableEvents.OnInteract.RemoveListener(OnMessCleaned);
            }
        }

        private void OnMessCleaned()
        {
            if (canSpawn == false)
            {
                return;
            }

            // Restart the timer if one is already running
            if (respawnRoutine != null)
            {
                StopCoroutine(respawnRoutine);
            }

            respawnRoutine = StartCoroutine(RespawnAfterDelay());
        }

        private IEnumerator RespawnAfterDelay()
        {
            yield return new WaitForSeconds(respawnDelay);

            respawnRoutine = null;

            // The leak might have been fixed while we were waiting
            if (canSpawn == false)
            {
                yield break;
            }

            mess.ResetMess();
        }

        // Call this when the leak is fixed (e.g. from the valve's OnInteract)
        public void DisableSpawn()
        {
            canSpawn = false;

            if (respawnRoutine != null)
            {
                StopCoroutine(respawnRoutine);
                respawnRoutine = null;
            }
        }
    }
}