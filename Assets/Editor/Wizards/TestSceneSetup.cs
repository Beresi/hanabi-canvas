// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using HanabiCanvas.Runtime;
using HanabiCanvas.Runtime.Events;
using HanabiCanvas.Runtime.Firework;

namespace HanabiCanvas.Editor
{
    public class TestSceneSetup : EditorWindow
    {
        // ---- Constants ----
        private const string SCENE_PATH = "Assets/Scenes/FireworkTest.unity";
        private const string FIREWORK_PREFAB_PATH = "Assets/Prefabs/Firework.prefab";
        private const string LAUNCHER_PREFAB_PATH = "Assets/Prefabs/FireworkLauncher.prefab";
        private const string ON_LAUNCH_EVENT_PATH = "Assets/Data/Config/OnLaunchFirework.asset";
        private const string TEST_SMILEY_PATH = "Assets/Data/Config/Test Smiley.asset";
        private const string FIREWORK_CONFIG_PATH = "Assets/Data/Fireworks/Default Firework Config.asset";
        private const string ACTIVE_FIREWORKS_PATH = "Assets/Data/Config/Active Fireworks.asset";

        // ---- Private Fields ----
        private string _statusMessage = "";

        // ---- Menu Item ----
        [MenuItem("Tools/Hanabi Canvas/Setup Firework Test Scene")]
        private static void ShowWindow()
        {
            TestSceneSetup window = GetWindow<TestSceneSetup>("Firework Test Scene");
            window.minSize = new Vector2(400, 250);
            window.Show();
        }

        // ---- Unity Methods ----
        private void OnGUI()
        {
            EditorGUILayout.LabelField("Hanabi Canvas — Firework Test Scene", EditorStyles.boldLabel);
            EditorGUILayout.Space(8);
            EditorGUILayout.HelpBox(
                "Creates a minimal scene to test the firework system:\n" +
                "• Camera aimed at spawn point\n" +
                "• FireworkLauncher with Test Smiley data\n" +
                "• Press Space to launch fireworks\n\n" +
                "Run the Firework Prefab Builder wizard first!",
                MessageType.Info);
            EditorGUILayout.Space(8);

            if (!ValidatePrerequisites())
            {
                EditorGUILayout.HelpBox(_statusMessage, MessageType.Warning);
                return;
            }

            if (GUILayout.Button("Create Test Scene", GUILayout.Height(32)))
            {
                CreateTestScene();
            }

            if (!string.IsNullOrEmpty(_statusMessage))
            {
                EditorGUILayout.Space(8);
                EditorGUILayout.HelpBox(_statusMessage, MessageType.Info);
            }
        }

        // ---- Private Methods ----
        private bool ValidatePrerequisites()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(FIREWORK_PREFAB_PATH) == null)
            {
                _statusMessage = "Firework prefab not found. Run the Firework Prefab Builder first.";
                return false;
            }

            if (AssetDatabase.LoadAssetAtPath<GameEventSO>(ON_LAUNCH_EVENT_PATH) == null)
            {
                _statusMessage = "OnLaunchFirework event asset not found.";
                return false;
            }

            if (AssetDatabase.LoadAssetAtPath<PixelDataSO>(TEST_SMILEY_PATH) == null)
            {
                _statusMessage = "Test Smiley asset not found. Run the Firework Prefab Builder first.";
                return false;
            }

            _statusMessage = "";
            return true;
        }

        private void CreateTestScene()
        {
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            EnsureDirectory("Assets/Scenes");

            GameEventSO onLaunchEvent = AssetDatabase.LoadAssetAtPath<GameEventSO>(ON_LAUNCH_EVENT_PATH);
            PixelDataSO testSmiley = AssetDatabase.LoadAssetAtPath<PixelDataSO>(TEST_SMILEY_PATH);
            FireworkConfigSO fireworkConfig = AssetDatabase.LoadAssetAtPath<FireworkConfigSO>(FIREWORK_CONFIG_PATH);
            GameObject fireworkPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(FIREWORK_PREFAB_PATH);

            // Camera
            GameObject cameraObj = new GameObject("Main Camera");
            cameraObj.tag = "MainCamera";
            Camera camera = cameraObj.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.02f, 0.02f, 0.08f, 1f);
            camera.orthographic = false;
            camera.fieldOfView = 60f;
            cameraObj.transform.position = new Vector3(0f, 10f, -15f);
            cameraObj.transform.LookAt(new Vector3(0f, 10f, 0f));

            // Directional Light (dim)
            GameObject lightObj = new GameObject("Directional Light");
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 0.2f;
            light.color = new Color(0.5f, 0.5f, 0.7f, 1f);
            lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            // Firework Launcher
            GameObject launcherPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(LAUNCHER_PREFAB_PATH);
            GameObject launcherObj;

            if (launcherPrefab != null)
            {
                launcherObj = (GameObject)PrefabUtility.InstantiatePrefab(launcherPrefab);
                launcherObj.name = "FireworkLauncher";

                FireworkLauncher launcher = launcherObj.GetComponent<FireworkLauncher>();
                if (launcher != null)
                {
                    SerializedObject launcherSO = new SerializedObject(launcher);
                    launcherSO.FindProperty("_pixelData").objectReferenceValue = testSmiley;
                    launcherSO.ApplyModifiedPropertiesWithoutUndo();
                }
            }
            else
            {
                launcherObj = new GameObject("FireworkLauncher");

                GameObject spawnPoint = new GameObject("SpawnPoint");
                spawnPoint.transform.SetParent(launcherObj.transform);
                spawnPoint.transform.localPosition = new Vector3(0f, 10f, 0f);

                FireworkLauncher launcher = launcherObj.AddComponent<FireworkLauncher>();
                SerializedObject launcherSO = new SerializedObject(launcher);
                launcherSO.FindProperty("_onLaunchFirework").objectReferenceValue = onLaunchEvent;
                launcherSO.FindProperty("_fireworkConfig").objectReferenceValue = fireworkConfig;
                launcherSO.FindProperty("_pixelData").objectReferenceValue = testSmiley;
                launcherSO.FindProperty("_fireworkPrefab").objectReferenceValue = fireworkPrefab;
                launcherSO.FindProperty("_spawnPoint").objectReferenceValue = spawnPoint.transform;
                launcherSO.ApplyModifiedPropertiesWithoutUndo();
            }

            // Test Trigger (Space to launch)
            GameObject triggerObj = new GameObject("TestTrigger");
            FireworkTestTrigger trigger = triggerObj.AddComponent<FireworkTestTrigger>();
            SerializedObject triggerSO = new SerializedObject(trigger);
            triggerSO.FindProperty("_onLaunchFirework").objectReferenceValue = onLaunchEvent;
            triggerSO.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(newScene, SCENE_PATH);
            AssetDatabase.Refresh();

            _statusMessage = "Test scene created at " + SCENE_PATH + ". Press Play, then Space to launch!";
            Debug.Log("[TestSceneSetup] Scene created: " + SCENE_PATH);
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
