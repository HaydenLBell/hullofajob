using UnityEngine;

namespace cowsins
{
    public enum MessType
    {
        WallSmudge,
        Puddle,
        Pile,
        Graffiti
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

        public bool IsRepaired => alreadyInteracted;

        private PlayerDependencies player;

        private void Start()
        {
            player = FindFirstObjectByType<PlayerDependencies>();

            if (player != null && player.WeaponEvents != null)
            {
                player.WeaponEvents.Events.OnUnholster.AddListener(OnWeaponEquipped);
                player.WeaponEvents.Events.OnUnselectingWeapon.AddListener(OnWeaponRemoved);
                player.WeaponEvents.Events.OnReleaseWeapon.AddListener(OnWeaponRemoved);
            }

            RefreshText();
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
            if (alreadyInteracted) return;
            if (!HasRequiredWeapon()) return;

            base.Interact(playerTransform); // sets alreadyInteracted and fires OnInteract
            RepairEvents.Notify();

            if (hideOnClean) gameObject.SetActive(false);
            else GetComponent<Collider>().enabled = false; // stop re-interacting
        }

        private void OnWeaponEquipped(bool autoReload, bool prop2) => RefreshText();

        // Nothing is equipped at this point, so the requirement can't be met
        private void OnWeaponRemoved()
        {
            interactText = requiredWeapon == null ? GetCleanText() : GetMissingText();
        }

        private void RefreshText()
        {
            interactText = HasRequiredWeapon() ? GetCleanText() : GetMissingText();
        }

        private bool HasRequiredWeapon()
        {
            if (requiredWeapon == null) return true;
            if (player == null || player.WeaponReference == null) return false;

            return player.WeaponReference.Weapon == requiredWeapon;
        }

        private string GetMissingText() => $"<color=#{ColorUtility.ToHtmlStringRGB(missingWeaponColor)}>{missingWeaponText}</color>";

        private string GetCleanText() => "" + GetMessName();

        private string GetMessName()
        {
            switch (messType)
            {
                case MessType.WallSmudge: return "Wipe Wall Smudge";
                case MessType.Puddle: return "Mop up Puddle";
                case MessType.Graffiti: return "Clean Graffiti";
                case MessType.Pile: return "Pick up Pile";
                default: return "Mess";
            }
        }


    }
}