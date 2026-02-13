// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using UnityEditor;
using UnityEngine;
using HanabiCanvas.Runtime;
using HanabiCanvas.Runtime.Events;
using HanabiCanvas.Runtime.Firework;

namespace HanabiCanvas.Editor
{
    public class HanabiSetupWizard : EditorWindow
    {
        // ---- Constants ----
        private const string CANVAS_CONFIG_PATH = "Assets/Data/Config/Default Canvas Config.asset";
        private const string BURST_BEHAVIOUR_PATH = "Assets/Data/Fireworks/Default Burst Behaviour.asset";
        private const string RING_BEHAVIOUR_PATH = "Assets/Data/Fireworks/Default Ring Behaviour.asset";
        private const string PATTERN_BEHAVIOUR_PATH = "Assets/Data/Fireworks/Default Pattern Behaviour.asset";
        private const string FIREWORK_EVENT_PATH = "Assets/Data/Fireworks/On Firework Requested.asset";
        private const string EVENT_LAUNCH_PATH = "Assets/Data/Config/OnLaunchFirework.asset";
        private const string EVENT_CLEARED_PATH = "Assets/Data/Config/OnCanvasCleared.asset";
        private const string EVENT_PHASE_CHANGED_PATH = "Assets/Data/Config/OnPhaseChanged.asset";

        // ---- Private Fields ----
        private Vector2 _scrollPosition;
        private string _statusMessage = "";
        private int _createdCount;

        // ---- Menu Item ----
        [MenuItem("Tools/Hanabi Canvas/Setup Wizard")]
        private static void ShowWindow()
        {
            HanabiSetupWizard window = GetWindow<HanabiSetupWizard>("Hanabi Setup Wizard");
            window.minSize = new Vector2(400, 300);
            window.Show();
        }

        // ---- Unity Methods ----
        private void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            EditorGUILayout.LabelField("Hanabi Canvas — Setup Wizard", EditorStyles.boldLabel);
            EditorGUILayout.Space(8);
            EditorGUILayout.HelpBox(
                "Creates default ScriptableObject assets for the Hanabi Canvas project.\n" +
                "This includes a canvas config, firework behaviours, " +
                "firework request event, and game event channels.",
                MessageType.Info);
            EditorGUILayout.Space(8);

            EditorGUILayout.LabelField("Assets to Create:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("  - Default Canvas Config (32x32 grid)");
            EditorGUILayout.LabelField("  - 3 Firework Behaviours (Burst, Ring, Pattern)");
            EditorGUILayout.LabelField("  - Firework Request Event");
            EditorGUILayout.LabelField("  - 3 Game Events (OnLaunchFirework, OnCanvasCleared, OnPhaseChanged)");
            EditorGUILayout.Space(8);

            if (GUILayout.Button("Create Default Assets", GUILayout.Height(32)))
            {
                CreateDefaultAssets();
            }

            if (!string.IsNullOrEmpty(_statusMessage))
            {
                EditorGUILayout.Space(8);
                EditorGUILayout.HelpBox(_statusMessage, MessageType.Info);
            }

            EditorGUILayout.EndScrollView();
        }

        // ---- Private Methods ----
        private void CreateDefaultAssets()
        {
            _createdCount = 0;

            if (HasExistingAssets())
            {
                bool shouldOverwrite = EditorUtility.DisplayDialog(
                    "Overwrite Existing Assets?",
                    "Some default assets already exist. Do you want to overwrite them?",
                    "Overwrite",
                    "Cancel");

                if (!shouldOverwrite)
                {
                    _statusMessage = "Operation cancelled.";
                    return;
                }
            }

            EnsureDirectoriesExist();

            CreateCanvasConfig();
            CreateFireworkBehaviour<BurstFireworkBehaviourSO>(BURST_BEHAVIOUR_PATH);
            CreateFireworkBehaviour<RingFireworkBehaviourSO>(RING_BEHAVIOUR_PATH);
            CreateFireworkBehaviour<PatternFireworkBehaviourSO>(PATTERN_BEHAVIOUR_PATH);
            CreateFireworkRequestEvent();
            CreateGameEvent(EVENT_LAUNCH_PATH, "OnLaunchFirework");
            CreateGameEvent(EVENT_CLEARED_PATH, "OnCanvasCleared");
            CreateGameEvent(EVENT_PHASE_CHANGED_PATH, "OnPhaseChanged");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            _statusMessage = $"Setup complete! Created {_createdCount} assets.";
            Debug.Log($"[HanabiSetupWizard] Setup complete. Created {_createdCount} assets.");
        }

        private bool HasExistingAssets()
        {
            return AssetDatabase.LoadAssetAtPath<Object>(CANVAS_CONFIG_PATH) != null ||
                   AssetDatabase.LoadAssetAtPath<Object>(BURST_BEHAVIOUR_PATH) != null;
        }

        private void EnsureDirectoriesExist()
        {
            EnsureDirectory("Assets/Data");
            EnsureDirectory("Assets/Data/Config");
            EnsureDirectory("Assets/Data/Fireworks");
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

        private CanvasConfigSO CreateCanvasConfig()
        {
            CanvasConfigSO config = CreateInstance<CanvasConfigSO>();

            SerializedObject serialized = new SerializedObject(config);
            serialized.FindProperty("_gridWidth").intValue = 32;
            serialized.FindProperty("_gridHeight").intValue = 32;
            serialized.FindProperty("_cellSize").floatValue = 0.25f;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.CreateAsset(config, CANVAS_CONFIG_PATH);
            LogCreated(CANVAS_CONFIG_PATH);
            return config;
        }

        private void CreateFireworkBehaviour<T>(string path) where T : FireworkBehaviourSO
        {
            T behaviour = CreateInstance<T>();
            AssetDatabase.CreateAsset(behaviour, path);
            LogCreated(path);
        }

        private void CreateFireworkRequestEvent()
        {
            FireworkRequestEventSO requestEvent = CreateInstance<FireworkRequestEventSO>();
            AssetDatabase.CreateAsset(requestEvent, FIREWORK_EVENT_PATH);
            LogCreated(FIREWORK_EVENT_PATH);
        }

        private void CreateGameEvent(string path, string eventName)
        {
            GameEventSO gameEvent = CreateInstance<GameEventSO>();
            gameEvent.name = eventName;

            AssetDatabase.CreateAsset(gameEvent, path);
            LogCreated(path);
        }

        private void LogCreated(string path)
        {
            _createdCount++;
            Debug.Log($"[HanabiSetupWizard] Created: {path}");
        }
    }
}
