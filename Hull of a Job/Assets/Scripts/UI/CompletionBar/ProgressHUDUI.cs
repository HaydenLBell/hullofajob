using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using hb.framework;
using hb.UI;

namespace hb
{
    public class ProgressHUDUI : MonoBehaviour
    {
        [Header("Source")]
        [SerializeField] private GameManager gameManager;

        [Header("Slots")]
        [SerializeField] private RoomSlotUI slotPrefab;
        [SerializeField] private Transform slotRow;

        [Tooltip("With this many rooms or fewer, show labelled segments. Above it, show pips.")]
        [SerializeField] private int maxLabelledRooms = 5;

        [Header("Current room")]
        [SerializeField] private TextMeshProUGUI currentRoomLabel;
        [SerializeField] private Image currentRoomFill;

        [Header("Fading")]
        [SerializeField] private CanvasGroup hudGroup;
        [SerializeField] private float idleAlpha = 0.3f;
        [SerializeField] private float showDuration = 3f;
        [SerializeField] private float fadeSpeed = 1.5f;

        [Header("Room complete toast")]
        [SerializeField] private CanvasGroup toastGroup;
        [SerializeField] private TextMeshProUGUI toastText;
        [SerializeField] private float toastDuration = 2f;

        private RoomZone[] rooms = new RoomZone[0];
        private RoomSlotUI[] slots = new RoomSlotUI[0];
        private RoomZone currentRoom;

        private Coroutine fadeRoutine;
        private Coroutine toastRoutine;

        

        private void OnEnable()
        {
            RoomZone.PlayerEntered += OnPlayerEnteredRoom;

            if (gameManager != null)
            {
                gameManager.OnProgressChanged.AddListener(OnProgressChanged);
                gameManager.OnRoomCompleted.AddListener(OnRoomCompleted);
            }
        }

        private void OnDisable()
        {
            RoomZone.PlayerEntered -= OnPlayerEnteredRoom;

            if (gameManager != null)
            {
                gameManager.OnProgressChanged.RemoveListener(OnProgressChanged);
                gameManager.OnRoomCompleted.RemoveListener(OnRoomCompleted);
            }
        }

        private void OnProgressChanged(float percent)
        {
            Refresh();
        }

        private void OnRoomCompleted(RoomZone room)
        {
            ShowToast(room.RoomName + " complete");
        }

        private void Start()
        {
            BuildSlots();
            Refresh();

            if (toastGroup != null)
            {
                toastGroup.alpha = 0f;
            }
        }

        private void BuildSlots()
        {
            rooms = gameManager.Rooms;

            bool showAsSegments = rooms.Length <= maxLabelledRooms;
            slots = new RoomSlotUI[rooms.Length];

            for (int i = 0; i < rooms.Length; i++)
            {
                RoomSlotUI slot = Instantiate(slotPrefab, slotRow);
                slot.Setup(rooms[i].RoomName, showAsSegments);
                slots[i] = slot;
            }
        }


        private void OnPlayerEnteredRoom(RoomZone room)
        {
            currentRoom = room;
            Refresh();
        }

        public void Refresh()
        {
            for (int i = 0; i < rooms.Length; i++)
            {
                bool isCurrent = rooms[i] == currentRoom;
                slots[i].SetProgress(rooms[i].Progress, isCurrent);
            }

            UpdateCurrentRoomDisplay();
            ShowBriefly();
        }

        private void UpdateCurrentRoomDisplay()
        {
            if (currentRoom == null)
            {
                return;
            }

            if (currentRoomLabel != null)
            {
                int percent = Mathf.RoundToInt(currentRoom.Progress * 100f);
                currentRoomLabel.text = currentRoom.RoomName + "  " + percent + "%";
            }

            if (currentRoomFill != null)
            {
                currentRoomFill.fillAmount = currentRoom.Progress;
            }
        }

        // ---------- Fading ----------

        private void ShowBriefly()
        {
            if (hudGroup == null)
            {
                return;
            }

            if (fadeRoutine != null)
            {
                StopCoroutine(fadeRoutine);
            }

            fadeRoutine = StartCoroutine(ShowThenFade());
        }

        private IEnumerator ShowThenFade()
        {
            hudGroup.alpha = 1f;

            yield return new WaitForSeconds(showDuration);

            while (hudGroup.alpha > idleAlpha)
            {
                hudGroup.alpha = Mathf.MoveTowards(hudGroup.alpha, idleAlpha, fadeSpeed * Time.deltaTime);
                yield return null;
            }

            fadeRoutine = null;
        }

        private void ShowToast(string message)
        {
            if (toastGroup == null || toastText == null)
            {
                return;
            }

            toastText.text = message;

            if (toastRoutine != null)
            {
                StopCoroutine(toastRoutine);
            }

            toastRoutine = StartCoroutine(ToastRoutine());
        }

        private IEnumerator ToastRoutine()
        {
            while (toastGroup.alpha < 1f)
            {
                toastGroup.alpha = Mathf.MoveTowards(toastGroup.alpha, 1f, 3f * Time.deltaTime);
                yield return null;
            }

            yield return new WaitForSeconds(toastDuration);

            while (toastGroup.alpha > 0f)
            {
                toastGroup.alpha = Mathf.MoveTowards(toastGroup.alpha, 0f, 1.5f * Time.deltaTime);
                yield return null;
            }

            toastRoutine = null;
        }
    }
}