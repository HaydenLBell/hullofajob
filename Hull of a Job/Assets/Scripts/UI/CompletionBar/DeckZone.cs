using UnityEngine;

namespace hb.UI
{
    public class DeckZone : MonoBehaviour
    {
        [SerializeField] private string deckName = "Upper deck";

        [Tooltip("Lower numbers appear first on the HUD.")]
        [SerializeField] private int order = 0;

        private RoomZone[] rooms = new RoomZone[0];

        public string DeckName
        {
            get { return deckName; }
        }

        public int Order
        {
            get { return order; }
        }

        private void Awake()
        {
            // Every room under this object belongs to this deck
            rooms = GetComponentsInChildren<RoomZone>(true);
        }

        public int TotalTasks
        {
            get
            {
                int total = 0;

                foreach (RoomZone room in rooms)
                {
                    total = total + room.TotalTasks;
                }

                return total;
            }
        }

        public int CompletedTasks
        {
            get
            {
                int completed = 0;

                foreach (RoomZone room in rooms)
                {
                    completed = completed + room.CompletedTasks;
                }

                return completed;
            }
        }

        public float Progress
        {
            get
            {
                int total = TotalTasks;

                if (total == 0)
                {
                    return 1f;
                }

                return (float)CompletedTasks / total;
            }
        }

        public bool IsComplete
        {
            get { return CompletedTasks >= TotalTasks; }
        }
    }
}