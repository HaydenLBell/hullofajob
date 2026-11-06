using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace hb.framework
{
    public class EndLevelMenu : MonoBehaviour
    {
        [SerializeField] private PlayerDependencies playerDependencies;
        [SerializeField] private GameObject playerUI;

        [Tooltip("The panel to show. Must be a CHILD of this object, not this object itself.")]
        [SerializeField] private CanvasGroup menu;

        [SerializeField] private Selectable firstSelectedItem;
        [SerializeField] private float fadeSpeed = 1.5f;
        [SerializeField] private UnityEvent onShowEvents;

        public static bool IsShowing { get; private set; }

        private void Awake()
        {
            IsShowing = false;

            // Start hidden
            if (menu != null)
            {
                menu.alpha = 0f;
                menu.gameObject.SetActive(false);
            }
        }

        // Call this from the ladder's OnInteract event
        public void Show()
        {
            if (IsShowing)
            {
                return;
            }

            IsShowing = true;

            // Stop the player moving and looking around
            if (playerDependencies != null && playerDependencies.PlayerControl != null)
            {
                playerDependencies.PlayerControl.LoseControl();
            }

            // Stop Esc opening the pause menu on top of the end screen
            if (PauseMenu.Instance != null)
            {
                PauseMenu.Instance.enabled = false;
            }

            if (playerUI != null)
            {
                playerUI.SetActive(false);
            }

            // Free the mouse so the buttons can be clicked
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            StartCoroutine(FadeIn());

            if (Gamepad.current != null && firstSelectedItem != null && EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(firstSelectedItem.gameObject);
            }

            onShowEvents.Invoke();
        }

        private IEnumerator FadeIn()
        {
            menu.gameObject.SetActive(true);

            while (menu.alpha < 1f)
            {
                menu.alpha = menu.alpha + Time.deltaTime * fadeSpeed;
                yield return null;
            }

            menu.alpha = 1f;
        }

        // ---------- Buttons ----------

        public void RestartLevel()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        // Type the scene name into the button's OnClick, e.g. "MainMenu" or "Contract02"
        public void LoadScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }

        public void QuitGame()
        {
            Application.Quit();
        }
    }
}