#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using System.Reflection;
using UnityEngine.Rendering;

namespace cowsins
{
    public class MigrationCowsinsManagerTab : ITab
    {
        public string TabName => "Migration";
        // Always show this tab, even in BiRP, to inform users about the requirement
        public int Order => 1;
        public bool IsVisible => true;
        
        private GameObject playerPrefab;
        private Vector2 scrollPos;
        
        // Step 3
        private System.Collections.Generic.List<Material> incompatibleMaterials = new System.Collections.Generic.List<Material>();
        private bool hasScannedMaterials = false;
        private Vector2 materialScrollPos;

        private enum PipelineType { BiRP, URP, HDRP, Unknown }

        public void StartTab()
        {
            // Look for the player prefab
            if (playerPrefab == null) playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Cowsins/Prefabs/PlayerControllers/CowsinsFPSController.prefab");
        }

        private PipelineType GetCurrentPipeline()
        {
            RenderPipelineAsset currentPipeline = GraphicsSettings.currentRenderPipeline;
            if (currentPipeline == null) return PipelineType.BiRP;

            string pipelineName = currentPipeline.GetType().Name;
            if (pipelineName.Contains("UniversalRenderPipelineAsset") || pipelineName.Contains("Universal"))
                return PipelineType.URP;
            if (pipelineName.Contains("HDRenderPipelineAsset") || pipelineName.Contains("HD"))
                return PipelineType.HDRP;
                
            return PipelineType.Unknown;
        }

        public void OnGUI()
        {
            scrollPos = GUILayout.BeginScrollView(scrollPos);

            PipelineType pipeline = GetCurrentPipeline();
            string targetRP = pipeline == PipelineType.URP ? "URP" : (pipeline == PipelineType.HDRP ? "HDRP" : "URP/HDRP");

            GUILayout.Label($"Migration Tool (BiRP to {targetRP})", EditorStyles.boldLabel);
            GUILayout.Space(10);

            if (pipeline == PipelineType.BiRP)
            {
                EditorGUILayout.HelpBox("This project is currently using the Built-in Render Pipeline (BiRP). If you intend to use URP or HDRP, please redirect to the corresponding tutorial below.", MessageType.Warning);
                GUILayout.Space(5);
                GUILayout.BeginHorizontal();
                CowsinsEditorWindowUtilities.DrawTutorialCard(Resources.Load<Texture2D>("CustomEditor/URP"), "https://www.cowsins.com/videos/PyAywKlqKrY", .7f);
                CowsinsEditorWindowUtilities.DrawTutorialCard(Resources.Load<Texture2D>("CustomEditor/hdrp"), "https://www.cowsins.com/videos/kyJFwmcs-6Q", .7f);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
            else if (pipeline == PipelineType.Unknown)
            {
                // Unknown RP
            }
            else
            {
                DrawPlayerPrefabSection(pipeline);
                DrawUpdateMaterialsSection(pipeline);
                DrawIncompatibleMaterialsSection(pipeline);
            }

            GUILayout.EndScrollView();
        }

        private void DrawPlayerPrefabSection(PipelineType pipeline)
        {
            GUILayout.Label("1. Setup Player Prefab", EditorStyles.boldLabel);
            
            GUILayout.BeginHorizontal();
            playerPrefab = (GameObject)EditorGUILayout.ObjectField("Player Prefab", playerPrefab, typeof(GameObject), false);
            
            if (GUILayout.Button("Scan", GUILayout.Width(60)))
            {
                ScanForPlayerPrefab();
            }
            GUILayout.EndHorizontal();

            if (playerPrefab != null)
            {
                GUILayout.Space(10);
                string buttonText = pipeline == PipelineType.URP ? "Migrate Player Prefab to URP" : "Migrate Player Prefab to HDRP";
                
                if (GUILayout.Button(buttonText, GUILayout.Height(30)))
                {
                    if (playerPrefab.GetComponentInChildren<PlayerMovement>(true) == null)
                    {
                        EditorUtility.DisplayDialog("Error", "The selected prefab does not contain a PlayerMovement component. Please select a valid Player Prefab.", "OK");
                    }
                    else
                    {
                        if (pipeline == PipelineType.URP) MigrateToURP();
                        else MigrateToHDRP();
                    }
                }
            }
        }

        private void DrawUpdateMaterialsSection(PipelineType pipeline)
        {
            GUILayout.Space(20);
            GUILayout.Label("2. Update Materials", EditorStyles.boldLabel);

            if (pipeline == PipelineType.URP)
            {
                if (GUILayout.Button("Open Render Pipeline Converter", GUILayout.Height(30)))
                {
                    EditorApplication.ExecuteMenuItem("Window/Rendering/Render Pipeline Converter");
                }
            }
            else if (pipeline == PipelineType.HDRP)
            {
                if (GUILayout.Button("Open HDRP Wizard", GUILayout.Height(30)))
                {
                    EditorApplication.ExecuteMenuItem("Window/Rendering/HDRP Wizard");
                }
            }
        }

        private void DrawIncompatibleMaterialsSection(PipelineType pipeline)
        {
            GUILayout.Space(20);
            GUILayout.Label("3. Incompatible Materials", EditorStyles.boldLabel);
            
            if (!hasScannedMaterials)
            {
                if (GUILayout.Button("Scan for Incompatible Materials", GUILayout.Height(30)))
                {
                    if (EditorUtility.DisplayDialog("Scan Materials", "This process will scan all materials in your project and may take a few moments. Do you want to proceed?", "Yes, Scan", "Cancel"))
                    {
                        ScanForIncompatibleMaterials(pipeline);
                    }
                }
            }
            else
            {
                if (incompatibleMaterials.Count == 0)
                {
                    GUILayout.Label("No incompatible materials found!", EditorStyles.boldLabel);
                }
                else
                {
                    GUILayout.Label($"Found {incompatibleMaterials.Count} incompatible materials.");
                    
                    materialScrollPos = GUILayout.BeginScrollView(materialScrollPos, GUILayout.Height(140));
                    GUILayout.BeginHorizontal();
                    
                    for (int i = 0; i < incompatibleMaterials.Count; i++)
                    {
                        Material mat = incompatibleMaterials[i];
                        if (mat == null) continue;

                        GUILayout.BeginVertical("box", GUILayout.Width(90));
                        
                        Texture2D preview = AssetPreview.GetAssetPreview(mat);
                        if (preview != null)
                        {
                            GUILayout.Label(preview, GUILayout.Width(80), GUILayout.Height(80));
                        }
                        else
                        {
                            GUILayout.Box("No Preview", GUILayout.Width(80), GUILayout.Height(80));
                        }

                        // Truncate name if too long
                        string displayName = mat.name.Length > 12 ? mat.name.Substring(0, 10) + ".." : mat.name;
                        GUILayout.Label(displayName, EditorStyles.miniLabel, GUILayout.Width(80));

                        GUILayout.BeginHorizontal();
                        if (GUILayout.Button("View", GUILayout.Width(40)))
                        {
                            Selection.activeObject = mat;
                            EditorGUIUtility.PingObject(mat);
                        }
                        if (GUILayout.Button("Fix", GUILayout.Width(40)))
                        {
                            FixMaterial(mat, pipeline);
                            EditorUtility.DisplayDialog("Warning", "Revise your materials and ensure they've been updated properly", "OK");
                        }
                        GUILayout.EndHorizontal();

                        GUILayout.EndVertical();
                    }
                    
                    GUILayout.EndHorizontal();
                    GUILayout.EndScrollView();
                }

                GUILayout.Space(5);
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Rescan", GUILayout.Height(30)))
                {
                    if (EditorUtility.DisplayDialog("Rescan Materials", "This process will scan all materials in your project and may take a few moments. Do you want to proceed?", "Yes, Rescan", "Cancel"))
                    {
                        ScanForIncompatibleMaterials(pipeline);
                    }
                }
                
                if (incompatibleMaterials.Count > 0)
                {
                    if (GUILayout.Button("Fix All", GUILayout.Height(30)))
                    {
                        if (EditorUtility.DisplayDialog("Fix All Materials", $"This will forcefully upgrade all {incompatibleMaterials.Count} incompatible materials to {(pipeline == PipelineType.URP ? "URP" : "HDRP")}. This action may take a while. Proceed?", "Yes, Fix All", "Cancel"))
                        {
                            FixAllMaterials(pipeline);
                        }
                    }
                }
                GUILayout.EndHorizontal();
            }
        }

        private void ScanForIncompatibleMaterials(PipelineType pipeline)
        {
            incompatibleMaterials.Clear();
            string[] guids = AssetDatabase.FindAssets("t:Material");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                
                Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mat != null && IsMaterialIncompatible(mat, pipeline))
                {
                    incompatibleMaterials.Add(mat);
                }
            }
            hasScannedMaterials = true;
        }

        private bool IsMaterialIncompatible(Material mat, PipelineType pipeline)
        {
            if (mat.shader == null) return true;
            string shaderName = mat.shader.name;

            // Known compatible shaders
            if (shaderName.StartsWith("Shader Graphs/")) return false;
            if (shaderName.StartsWith("UI/") || shaderName.StartsWith("Sprites/") || shaderName.StartsWith("GUI/")) return false;
            if (shaderName.Contains("Skybox")) return false;
            
            if (pipeline == PipelineType.URP)
            {
                if (shaderName.StartsWith("Universal Render Pipeline/")) return false;
                return true; // Everything else is potentially incompatible in URP
            }
            else if (pipeline == PipelineType.HDRP)
            {
                if (shaderName.StartsWith("HDRP/")) return false;
                return true;
            }

            return false;
        }

        private Texture GetTextureFromSerialized(SerializedObject so, string propName)
        {
            SerializedProperty texEnvs = so.FindProperty("m_SavedProperties.m_TexEnvs");
            if (texEnvs != null && texEnvs.isArray)
            {
                for (int i = 0; i < texEnvs.arraySize; i++)
                {
                    SerializedProperty prop = texEnvs.GetArrayElementAtIndex(i);
                    if (prop.FindPropertyRelative("first").stringValue == propName)
                    {
                        return prop.FindPropertyRelative("second.m_Texture").objectReferenceValue as Texture;
                    }
                }
            }
            return null;
        }

        private Color? GetColorFromSerialized(SerializedObject so, string propName)
        {
            SerializedProperty colors = so.FindProperty("m_SavedProperties.m_Colors");
            if (colors != null && colors.isArray)
            {
                for (int i = 0; i < colors.arraySize; i++)
                {
                    SerializedProperty prop = colors.GetArrayElementAtIndex(i);
                    if (prop.FindPropertyRelative("first").stringValue == propName)
                    {
                        return prop.FindPropertyRelative("second").colorValue;
                    }
                }
            }
            return null;
        }

        private float? GetFloatFromSerialized(SerializedObject so, string propName)
        {
            SerializedProperty floats = so.FindProperty("m_SavedProperties.m_Floats");
            if (floats != null && floats.isArray)
            {
                for (int i = 0; i < floats.arraySize; i++)
                {
                    SerializedProperty prop = floats.GetArrayElementAtIndex(i);
                    if (prop.FindPropertyRelative("first").stringValue == propName)
                    {
                        return prop.FindPropertyRelative("second").floatValue;
                    }
                }
            }
            return null;
        }

        private void FixMaterial(Material mat, PipelineType pipeline)
        {
            Undo.RecordObject(mat, "Fix Material Shader");
            
            SerializedObject so = new SerializedObject(mat);
            
            // Cache old properties using SerializedObject to bypass Error Shader hiding properties
            Color color = GetColorFromSerialized(so, "_Color") ?? (GetColorFromSerialized(so, "_BaseColor") ?? Color.white);
            Texture mainTex = GetTextureFromSerialized(so, "_MainTex") ?? GetTextureFromSerialized(so, "_BaseMap");
            Texture normalMap = GetTextureFromSerialized(so, "_BumpMap") ?? GetTextureFromSerialized(so, "_NormalMap");
            float normalScale = GetFloatFromSerialized(so, "_BumpScale") ?? (GetFloatFromSerialized(so, "_NormalScale") ?? 1f);
            Texture emissionMap = GetTextureFromSerialized(so, "_EmissionMap") ?? GetTextureFromSerialized(so, "_EmissiveColorMap");
            Color emissionColor = GetColorFromSerialized(so, "_EmissionColor") ?? (GetColorFromSerialized(so, "_EmissiveColor") ?? Color.black);
            float metallic = GetFloatFromSerialized(so, "_Metallic") ?? 0f;
            float smoothness = GetFloatFromSerialized(so, "_Glossiness") ?? (GetFloatFromSerialized(so, "_Smoothness") ?? 0.5f);

            string targetShader = pipeline == PipelineType.URP ? "Universal Render Pipeline/Lit" : "HDRP/Lit";
            Shader newShader = Shader.Find(targetShader);
            if (newShader != null)
            {
                mat.shader = newShader;
                
                // Reapply properties based on target pipeline
                if (pipeline == PipelineType.URP)
                {
                    if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
                    if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", mainTex);
                    if (mat.HasProperty("_BumpMap")) mat.SetTexture("_BumpMap", normalMap);
                    if (mat.HasProperty("_BumpScale")) mat.SetFloat("_BumpScale", normalScale);
                    if (mat.HasProperty("_EmissionMap")) mat.SetTexture("_EmissionMap", emissionMap);
                    if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", emissionColor);
                    if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
                    if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
                }
                else if (pipeline == PipelineType.HDRP)
                {
                    if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
                    if (mat.HasProperty("_BaseColorMap")) mat.SetTexture("_BaseColorMap", mainTex);
                    if (mat.HasProperty("_NormalMap")) mat.SetTexture("_NormalMap", normalMap);
                    if (mat.HasProperty("_NormalScale")) mat.SetFloat("_NormalScale", normalScale);
                    if (mat.HasProperty("_EmissiveColorMap")) mat.SetTexture("_EmissiveColorMap", emissionMap);
                    if (mat.HasProperty("_EmissiveColor")) mat.SetColor("_EmissiveColor", emissionColor);
                    if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
                    if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
                }
                
                // Enable keywords
                if (normalMap != null) mat.EnableKeyword("_NORMALMAP");
                if (emissionMap != null || emissionColor != Color.black)
                {
                    mat.EnableKeyword("_EMISSION");
                    mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                }

                EditorUtility.SetDirty(mat);
                incompatibleMaterials.Remove(mat);
            }
            else
            {
                Debug.LogError($"Could not find shader: {targetShader}");
            }
        }

        private void ScanForPlayerPrefab()
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (go != null && go.GetComponentInChildren<PlayerMovement>(true) != null)
                {
                    playerPrefab = go;
                    EditorUtility.DisplayDialog("Success", $"Found player prefab: {go.name}", "OK");
                    return;
                }
            }
            EditorUtility.DisplayDialog("Not Found", "Could not find any prefab with a PlayerMovement component.", "OK");
        }

        private void FixAllMaterials(PipelineType pipeline)
        {
            // Iterate over a copy to avoid modified collection exceptions since FixMaterial removes from the list
            System.Collections.Generic.List<Material> materialsToFix = new System.Collections.Generic.List<Material>(incompatibleMaterials);
            foreach (Material mat in materialsToFix)
            {
                if (mat != null)
                {
                    FixMaterial(mat, pipeline);
                }
            }
            EditorUtility.DisplayDialog("Warning", "Revise your materials and ensure they've been updated properly", "OK");
        }

        private void MigrateToURP()
        {
            if (playerPrefab == null) return;

            string assetPath = AssetDatabase.GetAssetPath(playerPrefab);
            if (string.IsNullOrEmpty(assetPath))
            {
                Debug.LogError("The selected object is not a saved prefab asset.");
                return;
            }

            GameObject contentsRoot = PrefabUtility.LoadPrefabContents(assetPath);

            Camera mainCamera = null;
            Camera weaponCamera = null;
            Camera[] cameras = contentsRoot.GetComponentsInChildren<Camera>(true);
            
            foreach (Camera cam in cameras)
            {
                if (cam.gameObject.name.Contains("Weapon"))
                {
                    weaponCamera = cam;
                    GameObjectUtility.RemoveMonoBehavioursWithMissingScript(cam.gameObject);
                }
                else if (cam.gameObject.name.Contains("Main") || cam.gameObject.CompareTag("MainCamera"))
                {
                    mainCamera = cam;
                    GameObjectUtility.RemoveMonoBehavioursWithMissingScript(cam.gameObject);
                }
            }

            if (mainCamera == null || weaponCamera == null)
            {
                Debug.LogWarning("Could not find both MainCamera and WeaponCamera in the prefab. Camera stack migration will be skipped.");
            }
            else
            {
                SetupURPCameras(mainCamera, weaponCamera);
            }

            PrefabUtility.SaveAsPrefabAsset(contentsRoot, assetPath);
            PrefabUtility.UnloadPrefabContents(contentsRoot);
            
            Debug.Log("Migrated Player Prefab to URP successfully!");
        }

        private void SetupURPCameras(Camera mainCamera, Camera weaponCamera)
        {
            Type additionalDataType = Type.GetType("UnityEngine.Rendering.Universal.UniversalAdditionalCameraData, Unity.RenderPipelines.Universal.Runtime");
            
            if (additionalDataType == null)
            {
                Debug.LogError("URP is not installed or could not be found! Make sure the Universal Render Pipeline package is installed in your project.");
                return;
            }

            Component mainCamData = mainCamera.GetComponent(additionalDataType) ?? mainCamera.gameObject.AddComponent(additionalDataType);
            Component weaponCamData = weaponCamera.GetComponent(additionalDataType) ?? weaponCamera.gameObject.AddComponent(additionalDataType);

            // Configure Weapon Camera as Overlay
            SerializedObject soWeapon = new SerializedObject(weaponCamData);
            soWeapon.Update();
            SerializedProperty renderTypeProp = soWeapon.FindProperty("m_CameraType");
            
            if (renderTypeProp != null)
            {
                renderTypeProp.intValue = 1; // Overlay
                renderTypeProp.enumValueIndex = 1;
                soWeapon.ApplyModifiedProperties();
            }
            else
            {
                Debug.LogWarning("[Migration] Could not find m_CameraType property on WeaponCamera's URP data.");
                PropertyInfo propInfo = additionalDataType.GetProperty("renderType");
                if (propInfo != null)
                {
                    propInfo.SetValue(weaponCamData, Enum.ToObject(propInfo.PropertyType, 1));
                    EditorUtility.SetDirty(weaponCamData);
                }
            }

            // Add Weapon Camera to Main Camera Stack
            SerializedObject soMain = new SerializedObject(mainCamData);
            soMain.Update();
            SerializedProperty camerasStackProp = soMain.FindProperty("m_Cameras");
            
            if (camerasStackProp != null)
            {
                bool alreadyInStack = false;
                for (int i = 0; i < camerasStackProp.arraySize; i++)
                {
                    if (camerasStackProp.GetArrayElementAtIndex(i).objectReferenceValue == weaponCamera)
                    {
                        alreadyInStack = true;
                        break;
                    }
                }

                if (!alreadyInStack)
                {
                    camerasStackProp.arraySize++;
                    camerasStackProp.GetArrayElementAtIndex(camerasStackProp.arraySize - 1).objectReferenceValue = weaponCamera;
                    soMain.ApplyModifiedProperties();
                }
            }
            else
            {
                Debug.LogWarning("Could not find m_Cameras property on MainCamera's URP data.");
            }
        }

        private void MigrateToHDRP()
        {
            if (playerPrefab == null) return;

            string assetPath = AssetDatabase.GetAssetPath(playerPrefab);
            if (string.IsNullOrEmpty(assetPath))
            {
                Debug.LogError("The selected object is not a saved prefab asset.");
                return;
            }

            GameObject contentsRoot = PrefabUtility.LoadPrefabContents(assetPath);

            RemoveMissingScriptsFromCameras(contentsRoot);

            PrefabUtility.SaveAsPrefabAsset(contentsRoot, assetPath);
            PrefabUtility.UnloadPrefabContents(contentsRoot);
            
            Debug.Log("Migrated Player Prefab to HDRP successfully!");
        }

        private void RemoveMissingScriptsFromCameras(GameObject root)
        {
            Camera[] cameras = root.GetComponentsInChildren<Camera>(true);
            foreach (Camera cam in cameras)
            {
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(cam.gameObject);
            }
        }
    }
}
#endif
