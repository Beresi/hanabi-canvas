// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using HanabiCanvas.Runtime;
using HanabiCanvas.Runtime.Canvas;
using HanabiCanvas.Runtime.Events;
using HanabiCanvas.Runtime.UI;

namespace HanabiCanvas.Editor
{
    public class CanvasPrefabWizard : EditorWindow
    {
        // ---- Constants ----
        private const string PREFAB_FOLDER = "Assets/Prefabs";
        private const string MATERIAL_FOLDER = "Assets/Art/Materials";
        private const string SHADER_NAME = "HanabiCanvas/PixelGrid";
        private const string CANVAS_PREFAB_PATH = "Assets/Prefabs/PixelCanvas.prefab";
        private const string PALETTE_UI_PREFAB_PATH = "Assets/Prefabs/PaletteUI.prefab";
        private const string TOOLBAR_PREFAB_PATH = "Assets/Prefabs/CanvasToolbar.prefab";
        private const string COLOR_BUTTON_PREFAB_PATH = "Assets/Prefabs/ColorButton.prefab";
        private const string MATERIAL_PATH = "Assets/Art/Materials/PixelGrid.mat";
        private const string OUTPUT_PIXEL_DATA_PATH = "Assets/Data/Config/Output Pixel Data.asset";
        private const string ACTIVE_COLOR_INDEX_PATH = "Assets/Data/Config/Active Color Index.asset";
        private const string ACTIVE_TOOL_INDEX_PATH = "Assets/Data/Config/Active Tool Index.asset";

        // ---- Serialized Fields ----
        private CanvasConfigSO _canvasConfig;
        private ColorPaletteSO _colorPalette;
        private GameEventSO _onLaunchFirework;
        private GameEventSO _onCanvasCleared;

        // ---- Private Fields ----
        private Vector2 _scrollPosition;
        private string _statusMessage = "";
        private int _createdCount;

        // ---- Menu Item ----
        [MenuItem("Tools/Hanabi Canvas/Canvas Prefab Builder")]
        private static void ShowWindow()
        {
            CanvasPrefabWizard window = GetWindow<CanvasPrefabWizard>("Canvas Prefab Builder");
            window.minSize = new Vector2(450, 400);
            window.Show();
        }

        // ---- Unity Methods ----
        private void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            EditorGUILayout.LabelField("Hanabi Canvas — Canvas Prefab Builder", EditorStyles.boldLabel);
            EditorGUILayout.Space(8);
            EditorGUILayout.HelpBox(
                "Creates prefabs for the Pixel Canvas system:\n" +
                "• PixelCanvas prefab (grid + renderer + quad)\n" +
                "• PaletteUI prefab (color selection buttons)\n" +
                "• CanvasToolbar prefab (tool + action buttons)\n" +
                "• Auto-generated PixelGrid material",
                MessageType.Info);
            EditorGUILayout.Space(8);

            EditorGUILayout.LabelField("Input Assets", EditorStyles.boldLabel);
            _canvasConfig = (CanvasConfigSO)EditorGUILayout.ObjectField(
                "Canvas Config", _canvasConfig, typeof(CanvasConfigSO), false);
            _colorPalette = (ColorPaletteSO)EditorGUILayout.ObjectField(
                "Color Palette", _colorPalette, typeof(ColorPaletteSO), false);
            _onLaunchFirework = (GameEventSO)EditorGUILayout.ObjectField(
                "OnLaunchFirework Event", _onLaunchFirework, typeof(GameEventSO), false);
            _onCanvasCleared = (GameEventSO)EditorGUILayout.ObjectField(
                "OnCanvasCleared Event", _onCanvasCleared, typeof(GameEventSO), false);

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
            if (_canvasConfig == null)
            {
                _statusMessage = "Please assign a CanvasConfigSO.";
                return false;
            }

            if (_colorPalette == null)
            {
                _statusMessage = "Please assign a ColorPaletteSO.";
                return false;
            }

            if (_onLaunchFirework == null)
            {
                _statusMessage = "Please assign the OnLaunchFirework event.";
                return false;
            }

            if (_onCanvasCleared == null)
            {
                _statusMessage = "Please assign the OnCanvasCleared event.";
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
                    "Some canvas prefabs already exist. Do you want to overwrite them?",
                    "Overwrite",
                    "Cancel");

                if (!shouldOverwrite)
                {
                    _statusMessage = "Operation cancelled.";
                    return;
                }
            }

            EnsureDirectoriesExist();

            PixelDataSO outputData = CreateOutputPixelData();
            IntVariableSO activeColorIndex = CreateActiveColorIndex();
            IntVariableSO activeToolIndex = CreateActiveToolIndex();
            Material gridMaterial = CreatePixelGridMaterial();
            GameObject colorButtonPrefab = CreateColorButtonPrefab();
            CreateCanvasPrefab(outputData, gridMaterial, activeColorIndex, activeToolIndex);
            CreatePaletteUIPrefab(colorButtonPrefab, activeColorIndex);
            CreateToolbarPrefab(activeToolIndex);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            _statusMessage = $"Done! Created {_createdCount} assets. Check the Prefabs/ folder.";
            Debug.Log($"[CanvasPrefabWizard] Complete. Created {_createdCount} assets.");
        }

        private bool HasExistingPrefabs()
        {
            return AssetDatabase.LoadAssetAtPath<Object>(CANVAS_PREFAB_PATH) != null ||
                   AssetDatabase.LoadAssetAtPath<Object>(PALETTE_UI_PREFAB_PATH) != null ||
                   AssetDatabase.LoadAssetAtPath<Object>(TOOLBAR_PREFAB_PATH) != null;
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

        private PixelDataSO CreateOutputPixelData()
        {
            PixelDataSO existing = AssetDatabase.LoadAssetAtPath<PixelDataSO>(OUTPUT_PIXEL_DATA_PATH);
            if (existing != null)
            {
                return existing;
            }

            PixelDataSO pixelData = CreateInstance<PixelDataSO>();
            SerializedObject serialized = new SerializedObject(pixelData);
            serialized.FindProperty("_width").intValue = _canvasConfig.GridWidth;
            serialized.FindProperty("_height").intValue = _canvasConfig.GridHeight;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.CreateAsset(pixelData, OUTPUT_PIXEL_DATA_PATH);
            LogCreated(OUTPUT_PIXEL_DATA_PATH);
            return pixelData;
        }

        private IntVariableSO CreateActiveColorIndex()
        {
            IntVariableSO existing = AssetDatabase.LoadAssetAtPath<IntVariableSO>(ACTIVE_COLOR_INDEX_PATH);
            if (existing != null)
            {
                return existing;
            }

            IntVariableSO variable = CreateInstance<IntVariableSO>();
            variable.name = "Active Color Index";
            AssetDatabase.CreateAsset(variable, ACTIVE_COLOR_INDEX_PATH);
            LogCreated(ACTIVE_COLOR_INDEX_PATH);
            return variable;
        }

        private IntVariableSO CreateActiveToolIndex()
        {
            IntVariableSO existing = AssetDatabase.LoadAssetAtPath<IntVariableSO>(ACTIVE_TOOL_INDEX_PATH);
            if (existing != null)
            {
                return existing;
            }

            IntVariableSO variable = CreateInstance<IntVariableSO>();
            variable.name = "Active Tool Index";
            AssetDatabase.CreateAsset(variable, ACTIVE_TOOL_INDEX_PATH);
            LogCreated(ACTIVE_TOOL_INDEX_PATH);
            return variable;
        }

        private Material CreatePixelGridMaterial()
        {
            Material existing = AssetDatabase.LoadAssetAtPath<Material>(MATERIAL_PATH);
            if (existing != null)
            {
                return existing;
            }

            Shader shader = Shader.Find(SHADER_NAME);
            if (shader == null)
            {
                Debug.LogWarning($"[CanvasPrefabWizard] Shader '{SHADER_NAME}' not found. " +
                    "Using URP/Unlit fallback.");
                shader = Shader.Find("Universal Render Pipeline/Unlit");
            }

            Material material = new Material(shader);
            material.name = "PixelGrid";

            AssetDatabase.CreateAsset(material, MATERIAL_PATH);
            LogCreated(MATERIAL_PATH);
            return material;
        }

        private void CreateCanvasPrefab(PixelDataSO outputData, Material gridMaterial,
            IntVariableSO activeColorIndex, IntVariableSO activeToolIndex)
        {
            GameObject root = new GameObject("PixelCanvas");

            PixelCanvas pixelCanvas = root.AddComponent<PixelCanvas>();
            SerializedObject canvasSO = new SerializedObject(pixelCanvas);
            canvasSO.FindProperty("_config").objectReferenceValue = _canvasConfig;
            canvasSO.FindProperty("_outputData").objectReferenceValue = outputData;
            canvasSO.FindProperty("_activeColorIndex").objectReferenceValue = activeColorIndex;
            canvasSO.FindProperty("_activeToolIndex").objectReferenceValue = activeToolIndex;
            canvasSO.FindProperty("_onCanvasCleared").objectReferenceValue = _onCanvasCleared;
            canvasSO.FindProperty("_onLaunchFirework").objectReferenceValue = _onLaunchFirework;
            canvasSO.ApplyModifiedPropertiesWithoutUndo();

            GameObject gridQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            gridQuad.name = "Grid";
            gridQuad.transform.SetParent(root.transform);
            gridQuad.transform.localPosition = Vector3.zero;
            gridQuad.transform.localRotation = Quaternion.identity;

            Object.DestroyImmediate(gridQuad.GetComponent<Collider>());

            MeshRenderer meshRenderer = gridQuad.GetComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = gridMaterial;

            PixelGridRenderer gridRenderer = gridQuad.AddComponent<PixelGridRenderer>();
            SerializedObject rendererSO = new SerializedObject(gridRenderer);
            rendererSO.FindProperty("_pixelCanvas").objectReferenceValue = pixelCanvas;
            rendererSO.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(root, CANVAS_PREFAB_PATH);
            DestroyImmediate(root);
            LogCreated(CANVAS_PREFAB_PATH);
        }

        private GameObject CreateColorButtonPrefab()
        {
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(COLOR_BUTTON_PREFAB_PATH);
            if (existing != null)
            {
                return existing;
            }

            GameObject buttonObj = new GameObject("ColorButton");

            RectTransform rectTransform = buttonObj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(40f, 40f);

            Image image = buttonObj.AddComponent<Image>();
            image.color = Color.white;

            Button button = buttonObj.AddComponent<Button>();
            button.targetGraphic = image;

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(buttonObj, COLOR_BUTTON_PREFAB_PATH);
            DestroyImmediate(buttonObj);
            LogCreated(COLOR_BUTTON_PREFAB_PATH);
            return prefab;
        }

        private void CreatePaletteUIPrefab(GameObject colorButtonPrefab,
            IntVariableSO activeColorIndex)
        {
            GameObject root = new GameObject("PaletteUI");

            RectTransform rectTransform = root.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(360f, 50f);

            HorizontalLayoutGroup layoutGroup = root.AddComponent<HorizontalLayoutGroup>();
            layoutGroup.spacing = 5f;
            layoutGroup.childAlignment = TextAnchor.MiddleCenter;
            layoutGroup.childControlWidth = false;
            layoutGroup.childControlHeight = false;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childForceExpandHeight = false;

            PaletteUI paletteUI = root.AddComponent<PaletteUI>();
            SerializedObject paletteSO = new SerializedObject(paletteUI);
            paletteSO.FindProperty("_palette").objectReferenceValue = _colorPalette;
            paletteSO.FindProperty("_activeColorIndex").objectReferenceValue = activeColorIndex;
            paletteSO.FindProperty("_buttonContainer").objectReferenceValue = rectTransform;

            Button buttonPrefabComponent = colorButtonPrefab.GetComponent<Button>();
            paletteSO.FindProperty("_colorButtonPrefab").objectReferenceValue = buttonPrefabComponent;
            paletteSO.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(root, PALETTE_UI_PREFAB_PATH);
            DestroyImmediate(root);
            LogCreated(PALETTE_UI_PREFAB_PATH);
        }

        private void CreateToolbarPrefab(IntVariableSO activeToolIndex)
        {
            GameObject root = new GameObject("CanvasToolbar");

            RectTransform rootRect = root.AddComponent<RectTransform>();
            rootRect.sizeDelta = new Vector2(300f, 50f);

            HorizontalLayoutGroup layoutGroup = root.AddComponent<HorizontalLayoutGroup>();
            layoutGroup.spacing = 8f;
            layoutGroup.childAlignment = TextAnchor.MiddleCenter;
            layoutGroup.childControlWidth = false;
            layoutGroup.childControlHeight = false;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childForceExpandHeight = false;

            Button drawButton = CreateToolButton(root.transform, "DrawButton", "Draw");
            Button eraseButton = CreateToolButton(root.transform, "EraseButton", "Erase");
            Button fillButton = CreateToolButton(root.transform, "FillButton", "Fill");
            Button clearButton = CreateToolButton(root.transform, "ClearButton", "Clear");
            Button launchButton = CreateToolButton(root.transform, "LaunchButton", "Launch");

            Image launchImage = launchButton.GetComponent<Image>();
            if (launchImage != null)
            {
                launchImage.color = new Color(0.2f, 0.8f, 0.2f, 1f);
            }

            CanvasToolbar toolbar = root.AddComponent<CanvasToolbar>();
            SerializedObject toolbarSO = new SerializedObject(toolbar);
            toolbarSO.FindProperty("_activeToolIndex").objectReferenceValue = activeToolIndex;
            toolbarSO.FindProperty("_onCanvasCleared").objectReferenceValue = _onCanvasCleared;
            toolbarSO.FindProperty("_onLaunchFirework").objectReferenceValue = _onLaunchFirework;
            toolbarSO.FindProperty("_drawButton").objectReferenceValue = drawButton;
            toolbarSO.FindProperty("_eraseButton").objectReferenceValue = eraseButton;
            toolbarSO.FindProperty("_fillButton").objectReferenceValue = fillButton;
            toolbarSO.FindProperty("_clearButton").objectReferenceValue = clearButton;
            toolbarSO.FindProperty("_launchButton").objectReferenceValue = launchButton;
            toolbarSO.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(root, TOOLBAR_PREFAB_PATH);
            DestroyImmediate(root);
            LogCreated(TOOLBAR_PREFAB_PATH);
        }

        private Button CreateToolButton(Transform parent, string objectName, string label)
        {
            GameObject buttonObj = new GameObject(objectName);
            buttonObj.transform.SetParent(parent);

            RectTransform rectTransform = buttonObj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(50f, 40f);

            Image image = buttonObj.AddComponent<Image>();
            image.color = new Color(0.8f, 0.8f, 0.8f, 1f);

            Button button = buttonObj.AddComponent<Button>();
            button.targetGraphic = image;

            GameObject textObj = new GameObject("Label");
            textObj.transform.SetParent(buttonObj.transform);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            Text text = textObj.AddComponent<Text>();
            text.text = label;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 12;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.black;

            return button;
        }

        private void LogCreated(string path)
        {
            _createdCount++;
            Debug.Log($"[CanvasPrefabWizard] Created: {path}");
        }
    }
}
