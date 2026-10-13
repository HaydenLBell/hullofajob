using UnityEngine;
#if INVENTORY_PRO_ADD_ON
using cowsins.Inventory;
#endif
namespace cowsins
{
    public partial class BulletsPickeable : Pickeable
    {
        [Tooltip("How many bullets you will get"), SerializeField, SaveField] private int amountOfBullets;

        [SerializeField] private BulletTypeIdentifier_SO bulletTypeIdentifier;

        public int AmountOfBullets => amountOfBullets;
        public BulletTypeIdentifier_SO BulletTypeIdentifier => bulletTypeIdentifier;

        public override void Awake()
        {
            base.Awake();
            if (bulletTypeIdentifier == null) CowsinsUtilities.LogError("<b><color=yellow>Bullet_SO</color></b> not found!", this);
            GetVisuals();
        }

        public void GetVisuals()
        {
            if (bulletTypeIdentifier == null) return;

            if (image != null)
                image.sprite = bulletTypeIdentifier.icon;

            if (graphics != null && bulletTypeIdentifier.pickUpGraphics != null)
            {
                foreach (Transform child in graphics.transform)
                {
                    Destroy(child.gameObject);
                }
                Instantiate(bulletTypeIdentifier.pickUpGraphics, graphics);
            }
        }

        public override void Interact(Transform player)
        {
            if (bulletTypeIdentifier == null)
            {
                CowsinsUtilities.LogError("<b><color=yellow>Bullet_SO</color></b> not found! Skipping Interaction.", this);
                return;
            }

            PlayerDependencies playerDependencies = player.GetComponent<PlayerDependencies>() ?? player.GetComponentInParent<PlayerDependencies>() ?? player.GetComponentInChildren<PlayerDependencies>();
            IWeaponReferenceProvider weaponReference = playerDependencies != null ? playerDependencies.WeaponReference : player.GetComponentInChildren<IWeaponReferenceProvider>() ?? player.GetComponentInParent<IWeaponReferenceProvider>();

#if INVENTORY_PRO_ADD_ON
            if (InventoryProManager.instance)
            {
                (bool success, int remainingAmount) = InventoryProManager.instance._GridGenerator.Operations.AddItemToInventory(bulletTypeIdentifier, amountOfBullets);
                if (success)
                {
                    alreadyInteracted = true;
                    interactableEvents.OnInteract?.Invoke();
                    StoreData();
                    ToastManager.Instance?.ShowToast($"x{amountOfBullets - remainingAmount} {ToastManager.Instance.CollectedMsg}");
                    amountOfBullets = remainingAmount;
                    if(amountOfBullets <= 0) SpawnService.Despawn(this.gameObject);
                }
                else
                    ToastManager.Instance?.ShowToast(ToastManager.Instance.InventoryIsFullMsg);
                return;
            }
#else
            if (weaponReference == null || weaponReference.Weapon == null)
            {
                alreadyInteracted = false;
                return;
            }
#endif
            alreadyInteracted = true; 
            base.Interact(player);

            if (weaponReference != null && weaponReference.Id != null)
            {
                weaponReference.Id.totalBullets += amountOfBullets;
            }
            if (playerDependencies != null && playerDependencies.WeaponEvents != null)
            {
                playerDependencies.WeaponEvents.Events.OnAmmoChanged?.Invoke(false); 
            }
            SpawnService.Despawn(this.gameObject);
        }

        public void SetBullets(BulletTypeIdentifier_SO bulletsSO, int amountOfBullets)
        {
            this.amountOfBullets = amountOfBullets;
            this.bulletTypeIdentifier = bulletsSO;
            GetVisuals();
        }

        public override bool IsForbiddenInteraction(IWeaponReferenceProvider weaponController)
        {
            Weapon_SO weapon = weaponController.Weapon;

            return AddonManager.instance.isInventoryAddonAvailable
                ? false
                : weapon != null && !weapon.limitedMagazines || weapon != null && weapon.limitedMagazines && !BulletTypeIdentifier_SO.IsCompatibleWith(bulletTypeIdentifier, weapon.bulletTypeIdentifier) || weaponController.Weapon == null;
        }


#if SAVE_LOAD_ADD_ON
        // Destroy if picked up.
        // Interacted State is called after loading.
        public override void LoadedState()
        {
            if (this.alreadyInteracted) Destroy(this.gameObject);
        }
#endif
    }
}
