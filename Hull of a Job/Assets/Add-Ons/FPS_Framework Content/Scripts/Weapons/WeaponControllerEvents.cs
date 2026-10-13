using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System;

namespace cowsins
{
    public class WeaponControllerEvents
    {
        // INITIALIZATION
        public readonly UnityEvent<int> OnInitializeWeaponSystem = new();

        // SHOOTING
        public readonly UnityEvent OnShoot = new();
        public readonly UnityEvent<float> OnShootShake = new();
        public readonly UnityEvent<float> OnShootApplyFOV = new();
        public readonly UnityEvent OnShootSpawnEffects = new();
        public readonly UnityEvent OnShootHitscanProjectile = new();

        // Damage / Hit Detection
        public readonly UnityEvent<int, float, RaycastHit, bool> OnHit = new();
        public readonly UnityEvent<int, RaycastHit> OnInstantiateBulletHoleImpact = new();

        // AMMO / RELOAD
        public readonly UnityEvent OnReduceAmmo = new();
        public readonly UnityEvent<bool> OnAmmoChanged = new();
        public readonly UnityEvent OnStartReload = new();
        public readonly UnityEvent OnCancelReload = new();
        public readonly UnityEvent OnFinishReload = new();
        public readonly UnityEvent<bool, bool> OnReloadUIChanged = new();
        public readonly UnityEvent OnWeaponCooling = new();

        // AIMING
        public readonly UnityEvent<float> OnAimStart = new();
        public readonly UnityEvent OnAiming = new();
        public readonly UnityEvent OnAimStop = new();

        // WEAPON SELECTION
        public readonly UnityEvent OnSelectWeapon = new();
        public readonly UnityEvent<WeaponIdentification> OnEquipWeapon = new();
        public readonly UnityEvent OnSwitchingWeapon = new();
        public readonly UnityEvent<bool, bool> OnUnholster = new();
        public readonly UnityEvent OnUnselectingWeapon = new();
        public readonly UnityEvent OnReleaseWeapon = new();

        // INVENTORY / ATTACHMENTS
        public readonly UnityEvent<int, Weapon_SO> OnWeaponInventoryChanged = new();
        public readonly UnityEvent<WeaponIdentification, int, List<AttachmentIdentifier_SO>> OnAssignAttachmentsToWeapon = new();

        // ENEMY DETECTION
        public readonly UnityEvent<bool> OnEnemySpotted = new();

        // SECONDARY ATTACK
        public readonly UnityEvent<Transform> OnSecondaryAttack = new();

        // DATA REQUESTS
        public event Func<float> OnGetSpread;
        public float RequestSpread()
        {
            // If no listeners are subscribed, return default value
            return OnGetSpread?.Invoke() ?? 0f;
        }
    }

}
