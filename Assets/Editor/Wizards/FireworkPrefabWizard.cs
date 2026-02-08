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
    public class FireworkPrefabWizard : EditorWindow
    {
        // ---- Constants ----
        private const string PREFAB_FOLDER = "Assets/Prefabs";
        private const string MATERIAL_FOLDER = "Assets/Art/Materials";
        private const string DATA_CONFIG_FOLDER = "Assets/Data/Config";
        private const string SHADER_NAME = "HanabiCanvas/FireworkParticle";
        private const string FIREWORK_PREFAB_PATH = "Assets/Prefabs/Firework.prefab";
        private const string LAUNCHER_PREFAB_PATH = "Assets/Prefabs/FireworkLauncher.prefab";
        private const string MATERIAL_PATH = "Assets/Art/Materials/FireworkParticle.mat";
        private const string ACTIVE_FIREWORKS_PATH = "Assets/Data/Config/Active Fireworks.asset";
        private const string TEST_SMILEY_PATH = "Assets/Data/Config/Test Smiley.asset";

        // ---- Private Fields ----
        private FireworkConfigSO _fireworkConfig;
        private GameEventSO _onLaunchFirework;
        private PixelDataSO _pixelData;
        private Vector2 _scrollPosition;
        private string _statusMessage = "";
        private int _createdCount;

        // ---- Menu Item ----
        [MenuItem("Tools/Hanabi Canvas/Firework Prefab Builder")]
        private static void ShowWindow()
        {
            FireworkPrefabWizard window = GetWindow<FireworkPrefabWizard>("Firework Prefab Builder");
            window.minSize = new Vector2(450, 400);
            window.Show();
        }

        // ---- Unity Methods ----
        private void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            EditorGUILayout.LabelField("Hanabi Canvas — Firework Prefab Builder", EditorStyles.boldLabel);
            EditorGUILayout.Space(8);
            EditorGUILayout.HelpBox(
                "Creates prefabs for the Firework system:\n" +
                "• Firework prefab (particle instance + mesh renderer)\n" +
                "• FireworkLauncher prefab (event listener + spawner)\n" +
                "• Additive particle material\n" +
                "• Active Fireworks list SO\n" +
                "• Test Smiley pixel data for testing",
                MessageType.Info);
            EditorGUILayout.Space(8);

            EditorGUILayout.LabelField("Input Assets", EditorStyles.boldLabel);
            _fireworkConfig = (FireworkConfigSO)EditorGUILayout.ObjectField(
                "Firework Config", _fireworkConfig, typeof(FireworkConfigSO), false);
            _onLaunchFirework = (GameEventSO)EditorGUILayout.ObjectField(
                "OnLaunchFirework Event", _onLaunchFirework, typeof(GameEventSO), false);
            _pixelData = (PixelDataSO)EditorGUILayout.ObjectField(
                "Pixel Data (Output)", _pixelData, typeof(PixelDataSO), false);

            EditorGUILayout.Space(8);

            bool isValid = Validate();

            EditorGUI.BeginDisabledGroup(!isValid);
            if (GUILayout.Button("Create Prefabs", GUILayout.Height(32)))
            {
                CreatePrefabs();
            }
            EditorGUI.EndDisabledGroup();

            if (!string.IsNullOrEmpty(_statusMessage))
            {
                EditorGUILayout.Space(8);
                EditorGUILayout.HelpBox(_statusMessage, MessageType.Info);
            }

            EditorGUILayout.EndScrollView();
        }

        // ---- Private Methods ----
        private bool Validate()
        {
            if (_fireworkConfig == null)
            {
                _statusMessage = "Please assign a FireworkConfigSO.";
                return false;
            }

            if (_onLaunchFirework == null)
            {
                _statusMessage = "Please assign the OnLaunchFirework event.";
                return false;
            }

            if (_pixelData == null)
            {
                _statusMessage = "Please assign the Pixel Data (Output) SO.";
                return false;
            }

            _statusMessage = "";
            return true;
        }

        private void CreatePrefabs()
        {
            _createdCount = 0;

            if (HasExistingPrefabs())
            {
                bool shouldOverwrite = EditorUtility.DisplayDialog(
                    "Overwrite Existing Prefabs?",
                    "Some firework prefabs already exist. Do you want to overwrite them?",
                    "Overwrite",
                    "Cancel");

                if (!shouldOverwrite)
                {
                    _statusMessage = "Operation cancelled.";
                    return;
                }
            }

            EnsureDirectoriesExist();

            Material particleMaterial = CreateParticleMaterial();
            FireworkInstanceListSO activeFireworks = CreateActiveFireworksList();
            CreateFireworkPrefab(particleMaterial, activeFireworks);
            CreateLauncherPrefab(activeFireworks);
            CreateTestSmiley();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            _statusMessage = $"Done! Created {_createdCount} assets. Check the Prefabs/ folder.";
            Debug.Log($"[FireworkPrefabWizard] Complete. Created {_createdCount} assets.");
        }

        private bool HasExistingPrefabs()
        {
            return AssetDatabase.LoadAssetAtPath<Object>(FIREWORK_PREFAB_PATH) != null ||
                   AssetDatabase.LoadAssetAtPath<Object>(LAUNCHER_PREFAB_PATH) != null;
        }

        private void EnsureDirectoriesExist()
        {
            EnsureDirectory("Assets/Prefabs");
            EnsureDirectory("Assets/Art");
            EnsureDirectory("Assets/Art/Materials");
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

        private Material CreateParticleMaterial()
        {
            Material existing = AssetDatabase.LoadAssetAtPath<Material>(MATERIAL_PATH);
            if (existing != null)
            {
                return existing;
            }

            Shader shader = Shader.Find(SHADER_NAME);
            if (shader == null)
            {
                Debug.LogWarning(
                    $"[FireworkPrefabWizard] Shader '{SHADER_NAME}' not found. " +
                    "Using URP/Unlit fallback.");
                shader = Shader.Find("Universal Render Pipeline/Unlit");
            }

            Material material = new Material(shader);
            material.name = "FireworkParticle";

            AssetDatabase.CreateAsset(material, MATERIAL_PATH);
            LogCreated(MATERIAL_PATH);
            return material;
        }

        private FireworkInstanceListSO CreateActiveFireworksList()
        {
            FireworkInstanceListSO existing =
                AssetDatabase.LoadAssetAtPath<FireworkInstanceListSO>(ACTIVE_FIREWORKS_PATH);
            if (existing != null)
            {
                return existing;
            }

            FireworkInstanceListSO listSO = CreateInstance<FireworkInstanceListSO>();
            listSO.name = "Active Fireworks";
            AssetDatabase.CreateAsset(listSO, ACTIVE_FIREWORKS_PATH);
            LogCreated(ACTIVE_FIREWORKS_PATH);
            return listSO;
        }

        private void CreateFireworkPrefab(Material particleMaterial,
            FireworkInstanceListSO activeFireworks)
        {
            GameObject root = new GameObject("Firework");

            FireworkInstance instance = root.AddComponent<FireworkInstance>();
            SerializedObject instanceSO = new SerializedObject(instance);
            instanceSO.FindProperty("_config").objectReferenceValue = _fireworkConfig;
            instanceSO.FindProperty("_activeFireworks").objectReferenceValue = activeFireworks;
            instanceSO.ApplyModifiedPropertiesWithoutUndo();

            GameObject rendererObj = new GameObject("ParticleRenderer");
            rendererObj.transform.SetParent(root.transform);
            rendererObj.transform.localPosition = Vector3.zero;
            rendererObj.transform.localRotation = Quaternion.identity;

            MeshFilter meshFilter = rendererObj.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = rendererObj.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = particleMaterial;

            FireworkMeshRenderer particleRenderer = rendererObj.AddComponent<FireworkMeshRenderer>();
            SerializedObject rendererSO = new SerializedObject(particleRenderer);
            rendererSO.FindProperty("_fireworkInstance").objectReferenceValue = instance;
            rendererSO.FindProperty("_particleMaterial").objectReferenceValue = particleMaterial;
            rendererSO.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(root, FIREWORK_PREFAB_PATH);
            DestroyImmediate(root);
            LogCreated(FIREWORK_PREFAB_PATH);
        }

        private void CreateLauncherPrefab(FireworkInstanceListSO activeFireworks)
        {
            GameObject root = new GameObject("FireworkLauncher");

            GameObject spawnPoint = new GameObject("SpawnPoint");
            spawnPoint.transform.SetParent(root.transform);
            spawnPoint.transform.localPosition = new Vector3(0f, 10f, 0f);

            GameObject fireworkPrefab =
                AssetDatabase.LoadAssetAtPath<GameObject>(FIREWORK_PREFAB_PATH);

            FireworkLauncher launcher = root.AddComponent<FireworkLauncher>();
            SerializedObject launcherSO = new SerializedObject(launcher);
            launcherSO.FindProperty("_onLaunchFirework").objectReferenceValue = _onLaunchFirework;
            launcherSO.FindProperty("_fireworkConfig").objectReferenceValue = _fireworkConfig;
            launcherSO.FindProperty("_pixelData").objectReferenceValue = _pixelData;
            launcherSO.FindProperty("_fireworkPrefab").objectReferenceValue = fireworkPrefab;
            launcherSO.FindProperty("_spawnPoint").objectReferenceValue = spawnPoint.transform;
            launcherSO.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(root, LAUNCHER_PREFAB_PATH);
            DestroyImmediate(root);
            LogCreated(LAUNCHER_PREFAB_PATH);
        }

        private void CreateTestSmiley()
        {
            PixelDataSO existing = AssetDatabase.LoadAssetAtPath<PixelDataSO>(TEST_SMILEY_PATH);
            if (existing != null)
            {
                return;
            }

            PixelDataSO smiley = CreateInstance<PixelDataSO>();

            SerializedObject serialized = new SerializedObject(smiley);
            serialized.FindProperty("_width").intValue = 8;
            serialized.FindProperty("_height").intValue = 8;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            Color32 yellow = new Color32(255, 255, 0, 255);

            // Eyes
            smiley.SetPixel(2, 5, yellow);
            smiley.SetPixel(5, 5, yellow);

            // Mouth curve
            smiley.SetPixel(1, 2, yellow);
            smiley.SetPixel(2, 1, yellow);
            smiley.SetPixel(3, 1, yellow);
            smiley.SetPixel(4, 1, yellow);
            smiley.SetPixel(5, 1, yellow);
            smiley.SetPixel(6, 2, yellow);

            AssetDatabase.CreateAsset(smiley, TEST_SMILEY_PATH);
            LogCreated(TEST_SMILEY_PATH);
        }

        private void LogCreated(string path)
        {
            _createdCount++;
            Debug.Log($"[FireworkPrefabWizard] Created: {path}");
        }
    }
}
