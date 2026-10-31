using UnityEngine;

namespace hb.framework
{
    public enum MessType
    {
        WallSmudge,
        Puddle,
        Pile,
        Graffiti,
        Bed,
        Furniture
    }

    public class MessInteractable : Interactable, IRepairTask
    {
        [Tooltip("What kind of mess this is. Used for the interaction prompt.")]
        [SerializeField] private MessType messType = MessType.Puddle;

        [Tooltip("Hide the mess once it's cleaned.")]
        [SerializeField] private bool hideOnClean = true;

        [Tooltip("Weapon the player must have equipped to clean this. Leave empty to allow any.")]
        [SerializeField] private Weapon_SO requiredWeapon;

        [SerializeField] private string missingWeaponText = "Needs Mop!";
        [SerializeField] private Color missingWeaponColor = Color.red;

        [Header("Outline Colours")]
        [SerializeField] private Color canCleanOutlineColor = Color.white;
        [SerializeField] private Color missingWeaponOutlineColor = new Color(0.55f, 0.55f, 0.55f, 1f);

        [Tooltip("Can be interacted with again after the first time (e.g. doors). Still only counts towards progress once.")]
        [SerializeField] private bool repeatable = false;

        public bool IsRepaired => alreadyInteracted;

        private PlayerDependencies player;

        // Remembers whether the player can currently clean this
        private bool canCleanNow = false;

        private void Start()
        {
            player = FindFirstObjectByType<PlayerDependencies>();

            if (player != null && player.WeaponEvents != null)
            {
                player.WeaponEvents.Events.OnUnholster.AddListener(OnWeaponEquipped);
                player.WeaponEvents.Events.OnUnselectingWeapon.AddListener(OnWeaponRemoved);
                player.WeaponEvents.Events.OnReleaseWeapon.AddListener(OnWeaponRemoved);
            }

            RefreshState();
        }

        private void OnDestroy()
        {
            if (player == null || player.WeaponEvents == null) return;

            player.WeaponEvents.Events.OnUnholster.RemoveListener(OnWeaponEquipped);
            player.WeaponEvents.Events.OnUnselectingWeapon.RemoveListener(OnWeaponRemoved);
            player.WeaponEvents.Events.OnReleaseWeapon.RemoveListener(OnWeaponRemoved);
        }

        public override void Interact(Transform playerTransform)
        {
            if (!HasRequiredWeapon()) return;

            bool firstTime = alreadyInteracted == false;

            // Non-repeatable mess can only be done once
            if (firstTime == false && repeatable == false) return;

            base.Interact(playerTransform); // fires OnInteract and sets alreadyInteracted

            // Only count towards progress the first time
            if (firstTime)
            {
                RepairEvents.Notify();
            }

            // Repeatable objects stay in the world and stay interactable
            if (repeatable)
            {
                return;
            }

            if (hideOnClean) gameObject.SetActive(false);
            else GetComponent<Collider>().enabled = false; // stop re-interacting
        }

        public override void Highlight(Transform playerTransform)
        {
            SetOutlineColour(canCleanNow);
            base.Highlight(playerTransform);
        }

        private void OnWeaponEquipped(bool autoReload, bool prop2)
        {
            RefreshState();
        }

        // Nothing is equipped at this point, so only "no requirement" can pass
        private void OnWeaponRemoved()
        {
            bool canClean = requiredWeapon == null;
            ApplyState(canClean);
        }

        private void RefreshState()
        {
            bool canClean = HasRequiredWeapon();
            ApplyState(canClean);
        }

        // Updates the prompt text and the outline colour together
        private void ApplyState(bool canClean)
        {
            canCleanNow = canClean;

            if (canClean)
            {
                interactText = GetCleanText();
            }
            else
            {
                interactText = GetMissingText();
            }

            SetOutlineColour(canClean);
        }

        private void SetOutlineColour(bool canClean)
        {
            OutlineAsset outline = GetOutline();

            if (outline == null)
            {
                return;
            }

            Color targetColour = missingWeaponOutlineColor;
            if (canClean)
            {
                targetColour = canCleanOutlineColor;
            }

            // Only change it if it's actually different
            if (outline.OutlineColor != targetColour)
            {
                outline.OutlineColor = targetColour;
            }
        }

        private bool HasRequiredWeapon()
        {
            if (requiredWeapon == null) return true;
            if (player == null || player.WeaponReference == null) return false;

            return player.WeaponReference.Weapon == requiredWeapon;
        }

        private string GetMissingText()
        {
            string hexColour = ColorUtility.ToHtmlStringRGB(missingWeaponColor);
            return "<color=#" + hexColour + ">" + missingWeaponText + "</color>";
        }

        // Brings a cleaned mess back (used by MessSpawner for things like leaks)
public void ResetMess()
{
    alreadyInteracted = false;

    Collider messCollider = GetComponent<Collider>();
    if (messCollider != null)
    {
        messCollider.enabled = true;
    }

    gameObject.SetActive(true);

    RefreshState();

    // Tell the progress system this mess is back
    RepairEvents.Notify();
}
        private string GetCleanText()
        {
            return GetMessName();
        }

        private string GetMessName()
        {
            switch (messType)
            {
                case MessType.WallSmudge: return "Wipe Wall Smudge";
                case MessType.Puddle: return "Mop up Puddle";
                case MessType.Graffiti: return "Clean Graffiti";
                case MessType.Pile: return "Pick up Pile";
                case MessType.Bed: return "Make Bed";
                case MessType.Furniture: return interactText;
                default: return "Mess";
            }
        }
    }
}