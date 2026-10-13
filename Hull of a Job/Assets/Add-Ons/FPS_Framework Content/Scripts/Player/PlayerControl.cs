using UnityEngine;

namespace cowsins
{
    // This class controls the controllable state of the player.
    // If non-controllable ( controllable == false ), the player won´t be able to move or perform any action.
    // GrantControl, LoseControl, ToggleControl and CheckIfCanGrantControl methods can be accessed to handle the player control.
    public class PlayerControl : MonoBehaviour, IPlayerControlProvider
    {
        public bool IsControllable => controllable;
        public bool IsMovementControllable => movementControllable;
        public bool CameraControllable => cameraControllable;
        public bool ActionsControllable => actionsControllable;
        public bool ShootingControllable => shootingControllable;

        private bool controllable = true;
        private bool movementControllable = true;
        private bool cameraControllable = true;
        private bool actionsControllable = true;
        private bool shootingControllable = true;

        // Reference to PlayerStats.cs ( IPlayerStatsProvider is implemented in PlayerStats.cs )
        private IPlayerStatsProvider playerStatusProvider; 

        private void Awake()
        {
            playerStatusProvider = GetComponent<IPlayerStatsProvider>();
            GrantControl();
        }

        /***************************************** GLOBAL/CORE CONTROL *************************************************/

        /// <summary>
        /// Forces the player to be controlled. CheckIfCanGrantControl() method is recommended instead.
        /// </summary>
        public void GrantCameraControl() => cameraControllable = true;
        public void LoseCameraControl() => cameraControllable = false;

        public void GrantControl()
        {
            controllable = true;
            GrantMovementControl();
            GrantCameraControl();
            GrantActionsControl();
            GrantShootingControl();
        } 
            

        /// <summary>
        /// Disallows the player to be controlled.
        /// </summary>
        public void LoseControl()
        {
            controllable = false;
            LoseMovementControl();
            LoseCameraControl();
            LoseActionsControl();
            LoseShootingControl();
        }

        /// <summary>
        /// Toggles the controllable state of the player.
        /// </summary>
        public void ToggleControl() => controllable = !controllable;

        /// <summary>
        /// Checks if the game is paused or the player is dead before allowing the player to be controlled.
        /// </summary>
        public void CheckIfCanGrantControl()
        {
            if (PauseMenu.isPaused || playerStatusProvider?.IsDead == true) return;
            GrantControl();
        }


        /***************************************** MOVEMENT CONTROL *************************************************/
        /// <summary>
        /// Allows movement. If Global Control is disabled, this will have no effect.
        /// </summary>
        public void GrantMovementControl() => movementControllable = true;

        /// <summary>
        /// Disallows the player movement. This is independent to the Global Control System
        /// </summary>
        public void LoseMovementControl() => movementControllable = false;

        /// <summary>
        /// Toggles the movement controllable state of the player.
        /// </summary>
        public void ToggleMovementControl() => movementControllable = !movementControllable;

        /***************************************** ACTIONS CONTROL *************************************************/
        
        /// <summary>
        /// Allows actions (shooting, reloading, interacting, switching weapons)
        /// </summary>
        public void GrantActionsControl() => actionsControllable = true;

        /// <summary>
        /// Disallows player actions.
        /// </summary>
        public void LoseActionsControl() => actionsControllable = false;

        /// <summary>
        /// Toggles the actions controllable state of the player.
        /// </summary>
        public void ToggleActionsControl() => actionsControllable = !actionsControllable;

        /***************************************** SHOOTING CONTROL *************************************************/
        
        /// <summary>
        /// Allows shooting.
        /// </summary>
        public void GrantShootingControl() => shootingControllable = true;

        /// <summary>
        /// Disallows shooting.
        /// </summary>
        public void LoseShootingControl() => shootingControllable = false;

        /// <summary>
        /// Toggles the shooting controllable state of the player.
        /// </summary>
        public void ToggleShootingControl() => shootingControllable = !shootingControllable;
    }
}
