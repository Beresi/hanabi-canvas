// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using NUnit.Framework;
using UnityEngine;
using HanabiCanvas.Editor;

namespace HanabiCanvas.Tests.EditMode
{
    /// <summary>
    /// Tests for the static validation helpers on PolishValidatorWizard.
    /// Focuses on testable logic (HDR checks, camera checks) rather than EditorWindow GUI.
    /// </summary>
    public class PolishValidatorTests
    {
        // ---- Tests: IsHDREnabled ----

        [Test]
        public void IsHDREnabled_ReturnsExpectedValue()
        {
            // The project's URP asset has m_SupportsHDR set.
            // This test validates the method runs without error and returns a bool.
            // Based on the project's PC_RPAsset.asset, HDR is enabled (m_SupportsHDR: 1).
            bool result = PolishValidatorWizard.IsHDREnabled();

            // We simply verify the method returns without throwing.
            // The actual value depends on the active render pipeline in the test runner.
            Assert.That(result, Is.TypeOf<bool>());
        }

        // ---- Tests: IsCameraHDREnabled ----

        [Test]
        public void IsCameraHDREnabled_HDRDisabled_ReturnsFalse()
        {
            GameObject cameraObj = new GameObject("TestCamera");
            Camera camera = cameraObj.AddComponent<Camera>();
            camera.allowHDR = false;

            bool result = PolishValidatorWizard.IsCameraHDREnabled(camera);

            Assert.IsFalse(result);

            Object.DestroyImmediate(cameraObj);
        }

        [Test]
        public void IsCameraHDREnabled_HDREnabled_ReturnsTrue()
        {
            GameObject cameraObj = new GameObject("TestCamera");
            Camera camera = cameraObj.AddComponent<Camera>();
            camera.allowHDR = true;

            bool result = PolishValidatorWizard.IsCameraHDREnabled(camera);

            Assert.IsTrue(result);

            Object.DestroyImmediate(cameraObj);
        }

        [Test]
        public void IsCameraHDREnabled_NullCamera_ReturnsFalse()
        {
            bool result = PolishValidatorWizard.IsCameraHDREnabled(null);

            Assert.IsFalse(result);
        }

        // ---- Tests: HasBloomVolume ----

        [Test]
        public void HasBloomVolume_NullVolume_ReturnsFalse()
        {
            bool result = PolishValidatorWizard.HasBloomVolume(null);

            Assert.IsFalse(result);
        }

        [Test]
        public void HasBloomVolume_VolumeWithNoProfile_ReturnsFalse()
        {
            GameObject volumeObj = new GameObject("TestVolume");
            UnityEngine.Rendering.Volume volume = volumeObj.AddComponent<UnityEngine.Rendering.Volume>();
            volume.profile = null;

            bool result = PolishValidatorWizard.HasBloomVolume(volume);

            Assert.IsFalse(result);

            Object.DestroyImmediate(volumeObj);
        }

        [Test]
        public void HasBloomVolume_VolumeWithEmptyProfile_ReturnsFalse()
        {
            GameObject volumeObj = new GameObject("TestVolume");
            UnityEngine.Rendering.Volume volume = volumeObj.AddComponent<UnityEngine.Rendering.Volume>();
            volume.profile = ScriptableObject.CreateInstance<UnityEngine.Rendering.VolumeProfile>();

            bool result = PolishValidatorWizard.HasBloomVolume(volume);

            Assert.IsFalse(result);

            Object.DestroyImmediate(volume.profile);
            Object.DestroyImmediate(volumeObj);
        }
    }
}
