// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HanabiCanvas.Runtime.UI;

namespace HanabiCanvas.Tests.EditMode
{
    public class ConfirmationDialogTests
    {
        // ---- Private Fields ----
        private GameObject _dialogGO;
        private ConfirmationDialog _dialog;
        private GameObject _panel;
        private TextMeshProUGUI _titleText;
        private TextMeshProUGUI _messageText;
        private Button _confirmButton;
        private Button _cancelButton;

        // ---- Setup / Teardown ----

        [SetUp]
        public void Setup()
        {
            _dialogGO = new GameObject("TestConfirmationDialog");

            _panel = new GameObject("Panel");
            _panel.transform.SetParent(_dialogGO.transform);

            GameObject titleGO = new GameObject("Title");
            titleGO.transform.SetParent(_dialogGO.transform);
            _titleText = titleGO.AddComponent<TextMeshProUGUI>();

            GameObject messageGO = new GameObject("Message");
            messageGO.transform.SetParent(_dialogGO.transform);
            _messageText = messageGO.AddComponent<TextMeshProUGUI>();

            GameObject confirmGO = new GameObject("Confirm");
            confirmGO.transform.SetParent(_dialogGO.transform);
            _confirmButton = confirmGO.AddComponent<Button>();

            GameObject cancelGO = new GameObject("Cancel");
            cancelGO.transform.SetParent(_dialogGO.transform);
            _cancelButton = cancelGO.AddComponent<Button>();

            _dialog = _dialogGO.AddComponent<ConfirmationDialog>();

            // Wire fields via SerializedObject
#if UNITY_EDITOR
            var so = new UnityEditor.SerializedObject(_dialog);
            so.FindProperty("_panel").objectReferenceValue = _panel;
            so.FindProperty("_titleText").objectReferenceValue = _titleText;
            so.FindProperty("_messageText").objectReferenceValue = _messageText;
            so.FindProperty("_confirmButton").objectReferenceValue = _confirmButton;
            so.FindProperty("_cancelButton").objectReferenceValue = _cancelButton;
            so.ApplyModifiedPropertiesWithoutUndo();
#endif
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_dialogGO);
        }

        // ---- Tests ----

        [Test]
        public void Show_SetsTextAndShowsPanel()
        {
            _dialog.Show("Test Title", "Test Message", () => { });

            Assert.AreEqual("Test Title", _titleText.text);
            Assert.AreEqual("Test Message", _messageText.text);
            Assert.IsTrue(_panel.activeSelf);
        }

        [Test]
        public void Hide_DeactivatesPanel()
        {
            _dialog.Show("Title", "Message", () => { });
            _dialog.Hide();

            Assert.IsFalse(_panel.activeSelf);
        }

        [Test]
        public void Show_ConfirmCallback_InvokedOnConfirmClick()
        {
            bool wasConfirmed = false;
            _dialog.Show("Title", "Message", () => wasConfirmed = true);

            _confirmButton.onClick.Invoke();

            Assert.IsTrue(wasConfirmed);
        }

        [Test]
        public void Show_CancelCallback_InvokedOnCancelClick()
        {
            bool wasCancelled = false;
            _dialog.Show("Title", "Message", () => { }, () => wasCancelled = true);

            _cancelButton.onClick.Invoke();

            Assert.IsTrue(wasCancelled);
        }
    }
}
