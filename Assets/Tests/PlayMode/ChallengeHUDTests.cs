// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using HanabiCanvas.Runtime;
using HanabiCanvas.Runtime.Events;
using HanabiCanvas.Runtime.UI;

namespace HanabiCanvas.Tests.PlayMode
{
    public class ChallengeHUDTests
    {
        // ---- Private Fields ----
        private GameObject _hudGO;
        private ChallengeHUD _hud;
        private BoolVariableSO _isChallengeMode;
        private GameEventSO _onConstraintViolated;
        private Image _warningFlash;

        // ---- Setup / Teardown ----

        [SetUp]
        public void Setup()
        {
            _isChallengeMode = ScriptableObject.CreateInstance<BoolVariableSO>();
            _onConstraintViolated = ScriptableObject.CreateInstance<GameEventSO>();

            _hudGO = new GameObject("TestChallengeHUD");
            _hudGO.SetActive(false);

            GameObject flashGO = new GameObject("WarningFlash");
            flashGO.transform.SetParent(_hudGO.transform);
            _warningFlash = flashGO.AddComponent<Image>();
            flashGO.SetActive(false);

            _hud = _hudGO.AddComponent<ChallengeHUD>();

#if UNITY_EDITOR
            var so = new UnityEditor.SerializedObject(_hud);
            so.FindProperty("_isChallengeMode").objectReferenceValue = _isChallengeMode;
            so.FindProperty("_onConstraintViolated").objectReferenceValue = _onConstraintViolated;
            so.FindProperty("_warningFlash").objectReferenceValue = _warningFlash;
            so.ApplyModifiedPropertiesWithoutUndo();
#endif

            _isChallengeMode.Value = true;
            _hudGO.SetActive(true);
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_hudGO);
            Object.DestroyImmediate(_isChallengeMode);
            Object.DestroyImmediate(_onConstraintViolated);
        }

        // ---- Tests ----

        [UnityTest]
        public IEnumerator ChallengeMode_HUDIsVisible()
        {
            yield return null;

            Assert.IsTrue(_hudGO.activeSelf);
        }

        [UnityTest]
        public IEnumerator ConstraintViolated_FlashesWarning()
        {
            yield return null;

            _onConstraintViolated.Raise();

            Assert.IsTrue(_warningFlash.gameObject.activeSelf);
        }
    }
}
