using UnityEngine.UI;
using UnityEngine;
using TMPro;

namespace hb.framework
{
    /// <summary>
    /// Each slot you see on your UI, which is generated depending on your inventory size, requires this component
    /// </summary>
    public class WeaponsInventoryUISlot : MonoBehaviour
    {
        [SerializeField] private Image image;

        [SerializeField] private Sprite nullWeaponIndicator;
        [SerializeField] private TextMeshProUGUI numKeyText;

        private Weapon_SO weapon;

        private Vector3 initScale;
        private bool initScaleSet = false;

        private CanvasGroup canvasGroup;

        // Finds (or adds) the CanvasGroup the first time it's needed
        private CanvasGroup GetCanvasGroup()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();

                if (canvasGroup == null)
                {
                    canvasGroup = gameObject.AddComponent<CanvasGroup>();
                }
            }

            return canvasGroup;
        }

        // Records the normal scale the first time it's needed
        private Vector3 GetInitScale()
        {
            if (initScaleSet == false)
            {
                initScale = transform.localScale;
                initScaleSet = true;
            }

            return initScale;
        }

        public void GetImage()
        {
            if (image == null)
            {
                return;
            }

            if (weapon == null)
            {
                image.sprite = nullWeaponIndicator;
            }
            else
            {
                image.sprite = weapon.icon;
            }
        }

        public void Select()
        {
            transform.localScale = GetInitScale() * 1.2f;
            GetCanvasGroup().alpha = 1f;
        }

        public void Deselect()
        {
            transform.localScale = GetInitScale();
            GetCanvasGroup().alpha = 0.2f;
        }

        public void SetWeapon(Weapon_SO newWeapon)
        {
            weapon = newWeapon;
            GetImage();
        }

        public void Initialize(int index)
        {
            // Make sure the generated slot is visible, even if the template was inactive
            gameObject.SetActive(true);

            // Record the normal scale before anything changes it
            GetInitScale();

            if (numKeyText != null)
            {
                numKeyText.text = (index + 1).ToString();
            }

            GetImage();
        }
    }
}