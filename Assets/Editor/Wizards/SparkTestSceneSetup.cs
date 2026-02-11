// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using HanabiCanvas.Runtime;
using HanabiCanvas.Runtime.Firework;

namespace HanabiCanvas.Editor
{
    public class SparkTestSceneSetup : EditorWindow
    {
        // ---- Constants ----
        private const string SCENE_PATH = "Assets/Scenes/SparkTest.unity";
        private const string BURST_BEHAVIOUR_PATH = "Assets/Data/Fireworks/Default Burst Behaviour.asset";
        private const string RING_BEHAVIOUR_PATH = "Assets/Data/Fireworks/Default Ring Behaviour.asset";
        private const string PATTERN_BEHAVIOUR_PATH = "Assets/Data/Fireworks/Default Pattern Behaviour.asset";
        private const string SPARK_EVENT_PATH = "Assets/Data/Fireworks/On Spark Requested.asset";
        private const string MATERIAL_PATH = "Assets/Art/Materials/FireworkParticle.mat";
        private const string SHADER_NAME = "HanabiCanvas/FireworkParticle";
        private const string TEST_HEART_PATH = "Assets/Data/Config/Test Heart.asset";
        private const int HEART_GRID_SIZE = 32;

        // ---- Private Fields ----
        private string _statusMessage = "";

        // ---- Menu Item ----
        [MenuItem("Tools/Hanabi Canvas/Setup Spark Test Scene")]
        private static void ShowWindow()
        {
            SparkTestSceneSetup window = GetWindow<SparkTestSceneSetup>("Spark Test Scene");
            window.minSize = new Vector2(400, 250);
            window.Show();
        }

        // ---- Unity Methods ----
        private void OnGUI()
        {
            EditorGUILayout.LabelField("Hanabi Canvas — Spark Test Scene", EditorStyles.boldLabel);
            EditorGUILayout.Space(8);
            EditorGUILayout.HelpBox(
                "Creates a minimal scene to test the spark system:\n" +
                "• Camera aimed at spawn point\n" +
                "• SparkManager with ring + pattern behaviours\n" +
                "• SparkTestTrigger — press Space to burst",
                MessageType.Info);
            EditorGUILayout.Space(8);

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
        private void CreateTestScene()
        {
            EnsureDirectory("Assets/Scenes");
            EnsureDirectory("Assets/Data/Fireworks");
            EnsureDirectory("Assets/Data/Config");
            EnsureDirectory("Assets/Art/Materials");

            // Create SO assets if missing
            if (AssetDatabase.LoadAssetAtPath<BurstSparkBehaviourSO>(BURST_BEHAVIOUR_PATH) == null)
            {
                BurstSparkBehaviourSO newBurst = ScriptableObject.CreateInstance<BurstSparkBehaviourSO>();
                AssetDatabase.CreateAsset(newBurst, BURST_BEHAVIOUR_PATH);
            }

            if (AssetDatabase.LoadAssetAtPath<RingSparkBehaviourSO>(RING_BEHAVIOUR_PATH) == null)
            {
                RingSparkBehaviourSO newRing = ScriptableObject.CreateInstance<RingSparkBehaviourSO>();
                AssetDatabase.CreateAsset(newRing, RING_BEHAVIOUR_PATH);
            }

            if (AssetDatabase.LoadAssetAtPath<PatternSparkBehaviourSO>(PATTERN_BEHAVIOUR_PATH) == null)
            {
                PatternSparkBehaviourSO newPattern = ScriptableObject.CreateInstance<PatternSparkBehaviourSO>();
                AssetDatabase.CreateAsset(newPattern, PATTERN_BEHAVIOUR_PATH);
            }

            if (AssetDatabase.LoadAssetAtPath<SparkRequestEventSO>(SPARK_EVENT_PATH) == null)
            {
                SparkRequestEventSO newEvent = ScriptableObject.CreateInstance<SparkRequestEventSO>();
                AssetDatabase.CreateAsset(newEvent, SPARK_EVENT_PATH);
            }

            // Create material if missing
            if (AssetDatabase.LoadAssetAtPath<Material>(MATERIAL_PATH) == null)
            {
                Shader shader = Shader.Find(SHADER_NAME);
                if (shader == null)
                {
                    shader = Shader.Find("Universal Render Pipeline/Unlit");
                }

                Material newMaterial = new Material(shader);
                AssetDatabase.CreateAsset(newMaterial, MATERIAL_PATH);
            }

            // Create test heart pixel data if missing
            if (AssetDatabase.LoadAssetAtPath<PixelDataSO>(TEST_HEART_PATH) == null)
            {
                PixelDataSO heart = CreateHeartPattern();
                AssetDatabase.CreateAsset(heart, TEST_HEART_PATH);
            }

            // Flush and reimport so GUIDs are stable
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Reload all assets from disk to get proper persistent references
            RingSparkBehaviourSO ringBehaviour =
                AssetDatabase.LoadAssetAtPath<RingSparkBehaviourSO>(RING_BEHAVIOUR_PATH);
            PatternSparkBehaviourSO patternBehaviour =
                AssetDatabase.LoadAssetAtPath<PatternSparkBehaviourSO>(PATTERN_BEHAVIOUR_PATH);
            SparkRequestEventSO sparkEvent =
                AssetDatabase.LoadAssetAtPath<SparkRequestEventSO>(SPARK_EVENT_PATH);
            Material particleMaterial =
                AssetDatabase.LoadAssetAtPath<Material>(MATERIAL_PATH);
            PixelDataSO heartData =
                AssetDatabase.LoadAssetAtPath<PixelDataSO>(TEST_HEART_PATH);

            // Create new scene
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

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

            // SparkManager
            GameObject sparkManagerObj = new GameObject("SparkManager");
            SparkManager sparkManager = sparkManagerObj.AddComponent<SparkManager>();

            SerializedObject sparkManagerSO = new SerializedObject(sparkManager);

            // Wire behaviours array
            SerializedProperty behavioursArray = sparkManagerSO.FindProperty("_behaviours");
            behavioursArray.arraySize = 2;
            behavioursArray.GetArrayElementAtIndex(0).objectReferenceValue = ringBehaviour;
            behavioursArray.GetArrayElementAtIndex(1).objectReferenceValue = patternBehaviour;

            sparkManagerSO.FindProperty("_onSparkRequested").objectReferenceValue = sparkEvent;
            sparkManagerSO.FindProperty("_particleMaterial").objectReferenceValue = particleMaterial;
            sparkManagerSO.ApplyModifiedPropertiesWithoutUndo();

            MeshRenderer meshRenderer = sparkManagerObj.GetComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = particleMaterial;

            // SparkTestTrigger
            GameObject triggerObj = new GameObject("SparkTestTrigger");

            GameObject spawnPoint = new GameObject("SpawnPoint");
            spawnPoint.transform.SetParent(triggerObj.transform);
            spawnPoint.transform.localPosition = new Vector3(0f, 10f, 0f);

            SparkTestTrigger trigger = triggerObj.AddComponent<SparkTestTrigger>();
            SerializedObject triggerSO = new SerializedObject(trigger);
            triggerSO.FindProperty("_onSparkRequested").objectReferenceValue = sparkEvent;
            triggerSO.FindProperty("_spawnPoint").objectReferenceValue = spawnPoint.transform;
            triggerSO.FindProperty("_pixelData").objectReferenceValue = heartData;
            triggerSO.ApplyModifiedPropertiesWithoutUndo();

            // Save scene
            EditorSceneManager.SaveScene(newScene, SCENE_PATH);
            AssetDatabase.Refresh();

            _statusMessage = "Test scene created at Assets/Scenes/SparkTest.unity. Press Play, then Space to launch!";
            Debug.Log("[SparkTestSceneSetup] Scene created: " + SCENE_PATH);
        }

        /// <summary>
        /// Generates a 32x32 heart pixel art with 4 color zones using the
        /// implicit heart equation: (x^2 + y^2 - 1)^3 - x^2 * y^3 &lt; 0.
        /// </summary>
        private static PixelDataSO CreateHeartPattern()
        {
            PixelDataSO heart = ScriptableObject.CreateInstance<PixelDataSO>();

            SerializedObject so = new SerializedObject(heart);
            so.FindProperty("_width").intValue = HEART_GRID_SIZE;
            so.FindProperty("_height").intValue = HEART_GRID_SIZE;
            so.ApplyModifiedPropertiesWithoutUndo();

            // 4 color zones from edge to core
            Color32 deepCrimson = new Color32(180, 25, 40, 255);
            Color32 brightRed = new Color32(230, 50, 60, 255);
            Color32 warmPink = new Color32(255, 120, 140, 255);
            Color32 lightPink = new Color32(255, 200, 215, 255);

            // Center and scale for a heart that fills most of the 32x32 grid
            float centerX = 15.5f;
            float centerY = 14.0f;
            float scaleX = 11.0f;
            float scaleY = 13.0f;

            for (int y = 0; y < HEART_GRID_SIZE; y++)
            {
                for (int x = 0; x < HEART_GRID_SIZE; x++)
                {
                    // Normalize to heart equation space
                    float nx = (x - centerX) / scaleX;
                    float ny = (y - centerY) / scaleY; // y=0 is bottom, bumps at top

                    // Implicit heart: (x^2 + y^2 - 1)^3 - x^2 * y^3
                    float x2 = nx * nx;
                    float y3 = ny * ny * ny;
                    float sum = x2 + ny * ny - 1f;
                    float val = sum * sum * sum - x2 * y3;

                    if (val >= 0f)
                    {
                        continue;
                    }

                    // Pick color by depth inside the heart
                    Color32 color;
                    if (val > -0.02f)
                    {
                        color = deepCrimson;
                    }
                    else if (val > -0.15f)
                    {
                        color = brightRed;
                    }
                    else if (val > -0.4f)
                    {
                        color = warmPink;
                    }
                    else
                    {
                        color = lightPink;
                    }

                    heart.SetPixel((byte)x, (byte)y, color);
                }
            }

            return heart;
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
