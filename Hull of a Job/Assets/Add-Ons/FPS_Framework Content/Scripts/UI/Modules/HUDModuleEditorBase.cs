#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace cowsins
{
    public class HUDModuleEditorBase : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.Space();

            string moduleName = target.GetType().Name;

            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button) { fontStyle = FontStyle.Bold };
            
            if (GUILayout.Button($"You're editing {moduleName}, go to UIManager", buttonStyle, GUILayout.Height(30)))
            {
                UIManager uiManager = ((MonoBehaviour)target).GetComponentInParent<UIManager>();
                if (uiManager != null)
                {
                    Selection.activeGameObject = uiManager.gameObject;
                }
                else
                {
                    Debug.LogWarning("No UIManager found in parent hierarchy.");
                }
            }

            EditorGUILayout.Space();
            
            DrawModuleProperties();
        }

        public virtual void DrawModuleProperties()
        {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "m_Script");
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
