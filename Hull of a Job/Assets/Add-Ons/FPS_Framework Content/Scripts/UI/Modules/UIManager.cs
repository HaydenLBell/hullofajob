using UnityEngine;

namespace cowsins
{
    /// <summary>
    /// Coordinates HUD modules, discovers them and initializes them with PlayerDependencies.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private PauseMenu pauseMenu;

        private IHUDModule[] modules;

        private void OnEnable()
        {
            if (pauseMenu != null)
            {
                pauseMenu.OnPause += UnlockMouse;
                pauseMenu.OnUnPause += LockMouse;
            }
        }

        private void OnDisable()
        {
            if (pauseMenu != null)
            {
                pauseMenu.OnPause -= UnlockMouse;
                pauseMenu.OnUnPause -= LockMouse;
            }
        }

        public void Initialize(PlayerDependencies dependencies)
        {
            modules = GetComponentsInChildren<IHUDModule>(true);

            foreach (var module in modules)
            {
                module.Initialize(dependencies);
            }
        }

        public void UnlockMouse() => SetMouseLockState(false);

        public void LockMouse()
        {
            if (PauseMenu.isPaused) return;
            SetMouseLockState(true);
        }

        public void SetMouseLockState(bool isLocked)
        {
            Cursor.lockState = isLocked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !isLocked;
        }

        private void Update()
        {
            // Ensure the cursor is locked at the start of the game
            if (Time.timeSinceLevelLoad < 0.1f && !PauseMenu.isPaused) LockMouse();
        }
    }
}
