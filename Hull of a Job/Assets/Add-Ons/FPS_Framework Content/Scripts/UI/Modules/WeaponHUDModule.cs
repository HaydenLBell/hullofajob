using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace cowsins
{
    // Handles weapon HUD elements like ammo, magazine, heat, and currently equipped weapon icon
    public class WeaponHUDModule : MonoBehaviour, IHUDModule
    {
        [Tooltip("Attach the appropriate UI here"), SerializeField] private TextMeshProUGUI bulletsUI, magazineUI, reloadUI, lowAmmoUI;
        [Tooltip("Image that represents heat levels of your overheating weapon"), SerializeField] private Image overheatUI;
        [Tooltip("Display an icon of your current weapon"), SerializeField] private Image currentWeaponDisplay;

        private IWeaponReferenceProvider weaponController;
        private IWeaponEventsProvider weaponEvents;
        private CrosshairShape crosshairShape;

        public void Initialize(PlayerDependencies dependencies)
        {
            weaponController = dependencies.WeaponReference;
            weaponEvents = dependencies.WeaponEvents;
            crosshairShape = dependencies.Crosshair?.GetComponent<CrosshairShape>();

            // Subscribe to weapon events
            weaponEvents.Events.OnUnholster.AddListener(OnUnholster);
            weaponEvents.Events.OnUnselectingWeapon.AddListener(DisableWeaponUI);
            weaponEvents.Events.OnReleaseWeapon.AddListener(DisableWeaponUI);
            weaponEvents.Events.OnAmmoChanged.AddListener(UpdateWeaponReloadInfo);
            weaponEvents.Events.OnReloadUIChanged.AddListener(ConfigureReloadUIVisibility);

            if (weaponController.Weapon == null) DisableWeaponUI();
        }

        private void OnDestroy()
        {
            if (weaponEvents == null) return;

            weaponEvents.Events.OnUnholster.RemoveListener(OnUnholster);
            weaponEvents.Events.OnUnselectingWeapon.RemoveListener(DisableWeaponUI);
            weaponEvents.Events.OnReleaseWeapon.RemoveListener(DisableWeaponUI);
            weaponEvents.Events.OnAmmoChanged.RemoveListener(UpdateWeaponReloadInfo);
            weaponEvents.Events.OnReloadUIChanged.RemoveListener(ConfigureReloadUIVisibility);
        }

        private void ConfigureReloadUIVisibility(bool enable, bool useOverheat)
        {
            bulletsUI?.gameObject.SetActive(enable);
            magazineUI?.gameObject.SetActive(enable);
            overheatUI?.transform.parent.gameObject.SetActive(useOverheat);
        }

        private void UpdateHeatRatio(float heatRatio)
        {
            if(overheatUI != null) overheatUI.fillAmount = heatRatio;
        }

        private void UpdateBullets(int bullets, int mag, bool activeReloadUI, bool activeLowAmmoUI)
        {
            bulletsUI?.SetText("{0}", bullets);
            magazineUI?.SetText("{0}", mag);
            reloadUI?.gameObject.SetActive(activeReloadUI);
            lowAmmoUI?.gameObject.SetActive(activeLowAmmoUI);
        }

        private void UpdateWeaponReloadInfo(bool autoReload)
        {
            Weapon_SO weapon = weaponController.Weapon;
            WeaponIdentification id = weaponController.Id;

            if (weapon.reloadStyle == ReloadingStyle.defaultReload)
            {
                if (!weapon.infiniteBullets)
                {
                    bool activeReloadUI = id.bulletsLeftInMagazine == 0 && !autoReload && !weapon.infiniteBullets;
                    bool activeLowAmmoUI = id.bulletsLeftInMagazine < id.magazineSize / 3.5f && id.bulletsLeftInMagazine > 0;
                    
                    // Set different display settings for each shoot style 
                    if (weapon.limitedMagazines) UpdateBullets(id.bulletsLeftInMagazine, id.totalBullets, activeReloadUI, activeLowAmmoUI);
                    else UpdateBullets(id.bulletsLeftInMagazine, id.magazineSize, activeReloadUI, activeLowAmmoUI);
                }
                else UpdateBullets(id.bulletsLeftInMagazine, id.totalBullets, false, false);
            }
            else UpdateHeatRatio(id.heatRatio);
        }

        private void OnUnholster(bool autoReload, bool prop2)
        {
            Weapon_SO weapon = weaponController.Weapon;

            EnableDisplay();
            SetWeaponDisplay(weapon);
            UpdateWeaponReloadInfo(autoReload);
            crosshairShape?.SetCrosshair(weapon.crosshairParts);
        }

        private void DisableWeaponUI()
        {
            overheatUI?.transform.parent.gameObject.SetActive(false);
            bulletsUI?.gameObject.SetActive(false);
            magazineUI?.gameObject.SetActive(false);
            currentWeaponDisplay?.gameObject.SetActive(false);
            reloadUI?.gameObject.SetActive(false);
            lowAmmoUI?.gameObject.SetActive(false);
        }

        private void SetWeaponDisplay(Weapon_SO weapon) 
        { 
            if(currentWeaponDisplay != null) currentWeaponDisplay.sprite = weapon.icon; 
        }

        private void EnableDisplay() => currentWeaponDisplay?.gameObject.SetActive(true);
    }
}

#if UNITY_EDITOR
namespace cowsins { [UnityEditor.CustomEditor(typeof(WeaponHUDModule))] public class WeaponHUDModuleEditor : HUDModuleEditorBase { } }
#endif

