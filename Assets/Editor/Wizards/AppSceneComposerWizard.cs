// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;
using HanabiCanvas.Runtime;
using HanabiCanvas.Runtime.Events;
using HanabiCanvas.Runtime.Firework;
using HanabiCanvas.Runtime.GameFlow;
using HanabiCanvas.Runtime.Modes;
using HanabiCanvas.Runtime.Persistence;
using HanabiCanvas.Runtime.UI;
using HanabiCanvas.Runtime.Utils;

namespace HanabiCanvas.Editor
{
    /// <summary>
    /// Editor wizard that creates a complete app scene with all UI screens,
    /// GameFlowController, DataManager, AudioManager, and wires all SO references.
    /// Idempotent — running twice skips existing GameObjects and assets.
    /// </summary>
    public class AppSceneComposerWizard : EditorWindow
    {
        // ---- Constants ----
        private const string MENU_PATH = "Tools/Hanabi Canvas/Compose Full App Scene";
        private const string DATA_CONFIG_PATH = "Assets/Data/Config/";
        private const string DATA_FIREWORKS_PATH = "Assets/Data/Fireworks/";
        private const string PREFABS_PATH = "Assets/Prefabs/";

        // ---- New SO Asset Paths ----
        private const string IS_CHALLENGE_MODE_PATH = DATA_CONFIG_PATH + "Is Challenge Mode.asset";
        private const string SYMMETRY_ENABLED_PATH = DATA_CONFIG_PATH + "Symmetry Enabled.asset";
        private const string SYMMETRY_MODE_PATH = DATA_CONFIG_PATH + "Symmetry Mode.asset";
        private const string REMAINING_TIME_PATH = DATA_CONFIG_PATH + "Remaining Time.asset";
        private const string UNIQUE_COLOR_COUNT_PATH = DATA_CONFIG_PATH + "Unique Color Count.asset";
        private const string FILLED_PIXEL_COUNT_PATH = DATA_CONFIG_PATH + "Filled Pixel Count.asset";
        private const string SLIDESHOW_CURRENT_INDEX_PATH = DATA_CONFIG_PATH + "Slideshow Current Index.asset";
        private const string SLIDESHOW_TOTAL_COUNT_PATH = DATA_CONFIG_PATH + "Slideshow Total Count.asset";
        private const string SELECTED_REQUEST_INDEX_PATH = DATA_CONFIG_PATH + "Selected Request Index.asset";
        private const string MASTER_VOLUME_PATH = DATA_CONFIG_PATH + "Master Volume.asset";

        // ---- Existing SO Asset Paths ----
        private const string APP_STATE_PATH = DATA_CONFIG_PATH + "App State.asset";
        private const string GAME_STATE_PATH = DATA_CONFIG_PATH + "Game State.asset";
        private const string CANVAS_INPUT_ENABLED_PATH = DATA_CONFIG_PATH + "Canvas Input Enabled.asset";
        private const string IS_FIREWORK_PLAYING_PATH = DATA_CONFIG_PATH + "Is Firework Playing.asset";
        private const string CANVAS_CONFIG_PATH = DATA_CONFIG_PATH + "Default Canvas Config.asset";
        private const string CHALLENGE_CONFIG_PATH = DATA_CONFIG_PATH + "Default Challenge Config.asset";
        private const string SLIDESHOW_CONFIG_PATH = DATA_CONFIG_PATH + "Default Slideshow Config.asset";
        private const string AUDIO_CONFIG_PATH = DATA_CONFIG_PATH + "Default Audio Config.asset";
        private const string OUTPUT_PIXEL_DATA_PATH = DATA_CONFIG_PATH + "Output Pixel Data.asset";
        private const string ARTWORK_COUNT_PATH = DATA_CONFIG_PATH + "Artwork Count.asset";
        private const string ACTIVE_REQUEST_COUNT_PATH = DATA_CONFIG_PATH + "Active Request Count.asset";
        private const string FIREWORK_REQUESTED_PATH = DATA_FIREWORKS_PATH + "On Firework Requested.asset";
        private const string SLIDESHOW_ARTWORK_STARTED_PATH = DATA_CONFIG_PATH + "On Slideshow Artwork Started.asset";
        private const string ON_DATA_CHANGED_PATH = DATA_CONFIG_PATH + "On Data Changed.asset";
        private const string ON_FIREWORK_COMPLETE_PATH = DATA_CONFIG_PATH + "On Firework Complete.asset";
        private const string ON_PIXEL_PAINTED_PATH = DATA_CONFIG_PATH + "On Pixel Painted.asset";
        private const string ON_CONSTRAINT_VIOLATED_PATH = DATA_CONFIG_PATH + "On Constraint Violated.asset";
        private const string ON_SLIDESHOW_STARTED_PATH = DATA_CONFIG_PATH + "On Slideshow Started.asset";
        private const string ON_SLIDESHOW_ARTWORK_CHANGED_PATH = DATA_CONFIG_PATH + "On Slideshow Artwork Changed.asset";
        private const string ON_SLIDESHOW_COMPLETE_PATH = DATA_CONFIG_PATH + "On Slideshow Complete.asset";
        private const string ON_SLIDESHOW_EXIT_REQUESTED_PATH = DATA_CONFIG_PATH + "On Slideshow Exit Requested.asset";
        private const string ON_ARTWORK_LIKED_PATH = DATA_CONFIG_PATH + "On Artwork Liked.asset";
        private const string ON_DRAWING_COMPLETE_PATH = DATA_CONFIG_PATH + "On Drawing Complete.asset";
        private const string ON_LAUNCH_FIREWORK_PATH = DATA_CONFIG_PATH + "OnLaunchFirework.asset";
        private const string ON_CANVAS_CLEARED_PATH = DATA_CONFIG_PATH + "OnCanvasCleared.asset";

        // ---- Prefab Paths ----
        private const string REQUEST_CARD_PREFAB_PATH = PREFABS_PATH + "RequestCard.prefab";

        // ---- Private Fields ----
        private Vector2 _scrollPosition;
        private string _logOutput = "";
        private int _createdCount;
        private int _skippedCount;

        // ---- SO References (loaded during compose) ----
        private BoolVariableSO _isChallengeMode;
        private BoolVariableSO _symmetryEnabled;
        private IntVariableSO _symmetryMode;
        private FloatVariableSO _remainingTime;
        private IntVariableSO _uniqueColorCount;
        private IntVariableSO _filledPixelCount;
        private IntVariableSO _slideshowCurrentIndex;
        private IntVariableSO _slideshowTotalCount;
        private IntVariableSO _selectedRequestIndex;
        private FloatVariableSO _masterVolume;
        private AppStateVariableSO _appState;
        private BoolVariableSO _canvasInputEnabled;
        private CanvasConfigSO _canvasConfig;
        private ChallengeConfigSO _challengeConfig;
        private SlideshowConfigSO _slideshowConfig;
        private AudioConfigSO _audioConfig;
        private PixelDataSO _outputPixelData;
        private IntVariableSO _artworkCount;
        private IntVariableSO _activeRequestCount;
        private FireworkRequestEventSO _fireworkRequested;
        private ArtworkEventSO _slideshowArtworkStarted;
        private GameEventSO _onDataChanged;
        private GameEventSO _onFireworkComplete;
        private GameEventSO _onPixelPainted;
        private GameEventSO _onConstraintViolated;
        private GameEventSO _onSlideshowStarted;
        private GameEventSO _onSlideshowArtworkChanged;
        private GameEventSO _onSlideshowComplete;
        private GameEventSO _onSlideshowExitRequested;
        private GameEventSO _onArtworkLiked;
        private GameEventSO _onDrawingComplete;
        private GameEventSO _onLaunchFirework;
        private GameEventSO _onCanvasCleared;

        // ---- Scene References (created during compose) ----
        private DataManager _dataManagerComponent;
        private FreeModeController _freeModeComponent;
        private ChallengeModeController _challengeModeComponent;
        private SlideshowController _slideshowComponent;
        private Transform _fireworkSpawnPoint;

        // ---- Menu Item ----

        /// <summary>Opens the App Scene Composer wizard window.</summary>
        [MenuItem(MENU_PATH)]
        public static void ShowWindow()
        {
            AppSceneComposerWizard window = GetWindow<AppSceneComposerWizard>("App Scene Composer");
            window.minSize = new Vector2(450, 400);
            window.Show();
        }

        // ---- GUI ----

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Hanabi Canvas \u2014 Full App Scene Composer", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "Creates all UI screens, GameFlowController, DataManager, and AudioManager\n" +
                "GameObjects in the current scene. Creates/loads all SO assets.\n" +
                "Wires ALL serialized field references. Existing objects are skipped.",
                MessageType.Info);
            EditorGUILayout.Space();

            if (GUILayout.Button("Compose Scene", GUILayout.Height(32)))
            {
                ComposeScene();
            }

            EditorGUILayout.Space();

            if (!string.IsNullOrEmpty(_logOutput))
            {
                EditorGUILayout.LabelField("Log:", EditorStyles.boldLabel);
                _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(300));
                EditorGUILayout.TextArea(_logOutput, GUILayout.ExpandHeight(true));
                EditorGUILayout.EndScrollView();
            }
        }

        // ---- Scene Composition ----

        private void ComposeScene()
        {
            _logOutput = "";
            _createdCount = 0;
            _skippedCount = 0;

            EnsureDirectory("Assets/Data");
            EnsureDirectory("Assets/Data/Config");
            EnsureDirectory("Assets/Data/Fireworks");
            EnsureDirectory("Assets/Prefabs");

            // Step 1: Load/create all SO assets
            LoadOrCreateAllAssets();

            // Step 2: Create all GameObjects (first pass — no wiring yet)
            Canvas uiCanvas = CreateOrFindUICanvas();
            CreateOrFindDataManager();
            CreateOrFindGameFlowController();
            CreateOrFindAudioManager();

            GameObject mainMenuGO = CreateOrFindUIPanel<MainMenuUI>("MainMenuUI", uiCanvas.transform, false);
            GameObject requestBoardGO = CreateOrFindUIPanel<RequestBoardUI>("RequestBoardUI", uiCanvas.transform, false);
            GameObject settingsGO = CreateOrFindUIPanel<SettingsUI>("SettingsUI", uiCanvas.transform, false);
            GameObject slideshowGO = CreateOrFindUIPanel<SlideshowUI>("SlideshowUI", uiCanvas.transform, false);
            GameObject challengeHUDGO = CreateOrFindUIPanel<ChallengeHUD>("ChallengeHUD", uiCanvas.transform, false);
            GameObject confirmationGO = CreateOrFindUIPanel<ConfirmationDialog>("ConfirmationDialog", uiCanvas.transform, true);

            // Ensure EventSystem exists
            if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystemObj = new GameObject("EventSystem");
                Undo.RegisterCreatedObjectUndo(eventSystemObj, "Create EventSystem");
                eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                Log("Created: EventSystem");
                _createdCount++;
            }

            // Step 3: Wire all references (second pass)
            FireworkSessionManager sessionManager = Object.FindObjectOfType<FireworkSessionManager>();
            if (sessionManager == null)
            {
                Debug.LogWarning("[AppSceneComposerWizard] FireworkSessionManager not found in scene. " +
                    "Mode controllers will have null _sessionManager references.");
                Log("WARNING: FireworkSessionManager not found in scene.");
            }

            WireDataManager();
            WireGameFlowController(sessionManager);
            WireAudioManager();
            WireMainMenuUI(mainMenuGO);
            WireRequestBoardUI(requestBoardGO);
            WireSettingsUI(settingsGO);
            WireSlideshowUI(slideshowGO);
            WireChallengeHUD(challengeHUDGO);
            WireConfirmationDialog(confirmationGO);
            WireDrawingCanvasIfPresent();
            CreateOrFindMockDataSeeder();

            // Summary
            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());

            Log("");
            Log("=== Summary ===");
            Log("Created: " + _createdCount + " objects/assets");
            Log("Skipped: " + _skippedCount + " (already existed)");

            Debug.Log("[AppSceneComposerWizard] Scene composition complete. " +
                "Created: " + _createdCount + ", Skipped: " + _skippedCount);
        }

        // ================================================================
        //  ASSET LOADING
        // ================================================================

        private void LoadOrCreateAllAssets()
        {
            // Create new SO assets (idempotent)
            _isChallengeMode = LoadOrCreate<BoolVariableSO>(IS_CHALLENGE_MODE_PATH);
            _symmetryEnabled = LoadOrCreate<BoolVariableSO>(SYMMETRY_ENABLED_PATH);
            _symmetryMode = LoadOrCreate<IntVariableSO>(SYMMETRY_MODE_PATH);
            _remainingTime = LoadOrCreate<FloatVariableSO>(REMAINING_TIME_PATH);
            _uniqueColorCount = LoadOrCreate<IntVariableSO>(UNIQUE_COLOR_COUNT_PATH);
            _filledPixelCount = LoadOrCreate<IntVariableSO>(FILLED_PIXEL_COUNT_PATH);
            _slideshowCurrentIndex = LoadOrCreate<IntVariableSO>(SLIDESHOW_CURRENT_INDEX_PATH);
            _slideshowTotalCount = LoadOrCreate<IntVariableSO>(SLIDESHOW_TOTAL_COUNT_PATH);
            _selectedRequestIndex = LoadOrCreate<IntVariableSO>(SELECTED_REQUEST_INDEX_PATH);
            _masterVolume = LoadOrCreate<FloatVariableSO>(MASTER_VOLUME_PATH);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Reload new assets from disk for persistent references
            _isChallengeMode = AssetDatabase.LoadAssetAtPath<BoolVariableSO>(IS_CHALLENGE_MODE_PATH);
            _symmetryEnabled = AssetDatabase.LoadAssetAtPath<BoolVariableSO>(SYMMETRY_ENABLED_PATH);
            _symmetryMode = AssetDatabase.LoadAssetAtPath<IntVariableSO>(SYMMETRY_MODE_PATH);
            _remainingTime = AssetDatabase.LoadAssetAtPath<FloatVariableSO>(REMAINING_TIME_PATH);
            _uniqueColorCount = AssetDatabase.LoadAssetAtPath<IntVariableSO>(UNIQUE_COLOR_COUNT_PATH);
            _filledPixelCount = AssetDatabase.LoadAssetAtPath<IntVariableSO>(FILLED_PIXEL_COUNT_PATH);
            _slideshowCurrentIndex = AssetDatabase.LoadAssetAtPath<IntVariableSO>(SLIDESHOW_CURRENT_INDEX_PATH);
            _slideshowTotalCount = AssetDatabase.LoadAssetAtPath<IntVariableSO>(SLIDESHOW_TOTAL_COUNT_PATH);
            _selectedRequestIndex = AssetDatabase.LoadAssetAtPath<IntVariableSO>(SELECTED_REQUEST_INDEX_PATH);
            _masterVolume = AssetDatabase.LoadAssetAtPath<FloatVariableSO>(MASTER_VOLUME_PATH);

            // Load existing SO assets
            _appState = AssetDatabase.LoadAssetAtPath<AppStateVariableSO>(APP_STATE_PATH);
            _canvasInputEnabled = AssetDatabase.LoadAssetAtPath<BoolVariableSO>(CANVAS_INPUT_ENABLED_PATH);
            _canvasConfig = AssetDatabase.LoadAssetAtPath<CanvasConfigSO>(CANVAS_CONFIG_PATH);
            _challengeConfig = AssetDatabase.LoadAssetAtPath<ChallengeConfigSO>(CHALLENGE_CONFIG_PATH);
            _slideshowConfig = AssetDatabase.LoadAssetAtPath<SlideshowConfigSO>(SLIDESHOW_CONFIG_PATH);
            _audioConfig = AssetDatabase.LoadAssetAtPath<AudioConfigSO>(AUDIO_CONFIG_PATH);
            _outputPixelData = AssetDatabase.LoadAssetAtPath<PixelDataSO>(OUTPUT_PIXEL_DATA_PATH);
            _artworkCount = AssetDatabase.LoadAssetAtPath<IntVariableSO>(ARTWORK_COUNT_PATH);
            _activeRequestCount = AssetDatabase.LoadAssetAtPath<IntVariableSO>(ACTIVE_REQUEST_COUNT_PATH);
            _fireworkRequested = AssetDatabase.LoadAssetAtPath<FireworkRequestEventSO>(FIREWORK_REQUESTED_PATH);
            _slideshowArtworkStarted = AssetDatabase.LoadAssetAtPath<ArtworkEventSO>(SLIDESHOW_ARTWORK_STARTED_PATH);
            _onDataChanged = AssetDatabase.LoadAssetAtPath<GameEventSO>(ON_DATA_CHANGED_PATH);
            _onFireworkComplete = AssetDatabase.LoadAssetAtPath<GameEventSO>(ON_FIREWORK_COMPLETE_PATH);
            _onPixelPainted = AssetDatabase.LoadAssetAtPath<GameEventSO>(ON_PIXEL_PAINTED_PATH);
            _onConstraintViolated = AssetDatabase.LoadAssetAtPath<GameEventSO>(ON_CONSTRAINT_VIOLATED_PATH);
            _onSlideshowStarted = AssetDatabase.LoadAssetAtPath<GameEventSO>(ON_SLIDESHOW_STARTED_PATH);
            _onSlideshowArtworkChanged = AssetDatabase.LoadAssetAtPath<GameEventSO>(ON_SLIDESHOW_ARTWORK_CHANGED_PATH);
            _onSlideshowComplete = AssetDatabase.LoadAssetAtPath<GameEventSO>(ON_SLIDESHOW_COMPLETE_PATH);
            _onSlideshowExitRequested = AssetDatabase.LoadAssetAtPath<GameEventSO>(ON_SLIDESHOW_EXIT_REQUESTED_PATH);
            _onArtworkLiked = AssetDatabase.LoadAssetAtPath<GameEventSO>(ON_ARTWORK_LIKED_PATH);
            _onDrawingComplete = AssetDatabase.LoadAssetAtPath<GameEventSO>(ON_DRAWING_COMPLETE_PATH);
            _onLaunchFirework = AssetDatabase.LoadAssetAtPath<GameEventSO>(ON_LAUNCH_FIREWORK_PATH);
            _onCanvasCleared = AssetDatabase.LoadAssetAtPath<GameEventSO>(ON_CANVAS_CLEARED_PATH);

            Log("All SO assets loaded/created.");
        }

        // ================================================================
        //  GAMEOBJECT CREATION
        // ================================================================

        private Canvas CreateOrFindUICanvas()
        {
            Canvas uiCanvas = Object.FindObjectOfType<Canvas>();
            if (uiCanvas != null)
            {
                Log("Skipped: UI Canvas (already exists)");
                _skippedCount++;
                return uiCanvas;
            }

            GameObject canvasGO = new GameObject("UI Canvas");
            Undo.RegisterCreatedObjectUndo(canvasGO, "Create UI Canvas");

            uiCanvas = canvasGO.AddComponent<Canvas>();
            uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();

            Log("Created: UI Canvas");
            _createdCount++;
            return uiCanvas;
        }

        private void CreateOrFindDataManager()
        {
            _dataManagerComponent = Object.FindObjectOfType<DataManager>();
            if (_dataManagerComponent != null)
            {
                Log("Skipped: DataManager (already exists)");
                _skippedCount++;
                return;
            }

            GameObject go = new GameObject("DataManager");
            Undo.RegisterCreatedObjectUndo(go, "Create DataManager");
            _dataManagerComponent = go.AddComponent<DataManager>();

            Log("Created: DataManager");
            _createdCount++;
        }

        private void CreateOrFindGameFlowController()
        {
            GameFlowController existing = Object.FindObjectOfType<GameFlowController>();
            if (existing != null)
            {
                _freeModeComponent = Object.FindObjectOfType<FreeModeController>();
                _challengeModeComponent = Object.FindObjectOfType<ChallengeModeController>();
                _slideshowComponent = Object.FindObjectOfType<SlideshowController>();

                // Find spawn point under slideshow controller
                if (_slideshowComponent != null)
                {
                    Transform spawnChild = _slideshowComponent.transform.Find("FireworkSpawnPoint");
                    if (spawnChild != null)
                    {
                        _fireworkSpawnPoint = spawnChild;
                    }
                }

                Log("Skipped: GameFlowController (already exists)");
                _skippedCount++;
                return;
            }

            // Root GO
            GameObject go = new GameObject("GameFlowController");
            Undo.RegisterCreatedObjectUndo(go, "Create GameFlowController");
            go.AddComponent<GameFlowController>();
            Log("Created: GameFlowController");
            _createdCount++;

            // FreeModeController child
            GameObject freeGO = new GameObject("FreeModeController");
            freeGO.transform.SetParent(go.transform, false);
            _freeModeComponent = freeGO.AddComponent<FreeModeController>();
            Log("Created: FreeModeController (child)");
            _createdCount++;

            // ChallengeModeController child
            GameObject challengeGO = new GameObject("ChallengeModeController");
            challengeGO.transform.SetParent(go.transform, false);
            _challengeModeComponent = challengeGO.AddComponent<ChallengeModeController>();
            Log("Created: ChallengeModeController (child)");
            _createdCount++;

            // SlideshowController child
            GameObject slideshowGO = new GameObject("SlideshowController");
            slideshowGO.transform.SetParent(go.transform, false);
            _slideshowComponent = slideshowGO.AddComponent<SlideshowController>();
            Log("Created: SlideshowController (child)");
            _createdCount++;

            // FireworkSpawnPoint child of SlideshowController
            GameObject spawnPointGO = new GameObject("FireworkSpawnPoint");
            spawnPointGO.transform.SetParent(slideshowGO.transform, false);
            spawnPointGO.transform.localPosition = new Vector3(0f, 10f, 0f);
            _fireworkSpawnPoint = spawnPointGO.transform;
            Log("Created: FireworkSpawnPoint (child of SlideshowController)");
            _createdCount++;
        }

        private void CreateOrFindAudioManager()
        {
            if (Object.FindObjectOfType<AudioManager>() != null)
            {
                Log("Skipped: AudioManager (already exists)");
                _skippedCount++;
                return;
            }

            GameObject go = new GameObject("AudioManager");
            Undo.RegisterCreatedObjectUndo(go, "Create AudioManager");
            go.AddComponent<AudioManager>();

            Log("Created: AudioManager");
            _createdCount++;
        }

        /// <summary>
        /// Creates or finds a UI panel GO. If <paramref name="isStartActive"/> is false,
        /// the GO is deactivated BEFORE adding the component so OnEnable does not fire.
        /// </summary>
        private GameObject CreateOrFindUIPanel<T>(string name, Transform canvasTransform, bool isStartActive) where T : Component
        {
            // Use FindObjectsOfType(true) to find inactive GameObjects (panels start inactive)
            T[] found = Object.FindObjectsOfType<T>(true);
            if (found.Length > 0)
            {
                Log("Skipped: " + name + " (already exists)");
                _skippedCount++;
                return found[0].gameObject;
            }

            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(canvasTransform, false);

            RectTransform rect = go.GetComponent<RectTransform>();
            StretchFull(rect);

            if (!isStartActive)
            {
                go.SetActive(false);
            }

            go.AddComponent<T>();

            Log("Created: " + name);
            _createdCount++;
            return go;
        }

        // ================================================================
        //  UI CHILD CREATION
        // ================================================================

        /// <summary>
        /// Ensures a named child with RectTransform exists under the parent.
        /// Returns the existing child or creates a new one.
        /// </summary>
        private GameObject EnsureUIChild(string childName, GameObject parent)
        {
            Transform existing = parent.transform.Find(childName);
            if (existing != null)
            {
                return existing.gameObject;
            }

            GameObject child = new GameObject(childName, typeof(RectTransform));
            child.transform.SetParent(parent.transform, false);
            return child;
        }

        /// <summary>
        /// Ensures a named Button child exists under the parent, with Image, Button,
        /// and a Text child. Returns the Button component.
        /// </summary>
        private Button EnsureButtonChild(string childName, string label, GameObject parent)
        {
            GameObject buttonGO = EnsureUIChild(childName, parent);

            if (buttonGO.GetComponent<Image>() == null)
            {
                Image image = buttonGO.AddComponent<Image>();
                image.color = new Color(0.25f, 0.25f, 0.3f, 1f);
            }

            Button button = buttonGO.GetComponent<Button>();
            if (button == null)
            {
                button = buttonGO.AddComponent<Button>();
            }

            // Text child
            EnsureTextChild("Text", label, buttonGO);

            return button;
        }

        /// <summary>
        /// Ensures a named TextMeshProUGUI child exists under the parent. Returns the component.
        /// </summary>
        private TextMeshProUGUI EnsureTextChild(string childName, string defaultText, GameObject parent)
        {
            GameObject textGO = EnsureUIChild(childName, parent);

            TextMeshProUGUI text = textGO.GetComponent<TextMeshProUGUI>();
            if (text == null)
            {
                text = textGO.AddComponent<TextMeshProUGUI>();
                text.text = defaultText;
                text.fontSize = 18;
                text.alignment = TextAlignmentOptions.Center;
                text.color = Color.white;
                StretchFull(textGO.GetComponent<RectTransform>());
            }

            return text;
        }

        /// <summary>
        /// Ensures a named Image child exists under the parent. Returns the Image component.
        /// </summary>
        private Image EnsureImageChild(string childName, GameObject parent, Color defaultColor, bool isStartActive = true)
        {
            GameObject imageGO = EnsureUIChild(childName, parent);

            Image image = imageGO.GetComponent<Image>();
            if (image == null)
            {
                image = imageGO.AddComponent<Image>();
                image.color = defaultColor;
                StretchFull(imageGO.GetComponent<RectTransform>());
            }

            if (!isStartActive)
            {
                imageGO.SetActive(false);
            }

            return image;
        }

        /// <summary>
        /// Ensures a Slider child exists under the parent. Returns the Slider component.
        /// </summary>
        private Slider EnsureSliderChild(string childName, GameObject parent)
        {
            GameObject sliderGO = EnsureUIChild(childName, parent);

            Slider slider = sliderGO.GetComponent<Slider>();
            if (slider == null)
            {
                // Add Image as background
                Image bg = sliderGO.AddComponent<Image>();
                bg.color = new Color(0.2f, 0.2f, 0.25f, 1f);

                // Set size
                RectTransform sliderRect = sliderGO.GetComponent<RectTransform>();
                sliderRect.sizeDelta = new Vector2(200f, 30f);

                // Fill area
                GameObject fillArea = EnsureUIChild("Fill Area", sliderGO);
                StretchFull(fillArea.GetComponent<RectTransform>());

                GameObject fill = EnsureUIChild("Fill", fillArea);
                Image fillImage = fill.AddComponent<Image>();
                fillImage.color = new Color(0.4f, 0.6f, 1f, 1f);
                StretchFull(fill.GetComponent<RectTransform>());

                // Handle area
                GameObject handleArea = EnsureUIChild("Handle Slide Area", sliderGO);
                StretchFull(handleArea.GetComponent<RectTransform>());

                GameObject handle = EnsureUIChild("Handle", handleArea);
                Image handleImage = handle.AddComponent<Image>();
                handleImage.color = Color.white;
                RectTransform handleRect = handle.GetComponent<RectTransform>();
                handleRect.sizeDelta = new Vector2(20f, 0f);

                slider = sliderGO.AddComponent<Slider>();
                slider.fillRect = fill.GetComponent<RectTransform>();
                slider.handleRect = handleRect;
                slider.minValue = 0f;
                slider.maxValue = 1f;
                slider.value = 1f;
            }

            return slider;
        }

        /// <summary>
        /// Ensures a TMP_InputField child exists under the parent. Returns the component.
        /// </summary>
        private TMP_InputField EnsureInputFieldChild(string childName, GameObject parent)
        {
            GameObject inputGO = EnsureUIChild(childName, parent);

            TMP_InputField inputField = inputGO.GetComponent<TMP_InputField>();
            if (inputField == null)
            {
                Image bg = inputGO.AddComponent<Image>();
                bg.color = new Color(0.15f, 0.15f, 0.2f, 1f);

                RectTransform inputRect = inputGO.GetComponent<RectTransform>();
                inputRect.sizeDelta = new Vector2(400f, 80f);

                // Text child
                TextMeshProUGUI textComponent = EnsureTextChild("Text", "", inputGO);
                textComponent.richText = false;
                textComponent.alignment = TextAlignmentOptions.TopLeft;

                // Placeholder text child
                TextMeshProUGUI placeholder = EnsureTextChild("Placeholder", "Paste JSON here...", inputGO);
                placeholder.fontStyle = FontStyles.Italic;
                placeholder.color = new Color(1f, 1f, 1f, 0.3f);

                inputField = inputGO.AddComponent<TMP_InputField>();
                inputField.textComponent = textComponent;
                inputField.placeholder = placeholder;
                inputField.lineType = TMP_InputField.LineType.MultiLineNewline;
            }

            return inputField;
        }

        // ================================================================
        //  PREFAB CREATION
        // ================================================================

        private GameObject EnsureRequestCardPrefab()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(REQUEST_CARD_PREFAB_PATH);
            if (prefab != null)
            {
                return prefab;
            }

            GameObject cardObj = new GameObject("RequestCard");

            RectTransform rect = cardObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(200f, 80f);

            Image image = cardObj.AddComponent<Image>();
            image.color = new Color(0.15f, 0.15f, 0.2f, 1f);

            cardObj.AddComponent<Button>();

            // Text child
            GameObject textObj = new GameObject("Text", typeof(RectTransform));
            textObj.transform.SetParent(cardObj.transform, false);

            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = "Request";
            text.fontSize = 16;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            StretchFull(textObj.GetComponent<RectTransform>());

            prefab = PrefabUtility.SaveAsPrefabAsset(cardObj, REQUEST_CARD_PREFAB_PATH);
            Object.DestroyImmediate(cardObj);

            Log("Created prefab: " + REQUEST_CARD_PREFAB_PATH);
            _createdCount++;
            return prefab;
        }

        // ================================================================
        //  WIRING — All SerializedObject field wiring
        // ================================================================

        private void WireDataManager()
        {
            if (_dataManagerComponent == null)
            {
                return;
            }

            SerializedObject so = new SerializedObject(_dataManagerComponent);
            so.FindProperty("_onDataChanged").objectReferenceValue = _onDataChanged;
            so.FindProperty("_artworkCount").objectReferenceValue = _artworkCount;
            so.FindProperty("_activeRequestCount").objectReferenceValue = _activeRequestCount;
            so.FindProperty("_challengeConfig").objectReferenceValue = _challengeConfig;
            so.ApplyModifiedPropertiesWithoutUndo();

            Log("Wired: DataManager (4 fields)");
        }

        private void WireGameFlowController(FireworkSessionManager sessionManager)
        {
            // --- GameFlowController ---
            GameFlowController gfc = Object.FindObjectOfType<GameFlowController>();
            if (gfc != null)
            {
                SerializedObject so = new SerializedObject(gfc);
                so.FindProperty("_appState").objectReferenceValue = _appState;
                so.FindProperty("_freeModeController").objectReferenceValue = _freeModeComponent;
                so.FindProperty("_challengeModeController").objectReferenceValue = _challengeModeComponent;
                so.FindProperty("_slideshowController").objectReferenceValue = _slideshowComponent;
                so.FindProperty("_isChallengeMode").objectReferenceValue = _isChallengeMode;
                so.FindProperty("_onSlideshowComplete").objectReferenceValue = _onSlideshowComplete;
                so.FindProperty("_onSlideshowExitRequested").objectReferenceValue = _onSlideshowExitRequested;
                so.ApplyModifiedPropertiesWithoutUndo();
                Log("Wired: GameFlowController (7 fields)");
            }

            // --- FreeModeController ---
            if (_freeModeComponent != null)
            {
                SerializedObject so = new SerializedObject(_freeModeComponent);
                so.FindProperty("_sessionManager").objectReferenceValue = sessionManager;
                so.FindProperty("_dataManager").objectReferenceValue = _dataManagerComponent;
                so.FindProperty("_pixelData").objectReferenceValue = _outputPixelData;
                so.FindProperty("_canvasConfig").objectReferenceValue = _canvasConfig;
                so.FindProperty("_isCanvasInputEnabled").objectReferenceValue = _canvasInputEnabled;
                so.FindProperty("_onFireworkComplete").objectReferenceValue = _onFireworkComplete;
                so.FindProperty("_onCanvasCleared").objectReferenceValue = _onCanvasCleared;
                so.ApplyModifiedPropertiesWithoutUndo();
                Log("Wired: FreeModeController (7 fields)");
            }

            // --- ChallengeModeController ---
            if (_challengeModeComponent != null)
            {
                SerializedObject so = new SerializedObject(_challengeModeComponent);
                so.FindProperty("_sessionManager").objectReferenceValue = sessionManager;
                so.FindProperty("_dataManager").objectReferenceValue = _dataManagerComponent;
                so.FindProperty("_pixelData").objectReferenceValue = _outputPixelData;
                so.FindProperty("_canvasConfig").objectReferenceValue = _canvasConfig;
                so.FindProperty("_challengeConfig").objectReferenceValue = _challengeConfig;
                so.FindProperty("_isCanvasInputEnabled").objectReferenceValue = _canvasInputEnabled;
                so.FindProperty("_isSymmetryEnabled").objectReferenceValue = _symmetryEnabled;
                so.FindProperty("_remainingTime").objectReferenceValue = _remainingTime;
                so.FindProperty("_uniqueColorCount").objectReferenceValue = _uniqueColorCount;
                so.FindProperty("_filledPixelCount").objectReferenceValue = _filledPixelCount;
                so.FindProperty("_onFireworkComplete").objectReferenceValue = _onFireworkComplete;
                so.FindProperty("_onPixelPainted").objectReferenceValue = _onPixelPainted;
                so.FindProperty("_onConstraintViolated").objectReferenceValue = _onConstraintViolated;
                so.FindProperty("_onCanvasCleared").objectReferenceValue = _onCanvasCleared;
                so.ApplyModifiedPropertiesWithoutUndo();
                Log("Wired: ChallengeModeController (14 fields)");
            }

            // --- SlideshowController ---
            if (_slideshowComponent != null)
            {
                SerializedObject so = new SerializedObject(_slideshowComponent);
                so.FindProperty("_slideshowConfig").objectReferenceValue = _slideshowConfig;
                so.FindProperty("_dataManager").objectReferenceValue = _dataManagerComponent;
                so.FindProperty("_fireworkSpawnPoint").objectReferenceValue = _fireworkSpawnPoint;
                so.FindProperty("_onFireworkComplete").objectReferenceValue = _onFireworkComplete;
                so.FindProperty("_onSlideshowExitRequested").objectReferenceValue = _onSlideshowExitRequested;
                so.FindProperty("_onFireworkRequested").objectReferenceValue = _fireworkRequested;
                so.FindProperty("_onSlideshowStarted").objectReferenceValue = _onSlideshowStarted;
                so.FindProperty("_onSlideshowArtworkChanged").objectReferenceValue = _onSlideshowArtworkChanged;
                so.FindProperty("_onSlideshowComplete").objectReferenceValue = _onSlideshowComplete;
                so.FindProperty("_onSlideshowArtworkStarted").objectReferenceValue = _slideshowArtworkStarted;
                so.FindProperty("_slideshowCurrentIndex").objectReferenceValue = _slideshowCurrentIndex;
                so.FindProperty("_slideshowTotalCount").objectReferenceValue = _slideshowTotalCount;
                so.ApplyModifiedPropertiesWithoutUndo();
                Log("Wired: SlideshowController (12 fields)");
            }
        }

        private void WireAudioManager()
        {
            AudioManager audioManager = Object.FindObjectOfType<AudioManager>();
            if (audioManager == null)
            {
                return;
            }

            SerializedObject so = new SerializedObject(audioManager);
            so.FindProperty("_audioConfig").objectReferenceValue = _audioConfig;
            so.FindProperty("_masterVolume").objectReferenceValue = _masterVolume;
            so.FindProperty("_onPixelPainted").objectReferenceValue = _onPixelPainted;
            so.FindProperty("_onLaunchFirework").objectReferenceValue = _onLaunchFirework;
            so.FindProperty("_onFireworkComplete").objectReferenceValue = _onFireworkComplete;
            so.ApplyModifiedPropertiesWithoutUndo();

            Log("Wired: AudioManager (5 fields)");
        }

        private void WireMainMenuUI(GameObject panelGO)
        {
            MainMenuUI component = panelGO.GetComponent<MainMenuUI>();
            if (component == null)
            {
                return;
            }

            // Create child UI elements
            Button freeModeButton = EnsureButtonChild("FreeModeButton", "Free Mode", panelGO);
            Button challengeModeButton = EnsureButtonChild("ChallengeModeButton", "Challenge Mode", panelGO);
            Button slideshowButton = EnsureButtonChild("SlideshowButton", "Slideshow", panelGO);
            Button settingsButton = EnsureButtonChild("SettingsButton", "Settings", panelGO);
            TextMeshProUGUI artworkCountText = EnsureTextChild("ArtworkCountText", "0 Artworks Saved", panelGO);

            SerializedObject so = new SerializedObject(component);
            so.FindProperty("_appState").objectReferenceValue = _appState;
            so.FindProperty("_isChallengeMode").objectReferenceValue = _isChallengeMode;
            so.FindProperty("_artworkCount").objectReferenceValue = _artworkCount;
            so.FindProperty("_onDataChanged").objectReferenceValue = _onDataChanged;
            so.FindProperty("_freeModeButton").objectReferenceValue = freeModeButton;
            so.FindProperty("_challengeModeButton").objectReferenceValue = challengeModeButton;
            so.FindProperty("_slideshowButton").objectReferenceValue = slideshowButton;
            so.FindProperty("_settingsButton").objectReferenceValue = settingsButton;
            so.FindProperty("_artworkCountText").objectReferenceValue = artworkCountText;
            so.ApplyModifiedPropertiesWithoutUndo();

            Log("Wired: MainMenuUI (9 fields)");
        }

        private void WireRequestBoardUI(GameObject panelGO)
        {
            RequestBoardUI component = panelGO.GetComponent<RequestBoardUI>();
            if (component == null)
            {
                return;
            }

            // Create child UI elements
            GameObject cardContainer = EnsureUIChild("CardContainer", panelGO);
            Button backButton = EnsureButtonChild("BackButton", "Back", panelGO);
            GameObject requestCardPrefab = EnsureRequestCardPrefab();

            SerializedObject so = new SerializedObject(component);
            so.FindProperty("_appState").objectReferenceValue = _appState;
            so.FindProperty("_dataManager").objectReferenceValue = _dataManagerComponent;
            so.FindProperty("_selectedRequestIndex").objectReferenceValue = _selectedRequestIndex;
            so.FindProperty("_isChallengeMode").objectReferenceValue = _isChallengeMode;
            so.FindProperty("_onDataChanged").objectReferenceValue = _onDataChanged;
            so.FindProperty("_cardContainer").objectReferenceValue = cardContainer.transform;
            so.FindProperty("_requestCardPrefab").objectReferenceValue = requestCardPrefab;
            so.FindProperty("_backButton").objectReferenceValue = backButton;
            so.ApplyModifiedPropertiesWithoutUndo();

            Log("Wired: RequestBoardUI (8 fields)");
        }

        private void WireSettingsUI(GameObject panelGO)
        {
            SettingsUI component = panelGO.GetComponent<SettingsUI>();
            if (component == null)
            {
                return;
            }

            // Create child UI elements
            Slider volumeSlider = EnsureSliderChild("VolumeSlider", panelGO);
            Button exportButton = EnsureButtonChild("ExportButton", "Export", panelGO);
            Button importButton = EnsureButtonChild("ImportButton", "Import", panelGO);
            Button backButton = EnsureButtonChild("BackButton", "Back", panelGO);
            TMP_InputField importJsonField = EnsureInputFieldChild("ImportJsonField", panelGO);
            TextMeshProUGUI statusText = EnsureTextChild("StatusText", "", panelGO);

            SerializedObject so = new SerializedObject(component);
            so.FindProperty("_appState").objectReferenceValue = _appState;
            so.FindProperty("_dataManager").objectReferenceValue = _dataManagerComponent;
            so.FindProperty("_masterVolume").objectReferenceValue = _masterVolume;
            so.FindProperty("_onDataChanged").objectReferenceValue = _onDataChanged;
            so.FindProperty("_exportButton").objectReferenceValue = exportButton;
            so.FindProperty("_importButton").objectReferenceValue = importButton;
            so.FindProperty("_backButton").objectReferenceValue = backButton;
            so.FindProperty("_volumeSlider").objectReferenceValue = volumeSlider;
            so.FindProperty("_importJsonField").objectReferenceValue = importJsonField;
            so.FindProperty("_statusText").objectReferenceValue = statusText;
            so.ApplyModifiedPropertiesWithoutUndo();

            Log("Wired: SettingsUI (10 fields)");
        }

        private void WireSlideshowUI(GameObject panelGO)
        {
            SlideshowUI component = panelGO.GetComponent<SlideshowUI>();
            if (component == null)
            {
                return;
            }

            // Create child UI elements
            TextMeshProUGUI progressText = EnsureTextChild("ProgressText", "Artwork 0 of 0", panelGO);
            TextMeshProUGUI artworkNameText = EnsureTextChild("ArtworkNameText", "", panelGO);
            Button skipButton = EnsureButtonChild("SkipButton", "Skip", panelGO);
            Button exitButton = EnsureButtonChild("ExitButton", "Exit", panelGO);

            // LikeButton with LikeIcon child
            Button likeButton = EnsureButtonChild("LikeButton", "Like", panelGO);
            Image likeIcon = EnsureImageChild("LikeIcon", likeButton.gameObject, Color.white);

            SerializedObject so = new SerializedObject(component);
            so.FindProperty("_appState").objectReferenceValue = _appState;
            so.FindProperty("_slideshowCurrentIndex").objectReferenceValue = _slideshowCurrentIndex;
            so.FindProperty("_slideshowTotalCount").objectReferenceValue = _slideshowTotalCount;
            so.FindProperty("_onSlideshowArtworkChanged").objectReferenceValue = _onSlideshowArtworkChanged;
            so.FindProperty("_onSlideshowArtworkStarted").objectReferenceValue = _slideshowArtworkStarted;
            so.FindProperty("_onSlideshowComplete").objectReferenceValue = _onSlideshowComplete;
            so.FindProperty("_onSlideshowExitRequested").objectReferenceValue = _onSlideshowExitRequested;
            so.FindProperty("_onArtworkLiked").objectReferenceValue = _onArtworkLiked;
            so.FindProperty("_progressText").objectReferenceValue = progressText;
            so.FindProperty("_artworkNameText").objectReferenceValue = artworkNameText;
            so.FindProperty("_skipButton").objectReferenceValue = skipButton;
            so.FindProperty("_exitButton").objectReferenceValue = exitButton;
            so.FindProperty("_likeButton").objectReferenceValue = likeButton;
            so.FindProperty("_likeIcon").objectReferenceValue = likeIcon;
            so.ApplyModifiedPropertiesWithoutUndo();

            Log("Wired: SlideshowUI (14 fields)");
        }

        private void WireChallengeHUD(GameObject panelGO)
        {
            ChallengeHUD component = panelGO.GetComponent<ChallengeHUD>();
            if (component == null)
            {
                return;
            }

            // Create child UI elements
            TextMeshProUGUI timerText = EnsureTextChild("TimerText", "0", panelGO);
            TextMeshProUGUI colorCountText = EnsureTextChild("ColorCountText", "Colors: 0", panelGO);
            TextMeshProUGUI pixelCountText = EnsureTextChild("PixelCountText", "Pixels: 0", panelGO);
            Image warningFlash = EnsureImageChild("WarningFlash", panelGO,
                new Color(1f, 0f, 0f, 0.3f), false);

            SerializedObject so = new SerializedObject(component);
            so.FindProperty("_isChallengeMode").objectReferenceValue = _isChallengeMode;
            so.FindProperty("_remainingTime").objectReferenceValue = _remainingTime;
            so.FindProperty("_uniqueColorCount").objectReferenceValue = _uniqueColorCount;
            so.FindProperty("_filledPixelCount").objectReferenceValue = _filledPixelCount;
            so.FindProperty("_onPixelPainted").objectReferenceValue = _onPixelPainted;
            so.FindProperty("_onConstraintViolated").objectReferenceValue = _onConstraintViolated;
            so.FindProperty("_timerText").objectReferenceValue = timerText;
            so.FindProperty("_colorCountText").objectReferenceValue = colorCountText;
            so.FindProperty("_pixelCountText").objectReferenceValue = pixelCountText;
            so.FindProperty("_warningFlash").objectReferenceValue = warningFlash;
            so.ApplyModifiedPropertiesWithoutUndo();

            Log("Wired: ChallengeHUD (10 fields)");
        }

        private void WireConfirmationDialog(GameObject panelGO)
        {
            ConfirmationDialog component = panelGO.GetComponent<ConfirmationDialog>();
            if (component == null)
            {
                return;
            }

            // Create child hierarchy: Panel (starts inactive) > Title, Message, Confirm, Cancel
            GameObject dialogPanel = EnsureUIChild("Panel", panelGO);
            if (dialogPanel.GetComponent<Image>() == null)
            {
                Image panelImage = dialogPanel.AddComponent<Image>();
                panelImage.color = new Color(0.1f, 0.1f, 0.15f, 0.95f);
                StretchFull(dialogPanel.GetComponent<RectTransform>());
            }
            dialogPanel.SetActive(false);

            TextMeshProUGUI titleText = EnsureTextChild("TitleText", "Confirm", dialogPanel);
            TextMeshProUGUI messageText = EnsureTextChild("MessageText", "", dialogPanel);
            Button confirmButton = EnsureButtonChild("ConfirmButton", "Confirm", dialogPanel);
            Button cancelButton = EnsureButtonChild("CancelButton", "Cancel", dialogPanel);

            SerializedObject so = new SerializedObject(component);
            so.FindProperty("_panel").objectReferenceValue = dialogPanel;
            so.FindProperty("_titleText").objectReferenceValue = titleText;
            so.FindProperty("_messageText").objectReferenceValue = messageText;
            so.FindProperty("_confirmButton").objectReferenceValue = confirmButton;
            so.FindProperty("_cancelButton").objectReferenceValue = cancelButton;
            so.ApplyModifiedPropertiesWithoutUndo();

            Log("Wired: ConfirmationDialog (5 fields)");
        }

        private void WireDrawingCanvasIfPresent()
        {
            DrawingCanvas drawingCanvas = Object.FindObjectOfType<DrawingCanvas>();
            if (drawingCanvas == null)
            {
                Log("Skipped: DrawingCanvas wiring (not found in scene)");
                return;
            }

            SerializedObject so = new SerializedObject(drawingCanvas);
            so.FindProperty("_onPixelPainted").objectReferenceValue = _onPixelPainted;
            so.FindProperty("_isSymmetryEnabled").objectReferenceValue = _symmetryEnabled;
            so.FindProperty("_symmetryMode").objectReferenceValue = _symmetryMode;
            so.ApplyModifiedPropertiesWithoutUndo();

            Log("Wired: DrawingCanvas (3 Phase 7 fields)");
        }

        // ================================================================
        //  MOCK DATA SEEDER
        // ================================================================

        private void CreateOrFindMockDataSeeder()
        {
            MockDataSeeder existing = Object.FindObjectOfType<MockDataSeeder>();
            if (existing != null)
            {
                Log("Skipped: MockDataSeeder (already exists)");
                _skippedCount++;
                return;
            }

            if (_dataManagerComponent == null)
            {
                return;
            }

            GameObject go = new GameObject("MockDataSeeder");
            Undo.RegisterCreatedObjectUndo(go, "Create MockDataSeeder");
            MockDataSeeder seeder = go.AddComponent<MockDataSeeder>();

            SerializedObject so = new SerializedObject(seeder);
            so.FindProperty("_dataManager").objectReferenceValue = _dataManagerComponent;
            so.ApplyModifiedPropertiesWithoutUndo();

            Log("Created: MockDataSeeder (wired to DataManager)");
            _createdCount++;
        }

        // ================================================================
        //  UTILITY METHODS
        // ================================================================

        private T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
            {
                return asset;
            }

            asset = ScriptableObject.CreateInstance<T>();
            asset.name = System.IO.Path.GetFileNameWithoutExtension(path);
            AssetDatabase.CreateAsset(asset, path);
            Log("Created asset: " + path);
            _createdCount++;
            return asset;
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

        private void Log(string message)
        {
            _logOutput += message + "\n";
        }
    }
}
