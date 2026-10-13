using UnityEngine;

namespace cowsins
{
    // Called when a weapon is unholstered.
    public class WeaponUnholsterState : WeaponBaseState
    {
        private float timer;
        private float duration;
        private bool isDefaultReload;

        private IPlayerControlProvider playerControl; // Reference to PlayerControl.cs ( IPlayerControlProvider is implemented in PlayerControl.cs )
        // Weapon FSM ( Finite State Machine ) is dependant on WeaponController.
        private WeaponController controller;
        private InputManager inputManager;

        public WeaponUnholsterState(WeaponStates currentContext, WeaponStateFactory playerStateFactory)
            : base(currentContext, playerStateFactory) 
        {
            playerControl = _ctx.PlayerControlProvider;
            controller = _ctx.WeaponController;
            inputManager = _ctx.Dependencies.InputManager;
        }

        public sealed override void EnterState()
        {
            // Reset timer to ensure the timing is tracked accordingly.
            timer = 0;
            isDefaultReload = controller.Weapon.reloadStyle == ReloadingStyle.defaultReload;
            duration = controller.Weapon.overrideUnholsterTime ? controller.Weapon.unholsterTime : .5f;
        }

        public sealed override void UpdateState()
        {
            if (timer < duration) timer += Time.deltaTime;
            CheckSwitchState();

            if (!playerControl.IsControllable || !playerControl.ActionsControllable)
            {
                controller.aimBehaviour?.Exit();
                return;
            }
            controller.weaponInventoryBehaviour.HandleInventory();
        }

        public sealed override void FixedUpdateState() { }

        public sealed override void ExitState() {}

        public sealed override void CheckSwitchState()
        {
            if (timer >= duration) SwitchState(_factory.Default());

            if (controller.settings.allowReloadWhileUnholstering && isDefaultReload && CanSwitchToReload(controller))
                SwitchState(_factory.Reload());
        }

        private bool CanSwitchToReload(WeaponController controller)
        {
            if (controller.Weapon == null || controller.Id == null) return false;

            bool outOfAmmo = controller.Id.bulletsLeftInMagazine <= 0;
            bool needsReload = controller.Id.bulletsLeftInMagazine < controller.Id.magazineSize;
            bool hasAmmo = controller.Id.totalBullets > 0;
            bool isNotMelee = controller.Weapon.shootStyle != ShootStyle.Melee;

            return (inputManager.Reloading && isNotMelee && needsReload && hasAmmo) ||
                   (outOfAmmo && controller.settings.autoReload && isNotMelee && hasAmmo);
        }
    }
}
