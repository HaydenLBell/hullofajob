using UnityEngine;
using UnityEngine.Events;
using UnityEditor;

namespace hb.framework
{
    /// <summary>
    /// Base class for anything the player can interact with.
    /// Override Interact() in child classes, and call base.Interact(player) to keep the event and saving.
    /// </summary>
    public partial class Interactable : Identifiable
    {
        [HideInInspector] public bool interactable = false;

        [Tooltip("Text that will be displayed on the Interaction UI"), Title("INTERACTABLE")]
        public string interactText;

        [SerializeField] private bool instantInteraction = false;

        [Tooltip("Seconds to hold interact. -1 uses the InteractManager's default.")]
        [SerializeField] private float holdTimeOverride = -1f;

        [Tooltip("Stop showing the interaction prompt after this has been interacted with once.")]
        [SerializeField] private bool hideOnInteract = false;

        public float HoldTimeOverride => holdTimeOverride;
        public bool HideOnInteract => hideOnInteract;
        public bool InstantInteraction => instantInteraction;

        [System.Serializable]
        public class InteractableEvents // Store your events
        {
            public UnityEvent OnInteract;
        }

        public InteractableEvents interactableEvents;

        [SaveField] public bool alreadyInteracted = false;

        // Cached reference to the outline, so we only search for it once
        private OutlineAsset outline;
        private bool outlineSearched = false;

        public virtual void Interact(Transform player)
        {
            interactableEvents.OnInteract?.Invoke();
            alreadyInteracted = true;

            if (hideOnInteract)
            {
                DisableInteraction();
            }

#if SAVE_LOAD_ADD_ON
            StoreData();
#endif
        }

        /// <summary>
        /// Stops the interaction prompt from appearing by moving this object
        /// (and any child colliders) off the Interactable layer.
        /// </summary>
        protected void DisableInteraction()
        {
            interactable = false;
            Unhighlight();

            int defaultLayer = LayerMask.NameToLayer("Default");
            gameObject.layer = defaultLayer;

            foreach (Collider col in GetComponentsInChildren<Collider>())
            {
                col.gameObject.layer = defaultLayer;
                col.enabled = false;
            }
        }

        protected OutlineAsset GetOutline()
        {
            if (outlineSearched == false)
            {
                outline = GetComponent<OutlineAsset>();
                outlineSearched = true;
            }

            return outline;
        }

        public virtual void Highlight(Transform player)
        {
            OutlineAsset currentOutline = GetOutline();

            // No outline on this object, so do nothing
            if (currentOutline == null)
            {
                return;
            }

            // Already showing, so skip
            if (currentOutline.enabled)
            {
                return;
            }

            currentOutline.enabled = true;
        }

        public virtual void Unhighlight()
        {
            OutlineAsset currentOutline = GetOutline();

            if (currentOutline == null)
            {
                return;
            }

            if (currentOutline.enabled == false)
            {
                return;
            }

            currentOutline.enabled = false;
        }

        public virtual bool IsForbiddenInteraction(IWeaponReferenceProvider weaponController)
        {
            return false;
        }
    }


#if UNITY_EDITOR
    [System.Serializable]
    [CustomEditor(typeof(Interactable))]
    public class InteractableEditor : Editor
    {
        override public void OnInspectorGUI()
        {
            serializedObject.Update();
            Interactable myScript = target as Interactable;

            EditorGUILayout.Space(5);
            if (myScript.gameObject.layer != LayerMask.NameToLayer("Interactable"))
            {
                EditorGUILayout.HelpBox("Ensure your custom interactable object has the `Interactable` layer assigned.", MessageType.Error);
            }

            EditorGUILayout.Space(5);
            if (!myScript.gameObject.GetComponent<Collider>())
            {
                EditorGUILayout.HelpBox("Ensure your custom interactable object has a COLLIDER component assigned.", MessageType.Error);
            }

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("INTERACTABLE", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("interactText"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("interactableEvents"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("instantInteraction"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("holdTimeOverride"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("hideOnInteract"));

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}