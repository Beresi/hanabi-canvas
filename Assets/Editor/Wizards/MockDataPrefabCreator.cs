// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using UnityEngine;
using UnityEditor;
using HanabiCanvas.Runtime.Persistence;
using HanabiCanvas.Runtime.Utils;

namespace HanabiCanvas.Editor
{
    /// <summary>
    /// Creates a MockDataSeeder prefab that can be dragged into any scene.
    /// Wire the DataManager reference in the Inspector after placing it.
    /// </summary>
    public static class MockDataPrefabCreator
    {
        private const string PREFAB_PATH = "Assets/Prefabs/MockDataSeeder.prefab";

        [MenuItem("Tools/Hanabi Canvas/Create MockDataSeeder Prefab")]
        public static void CreatePrefab()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            {
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            }

            // Check if already exists
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PATH);
            if (existing != null)
            {
                Debug.Log("[MockDataPrefabCreator] Prefab already exists at " + PREFAB_PATH);
                EditorGUIUtility.PingObject(existing);
                Selection.activeObject = existing;
                return;
            }

            // Build temporary GO
            GameObject go = new GameObject("MockDataSeeder");
            go.AddComponent<MockDataSeeder>();

            // Save as prefab
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, PREFAB_PATH);
            Object.DestroyImmediate(go);

            Debug.Log("[MockDataPrefabCreator] Created prefab at " + PREFAB_PATH);
            EditorGUIUtility.PingObject(prefab);
            Selection.activeObject = prefab;
        }
    }
}
