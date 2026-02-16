// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace HanabiCanvas.Editor
{
    /// <summary>
    /// EditorWindow wizard that validates the Phase 5 polish setup:
    /// FireworkEffectSO assets, URP HDR, camera HDR, Bloom volume, and particle material.
    /// Accessible via Tools/Hanabi Canvas/Polish Validator.
    /// </summary>
    public class PolishValidatorWizard : EditorWindow
    {
        // ---- Constants ----
        private const string FIREWORKS_DATA_PATH = "Assets/Data/Fireworks";
        private const string PARTICLE_MATERIAL_PATH = "Assets/Art/Materials/FireworkParticle.mat";
        private const string EXPECTED_SHADER_NAME = "HanabiCanvas/FireworkParticle";

        private const string DEFAULT_TRAIL_EFFECT_PATH = "Assets/Data/Fireworks/Default Trail Effect.asset";
        private const string DEFAULT_BREATHING_EFFECT_PATH = "Assets/Data/Fireworks/Default Breathing Effect.asset";
        private const string DEFAULT_COLOR_SHIFT_EFFECT_PATH = "Assets/Data/Fireworks/Default Color Shift Effect.asset";
        private const string DEFAULT_GRAVITY_VARIANCE_EFFECT_PATH = "Assets/Data/Fireworks/Default Gravity Variance Effect.asset";

        private const float BLOOM_THRESHOLD = 0.8f;
        private const float BLOOM_INTENSITY = 1.5f;
        private const float BLOOM_SCATTER = 0.7f;

        // ---- Nested Types ----
        private enum ResultLevel
        {
            Pass,
            Warning,
            Error
        }

        private struct ValidationResult
        {
            public string Message;
            public ResultLevel Level;
        }

        // ---- Private Fields ----
        private readonly List<ValidationResult> _results = new List<ValidationResult>();
        private Vector2 _resultsScrollPosition;
        private Vector2 _logScrollPosition;
        private readonly List<string> _logMessages = new List<string>();
        private bool _hasRun;

        // ---- Menu Item ----
        [MenuItem("Tools/Hanabi Canvas/Polish Validator")]
        private static void ShowWindow()
        {
            PolishValidatorWizard window = GetWindow<PolishValidatorWizard>("Polish Validator");
            window.minSize = new Vector2(450, 500);
            window.Show();
        }

        // ---- Unity Methods ----
        private void OnGUI()
        {
            EditorGUILayout.LabelField("Hanabi Canvas — Polish Validator", EditorStyles.boldLabel);
            EditorGUILayout.Space(8);

            if (GUILayout.Button("Run Validation", GUILayout.Height(28)))
            {
                RunValidation();
            }

            EditorGUILayout.Space(4);

            if (_hasRun)
            {
                DrawValidationResults();
            }

            EditorGUILayout.Space(8);
            DrawSeparator();
            EditorGUILayout.Space(8);

            if (GUILayout.Button("Apply Recommended Defaults", GUILayout.Height(28)))
            {
                if (EditorUtility.DisplayDialog(
                    "Apply Recommended Defaults",
                    "This will:\n" +
                    "- Create default effect SO assets (if missing)\n" +
                    "- Enable HDR on the active URP Renderer Asset\n" +
                    "- Enable HDR on the main camera\n" +
                    "- Create a Global Volume with Bloom override\n\n" +
                    "Proceed?",
                    "Apply",
                    "Cancel"))
                {
                    ApplyRecommendedDefaults();
                }
            }

            EditorGUILayout.Space(4);

            if (_logMessages.Count > 0)
            {
                DrawLogArea();
            }
        }

        // ---- Validation ----
        private void RunValidation()
        {
            _results.Clear();
            _hasRun = true;

            ValidateEffectAssets();
            ValidateURPHDR();
            ValidateCameraHDR();
            ValidateBloomVolume();
            ValidateParticleMaterial();

            Repaint();
        }

        private void ValidateEffectAssets()
        {
            string[] guids = AssetDatabase.FindAssets("t:ScriptableObject", new[] { FIREWORKS_DATA_PATH });
            int trailCount = 0;
            int breathingCount = 0;
            int colorShiftCount = 0;
            int gravityVarianceCount = 0;
            int otherEffectCount = 0;

            Type baseEffectType = FindType("HanabiCanvas.Runtime.Firework.FireworkEffectSO");
            Type trailType = FindType("HanabiCanvas.Runtime.Firework.TrailEffectSO");
            Type breathingType = FindType("HanabiCanvas.Runtime.Firework.BreathingEffectSO");
            Type colorShiftType = FindType("HanabiCanvas.Runtime.Firework.ColorShiftEffectSO");
            Type gravityVarianceType = FindType("HanabiCanvas.Runtime.Firework.GravityVarianceEffectSO");

            if (baseEffectType == null)
            {
                _results.Add(new ValidationResult
                {
                    Message = "FireworkEffectSO type not found. Effect SO classes may not be compiled yet.",
                    Level = ResultLevel.Warning
                });
                return;
            }

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ScriptableObject so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (so == null)
                {
                    continue;
                }

                Type soType = so.GetType();
                if (!baseEffectType.IsAssignableFrom(soType))
                {
                    continue;
                }

                if (trailType != null && trailType.IsAssignableFrom(soType))
                {
                    trailCount++;
                }
                else if (breathingType != null && breathingType.IsAssignableFrom(soType))
                {
                    breathingCount++;
                }
                else if (colorShiftType != null && colorShiftType.IsAssignableFrom(soType))
                {
                    colorShiftCount++;
                }
                else if (gravityVarianceType != null && gravityVarianceType.IsAssignableFrom(soType))
                {
                    gravityVarianceCount++;
                }
                else
                {
                    otherEffectCount++;
                }
            }

            int totalEffects = trailCount + breathingCount + colorShiftCount + gravityVarianceCount + otherEffectCount;

            if (totalEffects == 0)
            {
                _results.Add(new ValidationResult
                {
                    Message = "No FireworkEffectSO assets found in " + FIREWORKS_DATA_PATH +
                              ". Use 'Apply Recommended Defaults' to create them.",
                    Level = ResultLevel.Warning
                });
            }
            else
            {
                string breakdown = $"Trail: {trailCount}, Breathing: {breathingCount}, " +
                                   $"ColorShift: {colorShiftCount}, GravityVariance: {gravityVarianceCount}";
                if (otherEffectCount > 0)
                {
                    breakdown += $", Other: {otherEffectCount}";
                }

                _results.Add(new ValidationResult
                {
                    Message = $"Found {totalEffects} FireworkEffectSO asset(s). [{breakdown}]",
                    Level = ResultLevel.Pass
                });
            }
        }

        private void ValidateURPHDR()
        {
            bool isEnabled = IsHDREnabled();
            if (isEnabled)
            {
                _results.Add(new ValidationResult
                {
                    Message = "URP Renderer Asset has HDR enabled.",
                    Level = ResultLevel.Pass
                });
            }
            else
            {
                _results.Add(new ValidationResult
                {
                    Message = "URP Renderer Asset does NOT have HDR enabled. Bloom requires HDR.",
                    Level = ResultLevel.Error
                });
            }
        }

        private void ValidateCameraHDR()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                _results.Add(new ValidationResult
                {
                    Message = "No main camera found in the active scene (tag 'MainCamera').",
                    Level = ResultLevel.Warning
                });
                return;
            }

            if (IsCameraHDREnabled(mainCamera))
            {
                _results.Add(new ValidationResult
                {
                    Message = "Main camera has HDR enabled.",
                    Level = ResultLevel.Pass
                });
            }
            else
            {
                _results.Add(new ValidationResult
                {
                    Message = "Main camera does NOT have HDR enabled. Set allowHDR = true.",
                    Level = ResultLevel.Error
                });
            }
        }

        private void ValidateBloomVolume()
        {
            Volume[] volumes = FindObjectsByType<Volume>(FindObjectsSortMode.None);

            if (volumes == null || volumes.Length == 0)
            {
                _results.Add(new ValidationResult
                {
                    Message = "No Volume component found in the active scene. Bloom requires a Volume with Bloom override.",
                    Level = ResultLevel.Error
                });
                return;
            }

            bool hasBloom = false;
            foreach (Volume volume in volumes)
            {
                if (HasBloomVolume(volume))
                {
                    hasBloom = true;
                    break;
                }
            }

            if (hasBloom)
            {
                _results.Add(new ValidationResult
                {
                    Message = "Found Volume with Bloom override enabled.",
                    Level = ResultLevel.Pass
                });
            }
            else
            {
                _results.Add(new ValidationResult
                {
                    Message = "Volume found but no active Bloom override detected.",
                    Level = ResultLevel.Warning
                });
            }
        }

        private void ValidateParticleMaterial()
        {
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(PARTICLE_MATERIAL_PATH);
            if (mat == null)
            {
                _results.Add(new ValidationResult
                {
                    Message = $"Particle material not found at {PARTICLE_MATERIAL_PATH}.",
                    Level = ResultLevel.Error
                });
                return;
            }

            if (mat.shader == null)
            {
                _results.Add(new ValidationResult
                {
                    Message = "Particle material has no shader assigned.",
                    Level = ResultLevel.Error
                });
                return;
            }

            if (mat.shader.name == EXPECTED_SHADER_NAME)
            {
                _results.Add(new ValidationResult
                {
                    Message = $"Particle material uses correct shader: {EXPECTED_SHADER_NAME}.",
                    Level = ResultLevel.Pass
                });
            }
            else
            {
                _results.Add(new ValidationResult
                {
                    Message = $"Particle material uses shader '{mat.shader.name}' instead of '{EXPECTED_SHADER_NAME}'.",
                    Level = ResultLevel.Warning
                });
            }
        }

        // ---- Static Validation Helpers (testable) ----

        /// <summary>
        /// Checks whether HDR is enabled on the active URP Renderer Pipeline Asset
        /// via SerializedObject (avoids direct URP type dependency in Editor asmdef).
        /// </summary>
        public static bool IsHDREnabled()
        {
            RenderPipelineAsset rpAsset = GraphicsSettings.currentRenderPipeline;
            if (rpAsset == null)
            {
                return false;
            }

            SerializedObject so = new SerializedObject(rpAsset);
            SerializedProperty hdrProp = so.FindProperty("m_SupportsHDR");
            if (hdrProp == null)
            {
                return false;
            }

            return hdrProp.boolValue;
        }

        /// <summary>
        /// Checks whether the given camera has HDR rendering enabled.
        /// </summary>
        public static bool IsCameraHDREnabled(Camera camera)
        {
            if (camera == null)
            {
                return false;
            }

            return camera.allowHDR;
        }

        /// <summary>
        /// Checks whether the given Volume has an active Bloom override.
        /// Uses reflection to access URP Bloom type without a direct asmdef reference.
        /// </summary>
        public static bool HasBloomVolume(Volume volume)
        {
            if (volume == null || volume.profile == null)
            {
                return false;
            }

            VolumeProfile profile = volume.profile;
            foreach (VolumeComponent component in profile.components)
            {
                if (component == null)
                {
                    continue;
                }

                // Check by type name to avoid needing direct URP asmdef reference
                if (component.GetType().Name == "Bloom" && component.active)
                {
                    return true;
                }
            }

            return false;
        }

        // ---- Apply Defaults ----
        private void ApplyRecommendedDefaults()
        {
            _logMessages.Clear();

            EnsureDirectory("Assets/Data");
            EnsureDirectory(FIREWORKS_DATA_PATH);

            CreateDefaultEffectAssets();
            EnableURPHDR();
            EnableCameraHDR();
            CreateBloomVolume();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            _logMessages.Add("Apply complete. Run validation again to verify.");
            Repaint();
        }

        private void CreateDefaultEffectAssets()
        {
            CreateEffectAssetIfMissing("HanabiCanvas.Runtime.Firework.TrailEffectSO", DEFAULT_TRAIL_EFFECT_PATH);
            CreateEffectAssetIfMissing("HanabiCanvas.Runtime.Firework.BreathingEffectSO", DEFAULT_BREATHING_EFFECT_PATH);
            CreateEffectAssetIfMissing("HanabiCanvas.Runtime.Firework.ColorShiftEffectSO", DEFAULT_COLOR_SHIFT_EFFECT_PATH);
            CreateEffectAssetIfMissing("HanabiCanvas.Runtime.Firework.GravityVarianceEffectSO", DEFAULT_GRAVITY_VARIANCE_EFFECT_PATH);
        }

        private void CreateEffectAssetIfMissing(string typeName, string path)
        {
            if (AssetDatabase.LoadAssetAtPath<ScriptableObject>(path) != null)
            {
                _logMessages.Add($"Already exists: {path}");
                return;
            }

            Type effectType = FindType(typeName);
            if (effectType == null)
            {
                _logMessages.Add($"Type not found: {typeName}. Compile effect scripts first.");
                Debug.LogWarning($"[PolishValidator] Type not found: {typeName}");
                return;
            }

            ScriptableObject asset = ScriptableObject.CreateInstance(effectType);
            AssetDatabase.CreateAsset(asset, path);
            _logMessages.Add($"Created: {path}");
            Debug.Log($"[PolishValidator] Created asset: {path}");
        }

        private void EnableURPHDR()
        {
            RenderPipelineAsset rpAsset = GraphicsSettings.currentRenderPipeline;
            if (rpAsset == null)
            {
                _logMessages.Add("No active URP Renderer Asset found. Cannot enable HDR.");
                Debug.LogWarning("[PolishValidator] No active URP Renderer Asset found.");
                return;
            }

            SerializedObject so = new SerializedObject(rpAsset);
            SerializedProperty hdrProp = so.FindProperty("m_SupportsHDR");
            if (hdrProp == null)
            {
                _logMessages.Add("Could not find m_SupportsHDR property on URP asset.");
                Debug.LogWarning("[PolishValidator] Could not find m_SupportsHDR on URP asset.");
                return;
            }

            if (hdrProp.boolValue)
            {
                _logMessages.Add("URP HDR already enabled.");
            }
            else
            {
                hdrProp.boolValue = true;
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(rpAsset);
                _logMessages.Add("Enabled HDR on URP Renderer Asset.");
                Debug.Log("[PolishValidator] Enabled HDR on URP Renderer Asset.");
            }
        }

        private void EnableCameraHDR()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                _logMessages.Add("No main camera found. Cannot enable camera HDR.");
                Debug.LogWarning("[PolishValidator] No main camera found in active scene.");
                return;
            }

            if (mainCamera.allowHDR)
            {
                _logMessages.Add("Camera HDR already enabled.");
            }
            else
            {
                mainCamera.allowHDR = true;
                EditorUtility.SetDirty(mainCamera);
                _logMessages.Add("Enabled HDR on main camera.");
                Debug.Log("[PolishValidator] Enabled HDR on main camera.");
            }
        }

        private void CreateBloomVolume()
        {
            // Check if a Volume with Bloom already exists
            Volume[] existingVolumes = FindObjectsByType<Volume>(FindObjectsSortMode.None);
            foreach (Volume volume in existingVolumes)
            {
                if (HasBloomVolume(volume))
                {
                    _logMessages.Add("Volume with Bloom already exists in scene.");
                    return;
                }
            }

            // Create Volume GameObject
            GameObject volumeObj = new GameObject("Global Volume (Bloom)");
            Undo.RegisterCreatedObjectUndo(volumeObj, "Create Global Volume");

            Volume volumeComponent = volumeObj.AddComponent<Volume>();
            volumeComponent.isGlobal = true;

            // Create a new VolumeProfile
            VolumeProfile profile = ScriptableObject.CreateInstance<VolumeProfile>();
            volumeComponent.profile = profile;

            // Use reflection to add Bloom override
            Type bloomType = FindType("UnityEngine.Rendering.Universal.Bloom");
            if (bloomType == null)
            {
                _logMessages.Add("Bloom type not found. URP may not be compiled. Volume created without Bloom.");
                Debug.LogWarning("[PolishValidator] Bloom type not found. Volume created without Bloom override.");
                return;
            }

            // VolumeProfile.Add(Type type, bool overrideState = false) returns VolumeComponent
            MethodInfo addMethod = typeof(VolumeProfile).GetMethod("Add",
                new Type[] { typeof(Type), typeof(bool) });

            if (addMethod == null)
            {
                _logMessages.Add("Could not find VolumeProfile.Add method. Volume created without Bloom.");
                Debug.LogWarning("[PolishValidator] VolumeProfile.Add method not found.");
                return;
            }

            VolumeComponent bloomComponent = addMethod.Invoke(profile, new object[] { bloomType, true }) as VolumeComponent;
            if (bloomComponent == null)
            {
                _logMessages.Add("Failed to add Bloom component to Volume profile.");
                Debug.LogWarning("[PolishValidator] Failed to add Bloom to Volume profile.");
                return;
            }

            bloomComponent.active = true;

            // Set Bloom parameters via reflection
            SetVolumeParameterValue(bloomComponent, "threshold", BLOOM_THRESHOLD);
            SetVolumeParameterValue(bloomComponent, "intensity", BLOOM_INTENSITY);
            SetVolumeParameterValue(bloomComponent, "scatter", BLOOM_SCATTER);

            EditorUtility.SetDirty(volumeObj);
            EditorUtility.SetDirty(profile);

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene());

            _logMessages.Add("Created Global Volume with Bloom override (threshold=0.8, intensity=1.5, scatter=0.7).");
            Debug.Log("[PolishValidator] Created Global Volume with Bloom override.");
        }

        // ---- UI Drawing ----
        private void DrawValidationResults()
        {
            EditorGUILayout.LabelField("Validation Results", EditorStyles.boldLabel);

            _resultsScrollPosition = EditorGUILayout.BeginScrollView(
                _resultsScrollPosition, GUILayout.MinHeight(150), GUILayout.MaxHeight(300));

            foreach (ValidationResult result in _results)
            {
                MessageType messageType;
                switch (result.Level)
                {
                    case ResultLevel.Pass:
                        messageType = MessageType.Info;
                        break;
                    case ResultLevel.Warning:
                        messageType = MessageType.Warning;
                        break;
                    case ResultLevel.Error:
                        messageType = MessageType.Error;
                        break;
                    default:
                        messageType = MessageType.None;
                        break;
                }

                EditorGUILayout.HelpBox(result.Message, messageType);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawLogArea()
        {
            EditorGUILayout.LabelField("Status Log", EditorStyles.boldLabel);

            _logScrollPosition = EditorGUILayout.BeginScrollView(
                _logScrollPosition, GUILayout.MinHeight(80), GUILayout.MaxHeight(200));

            foreach (string message in _logMessages)
            {
                EditorGUILayout.LabelField(message, EditorStyles.wordWrappedMiniLabel);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawSeparator()
        {
            Rect rect = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.5f));
        }

        // ---- Utility ----
        private static Type FindType(string fullTypeName)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = assembly.GetType(fullTypeName);
                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }

        private static void SetVolumeParameterValue(VolumeComponent component, string fieldName, float value)
        {
            FieldInfo field = component.GetType().GetField(fieldName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (field == null)
            {
                Debug.LogWarning($"[PolishValidator] Could not find field '{fieldName}' on {component.GetType().Name}.");
                return;
            }

            object parameter = field.GetValue(component);
            if (parameter == null)
            {
                return;
            }

            // VolumeParameter<T> has 'value' property and 'overrideState' field
            Type paramType = parameter.GetType();

            PropertyInfo valueProp = paramType.GetProperty("value",
                BindingFlags.Public | BindingFlags.Instance);
            if (valueProp != null)
            {
                valueProp.SetValue(parameter, value);
            }

            FieldInfo overrideField = paramType.GetField("overrideState",
                BindingFlags.Public | BindingFlags.Instance);
            if (overrideField != null)
            {
                overrideField.SetValue(parameter, true);
            }
        }

        private void EnsureDirectory(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string parent = System.IO.Path.GetDirectoryName(path).Replace("\\", "/");
                string folderName = System.IO.Path.GetFileName(path);
                AssetDatabase.CreateFolder(parent, folderName);
            }
        }
    }
}
