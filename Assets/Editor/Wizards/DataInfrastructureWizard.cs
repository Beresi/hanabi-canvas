// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using UnityEditor;
using UnityEngine;
using HanabiCanvas.Runtime;
using HanabiCanvas.Runtime.Events;

namespace HanabiCanvas.Editor
{
    public class DataInfrastructureWizard : EditorWindow
    {
        // ---- Path Constants ----
        private const string EVENT_FIREWORK_COMPLETE_PATH = "Assets/Data/Config/On Firework Complete.asset";
        private const string EVENT_DRAWING_COMPLETE_PATH = "Assets/Data/Config/On Drawing Complete.asset";
        private const string EVENT_PIXEL_PAINTED_PATH = "Assets/Data/Config/On Pixel Painted.asset";
        private const string EVENT_CONSTRAINT_VIOLATED_PATH = "Assets/Data/Config/On Constraint Violated.asset";
        private const string EVENT_DATA_CHANGED_PATH = "Assets/Data/Config/On Data Changed.asset";
        private const string EVENT_SLIDESHOW_STARTED_PATH = "Assets/Data/Config/On Slideshow Started.asset";
        private const string EVENT_SLIDESHOW_ARTWORK_CHANGED_PATH = "Assets/Data/Config/On Slideshow Artwork Changed.asset";
        private const string EVENT_SLIDESHOW_COMPLETE_PATH = "Assets/Data/Config/On Slideshow Complete.asset";
        private const string EVENT_SLIDESHOW_EXIT_REQUESTED_PATH = "Assets/Data/Config/On Slideshow Exit Requested.asset";
        private const string EVENT_ARTWORK_LIKED_PATH = "Assets/Data/Config/On Artwork Liked.asset";
        private const string EVENT_SLIDESHOW_ARTWORK_STARTED_PATH = "Assets/Data/Config/On Slideshow Artwork Started.asset";
        private const string APP_STATE_PATH = "Assets/Data/Config/App State.asset";
        private const string CHALLENGE_CONFIG_PATH = "Assets/Data/Config/Default Challenge Config.asset";
        private const string SLIDESHOW_CONFIG_PATH = "Assets/Data/Config/Default Slideshow Config.asset";
        private const string AUDIO_CONFIG_PATH = "Assets/Data/Config/Default Audio Config.asset";
        private const string ARTWORK_COUNT_PATH = "Assets/Data/Config/Artwork Count.asset";
        private const string ACTIVE_REQUEST_COUNT_PATH = "Assets/Data/Config/Active Request Count.asset";

        // ---- Private Fields ----
        private Vector2 _scrollPosition;
        private string _statusMessage = "";
        private int _createdCount;

        // ---- Menu Item ----
        [MenuItem("Tools/Hanabi Canvas/Setup Data Infrastructure")]
        private static void ShowWindow()
        {
            DataInfrastructureWizard window = GetWindow<DataInfrastructureWizard>("Data Infrastructure Setup");
            window.minSize = new Vector2(450, 400);
            window.Show();
        }

        // ---- Unity Methods ----
        private void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            EditorGUILayout.LabelField("Hanabi Canvas \u2014 Data Infrastructure Setup", EditorStyles.boldLabel);
            EditorGUILayout.Space(8);
            EditorGUILayout.HelpBox(
                "Creates Phase 6 ScriptableObject assets for data infrastructure.\n" +
                "This includes 10 GameEventSO channels, 1 ArtworkEventSO, 1 AppStateVariableSO, " +
                "and 3 config SOs (Challenge, Slideshow, Audio).",
                MessageType.Info);
            EditorGUILayout.Space(8);

            EditorGUILayout.LabelField("Game Events (10):", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("  - On Firework Complete");
            EditorGUILayout.LabelField("  - On Drawing Complete");
            EditorGUILayout.LabelField("  - On Pixel Painted");
            EditorGUILayout.LabelField("  - On Constraint Violated");
            EditorGUILayout.LabelField("  - On Data Changed");
            EditorGUILayout.LabelField("  - On Slideshow Started");
            EditorGUILayout.LabelField("  - On Slideshow Artwork Changed");
            EditorGUILayout.LabelField("  - On Slideshow Complete");
            EditorGUILayout.LabelField("  - On Slideshow Exit Requested");
            EditorGUILayout.LabelField("  - On Artwork Liked");
            EditorGUILayout.Space(4);

            EditorGUILayout.LabelField("Typed Events (1):", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("  - On Slideshow Artwork Started (ArtworkEventSO)");
            EditorGUILayout.Space(4);

            EditorGUILayout.LabelField("Variable SOs (3):", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("  - App State (AppStateVariableSO)");
            EditorGUILayout.LabelField("  - Artwork Count (IntVariableSO)");
            EditorGUILayout.LabelField("  - Active Request Count (IntVariableSO)");
            EditorGUILayout.Space(4);

            EditorGUILayout.LabelField("Config SOs (3):", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("  - Default Challenge Config (5 sample requests)");
            EditorGUILayout.LabelField("  - Default Slideshow Config");
            EditorGUILayout.LabelField("  - Default Audio Config");
            EditorGUILayout.Space(8);

            if (GUILayout.Button("Create Assets", GUILayout.Height(32)))
            {
                CreateAllAssets();
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
                    "Some data infrastructure assets already exist. Overwrite?",
                    "Overwrite",
                    "Cancel");

                if (!shouldOverwrite)
                {
                    _statusMessage = "Cancelled.";
                    return;
                }
            }

            EnsureDirectoriesExist();

            // Create 10 GameEventSO assets
            CreateGameEvent(EVENT_FIREWORK_COMPLETE_PATH, "On Firework Complete");
            CreateGameEvent(EVENT_DRAWING_COMPLETE_PATH, "On Drawing Complete");
            CreateGameEvent(EVENT_PIXEL_PAINTED_PATH, "On Pixel Painted");
            CreateGameEvent(EVENT_CONSTRAINT_VIOLATED_PATH, "On Constraint Violated");
            CreateGameEvent(EVENT_DATA_CHANGED_PATH, "On Data Changed");
            CreateGameEvent(EVENT_SLIDESHOW_STARTED_PATH, "On Slideshow Started");
            CreateGameEvent(EVENT_SLIDESHOW_ARTWORK_CHANGED_PATH, "On Slideshow Artwork Changed");
            CreateGameEvent(EVENT_SLIDESHOW_COMPLETE_PATH, "On Slideshow Complete");
            CreateGameEvent(EVENT_SLIDESHOW_EXIT_REQUESTED_PATH, "On Slideshow Exit Requested");
            CreateGameEvent(EVENT_ARTWORK_LIKED_PATH, "On Artwork Liked");

            // Create ArtworkEventSO
            CreateArtworkEvent();

            // Create Variable SOs
            CreateAppStateVariable();
            CreateIntVariable(ARTWORK_COUNT_PATH, "Artwork Count");
            CreateIntVariable(ACTIVE_REQUEST_COUNT_PATH, "Active Request Count");

            // Create config SOs
            CreateChallengeConfig();
            CreateSlideshowConfig();
            CreateAudioConfig();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            _statusMessage = $"Setup complete! Created {_createdCount} assets.";
            Debug.Log($"[DataInfrastructureWizard] Setup complete. Created {_createdCount} assets.");
        }

        private bool HasExistingAssets()
        {
            return AssetDatabase.LoadAssetAtPath<Object>(EVENT_FIREWORK_COMPLETE_PATH) != null ||
                   AssetDatabase.LoadAssetAtPath<Object>(APP_STATE_PATH) != null ||
                   AssetDatabase.LoadAssetAtPath<Object>(CHALLENGE_CONFIG_PATH) != null ||
                   AssetDatabase.LoadAssetAtPath<Object>(ARTWORK_COUNT_PATH) != null;
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

        private void CreateGameEvent(string path, string eventName)
        {
            GameEventSO gameEvent = CreateInstance<GameEventSO>();
            gameEvent.name = eventName;
            AssetDatabase.CreateAsset(gameEvent, path);
            LogCreated(path);
        }

        private void CreateArtworkEvent()
        {
            ArtworkEventSO artworkEvent = CreateInstance<ArtworkEventSO>();
            artworkEvent.name = "On Slideshow Artwork Started";
            AssetDatabase.CreateAsset(artworkEvent, EVENT_SLIDESHOW_ARTWORK_STARTED_PATH);
            LogCreated(EVENT_SLIDESHOW_ARTWORK_STARTED_PATH);
        }

        private void CreateAppStateVariable()
        {
            AppStateVariableSO appState = CreateInstance<AppStateVariableSO>();
            appState.name = "App State";
            AssetDatabase.CreateAsset(appState, APP_STATE_PATH);
            LogCreated(APP_STATE_PATH);
        }

        private void CreateChallengeConfig()
        {
            ChallengeConfigSO config = CreateInstance<ChallengeConfigSO>();

            SerializedObject serialized = new SerializedObject(config);
            SerializedProperty requestsProp = serialized.FindProperty("_predefinedRequests");
            requestsProp.arraySize = 5;

            // Request 0: Draw a heart — ColorLimit(3)
            SerializedProperty req0 = requestsProp.GetArrayElementAtIndex(0);
            req0.FindPropertyRelative("_id").stringValue = "req-heart";
            req0.FindPropertyRelative("_prompt").stringValue = "Draw a heart";
            req0.FindPropertyRelative("_isCompleted").boolValue = false;
            SerializedProperty constraints0 = req0.FindPropertyRelative("_constraints");
            constraints0.arraySize = 1;
            SerializedProperty c0_0 = constraints0.GetArrayElementAtIndex(0);
            c0_0.FindPropertyRelative("_type").enumValueIndex = (int)ConstraintType.ColorLimit;
            c0_0.FindPropertyRelative("_intValue").intValue = 3;

            // Request 1: Draw a star — TimeLimit(30)
            SerializedProperty req1 = requestsProp.GetArrayElementAtIndex(1);
            req1.FindPropertyRelative("_id").stringValue = "req-star";
            req1.FindPropertyRelative("_prompt").stringValue = "Draw a star";
            req1.FindPropertyRelative("_isCompleted").boolValue = false;
            SerializedProperty constraints1 = req1.FindPropertyRelative("_constraints");
            constraints1.arraySize = 1;
            SerializedProperty c1_0 = constraints1.GetArrayElementAtIndex(0);
            c1_0.FindPropertyRelative("_type").enumValueIndex = (int)ConstraintType.TimeLimit;
            c1_0.FindPropertyRelative("_floatValue").floatValue = 30f;

            // Request 2: Draw a smiley face — SymmetryRequired(true)
            SerializedProperty req2 = requestsProp.GetArrayElementAtIndex(2);
            req2.FindPropertyRelative("_id").stringValue = "req-smiley";
            req2.FindPropertyRelative("_prompt").stringValue = "Draw a smiley face";
            req2.FindPropertyRelative("_isCompleted").boolValue = false;
            SerializedProperty constraints2 = req2.FindPropertyRelative("_constraints");
            constraints2.arraySize = 1;
            SerializedProperty c2_0 = constraints2.GetArrayElementAtIndex(0);
            c2_0.FindPropertyRelative("_type").enumValueIndex = (int)ConstraintType.SymmetryRequired;
            c2_0.FindPropertyRelative("_boolValue").boolValue = true;

            // Request 3: Draw a rocket — PixelLimit(50)
            SerializedProperty req3 = requestsProp.GetArrayElementAtIndex(3);
            req3.FindPropertyRelative("_id").stringValue = "req-rocket";
            req3.FindPropertyRelative("_prompt").stringValue = "Draw a rocket";
            req3.FindPropertyRelative("_isCompleted").boolValue = false;
            SerializedProperty constraints3 = req3.FindPropertyRelative("_constraints");
            constraints3.arraySize = 1;
            SerializedProperty c3_0 = constraints3.GetArrayElementAtIndex(0);
            c3_0.FindPropertyRelative("_type").enumValueIndex = (int)ConstraintType.PixelLimit;
            c3_0.FindPropertyRelative("_intValue").intValue = 50;

            // Request 4: Draw a flower — ColorLimit(4) + TimeLimit(45)
            SerializedProperty req4 = requestsProp.GetArrayElementAtIndex(4);
            req4.FindPropertyRelative("_id").stringValue = "req-flower";
            req4.FindPropertyRelative("_prompt").stringValue = "Draw a flower";
            req4.FindPropertyRelative("_isCompleted").boolValue = false;
            SerializedProperty constraints4 = req4.FindPropertyRelative("_constraints");
            constraints4.arraySize = 2;
            SerializedProperty c4_0 = constraints4.GetArrayElementAtIndex(0);
            c4_0.FindPropertyRelative("_type").enumValueIndex = (int)ConstraintType.ColorLimit;
            c4_0.FindPropertyRelative("_intValue").intValue = 4;
            SerializedProperty c4_1 = constraints4.GetArrayElementAtIndex(1);
            c4_1.FindPropertyRelative("_type").enumValueIndex = (int)ConstraintType.TimeLimit;
            c4_1.FindPropertyRelative("_floatValue").floatValue = 45f;

            serialized.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.CreateAsset(config, CHALLENGE_CONFIG_PATH);
            LogCreated(CHALLENGE_CONFIG_PATH);
        }

        private void CreateIntVariable(string path, string variableName)
        {
            IntVariableSO variable = CreateInstance<IntVariableSO>();
            variable.name = variableName;
            AssetDatabase.CreateAsset(variable, path);
            LogCreated(path);
        }

        private void CreateSlideshowConfig()
        {
            SlideshowConfigSO config = CreateInstance<SlideshowConfigSO>();
            AssetDatabase.CreateAsset(config, SLIDESHOW_CONFIG_PATH);
            LogCreated(SLIDESHOW_CONFIG_PATH);
        }

        private void CreateAudioConfig()
        {
            AudioConfigSO config = CreateInstance<AudioConfigSO>();
            AssetDatabase.CreateAsset(config, AUDIO_CONFIG_PATH);
            LogCreated(AUDIO_CONFIG_PATH);
        }

        private void LogCreated(string path)
        {
            _createdCount++;
            Debug.Log($"[DataInfrastructureWizard] Created: {path}");
        }
    }
}
