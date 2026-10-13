using UnityEngine;
using UnityEngine.InputSystem;
using System;
using TMPro;

namespace cowsins
{
    /// <summary>
    /// Manages player inputs and broadcasts them to other scripts.
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        #region events
        public event Action OnInventoryOpenPressed, OnInventoryFavOpenPressed;
        public event Action OnDrop, OnInspect, OnMelee, OnShoot, OnStopShoot, OnTogglePause, OnToggleFlashlight,OnBackUI, OnAimPressed, OnAimReleased;
        public event Action OnJump, OnDash, OnStartGrapple, OnStopGrapple, OnMoveInputChanged, OnSprintPressed, OnSprintReleased, OnCrouchPressed, OnCrouchReleased;

        #endregion

        #region variables
        private const float MOUSE_SENSITIVITY_MULTIPLIER = 0.02f;
        private const float CONTROLLER_SENSITIVITY_MULTIPLIER = 2.5f;

        // Inputs
        [HideInInspector] public bool Jumping, Sprinting, Crouching, Dashing, Shooting, Reloading, Aiming, Melee, Inspecting, Interacting, StartInteraction, Dropping, NextWeapon, PreviousWeapon,
            Pausing, OpenInventory, OpenFavMenu, ToggleFlashlight, Grappling, BackUI, SelectUI, WestButtonUI, NorthButtonUI;
        [HideInInspector] public float X, Y, Scrolling, MouseX, MouseY, ControllerX, ControllerY;


        public static PlayerActions inputActions;
        private static int enabledInstances = 0;

        private PlayerDependencies playerDependencies;

        private Vector2 lastMoveInput;
        private bool lastSprintState, lastCrouchState, lastAimState;
        private bool alternateAiming, alternateSprint, alternateCrouch;

        private System.Action<InputAction.CallbackContext> sprintStarted, sprintCanceled;
        private System.Action<InputAction.CallbackContext> crouchStarted, crouchCanceled;
        private System.Action<InputAction.CallbackContext> aimStarted, aimCanceled;
        private System.Action<InputAction.CallbackContext> pauseHandler, inventoryOpenHandler, inventoryFavOpenHandler;
        private System.Action<InputAction.CallbackContext> dropHandler, jumpHandler, dashHandler, inspectHandler;
        private System.Action<InputAction.CallbackContext> meleeHandler, flashlightHandler, grappleStartHandler, grappleStopHandler;
        private System.Action<InputAction.CallbackContext> backUIHandler, firingStartHandler, firingStopHandler;

        #endregion

        private void Awake()
        {
            sprintStarted = ctx => HandleActionInput(ref Sprinting,ref lastSprintState,true,alternateSprint,OnSprintPressed,OnSprintReleased);
            sprintCanceled = ctx => HandleActionInput(ref Sprinting,ref lastSprintState,false,alternateSprint,OnSprintPressed,OnSprintReleased);
            crouchStarted = ctx => HandleActionInput(ref Crouching, ref lastCrouchState, true, alternateCrouch, OnCrouchPressed, OnCrouchReleased);
            crouchCanceled = ctx => HandleActionInput(ref Crouching, ref lastCrouchState, false, alternateCrouch, OnCrouchPressed, OnCrouchReleased);
            aimStarted = ctx => HandleActionInput(ref Aiming, ref lastAimState, true, alternateAiming, OnAimPressed, OnAimReleased);
            aimCanceled = ctx => HandleActionInput(ref Aiming, ref lastAimState, false, alternateAiming, OnAimPressed, OnAimReleased);
            pauseHandler = ctx => OnTogglePause?.Invoke();
            inventoryOpenHandler = ctx => OnInventoryOpenPressed?.Invoke();
            inventoryFavOpenHandler = ctx => OnInventoryFavOpenPressed?.Invoke();
            dropHandler = ctx => OnDrop?.Invoke();
            jumpHandler = ctx => OnJump?.Invoke();
            dashHandler = ctx => OnDash?.Invoke();
            inspectHandler = ctx => OnInspect?.Invoke();
            meleeHandler = ctx => OnMelee?.Invoke();
            flashlightHandler = ctx => OnToggleFlashlight?.Invoke();
            grappleStartHandler = ctx => OnStartGrapple?.Invoke();
            grappleStopHandler = ctx => OnStopGrapple?.Invoke();
            backUIHandler = ctx => OnBackUI?.Invoke();
            firingStartHandler = ctx => {
                Shooting = true;
                OnShoot?.Invoke();
            };
            firingStopHandler = ctx => {
                Shooting = false;
                OnStopShoot?.Invoke();
            };
        }

        private void OnEnable()
        {
            Init();

            // track enabled instances
            enabledInstances++;

            inputActions.GameControls.Crouching.started += crouchStarted;
            inputActions.GameControls.Crouching.canceled += crouchCanceled;
            inputActions.GameControls.Sprinting.started += sprintStarted;
            inputActions.GameControls.Sprinting.canceled += sprintCanceled;
            inputActions.GameControls.Aiming.started += aimStarted;
            inputActions.GameControls.Aiming.canceled += aimCanceled;


            inputActions.GameControls.Pause.started += pauseHandler;
            inputActions.GameControls.InventoryOpen.performed += inventoryOpenHandler;
            inputActions.GameControls.InventoryFavOpen.performed += inventoryFavOpenHandler;
            inputActions.GameControls.Drop.started += dropHandler;
            inputActions.GameControls.Jumping.started += jumpHandler;
            inputActions.GameControls.Dashing.started += dashHandler;
            inputActions.GameControls.Inspect.started += inspectHandler;
            inputActions.GameControls.Melee.started += meleeHandler;
            inputActions.GameControls.ToggleFlashLight.started += flashlightHandler;
            inputActions.GameControls.Grapple.started += grappleStartHandler;
            inputActions.GameControls.Grapple.canceled += grappleStopHandler;
            inputActions.UI.Back.started += backUIHandler;
            inputActions.GameControls.Firing.started += firingStartHandler;
            inputActions.GameControls.Firing.canceled += firingStopHandler;
        }

        private void OnDisable()
        {
            inputActions.GameControls.Sprinting.started -= sprintStarted;
            inputActions.GameControls.Sprinting.canceled -= sprintCanceled;
            inputActions.GameControls.Crouching.started -= crouchStarted;
            inputActions.GameControls.Crouching.canceled -= crouchCanceled;
            inputActions.GameControls.Aiming.started -= aimStarted;
            inputActions.GameControls.Aiming.canceled -= aimCanceled;

            inputActions.GameControls.Pause.started -= pauseHandler;
            inputActions.GameControls.InventoryOpen.performed -= inventoryOpenHandler;
            inputActions.GameControls.InventoryFavOpen.performed -= inventoryFavOpenHandler;
            inputActions.GameControls.Drop.started -= dropHandler;
            inputActions.GameControls.Jumping.started -= jumpHandler;
            inputActions.GameControls.Dashing.started -= dashHandler;
            inputActions.GameControls.Inspect.started -= inspectHandler;
            inputActions.GameControls.Melee.started -= meleeHandler;
            inputActions.GameControls.ToggleFlashLight.started -= flashlightHandler;
            inputActions.GameControls.Grapple.started -= grappleStartHandler;
            inputActions.GameControls.Grapple.canceled -= grappleStopHandler;
            inputActions.UI.Back.started -= backUIHandler;
            inputActions.GameControls.Firing.started -= firingStartHandler;
            inputActions.GameControls.Firing.canceled -= firingStopHandler;

            // Only disable the static inputActions when no InputManager instances remain enabled
            enabledInstances = Mathf.Max(0, enabledInstances - 1);
            if (enabledInstances == 0)
            {
                inputActions.Disable();
            }
        }

        private void OnDestroy()
        {
            if (enabledInstances == 0 && inputActions != null)
            {
                inputActions.Dispose();
                inputActions = null;
            }
        }
        private void Update()
        {
            if (playerDependencies == null) return;

            UpdateLookInput();
            UpdateMovementInput();
            UpdateWeaponInput();
            UpdateActionInput();
            UpdateUIInput();
        }

        private void UpdateLookInput()
        {
            if (Mouse.current != null)
            {
                bool isLocked = Cursor.lockState == CursorLockMode.Locked;
                float dx = isLocked ? Mouse.current.delta.x.ReadValue() : 0f;
                float dy = isLocked ? Mouse.current.delta.y.ReadValue() : 0f;

                MouseX = float.IsNaN(dx) ? 0f : dx;
                MouseY = float.IsNaN(dy) ? 0f : dy;
            }
            else
            {
                MouseX = 0f;
                MouseY = 0f;
            }


            if (Gamepad.current != null)
            {
                bool isLocked = Cursor.lockState == CursorLockMode.Locked;
                float dx = isLocked ? Gamepad.current.rightStick.x.ReadValue() : 0f;
                float dy = isLocked ? -Gamepad.current.rightStick.y.ReadValue() : 0f;

                ControllerX = float.IsNaN(dx) ? 0f : dx;
                ControllerY = float.IsNaN(dy) ? 0f : dy;
            }
            else
            {
                ControllerX = 0f;
                ControllerY = 0f;
            }
        }

        private void UpdateMovementInput()
        {
            var game = inputActions.GameControls;
            Vector2 moveInput = game.Movement.ReadValue<Vector2>();
            
            if (moveInput != lastMoveInput)
            {
                lastMoveInput = moveInput;
                X = moveInput.x;
                Y = moveInput.y;
                OnMoveInputChanged?.Invoke();
            }

            Dashing = game.Dashing.WasPressedThisFrame();
            Jumping = game.Jumping.WasPressedThisFrame();
            Grappling = game.Grapple.IsPressed();
        }

        private void UpdateWeaponInput()
        {
            var game = inputActions.GameControls;
            Reloading = game.Reloading.IsPressed();
            Melee = game.Melee.WasPressedThisFrame();
            Scrolling = game.Scrolling.ReadValue<Vector2>().y;

            var changeWeapons = game.ChangeWeapons;
            bool changePressed = changeWeapons.WasPressedThisFrame();
            float changeValue = changeWeapons.ReadValue<float>();
            
            NextWeapon = changePressed && changeValue > 0;
            PreviousWeapon = changePressed && changeValue < 0;
            
            Dropping = game.Drop.WasPressedThisFrame();
            Inspecting = game.Inspect.IsPressed();
        }

        private void UpdateActionInput()
        {
            var game = inputActions.GameControls;
            Interacting = game.Interacting.IsPressed();
            StartInteraction = game.Interacting.WasPressedThisFrame();
            OpenInventory = game.InventoryOpen.WasPressedThisFrame();
            OpenFavMenu = game.InventoryFavOpen.WasPressedThisFrame();
            ToggleFlashlight = game.ToggleFlashLight.WasPressedThisFrame();
            Pausing = game.Pause.WasPressedThisFrame();
        }

        private void UpdateUIInput()
        {
            var ui = inputActions.UI;
            BackUI = ui.Back.WasPressedThisFrame();
            SelectUI = ui.Select.WasPressedThisFrame();
            WestButtonUI = ui.WestButton.WasPressedThisFrame();
            NorthButtonUI = ui.NorthButton.WasPressedThisFrame();
        }



        public void ForceUncrouch()
        {
            if (Crouching)
            {
                Crouching = false;
                lastCrouchState = false;
                OnCrouchReleased?.Invoke();
            }
        }

        #region others

        private static void HandleActionInput(ref bool currentState, ref bool lastState, bool pressed, bool useToggleMode, Action onPressed, Action onReleased)
        {
            bool newState = useToggleMode ? (pressed ? !currentState : currentState) : pressed;

            if (newState == lastState)
                return;

            currentState = newState;
            lastState = newState;

            if (newState) onPressed?.Invoke();
            else onReleased?.Invoke();
        }

        public static void ToggleGameControls(bool enable)
        {
            if (enable) inputActions.GameControls.Enable();
            else inputActions.GameControls.Disable();
        }

        public static void ToggleUIControls(bool enable)
        {
            if (enable) inputActions.UI.Enable();
            else inputActions.UI.Disable();
        }

        public float GatherRawMouseX(float currentSensX, float currentControllerSensX)
        {
            // Mouse delta is already a physical movement, so we use a constant multiplier instead of delta time.
            // Controller input is a continuous rate, so we multiply by unscaledDeltaTime.
            return (MouseX * currentSensX * MOUSE_SENSITIVITY_MULTIPLIER) + 
                   (ControllerX * Time.unscaledDeltaTime * currentControllerSensX * CONTROLLER_SENSITIVITY_MULTIPLIER);
        }
        public float GatherRawMouseY(int sensYInverted, int sensYInvertedController, float currentSensY, float currentControllerSensY)
        {
            return (MouseY * currentSensY * sensYInverted * MOUSE_SENSITIVITY_MULTIPLIER) + 
                   (ControllerY * sensYInvertedController * Time.unscaledDeltaTime * currentControllerSensY * CONTROLLER_SENSITIVITY_MULTIPLIER);
        }
        private void Init()
        {
            // Initialize Inputs
            if (inputActions == null) inputActions = new PlayerActions();
            inputActions.Enable();
            
            // Load saved bindings overrides
            InputRebindingService.LoadAllBindings();

            ToggleGameControls(true);
            ToggleUIControls(false);
        }


        /// <summary>
        /// Sets the player that the InputManager will take as a reference
        /// </summary>
        /// <param name="player"></param>
        public void SetPlayer(PlayerDependencies player)
        {
            this.playerDependencies = player;
        }
        
        public void SetPlayerInputModes(PlayerMovementSettings playerSettings)
        {
            this.alternateSprint = playerSettings.alternateSprint; 
            this.alternateCrouch = playerSettings.alternateCrouch;
        }
        public void SetWeaponInputModes(WeaponControllerSettings weaponControllerSettings)
        {
            this.alternateAiming = weaponControllerSettings.alternateAiming;
        }

        #endregion
    }

}
