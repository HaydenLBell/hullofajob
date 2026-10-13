using System.Collections.Generic;
using UnityEngine;

namespace cowsins
{
    // Handles dash HUD Elements
    public class DashHUDModule : MonoBehaviour, IHUDModule
    {
        [SerializeField, Tooltip("Contains dashUIElements in game.")] private Transform dashUIContainer;
        [SerializeField, Tooltip("Displays a dash slot in-game. This keeps stored at dashUIContainer during runtime.")] private Transform dashUIElement;

        private List<GameObject> dashElements; // Stores the UI Elements required to display the current dashes amount
        private IPlayerMovementEventsProvider playerEvents;

        public void Initialize(PlayerDependencies dependencies)
        {
            playerEvents = dependencies.PlayerMovementEvents;

            playerEvents.Events.OnInitializeDash.AddListener(DrawDashUI);
            playerEvents.Events.OnDashUsed.AddListener(DashUsed);
            playerEvents.Events.OnDashGained.AddListener(GainDash);
        }

        private void OnDestroy()
        {
            if (playerEvents == null) return;
            playerEvents.Events.OnInitializeDash.RemoveListener(DrawDashUI);
            playerEvents.Events.OnDashUsed.RemoveListener(DashUsed);
            playerEvents.Events.OnDashGained.RemoveListener(GainDash);
        }

        /// <summary>
        /// Draws the dash UI 
        /// </summary>
        private void DrawDashUI(int amountOfDashes)
        {
            if (dashUIContainer == null || dashUIElement == null) return;   
            
            dashElements = new List<GameObject>(amountOfDashes);
            for (int i = 0; i < amountOfDashes; i++)
            {
                var uiElement = Instantiate(dashUIElement.gameObject, dashUIContainer);
                dashElements.Add(uiElement);
            }
        }

        private void GainDash(int dashIndex)
        {
            if (dashElements == null || dashElements.Count <= 0) return;

            dashIndex--;
            if (dashIndex >= 0 && dashIndex < dashElements.Count) dashElements[dashIndex].SetActive(true);
        }

        private void DashUsed(int dashIndex)
        {
            if(dashElements == null || dashElements.Count <= 0) return;

            if (dashIndex >= 0 && dashIndex < dashElements.Count) dashElements[dashIndex].SetActive(false);
        }
    }
}

#if UNITY_EDITOR
namespace cowsins { [UnityEditor.CustomEditor(typeof(DashHUDModule))] public class DashHUDModuleEditor : HUDModuleEditorBase { } }
#endif

