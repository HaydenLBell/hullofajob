using UnityEngine;
using UnityEngine.InputSystem;
using System;
using TMPro;

namespace cowsins
{
    public static class InputRebindingService
    {
        public static event Action OnRebindComplete;
        public static event Action OnRebindCanceled;
        public static event Action<InputAction, int> OnRebindStarted;

        public static void LoadAllBindings()
        {
            if (InputManager.inputActions == null) return;

            foreach (var actionMap in InputManager.inputActions.asset.actionMaps)
            {
                foreach (var action in actionMap.actions)
                {
                    // For each of the bindings of each of the actions, load the binding binding from PlayerPrefs
                    for (int i = 0; i < action.bindings.Count; i++)
                    {
                        LoadBindingOverride(action, i);
                    }
                }
            }
        }

        private static void LoadBindingOverride(InputAction action, int bindingIndex)
        {
            // Gather the path from the Player Prefs
            string overridePath = PlayerPrefs.GetString(action.actionMap + action.name + bindingIndex);
            // If the path is valid, apply it to the action that needs to be loaded.
            if (!string.IsNullOrEmpty(overridePath))
            {
                action.ApplyBindingOverride(bindingIndex, overridePath);
            }
        }

        public static void StartRebind(string actionName, int bindingIndex, TextMeshProUGUI statusTxt, bool excludeMouse, GameObject rebindOverlay, TextMeshProUGUI rebindOverlayTitle)
        {
            if (InputManager.inputActions == null) return;
            // Find the Input Action based on its name
            InputAction action = InputManager.inputActions.asset.FindAction(actionName);

            if (action == null || action.bindings.Count <= bindingIndex)
            {
                Debug.LogError("Action or Binding not Found");
                return;
            }

            // If it is valid check if it is a composite
            // Iterate through each each composite part and rebind it
            if (action.bindings[bindingIndex].isComposite)
            {
                var firstPartIndex = bindingIndex + 1;

                if (firstPartIndex < action.bindings.Count && action.bindings[firstPartIndex].isComposite) PerformRebind(action, bindingIndex, statusTxt, true, excludeMouse, rebindOverlay, rebindOverlayTitle);
            }
            else PerformRebind(action, bindingIndex, statusTxt, false, excludeMouse, rebindOverlay, rebindOverlayTitle);
        }

        private static void PerformRebind(InputAction actionToRebind, int bindingIndex, TextMeshProUGUI statusTxt, bool allCompositeParts, bool excludeMouse, GameObject rebindOverlay, TextMeshProUGUI rebindOverlayTitle)
        {
            if (actionToRebind == null || bindingIndex < 0)
                return;

            // Update the text status
            statusTxt.text = $"Press a {actionToRebind.expectedControlType}";
            rebindOverlay.SetActive(true);
            rebindOverlayTitle.text = $"Rebinding {actionToRebind.name}";
            actionToRebind.Disable();

            var rebind = actionToRebind.PerformInteractiveRebinding(bindingIndex);

            // Handle rebind completion
            rebind.OnComplete(operation =>
            {
                rebindOverlay.SetActive(false);
                // Enable the rebind and stop the operation
                actionToRebind.Enable();
                operation.Dispose();

                // Rebind for Composite
                if (allCompositeParts)
                {
                    var nextBindingIndex = bindingIndex + 1;
                    if (nextBindingIndex < actionToRebind.bindings.Count && actionToRebind.bindings[nextBindingIndex].isComposite) PerformRebind(actionToRebind, nextBindingIndex, statusTxt, allCompositeParts, excludeMouse, rebindOverlay, rebindOverlayTitle);
                }

                // Save the new rebinds
                SaveBindingOverride(actionToRebind);

                OnRebindComplete?.Invoke();
            });

            // Handle rebind cancel
            rebind.OnCancel(operation =>
            {
                rebindOverlay.SetActive(false);
                actionToRebind.Enable();
                operation.Dispose();

                OnRebindCanceled?.Invoke();
            });

            // Cancel rebind if pressing escape
            rebind.WithCancelingThrough("<Keyboard>/escape");

            // Exclude mouse
            if (excludeMouse)
                rebind.WithControlsExcluding("<Mouse>/escape");

            OnRebindStarted?.Invoke(actionToRebind, bindingIndex);
            // Actually start the rebind process
            rebind.Start();
        }

        public static string GetBindingName(string actionName, int bindingIndex)
        {
            if (InputManager.inputActions == null) InputManager.inputActions = new PlayerActions();

            InputAction action = InputManager.inputActions.asset.FindAction(actionName);
            return action.GetBindingDisplayString(bindingIndex);
        }

        // Save the bindings into player prefs for each action
        private static void SaveBindingOverride(InputAction action)
        {
            for (int i = 0; i < action.bindings.Count; i++)
            {
                PlayerPrefs.SetString(action.actionMap + action.name + i, action.bindings[i].overridePath);
            }
        }

        public static void LoadBindingOverride(string actionName)
        {
            if (InputManager.inputActions == null)
                InputManager.inputActions = new PlayerActions();
            // Gather the Input Action given its name
            InputAction action = InputManager.inputActions.asset.FindAction(actionName);

            // For each binding apply the binding from PlayerPrefs
            for (int i = 0; i < action.bindings.Count; i++)
            {
                if (!string.IsNullOrEmpty(PlayerPrefs.GetString(action.actionMap + action.name + i)))
                    action.ApplyBindingOverride(i, PlayerPrefs.GetString(action.actionMap + action.name + i));
            }
        }

        public static void ResetBinding(string actionName, int bindingIndex)
        {
            if (InputManager.inputActions == null) return;
            // Gather the Input Action given its name
            InputAction action = InputManager.inputActions.asset.FindAction(actionName);

            if (action == null || action.bindings.Count <= bindingIndex)
            {
                Debug.LogError("Action or Binding not found");
                return;
            }
            if (action.bindings[bindingIndex].isComposite)
            {
                for (int i = bindingIndex; i < action.bindings.Count && action.bindings[i].isComposite; i++)
                    action.RemoveBindingOverride(i);
            }
            else
                action.RemoveBindingOverride(bindingIndex);

            SaveBindingOverride(action);
        }

        public static void ResetAllBindings()
        {
            if (InputManager.inputActions == null) return;
            foreach (var actionMap in InputManager.inputActions.asset.actionMaps)
            {
                foreach (var action in actionMap.actions)
                {
                    action.RemoveAllBindingOverrides();
                    SaveBindingOverride(action);
                }
            }
            OnRebindComplete?.Invoke();
        }
    }
}
