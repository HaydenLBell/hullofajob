using UnityEngine;

namespace cowsins
{
    public abstract class PlayerDeathBehaviour : MonoBehaviour
    {
        [Tooltip("Direct reference to the PlayerDependencies component on the root of the player prefab.")]
        [SerializeField] protected PlayerDependencies playerDependencies;
        protected IPlayerStatsProvider playerStats;

        protected virtual void Awake()
        {
            if (playerDependencies == null)
            {
                Debug.LogError($"[PlayerDeathBehaviour] PlayerDependencies reference is missing on {gameObject.name}. Please assign it in the inspector.");
            }
        }

        protected virtual void Start()
        {
            if (playerDependencies == null) return;
            
            playerStats = playerDependencies.PlayerStats;

            if (playerStats != null)
            {
                playerStats.AddOnDieListener(HandleDeath);
            }

            if (playerDependencies.PlayerMovementEvents != null)
            {
                playerDependencies.PlayerMovementEvents.Events.OnRespawn.AddListener(HandleRespawnAdapter);
            }
        }

        protected virtual void OnDestroy()
        {
            if (playerStats != null)
            {
                playerStats.RemoveOnDieListener(HandleDeath);
            }

            if (playerDependencies != null && playerDependencies.PlayerMovementEvents != null)
            {
                playerDependencies.PlayerMovementEvents.Events.OnRespawn.RemoveListener(HandleRespawnAdapter);
            }
        }

        private void HandleDeath()
        {
            SetWeaponHolderVisible(false);
            OnDeath();
        }

        private void HandleRespawnAdapter(Vector3 position, Quaternion rotation, bool resetVelocity, bool resetVerticalLook)
        {
            HandleRespawn();
        }

        private void HandleRespawn()
        {
            SetWeaponHolderVisible(true);
            OnRespawn();
        }

        private void SetWeaponHolderVisible(bool visible)
        {
            var holder = playerDependencies.WeaponReference?.WeaponHolder;
            if (holder == null) return;
            foreach (var r in holder.GetComponentsInChildren<Renderer>(true))
                r.enabled = visible;
        }

        /// <summary>
        /// Called when the player dies. Override to implement your death behaviour.
        /// </summary>
        protected abstract void OnDeath();

        /// <summary>
        /// Called when the player respawns. Override to clean up your death behaviour.
        /// </summary>
        protected abstract void OnRespawn();
    }
}
