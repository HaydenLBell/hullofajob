using System.Collections;
using UnityEngine;

namespace cowsins
{
    // Handles weapon inventory UI
    public class InventoryHUDModule : MonoBehaviour, IHUDModule
    {
        [Tooltip("Attach the CanvasGroup that contains the inventory"), SerializeField] private CanvasGroup inventoryContainer;
        [SerializeField] private WeaponsInventoryUISlot inventoryUISlot;
        [SerializeField] private Crosshair crosshair;

        public WeaponsInventoryUISlot[] weaponsInventoryUISlots { get; private set; }
        public Crosshair Crosshair => crosshair;
        public CrosshairShape crosshairShape { get; private set; }

        private Coroutine inventoryFadeCoroutine;
        private IWeaponReferenceProvider weaponController;
        private IWeaponEventsProvider weaponEvents;
        private IInteractEventsProvider interactEvents;

        public void Initialize(PlayerDependencies dependencies)
        {
            this.weaponController = dependencies.WeaponReference;
            this.weaponEvents = dependencies.WeaponEvents;
            this.interactEvents = dependencies.InteractEvents;

            crosshairShape = crosshair?.GetComponent<CrosshairShape>();

            if (weaponEvents != null)
            {
                weaponEvents.Events.OnSwitchingWeapon.AddListener(StartFadingInventory);
                weaponEvents.Events.OnSelectWeapon.AddListener(SelectInventoryUISlot);
                weaponEvents.Events.OnWeaponInventoryChanged.AddListener(SetInventoryUISlotWeapon);
                weaponEvents.Events.OnInitializeWeaponSystem.AddListener(CreateInventoryUI);
            }

            if (interactEvents != null) interactEvents.Events.OnDrop.AddListener(ResetCrosshairToDefault);

            StartFadingInventory();
        }

        private void OnDestroy()
        {
            if (weaponEvents != null)
            {
                weaponEvents.Events.OnSwitchingWeapon.RemoveListener(StartFadingInventory);
                weaponEvents.Events.OnSelectWeapon.RemoveListener(SelectInventoryUISlot);
                weaponEvents.Events.OnWeaponInventoryChanged.RemoveListener(SetInventoryUISlotWeapon);
                weaponEvents.Events.OnInitializeWeaponSystem.RemoveListener(CreateInventoryUI);
            }

            if (interactEvents != null) interactEvents.Events.OnDrop.RemoveListener(ResetCrosshairToDefault);
        }

        private void StartFadingInventory()
        {
            if (inventoryContainer != null) inventoryContainer.alpha = 1f;

            if (!gameObject.activeInHierarchy) return;

            if (inventoryFadeCoroutine != null) StopCoroutine(inventoryFadeCoroutine);
            inventoryFadeCoroutine = StartCoroutine(FadeInventory());
        }

        private IEnumerator FadeInventory()
        {
            if (inventoryContainer == null) yield break;

            while (inventoryContainer.alpha > 0)
            {
                inventoryContainer.alpha -= Time.deltaTime;
                yield return null;
            }
        }

        /// <summary>
        /// Procedurally generate the Inventory UI
        /// </summary>
        private void CreateInventoryUI(int inventorySize)
        {
            // Adjust the inventory size 
            weaponsInventoryUISlots = new WeaponsInventoryUISlot[inventorySize];

            if (inventoryContainer == null || inventoryUISlot == null) return;

            foreach (Transform child in inventoryContainer.transform) Destroy(child.gameObject);

            int i = 0; // Control variable
            while (i < inventorySize)
            {
                // Load the slot, instantiate it and set it to the slots array
                WeaponsInventoryUISlot slot = Instantiate(inventoryUISlot, Vector3.zero, Quaternion.identity, inventoryContainer.transform) as WeaponsInventoryUISlot;
                slot.Initialize(i);
                weaponsInventoryUISlots[i] = slot;
                i++;
            }
        }

        public void SelectInventoryUISlot()
        {
            if (weaponController == null) return;

            int selectedIndex = weaponController.CurrentWeaponIndex;
            if (weaponsInventoryUISlots == null || selectedIndex >= weaponsInventoryUISlots.Length) return;

            foreach (WeaponsInventoryUISlot slot in weaponsInventoryUISlots)
            {
                slot?.Deselect();
            }
            weaponsInventoryUISlots[selectedIndex]?.Select();
        }

        public void SetInventoryUISlotWeapon(int selectedIndex, Weapon_SO newWeapon)
        {
            if (weaponsInventoryUISlots == null || selectedIndex >= weaponsInventoryUISlots.Length) return;

            weaponsInventoryUISlots[selectedIndex]?.SetWeapon(newWeapon);
            // update crosshair if the inventory slot being updated corresponds to the currently equipped weapon.
            if (weaponController != null && weaponController.CurrentWeaponIndex == selectedIndex)
            {
                crosshairShape?.SetCrosshair(newWeapon?.crosshairParts);
            }
        }

        private void ResetCrosshairToDefault() => crosshairShape?.ResetCrosshairToDefault();
    }
}

#if UNITY_EDITOR
namespace cowsins { [UnityEditor.CustomEditor(typeof(InventoryHUDModule))] public class InventoryHUDModuleEditor : HUDModuleEditorBase { } }
#endif

