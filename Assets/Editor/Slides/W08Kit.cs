using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Building blocks for the W8 deck: 3D primitives, URP materials, real lights and cameras.
///
/// This is editor-side furniture (like DemoKit for the 2D weeks) - the runtime demo scripts only
/// see the components and materials this file creates. Generated textures and materials live under
/// Assets/Art/W08Materials so several demos can share one material asset, which is itself the point
/// of the "shared material" demo.
/// </summary>
public static class W08Kit
{
    public const string Folder = "Assets/Art/W08Materials";
    public const string TextureFolder = Folder + "/Textures";

    public static readonly Color Bg = new Color(0.055f, 0.078f, 0.125f);

    // ------------------------------------------------------------------ colour

    /// <summary>An sRGB hex colour as a linear material colour.</summary>
    public static Color Srgb(int hex)
    {
        Color c = new Color32((byte)((hex >> 16) & 0xFF), (byte)((hex >> 8) & 0xFF), (byte)(hex & 0xFF), 0xFF);
        return c.linear;
    }

    // ------------------------------------------------------------------ generated textures

    public static Texture2D Checker()
    {
        return EnsureTexture("W08_Checker", 256, 256, (x, y) =>
        {
            bool light = ((x / 32) + (y / 32)) % 2 == 0;
            return light ? new Color32(0xE8, 0xEE, 0xFA, 0xFF) : new Color32(0x35, 0x5B, 0xD6, 0xFF);
        }, false);
    }

    public static Texture2D Wood()
    {
        return EnsureTexture("W08_Wood", 256, 256, (x, y) =>
        {
            float broad = Mathf.PerlinNoise(x * 0.025f, y * 0.120f);
            float fine = Mathf.PerlinNoise(x * 0.250f, y * 0.060f);
            float v = 0.84f + 0.10f * broad + 0.06f * fine;
            Color baseCol = new Color32(0x8B, 0x5A, 0x2B, 0xFF);
            return new Color32(
                (byte)Mathf.Clamp(baseCol.r * v * 255f, 0f, 255f),
                (byte)Mathf.Clamp(baseCol.g * v * 255f, 0f, 255f),
                (byte)Mathf.Clamp(baseCol.b * v * 255f, 0f, 255f), 0xFF);
        }, false);
    }

    public static Texture2D BumpNormal()
    {
        const int n = 256, bumps = 8;
        float[,] height = new float[n, n];
        for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float h = 0f;
                for (int by = 0; by < bumps; by++)
                    for (int bx = 0; bx < bumps; bx++)
                    {
                        float cx = (bx + 0.5f) * n / bumps;
                        float cy = (by + 0.5f) * n / bumps;
                        float dx = x - cx, dy = y - cy;
                        h += Mathf.Exp(-(dx * dx + dy * dy) / (2f * 11f * 11f));
                    }
                height[y, x] = h;
            }

        return EnsureRawTexture("W08_BumpNormal", n, n, (x, y) =>
        {
            float dx = height[y, (x + 1) % n] - height[y, (x - 1 + n) % n];
            float dy = height[(y + 1) % n, x] - height[(y - 1 + n) % n, x];
            Vector3 normal = new Vector3(-dx * 14f, -dy * 14f, 1f).normalized;
            return new Color32(
                (byte)Mathf.Clamp((normal.x * 0.5f + 0.5f) * 255f, 0f, 255f),
                (byte)Mathf.Clamp((normal.y * 0.5f + 0.5f) * 255f, 0f, 255f),
                (byte)Mathf.Clamp((normal.z * 0.5f + 0.5f) * 255f, 0f, 255f), 0xFF);
        }, true);
    }

    private static Texture2D EnsureTexture(string name, int w, int h, Func<int, int, Color32> pixel, bool linear)
    {
        EnsureFolders();
        string path = TextureFolder + "/" + name + ".png";
        if (!File.Exists(path))
        {
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false, linear);
            Color32[] px = new Color32[w * h];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    px[y * w + x] = pixel(x, y);
            tex.SetPixels32(px);
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        }

        TextureImporter imp = (TextureImporter)AssetImporter.GetAtPath(path);
        if (imp != null)
        {
            imp.textureType = TextureImporterType.Default;
            imp.sRGBTexture = !linear;
            imp.wrapMode = TextureWrapMode.Repeat;
            imp.filterMode = FilterMode.Bilinear;
            imp.mipmapEnabled = true;
            imp.textureCompression = TextureImporterCompression.Uncompressed;
            imp.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }

    /// <summary>A normal map: the RGB values ARE the normal, so import it as Normal map.</summary>
    private static Texture2D EnsureRawTexture(string name, int w, int h, Func<int, int, Color32> pixel, bool normalMap)
    {
        EnsureFolders();
        string path = TextureFolder + "/" + name + ".png";
        if (!File.Exists(path))
        {
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false, true);
            Color32[] px = new Color32[w * h];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    px[y * w + x] = pixel(x, y);
            tex.SetPixels32(px);
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        }

        TextureImporter imp = (TextureImporter)AssetImporter.GetAtPath(path);
        if (imp != null)
        {
            imp.textureType = normalMap ? TextureImporterType.NormalMap : TextureImporterType.Default;
            imp.sRGBTexture = false;
            imp.wrapMode = TextureWrapMode.Repeat;
            imp.filterMode = FilterMode.Bilinear;
            imp.mipmapEnabled = true;
            imp.textureCompression = TextureImporterCompression.Uncompressed;
            imp.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }

    // ------------------------------------------------------------------ materials

    public static Material Lit(string name, Color srgb, float metallic, float smoothness,
                               Texture albedo = null, Vector2? tiling = null)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        Material m = LoadOrCreate(name, shader);
        m.SetColor("_BaseColor", albedo != null ? Color.white : srgb);
        m.SetFloat("_Metallic", metallic);
        m.SetFloat("_Smoothness", smoothness);
        m.SetFloat("_SmoothnessTextureChannel", 0f);
        if (albedo != null)
        {
            m.SetTexture("_BaseMap", albedo);
            Vector2 t = tiling ?? Vector2.one;
            m.SetTextureScale("_BaseMap", t);
            m.SetTextureOffset("_BaseMap", Vector2.zero);
        }
        EditorUtility.SetDirty(m);
        return m;
    }

    public static Material Transparent(string name, Color srgb, float smoothness, float alpha)
    {
        Material m = LoadOrCreate(name, Shader.Find("Universal Render Pipeline/Lit"));
        Color c = srgb;
        c.a = alpha;
        m.SetColor("_BaseColor", c);
        m.SetFloat("_Metallic", 0f);
        m.SetFloat("_Smoothness", smoothness);
        m.SetFloat("_Surface", 1f);                 // Transparent
        m.SetFloat("_Blend", 0f);                   // Alpha
        m.SetFloat("_BlendModePreserveSpecular", 1f);
        m.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
        m.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
        m.SetFloat("_ZWrite", 0f);
        m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        m.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        m.SetOverrideTag("RenderType", "Transparent");
        m.renderQueue = (int)RenderQueue.Transparent;
        EditorUtility.SetDirty(m);
        return m;
    }

    public static Material Unlit(string name, Color srgb)
    {
        Material m = LoadOrCreate(name, Shader.Find("Universal Render Pipeline/Unlit"));
        m.SetColor("_BaseColor", srgb);
        EditorUtility.SetDirty(m);
        return m;
    }

    public static Material UnlitTexture(string name, Texture texture)
    {
        Material m = LoadOrCreate(name, Shader.Find("Universal Render Pipeline/Unlit"));
        m.SetColor("_BaseColor", Color.white);
        m.SetTexture("_BaseMap", texture);
        EditorUtility.SetDirty(m);
        return m;
    }

    /// <summary>The classic URP mistake: a material still using the Built-in Standard shader.</summary>
    public static Material Broken()
    {
        Shader shader = Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Hidden/InternalErrorShader");
        Material m = LoadOrCreate("W08_BrokenShader", shader);
        m.SetColor("_Color", new Color(0.5f, 0.5f, 0.5f, 1f));
        EditorUtility.SetDirty(m);
        return m;
    }

    private static Material LoadOrCreate(string name, Shader shader)
    {
        EnsureFolders();
        string path = Folder + "/" + name + ".mat";
        Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m == null)
        {
            m = new Material(shader);
            AssetDatabase.CreateAsset(m, path);
        }
        else if (shader != null && m.shader != shader)
        {
            m.shader = shader;
        }
        return m;
    }

    private static void EnsureFolders()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Art")) AssetDatabase.CreateFolder("Assets", "Art");
        if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/Art", "W08Materials");
        if (!AssetDatabase.IsValidFolder(TextureFolder)) AssetDatabase.CreateFolder(Folder, "Textures");
    }

    // ------------------------------------------------------------------ objects

    public static GameObject Primitive(Transform parent, string name, PrimitiveType type,
                                       Vector3 pos, Vector3 scale, Material material)
    {
        GameObject go = GameObject.CreatePrimitive(type);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        go.transform.localScale = scale;
        UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());
        go.GetComponent<Renderer>().sharedMaterial = material;
        return go;
    }

    /// <summary>A primitive with no scale applied to the transform, so its collider/child space is
    /// the mesh's own. Used for the cube in the mesh demo, where the readout quotes vertex counts.</summary>
    public static GameObject MeshObject(Transform parent, string name, PrimitiveType type, Vector3 pos, float size, Material material)
    {
        return Primitive(parent, name, type, pos, Vector3.one * size, material);
    }

    public static Camera Camera(Transform parent, string name, Vector3 eye, Vector3 target, float fov = 38f)
    {
        GameObject go = LabKit.Child(parent, name, eye);
        Camera c = go.AddComponent<Camera>();
        c.orthographic = false;
        c.fieldOfView = fov;
        c.nearClipPlane = 0.1f;
        c.farClipPlane = 100f;
        c.clearFlags = CameraClearFlags.SolidColor;
        c.backgroundColor = Bg;
        c.allowHDR = true;
        c.transform.LookAt(parent.TransformPoint(target), Vector3.up);
        return c;
    }

    public static Light Directional(Transform parent, string name, Quaternion rotation, Color color, float intensity, LightShadows shadows)
    {
        GameObject go = LabKit.Child(parent, name, new Vector3(0f, 4f, 0f));
        Light l = go.AddComponent<Light>();
        l.type = LightType.Directional;
        l.color = color;
        l.intensity = intensity;
        l.shadows = shadows;
        l.shadowStrength = 0.7f;
        go.transform.rotation = rotation;
        return l;
    }

    public static Light Point(Transform parent, string name, Vector3 pos, Color color, float intensity, float range)
    {
        GameObject go = LabKit.Child(parent, name, pos);
        Light l = go.AddComponent<Light>();
        l.type = LightType.Point;
        l.color = color;
        l.intensity = intensity;
        l.range = range;
        l.shadows = LightShadows.None;
        return l;
    }

    public static Light Spot(Transform parent, string name, Vector3 pos, Vector3 target, float angle, float range, Color color, float intensity)
    {
        GameObject go = LabKit.Child(parent, name, pos);
        Light l = go.AddComponent<Light>();
        l.type = LightType.Spot;
        l.color = color;
        l.intensity = intensity;
        l.range = range;
        l.spotAngle = angle;
        l.shadows = LightShadows.None;
        go.transform.LookAt(parent.TransformPoint(target), Vector3.up);
        return l;
    }

    /// <summary>A tiny unlit ball that shows where a point/spot light is.</summary>
    public static GameObject Marker(Transform parent, string name, Vector3 pos, Material material, float size = 0.16f)
    {
        GameObject go = Primitive(parent, name, PrimitiveType.Sphere, pos, Vector3.one * size, material);
        go.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
        return go;
    }

    public static LineRenderer Line(Transform parent, string name, Vector3 from, Vector3 to, Color color, float width = 0.02f)
    {
        GameObject go = LabKit.Child(parent, name, Vector3.zero);
        LineRenderer lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.positionCount = 2;
        lr.SetPosition(0, from);
        lr.SetPosition(1, to);
        lr.widthMultiplier = width;
        lr.material = Unlit("W08_Line_" + ColorUtility.ToHtmlStringRGB(color), color.linear);
        lr.shadowCastingMode = ShadowCastingMode.Off;
        lr.receiveShadows = false;
        return lr;
    }

    /// <summary>A realtime reflection probe. Realtime rather than baked because the demos build
    /// their objects while the scene is being generated and the camera/demo is only enabled later.</summary>
    public static ReflectionProbe Probe(Transform parent, Vector3 pos, Vector3 size)
    {
        GameObject go = LabKit.Child(parent, "Reflection Probe", pos);
        ReflectionProbe p = go.AddComponent<ReflectionProbe>();
        p.mode = ReflectionProbeMode.Realtime;
        p.refreshMode = ReflectionProbeRefreshMode.EveryFrame;
        p.timeSlicingMode = ReflectionProbeTimeSlicingMode.NoTimeSlicing;
        p.size = size;
        p.resolution = 128;
        p.nearClipPlane = 0.1f;
        p.farClipPlane = 50f;
        p.cullingMask = ~0;
        return p;
    }

    /// <summary>Two bright cards behind / beside the camera. They are outside the frame but inside
    /// the reflection probe, so metal and glass get the long soft highlight a studio would have.</summary>
    public static void Softboxes(Transform parent)
    {
        Material white = Unlit("W08_Softbox", new Color(1f, 1f, 1f));
        // Top light: the broad band a metal top face reflects.
        Primitive(parent, "Softbox Top", PrimitiveType.Cube,
            new Vector3(0f, 6.0f, -1.5f), new Vector3(9f, 0.06f, 5f), white);
        // Low card behind the camera: what the front faces of the cubes see.
        Primitive(parent, "Softbox Front", PrimitiveType.Cube,
            new Vector3(0f, 1.2f, -9.2f), new Vector3(9f, 3.2f, 0.06f), white);
        // Two side cards, out of frame, for the rim and the glass.
        GameObject left = Primitive(parent, "Softbox Left", PrimitiveType.Cube,
            new Vector3(-5.6f, 2.6f, -3.6f), new Vector3(3.2f, 4.4f, 0.06f), white);
        left.transform.LookAt(parent.TransformPoint(new Vector3(0f, 0.8f, 0f)), Vector3.up);
        GameObject right = Primitive(parent, "Softbox Right", PrimitiveType.Cube,
            new Vector3(5.6f, 2.6f, -3.6f), new Vector3(3.2f, 4.4f, 0.06f), white);
        right.transform.LookAt(parent.TransformPoint(new Vector3(0f, 0.8f, 0f)), Vector3.up);
    }

    // ------------------------------------------------------------------ scene lighting

    /// <summary>A dark studio with no skybox: the camera background and the distance fog are the
    /// slide's own navy, so the 3D stage reads as part of the deck. The reflections come from the
    /// softboxes and the realtime probe, not from a sky.</summary>
    public static void SetupRenderSettings()
    {
        RenderSettings.skybox = null;

        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = Srgb(0x35415C);
        RenderSettings.ambientEquatorColor = Srgb(0x222C40);
        RenderSettings.ambientGroundColor = Srgb(0x10151F);

        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = Bg;
        RenderSettings.fogStartDistance = 10f;
        RenderSettings.fogEndDistance = 24f;
    }

    public static Material Ground()
    {
        return Lit("W08_Ground", Srgb(0x4C5870), 0f, 0.30f);
    }

    public static GameObject Floor(Transform parent, Material ground, float size = 34f)
    {
        return Primitive(parent, "Ground", PrimitiveType.Plane, Vector3.zero,
            new Vector3(size / 10f, 1f, size / 10f), ground);
    }
}
