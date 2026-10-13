using UnityEngine;

/// <summary>
/// The reusable furniture every slide demo is built from, so adding a demo to a later week is a few
/// lines rather than forty. All of it is editor-side: SlideDemoBuilder calls these, they are never
/// in a built player.
/// </summary>
public static class DemoKit
{
    // ---- standard palette, so every week's demos look like the same course ----
    public static readonly Color Orange = new Color32(0xFF, 0xB4, 0x54, 0xFF);
    public static readonly Color Blue = new Color32(0x6F, 0xA8, 0xFF, 0xFF);
    public static readonly Color Green = new Color32(0x7C, 0xE3, 0x8B, 0xFF);
    public static readonly Color Pink = new Color32(0xF2, 0x6B, 0x8A, 0xFF);
    public static readonly Color Slate = new Color32(0x2A, 0x36, 0x4E, 0xFF);
    public static readonly Color Dim = new Color32(0x8F, 0xA3, 0xBF, 0xFF);
    public static readonly Color Dark = new Color32(0x0E, 0x14, 0x20, 0xFF);

    /// <summary>A sprite object scaled to a rectangle - the basic building block.</summary>
    public static GameObject Visual(Transform parent, string name, Sprite sprite, Color color,
                                    Vector2 pos, Vector2 scale, int order)
    {
        GameObject go = LabKit.Child(parent, name, pos);
        go.transform.localScale = new Vector3(scale.x, scale.y, 1f);
        LabKit.AddSprite(go, sprite, color, order);
        return go;
    }

    /// <summary>The shared character sprite, so demos across weeks show the same character.</summary>
    public static GameObject Character(Transform parent, string name, Vector2 pos, Vector2 scale, int order)
    {
        return Visual(parent, name, LabKit.CharacterSprite("idle"), Color.white, pos, scale, order);
    }

    /// <summary>A static 2D floor slab on the Ground layer (needs no Rigidbody).</summary>
    public static GameObject Floor(Transform parent, string name, float cx, float cy, float w, float h, Color color)
    {
        return LabKit.Platform(parent, name, cx, cy, w, h, color, "Ground");
    }

    /// <summary>A ray / line for the raycast and follow demos.</summary>
    public static LineRenderer Line(Transform parent, string name, int order)
    {
        GameObject go = LabKit.Child(parent, name, Vector3.zero);
        LineRenderer lr = go.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.widthMultiplier = 0.08f;
        lr.useWorldSpace = true;
        lr.sortingOrder = order;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        return lr;
    }

    /// <summary>A track + a fill, the shape the field-modifier and slider demos keep needing.</summary>
    public static GameObject Bar(Transform parent, string name, Color fillColor,
                                out Transform fill, out SpriteRenderer fillSprite, float width = 6f)
    {
        Visual(parent, name + "Track", LabKit.White, new Color32(0x1E, 0x24, 0x30, 0xFF),
               new Vector2(0f, 0f), new Vector2(width, 0.5f), 1);
        GameObject bar = Visual(parent, name + "Fill", LabKit.White, fillColor,
                                new Vector2(-width * 0.5f, 0f), new Vector2(0.1f, 1.1f), 2);
        fill = bar.transform;
        fillSprite = bar.GetComponent<SpriteRenderer>();
        return bar;
    }

    /// <summary>A camera for a demo that owns its own (camera demos, the platformer). No AudioListener:
    /// the scene's stage camera has the only one.</summary>
    public static Camera OwnCamera(Transform parent, string name, Vector3 pos, float size, bool orthographic)
    {
        GameObject go = LabKit.Child(parent, name, pos);
        Camera c = go.AddComponent<Camera>();
        c.orthographic = orthographic;
        c.orthographicSize = size;
        c.fieldOfView = 60f;
        c.clearFlags = CameraClearFlags.SolidColor;
        c.backgroundColor = Dark;
        c.nearClipPlane = -50f;
        c.farClipPlane = 100f;
        return c;
    }

    /// <summary>The little scene the camera demos look at: something on Ground, something on Player,
    /// so a Culling Mask or a Depth demo has two distinguishable layers.</summary>
    public static void CameraKit(Transform t, out SpriteRenderer groundSprite, out SpriteRenderer playerSprite)
    {
        GameObject ground = Floor(t, "KitGround", -2.5f, -2f, 4f, 0.6f, Orange);
        groundSprite = ground.GetComponentInChildren<SpriteRenderer>();
        GameObject player = Character(t, "KitPlayer", new Vector2(1.5f, -1.2f), Vector2.one, 2);
        player.layer = LabKit.Layer("Player");
        playerSprite = player.GetComponent<SpriteRenderer>();
    }
}
