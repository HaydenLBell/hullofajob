using UnityEngine;
using hb.framework;

namespace hb.UI
{
    [RequireComponent(typeof(Collider))]
    public class RoomZone : MonoBehaviour
    {
        [SerializeField] private string roomName = "Room";

        [Tooltip("Lower numbers appear first on the HUD.")]
        [SerializeField] private int order = 0;

        // Fired when the player walks into any room
        public static event System.Action<RoomZone> PlayerEntered;

        private IRepairTask[] tasks = new IRepairTask[0];

        public string RoomName
        {
            get { return roomName; }
        }

        public int Order
        {
            get { return order; }
        }

        private void Awake()
        {
            // Every task under this object belongs to this room (including hidden ones)
            tasks = GetComponentsInChildren<IRepairTask>(true);

            Collider trigger = GetComponent<Collider>();
            trigger.isTrigger = true;
        }

        public int TotalTasks
        {
            get { return tasks.Length; }
        }

        public int CompletedTasks
        {
            get
            {
                int count = 0;

                foreach (IRepairTask task in tasks)
                {
                    if (task.IsRepaired)
                    {
                        count = count + 1;
                    }
                }

                return count;
            }
        }

        public float Progress
        {
            get
            {
                if (tasks.Length == 0)
                {
                    return 1f;
                }

                return (float)CompletedTasks / tasks.Length;
            }
        }

        public bool IsComplete
        {
            get { return CompletedTasks >= tasks.Length; }
        }

        private void OnTriggerEnter(Collider other)
        {
            // Works even if the collider is on a child of the player
            if (other.GetComponentInParent<PlayerDependencies>() == null)
            {
                return;
            }

            if (PlayerEntered != null)
            {
                PlayerEntered.Invoke(this);
            }
        }
    }
}