using UnityEngine;
using UnityEngine.Events;
using hb.framework;
using hb.UI;

public class GameManager : MonoBehaviour
{
    [Header("Lighting")]
    [SerializeField] private GameObject emergencyLights;
    [SerializeField] private GameObject normalLights;

    [Header("Progress (read only)")]
    [SerializeField] private int totalTasks;
    [SerializeField] private int completedTasks;
    [SerializeField] private float percentRepaired;

    [Header("Events")]
    public UnityEvent<float> OnProgressChanged;
    public UnityEvent<RoomZone> OnRoomCompleted;
    public UnityEvent OnAllRepaired;

    public float PercentRepaired
    {
        get { return percentRepaired; }
    }

    public RoomZone[] Rooms
    {
        get { return rooms; }
    }

    private RoomZone[] rooms = new RoomZone[0];
    private bool[] roomWasComplete = new bool[0];
    private bool allRepairedFired = false;
    private bool initialised = false;

    private void OnEnable()
    {
        RepairEvents.OnTaskStateChanged += Recount;
    }

    private void OnDisable()
    {
        RepairEvents.OnTaskStateChanged -= Recount;
    }

    private void Awake()
    {
        // Find rooms early so the HUD can build its slots in its own Start
        rooms = FindObjectsByType<RoomZone>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        System.Array.Sort(rooms, CompareRoomOrder);
    }

    private void Start()
    {
        if (emergencyLights != null) emergencyLights.SetActive(true);
        if (normalLights != null) normalLights.SetActive(false);

        // Rooms that start with nothing to do count as already complete (no toast)
        roomWasComplete = new bool[rooms.Length];
        for (int i = 0; i < rooms.Length; i++)
        {
            roomWasComplete[i] = rooms[i].IsComplete;
        }

        WarnAboutTasksOutsideRooms();

        initialised = true;
        Recount();
    }

    private int CompareRoomOrder(RoomZone a, RoomZone b)
    {
        return a.Order.CompareTo(b.Order);
    }

    private void Recount()
    {
        if (initialised == false)
        {
            return;
        }

        totalTasks = 0;
        completedTasks = 0;

        for (int i = 0; i < rooms.Length; i++)
        {
            RoomZone room = rooms[i];

            totalTasks = totalTasks + room.TotalTasks;
            completedTasks = completedTasks + room.CompletedTasks;

            // Room has just been finished
            if (room.IsComplete && roomWasComplete[i] == false)
            {
                if (OnRoomCompleted != null)
                {
                    OnRoomCompleted.Invoke(room);
                }
            }

            roomWasComplete[i] = room.IsComplete;
        }

        if (totalTasks > 0)
        {
            percentRepaired = (float)completedTasks / totalTasks * 100f;
        }
        else
        {
            percentRepaired = 100f;
        }

        if (OnProgressChanged != null)
        {
            OnProgressChanged.Invoke(percentRepaired);
        }

        if (totalTasks > 0 && completedTasks == totalTasks && allRepairedFired == false)
        {
            allRepairedFired = true;

            if (OnAllRepaired != null)
            {
                OnAllRepaired.Invoke();
            }
        }
    }

    // Any task that isn't under a Room won't be counted, so flag it
    private void WarnAboutTasksOutsideRooms()
    {
        MonoBehaviour[] allBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (MonoBehaviour behaviour in allBehaviours)
        {
            if (behaviour is IRepairTask)
            {
                if (behaviour.GetComponentInParent<RoomZone>(true) == null)
                {
                    Debug.LogWarning("Task isn't inside a Room, so it won't count towards progress: " + behaviour.name, behaviour);
                }
            }
        }
    }
}