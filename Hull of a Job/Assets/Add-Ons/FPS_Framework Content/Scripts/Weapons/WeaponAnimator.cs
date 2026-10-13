using System.Runtime.CompilerServices;
using UnityEngine;

namespace cowsins
{
    public class WeaponAnimator : MonoBehaviour
    {
        [SerializeField] private CameraAnimations cameraAnimations;

        [SerializeField] private Animator holsterMotionObject;

        [Header("Locomotion Transition Speeds")]
        [Tooltip("Speed of transition from Idle (0) to Walk (0.5) Lower = Smoother & Slower, Higher = Snappier & Faster")]
        [SerializeField, Range(0.1f, 10f)] private float idleToWalkSpeed;
        
        [Tooltip("Speed of transition from Walk (0.5) to Idle (0) Lower = Smoother & Slower, Higher = Snappier & Faster")]
        [SerializeField, Range(0.1f, 10f)] private float walkToIdleSpeed;
        
        [Tooltip("Speed of transition from Walk (0.5) to Run (1.0) Lower = Smoother & Slower, Higher = Snappier & Faster")]
        [SerializeField, Range(0.1f, 10f)] private float walkToRunSpeed;
        
        [Tooltip("Speed of transition from Run (1.0) to Walk (0.5) Lower = Smoother & Slower, Higher = Snappier & Faster")]
        [SerializeField, Range(0.1f, 10f)] private float runToWalkSpeed;

        public Animator HolsterMotionObject => holsterMotionObject;

        private PlayerDependencies playerDependencies;
        private WeaponStates weaponStates;
        private IPlayerMovementStateProvider player; // IPlayerMovementStateProvider is implemented in PlayerMovement.cs
        private IPlayerMovementEventsProvider playerEvents; // IPlayerMovementEventsProvider is implemented in PlayerMovement.cs
        private IWeaponReferenceProvider weaponController; // IWeaponReferenceProvider is implemented in WeaponController.cs
        private IWeaponBehaviourProvider weaponBehaviour; // IWeaponBehaviourProvider is implemented in WeaponController.cs
        private IWeaponEventsProvider weaponEvents;// IWeaponEventsProvider is implemented in WeaponController.cs
        private IInteractManagerProvider interactManager; // IWeaponReferenceProvider is implemented in InteractManager.cs

        private const string UNHOLSTER_KEY = "unholster";
        private const string SPEED_KEY = "speed";
        private const string RELOADING_KEY = "reloading";
        private const string EMPTY_RELOADING_KEY = "emptyReloading";
        private const string INSPECT_KEY = "inspect";
        private const string FINISHED_INSPECT_KEY = "finishedInspect";

        private static readonly int UNHOLSTER_HASH = Animator.StringToHash(UNHOLSTER_KEY);
        private static readonly int SPEED_HASH = Animator.StringToHash(SPEED_KEY);
        private static readonly int RELOADING_HASH = Animator.StringToHash(RELOADING_KEY);
        private static readonly int EMPTY_RELOADING_HASH = Animator.StringToHash(EMPTY_RELOADING_KEY);
        private static readonly int INSPECT_HASH = Animator.StringToHash(INSPECT_KEY);
        private static readonly int FINISHED_INSPECT_HASH = Animator.StringToHash(FINISHED_INSPECT_KEY);
        
        private float currentAnimatorSpeed = 0f;

        private void Start()
        {
            playerDependencies = GetComponent<PlayerDependencies>();
            weaponStates = GetComponent<WeaponStates>();
            player = playerDependencies.PlayerMovementState;
            playerEvents = playerDependencies.PlayerMovementEvents;
            weaponController = playerDependencies.WeaponReference;
            weaponBehaviour = playerDependencies.WeaponBehaviour;
            weaponEvents = playerDependencies.WeaponEvents;
            interactManager = playerDependencies.InteractManager;

            playerEvents.Events.OnClimbStart.AddListener(TryHideWeapon);
            playerEvents.Events.OnClimbStop.AddListener(TryShowWeapon);

            weaponEvents.Events.OnUnholster.AddListener(OnUnholster);
            weaponEvents.Events.OnSecondaryAttack.AddListener(SetParentConstraintSource);
            weaponEvents.Events.OnStartReload.AddListener(StartReload);
            weaponEvents.Events.OnCancelReload.AddListener(CancelReload); 
        }

        private void OnDestroy()
        {
            if (playerEvents != null && playerEvents.Events != null)
            {
                playerEvents.Events.OnClimbStart.RemoveListener(TryHideWeapon);
                playerEvents.Events.OnClimbStop.RemoveListener(TryShowWeapon);
            }

            if (weaponEvents != null && weaponEvents.Events != null)
            {
                weaponEvents.Events.OnUnholster.RemoveListener(OnUnholster);
                weaponEvents.Events.OnSecondaryAttack.RemoveListener(SetParentConstraintSource);
                weaponEvents.Events.OnStartReload.RemoveListener(StartReload);
                weaponEvents.Events.OnCancelReload.RemoveListener(CancelReload);
            }
        }

        private void Update()
        {
            if (weaponController == null || weaponController.Id == null) return;

            Animator currentAnimator = weaponController.Id.Animator;

            // Calculate normalized speed value where 0 = Idle, .5 = Walk, 1 = Run
            float targetSpeed = CalculateNormalizedSpeed();
            
            // Interpolate to target speed with transitions speeds
            currentAnimatorSpeed = SmoothSpeedTransition(currentAnimatorSpeed, targetSpeed);
            
            currentAnimator.SetFloat(SPEED_HASH, currentAnimatorSpeed);
        }

        /// <summary>
        /// Calculate normalized speed value where 0 = Idle, .5 = Walk, 1 = Run
        /// </summary>
        private float CalculateNormalizedSpeed()
        {
            bool forceIdle = weaponBehaviour.IsReloading || player.IsCrouching || player.IsIdle || weaponBehaviour.IsAiming || (!player.Grounded && !player.IsWallRunning);
            if (forceIdle) return 0f;

            if (player.IsWallRunning) return 0.5f;

            return player.CurrentSpeed >= player.RunSpeed ? 1f 
                 : player.CurrentSpeed > player.CrouchSpeed && !interactManager.Inspecting ? 0.5f 
                 : 0f;
        }

        private float SmoothSpeedTransition(float current, float target)
        {
            // Determine transition speed
            float transitionSpeed = GetTransitionSpeed(current, target);
            
            return Mathf.MoveTowards(current, target, transitionSpeed * Time.deltaTime);
        }

        /// <summary>
        /// Returns the appropriate transition speed based on state
        /// </summary>
        private float GetTransitionSpeed(float current, float target)
        {
            if (target == current) return 0f;

            var weapon = weaponController.Id?.weapon;
            bool hasOverride = weapon != null && weapon.locomotionTransitions.overrideTransitions;

            if (target > current)
            {
                return target <= 0.5f 
                    ? (hasOverride ? weapon.locomotionTransitions.idleToWalkSpeed : idleToWalkSpeed)
                    : (hasOverride ? weapon.locomotionTransitions.walkToRunSpeed : walkToRunSpeed);
            }
            
            return target >= 0.5f 
                ? (hasOverride ? weapon.locomotionTransitions.runToWalkSpeed : runToWalkSpeed)
                : (hasOverride ? weapon.locomotionTransitions.walkToIdleSpeed : walkToIdleSpeed);
        }

        public void StopWalkAndRunMotion()
        {
            if (weaponController == null) return; 
            Animator weapon = weaponController.Id.Animator;
            ResetSpeed(weapon);
        }

        /// <summary>
        /// Resets the speed parameter to the current movement state
        /// </summary>
        private void ResetSpeed(Animator animator)
        {
            if (animator == null) return;
            currentAnimatorSpeed = CalculateNormalizedSpeed();
            animator.SetFloat(SPEED_HASH, currentAnimatorSpeed);
        }

        private void TryHideWeapon(bool? hide)
        {
            if (hide == true) HideWeapon();
        }
        private void TryShowWeapon(bool? show)
        {
            if (show == true) ShowWeapon();
        }
        public void HideWeapon() => weaponStates.ForceChangeState(weaponStates._States.Hidden());

        public void ShowWeapon() => weaponStates.ForceChangeState(weaponStates._States.Default());

        public void SetParentConstraintSource(Transform transform) => cameraAnimations?.SetTarget(transform);

        private void OnUnholster(bool prop, bool playAnim)
        {
            var animator = weaponController.Id.GetComponentInChildren<Animator>(true);
            animator.Rebind();
            animator.Update(0f);
            animator.enabled = true;
            if (playAnim)
                CowsinsUtilities.PlayAnim(UNHOLSTER_HASH, animator);

            StopWalkAndRunMotion();
            SetParentConstraintSource(weaponController.Id.HeadBone);
        }

        private void StartReload()
        {
            if(weaponController == null) return;
            int reloadKey = weaponController.Id.bulletsLeftInMagazine == 0 ? EMPTY_RELOADING_HASH : RELOADING_HASH;
            CowsinsUtilities.PlayAnim(reloadKey, weaponController.Id.Animator);
        }

        private void CancelReload()
        {
            if (weaponController == null || weaponController.Id == null) return;
            Animator animator = weaponController.Id.Animator;
            if (animator == null) return;
            animator.CrossFadeInFixedTime("Locomotion", 0.1f, 0);
        }

        private bool IsAnimatorLocked(Animator animator)
        {
            if (animator == null) return false;

            // Current state
            if (animator.GetCurrentAnimatorStateInfo(0).IsTag("Locked"))
                return true;

            // Destination state
            if (animator.IsInTransition(0))
            {
                var nextState = animator.GetNextAnimatorStateInfo(0);
                if (nextState.IsTag("Locked"))
                    return true;
            }

            return false;
        }


        #region INSPECT
        public void InitializeInspection()
        {
            WeaponIdentification wiD = weaponController.Id;
            CowsinsUtilities.PlayAnim(INSPECT_HASH, wiD.Animator);
            CowsinsUtilities.StopAnim(FINISHED_INSPECT_HASH, wiD.Animator);
        }

        public void DisableInspection()
        {
            WeaponIdentification wID = weaponController.Id;
            CowsinsUtilities.PlayAnim(FINISHED_INSPECT_HASH, wID.Animator);
            CowsinsUtilities.StopAnim(INSPECT_HASH, wID.Animator);
        }
        #endregion
    }
}