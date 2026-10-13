using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
namespace cowsins
{
    public class Compass : MonoBehaviour, IHUDModule
    {
        [SerializeField] private Transform cam;
        [SerializeField] private TextMeshProUGUI compassText;

        [SerializeField] private GameObject compassElementIcon;

        private List<CompassElement> compassElements = new List<CompassElement>();
        private RawImage compass;
        private int lastAngleDisplayed = -1;

        public static System.Action OnCompassUpdated;

        private static Compass _instance;
        public static Compass Instance 
        { 
            get => _instance; 
            set 
            {
                _instance = value;
                OnCompassUpdated?.Invoke();
            }
        }

        public void Initialize(PlayerDependencies dependencies)
        {
            // Initialization handled by UIManager
        }

        private void Awake()
        {
            if (Instance == null) Instance = this;

            compass = GetComponent<RawImage>();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
        private void Update()
        {
            float angle = cam.eulerAngles.y;
            compass.uvRect = new Rect(angle / 360, 0, 1, 1);

            int angleInt = Mathf.RoundToInt(angle);
            if (angleInt != lastAngleDisplayed)
            {
                lastAngleDisplayed = angleInt;
                compassText.SetText("{0}", angleInt);
            }

            foreach (CompassElement el in compassElements)
            {
                el.image.rectTransform.anchoredPosition = GetElementPositionInCompass(el);
            }
        }

        // We want to call this to add new compass elements.
        public void AddCompassElement(CompassElement element)
        {
            GameObject newElement = Instantiate(compassElementIcon, compass.transform);
            element.image = newElement.transform.GetChild(0).GetComponent<Image>();
            element.image.sprite = element.icon;
            compassElements.Add(element);
        }

        // We want to call this to remove new compass elements.
        public void RemoveCompassElement(CompassElement element)
        {
            compassElements.Remove(element);
            if (element.image != null)
            {
                Destroy(element.image.transform.parent.gameObject);
                element.image = null;
            }
        }

        // Calculates the position of the image depending on where the compass element and the players are.
        private Vector2 GetElementPositionInCompass(CompassElement element)
        {
            Vector2 playerPosition = new Vector2(cam.position.x, cam.position.z);
            Vector2 playerForward = new Vector2(cam.forward.x, cam.forward.z);
            float angle = Vector2.SignedAngle(element.GetVector2Pos() - playerPosition, playerForward);

            return new Vector2(angle * compass.rectTransform.rect.width / 360, 0);
        }
    }
}

#if UNITY_EDITOR
namespace cowsins { [UnityEditor.CustomEditor(typeof(Compass))] public class CompassEditor : HUDModuleEditorBase { } }
#endif
