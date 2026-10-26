/// <summary>
/// This script belongs to cowsins™ as a part of the cowsins´ FPS Engine. All rights reserved. 
/// </summary>
using UnityEngine;
using UnityEngine.Events;
using UnityEditor;
namespace cowsins
{
    /// <summary>
    /// This component must be add to ANY interactable object in your environment.
    /// However, you might want to check other examples such as DoorInteractable.cs or PickUps
    /// They are different things, they do different things, but they both are INTERACTABLES and you can interact with both of them.
    /// Then, how does that work? Their custom scripts both inherit from interactable.cs, which inherits from MonoBehaviour, so even though they do 
    /// different things they are actually the same kind of thing.
    /// Since Interact() is a virtual void you can override it on your custom script inheriting from this one
    /// 
    /// For any doubts check the documentation or contact the support.
    /// 
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

        /// <summary>
        /// Make sure to override this on your new custom class.
        /// If you still wanna call this method, make sure to write the following line:
        /// base.Interact(player);
        /// </summary>
        public virtual void Interact(Transform player)
        {
            interactableEvents.OnInteract?.Invoke();
            alreadyInteracted = true;
            if (hideOnInteract) DisableInteraction();
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