// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using UnityEditor;
using UnityEngine;
using HanabiCanvas.Runtime;
using HanabiCanvas.Runtime.Firework;

namespace HanabiCanvas.Editor
{
    /// <summary>
    /// EditorWindow wizard that creates all ScriptableObject assets needed by the
    /// rocket system: RocketConfigSO, three path behaviours, a launch event channel,
    /// and two BoolVariableSOs.
    /// </summary>
    public class RocketSystemWizard : EditorWindow
    {
        // ---- Path Constants ----
        private const string ROCKET_CONFIG_PATH = "Assets/Data/Config/Default Rocket Config.asset";
        private const string STRAIGHT_PATH_PATH = "Assets/Data/Config/Default Straight Path.asset";
        private const string ARC_PATH_PATH = "Assets/Data/Config/Default Arc Path.asset";
        private const string CURVE_PATH_PATH = "Assets/Data/Config/Default Curve Path.asset";
        private const string EVENT_ROCKET_LAUNCH_REQUESTED_PATH = "Assets/Data/Config/On Rocket Launch Requested.asset";
        private const string IS_ROCKET_ASCENDING_PATH = "Assets/Data/Config/Is Rocket Ascending.asset";
        private const string IS_ROCKET_ENABLED_PATH = "Assets/Data/Config/Is Rocket Enabled.asset";

        // ---- Private Fields ----
        private Vector2 _scrollPosition;
        private string _statusMessage = "";
        private int _createdCount;

        // ---- Menu Item ----
        [MenuItem("Tools/Hanabi Canvas/Setup Rocket System")]
        private static void ShowWindow()
        {
            RocketSystemWizard window = GetWindow<RocketSystemWizard>("Rocket System Setup");
            window.minSize = new Vector2(450, 400);
            window.Show();
        }

        // ---- Unity Methods ----
        private void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            EditorGUILayout.LabelField("Hanabi Canvas \u2014 Rocket System Setup", EditorStyles.boldLabel);
            EditorGUILayout.Space(8);
            EditorGUILayout.HelpBox(
                "Creates all ScriptableObject assets for the rocket system.\n" +
                "This includes the RocketConfigSO, 3 path behaviour SOs, " +
                "1 FireworkRequestEventSO event channel, and 2 BoolVariableSOs.\n\n" +
                "After creation, the 3 path SOs are wired into the config's path behaviours array, " +
                "and default spawn/destination positions are set.",
                MessageType.Info);
            EditorGUILayout.Space(8);

            EditorGUILayout.LabelField("Config SOs (1):", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("  - Default Rocket Config (RocketConfigSO)");
            EditorGUILayout.Space(4);

            EditorGUILayout.LabelField("Path Behaviours (3):", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("  - Default Straight Path (StraightRocketPathSO)");
            EditorGUILayout.LabelField("  - Default Arc Path (ArcRocketPathSO)");
            EditorGUILayout.LabelField("  - Default Curve Path (CurveRocketPathSO)");
            EditorGUILayout.Space(4);

            EditorGUILayout.LabelField("Event Channels (1):", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("  - On Rocket Launch Requested (FireworkRequestEventSO)");
            EditorGUILayout.Space(4);

            EditorGUILayout.LabelField("Variable SOs (2):", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("  - Is Rocket Ascending (BoolVariableSO)");
            EditorGUILayout.LabelField("  - Is Rocket Enabled (BoolVariableSO, initial = true)");
            EditorGUILayout.Space(8);

            if (GUILayout.Button("Create Assets", GUILayout.Height(32)))
            {
                try
                {
                    CreateAllAssets();
                }
                catch (System.Exception ex)
                {
                    _statusMessage = $"Error: {ex.Message}";
                    Debug.LogError($"[RocketSystemWizard] {ex}");
                }
            }

            if (!string.IsNullOrEmpty(_statusMessage))
            {
                EditorGUILayout.Space(8);
                EditorGUILayout.HelpBox(_statusMessage, MessageType.Info);
            }

            EditorGUILayout.EndScrollView();
        }

        // ---- Private Methods ----
        private void CreateAllAssets()
        {
            _createdCount = 0;

            if (HasExistingAssets())
            {
                bool shouldOverwrite = EditorUtility.DisplayDialog(
                    "Overwrite Existing Assets?",
                    "Some rocket system assets already exist. Overwrite?",
                    "Overwrite",
                    "Cancel");

                if (!shouldOverwrite)
                {
                    _statusMessage = "Cancelled.";
                    return;
                }
            }

            EnsureDirectoriesExist();

            // Create path behaviour SOs
            StraightRocketPathSO straightPath = CreateInstance<StraightRocketPathSO>();
            straightPath.name = "Default Straight Path";
            AssetDatabase.CreateAsset(straightPath, STRAIGHT_PATH_PATH);
            LogCreated(STRAIGHT_PATH_PATH);

            ArcRocketPathSO arcPath = CreateInstance<ArcRocketPathSO>();
            arcPath.name = "Default Arc Path";
            AssetDatabase.CreateAsset(arcPath, ARC_PATH_PATH);
            LogCreated(ARC_PATH_PATH);

            CurveRocketPathSO curvePath = CreateInstance<CurveRocketPathSO>();
            curvePath.name = "Default Curve Path";
            AssetDatabase.CreateAsset(curvePath, CURVE_PATH_PATH);
            LogCreated(CURVE_PATH_PATH);

            // Create RocketConfigSO and wire path behaviours + positions
            RocketConfigSO rocketConfig = CreateInstance<RocketConfigSO>();
            rocketConfig.name = "Default Rocket Config";
            AssetDatabase.CreateAsset(rocketConfig, ROCKET_CONFIG_PATH);
            LogCreated(ROCKET_CONFIG_PATH);

            WireRocketConfig(rocketConfig, straightPath, arcPath, curvePath);

            // Create FireworkRequestEventSO event channel
            FireworkRequestEventSO launchEvent = CreateInstance<FireworkRequestEventSO>();
            launchEvent.name = "On Rocket Launch Requested";
            AssetDatabase.CreateAsset(launchEvent, EVENT_ROCKET_LAUNCH_REQUESTED_PATH);
            LogCreated(EVENT_ROCKET_LAUNCH_REQUESTED_PATH);

            // Create BoolVariableSO — Is Rocket Ascending
            BoolVariableSO isRocketAscending = CreateInstance<BoolVariableSO>();
            isRocketAscending.name = "Is Rocket Ascending";
            AssetDatabase.CreateAsset(isRocketAscending, IS_ROCKET_ASCENDING_PATH);
            LogCreated(IS_ROCKET_ASCENDING_PATH);

            // Create BoolVariableSO — Is Rocket Enabled (initial = true)
            BoolVariableSO isRocketEnabled = CreateInstance<BoolVariableSO>();
            isRocketEnabled.name = "Is Rocket Enabled";
            AssetDatabase.CreateAsset(isRocketEnabled, IS_ROCKET_ENABLED_PATH);
            LogCreated(IS_ROCKET_ENABLED_PATH);

            SetBoolInitialValue(isRocketEnabled, true);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            _statusMessage = $"Setup complete! Created {_createdCount} assets.";
            Debug.Log($"[RocketSystemWizard] Setup complete. Created {_createdCount} assets.");
        }

        private void WireRocketConfig(
            RocketConfigSO rocketConfig,
            StraightRocketPathSO straightPath,
            ArcRocketPathSO arcPath,
            CurveRocketPathSO curvePath)
        {
            SerializedObject configSO = new SerializedObject(rocketConfig);

            // Wire path behaviours array
            SerializedProperty pathsProp = configSO.FindProperty("_pathBehaviours");
            pathsProp.arraySize = 3;
            pathsProp.GetArrayElementAtIndex(0).objectReferenceValue = straightPath;
            pathsProp.GetArrayElementAtIndex(1).objectReferenceValue = arcPath;
            pathsProp.GetArrayElementAtIndex(2).objectReferenceValue = curvePath;

            // Set spawn positions
            SerializedProperty spawnProp = configSO.FindProperty("_spawnPositions");
            spawnProp.arraySize = 3;
            spawnProp.GetArrayElementAtIndex(0).vector3Value = new Vector3(-3f, -8f, 0f);
            spawnProp.GetArrayElementAtIndex(1).vector3Value = new Vector3(0f, -8f, 0f);
            spawnProp.GetArrayElementAtIndex(2).vector3Value = new Vector3(3f, -8f, 0f);

            // Set destination positions
            SerializedProperty destProp = configSO.FindProperty("_destinationPositions");
            destProp.arraySize = 3;
            destProp.GetArrayElementAtIndex(0).vector3Value = new Vector3(-5f, 12f, 0f);
            destProp.GetArrayElementAtIndex(1).vector3Value = new Vector3(0f, 15f, 0f);
            destProp.GetArrayElementAtIndex(2).vector3Value = new Vector3(5f, 12f, 0f);

            configSO.ApplyModifiedPropertiesWithoutUndo();
        }

        private void SetBoolInitialValue(BoolVariableSO boolVariable, bool value)
        {
            SerializedObject boolSO = new SerializedObject(boolVariable);
            boolSO.FindProperty("initialValue").boolValue = value;
            boolSO.ApplyModifiedPropertiesWithoutUndo();
        }

        private bool HasExistingAssets()
        {
            return AssetDatabase.LoadAssetAtPath<Object>(ROCKET_CONFIG_PATH) != null ||
                   AssetDatabase.LoadAssetAtPath<Object>(STRAIGHT_PATH_PATH) != null ||
                   AssetDatabase.LoadAssetAtPath<Object>(ARC_PATH_PATH) != null ||
                   AssetDatabase.LoadAssetAtPath<Object>(CURVE_PATH_PATH) != null ||
                   AssetDatabase.LoadAssetAtPath<Object>(EVENT_ROCKET_LAUNCH_REQUESTED_PATH) != null ||
                   AssetDatabase.LoadAssetAtPath<Object>(IS_ROCKET_ASCENDING_PATH) != null ||
                   AssetDatabase.LoadAssetAtPath<Object>(IS_ROCKET_ENABLED_PATH) != null;
        }

        private void EnsureDirectoriesExist()
        {
            EnsureDirectory("Assets/Data");
            EnsureDirectory("Assets/Data/Config");
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

        private void LogCreated(string path)
        {
            _createdCount++;
            Debug.Log($"[RocketSystemWizard] Created: {path}");
        }
    }
}
