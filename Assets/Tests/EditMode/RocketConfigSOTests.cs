// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using HanabiCanvas.Runtime;

namespace HanabiCanvas.Tests.EditMode
{
    public class RocketConfigSOTests
    {
        // ---- Private Fields ----
        private RocketConfigSO _config;

        // ---- Setup / Teardown ----
        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<RocketConfigSO>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_config);
        }

        // ---- GetRandomSpawnPosition Tests ----
        [Test]
        public void GetRandomSpawnPosition_EmptyArray_ReturnsFallback()
        {
            SerializedObject so = new SerializedObject(_config);
            SerializedProperty spawnProp = so.FindProperty("_spawnPositions");
            spawnProp.ClearArray();
            so.ApplyModifiedPropertiesWithoutUndo();

            Vector3 result = _config.GetRandomSpawnPosition();

            Assert.AreEqual(0f, result.x, 0.001f);
            Assert.AreEqual(-5f, result.y, 0.001f);
            Assert.AreEqual(0f, result.z, 0.001f);
        }

        [Test]
        public void GetRandomSpawnPosition_SingleElement_ReturnsThatElement()
        {
            Vector3 expected = new Vector3(1f, 2f, 3f);

            SerializedObject so = new SerializedObject(_config);
            SerializedProperty spawnProp = so.FindProperty("_spawnPositions");
            spawnProp.ClearArray();
            spawnProp.InsertArrayElementAtIndex(0);
            spawnProp.GetArrayElementAtIndex(0).vector3Value = expected;
            so.ApplyModifiedPropertiesWithoutUndo();

            Vector3 result = _config.GetRandomSpawnPosition();

            Assert.AreEqual(expected.x, result.x, 0.001f);
            Assert.AreEqual(expected.y, result.y, 0.001f);
            Assert.AreEqual(expected.z, result.z, 0.001f);
        }

        // ---- GetRandomDestinationPosition Tests ----
        [Test]
        public void GetRandomDestinationPosition_EmptyArray_ReturnsFallback()
        {
            SerializedObject so = new SerializedObject(_config);
            SerializedProperty destProp = so.FindProperty("_destinationPositions");
            destProp.ClearArray();
            so.ApplyModifiedPropertiesWithoutUndo();

            Vector3 result = _config.GetRandomDestinationPosition();

            Assert.AreEqual(0f, result.x, 0.001f);
            Assert.AreEqual(10f, result.y, 0.001f);
            Assert.AreEqual(0f, result.z, 0.001f);
        }

        [Test]
        public void GetRandomDestinationPosition_SingleElement_ReturnsThatElement()
        {
            Vector3 expected = new Vector3(5f, 15f, -2f);

            SerializedObject so = new SerializedObject(_config);
            SerializedProperty destProp = so.FindProperty("_destinationPositions");
            destProp.ClearArray();
            destProp.InsertArrayElementAtIndex(0);
            destProp.GetArrayElementAtIndex(0).vector3Value = expected;
            so.ApplyModifiedPropertiesWithoutUndo();

            Vector3 result = _config.GetRandomDestinationPosition();

            Assert.AreEqual(expected.x, result.x, 0.001f);
            Assert.AreEqual(expected.y, result.y, 0.001f);
            Assert.AreEqual(expected.z, result.z, 0.001f);
        }

        // ---- GetRandomPath Tests ----
        [Test]
        public void GetRandomPath_EmptyArray_ReturnsNull()
        {
            SerializedObject so = new SerializedObject(_config);
            SerializedProperty pathProp = so.FindProperty("_pathBehaviours");
            pathProp.ClearArray();
            so.ApplyModifiedPropertiesWithoutUndo();

            RocketPathSO result = _config.GetRandomPath();

            Assert.IsNull(result);
        }

        [Test]
        public void GetRandomPath_SingleElement_ReturnsThatElement()
        {
            StraightRocketPathSO path = ScriptableObject.CreateInstance<StraightRocketPathSO>();

            SerializedObject so = new SerializedObject(_config);
            SerializedProperty pathProp = so.FindProperty("_pathBehaviours");
            pathProp.ClearArray();
            pathProp.InsertArrayElementAtIndex(0);
            pathProp.GetArrayElementAtIndex(0).objectReferenceValue = path;
            so.ApplyModifiedPropertiesWithoutUndo();

            RocketPathSO result = _config.GetRandomPath();

            Assert.AreEqual(path, result);

            Object.DestroyImmediate(path);
        }
    }
}
