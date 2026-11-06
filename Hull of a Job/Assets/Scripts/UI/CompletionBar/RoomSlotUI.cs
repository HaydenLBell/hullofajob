using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace hb.UI
{
    public class RoomSlotUI : MonoBehaviour
    {
        [SerializeField] private Image fill;
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private GameObject tick;
        [SerializeField] private LayoutElement layoutElement;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Sizes")]
        [SerializeField] private float segmentWidth = 100f;
        [SerializeField] private float pipWidth = 14f;

        [Header("Look")]
        [SerializeField] private float currentRoomAlpha = 1f;
        [SerializeField] private float otherRoomAlpha = 0.55f;
        [SerializeField] private float fillSpeed = 2f;

        private float targetFill = 0f;
        private float shownFill = 0f;

        public void Setup(string roomName, bool showAsSegment)
        {
            if (label != null)
            {
                label.text = roomName;
                label.gameObject.SetActive(showAsSegment);
            }

            if (layoutElement != null)
            {
                if (showAsSegment)
                {
                    layoutElement.preferredWidth = segmentWidth;
                }
                else
                {
                    layoutElement.preferredWidth = pipWidth;
                }
            }

            if (fill != null)
            {
                fill.fillAmount = 0f;
            }

            if (tick != null)
            {
                tick.SetActive(false);
            }
        }

        public void SetProgress(float progress, bool isCurrentRoom)
        {
            targetFill = progress;

            if (tick != null)
            {
                tick.SetActive(progress >= 1f);
            }

            if (canvasGroup != null)
            {
                if (isCurrentRoom)
                {
                    canvasGroup.alpha = currentRoomAlpha;
                }
                else
                {
                    canvasGroup.alpha = otherRoomAlpha;
                }
            }
        }

        private void Update()
        {
            // Nothing to animate
            if (shownFill == targetFill)
            {
                return;
            }

            shownFill = Mathf.MoveTowards(shownFill, targetFill, fillSpeed * Time.deltaTime);

            if (fill != null)
            {
                fill.fillAmount = shownFill;
            }
        }
    }
}