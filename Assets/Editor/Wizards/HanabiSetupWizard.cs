// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using UnityEditor;
using UnityEngine;
using HanabiCanvas.Runtime;
using HanabiCanvas.Runtime.Events;

namespace HanabiCanvas.Editor
{
    public class HanabiSetupWizard : EditorWindow
    {
        // ---- Constants ----
        private const string PALETTE_PATH = "Assets/Data/Palettes/Default Color Palette.asset";
        private const string CANVAS_CONFIG_PATH = "Assets/Data/Config/Default Canvas Config.asset";
        private const string BURST_PHASE_PATH = "Assets/Data/Fireworks/Burst Phase.asset";
        private const string STEER_PHASE_PATH = "Assets/Data/Fireworks/Steer Phase.asset";
        private const string HOLD_PHASE_PATH = "Assets/Data/Fireworks/Hold Phase.asset";
        private const string FADE_PHASE_PATH = "Assets/Data/Fireworks/Fade Phase.asset";
        private const string FIREWORK_CONFIG_PATH = "Assets/Data/Fireworks/Default Firework Config.asset";
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
                "This includes a color palette, canvas config, firework phases, " +
                "firework config, and game event channels.",
                MessageType.Info);
            EditorGUILayout.Space(8);

            EditorGUILayout.LabelField("Assets to Create:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("  - Default Color Palette (8 classic firework colors)");
            EditorGUILayout.LabelField("  - Default Canvas Config (32x32 grid)");
            EditorGUILayout.LabelField("  - 4 Firework Phases (Burst, Steer, Hold, Fade)");
            EditorGUILayout.LabelField("  - Default Firework Config");
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

            ColorPaletteSO palette = CreateColorPalette();
            CanvasConfigSO canvasConfig = CreateCanvasConfig(palette);
            FireworkPhaseSO burstPhase = CreateFireworkPhase(
                BURST_PHASE_PATH, "Burst", 0.15f,
                AnimationCurve.Linear(0f, 0f, 1f, 1f),
                "All particles move outward along initial velocity.");
            FireworkPhaseSO steerPhase = CreateFireworkPhase(
                STEER_PHASE_PATH, "Steer", 0.7f,
                AnimationCurve.EaseInOut(0f, 0f, 1f, 1f),
                "Pattern particles steer toward formation positions.");
            FireworkPhaseSO holdPhase = CreateFireworkPhase(
                HOLD_PHASE_PATH, "Hold", 2.0f,
                AnimationCurve.Linear(0f, 0f, 1f, 1f),
                "Pattern particles hold at formation with sparkle. Debris fades.");
            FireworkPhaseSO fadePhase = CreateFireworkPhase(
                FADE_PHASE_PATH, "Fade", 1.5f,
                AnimationCurve.Linear(0f, 0f, 1f, 1f),
                "All particles drift down, shrink, and fade to zero.");

            CreateFireworkConfig(burstPhase, steerPhase, holdPhase, fadePhase);
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
            return AssetDatabase.LoadAssetAtPath<Object>(PALETTE_PATH) != null ||
                   AssetDatabase.LoadAssetAtPath<Object>(CANVAS_CONFIG_PATH) != null ||
                   AssetDatabase.LoadAssetAtPath<Object>(FIREWORK_CONFIG_PATH) != null;
        }

        private void EnsureDirectoriesExist()
        {
            EnsureDirectory("Assets/Data");
            EnsureDirectory("Assets/Data/Palettes");
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

        private ColorPaletteSO CreateColorPalette()
        {
            ColorPaletteSO palette = CreateInstance<ColorPaletteSO>();

            SerializedObject serialized = new SerializedObject(palette);
            serialized.FindProperty("_paletteName").stringValue = "Default Palette";

            SerializedProperty colorsProperty = serialized.FindProperty("_colors");
            colorsProperty.arraySize = 8;

            SetColor32(colorsProperty.GetArrayElementAtIndex(0), 255, 50, 50, 255);
            SetColor32(colorsProperty.GetArrayElementAtIndex(1), 255, 128, 0, 255);
            SetColor32(colorsProperty.GetArrayElementAtIndex(2), 255, 255, 0, 255);
            SetColor32(colorsProperty.GetArrayElementAtIndex(3), 0, 255, 0, 255);
            SetColor32(colorsProperty.GetArrayElementAtIndex(4), 0, 255, 255, 255);
            SetColor32(colorsProperty.GetArrayElementAtIndex(5), 0, 128, 255, 255);
            SetColor32(colorsProperty.GetArrayElementAtIndex(6), 128, 0, 255, 255);
            SetColor32(colorsProperty.GetArrayElementAtIndex(7), 255, 255, 255, 255);

            SetColor32(serialized.FindProperty("_backgroundColor"), 32, 32, 32, 255);

            serialized.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.CreateAsset(palette, PALETTE_PATH);
            LogCreated(PALETTE_PATH);
            return palette;
        }

        private CanvasConfigSO CreateCanvasConfig(ColorPaletteSO palette)
        {
            CanvasConfigSO config = CreateInstance<CanvasConfigSO>();

            SerializedObject serialized = new SerializedObject(config);
            serialized.FindProperty("_gridWidth").intValue = 32;
            serialized.FindProperty("_gridHeight").intValue = 32;
            serialized.FindProperty("_cellSize").floatValue = 0.25f;
            serialized.FindProperty("_defaultPalette").objectReferenceValue = palette;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.CreateAsset(config, CANVAS_CONFIG_PATH);
            LogCreated(CANVAS_CONFIG_PATH);
            return config;
        }

        private FireworkPhaseSO CreateFireworkPhase(
            string path, string phaseName, float duration,
            AnimationCurve curve, string description)
        {
            FireworkPhaseSO phase = CreateInstance<FireworkPhaseSO>();

            SerializedObject serialized = new SerializedObject(phase);
            serialized.FindProperty("_phaseName").stringValue = phaseName;
            serialized.FindProperty("_duration").floatValue = duration;
            serialized.FindProperty("_progressCurve").animationCurveValue = curve;
            serialized.FindProperty("_description").stringValue = description;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.CreateAsset(phase, path);
            LogCreated(path);
            return phase;
        }

        private void CreateFireworkConfig(
            FireworkPhaseSO burst, FireworkPhaseSO steer,
            FireworkPhaseSO hold, FireworkPhaseSO fade)
        {
            FireworkConfigSO config = CreateInstance<FireworkConfigSO>();

            SerializedObject serialized = new SerializedObject(config);

            SerializedProperty phasesProperty = serialized.FindProperty("_phases");
            phasesProperty.arraySize = 4;
            phasesProperty.GetArrayElementAtIndex(0).objectReferenceValue = burst;
            phasesProperty.GetArrayElementAtIndex(1).objectReferenceValue = steer;
            phasesProperty.GetArrayElementAtIndex(2).objectReferenceValue = hold;
            phasesProperty.GetArrayElementAtIndex(3).objectReferenceValue = fade;

            serialized.FindProperty("_burstRadius").floatValue = 5f;
            serialized.FindProperty("_steerStrength").floatValue = 8f;
            serialized.FindProperty("_steerCurve").animationCurveValue =
                AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
            serialized.FindProperty("_holdSparkleIntensity").floatValue = 1f;
            serialized.FindProperty("_fadeGravity").floatValue = 2f;
            serialized.FindProperty("_debrisParticleCount").intValue = 200;
            serialized.FindProperty("_debrisSpeedMultiplier").floatValue = 1.5f;
            serialized.FindProperty("_particleSize").floatValue = 0.1f;
            serialized.FindProperty("_particleSizeFadeMultiplier").floatValue = 0.5f;
            serialized.FindProperty("_formationScale").floatValue = 0.1f;

            serialized.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.CreateAsset(config, FIREWORK_CONFIG_PATH);
            LogCreated(FIREWORK_CONFIG_PATH);
        }

        private void CreateGameEvent(string path, string eventName)
        {
            GameEventSO gameEvent = CreateInstance<GameEventSO>();
            gameEvent.name = eventName;

            AssetDatabase.CreateAsset(gameEvent, path);
            LogCreated(path);
        }

        private void SetColor32(SerializedProperty property, byte r, byte g, byte b, byte a)
        {
            property.colorValue = new Color(r / 255f, g / 255f, b / 255f, a / 255f);
        }

        private void LogCreated(string path)
        {
            _createdCount++;
            Debug.Log($"[HanabiSetupWizard] Created: {path}");
        }
    }
}
