// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using HanabiCanvas.Runtime;
using HanabiCanvas.Runtime.Events;
using HanabiCanvas.Runtime.Firework;
using HanabiCanvas.Runtime.GameFlow;

namespace HanabiCanvas.Editor
{
    public class DrawingScreenWizard : EditorWindow
    {
        // ---- Constants ----
        private const string COLOR_PALETTE_PATH = "Assets/Data/Palettes/Default Color Palette.asset";
        private const string CURRENT_COLOR_PATH = "Assets/Data/Config/CurrentColor.asset";
        private const string SAVE_EVENT_PATH = "Assets/Data/Config/OnSavePattern.asset";
        private const string CLEAR_EVENT_PATH = "Assets/Data/Config/OnCanvasCleared.asset";
        private const string PATTERN_LIBRARY_PATH = "Assets/Data/Config/Pattern Library.asset";
        private const string CANVAS_CONFIG_PATH = "Assets/Data/Config/New Canvas Config.asset";
        private const string COLOR_BUTTON_PREFAB_PATH = "Assets/Prefabs/ColorButton.prefab";
        private const string LAUNCH_PATTERN_EVENT_PATH = "Assets/Data/Config/OnLaunchPattern.asset";
        private const string SELECTED_PATTERN_INDEX_PATH = "Assets/Data/Config/Selected Pattern Index.asset";
        private const string FIREWORK_REQUESTED_EVENT_PATH = "Assets/Data/Fireworks/On Firework Requested.asset";
        private const string IS_FIREWORK_PLAYING_PATH = "Assets/Data/Config/Is Firework Playing.asset";
        private const string THUMBNAIL_PREFAB_PATH = "Assets/Prefabs/PatternThumbnail.prefab";

        // ---- Private Fields ----
        private string _statusMessage = "";

        // ---- Menu Item ----
        [MenuItem("Tools/Hanabi Canvas/Drawing Screen Setup")]
        private static void ShowWindow()
        {
            DrawingScreenWizard window = GetWindow<DrawingScreenWizard>("Drawing Screen Setup");
            window.minSize = new Vector2(400, 250);
            window.Show();
        }

        // ---- Unity Methods ----
        private void OnGUI()
        {
            EditorGUILayout.LabelField("Hanabi Canvas — Drawing Screen Setup", EditorStyles.boldLabel);
            EditorGUILayout.Space(8);
            EditorGUILayout.HelpBox(
                "Creates the full drawing screen UI hierarchy in the active scene:\n" +
                "• DrawingScreen Canvas with CanvasScaler (1920x1080)\n" +
                "• Background, DrawingPanel (with GridBoard + GridOverlay + DrawingCanvas)\n" +
                "• PalettePanel (with PaletteUI + VerticalLayoutGroup)\n" +
                "• Save and Clear buttons\n" +
                "• ColorButton prefab (if missing)\n" +
                "• All required SO assets wired automatically",
                MessageType.Info);
            EditorGUILayout.Space(8);

            if (GUILayout.Button("Create Drawing Screen", GUILayout.Height(32)))
            {
                CreateDrawingScreen();
            }

            if (!string.IsNullOrEmpty(_statusMessage))
            {
                EditorGUILayout.Space(8);
                EditorGUILayout.HelpBox(_statusMessage, MessageType.Info);
            }
        }

        // ---- Private Methods ----
        private void CreateDrawingScreen()
        {
            EnsureDirectory("Assets/Data");
            EnsureDirectory("Assets/Data/Palettes");
            EnsureDirectory("Assets/Data/Config");
            EnsureDirectory("Assets/Prefabs");

            // Load or create SO assets
            ColorPaletteSO palette = LoadOrCreate<ColorPaletteSO>(COLOR_PALETTE_PATH);
            ColorVariableSO currentColor = LoadOrCreate<ColorVariableSO>(CURRENT_COLOR_PATH);
            GameEventSO saveEvent = LoadOrCreate<GameEventSO>(SAVE_EVENT_PATH);
            GameEventSO clearEvent = LoadOrCreate<GameEventSO>(CLEAR_EVENT_PATH);
            PatternListSO patternLibrary = LoadOrCreate<PatternListSO>(PATTERN_LIBRARY_PATH);
            DrawingCanvasConfigSO canvasConfig = LoadOrCreate<DrawingCanvasConfigSO>(CANVAS_CONFIG_PATH);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Reload from disk so references are persistent
            palette = AssetDatabase.LoadAssetAtPath<ColorPaletteSO>(COLOR_PALETTE_PATH);
            currentColor = AssetDatabase.LoadAssetAtPath<ColorVariableSO>(CURRENT_COLOR_PATH);
            saveEvent = AssetDatabase.LoadAssetAtPath<GameEventSO>(SAVE_EVENT_PATH);
            clearEvent = AssetDatabase.LoadAssetAtPath<GameEventSO>(CLEAR_EVENT_PATH);
            patternLibrary = AssetDatabase.LoadAssetAtPath<PatternListSO>(PATTERN_LIBRARY_PATH);
            canvasConfig = AssetDatabase.LoadAssetAtPath<DrawingCanvasConfigSO>(CANVAS_CONFIG_PATH);

            // Create ColorButton prefab if missing
            GameObject colorButtonPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(COLOR_BUTTON_PREFAB_PATH);
            if (colorButtonPrefab == null)
            {
                colorButtonPrefab = CreateColorButtonPrefab();
            }

            // ---- Build the UI hierarchy ----

            // Root: DrawingScreen Canvas
            GameObject drawingScreenObj = new GameObject("DrawingScreen");
            Undo.RegisterCreatedObjectUndo(drawingScreenObj, "Create Drawing Screen");

            Canvas canvas = drawingScreenObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = drawingScreenObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            drawingScreenObj.AddComponent<GraphicRaycaster>();

            RectTransform rootRect = drawingScreenObj.GetComponent<RectTransform>();

            // Background
            GameObject backgroundObj = CreateUIChild("Background", drawingScreenObj);
            Image backgroundImage = backgroundObj.AddComponent<Image>();
            backgroundImage.color = new Color(0.1f, 0.1f, 0.15f, 1f);
            StretchFull(backgroundObj.GetComponent<RectTransform>());

            // DrawingPanel (center area)
            GameObject drawingPanelObj = CreateUIChild("DrawingPanel", drawingScreenObj);
            RectTransform drawingPanelRect = drawingPanelObj.GetComponent<RectTransform>();
            drawingPanelRect.anchorMin = new Vector2(0.5f, 0.5f);
            drawingPanelRect.anchorMax = new Vector2(0.5f, 0.5f);
            drawingPanelRect.pivot = new Vector2(0.5f, 0.5f);
            drawingPanelRect.sizeDelta = new Vector2(600f, 600f);
            drawingPanelRect.anchoredPosition = new Vector2(-80f, 0f);

            // CanvasBackground (behind the grid)
            GameObject canvasBgObj = CreateUIChild("CanvasBackground", drawingPanelObj);
            Image canvasBgImage = canvasBgObj.AddComponent<Image>();
            canvasBgImage.color = new Color(0.15f, 0.15f, 0.2f, 1f);
            StretchFull(canvasBgObj.GetComponent<RectTransform>());

            // GridBoard (RawImage for the drawing texture)
            GameObject gridBoardObj = CreateUIChild("GridBoard", drawingPanelObj);
            RawImage gridBoard = gridBoardObj.AddComponent<RawImage>();
            gridBoard.color = Color.white;
            StretchFull(gridBoardObj.GetComponent<RectTransform>());

            // GridOverlay (RawImage for grid lines, on top)
            GameObject gridOverlayObj = CreateUIChild("GridOverlay", drawingPanelObj);
            RawImage gridOverlay = gridOverlayObj.AddComponent<RawImage>();
            gridOverlay.color = Color.white;
            gridOverlay.raycastTarget = false;
            StretchFull(gridOverlayObj.GetComponent<RectTransform>());

            // Add DrawingCanvas MonoBehaviour on DrawingPanel
            DrawingCanvas drawingCanvas = drawingPanelObj.AddComponent<DrawingCanvas>();
            SerializedObject drawingCanvasSO = new SerializedObject(drawingCanvas);
            drawingCanvasSO.FindProperty("_config").objectReferenceValue = canvasConfig;
            drawingCanvasSO.FindProperty("_currentColor").objectReferenceValue = currentColor;
            drawingCanvasSO.FindProperty("_gridBoard").objectReferenceValue = gridBoard;
            drawingCanvasSO.FindProperty("_gridOverylay").objectReferenceValue = gridOverlay;
            drawingCanvasSO.FindProperty("_patternLibrary").objectReferenceValue = patternLibrary;
            drawingCanvasSO.FindProperty("_onSavePattern").objectReferenceValue = saveEvent;
            drawingCanvasSO.FindProperty("_onCanvasCleared").objectReferenceValue = clearEvent;
            drawingCanvasSO.ApplyModifiedPropertiesWithoutUndo();

            // PalettePanel (right side)
            GameObject palettePanelObj = CreateUIChild("PalettePanel", drawingScreenObj);
            RectTransform palettePanelRect = palettePanelObj.GetComponent<RectTransform>();
            palettePanelRect.anchorMin = new Vector2(1f, 0.5f);
            palettePanelRect.anchorMax = new Vector2(1f, 0.5f);
            palettePanelRect.pivot = new Vector2(1f, 0.5f);
            palettePanelRect.sizeDelta = new Vector2(60f, 500f);
            palettePanelRect.anchoredPosition = new Vector2(-40f, 0f);

            VerticalLayoutGroup vlg = palettePanelObj.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 8f;
            vlg.padding = new RectOffset(4, 4, 4, 4);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = false;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = false;
            vlg.childForceExpandHeight = false;

            // Add PaletteUI MonoBehaviour on PalettePanel
            PaletteUI paletteUI = palettePanelObj.AddComponent<PaletteUI>();
            SerializedObject paletteUISO = new SerializedObject(paletteUI);
            paletteUISO.FindProperty("_palette").objectReferenceValue = palette;
            paletteUISO.FindProperty("_currentColor").objectReferenceValue = currentColor;
            paletteUISO.FindProperty("_colorButtonContainer").objectReferenceValue = palettePanelRect;
            paletteUISO.FindProperty("_colorButtonPrefab").objectReferenceValue = colorButtonPrefab;
            paletteUISO.ApplyModifiedPropertiesWithoutUndo();

            // SaveButton
            GameObject saveButtonObj = CreateButtonChild("SaveButton", "Save", drawingScreenObj);
            RectTransform saveButtonRect = saveButtonObj.GetComponent<RectTransform>();
            saveButtonRect.anchorMin = new Vector2(0.5f, 0f);
            saveButtonRect.anchorMax = new Vector2(0.5f, 0f);
            saveButtonRect.pivot = new Vector2(0.5f, 0f);
            saveButtonRect.sizeDelta = new Vector2(140f, 40f);
            saveButtonRect.anchoredPosition = new Vector2(-160f, 100f);

            SavePatternButton savePatternButton = saveButtonObj.AddComponent<SavePatternButton>();
            SerializedObject savePatternSO = new SerializedObject(savePatternButton);
            savePatternSO.FindProperty("_onSavePattern").objectReferenceValue = saveEvent;
            savePatternSO.ApplyModifiedPropertiesWithoutUndo();

            // ClearButton
            GameObject clearButtonObj = CreateButtonChild("ClearButton", "Clear", drawingScreenObj);
            RectTransform clearButtonRect = clearButtonObj.GetComponent<RectTransform>();
            clearButtonRect.anchorMin = new Vector2(0.5f, 0f);
            clearButtonRect.anchorMax = new Vector2(0.5f, 0f);
            clearButtonRect.pivot = new Vector2(0.5f, 0f);
            clearButtonRect.sizeDelta = new Vector2(140f, 40f);
            clearButtonRect.anchoredPosition = new Vector2(0f, 100f);

            ClearCanvasButton clearCanvasButton = clearButtonObj.AddComponent<ClearCanvasButton>();
            SerializedObject clearCanvasSO = new SerializedObject(clearCanvasButton);
            clearCanvasSO.FindProperty("_onCanvasCleared").objectReferenceValue = clearEvent;
            clearCanvasSO.ApplyModifiedPropertiesWithoutUndo();

            // Load or create gallery/launch SOs
            GameEventSO launchPatternEvent = LoadOrCreate<GameEventSO>(LAUNCH_PATTERN_EVENT_PATH);
            IntVariableSO selectedPatternIndex = LoadOrCreate<IntVariableSO>(SELECTED_PATTERN_INDEX_PATH);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            launchPatternEvent = AssetDatabase.LoadAssetAtPath<GameEventSO>(LAUNCH_PATTERN_EVENT_PATH);
            selectedPatternIndex = AssetDatabase.LoadAssetAtPath<IntVariableSO>(SELECTED_PATTERN_INDEX_PATH);

            FireworkRequestEventSO fireworkRequestEvent =
                AssetDatabase.LoadAssetAtPath<FireworkRequestEventSO>(FIREWORK_REQUESTED_EVENT_PATH);
            BoolVariableSO isFireworkPlaying =
                AssetDatabase.LoadAssetAtPath<BoolVariableSO>(IS_FIREWORK_PLAYING_PATH);

            // Create PatternThumbnail prefab if missing
            GameObject thumbnailPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(THUMBNAIL_PREFAB_PATH);
            if (thumbnailPrefab == null)
            {
                thumbnailPrefab = CreateThumbnailPrefab();
            }

            // GalleryPanel (bottom of canvas)
            GameObject galleryPanelObj = CreateUIChild("GalleryPanel", drawingScreenObj);
            RectTransform galleryPanelRect = galleryPanelObj.GetComponent<RectTransform>();
            galleryPanelRect.anchorMin = new Vector2(0f, 0f);
            galleryPanelRect.anchorMax = new Vector2(1f, 0f);
            galleryPanelRect.pivot = new Vector2(0.5f, 0f);
            galleryPanelRect.sizeDelta = new Vector2(0f, 80f);
            galleryPanelRect.anchoredPosition = Vector2.zero;

            // Viewport (child of GalleryPanel with Mask)
            GameObject viewportObj = CreateUIChild("Viewport", galleryPanelObj);
            Image viewportImage = viewportObj.AddComponent<Image>();
            viewportImage.color = new Color(0.08f, 0.08f, 0.12f, 1f);
            viewportObj.AddComponent<Mask>().showMaskGraphic = true;
            StretchFull(viewportObj.GetComponent<RectTransform>());

            // ThumbnailContainer (child of Viewport with HorizontalLayoutGroup)
            GameObject thumbnailContainerObj = CreateUIChild("ThumbnailContainer", viewportObj);
            RectTransform thumbnailContainerRect = thumbnailContainerObj.GetComponent<RectTransform>();
            thumbnailContainerRect.anchorMin = new Vector2(0f, 0f);
            thumbnailContainerRect.anchorMax = new Vector2(0f, 1f);
            thumbnailContainerRect.pivot = new Vector2(0f, 0.5f);
            thumbnailContainerRect.sizeDelta = new Vector2(0f, 0f);
            thumbnailContainerRect.anchoredPosition = Vector2.zero;

            ContentSizeFitter csf = thumbnailContainerObj.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            csf.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

            HorizontalLayoutGroup hlg = thumbnailContainerObj.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 8f;
            hlg.padding = new RectOffset(4, 4, 4, 4);
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            // ScrollRect on GalleryPanel
            ScrollRect scrollRect = galleryPanelObj.AddComponent<ScrollRect>();
            scrollRect.horizontal = true;
            scrollRect.vertical = false;
            scrollRect.viewport = viewportObj.GetComponent<RectTransform>();
            scrollRect.content = thumbnailContainerRect;

            // Wire PatternGalleryUI on GalleryPanel
            PatternGalleryUI galleryUI = galleryPanelObj.AddComponent<PatternGalleryUI>();
            SerializedObject galleryUISO = new SerializedObject(galleryUI);
            galleryUISO.FindProperty("_patternLibrary").objectReferenceValue = patternLibrary;
            galleryUISO.FindProperty("_selectedPatternIndex").objectReferenceValue = selectedPatternIndex;
            galleryUISO.FindProperty("_onLaunchPattern").objectReferenceValue = launchPatternEvent;
            galleryUISO.FindProperty("_thumbnailContainer").objectReferenceValue = thumbnailContainerRect;
            galleryUISO.FindProperty("_thumbnailPrefab").objectReferenceValue = thumbnailPrefab;
            galleryUISO.ApplyModifiedPropertiesWithoutUndo();

            // LaunchManagerHolder (NOT under Canvas — world-space Transform)
            GameObject launchManagerObj = new GameObject("LaunchManagerHolder");
            Undo.RegisterCreatedObjectUndo(launchManagerObj, "Create LaunchManagerHolder");

            GameObject spawnPointObj = new GameObject("FireworkSpawnPoint");
            spawnPointObj.transform.SetParent(launchManagerObj.transform, false);
            spawnPointObj.transform.localPosition = new Vector3(0f, 10f, 0f);

            PatternLaunchManager launchManager = launchManagerObj.AddComponent<PatternLaunchManager>();
            SerializedObject launchManagerSO = new SerializedObject(launchManager);
            launchManagerSO.FindProperty("_patternLibrary").objectReferenceValue = patternLibrary;
            launchManagerSO.FindProperty("_selectedPatternIndex").objectReferenceValue = selectedPatternIndex;
            launchManagerSO.FindProperty("_onLaunchPattern").objectReferenceValue = launchPatternEvent;
            launchManagerSO.FindProperty("_onFireworkRequested").objectReferenceValue = fireworkRequestEvent;
            launchManagerSO.FindProperty("_isFireworkPlaying").objectReferenceValue = isFireworkPlaying;
            launchManagerSO.FindProperty("_fireworkSpawnPoint").objectReferenceValue = spawnPointObj.transform;
            launchManagerSO.FindProperty("_drawingScreen").objectReferenceValue = drawingScreenObj;
            launchManagerSO.ApplyModifiedPropertiesWithoutUndo();

            Debug.Log("[DrawingScreenWizard] Note: Wire CameraController on PatternLaunchManager manually for camera transitions.");

            // Ensure there's an EventSystem in the scene
            if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystemObj = new GameObject("EventSystem");
                Undo.RegisterCreatedObjectUndo(eventSystemObj, "Create EventSystem");
                eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());

            _statusMessage = "Drawing screen created successfully. Save the scene to persist changes.";
            Debug.Log("[DrawingScreenWizard] Drawing screen hierarchy created in active scene.");
        }

        private T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
            {
                return asset;
            }

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            Debug.Log($"[DrawingScreenWizard] Created asset: {path}");
            return asset;
        }

        private GameObject CreateColorButtonPrefab()
        {
            GameObject buttonObj = new GameObject("ColorButton");

            RectTransform rect = buttonObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(40f, 40f);

            Image image = buttonObj.AddComponent<Image>();
            image.color = Color.white;

            buttonObj.AddComponent<Button>();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(buttonObj, COLOR_BUTTON_PREFAB_PATH);
            Object.DestroyImmediate(buttonObj);

            Debug.Log($"[DrawingScreenWizard] Created prefab: {COLOR_BUTTON_PREFAB_PATH}");
            return prefab;
        }

        private GameObject CreateThumbnailPrefab()
        {
            GameObject thumbnailObj = new GameObject("PatternThumbnail");

            RectTransform rect = thumbnailObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(64f, 64f);

            thumbnailObj.AddComponent<RawImage>();
            thumbnailObj.AddComponent<Button>();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(thumbnailObj, THUMBNAIL_PREFAB_PATH);
            Object.DestroyImmediate(thumbnailObj);

            Debug.Log($"[DrawingScreenWizard] Created prefab: {THUMBNAIL_PREFAB_PATH}");
            return prefab;
        }

        private GameObject CreateUIChild(string name, GameObject parent)
        {
            GameObject child = new GameObject(name, typeof(RectTransform));
            child.transform.SetParent(parent.transform, false);
            return child;
        }

        private GameObject CreateButtonChild(string name, string label, GameObject parent)
        {
            GameObject buttonObj = new GameObject(name, typeof(RectTransform));
            buttonObj.transform.SetParent(parent.transform, false);

            Image image = buttonObj.AddComponent<Image>();
            image.color = new Color(0.25f, 0.25f, 0.3f, 1f);

            buttonObj.AddComponent<Button>();

            // Text child
            GameObject textObj = new GameObject("Text", typeof(RectTransform));
            textObj.transform.SetParent(buttonObj.transform, false);

            Text text = textObj.AddComponent<Text>();
            text.text = label;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 18;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;

            StretchFull(textObj.GetComponent<RectTransform>());

            return buttonObj;
        }

        private void StretchFull(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
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
