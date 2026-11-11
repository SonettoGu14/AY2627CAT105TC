using System.Collections.Generic;
using UnityEngine;


// ==============================================================================================
//  THE AUDIO MAP: clip -> source -> listener, and source -> mixer -> master. Two dots travel the
//  two paths; 1 / 2 highlights one path, P plays the coin again.
// ==============================================================================================

public class AudioMapDemo : AudioDemoBase
{
    public SpriteRenderer nodeClip;
    public SpriteRenderer nodeSource;
    public SpriteRenderer nodeListener;
    public SpriteRenderer nodeMixer;
    public SpriteRenderer nodeMaster;

    public LineRenderer linkClipSource;
    public LineRenderer linkSourceListener;
    public LineRenderer linkSourceMixer;
    public LineRenderer linkMixerMaster;

    public Transform dotOne;
    public Transform dotTwo;
    public Vector3[] pathOne;
    public Vector3[] pathTwo;
    public float travelTime = 1.1f;

    private int path;
    private float travel = -1f;

    private bool cached;
    private Color baseClip, baseSource, baseListener, baseMixer, baseMaster;
    private Color baseLink1, baseLink2, baseLink3, baseLink4;

    public override string Title { get { return "The audio map: four pieces, two paths"; } }
    public override string Keys { get { return "P play    C clip path    M mixer path"; } }

    public override void OnActivate()
    {
        CacheColours();
        path = 0;
        travel = -1f;
        ApplyPath();
        SetDots(false);
        PlayCoin();
    }

    public override void OnDeactivate()
    {
        if (source != null) source.Stop();
        travel = -1f;
        SetDots(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P)) PlayCoin();
        if (Input.GetKeyDown(KeyCode.C)) { path = path == 1 ? 0 : 1; ApplyPath(); }
        if (Input.GetKeyDown(KeyCode.M)) { path = path == 2 ? 0 : 2; ApplyPath(); }

        if (travel >= 0f)
        {
            travel += Time.deltaTime;
            float u = Mathf.Clamp01(travel / travelTime);
            SetDots(true);
            if (dotOne != null) dotOne.position = Along(pathOne, u);
            if (dotTwo != null) dotTwo.position = Along(pathTwo, u);
            if (travel > travelTime) { travel = -1f; SetDots(false); }
        }
    }

    private void PlayCoin()
    {
        if (source != null && source.clip != null) source.PlayOneShot(source.clip);
        travel = 0f;
    }

    private void SetDots(bool on)
    {
        if (dotOne != null) dotOne.gameObject.SetActive(on);
        if (dotTwo != null) dotTwo.gameObject.SetActive(on);
    }

    /// <summary>Position along a polyline of world points, u in 0..1.</summary>
    private static Vector3 Along(Vector3[] points, float u)
    {
        if (points == null || points.Length == 0) return Vector3.zero;
        if (points.Length == 1) return points[0];
        float seg = Mathf.Clamp01(u) * (points.Length - 1);
        int i = Mathf.Clamp(Mathf.FloorToInt(seg), 0, points.Length - 2);
        return Vector3.Lerp(points[i], points[i + 1], seg - i);
    }

    private void CacheColours()
    {
        if (cached) return;
        cached = true;
        baseClip = NodeColour(nodeClip);
        baseSource = NodeColour(nodeSource);
        baseListener = NodeColour(nodeListener);
        baseMixer = NodeColour(nodeMixer);
        baseMaster = NodeColour(nodeMaster);
        baseLink1 = LinkColour(linkClipSource);
        baseLink2 = LinkColour(linkSourceListener);
        baseLink3 = LinkColour(linkSourceMixer);
        baseLink4 = LinkColour(linkMixerMaster);
    }

    private static Color NodeColour(SpriteRenderer sr) { return sr != null ? sr.color : Color.white; }
    private static Color LinkColour(LineRenderer lr) { return lr != null ? lr.startColor : Color.white; }

    private void ApplyPath()
    {
        if (!cached) return;
        Color dim = UiPalette.Muted;
        switch (path)
        {
            case 1:
                SetNode(nodeClip, baseClip, UiPalette.Gold);
                SetNode(nodeSource, baseSource, UiPalette.Gold);
                SetNode(nodeListener, baseListener, UiPalette.Gold);
                SetNode(nodeMixer, baseMixer, dim);
                SetNode(nodeMaster, baseMaster, dim);
                SetLink(linkClipSource, baseLink1, UiPalette.Gold);
                SetLink(linkSourceListener, baseLink2, UiPalette.Gold);
                SetLink(linkSourceMixer, baseLink3, dim);
                SetLink(linkMixerMaster, baseLink4, dim);
                break;
            case 2:
                SetNode(nodeClip, baseClip, dim);
                SetNode(nodeSource, baseSource, UiPalette.Accent);
                SetNode(nodeListener, baseListener, dim);
                SetNode(nodeMixer, baseMixer, UiPalette.Accent);
                SetNode(nodeMaster, baseMaster, UiPalette.Accent);
                SetLink(linkClipSource, baseLink1, dim);
                SetLink(linkSourceListener, baseLink2, dim);
                SetLink(linkSourceMixer, baseLink3, UiPalette.Accent);
                SetLink(linkMixerMaster, baseLink4, UiPalette.Accent);
                break;
            default:
                SetNode(nodeClip, baseClip, baseClip);
                SetNode(nodeSource, baseSource, baseSource);
                SetNode(nodeListener, baseListener, baseListener);
                SetNode(nodeMixer, baseMixer, baseMixer);
                SetNode(nodeMaster, baseMaster, baseMaster);
                SetLink(linkClipSource, baseLink1, baseLink1);
                SetLink(linkSourceListener, baseLink2, baseLink2);
                SetLink(linkSourceMixer, baseLink3, baseLink3);
                SetLink(linkMixerMaster, baseLink4, baseLink4);
                break;
        }
    }

    private static void SetNode(SpriteRenderer sr, Color baseColour, Color target)
    {
        if (sr != null) sr.color = target;
    }

    private static void SetLink(LineRenderer lr, Color baseColour, Color target)
    {
        if (lr == null) return;
        lr.startColor = target;
        lr.endColor = target;
    }

    public override void Readout(List<string> lines)
    {
        lines.Add("CLIP      the recorded waveform (coin.wav)");
        lines.Add("SOURCE    plays it at a point, with a volume");
        lines.Add("LISTENER  turns that into two ears");
        lines.Add("MIXER -> MASTER   volume and tone, then out");
        lines.Add(path == 1
            ? "path 1: clip -> source -> listener"
            : path == 2
                ? "path 2: source -> mixer -> master"
                : "press C or M to light up one path");
    }
}
