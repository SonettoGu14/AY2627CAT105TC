using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Editor-side furniture for the Week 9 (audio) demos, the same job DemoKit does for the 2D weeks
/// and W08Kit does for the 3D week:
///
///   Clip("coin")                          an AudioClip from Assets/Audio/W09
///   Clips("footstep_dirt_1", ...)         several of them
///   Group("SFX")                          the mixer group to route a source into (null if the
///                                         mixer asset has not been created yet)
///   Camera(t)                             the demo's own orthographic camera (NO AudioListener -
///                                         the ears are a separate object, see below)
///   Listener(t, pos)                      the demo's ears: an AudioListener plus its marker
///   Source(...)                           a configured AudioSource at a position
///   Icon / Label / Ring / Meter / Slider  set dressing so a sound has something to point at
///
/// Why the camera and the ears are separate objects: every W09 demo owns a camera, so the shared
/// stage camera (and its AudioListener) is switched off while the demo runs. That leaves exactly
/// one listener, and it can sit somewhere other than the camera - which is the whole point of the
/// 3D demos, where you move the listening position and watch what happens to the mix.
///
/// The readout panel covers the bottom-left of the screen (880x380 at 28,28 of 1920x1080), so keep
/// the interesting part of a demo above y = -1.2 or to the right of x = 0.
/// </summary>
public static class W09Kit
{
    public const string AudioFolder = "Assets/Audio/W09";
    public const string MixerPath = AudioFolder + "/W09Mixer.mixer";

    /// <summary>Every W09 demo frames itself at this orthographic size, so the week looks consistent.</summary>
    public const float ViewSize = 5f;

    // ------------------------------------------------------------------ assets

    public static AudioClip Clip(string name)
    {
        string path = AudioFolder + "/" + name + ".wav";
        AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        if (clip == null) Debug.LogWarning("[W09Kit] missing audio clip " + path);
        return clip;
    }

    public static AudioClip[] Clips(params string[] names)
    {
        AudioClip[] clips = new AudioClip[names.Length];
        for (int i = 0; i < names.Length; i++) clips[i] = Clip(names[i]);
        return clips;
    }

    /// <summary>The mixer group with this name, or null (with a warning) when the mixer asset does
    /// not exist yet - demos must survive a null group, the builder is allowed to run before it.
    /// FindMatchingGroups matches on the path, so "Master" also matches "Master/BGM" and
    /// "Master/SFX"; prefer the group whose own name is an exact match.</summary>
    public static AudioMixerGroup Group(string name)
    {
        AudioMixer mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(MixerPath);
        if (mixer == null)
        {
            Debug.LogWarning("[W09Kit] no mixer at " + MixerPath + " - sources will bypass the mixer");
            return null;
        }
        AudioMixerGroup[] groups = mixer.FindMatchingGroups(name);
        if (groups == null || groups.Length == 0)
        {
            Debug.LogWarning("[W09Kit] the mixer has no group called '" + name + "'");
            return null;
        }
        for (int i = 0; i < groups.Length; i++)
            if (groups[i].name == name) return groups[i];
        return groups[0];
    }

    // ------------------------------------------------------------------ camera + ears

    /// <summary>The demo's own camera: orthographic, looking at the XY plane from z = -10. It has NO
    /// AudioListener on purpose - W09Kit.Listener makes the ears, and they can be moved on their own.</summary>
    public static Camera Camera(Transform t)
    {
        return DemoKit.OwnCamera(t, "Camera", new Vector3(0f, 0f, -10f), ViewSize, true);
    }

    /// <summary>The demo's ears: an object carrying the scene's only active AudioListener, with a
    /// visible marker so "the listener is here" is something you can see as well as hear.</summary>
    public static AudioListener Listener(Transform t, Vector3 pos, string label = "EARS")
    {
        GameObject go = LabKit.Child(t, "Listener", pos);
        AudioListener listener = go.AddComponent<AudioListener>();
        Icon(go.transform, "Marker", LabKit.Circle, new Color(0.55f, 0.78f, 0.98f, 0.9f),
             Vector3.zero, 0.45f, 20);
        Ring(go.transform, "Range", Vector3.zero, 0.75f, new Color(0.55f, 0.78f, 0.98f, 0.35f), 19);
        if (!string.IsNullOrEmpty(label)) Label(go.transform, label, new Vector3(0f, -0.85f, 0f), DemoKit.Dim);
        return listener;
    }

    // ------------------------------------------------------------------ sources

    /// <summary>A ready AudioSource. 3D sources use Logarithmic rolloff (the one the slide calls
    /// natural); doppler is off everywhere so moving a source does not bend its pitch.</summary>
    public static AudioSource Source(Transform t, string name, AudioClip clip, Vector3 pos,
                                     bool spatial, float minDistance, float maxDistance,
                                     AudioMixerGroup group, bool loop = false,
                                     float volume = 1f, bool playOnAwake = false)
    {
        GameObject go = LabKit.Child(t, name, pos);
        AudioSource s = go.AddComponent<AudioSource>();
        s.clip = clip;
        s.playOnAwake = playOnAwake;
        s.loop = loop;
        s.volume = volume;
        s.spatialBlend = spatial ? 1f : 0f;
        s.rolloffMode = AudioRolloffMode.Logarithmic;
        s.minDistance = minDistance;
        s.maxDistance = maxDistance;
        s.dopplerLevel = 0f;
        s.outputAudioMixerGroup = group;
        return s;
    }

    /// <summary>A source and its speaker icon in one call - the common case.</summary>
    public static AudioSource Emitter(Transform t, string name, string clipName, Vector3 pos,
                                      bool spatial, float minDistance, float maxDistance,
                                      AudioMixerGroup group, Color color, bool loop = false,
                                      float volume = 1f)
    {
        AudioSource s = Source(t, name, Clip(clipName), pos, spatial, minDistance, maxDistance, group, loop, volume);
        Icon(s.transform, "Speaker", LabKit.Circle, color, Vector3.zero, 0.6f, 10);
        Ring(s.transform, "Halo", Vector3.zero, 0.95f, new Color(color.r, color.g, color.b, 0.3f), 9);
        Label(s.transform, clipName, new Vector3(0f, 1.0f, 0f), color);
        return s;
    }

    // ------------------------------------------------------------------ set dressing

    public static GameObject Icon(Transform parent, string name, Sprite sprite, Color color,
                                  Vector3 pos, float size, int order)
    {
        GameObject go = DemoKit.Visual(parent, name, sprite, color, pos, new Vector2(size, size), order);
        return go;
    }

    public static GameObject Label(Transform parent, string text, Vector3 pos, Color color)
    {
        return LabKit.WorldLabel(parent, text, pos, color);
    }

    /// <summary>A thin circle, for "Min Distance" / "Max Distance" rings and for listener range.</summary>
    public static LineRenderer Ring(Transform parent, string name, Vector3 center, float radius,
                                    Color color, int order, int segments = 64)
    {
        GameObject go = LabKit.Child(parent, name, center);
        LineRenderer lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.loop = true;
        lr.positionCount = segments;
        lr.widthMultiplier = 0.055f;
        lr.startColor = color;
        lr.endColor = color;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.sortingOrder = order;
        lr.numCapVertices = 2;
        for (int i = 0; i < segments; i++)
        {
            float a = i * Mathf.PI * 2f / segments;
            lr.SetPosition(i, new Vector3(Mathf.Cos(a) * radius, Mathf.Sin(a) * radius, 0f));
        }
        return lr;
    }

    /// <summary>A straight line between two local points (the signal path in the Audio Map demo).</summary>
    public static LineRenderer Line(Transform parent, string name, Color color, float width, int order)
    {
        LineRenderer lr = DemoKit.Line(parent, name, order);
        lr.widthMultiplier = width;
        lr.startColor = color;
        lr.endColor = color;
        return lr;
    }

    /// <summary>A horizontal meter: a dark track and a fill that starts at zero width. Returns the
    /// fill transform to scale (see Fill() below) and its sprite renderer to recolour.</summary>
    public static GameObject Meter(Transform parent, string name, Vector3 pos, float width,
                                   Color color, out Transform fill, out SpriteRenderer fillSprite)
    {
        GameObject holder = LabKit.Child(parent, name, pos);
        GameObject bar = DemoKit.Bar(holder.transform, "Meter", color, out fill, out fillSprite, width);
        Fill(fill, 0f, width);
        return bar;
    }

    /// <summary>Set a meter fill to a 0..1 level. Keeps a sliver of geometry so "empty" still reads.
    /// The bar sprites are centre-pivoted, so the fill is re-centred on its own left edge - scaling
    /// it in place would grow it both ways and spill past the track.</summary>
    public static void Fill(Transform fill, float level, float width)
    {
        if (fill == null) return;
        float w = Mathf.Max(0.02f, Mathf.Clamp01(level) * width);
        fill.localScale = new Vector3(w, 0.32f, 1f);
        fill.localPosition = new Vector3(-width * 0.5f + w * 0.5f, 0f, 0f);
    }

    /// <summary>A horizontal slider the demo can move with the keyboard: track, fill and a knob.
    /// Returns the fill transform the demo scales; <paramref name="knob"/> marks the handle.</summary>
    public static GameObject Slider(Transform parent, string name, Vector3 pos, float width,
                                    Color color, out Transform fill, out Transform knob)
    {
        GameObject holder = LabKit.Child(parent, name, pos);
        SpriteRenderer fillSprite;
        GameObject bar = DemoKit.Bar(holder.transform, "Slider", color, out fill, out fillSprite, width);
        GameObject knobGo = DemoKit.Visual(holder.transform, "Knob", LabKit.Circle, Color.white,
                                           new Vector3(-width * 0.5f, 0f, 0f), new Vector2(0.55f, 0.55f), 4);
        knob = knobGo.transform;
        Fill(fill, 1f, width);
        return bar;
    }

    /// <summary>Move a slider's fill + knob to a 0..1 position.</summary>
    public static void SliderValue(Transform fill, Transform knob, float value, float width)
    {
        float v = Mathf.Clamp01(value);
        Fill(fill, v, width);
        if (knob != null) knob.localPosition = new Vector3(-width * 0.5f + v * width, 0f, 0f);
    }
}
