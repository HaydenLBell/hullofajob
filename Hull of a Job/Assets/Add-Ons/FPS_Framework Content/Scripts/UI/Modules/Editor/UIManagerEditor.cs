#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace cowsins
{
    [CustomEditor(typeof(UIManager))]
    public class UIManagerEditor : Editor
    {
        private int currentTab = 0;
        private string[] tabNames;
        private MonoBehaviour[] modules;
        private Editor[] moduleEditors;

        private void OnEnable()
        {
            UIManager uiManager = (UIManager)target;
            
            // Find all components implementing IHUDModule on this GameObject or children
            var foundModules = uiManager.GetComponentsInChildren<MonoBehaviour>(true).Where(c => c is IHUDModule).OrderBy(c => c.GetType().Name).ToArray();

            modules = foundModules;
            tabNames = new string[modules.Length];
            moduleEditors = new Editor[modules.Length];

            for (int i = 0; i < modules.Length; i++)
            {
                // Clean up the name for the tab
                string name = modules[i].GetType().Name;
                if (name.EndsWith("HUDModule")) name = name.Substring(0, name.Length - 9);
                tabNames[i] = name;
            }
        }

        private void OnDisable()
        {
            // Clean up cached editors
            if (moduleEditors != null)
            {
                foreach (var editor in moduleEditors)
                {
                    if (editor != null) DestroyImmediate(editor);
                }
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space(10f);
            
            // Draw default properties of UIManager (like PauseMenu)
            DrawPropertiesExcluding(serializedObject, "m_Script");

            EditorGUILayout.Space(10f);

            if (tabNames.Length > 0)
            {
                // Ensure currentTab is within bounds
                if (currentTab >= tabNames.Length) currentTab = 0;

                // Draw Tabs
                int tabsPerRow = 4;
                int rows = Mathf.CeilToInt((float)tabNames.Length / tabsPerRow);
                
                for (int r = 0; r < rows; r++)
                {
                    int length = Mathf.Min(tabsPerRow, tabNames.Length - r * tabsPerRow);
                    string[] rowNames = new string[length];
                    System.Array.Copy(tabNames, r * tabsPerRow, rowNames, 0, length);
                    
                    int selected = GUILayout.Toolbar(
                        currentTab >= r * tabsPerRow && currentTab < (r + 1) * tabsPerRow ? currentTab - r * tabsPerRow : -1, 
                        rowNames
                    );
                    
                    if (selected != -1)
                    {
                        currentTab = r * tabsPerRow + selected;
                    }
                }

                EditorGUILayout.Space(10f);
                EditorGUILayout.EndVertical();

                // Draw the selected module's editor
                if (currentTab >= 0 && currentTab < modules.Length)
                {
                    MonoBehaviour selectedModule = modules[currentTab];
                    if (selectedModule != null)
                    {
                        if (moduleEditors[currentTab] == null)
                        {
                            moduleEditors[currentTab] = Editor.CreateEditor(selectedModule);
                        }

                        EditorGUILayout.BeginVertical("box");
                        EditorGUILayout.LabelField(tabNames[currentTab].ToUpper(), EditorStyles.boldLabel);
                        EditorGUILayout.Space(5f);
                        
                        SerializedObject so = moduleEditors[currentTab].serializedObject;
                        so.Update();
                        
                        // If the editor is a HUDModuleEditorBase, invoke its custom property drawing method
                        if (moduleEditors[currentTab] is HUDModuleEditorBase hudEditor)
                        {
                            hudEditor.DrawModuleProperties();
                        }
                        else
                        {
                            // Fallback for modules that don't inherit HUDModuleEditorBase
                            SerializedProperty iterator = so.GetIterator();
                            bool enterChildren = true;
                            while (iterator.NextVisible(enterChildren))
                            {
                                if (iterator.propertyPath != "m_Script")
                                {
                                    EditorGUILayout.PropertyField(iterator, true);
                                }
                                enterChildren = false;
                            }
                        }
                        
                        so.ApplyModifiedProperties();

                        EditorGUILayout.EndVertical();
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox("No HUD Modules found in children. Please add HUD modules to the children of this UI Manager.", MessageType.Warning);
                EditorGUILayout.EndVertical();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
