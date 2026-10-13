using UnityEngine;

namespace cowsins
{
    /// <summary>
    /// Forwards damage from a child collider hierarchy (like PlayerGraphics) to the actual PlayerStats.
    /// This fixes hit detection in hierarchies where GatherDamageableParent would fail to find PlayerStats.
    /// </summary>
    public class DamageForwarder : MonoBehaviour
    {
        [Tooltip("The actual stats component that should receive damage. If null, it will search the root on Awake.")]
        public PlayerStats targetStats;

        private void Awake()
        {
            if (targetStats == null)
            {
                targetStats = GetComponentInParent<PlayerStats>();
                if (targetStats == null && transform.root != null)
                    targetStats = transform.root.GetComponentInChildren<PlayerStats>();
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (targetStats == null)
            {
                targetStats = GetComponentInParent<PlayerStats>();
                if (targetStats == null && transform.root != null)
                    targetStats = transform.root.GetComponentInChildren<PlayerStats>();
            }
        }
#endif
    }
}

