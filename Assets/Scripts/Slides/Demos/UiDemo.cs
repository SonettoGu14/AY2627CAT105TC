using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// W5 — the UI component demos. One Mode per knowledge point; the builder wires only the pieces a
/// mode uses, and each mode is interactive because the point of the slide is something you can poke.
///
/// Key rule: SlideInput owns the digit keys (they jump to a demo), so demos use Z / X / C for a
/// slide's three properties, M to cycle a mode, and never 1-9.
///
/// Buttons are wired in Awake, not by the builder: UnityEvent.AddListener creates a NON-persistent
/// listener, which is not serialized, so anything the editor builder wired would be gone on reload.
/// </summary>
public class UiDemo : DemoBase
{
    public enum Mode
    {
        VsScene,        // slide  9  UI floats above the scene; a scene object cannot cover it
        CanvasRequired, // slide 10  a Graphic draws only if a Canvas is one of its parents
        CanvasMode,     // slides 11-12  Overlay / Screen Space Camera / World Space
        Scaler,         // slide 13  CanvasScaler: Constant Pixel Size vs Scale With Screen Size
        Raycaster,      // slide 14  GraphicRaycaster is what lets the UI receive clicks
        Image,          // slide 15  Source Image / Color / Raycast Target
        Text,           // slide 16  font size / colour / alignment
        Button,         // slide 17  Normal / Highlighted / Pressed / Disabled + onClick
        Code,           // slide 18  change an Image and a Text from a button's onClick
        Slider,         // slide 25  value between 0 and 1
        Mask,           // slide 26  Mask vs RectMask2D
        Rect,           // slides 27-28  anchoredPosition and sizeDelta
        Pivot,          // slide 29  Pivot rotates, Anchor pins
        LerpMove,       // slide 21  Vector3.Lerp eases a UI element toward a target
        Fade,           // slide 30  fading a whole panel in
        Group,          // slide 31  Canvas Group: alpha / interactable / blocksRaycasts
    }

    public Mode mode = Mode.Image;

    // ---- wired by the builder (only the ones the mode uses) ----
    public Canvas canvas;
    public Camera cam;                  // CanvasMode / VsScene
    public SpriteRenderer worldSprite;  // VsScene
    public RectTransform panel;         // the demo's main panel
    public Image image;                 // Image / Code / Raycaster
    public Image meter;                 // Slider: the bar that follows the value
    public TMP_Text text;               // Text / Code
    public Button button;               // Button / Code / Raycaster / Group
    public UiPointerProbe probe;        // image / button click + hover reporting
    public Slider slider;               // Slider
    public CanvasGroup group;           // Fade / Group
    public RectTransform mover;         // Rect / Pivot / LerpMove
    public RectTransform detached;      // CanvasRequired: the Graphic with no Canvas above it
    public RectTransform clipA;         // Mask: child sliding through a Mask
    public RectTransform clipB;         // Mask: child sliding through a RectMask2D
    public Sprite[] sprites = new Sprite[0];

    private int pick;
    private int clicks;
    private int clicksAnywhere;
    private int modeIndex;              // CanvasMode
    private int scalerMode = 1;         // Scaler
    private int refIndex;
    private float pivotX = 0.5f, anchorX = 0.5f;
    private float rectX, rectY, rectW, rectH;
    private float targetX, targetY;     // LerpMove
    private float fadeTo = 1f;          // Fade
    private readonly float[] refWidths = { 1920f, 1280f, 960f };

    private void Awake()
    {
        if (button == null) return;
        button.onClick.AddListener(() => clicks++);
        if (mode == Mode.Code) button.onClick.AddListener(AdvanceCode);
    }

    /// <summary>Slide 18's demo: the button's onClick is what changes the Image and the Text.</summary>
    private void AdvanceCode()
    {
        pick = (pick + 1) % Mathf.Max(1, sprites.Length);
        ApplyImage(); ApplyImageColors();
        if (text != null) text.text = "Picture " + pick;
    }

    public override string Title
    {
        get
        {
            switch (mode)
            {
                case Mode.VsScene:        return "UI floats above the scene";
                case Mode.CanvasRequired: return "A Canvas is what makes UI draw";
                case Mode.CanvasMode:     return "Canvas render mode: Overlay / Camera / World";
                case Mode.Scaler:         return "Canvas Scaler adapts the UI to the screen";
                case Mode.Raycaster:      return "Graphic Raycaster is what receives the clicks";
                case Mode.Image:          return "Image: Source Image, Color, Raycast Target";
                case Mode.Text:           return "Which properties does a Text have?";
                case Mode.Button:         return "Button: what counts as clicking it?";
                case Mode.Code:           return "Changing UI from code";
                case Mode.Slider:         return "Slider: a value from 0 to 1";
                case Mode.Mask:           return "Mask clips the children to a shape";
                case Mode.Rect:           return "RectTransform: position and size";
                case Mode.Pivot:          return "Pivot rotates; Anchor pins";
                case Mode.LerpMove:       return "Vector3.Lerp eases toward a target";
                case Mode.Fade:           return "Fading a panel in";
                default:                  return "Canvas Group: alpha, interactable, raycasts";
            }
        }
    }

    public override string Keys
    {
        get
        {
            switch (mode)
            {
                case Mode.VsScene:        return "A / D  slide the scene object";
                case Mode.CanvasRequired: return "C  move the Graphic under the Canvas";
                case Mode.CanvasMode:     return "M  cycle the render mode";
                case Mode.Scaler:         return "M  cycle the scale mode        A / D  reference width";
                case Mode.Raycaster:      return "R  enable / disable the Graphic Raycaster";
                case Mode.Image:          return "Z  source image   X  colour   C  raycast target";
                case Mode.Text:           return "Z  font size   X  colour   C  alignment";
                case Mode.Button:         return "R  interactable        (hover and click it)";
                case Mode.Code:           return "(click the button)";
                case Mode.Slider:         return "(drag the slider)";
                case Mode.Mask:           return "A / D  slide the children through both frames";
                case Mode.Rect:           return "A / D  position X        W / S  height";
                case Mode.Pivot:          return "A / D  pivot X   Z / X  anchor X   R  rotate";
                case Mode.LerpMove:       return "Space  move the target";
                case Mode.Fade:           return "Space  fade in / out";
                default:                  return "Z  alpha   X  interactable   C  blocks raycasts";
            }
        }
    }

    public override void OnActivate()
    {
        clicks = 0; clicksAnywhere = 0; pick = 0;
        modeIndex = 0; scalerMode = 1; refIndex = 0;
        pivotX = 0.5f; anchorX = 0.5f;
        if (mover != null)
        {
            rectX = mover.anchoredPosition.x; rectY = mover.anchoredPosition.y;
            rectW = mover.sizeDelta.x; rectH = mover.sizeDelta.y;
            mover.localRotation = Quaternion.identity;
        }
        targetX = -340f; targetY = 0f;
        fadeTo = 1f;
        if (group != null) { group.alpha = 1f; group.interactable = true; group.blocksRaycasts = true; }
        ApplyCanvasMode(); ApplyScaler(); ApplyImage(); ApplyImageColors(); ApplyPivot();
        if (mode == Mode.Rect && mover != null) { mover.anchoredPosition = new Vector2(rectX, rectY); mover.sizeDelta = new Vector2(rectW, rectH); }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) clicksAnywhere++;

        switch (mode)
        {
            case Mode.VsScene:
                if (worldSprite != null)
                {
                    float dx = Mathf.Sin(Time.unscaledTime * 0.6f) * 0.035f;
                    if (Input.GetKey(KeyCode.A)) dx = -0.06f;
                    if (Input.GetKey(KeyCode.D)) dx = 0.06f;
                    Vector3 p = worldSprite.transform.localPosition;
                    p.x = Mathf.Clamp(p.x + dx, -5.5f, 5.5f);
                    worldSprite.transform.localPosition = p;
                }
                break;

            case Mode.CanvasRequired:
                if (Input.GetKeyDown(KeyCode.C) && detached != null)
                    detached.SetParent(detached.parent == canvas.transform ? transform : canvas.transform, false);
                break;

            case Mode.CanvasMode:
                if (Input.GetKeyDown(KeyCode.M)) { modeIndex = (modeIndex + 1) % 3; ApplyCanvasMode(); }
                break;

            case Mode.Scaler:
                if (Input.GetKeyDown(KeyCode.M)) scalerMode = (scalerMode + 1) % 2;
                if (Input.GetKeyDown(KeyCode.A)) refIndex = (refIndex + refWidths.Length - 1) % refWidths.Length;
                if (Input.GetKeyDown(KeyCode.D)) refIndex = (refIndex + 1) % refWidths.Length;
                ApplyScaler();
                break;

            case Mode.Raycaster:
                if (Input.GetKeyDown(KeyCode.R) && canvas != null)
                {
                    GraphicRaycaster g = canvas.GetComponent<GraphicRaycaster>();
                    if (g != null) g.enabled = !g.enabled;
                }
                break;

            case Mode.Image:
                if (Input.GetKeyDown(KeyCode.Z)) { pick = (pick + 1) % 3; ApplyImage(); }
                if (Input.GetKeyDown(KeyCode.X)) { pick = (pick + 1) % 3; ApplyImageColors(); }
                if (Input.GetKeyDown(KeyCode.C) && image != null) image.raycastTarget = !image.raycastTarget;
                break;

            case Mode.Text:
                if (Input.GetKeyDown(KeyCode.Z) && text != null)
                    text.fontSize = text.fontSize >= 48f ? 24f : text.fontSize + 12f;
                if (Input.GetKeyDown(KeyCode.X) && text != null)
                {
                    pick = (pick + 1) % 3;
                    text.color = pick == 0 ? UiPalette.Paper : pick == 1 ? UiPalette.Gold : UiPalette.Green;
                }
                if (Input.GetKeyDown(KeyCode.C) && text != null)
                    text.alignment = text.alignment == TextAlignmentOptions.TopLeft ? TextAlignmentOptions.Center
                                   : text.alignment == TextAlignmentOptions.Center ? TextAlignmentOptions.TopRight
                                   : TextAlignmentOptions.TopLeft;
                break;

            case Mode.Button:
                if (Input.GetKeyDown(KeyCode.R) && button != null) button.interactable = !button.interactable;
                break;

            case Mode.Slider:
                if (meter != null && slider != null)
                    meter.rectTransform.sizeDelta = new Vector2(Mathf.Max(2f, slider.value * 420f), meter.rectTransform.sizeDelta.y);
                break;

            case Mode.Mask:
                float mdx = 0f;
                if (Input.GetKey(KeyCode.A)) mdx = -1.6f;
                if (Input.GetKey(KeyCode.D)) mdx = 1.6f;
                if (mdx != 0f)
                {
                    if (clipA != null) { Vector2 a = clipA.anchoredPosition; a.x = Mathf.Clamp(a.x + mdx, -160f, 160f); clipA.anchoredPosition = a; }
                    if (clipB != null) { Vector2 b = clipB.anchoredPosition; b.x = Mathf.Clamp(b.x + mdx, -160f, 160f); clipB.anchoredPosition = b; }
                }
                break;

            case Mode.Rect:
                if (mover != null)
                {
                    if (Input.GetKey(KeyCode.A)) rectX -= 3f;
                    if (Input.GetKey(KeyCode.D)) rectX += 3f;
                    if (Input.GetKey(KeyCode.W)) rectH += 3f;
                    if (Input.GetKey(KeyCode.S)) rectH -= 3f;
                    rectX = Mathf.Clamp(rectX, -400f, 400f); rectH = Mathf.Clamp(rectH, 40f, 400f);
                    mover.anchoredPosition = new Vector2(rectX, rectY);
                    mover.sizeDelta = new Vector2(rectW, rectH);
                }
                break;

            case Mode.Pivot:
                if (Input.GetKeyDown(KeyCode.A)) pivotX = Mathf.Clamp(pivotX - 0.25f, 0f, 1f);
                if (Input.GetKeyDown(KeyCode.D)) pivotX = Mathf.Clamp(pivotX + 0.25f, 0f, 1f);
                if (Input.GetKeyDown(KeyCode.Z)) anchorX = Mathf.Clamp(anchorX - 0.25f, 0f, 1f);
                if (Input.GetKeyDown(KeyCode.X)) anchorX = Mathf.Clamp(anchorX + 0.25f, 0f, 1f);
                if (Input.GetKeyDown(KeyCode.R) && mover != null)
                    mover.localRotation = Quaternion.Euler(0f, 0f, mover.localRotation.eulerAngles.z > 1f ? 0f : 30f);
                ApplyPivot();
                break;

            case Mode.LerpMove:
                if (Input.GetKeyDown(KeyCode.Space)) targetX = -targetX;
                if (mover != null)
                    mover.anchoredPosition = Vector3.Lerp(mover.anchoredPosition, new Vector3(targetX, targetY, 0f), 0.06f);
                break;

            case Mode.Fade:
                if (Input.GetKeyDown(KeyCode.Space)) fadeTo = fadeTo > 0.5f ? 0f : 1f;
                if (group != null) group.alpha = Mathf.Lerp(group.alpha, fadeTo, 0.06f);
                break;

            case Mode.Group:
                if (Input.GetKeyDown(KeyCode.Z) && group != null) group.alpha = group.alpha > 0.5f ? 0.35f : 1f;
                if (Input.GetKeyDown(KeyCode.X) && group != null) group.interactable = !group.interactable;
                if (Input.GetKeyDown(KeyCode.C) && group != null) group.blocksRaycasts = !group.blocksRaycasts;
                break;
        }
    }

    // ------------------------------------------------------------------ apply helpers

    private void ApplyCanvasMode()
    {
        if (canvas == null) return;
        switch (modeIndex)
        {
            case 0:
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.transform.localPosition = Vector3.zero;
                canvas.transform.localScale = Vector3.one;
                break;
            case 1:
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = cam;
                canvas.planeDistance = 8f;
                canvas.transform.localPosition = Vector3.zero;
                canvas.transform.localScale = Vector3.one;
                break;
            default:
                canvas.renderMode = RenderMode.WorldSpace;
                canvas.worldCamera = cam;
                canvas.transform.localPosition = new Vector3(0f, 0f, 8f);
                canvas.transform.localScale = Vector3.one * 0.012f;
                break;
        }
    }

    private void ApplyScaler()
    {
        if (canvas == null) return;
        CanvasScaler sc = canvas.GetComponent<CanvasScaler>();
        if (sc == null) return;
        sc.uiScaleMode = scalerMode == 0 ? CanvasScaler.ScaleMode.ConstantPixelSize
                                          : CanvasScaler.ScaleMode.ScaleWithScreenSize;
        sc.referenceResolution = new Vector2(refWidths[refIndex], refWidths[refIndex] * 0.5625f);
    }

    private void ApplyImage()
    {
        if (image == null || sprites.Length < 3) return;
        image.sprite = sprites[Mathf.Clamp(pick, 0, sprites.Length - 1)];
    }

    private void ApplyImageColors()
    {
        if (image == null) return;
        image.color = pick == 0 ? Color.white : pick == 1 ? UiPalette.Gold : UiPalette.Green;
    }

    private void ApplyPivot()
    {
        if (mover == null) return;
        mover.pivot = new Vector2(pivotX, 0.5f);
        mover.anchorMin = mover.anchorMax = new Vector2(anchorX, 0.5f);
    }

    // ------------------------------------------------------------------ the readout

    public override void Readout(List<string> lines)
    {
        switch (mode)
        {
            case Mode.VsScene:
                lines.Add("The character is drawn by the CAMERA, in the world.");
                lines.Add("The panel is drawn by a CANVAS, above everything.");
                lines.Add("");
                lines.Add("sprite x = " + F(worldSprite != null ? worldSprite.transform.localPosition.x : 0f, 2));
                lines.Add("-> it slides behind the panel and is hidden. A scene");
                lines.Add("   object can never cover UI.");
                break;

            case Mode.CanvasRequired:
                bool under = detached != null && detached.parent == canvas.transform;
                lines.Add("A Graphic draws only if a Canvas is one of its parents.");
                lines.Add("");
                lines.Add("graphics on screen        = " + (under ? 2 : 1));
                lines.Add("detached Graphic's parent = " + (detached == null ? "(none)"
                          : detached.parent == canvas.transform ? "Canvas" : "not a Canvas"));
                lines.Add("detached Graphic drawn?   = " + B(under));
                break;

            case Mode.CanvasMode:
                string[] names = { "ScreenSpaceOverlay", "ScreenSpaceCamera", "WorldSpace" };
                lines.Add("canvas.renderMode = " + names[modeIndex]);
                lines.Add("");
                if (modeIndex == 0) lines.Add("Glued to the screen. The sprite can never cover it.");
                else if (modeIndex == 1) lines.Add("Drawn by the camera - the sprite CAN cover it now.");
                else lines.Add("It lives in the world: it has a position and can be walked past.");
                break;

            case Mode.Scaler:
                lines.Add("uiScaleMode         = " + (scalerMode == 0 ? "ConstantPixelSize" : "ScaleWithScreenSize"));
                lines.Add("referenceResolution = (" + F(refWidths[refIndex], 0) + ", " + F(refWidths[refIndex] * 0.5625f, 0) + ")");
                lines.Add("canvas.scaleFactor  = " + F(canvas != null ? canvas.scaleFactor : 0f, 2));
                lines.Add("");
                lines.Add("ConstantPixelSize ignores the screen. ScaleWithScreenSize");
                lines.Add("scales by screen/reference - a smaller reference makes");
                lines.Add("everything bigger. That is the multi-device story.");
                break;

            case Mode.Raycaster:
                GraphicRaycaster g = canvas != null ? canvas.GetComponent<GraphicRaycaster>() : null;
                bool on = g == null || g.enabled;
                lines.Add("GraphicRaycaster enabled = " + B(on));
                lines.Add("button onClick count     = " + clicks);
                lines.Add("");
                lines.Add(on ? "It is what turns a click into a UI event."
                            : "-> with it off, clicks no longer reach the UI at all.");
                break;

            case Mode.Image:
                if (image != null)
                {
                    lines.Add("image.sprite        = " + (image.sprite != null ? image.sprite.name : "(none)"));
                    lines.Add("image.color         = " + Col(image.color));
                    lines.Add("image.raycastTarget = " + B(image.raycastTarget));
                }
                lines.Add("");
                lines.Add("clicks received by the image = " + (probe != null ? probe.Clicks : 0));
                lines.Add("clicks anywhere              = " + clicksAnywhere);
                if (image != null && !image.raycastTarget)
                    lines.Add("-> the click passes straight through it.");
                break;

            case Mode.Text:
                if (text != null)
                {
                    lines.Add("fontSize  = " + F(text.fontSize, 0));
                    lines.Add("color     = " + Col(text.color));
                    lines.Add("alignment = " + text.alignment);
                }
                lines.Add("");
                lines.Add("Unity's Text vs TextMeshPro: TMP shapes glyphs better");
                lines.Add("and stays sharp when scaled. This deck uses TMP.");
                break;

            case Mode.Button:
                lines.Add("state         = " + (probe != null ? probe.State(button) : "?"));
                lines.Add("interactable  = " + B(button == null || button.IsInteractable()));
                lines.Add("onClick count = " + clicks);
                lines.Add("");
                lines.Add("A Button is an Image with a raycast target plus this");
                lines.Add("component. Hover it, click it, then press R.");
                break;

            case Mode.Code:
                lines.Add("void OnClick()");
                lines.Add("{");
                lines.Add("    image.sprite = sprites[i];");
                lines.Add("    text.text    = \"Picture \" + i;");
                lines.Add("}");
                lines.Add("");
                lines.Add("i = " + pick + "        (click the button)");
                break;

            case Mode.Slider:
                lines.Add("slider.value = " + F(slider != null ? slider.value : 0f, 2) + "    (min 0, max 1)");
                lines.Add("wholeNumbers = " + B(slider != null && slider.wholeNumbers));
                lines.Add("");
                lines.Add("Background / Fill / Handle are ordinary Images; the");
                lines.Add("Slider drives their anchors from value.");
                break;

            case Mode.Mask:
                lines.Add("frame A = Mask        (clips to the sprite's shape)");
                lines.Add("frame B = RectMask2D  (clips to the rectangle)");
                lines.Add("");
                lines.Add("Both children are sliding right. In frame A the circle");
                lines.Add("is cut to a disc; in frame B it is cut to a box.");
                break;

            case Mode.Rect:
                lines.Add("anchoredPosition = (" + F(rectX, 0) + ", " + F(rectY, 0) + ")");
                lines.Add("sizeDelta        = (" + F(rectW, 0) + ", " + F(rectH, 0) + ")");
                lines.Add("");
                lines.Add("RectTransform is Transform plus a rectangle: it adds");
                lines.Add("Pivot, Anchor, Width and Height.");
                break;

            case Mode.Pivot:
                lines.Add("pivot     = (" + F(pivotX, 2) + ", 0.50)");
                lines.Add("anchorMin = (" + F(anchorX, 2) + ", 0.50)");
                lines.Add("anchorMax = (" + F(anchorX, 2) + ", 0.50)");
                lines.Add("");
                lines.Add("Press R: the box turns around its PIVOT, not its centre.");
                lines.Add("The ANCHOR decides which point of the parent it is");
                lines.Add("pinned to, so moving it moves the box.");
                break;

            case Mode.LerpMove:
                lines.Add("target                = (" + F(targetX, 0) + ", " + F(targetY, 0) + ")");
                lines.Add("rect.anchoredPosition = " + (mover != null ? V(mover.anchoredPosition) : "?"));
                lines.Add("");
                lines.Add("anchoredPosition = Vector3.Lerp(anchoredPosition,");
                lines.Add("                                  target, 0.06f);");
                lines.Add("It closes 6% of the gap each frame, so it eases out.");
                break;

            case Mode.Fade:
                lines.Add("group.alpha = " + F(group != null ? group.alpha : 0f, 2));
                lines.Add("");
                lines.Add("One number fades the whole panel. This is how a");
                lines.Add("dialogue box or a menu slides in.");
                break;

            default:
                lines.Add("alpha          = " + F(group != null ? group.alpha : 0f, 2));
                lines.Add("interactable   = " + B(group != null && group.interactable));
                lines.Add("blocksRaycasts = " + B(group != null && group.blocksRaycasts));
                lines.Add("");
                lines.Add("Alpha is the one you will use most; interactable and");
                lines.Add("blocksRaycasts switch a whole panel off cleanly.");
                lines.Add("button onClick count = " + clicks);
                break;
        }
    }
}
