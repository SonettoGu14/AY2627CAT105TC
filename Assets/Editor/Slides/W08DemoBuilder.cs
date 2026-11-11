using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Builds the W8 demos: one 3D set per knowledge point, each with its own perspective camera,
/// lights and ground. Everything here is editor-side; the runtime components are the small
/// DemoBase scripts in Assets/Scripts/Slides/Demos/W08.
/// </summary>
internal static class W08DemoBuilder
{
    private static readonly Color KeyColor = new Color(1f, 0.96f, 0.90f);
    private static readonly Color FillColor = new Color(0.55f, 0.70f, 1f);

    public static List<SlideDemoBuilder.Spec> Specs()
    {
        List<SlideDemoBuilder.Spec> s = new List<SlideDemoBuilder.Spec>();

        s.Add(D("mesh_anatomy",     BuildMeshAnatomy));
        s.Add(D("mesh_normals",     BuildMeshNormals));
        s.Add(D("uv_map",           BuildUv));
        s.Add(D("mat_shader",       BuildShader));
        s.Add(D("mat_shared",       BuildShared));
        s.Add(D("pbr_metallic",     t => BuildPbr(t, PbrDemo.Mode.Metallic)));
        s.Add(D("pbr_smoothness",   t => BuildPbr(t, PbrDemo.Mode.Smoothness)));
        s.Add(D("pbr_normalmap",    BuildNormalMap));
        s.Add(D("pbr_three",        BuildPbrThree));
        s.Add(D("bug_smoothness",   t => BuildComparison(t, MaterialComparisonDemo.Mode.WetWood)));
        s.Add(D("bug_albedo",       t => BuildComparison(t, MaterialComparisonDemo.Mode.PaintedMetal)));
        s.Add(D("light_directional", t => BuildLight(t, LightDemo.Mode.Directional)));
        s.Add(D("light_point",      t => BuildLight(t, LightDemo.Mode.Point)));
        s.Add(D("light_spot",       t => BuildLight(t, LightDemo.Mode.Spot)));
        s.Add(D("shadow_realtime",  BuildShadow));
        s.Add(D("studio",           BuildStudio));

        return s;
    }

    private static SlideDemoBuilder.Spec D(string key, System.Action<Transform> build)
    {
        return new SlideDemoBuilder.Spec { key = key, build = build };
    }

    // ==================================================================================
    //  Shared set dressing
    // ==================================================================================

    private static void Ground(Transform t) { W08Kit.Floor(t, W08Kit.Ground()); }

    private static Camera Cam(Transform t, Vector3 eye, Vector3 target, float fov = 38f)
    {
        return W08Kit.Camera(t, "Camera", eye, target, fov);
    }

    private static Light Key(Transform t, float intensity = 1.6f)
    {
        return W08Kit.Directional(t, "Key Light", Quaternion.Euler(48f, -35f, 0f),
                                  KeyColor, intensity, LightShadows.Soft);
    }

    private static Light Fill(Transform t)
    {
        return W08Kit.Point(t, "Fill Light", new Vector3(-3.5f, 3f, -3.5f), FillColor, 3f, 14f);
    }

    private static GameObject Pedestal(Transform t, float height, float width)
    {
        return W08Kit.Primitive(t, "Pedestal", PrimitiveType.Cube, new Vector3(0f, height * 0.5f, 0f),
                                new Vector3(width, height, width),
                                W08Kit.Lit("W08_Pedestal", W08Kit.Srgb(0x2A3448), 0f, 0.4f));
    }

    private static Transform Spin(Transform t, string name, Vector3 pos)
    {
        return LabKit.Child(t, name, pos).transform;
    }

    // ==================================================================================
    //  Slide 4 - mesh anatomy
    // ==================================================================================

    private static void BuildMeshAnatomy(Transform t)
    {
        Ground(t);
        Key(t, 1.8f);
        Fill(t);
        Cam(t, new Vector3(0f, 2.25f, -5.4f), new Vector3(0f, 0.2f, 0f), 36f);

        Transform spin = Spin(t, "Spin", new Vector3(0f, 1.02f, 0f));
        Material cubeMat = W08Kit.Lit("W08_MeshCube", W08Kit.Srgb(0x6E9BD6), 0f, 0.35f);
        GameObject cube = W08Kit.MeshObject(spin, "Cube", PrimitiveType.Cube, Vector3.zero, 1.5f, cubeMat);

        Material vertexMat = W08Kit.Unlit("W08_Vertex", new Color(1f, 0.72f, 0.25f).linear);
        Transform vertices = LabKit.Child(spin, "Vertices", Vector3.zero).transform;
        const float h = 0.75f;
        for (int i = 0; i < 8; i++)
            W08Kit.Primitive(vertices, "Vertex " + i, PrimitiveType.Sphere, Corner(i, h),
                             Vector3.one * 0.13f, vertexMat);

        Transform edges = LabKit.Child(spin, "Edges", Vector3.zero).transform;
        for (int i = 0; i < 8; i++)
            for (int bit = 1; bit <= 4; bit <<= 1)
                if ((i & bit) == 0)
                    W08Kit.Line(edges, "Edge " + i + "-" + (i | bit), Corner(i, h), Corner(i | bit, h),
                                new Color(0.45f, 0.80f, 1f), 0.025f);

        MeshAnatomyDemo d = t.gameObject.AddComponent<MeshAnatomyDemo>();
        d.spin = spin;
        d.cube = cube.GetComponent<MeshFilter>();
        d.cubeRenderer = cube.GetComponent<MeshRenderer>();
        d.vertexRoot = vertices.gameObject;
        d.edgeRoot = edges.gameObject;
    }

    private static Vector3 Corner(int i, float h)
    {
        return new Vector3((i & 1) != 0 ? h : -h, (i & 2) != 0 ? h : -h, (i & 4) != 0 ? h : -h);
    }

    // ==================================================================================
    //  Slide 5 - normals
    // ==================================================================================

    private static void BuildMeshNormals(Transform t)
    {
        Ground(t);
        Key(t, 1.8f);
        Fill(t);
        Cam(t, new Vector3(0f, 2.3f, -5.9f), new Vector3(0f, 0.25f, 0f), 38f);

        Material mat = W08Kit.Lit("W08_Normals", W08Kit.Srgb(0xB8C2D2), 0f, 0.42f);
        GameObject left = W08Kit.Primitive(t, "Smooth", PrimitiveType.Sphere,
            new Vector3(-1.55f, 1.15f, 0f), Vector3.one * 1.9f, mat);
        GameObject right = W08Kit.Primitive(t, "Flat", PrimitiveType.Sphere,
            new Vector3(1.55f, 1.15f, 0f), Vector3.one * 1.9f, mat);

        Transform leftNormals = LabKit.Child(left.transform, "Normals", Vector3.zero).transform;
        Transform rightNormals = LabKit.Child(right.transform, "Normals", Vector3.zero).transform;

        MeshNormalsDemo d = t.gameObject.AddComponent<MeshNormalsDemo>();
        d.spinners = new[] { left.transform, right.transform };
        d.smoothFilter = left.GetComponent<MeshFilter>();
        d.flatFilter = right.GetComponent<MeshFilter>();
        d.smoothNormalsRoot = leftNormals;
        d.flatNormalsRoot = rightNormals;
    }

    // ==================================================================================
    //  Slide 6 - UVs
    // ==================================================================================

    private static void BuildUv(Transform t)
    {
        Ground(t);
        Key(t, 1.7f);
        Fill(t);
        Cam(t, new Vector3(0f, 2.25f, -6.3f), new Vector3(0f, 0.3f, 0f), 40f);

        Texture2D checker = W08Kit.Checker();
        Material preview = W08Kit.UnlitTexture("W08_UvPreview", checker);
        Material target = W08Kit.Lit("W08_UvTarget", Color.white, 0.05f, 0.40f, checker);

        W08Kit.Primitive(t, "Flat Image", PrimitiveType.Cube, new Vector3(-2.55f, 1.45f, 0.30f),
                         new Vector3(2.2f, 2.2f, 0.06f), preview);
        Pedestal(t, 0.36f, 1.6f).transform.localPosition = new Vector3(1.35f, 0.18f, 0f);

        Transform spin = Spin(t, "Spin", new Vector3(1.35f, 1.16f, 0f));
        W08Kit.MeshObject(spin, "Cube", PrimitiveType.Cube, Vector3.zero, 1.6f, target);

        UvDemo d = t.gameObject.AddComponent<UvDemo>();
        d.spin = spin;
        d.targetMaterial = target;
    }

    // ==================================================================================
    //  Slide 7 - shader + parameters, and shared materials
    // ==================================================================================

    private static void BuildShader(Transform t)
    {
        Ground(t);
        Key(t, 1.8f);
        Fill(t);
        Cam(t, new Vector3(0f, 2.25f, -5.6f), new Vector3(0f, 0.25f, 0f), 36f);
        Pedestal(t, 0.36f, 2.4f);

        Color orange = new Color(0.85f, 0.55f, 0.28f);
        Material lit = W08Kit.Lit("W08_ShaderLit", W08Kit.Srgb(0xD98A4A), 0.2f, 0.55f);
        Material unlit = W08Kit.Unlit("W08_ShaderUnlit", orange.linear);
        Material broken = W08Kit.Broken();

        Transform spin = Spin(t, "Spin", new Vector3(0f, 1.11f, 0f));
        GameObject cube = W08Kit.MeshObject(spin, "Cube", PrimitiveType.Cube, Vector3.zero, 1.5f, lit);

        ShaderDemo d = t.gameObject.AddComponent<ShaderDemo>();
        d.spin = spin;
        d.target = cube.GetComponent<Renderer>();
        d.lit = lit;
        d.unlit = unlit;
        d.broken = broken;
    }

    private static void BuildShared(Transform t)
    {
        Ground(t);
        Key(t, 1.7f);
        Fill(t);
        Cam(t, new Vector3(0f, 2.4f, -7.6f), new Vector3(0f, 0.15f, 0f), 38f);

        Material shared = W08Kit.Lit("W08_Shared", W08Kit.Srgb(0x4C7CF0), 0.1f, 0.45f);
        Material over = W08Kit.Lit("W08_SharedOverride", W08Kit.Srgb(0xF26B8A), 0.1f, 0.45f);
        Renderer[] targets = new Renderer[3];
        Transform[] spins = new Transform[3];
        for (int i = 0; i < 3; i++)
        {
            Transform spin = Spin(t, "Spin " + i, new Vector3((i - 1) * 2.7f, 0.72f, 0f));
            GameObject cube = W08Kit.MeshObject(spin, "Cube", PrimitiveType.Cube, Vector3.zero, 1.4f, shared);
            targets[i] = cube.GetComponent<Renderer>();
            spins[i] = spin;
        }

        SharedMaterialDemo d = t.gameObject.AddComponent<SharedMaterialDemo>();
        d.targets = targets;
        d.shared = shared;
        d.overrideMaterial = over;
        d.spinners = spins;
    }

    // ==================================================================================
    //  Slide 8 - the PBR values
    // ==================================================================================

    private static void BuildPbr(Transform t, PbrDemo.Mode mode)
    {
        Ground(t);
        Key(t, 1.2f);
        Fill(t);
        W08Kit.Point(t, "Highlight", new Vector3(1.8f, 3.2f, -2.2f),
                     new Color(1f, 0.95f, 0.85f), 7f, 10f);
        Cam(t, new Vector3(0f, 2.3f, -5.8f), new Vector3(0f, 0.3f, 0f), 36f);
        Pedestal(t, 0.36f, 2.4f);

        Material mat = W08Kit.Lit("W08_PBR", W08Kit.Srgb(0xB0B6C0), 0f, 0.35f);
        GameObject sphere = W08Kit.Primitive(t, "Sphere", PrimitiveType.Sphere,
            new Vector3(0f, 1.31f, 0f), Vector3.one * 1.9f, mat);

        W08Kit.Softboxes(t);
        W08Kit.Probe(t, new Vector3(0f, 2.4f, -6f), new Vector3(12f, 8f, 12f));

        PbrDemo d = t.gameObject.AddComponent<PbrDemo>();
        d.mode = mode;
        d.target = sphere.GetComponent<Renderer>();
        d.material = mat;
    }

    private static void BuildNormalMap(Transform t)
    {
        Ground(t);
        Key(t, 1.4f);
        Fill(t);
        W08Kit.Point(t, "Highlight", new Vector3(1.6f, 3.0f, -2.4f),
                     new Color(1f, 0.95f, 0.85f), 6f, 10f);
        Cam(t, new Vector3(0f, 2.3f, -5.8f), new Vector3(0f, 0.3f, 0f), 36f);
        Pedestal(t, 0.36f, 2.4f);

        Material mat = W08Kit.Lit("W08_NormalMap", W08Kit.Srgb(0x9AA3B2), 0f, 0.45f);
        Transform spin = Spin(t, "Spin", new Vector3(0f, 1.31f, 0f));
        GameObject sphere = W08Kit.Primitive(spin, "Sphere", PrimitiveType.Sphere,
            Vector3.zero, Vector3.one * 1.9f, mat);

        NormalMapDemo d = t.gameObject.AddComponent<NormalMapDemo>();
        d.spin = spin;
        d.target = sphere.GetComponent<Renderer>();
        d.material = mat;
        d.normalMap = W08Kit.BumpNormal();
    }

    // ==================================================================================
    //  Slide 9 - the three reference materials
    // ==================================================================================

    private static void BuildPbrThree(Transform t)
    {
        Ground(t);
        Light key = Key(t, 1.8f);
        W08Kit.Softboxes(t);
        W08Kit.Probe(t, new Vector3(0f, 2.4f, -7f), new Vector3(14f, 8f, 14f));
        Cam(t, new Vector3(0f, 2.8f, -8.0f), new Vector3(0f, -0.1f, 0f), 36f);

        Material metal = Metal();
        Material wood = Wood(0.2f);
        Material glass = Glass();

        Transform metalSpin = Spin(t, "Metal Spin", new Vector3(-2.3f, 0.75f, 0f));
        W08Kit.MeshObject(metalSpin, "Metal Cube", PrimitiveType.Cube, Vector3.zero, 1.5f, metal);

        Transform woodSpin = Spin(t, "Wood Spin", new Vector3(0f, 0.95f, 0f));
        W08Kit.Primitive(woodSpin, "Wood Sphere", PrimitiveType.Sphere, Vector3.zero, Vector3.one * 1.9f, wood);

        Transform glassSpin = Spin(t, "Glass Spin", new Vector3(2.3f, 1.2f, 0f));
        W08Kit.Primitive(glassSpin, "Glass Capsule", PrimitiveType.Capsule, Vector3.zero,
                         new Vector3(1.1f, 1.2f, 1.1f), glass);

        PbrThreeDemo d = t.gameObject.AddComponent<PbrThreeDemo>();
        d.spinners = new[] { metalSpin, woodSpin, glassSpin };
        d.keyLight = key;
    }

    // ==================================================================================
    //  Slide 10 - two failures, one property apart
    // ==================================================================================

    private static void BuildComparison(Transform t, MaterialComparisonDemo.Mode mode)
    {
        Ground(t);
        Key(t, 1.6f);
        Fill(t);
        W08Kit.Softboxes(t);
        W08Kit.Probe(t, new Vector3(0f, 2.3f, -6.5f), new Vector3(12f, 8f, 12f));
        Cam(t, new Vector3(0f, 2.3f, -6.6f), new Vector3(0f, 0.25f, 0f), 38f);

        Material left, right;
        if (mode == MaterialComparisonDemo.Mode.WetWood)
        {
            left = Wood(0.2f);
            right = Wood(0.9f);
        }
        else
        {
            left = W08Kit.Lit("W08_MetalGold", W08Kit.Srgb(0xD4AF37), 1f, 0.9f);
            right = W08Kit.Lit("W08_MetalPainted", W08Kit.Srgb(0x8B5A2B), 1f, 0.9f);
        }

        GameObject leftSphere = W08Kit.Primitive(t, "Left", PrimitiveType.Sphere,
            new Vector3(-1.35f, 1.05f, 0f), Vector3.one * 1.8f, left);
        GameObject rightSphere = W08Kit.Primitive(t, "Right", PrimitiveType.Sphere,
            new Vector3(1.35f, 1.05f, 0f), Vector3.one * 1.8f, right);

        MaterialComparisonDemo d = t.gameObject.AddComponent<MaterialComparisonDemo>();
        d.mode = mode;
        d.spinners = new[] { leftSphere.transform, rightSphere.transform };
    }

    // ==================================================================================
    //  Slide 11 - the three light types
    // ==================================================================================

    private static void BuildLight(Transform t, LightDemo.Mode mode)
    {
        Ground(t);
        Cam(t, new Vector3(0f, 3.2f, -8f), new Vector3(0f, 0.15f, 0f), 38f);

        Material obj = W08Kit.Lit("W08_LightObject", W08Kit.Srgb(0xC8D0DC), 0f, 0.35f);
        W08Kit.MeshObject(t, "Cube", PrimitiveType.Cube, new Vector3(-2.4f, 0.6f, 0f), 1.2f, obj);
        W08Kit.Primitive(t, "Sphere", PrimitiveType.Sphere, new Vector3(0f, 0.7f, 0f), Vector3.one * 1.4f, obj);
        W08Kit.Primitive(t, "Capsule", PrimitiveType.Capsule, new Vector3(2.4f, 0.9f, 0f),
                         new Vector3(0.9f, 0.9f, 0.9f), obj);

        Light lamp;
        switch (mode)
        {
            case LightDemo.Mode.Directional:
                lamp = W08Kit.Directional(t, "Directional Light", Quaternion.Euler(50f, -30f, 0f),
                                           KeyColor, 1.8f, LightShadows.Soft);
                break;

            case LightDemo.Mode.Point:
                lamp = W08Kit.Point(t, "Point Light", new Vector3(-2.5f, 2.6f, -1.5f),
                                     new Color(0.55f, 0.75f, 1f), 12f, 9f);
                W08Kit.Marker(lamp.transform, "Marker", Vector3.zero,
                              W08Kit.Unlit("W08_MarkerCool", new Color(0.55f, 0.75f, 1f).linear), 0.24f);
                break;

            default:
                lamp = W08Kit.Spot(t, "Spot Light", new Vector3(0f, 4.0f, -2f),
                                    new Vector3(0f, 0f, 0.6f), 35f, 10f, Color.white, 20f);
                W08Kit.Marker(lamp.transform, "Marker", Vector3.zero,
                              W08Kit.Unlit("W08_MarkerWhite", Color.white), 0.24f);
                break;
        }

        LightDemo d = t.gameObject.AddComponent<LightDemo>();
        d.mode = mode;
        d.lamp = lamp;
    }

    // ==================================================================================
    //  Slide 12 - realtime shadows
    // ==================================================================================

    private static void BuildShadow(Transform t)
    {
        Ground(t);
        Cam(t, new Vector3(0f, 3.0f, -6.8f), new Vector3(0f, 0.2f, 0f), 38f);
        Light lamp = W08Kit.Directional(t, "Sun", Quaternion.Euler(55f, -35f, 0f),
                                         KeyColor, 1.8f, LightShadows.Soft);

        Material mat = W08Kit.Lit("W08_ShadowObject", W08Kit.Srgb(0xD8DEE8), 0f, 0.4f);
        Transform spin = Spin(t, "Spin", Vector3.zero);
        W08Kit.Primitive(spin, "Post L", PrimitiveType.Cube, new Vector3(-0.9f, 0.9f, 0f),
                         new Vector3(0.35f, 1.8f, 0.35f), mat);
        W08Kit.Primitive(spin, "Post R", PrimitiveType.Cube, new Vector3(0.9f, 0.9f, 0f),
                         new Vector3(0.35f, 1.8f, 0.35f), mat);
        W08Kit.Primitive(spin, "Top", PrimitiveType.Cube, new Vector3(0f, 1.9f, 0f),
                         new Vector3(2.15f, 0.35f, 0.35f), mat);

        ShadowDemo d = t.gameObject.AddComponent<ShadowDemo>();
        d.spinner = spin;
        d.lamp = lamp;
    }

    // ==================================================================================
    //  Slide 13 - the tiny studio
    // ==================================================================================

    private static void BuildStudio(Transform t)
    {
        Ground(t);
        W08Kit.Softboxes(t);
        W08Kit.Probe(t, new Vector3(0f, 2.6f, -7.5f), new Vector3(16f, 10f, 16f));
        Camera cam = Cam(t, new Vector3(0f, 3.0f, -8.2f), new Vector3(0f, 0.0f, 0f), 36f);

        Light key = Key(t, 1.5f);
        Light point = W08Kit.Point(t, "Point Light", new Vector3(-3.2f, 2.6f, -1.8f),
                                   new Color(0.55f, 0.75f, 1f), 10f, 9f);
        Light spot = W08Kit.Spot(t, "Spot Light", new Vector3(0f, 4.2f, -2.5f),
                                 new Vector3(0f, 0f, 0.5f), 32f, 9f, Color.white, 22f);

        Material metal = Metal();
        Material wood = Wood(0.2f);
        Material glass = Glass();

        Transform metalSpin = Spin(t, "Metal Spin", new Vector3(-2.3f, 0.75f, 0f));
        W08Kit.MeshObject(metalSpin, "Metal Cube", PrimitiveType.Cube, Vector3.zero, 1.5f, metal);

        Transform woodSpin = Spin(t, "Wood Spin", new Vector3(0f, 0.95f, 0f));
        W08Kit.Primitive(woodSpin, "Wood Sphere", PrimitiveType.Sphere, Vector3.zero, Vector3.one * 1.9f, wood);

        Transform glassSpin = Spin(t, "Glass Spin", new Vector3(2.3f, 1.2f, 0f));
        W08Kit.Primitive(glassSpin, "Glass Capsule", PrimitiveType.Capsule, Vector3.zero,
                         new Vector3(1.1f, 1.2f, 1.1f), glass);

        StudioDemo d = t.gameObject.AddComponent<StudioDemo>();
        d.cam = cam;
        d.spinners = new[] { metalSpin, woodSpin, glassSpin };
        d.key = key;
        d.point = point;
        d.spot = spot;
    }

    // ==================================================================================
    //  The three reference materials, shared by slide 9 and the studio
    // ==================================================================================

    private static Material Metal()
    {
        return W08Kit.Lit("W08_Metal", W08Kit.Srgb(0x888888), 1f, 0.9f);
    }

    private static Material Wood(float smoothness)
    {
        string name = smoothness > 0.5f ? "W08_WoodWet" : "W08_WoodDry";
        return W08Kit.Lit(name, Color.white, 0f, smoothness, W08Kit.Wood(), new Vector2(2f, 2f));
    }

    private static Material Glass()
    {
        return W08Kit.Transparent("W08_Glass", W08Kit.Srgb(0xBDE8F0), 1f, 0.3f);
    }
}
