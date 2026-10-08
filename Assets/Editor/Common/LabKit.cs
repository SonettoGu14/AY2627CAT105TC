using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Week-independent scene-building toolkit for the CAT105TC labs.
///
/// Everything in here is generic infrastructure: paths, colours, layer/tag setup,
/// the generated shape sprites, physics materials, small GameObject/SpriteRenderer
/// helpers and the shared HUD. Nothing here belongs to a specific lecture - each
/// week's own builder lives in its own folder (W03_Physics2D, W04_AnimationCamera, ...).
///
/// Where the art / animation lives for the shared character:
///   * the imported sprites are week-independent art  -> Assets/Art/KenneyToonCharacters
///   * the AnimationClips and AnimatorController are W4 lecture content -> Assets/Animations/W04_AnimationCamera
///     and are built by CharacterRig (see W04_AnimationCamera/CharacterRig.cs)
/// </summary>
public static class LabKit
{
    // ------------------------------------------------------------------ paths
    public const string ShapesFolder = "Assets/Art/Shapes";
    public const string PhysMatFolder = "Assets/Art/Physics Materials";
    public const string CharacterArtFolder = "Assets/Art/KenneyToonCharacters/Male person/PNG/Poses";
    public const string ScenesFolder = "Assets/Scenes";
    public const string W3ScenePath = ScenesFolder + "/W3Lab.unity";
    public const string W4ScenePath = ScenesFolder + "/W4Lab.unity";

    // ------------------------------------------------------------------ palette
    public static readonly Color BgColor = new Color(0.085f, 0.105f, 0.16f);
    public static readonly Color GroundColor = new Color(0.24f, 0.29f, 0.40f);
    public static readonly Color PlatformColor = new Color(0.34f, 0.42f, 0.56f);
    public static readonly Color Accent = new Color(0.55f, 0.78f, 0.98f);
    public static readonly Color Gold = new Color(1f, 0.82f, 0.28f);
    public static readonly Color ZoneColor = new Color(0.95f, 0.72f, 0.25f, 0.30f);
    public static readonly Color PanelBg = new Color(0.04f, 0.05f, 0.08f, 0.72f);

    // ------------------------------------------------------------------ cached assets
    private static Font _font;
    private static Sprite _white;
    private static Sprite _circle;
    private static PhysicsMaterial2D _slick;
    private static PhysicsMaterial2D _bouncy;

    public static Font UiFont
    {
        get
        {
            if (_font == null) _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return _font;
        }
    }

    public static Sprite White { get { return EnsureWhiteSprite(); } }
    public static Sprite Circle { get { return EnsureCircleSprite(); } }
    public static PhysicsMaterial2D Slick { get { EnsurePhysicsMaterials(); return _slick; } }
    public static PhysicsMaterial2D Bouncy { get { EnsurePhysicsMaterials(); return _bouncy; } }

    /// <summary>Loads one of the shared character poses, e.g. "idle" or "walk3".</summary>
    public static Sprite CharacterSprite(string pose)
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>(CharacterArtFolder + "/character_malePerson_" + pose + ".png");
    }

    /// <summary>Layer lookup that does not depend on the editor having re-read the TagManager yet.</summary>
    public static int Layer(string name)
    {
        int idx = LayerMask.NameToLayer(name);
        if (idx >= 0) return idx;
        switch (name)
        {
            case "Ground": return 6;
            case "Player": return 7;
            case "OneWay": return 8;
            case "Hazard": return 9;
            case "Trigger": return 10;
            default: return 0;
        }
    }

    public static int GroundMask { get { return (1 << Layer("Ground")) | (1 << Layer("OneWay")); } }

    // ==================================================================================
    //  Shared project assets  (layers, tags, generated sprites, physics materials)
    // ==================================================================================

    public static void SetupSharedAssets()
    {
        EnsureLayersAndTags();
        EnsureWhiteSprite();
        EnsureCircleSprite();
        EnsurePhysicsMaterials();
        AssetDatabase.SaveAssets();
        Debug.Log("[LabKit] shared assets ready.");
    }

    private static void EnsureLayersAndTags()
    {
        Object[] managers = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
        if (managers == null || managers.Length == 0) return;

        SerializedObject so = new SerializedObject(managers[0]);

        SerializedProperty layers = so.FindProperty("layers");
        AssignLayer(layers, 6, "Ground");
        AssignLayer(layers, 7, "Player");
        AssignLayer(layers, 8, "OneWay");
        AssignLayer(layers, 9, "Hazard");
        AssignLayer(layers, 10, "Trigger");

        SerializedProperty tags = so.FindProperty("tags");
        AssignTag(tags, "Coin");
        AssignTag(tags, "Goal");
        AssignTag(tags, "Hazard");

        so.ApplyModifiedProperties();
        AssetDatabase.SaveAssets();
    }

    private static void AssignLayer(SerializedProperty layers, int index, string name)
    {
        SerializedProperty p = layers.GetArrayElementAtIndex(index);
        if (string.IsNullOrEmpty(p.stringValue)) p.stringValue = name;   // never clobber an existing layer
    }

    private static void AssignTag(SerializedProperty tags, string tag)
    {
        for (int i = 0; i < tags.arraySize; i++)
        {
            if (tags.GetArrayElementAtIndex(i).stringValue == tag) return;
        }
        tags.InsertArrayElementAtIndex(tags.arraySize);
        tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
    }

    private static Sprite EnsureWhiteSprite()
    {
        if (_white != null) return _white;
        string path = ShapesFolder + "/white.png";
        if (!File.Exists(path))
        {
            Directory.CreateDirectory(ShapesFolder);
            Texture2D tex = new Texture2D(32, 32, TextureFormat.RGBA32, false);
            Color[] px = new Color[32 * 32];
            for (int i = 0; i < px.Length; i++) px[i] = Color.white;
            tex.SetPixels(px);
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.Refresh();
            ImportGeneratedSprite(path, 32f);
        }
        _white = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        return _white;
    }

    private static Sprite EnsureCircleSprite()
    {
        if (_circle != null) return _circle;
        string path = ShapesFolder + "/circle.png";
        if (!File.Exists(path))
        {
            Directory.CreateDirectory(ShapesFolder);
            const int size = 64;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] px = new Color[size * size];
            float r = size * 0.46f;
            Vector2 c = new Vector2(size * 0.5f, size * 0.5f);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), c);
                    float a = Mathf.Clamp01((r - d) / 1.5f);
                    px[y * size + x] = new Color(1f, 1f, 1f, a);
                }
            }
            tex.SetPixels(px);
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.Refresh();
            ImportGeneratedSprite(path, 64f);
        }
        _circle = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        return _circle;
    }

    private static void ImportGeneratedSprite(string path, float pixelsPerUnit)
    {
        TextureImporter ti = (TextureImporter)AssetImporter.GetAtPath(path);
        ti.textureType = TextureImporterType.Sprite;
        ti.spritePixelsPerUnit = pixelsPerUnit;
        ti.alphaIsTransparency = true;
        ti.mipmapEnabled = false;
        ti.textureCompression = TextureImporterCompression.Uncompressed;
        ti.SaveAndReimport();
    }

    private static void EnsurePhysicsMaterials()
    {
        _slick = GetOrCreatePhysicsMaterial("Slick_NoFriction", 0f, 0f);
        _bouncy = GetOrCreatePhysicsMaterial("Bouncy_085", 0.3f, 0.85f);
    }

    private static PhysicsMaterial2D GetOrCreatePhysicsMaterial(string name, float friction, float bounciness)
    {
        string path = PhysMatFolder + "/" + name + ".physicsMaterial2D";
        PhysicsMaterial2D existing = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(path);
        if (existing != null) return existing;
        Directory.CreateDirectory(PhysMatFolder);
        PhysicsMaterial2D mat = new PhysicsMaterial2D();
        mat.friction = friction;
        mat.bounciness = bounciness;
        AssetDatabase.CreateAsset(mat, path);
        AssetDatabase.SaveAssets();
        return AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(path);
    }

    // ==================================================================================
    //  Small scene helpers
    // ==================================================================================

    public static GameObject Child(Transform parent, string name, Vector3 localPos)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        return go;
    }

    public static SpriteRenderer AddSprite(GameObject go, Sprite sprite, Color color, int order)
    {
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = color;
        sr.sortingOrder = order;
        return sr;
    }

    /// <summary>A static platform: root scale (1,1,1), the BoxCollider2D carries the size.</summary>
    public static GameObject Platform(Transform parent, string name, float cx, float cy, float w, float h, Color color, string layerName)
    {
        GameObject root = Child(parent, name, new Vector3(cx, cy, 0f));
        root.layer = Layer(layerName);
        BoxCollider2D box = root.AddComponent<BoxCollider2D>();
        box.size = new Vector2(w, h);
        GameObject vis = Child(root.transform, "Visual", Vector3.zero);
        vis.layer = root.layer;
        vis.transform.localScale = new Vector3(w, h, 1f);
        AddSprite(vis, White, color, 0);
        return root;
    }

    public static GameObject WorldLabel(Transform parent, string text, Vector3 pos, Color color)
    {
        GameObject go = Child(parent, "Label", pos);
        go.layer = 0;   // Default - always visible to the camera
        TextMesh tm = go.AddComponent<TextMesh>();
        tm.font = UiFont;
        tm.text = text;
        tm.fontSize = 64;
        tm.characterSize = 0.07f;
        tm.anchor = TextAnchor.LowerCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = color;
        tm.lineSpacing = 0.9f;
        MeshRenderer mr = go.GetComponent<MeshRenderer>();
        mr.sharedMaterial = UiFont.material;
        mr.sortingOrder = 200;
        return go;
    }

    public static Camera MakeCamera(Transform parent, Vector3 pos, float orthoSize)
    {
        GameObject go = Child(parent, "Main Camera", pos);
        go.tag = "MainCamera";
        Camera cam = go.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = orthoSize;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = BgColor;
        cam.nearClipPlane = -50f;
        cam.farClipPlane = 100f;
        go.AddComponent<AudioListener>();
        return cam;
    }

    public static void MakeLight(Transform parent)
    {
        GameObject go = Child(parent, "Directional Light", new Vector3(0f, 6f, -5f));
        Light light = go.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1f;
        go.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
    }

    public static void DestroyComponent<T>(GameObject go) where T : Component
    {
        T comp = go.GetComponent<T>();
        if (comp != null) Object.DestroyImmediate(comp);
    }

    public static void AddSceneToBuildSettings(string path)
    {
        System.Collections.Generic.List<EditorBuildSettingsScene> scenes =
            new System.Collections.Generic.List<EditorBuildSettingsScene>();
        foreach (EditorBuildSettingsScene s in EditorBuildSettings.scenes)
        {
            if (File.Exists(s.path)) scenes.Add(s);   // drop entries whose scene was deleted or moved
        }
        if (!scenes.Exists(s => s.path == path))
        {
            scenes.Add(new EditorBuildSettingsScene(path, true));
        }
        EditorBuildSettings.scenes = scenes.ToArray();
    }

    // ==================================================================================
    //  Shared HUD  (the status bar / input visualiser / debug panels every lab reuses)
    // ==================================================================================

    public static void BuildHud(string title, bool withChecklist, out LabHud hud)
    {
        GameObject canvasGO = new GameObject("HUD Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        Transform root = canvasGO.transform;

        hud = canvasGO.AddComponent<LabHud>();
        hud.title = title;

        Vector2 topLeft = new Vector2(0f, 1f);
        Vector2 topRight = new Vector2(1f, 1f);
        Vector2 bottomLeft = new Vector2(0f, 0f);
        Vector2 bottomRight = new Vector2(1f, 0f);
        Vector2 midLeft = new Vector2(0f, 0.5f);
        Vector2 midRight = new Vector2(1f, 0.5f);
        Vector2 bottomCenter = new Vector2(0.5f, 0f);

        // --- top bar -------------------------------------------------------------------
        UiImage(root, "TopBar", topLeft, topLeft, Vector2.zero, new Vector2(1920f, 78f), new Color(0.03f, 0.04f, 0.07f, 0.82f));
        hud.titleText = UiText(root, "Title", topLeft, topLeft, new Vector2(30f, -16f), new Vector2(1100f, 46f), 30, TextAnchor.MiddleLeft, Color.white);
        hud.scoreText = UiText(root, "Score", topRight, topRight, new Vector2(-30f, -12f), new Vector2(900f, 56f), 24, TextAnchor.MiddleRight, new Color(0.86f, 0.92f, 1f));

        // --- help (bottom-left) --------------------------------------------------------
        UiImage(root, "HelpPanel", bottomLeft, bottomLeft, new Vector2(28f, 28f), new Vector2(620f, 150f), PanelBg);
        UiText(root, "HelpHeader", bottomLeft, bottomLeft, new Vector2(48f, 142f), new Vector2(400f, 30f), 20, TextAnchor.MiddleLeft, Accent).text = "CONTROLS";
        hud.helpText = UiText(root, "Help", bottomLeft, bottomLeft, new Vector2(48f, 40f), new Vector2(590f, 100f), 21, TextAnchor.UpperLeft, new Color(0.85f, 0.88f, 0.94f));

        // --- input panel (mid-left) ----------------------------------------------------
        RectTransform inputPanel = UiPanelRect(root, "InputPanel", midLeft, midLeft, new Vector2(28f, -60f), new Vector2(480f, 280f));
        UiImage(inputPanel, "Bg", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(480f, 280f), PanelBg);
        UiText(inputPanel, "Header", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -10f), new Vector2(400f, 30f), 21, TextAnchor.MiddleLeft, Accent).text = "INPUT  (live)";
        UiImage(inputPanel, "MoveBarBg", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -52f), new Vector2(280f, 20f), new Color(0.16f, 0.18f, 0.24f, 1f));
        Image fill = UiImage(inputPanel, "MoveBarFill", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -52f), new Vector2(280f, 20f), Accent);
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillOrigin = 0;
        hud.moveBarFill = fill;
        UiText(inputPanel, "AxisLabel", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(304f, -52f), new Vector2(170f, 22f), 17, TextAnchor.MiddleLeft, new Color(0.75f, 0.8f, 0.88f)).text = "A -1 / D +1";
        hud.jumpDot = UiImage(inputPanel, "JumpDot", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -86f), new Vector2(18f, 18f), new Color(0.4f, 0.42f, 0.48f));
        UiText(inputPanel, "JumpLabel", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(44f, -82f), new Vector2(300f, 24f), 19, TextAnchor.MiddleLeft, new Color(0.8f, 0.84f, 0.9f)).text = "Space  (jump)";
        hud.groundDot = UiImage(inputPanel, "GroundDot", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -116f), new Vector2(18f, 18f), new Color(0.4f, 0.42f, 0.48f));
        UiText(inputPanel, "GroundLabel", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(44f, -112f), new Vector2(300f, 24f), 19, TextAnchor.MiddleLeft, new Color(0.8f, 0.84f, 0.9f)).text = "IsGrounded";
        hud.inputText = UiText(inputPanel, "Readout", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -146f), new Vector2(450f, 128f), 19, TextAnchor.UpperLeft, new Color(0.82f, 0.88f, 0.95f));

        // --- camera / rendering debug (mid-right) --------------------------------------
        RectTransform debugPanel = UiPanelRect(root, "DebugPanel", midRight, midRight, new Vector2(-28f, -60f), new Vector2(470f, 230f));
        UiImage(debugPanel, "Bg", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(470f, 230f), PanelBg);
        UiText(debugPanel, "Header", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -10f), new Vector2(400f, 30f), 21, TextAnchor.MiddleLeft, Accent).text = "CAMERA / RENDER   (C)";
        hud.debugText = UiText(debugPanel, "Readout", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -46f), new Vector2(440f, 180f), 19, TextAnchor.UpperLeft, new Color(0.82f, 0.88f, 0.95f));
        hud.debugPanel = debugPanel.gameObject;
        debugPanel.gameObject.SetActive(false);

        // --- checklist (bottom-right) --------------------------------------------------
        if (withChecklist)
        {
            RectTransform conceptPanel = UiPanelRect(root, "ChecklistPanel", bottomRight, bottomRight, new Vector2(-28f, 28f), new Vector2(560f, 320f));
            UiImage(conceptPanel, "Bg", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(560f, 320f), PanelBg);
            UiText(conceptPanel, "Header", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -10f), new Vector2(400f, 30f), 21, TextAnchor.MiddleLeft, Accent).text = "LAB CHECKLIST";
            hud.conceptsText = UiText(conceptPanel, "Readout", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -48f), new Vector2(530f, 260f), 20, TextAnchor.UpperLeft, new Color(0.85f, 0.9f, 0.96f));
        }

        // --- state banner (bottom-centre) ----------------------------------------------
        hud.stateText = UiText(root, "StateBanner", bottomCenter, bottomCenter, new Vector2(0f, 200f), new Vector2(1200f, 60f), 26, TextAnchor.MiddleCenter, new Color(1f, 0.95f, 0.7f));
    }

    public static RectTransform UiPanelRect(Transform parent, string name, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        RectTransform rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = pivot;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        return rt;
    }

    public static Image UiImage(Transform parent, string name, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size, Color color)
    {
        RectTransform rt = UiPanelRect(parent, name, anchor, pivot, pos, size);
        Image img = rt.gameObject.AddComponent<Image>();
        img.sprite = White;
        img.color = color;
        img.raycastTarget = false;
        return img;
    }

    public static Text UiText(Transform parent, string name, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size, int fontSize, TextAnchor align, Color color)
    {
        RectTransform rt = UiPanelRect(parent, name, anchor, pivot, pos, size);
        Text t = rt.gameObject.AddComponent<Text>();
        t.font = UiFont;
        t.fontSize = fontSize;
        t.alignment = align;
        t.color = color;
        t.raycastTarget = false;
        t.supportRichText = true;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.text = string.Empty;
        return t;
    }
}
