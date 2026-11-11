using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Slide 3-6 concept demos: WHY SOUND MATTERS, THE AUDIO MAP, PLAY VS PLAYONESHOT, pitch,
/// spatial blend, min/max distance and rolloff. One Build method per demo, each furnishing its own
/// root with W09Kit and adding a DemoBase component (see
/// Assets/Scripts/Slides/Demos/W09/ConceptAudioDemos.cs).
///
/// Contract (same as every week): camera first, then the ears, then sources, then set dressing,
/// then the component with its public fields wired. The roots are inactive while the deck is up.
/// </summary>
internal static class W09ConceptDemos
{
    // ---------------------------------------------------------------- 1. why sound

    public static void BuildWhySound(Transform t)
    {
        W09Kit.Camera(t);
        AudioMixerGroup sfx = W09Kit.Group("SFX");

        AudioListener ears = W09Kit.Listener(t, new Vector3(0f, -0.4f, 0f));
        AudioSource coin = W09Kit.Source(t, "CoinSource", W09Kit.Clip("coin"),
            new Vector3(-3.6f, 0.9f, 0f), true, 2f, 12f, sfx);
        AudioSource fire = W09Kit.Source(t, "FireSource", W09Kit.Clip("fire_loop"),
            new Vector3(3.6f, 0.9f, 0f), true, 2f, 12f, sfx, true, 0.55f);

        // Two big dark panels hide both emitters (icons, halos and labels) from the screen.
        Color fog = new Color(0.07f, 0.09f, 0.14f, 1f);
        SpriteRenderer left = Panel(t, "FogLeft", new Vector3(-3.6f, 0.9f, 0f), new Vector2(3.4f, 3.4f), fog, 300);
        SpriteRenderer right = Panel(t, "FogRight", new Vector3(3.6f, 0.9f, 0f), new Vector2(3.4f, 3.4f), fog, 300);

        WhySoundDemo demo = t.gameObject.AddComponent<WhySoundDemo>();
        demo.source = coin;
        demo.secondSource = fire;
        demo.listener = ears;
        demo.mixerGroup = sfx;
        demo.fogLeft = left;
        demo.fogRight = right;
    }

    // ---------------------------------------------------------------- 2. the audio map

    public static void BuildAudioMap(Transform t)
    {
        W09Kit.Camera(t);
        AudioMixerGroup sfx = W09Kit.Group("SFX");

        Vector3 clipPos = new Vector3(-6.6f, 1.7f, 0f);
        Vector3 sourcePos = new Vector3(-3.4f, 1.7f, 0f);
        Vector3 listenerPos = new Vector3(0.6f, 1.7f, 0f);
        Vector3 mixerPos = new Vector3(-3.4f, -0.3f, 0f);
        Vector3 masterPos = new Vector3(0.6f, -0.3f, 0f);

        AudioListener ears = W09Kit.Listener(t, listenerPos, "");
        W09Kit.Label(t, "LISTENER", listenerPos + new Vector3(0f, 1.35f, 0f), DemoKit.Dim);
        AudioSource coin = W09Kit.Source(t, "AudioSource", W09Kit.Clip("coin"), sourcePos,
            false, 1f, 10f, sfx);

        SpriteRenderer clipNode = Node(t, "NodeClip", "CLIP", clipPos, DemoKit.Orange);
        SpriteRenderer sourceNode = Node(t, "NodeSource", "SOURCE", sourcePos, DemoKit.Blue);
        SpriteRenderer listenerNode = ears.GetComponentInChildren<SpriteRenderer>();
        SpriteRenderer mixerNode = Node(t, "NodeMixer", "MIXER", mixerPos, DemoKit.Pink,
                                                new Vector3(-1.45f, 0.55f, 0f));
        SpriteRenderer masterNode = Node(t, "NodeMaster", "MASTER", masterPos, DemoKit.Green);

        LineRenderer clipToSource = Segment(t, "LinkClipSource", clipPos, sourcePos, DemoKit.Dim, 0.09f, 2);
        LineRenderer sourceToListener = Segment(t, "LinkSourceListener", sourcePos, listenerPos, DemoKit.Dim, 0.09f, 2);
        LineRenderer sourceToMixer = Segment(t, "LinkSourceMixer", sourcePos, mixerPos, DemoKit.Dim, 0.09f, 2);
        LineRenderer mixerToMaster = Segment(t, "LinkMixerMaster", mixerPos, masterPos, DemoKit.Dim, 0.09f, 2);

        Transform dotOne = Icon(t, "PathDotListener", DemoKit.Orange, clipPos, 0.3f, 12).transform;
        Transform dotTwo = Icon(t, "PathDotMixer", DemoKit.Pink, sourcePos, 0.3f, 12).transform;

        AudioMapDemo demo = t.gameObject.AddComponent<AudioMapDemo>();
        demo.source = coin;
        demo.listener = ears;
        demo.mixerGroup = sfx;
        demo.nodeClip = clipNode;
        demo.nodeSource = sourceNode;
        demo.nodeListener = listenerNode;
        demo.nodeMixer = mixerNode;
        demo.nodeMaster = masterNode;
        demo.linkClipSource = clipToSource;
        demo.linkSourceListener = sourceToListener;
        demo.linkSourceMixer = sourceToMixer;
        demo.linkMixerMaster = mixerToMaster;
        demo.dotOne = dotOne;
        demo.dotTwo = dotTwo;
        demo.pathOne = new[] { t.TransformPoint(clipPos), t.TransformPoint(sourcePos), t.TransformPoint(listenerPos) };
        demo.pathTwo = new[] { t.TransformPoint(sourcePos), t.TransformPoint(mixerPos), t.TransformPoint(masterPos) };
    }

    // ---------------------------------------------------------------- 3. play replaces

    public static void BuildPlayReplace(Transform t)
    {
        W09Kit.Camera(t);
        AudioMixerGroup sfx = W09Kit.Group("SFX");

        AudioListener ears = W09Kit.Listener(t, new Vector3(-4.6f, 1.6f, 0f));
        AudioSource source = W09Kit.Source(t, "Source", W09Kit.Clip("coin"),
            new Vector3(-2.6f, 0.6f, 0f), false, 1f, 10f, sfx);

        Icon(t, "SourceIcon", DemoKit.Orange, new Vector3(-2.6f, 0.6f, 0f), 0.8f, 10);
        Text(t, "SourceLabel", "ONE SOURCE", new Vector3(-2.6f, 1.3f, 0f), DemoKit.Dim);

        Transform coinFill, laserFill;
        SpriteRenderer coinSprite, laserSprite;
        W09Kit.Meter(t, "CoinMeter", new Vector3(2.2f, 1.1f, 0f), 5f, DemoKit.Orange,
            out coinFill, out coinSprite);
        Text(t, "CoinLabel", "COIN", new Vector3(2.2f, 1.55f, 0f), DemoKit.Orange);
        W09Kit.Meter(t, "LaserMeter", new Vector3(2.2f, -0.1f, 0f), 5f, DemoKit.Pink,
            out laserFill, out laserSprite);
        Text(t, "LaserLabel", "LASER", new Vector3(2.2f, 0.35f, 0f), DemoKit.Pink);
        TextMesh flash = Text(t, "ReplaceFlash", "", new Vector3(2.2f, 2.5f, 0f), DemoKit.Pink);

        PlayReplaceDemo demo = t.gameObject.AddComponent<PlayReplaceDemo>();
        demo.source = source;
        demo.listener = ears;
        demo.mixerGroup = sfx;
        demo.coinClip = W09Kit.Clip("coin");
        demo.laserClip = W09Kit.Clip("laser");
        demo.coinFill = coinFill;
        demo.laserFill = laserFill;
        demo.coinFillSprite = coinSprite;
        demo.laserFillSprite = laserSprite;
        demo.flash = flash;
    }

    // ---------------------------------------------------------------- 4. oneshot layers

    public static void BuildOneShotLayer(Transform t)
    {
        W09Kit.Camera(t);
        AudioMixerGroup sfx = W09Kit.Group("SFX");

        AudioListener ears = W09Kit.Listener(t, new Vector3(-6.2f, 1.8f, 0f));
        AudioSource source = W09Kit.Source(t, "Source", W09Kit.Clip("coin"),
            new Vector3(-3.2f, 0.4f, 0f), false, 1f, 10f, sfx);

        Icon(t, "SourceIcon", DemoKit.Orange, new Vector3(-3.2f, 0.4f, 0f), 0.8f, 10);
        Text(t, "SourceLabel", "ONE SOURCE", new Vector3(-3.2f, 1.15f, 0f), DemoKit.Dim);

        Transform[] fills = new Transform[5];
        SpriteRenderer[] sprites = new SpriteRenderer[5];
        for (int i = 0; i < 5; i++)
        {
            float x = 0.2f + i * 1.6f;
            W09Kit.Meter(t, "Voice" + (i + 1), new Vector3(x, 0.7f, 0f), 1.35f, DemoKit.Dim,
                out fills[i], out sprites[i]);
            Text(t, "VoiceLabel" + (i + 1), (i + 1).ToString(), new Vector3(x, 1.15f, 0f), DemoKit.Dim);
        }

        OneShotLayerDemo demo = t.gameObject.AddComponent<OneShotLayerDemo>();
        demo.source = source;
        demo.listener = ears;
        demo.mixerGroup = sfx;
        demo.coinClip = W09Kit.Clip("coin");
        demo.laserClip = W09Kit.Clip("laser");
        demo.voiceFills = fills;
        demo.voiceFillSprites = sprites;
    }

    // ---------------------------------------------------------------- 5. pitch vary

    public static void BuildPitchVary(Transform t)
    {
        W09Kit.Camera(t);
        AudioMixerGroup sfx = W09Kit.Group("SFX");

        AudioListener ears = W09Kit.Listener(t, new Vector3(-5.4f, 1.2f, 0f));
        AudioSource source = W09Kit.Source(t, "Source", W09Kit.Clip("click"),
            new Vector3(-3.6f, 0.4f, 0f), false, 1f, 10f, sfx);

        Icon(t, "SourceIcon", DemoKit.Orange, new Vector3(-3.6f, 0.4f, 0f), 0.8f, 10);
        Text(t, "SourceLabel", "ONE CLICK CLIP", new Vector3(-2.8f, 1.1f, 0f), DemoKit.Dim);

        Transform[] fills = new Transform[3];
        TextMesh[] labels = new TextMesh[3];
        for (int i = 0; i < 3; i++)
        {
            float x = 0.6f + i * 2f;
            SpriteRenderer unused;
            W09Kit.Meter(t, "Pitch" + (i + 1), new Vector3(x, 0.4f, 0f), 1.5f, DemoKit.Orange,
                out fills[i], out unused);
            labels[i] = Text(t, "PitchValue" + (i + 1), "--", new Vector3(x, 0.95f, 0f), DemoKit.Dim);
        }

        PitchVaryDemo demo = t.gameObject.AddComponent<PitchVaryDemo>();
        demo.source = source;
        demo.listener = ears;
        demo.mixerGroup = sfx;
        demo.pitchFills = fills;
        demo.pitchLabels = labels;
    }

    // ---------------------------------------------------------------- 6. spatial blend

    public static void BuildSpatialBlend(Transform t)
    {
        W09Kit.Camera(t);
        AudioMixerGroup sfx = W09Kit.Group("SFX");

        AudioListener ears = W09Kit.Listener(t, new Vector3(-1.4f, 0.4f, 0f));
        // A long musical loop, kept quiet: this demo is about HEARING the pan move as the blend
        // sweeps, so the source has to be continuous - a short SFX looped just buzzes.
        // The source sits high and to the right: its halo is 0.95 across, and at y = 0.4 it grazed
        // the blend slider below it. Same x, so the pan story is unchanged.
        AudioSource source = W09Kit.Emitter(t, "Music", "bgm_loop", new Vector3(5f, 1.5f, 0f),
            true, 1f, 12f, sfx, DemoKit.Orange, true, 0.45f);

        Transform leftFill, rightFill, blendFill, blendKnob;
        SpriteRenderer unused;
        // Labels sit beside the bars, not above them: stacked captions were crowding the bar
        // underneath, and the bars have to line up so "louder in one ear" reads as a level change.
        W09Kit.Meter(t, "LeftEarMeter", new Vector3(0.8f, 3.4f, 0f), 6f, DemoKit.Blue,
            out leftFill, out unused);
        Text(t, "LeftEarLabel", "LEFT EAR", new Vector3(-3.8f, 3.14f, 0f), DemoKit.Blue);
        W09Kit.Meter(t, "RightEarMeter", new Vector3(0.8f, 2.4f, 0f), 6f, DemoKit.Green,
            out rightFill, out unused);
        Text(t, "RightEarLabel", "RIGHT EAR", new Vector3(-3.8f, 2.14f, 0f), DemoKit.Green);

        W09Kit.Slider(t, "BlendSlider", new Vector3(2.8f, -0.6f, 0f), 5f, DemoKit.Blue,
            out blendFill, out blendKnob);
        Text(t, "BlendLabel", "spatialBlend", new Vector3(2.8f, 0.05f, 0f), DemoKit.Dim);

        SpatialBlendDemo demo = t.gameObject.AddComponent<SpatialBlendDemo>();
        demo.source = source;
        demo.listener = ears;
        demo.mixerGroup = sfx;
        demo.earWidth = 6f;          // the meters above are 6 wide, and the fill is drawn in world units
        demo.earLeftFill = leftFill;
        demo.earRightFill = rightFill;
        demo.blendFill = blendFill;
        demo.blendKnob = blendKnob;
    }

    // ---------------------------------------------------------------- 7. min and max

    public static void BuildMinMax(Transform t)
    {
        W09Kit.Camera(t);
        AudioMixerGroup sfx = W09Kit.Group("SFX");

        AudioListener ears = W09Kit.Listener(t, new Vector3(-7f, 0.4f, 0f));
        // Not looping: one fire per SPACE press is enough to hear the radius, and a permanent
        // crackle under a lecture is unbearable.
        AudioSource fire = W09Kit.Emitter(t, "Fire", "fire_loop", new Vector3(0f, 0.4f, 0f),
            true, 1f, 7f, sfx, DemoKit.Orange);
        // Linear, so "past max" really is silence - that is the state word the readout shows.
        fire.rolloffMode = AudioRolloffMode.Linear;

        W09Kit.Ring(fire.transform, "MinRing", Vector3.zero, 1f, DemoKit.Green, 5);
        W09Kit.Ring(fire.transform, "MaxRing", Vector3.zero, 7f, new Color(0.56f, 0.64f, 0.75f, 0.35f), 3);
        W09Kit.Label(fire.transform, "min 1", new Vector3(0f, -1.35f, 0f), DemoKit.Green);
        W09Kit.Label(fire.transform, "max 7", new Vector3(7.2f, 1.3f, 0f), DemoKit.Dim);

        LineRenderer line = W09Kit.Line(t, "DistanceLine", DemoKit.Dim, 0.06f, 2);
        line.SetPosition(0, t.TransformPoint(new Vector3(0f, 0.4f, 0f)));
        line.SetPosition(1, t.TransformPoint(new Vector3(-7f, 0.4f, 0f)));

        MinMaxDemo demo = t.gameObject.AddComponent<MinMaxDemo>();
        demo.source = fire;
        demo.listener = ears;
        demo.mixerGroup = sfx;
        demo.listenerTransform = ears.transform;
        demo.distanceLine = line;
    }

    // ---------------------------------------------------------------- 8. rolloff

    public static void BuildRolloff(Transform t)
    {
        W09Kit.Camera(t);
        AudioMixerGroup sfx = W09Kit.Group("SFX");

        AudioListener ears = W09Kit.Listener(t, new Vector3(-6f, 0.2f, 0f));
        AudioSource logSource = W09Kit.Source(t, "LogSource", W09Kit.Clip("fire_loop"),
            new Vector3(0f, 1.4f, 0f), true, 1f, 10f, sfx, true, 0.8f);
        AudioSource linearSource = W09Kit.Source(t, "LinearSource", W09Kit.Clip("fire_loop"),
            new Vector3(0f, 1.4f, 0f), true, 1f, 10f, sfx, true, 0.8f);
        linearSource.rolloffMode = AudioRolloffMode.Linear;

        // Both sources sit on the same point: two concentric markers, one label each.
        Icon(t, "LogIcon", DemoKit.Blue, new Vector3(0f, 1.4f, 0f), 0.9f, 10);
        Icon(t, "LinearIcon", DemoKit.Pink, new Vector3(0f, 1.4f, 0f), 0.45f, 11);
        // Off to the right of the rings: above them they collided with the meter labels, so the
        // screen read "LIN   LINEAR" as if it were one word.
        Text(t, "LogLabel", "LOGARITHMIC", new Vector3(1.7f, 2.1f, 0f), DemoKit.Blue);
        Text(t, "LinearLabel", "LINEAR", new Vector3(1.7f, 1.25f, 0f), DemoKit.Pink);

        Transform logFill, linearFill;
        SpriteRenderer logSprite, linearSprite;
        W09Kit.Meter(t, "LogMeter", new Vector3(-2.4f, 3.2f, 0f), 4.6f, DemoKit.Blue,
            out logFill, out logSprite);
        Text(t, "LogMeterLabel", "LOG", new Vector3(-2.4f, 3.65f, 0f), DemoKit.Blue);
        W09Kit.Meter(t, "LinearMeter", new Vector3(-2.4f, 2.2f, 0f), 4.6f, DemoKit.Pink,
            out linearFill, out linearSprite);
        Text(t, "LinearMeterLabel", "LIN", new Vector3(-2.4f, 2.65f, 0f), DemoKit.Pink);

        RolloffDemo demo = t.gameObject.AddComponent<RolloffDemo>();
        demo.source = logSource;
        demo.secondSource = linearSource;
        demo.listener = ears;
        demo.mixerGroup = sfx;
        demo.logFill = logFill;
        demo.linearFill = linearFill;
        demo.logFillSprite = logSprite;
        demo.linearFillSprite = linearSprite;
    }

    // ---------------------------------------------------------------- little builders

    private static SpriteRenderer Panel(Transform t, string name, Vector3 pos, Vector2 size,
                                        Color color, int order)
    {
        GameObject go = DemoKit.Visual(t, name, LabKit.White, color, pos, size, order);
        return go.GetComponent<SpriteRenderer>();
    }

    /// <summary>A map node: a coloured circle with a grey caption above it.</summary>
    private static SpriteRenderer Node(Transform t, string name, string caption, Vector3 pos, Color color,
                                       Vector3? captionOffset = null)
    {
        GameObject icon = Icon(t, name, color, pos, 0.9f, 10);
        Text(t, name + "Label", caption, pos + (captionOffset ?? new Vector3(0f, 0.55f, 0f)), DemoKit.Dim);
        return icon.GetComponent<SpriteRenderer>();
    }

    private static GameObject Icon(Transform t, string name, Color color, Vector3 pos, float size, int order)
    {
        return W09Kit.Icon(t, name, LabKit.Circle, color, pos, size, order);
    }

    private static TextMesh Text(Transform t, string name, string text, Vector3 pos, Color color)
    {
        GameObject go = W09Kit.Label(t, text, pos, color);
        go.name = name;
        return go.GetComponent<TextMesh>();
    }

    /// <summary>A world-space line between two LOCAL points, for the audio-map arrows.</summary>
    private static LineRenderer Segment(Transform t, string name, Vector3 a, Vector3 b,
                                        Color color, float width, int order)
    {
        LineRenderer lr = W09Kit.Line(t, name, color, width, order);
        lr.SetPosition(0, t.TransformPoint(a));
        lr.SetPosition(1, t.TransformPoint(b));
        return lr;
    }
}
