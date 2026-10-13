using UnityEngine;

namespace cowsins
{
    public class WeaponInspectState : WeaponBaseState
    {
        private readonly WeaponController weaponController;
        private readonly WeaponAnimator weaponAnimator;
        private readonly InteractManager interactManager;
        private readonly IPlayerControlProvider playerControlProvider; // IPlayerControlProvider is implemented in PlayerControl.cs
        private readonly IPlayerMovementStateProvider playerMovementStateProvider; // IPlayerMovementStateProvider is implemented in PlayerMovement.cs
        private readonly IInteractEventsProvider interactEventsProvider; // IInteractEventsProvider is implemented in InteractManager.cs
        private readonly InputManager inputManager;

        private float timer;
        private const float MinInspectTime = 1f;

        public WeaponInspectState(WeaponStates currentContext, WeaponStateFactory playerStateFactory)
            : base(currentContext, playerStateFactory)
        {
            PlayerDependencies playerDependencies = _ctx.Dependencies;
            playerMovementStateProvider = _ctx.PlayerMovement;
            weaponController = _ctx.WeaponController;
            weaponAnimator = _ctx.WeaponAnimator;
            interactManager = _ctx.InteractManager;
            playerControlProvider = _ctx.PlayerControlProvider;
            interactEventsProvider = playerDependencies.InteractEvents;
            inputManager = playerDependencies.InputManager;
        }

        public sealed override void EnterState()
        {
            timer = 0;

            weaponAnimator.InitializeInspection();
            interactManager.ToggleInspectionState(true);

            if (interactManager.RealtimeAttachmentCustomization)
                interactEventsProvider.Events.OnStartRealtimeInspection?.Invoke(interactManager.DisplayCurrentAttachmentsOnly);

            inputManager.OnInspect += SwitchToDefault;
        }


        public sealed override void UpdateState()
        {
            if (interactManager.RealtimeAttachmentCustomization) playerControlProvider.LoseControl();

            if (timer <= MinInspectTime) timer += Time.deltaTime;

            weaponController.aimBehaviour?.Exit();

            CheckSwitchState();
        }

        public sealed override void FixedUpdateState() { }

        public sealed override void ExitState()
        {
            interactManager.ToggleInspectionState(false);
            weaponAnimator.DisableInspection();
            playerControlProvider.CheckIfCanGrantControl();

            UIEvents.onEnableAttachmentUI?.Invoke(null);
            interactManager.Events.OnStopInspect?.Invoke();

            inputManager.OnInspect -= SwitchToDefault;
        }
        public sealed override void CheckSwitchState()
        {
            if (timer < MinInspectTime) return;
            if (inputManager.Shooting && !interactManager.RealtimeAttachmentCustomization || playerMovementStateProvider.CurrentSpeed == playerMovementStateProvider.RunSpeed) SwitchState(_factory.Default());
        }

        private void SwitchToDefault()
        {
            if (timer < MinInspectTime) return;
            SwitchState(_factory.Default());
        }
    }
}
