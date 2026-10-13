using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Slides 23-27, one camera concept per demo:
///   Projection  - Perspective vs Orthographic
///   Fov         - the perspective Field of View
///   OrthoSize   - the orthographic Size
///   ClearFlags  - Skybox / SolidColor / DepthOnly / Don't Clear
///   Background  - the background colour
///   CullingMask - which layers one camera draws
///   Depth       - which of two cameras draws on top
///   Follow      - following by keeping an offset
///   FollowSmooth- the "better way": smoothing the follow (split screen: instant vs smooth)
///
/// Each camera demo owns its own camera (built inside the demo root), so the stage camera steps
/// aside and the demo is free to be the camera under discussion.
/// </summary>
public class CameraDemo : DemoBase
{
    public enum Mode { Projection, Fov, OrthoSize, ClearFlags, Background, CullingMask, Depth, Follow, FollowSmooth }

    public Mode mode = Mode.Projection;

    public Camera cam;            // the camera this demo talks about
    public Camera backCam;        // ClearFlags / Depth need a second camera to show the difference
    public Transform target;      // Follow / FollowSmooth
    public Transform mover;       // the object the keys move
    public Camera smoothCam;      // FollowSmooth: the second, smoothed camera
    public Transform smoothPivot;

    private readonly int[] clearCycle = { 1, 2, 3, 4 };   // Skybox, SolidColor, DepthOnly, DontClear
    private int clearIndex;
    private int maskMode;
    private Vector3 followOffset;
    private Vector3 smoothVelocity;

    public override string Title
    {
        get
        {
            switch (mode)
            {
                case Mode.Projection: return "Perspective vs Orthographic";
                case Mode.Fov: return "Field of View is the width of the perspective frustum";
                case Mode.OrthoSize: return "Orthographic Size is half the view height";
                case Mode.ClearFlags: return "Clear Flags: what the camera shows where nothing is drawn";
                case Mode.Background: return "Background Color fills the empty area";
                case Mode.CullingMask: return "Culling Mask decides which layers a camera renders";
                case Mode.Depth: return "Depth orders the cameras; the higher depth draws on top";
                case Mode.Follow: return "the camera follows by keeping an offset";
                default: return "the better way: smoothing the follow";
            }
        }
    }

    public override string Keys
    {
        get
        {
            switch (mode)
            {
                case Mode.Projection: return "P  toggle Perspective / Orthographic";
                case Mode.Fov: return "W / S  change the Field of View";
                case Mode.OrthoSize: return "W / S  change the Size";
                case Mode.ClearFlags: return "Space  cycle Skybox / Solid Color / Depth Only / Don't Clear";
                case Mode.Background: return "R / G / B  change the background colour";
                case Mode.CullingMask: return "M  add / remove the Ground layer from the mask";
                case Mode.Depth: return "D  swap the two depths";
                case Mode.Follow: return "A / D  move the character";
                default: return "A / D  move the character      (left = instant, right = smoothed)";
            }
        }
    }

    public override void OnActivate()
    {
        if (mover != null) followOffset = cam != null ? cam.transform.position - mover.position : Vector3.zero;
        if (mode == Mode.ClearFlags && backCam != null) backCam.gameObject.SetActive(true);
        if (mode == Mode.Depth && backCam != null) backCam.gameObject.SetActive(true);
        if (mode == Mode.FollowSmooth && smoothCam != null) smoothCam.gameObject.SetActive(true);
    }

    public override void OnDeactivate()
    {
        if (backCam != null) backCam.gameObject.SetActive(false);
        if (smoothCam != null) smoothCam.gameObject.SetActive(false);
    }

    private void Update()
    {
        switch (mode)
        {
            case Mode.Projection:
                if (Input.GetKeyDown(KeyCode.P) && cam != null) cam.orthographic = !cam.orthographic;
                break;

            case Mode.Fov:
                if (cam != null)
                {
                    if (Input.GetKey(KeyCode.W)) cam.fieldOfView += 25f * Time.deltaTime;
                    if (Input.GetKey(KeyCode.S)) cam.fieldOfView -= 25f * Time.deltaTime;
                    cam.fieldOfView = Mathf.Clamp(cam.fieldOfView, 15f, 100f);
                }
                break;

            case Mode.OrthoSize:
                if (cam != null)
                {
                    if (Input.GetKey(KeyCode.W)) cam.orthographicSize += 2.5f * Time.deltaTime;
                    if (Input.GetKey(KeyCode.S)) cam.orthographicSize -= 2.5f * Time.deltaTime;
                    cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, 1.5f, 9f);
                }
                break;

            case Mode.ClearFlags:
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    clearIndex = (clearIndex + 1) % clearCycle.Length;
                    if (cam != null) cam.clearFlags = (CameraClearFlags)clearCycle[clearIndex];
                }
                break;

            case Mode.Background:
                if (cam != null)
                {
                    Color c = cam.backgroundColor;
                    if (Input.GetKey(KeyCode.R)) c.r += 0.5f * Time.deltaTime;
                    if (Input.GetKey(KeyCode.G)) c.g += 0.5f * Time.deltaTime;
                    if (Input.GetKey(KeyCode.B)) c.b += 0.5f * Time.deltaTime;
                    cam.backgroundColor = new Color(Mathf.Clamp01(c.r), Mathf.Clamp01(c.g), Mathf.Clamp01(c.b), 1f);
                }
                break;

            case Mode.CullingMask:
                if (Input.GetKeyDown(KeyCode.M))
                {
                    maskMode = (maskMode + 1) % 3;
                    ApplyMask();
                }
                break;

            case Mode.Depth:
                if (Input.GetKeyDown(KeyCode.D) && cam != null && backCam != null)
                {
                    float a = cam.depth;
                    cam.depth = backCam.depth;
                    backCam.depth = a;
                }
                break;
        }

        if (mover != null && (mode == Mode.Follow || mode == Mode.FollowSmooth))
        {
            float h = Input.GetAxisRaw("Horizontal");
            Vector3 p = mover.position;
            p.x += h * 6f * Time.deltaTime;
            p.x = Mathf.Clamp(p.x, -6f, 6f);
            mover.position = p;
        }
    }

    private void LateUpdate()
    {
        if (cam == null || mover == null) return;

        if (mode == Mode.Follow)
        {
            cam.transform.position = mover.position + followOffset;
        }
        else if (mode == Mode.FollowSmooth)
        {
            if (smoothPivot != null) smoothPivot.position = mover.position + followOffset;
            if (smoothCam != null)
            {
                Vector3 want = mover.position + followOffset;
                smoothCam.transform.position = Vector3.SmoothDamp(smoothCam.transform.position, want, ref smoothVelocity, 0.35f);
            }
        }
    }

    private void ApplyMask()
    {
        if (cam == null) return;
        int ground = LayerMask.NameToLayer("Ground");
        int all = cam.cullingMask;
        if (maskMode == 0) cam.cullingMask = ~0;
        else if (maskMode == 1) cam.cullingMask = all & ~(1 << ground);
        else cam.cullingMask = 1 << ground;
    }

    public override void Readout(List<string> lines)
    {
        switch (mode)
        {
            case Mode.Projection:
                if (cam == null) return;
                lines.Add("cam.orthographic = " + B(cam.orthographic));
                if (cam.orthographic) lines.Add("cam.orthographicSize = " + F(cam.orthographicSize, 1));
                else lines.Add("cam.fieldOfView      = " + F(cam.fieldOfView, 1));
                lines.Add("");
                lines.Add("Perspective: near objects look bigger (a cone).");
                lines.Add("Orthographic: size never depends on distance");
                lines.Add("(a rectangular prism) - what 2D games use.");
                break;

            case Mode.Fov:
                if (cam == null) return;
                lines.Add("cam.fieldOfView = " + F(cam.fieldOfView, 1) + " deg");
                lines.Add("");
                lines.Add("Only meaningful in Perspective. A wider FOV");
                lines.Add("fits more of the world into the same screen.");
                break;

            case Mode.OrthoSize:
                if (cam == null) return;
                lines.Add("cam.orthographicSize = " + F(cam.orthographicSize, 1));
                lines.Add("");
                lines.Add("This is HALF the visible height, in world units.");
                lines.Add("At 16:9 the visible width is 2 * size * 16/9.");
                break;

            case Mode.ClearFlags:
                if (cam == null) return;
                lines.Add("cam.clearFlags = " + cam.clearFlags);
                lines.Add("");
                lines.Add("Skybox      - draw the skybox behind everything");
                lines.Add("Solid Color - fill with Background Color");
                lines.Add("Depth Only  - clear depth only, so a lower-depth");
                lines.Add("              camera's picture shows through");
                lines.Add("Don't Clear - keep the previous frame");
                break;

            case Mode.Background:
                if (cam == null) return;
                lines.Add("cam.backgroundColor = " + Col(cam.backgroundColor));
                lines.Add("cam.clearFlags      = " + cam.clearFlags);
                lines.Add("");
                lines.Add("Background Color is only used when Clear Flags");
                lines.Add("is Solid Color.");
                break;

            case Mode.CullingMask:
                if (cam == null) return;
                lines.Add("cam.cullingMask = 0x" + cam.cullingMask.ToString("X8"));
                lines.Add("mode            = " + (maskMode == 0 ? "everything" : maskMode == 1 ? "everything except Ground" : "only Ground"));
                lines.Add("");
                lines.Add("orange = Ground layer, blue = Player layer.");
                lines.Add("The camera draws only what its mask allows.");
                break;

            case Mode.Depth:
                if (cam == null || backCam == null) return;
                lines.Add("camera A (orange tint) depth = " + F(cam.depth, 1));
                lines.Add("camera B (blue tint)   depth = " + F(backCam.depth, 1));
                lines.Add("on top                       = " + (cam.depth >= backCam.depth ? "A" : "B"));
                lines.Add("");
                lines.Add("Depth only picks the draw order; it does not");
                lines.Add("move the cameras.");
                break;

            case Mode.Follow:
                lines.Add("offset = " + V(followOffset));
                lines.Add("target = " + V(mover != null ? mover.position : Vector3.zero));
                lines.Add("camera = " + V(cam != null ? cam.transform.position : Vector3.zero));
                lines.Add("");
                lines.Add("Record the offset once, then camera.position =");
                lines.Add("target.position + offset every LateUpdate.");
                break;

            default:
                lines.Add("target = " + V(mover != null ? mover.position : Vector3.zero));
                lines.Add("left  (instant) x = " + F(cam != null ? cam.transform.position.x : 0f, 2));
                lines.Add("right (smooth)  x = " + F(smoothCam != null ? smoothCam.transform.position.x : 0f, 2));
                lines.Add("");
                lines.Add("Same offset, but the right camera lags behind");
                lines.Add("with Vector3.SmoothDamp. Run in LateUpdate so");
                lines.Add("it reads the target's final position.");
                break;
        }
    }
}
