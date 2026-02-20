// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using HanabiCanvas.Runtime;
using HanabiCanvas.Runtime.GameFlow;
using HanabiCanvas.Runtime.UI;

namespace HanabiCanvas.Tests.PlayMode
{
    public class MainMenuUITests
    {
        // ---- Private Fields ----
        private GameObject _menuGO;
        private MainMenuUI _menu;
        private AppStateVariableSO _appState;
        private BoolVariableSO _isChallengeMode;
        private Button _freeModeButton;

        // ---- Setup / Teardown ----

        [SetUp]
        public void Setup()
        {
            _appState = ScriptableObject.CreateInstance<AppStateVariableSO>();
            _isChallengeMode = ScriptableObject.CreateInstance<BoolVariableSO>();

            _menuGO = new GameObject("TestMainMenuUI");
            _menuGO.SetActive(false);

            GameObject buttonGO = new GameObject("FreeModeButton");
            buttonGO.transform.SetParent(_menuGO.transform);
            _freeModeButton = buttonGO.AddComponent<Button>();

            _menu = _menuGO.AddComponent<MainMenuUI>();

#if UNITY_EDITOR
            var so = new UnityEditor.SerializedObject(_menu);
            so.FindProperty("_appState").objectReferenceValue = _appState;
            so.FindProperty("_isChallengeMode").objectReferenceValue = _isChallengeMode;
            so.FindProperty("_freeModeButton").objectReferenceValue = _freeModeButton;
            so.ApplyModifiedPropertiesWithoutUndo();
#endif

            _appState.Value = AppState.Menu;
            _menuGO.SetActive(true);
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_menuGO);
            Object.DestroyImmediate(_appState);
            Object.DestroyImmediate(_isChallengeMode);
        }

        // ---- Tests ----

        [UnityTest]
        public IEnumerator AppStateMenu_MenuIsVisible()
        {
            yield return null;

            Assert.IsTrue(_menuGO.activeSelf);
        }

        [UnityTest]
        public IEnumerator AppStatePlaying_MenuIsHidden()
        {
            yield return null;

            _appState.Value = AppState.Playing;
            yield return null;

            Assert.IsFalse(_menuGO.activeSelf);
        }

        [UnityTest]
        public IEnumerator FreeModeButton_SetsAppStatePlaying()
        {
            yield return null;

            _freeModeButton.onClick.Invoke();

            Assert.AreEqual(AppState.Playing, _appState.Value);
            Assert.IsFalse(_isChallengeMode.Value);
        }
    }
}
