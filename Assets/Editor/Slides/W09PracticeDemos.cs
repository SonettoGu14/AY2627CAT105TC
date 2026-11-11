using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Slide 7-10 practice demos: THE FOOTSTEP MANAGER, THE MIXER (groups + bypass),
/// WHERE AUDIO GOES WRONG (five bugs), and the walkaround. One Build method per demo, each
/// furnishing its own root with W09Kit and adding a DemoBase component (see
/// Assets/Scripts/Slides/Demos/W09/PracticeAudioDemos.cs).
///
/// Every method does the same four things in the same order: the demo's own camera, its ears
/// (an AudioListener on a real object, so it can be moved or switched off), the sources and the set
/// dressing, then the component with its public fields wired. The readout panel covers the bottom
/// left of the screen (x &lt; -0.8 and y &lt; -1.5), so the interesting part of every demo sits above
/// y = -1.2.
/// </summary>
internal static class W09PracticeDemos
{
    // ------------------------------------------------------------------ slide 7: footsteps

    /// <summary>The worked example: a player that walks, and a step every 0.4 s from the current
    /// terrain's three clips, with a random pitch. Nothing loops - the timer makes the rhythm.</summary>
    public static void BuildFootsteps(Transform t)
    {
        W09Kit.Camera(t);
        W09Kit.Listener(t, new Vector3(7.6f, 4.3f, 0f), "");

        DemoKit.Visual(t, "Ground", LabKit.White, DemoKit.Slate,
                       new Vector2(0f, -1f), new Vector2(17.4f, 0.28f), 0);
        GameObject player = DemoKit.Character(t, "Player", new Vector2(0f, -0.4f), new Vector2(1.1f, 1.1f), 6);

        Transform[] prints = new Transform[9];
        for (int i = 0; i < prints.Length; i++)
        {
            GameObject print = W09Kit.Icon(t, "Print" + i, LabKit.Circle, DemoKit.Dim,
                                           new Vector3(0f, -0.94f, 0f), 0.3f, 3);
            print.SetActive(false);
            prints[i] = print.transform;
        }

        TextMesh terrainText = W09Kit.Label(t, "TERRAIN   DIRT   (1)", new Vector3(0f, 4f, 0f), DemoKit.Orange)
                                      .GetComponent<TextMesh>();
        TextMesh stepText = W09Kit.Label(t, "steps 0", new Vector3(0f, 3.15f, 0f), DemoKit.Dim)
                                   .GetComponent<TextMesh>();

        AudioClip[] dirt = W09Kit.Clips("footstep_dirt_1", "footstep_dirt_2", "footstep_dirt_3");
        AudioClip[] grass = W09Kit.Clips("footstep_grass_1", "footstep_grass_2", "footstep_grass_3");
        AudioClip[] snow = W09Kit.Clips("footstep_snow_1", "footstep_snow_2", "footstep_snow_3");
        AudioClip[] gravel = W09Kit.Clips("footstep_gravel_1", "footstep_gravel_2", "footstep_gravel_3");
        AudioMixerGroup sfx = W09Kit.Group("SFX");

        AudioSource steps = W09Kit.Source(t, "StepSource", dirt.Length > 0 ? dirt[0] : null,
                                          new Vector3(0f, -0.4f, 0f), false, 1f, 10f, sfx, false, 0.9f);

        FootstepsDemo demo = t.gameObject.AddComponent<FootstepsDemo>();
        demo.source = steps;
        demo.mixerGroup = sfx;
        demo.player = player.transform;
        demo.dirt = dirt;
        demo.grass = grass;
        demo.snow = snow;
        demo.gravel = gravel;
        demo.prints = prints;
        demo.terrainText = terrainText;
        demo.stepText = stepText;
        demo.terrainColors = new[] { DemoKit.Orange, DemoKit.Green, DemoKit.Blue, DemoKit.Pink };
    }

    // ------------------------------------------------------------------ slide 8: the mixer

    /// <summary>The tree Master → BGM / SFX with a meter on each branch, and one source per branch.
    /// The two branches are hard-panned so a measured meter is one group and nothing else.</summary>
    public static void BuildMixerGroups(Transform t)
    {
        W09Kit.Camera(t);
        W09Kit.Listener(t, new Vector3(7.9f, 4.0f, 0f), "");

        const float masterX = -6.4f;
        const float bgmX = -2.4f;
        const float sfxX = 2f;

        // the bus and its three nodes
        DemoKit.Visual(t, "Bus", LabKit.White, DemoKit.Slate,
                       new Vector2((masterX + sfxX) * 0.5f, 3.35f), new Vector2(sfxX - masterX, 0.06f), 1);
        float[] nodeX = { masterX, bgmX, sfxX };
        Color[] nodeColor = { DemoKit.Orange, DemoKit.Blue, DemoKit.Green };
        string[] nodeName = { "MASTER", "BGM", "SFX" };
        for (int i = 0; i < nodeX.Length; i++)
        {
            DemoKit.Visual(t, nodeName[i] + "Drop", LabKit.White, DemoKit.Slate,
                           new Vector2(nodeX[i], 3.47f), new Vector2(0.06f, 0.24f), 1);
            W09Kit.Icon(t, nodeName[i], LabKit.Circle, nodeColor[i],
                        new Vector3(nodeX[i], 3.9f, 0f), 0.62f, 10);
            W09Kit.Label(t, nodeName[i], new Vector3(nodeX[i], 2.95f, 0f), nodeColor[i]);
        }

        Transform masterFill, bgmFill, sfxFill;
        SpriteRenderer unused;
        W09Kit.Meter(t, "MasterMeter", new Vector3(masterX, 2.5f, 0f), 3.2f, DemoKit.Orange, out masterFill, out unused);
        W09Kit.Meter(t, "BgmMeter", new Vector3(bgmX, 2.5f, 0f), 3.2f, DemoKit.Blue, out bgmFill, out unused);
        W09Kit.Meter(t, "SfxMeter", new Vector3(sfxX, 2.5f, 0f), 3.2f, DemoKit.Green, out sfxFill, out unused);

        TextMesh masterDb = W09Kit.Label(t, "0.0 dB", new Vector3(masterX, 1.7f, 0f), DemoKit.Dim).GetComponent<TextMesh>();
        TextMesh bgmDb = W09Kit.Label(t, "0.0 dB", new Vector3(bgmX, 1.7f, 0f), DemoKit.Dim).GetComponent<TextMesh>();
        TextMesh sfxDb = W09Kit.Label(t, "0.0 dB", new Vector3(sfxX, 1.7f, 0f), DemoKit.Dim).GetComponent<TextMesh>();

        Transform[] marks = new Transform[nodeX.Length];
        for (int i = 0; i < nodeX.Length; i++)
        {
            GameObject mark = W09Kit.Icon(t, "Selected" + i, LabKit.Circle, DemoKit.Orange,
                                          new Vector3(nodeX[i], 4.5f, 0f), 0.24f, 30);
            mark.SetActive(false);
            marks[i] = mark.transform;
        }

        // one source per branch, plus a third one that is in no group at all
        AudioMixerGroup bgmGroup = W09Kit.Group("BGM");
        AudioMixerGroup sfxGroup = W09Kit.Group("SFX");

        AudioSource bgm = W09Kit.Source(t, "Bgm", W09Kit.Clip("bgm_loop"), new Vector3(bgmX, 0.8f, 0f),
                                       false, 1f, 10f, bgmGroup, true, 0.8f);
        bgm.panStereo = -1f;
        W09Kit.Icon(bgm.transform, "Speaker", LabKit.Circle, DemoKit.Blue, Vector3.zero, 0.5f, 10);
        W09Kit.Label(t, "bgm 2D", new Vector3(bgmX, -0.35f, 0f), DemoKit.Blue);

        AudioSource coin = W09Kit.Source(t, "Coin", W09Kit.Clip("coin"), new Vector3(bgmX + 4f, 0.8f, 0f),
                                        false, 1f, 10f, sfxGroup, false, 0.6f);
        coin.panStereo = 1f;
        W09Kit.Icon(coin.transform, "Speaker", LabKit.Circle, DemoKit.Green, Vector3.zero, 0.5f, 10);
        W09Kit.Label(t, "coin 2D", new Vector3(bgmX + 4f, -0.35f, 0f), DemoKit.Green);

        AudioSource unrouted = W09Kit.Source(t, "Unrouted", W09Kit.Clip("blip"), new Vector3(sfxX + 3.4f, 0.8f, 0f),
                                            false, 1f, 10f, null, false, 0.5f);
        W09Kit.Icon(unrouted.transform, "Speaker", LabKit.Circle, DemoKit.Dim, Vector3.zero, 0.5f, 10);
        W09Kit.Label(t, "unrouted", new Vector3(sfxX + 3.4f, -0.35f, 0f), DemoKit.Dim);
        W09Kit.Label(t, "OUTPUT: NONE", new Vector3(sfxX + 3.4f, -1.05f, 0f), DemoKit.Pink);

        MixerGroupsDemo demo = t.gameObject.AddComponent<MixerGroupsDemo>();
        demo.source = bgm;
        demo.secondSource = coin;
        demo.unroutedSource = unrouted;
        demo.mixerGroup = sfxGroup;
        demo.masterMeter = masterFill;
        demo.bgmMeter = bgmFill;
        demo.sfxMeter = sfxFill;
        demo.masterDbText = masterDb;
        demo.bgmDbText = bgmDb;
        demo.sfxDbText = sfxDb;
        demo.selectMarks = marks;
    }

    /// <summary>Two identical coins: one in the SFX group (hard left), one with Output None (hard
    /// right). Drop the group and only one speaker follows.</summary>
    public static void BuildMixerBypass(Transform t)
    {
        W09Kit.Camera(t);
        W09Kit.Listener(t, new Vector3(0f, 3.95f, 0f), "");

        AudioMixerGroup sfx = W09Kit.Group("SFX");

        AudioSource routed = W09Kit.Emitter(t, "Routed", "coin", new Vector3(-3.6f, 0.9f, 0f),
                                           false, 1f, 10f, sfx, DemoKit.Green);
        routed.panStereo = -1f;

        AudioSource unrouted = W09Kit.Emitter(t, "Unrouted", "coin", new Vector3(3.6f, 0.9f, 0f),
                                              false, 1f, 10f, null, DemoKit.Pink);
        unrouted.panStereo = 1f;

        W09Kit.Label(t, "ROUTED -> SFX", new Vector3(-3.6f, 2.6f, 0f), DemoKit.Green);
        W09Kit.Label(t, "OUTPUT: NONE", new Vector3(3.6f, 2.6f, 0f), DemoKit.Pink);
        TextMesh groupText = W09Kit.Label(t, "SFX group   0.0 dB", new Vector3(2.9f, 3.95f, 0f), DemoKit.Orange)
                                    .GetComponent<TextMesh>();

        Transform routedFill, unroutedFill;
        SpriteRenderer unused;
        W09Kit.Meter(t, "RoutedMeter", new Vector3(-3.6f, -0.45f, 0f), 4.4f, DemoKit.Green, out routedFill, out unused);
        W09Kit.Meter(t, "UnroutedMeter", new Vector3(3.6f, -0.45f, 0f), 4.4f, DemoKit.Pink, out unroutedFill, out unused);

        TextMesh routedText = W09Kit.Label(t, "L", new Vector3(-3.6f, -1.35f, 0f), DemoKit.Dim).GetComponent<TextMesh>();
        TextMesh unroutedText = W09Kit.Label(t, "R", new Vector3(3.6f, -1.35f, 0f), DemoKit.Dim).GetComponent<TextMesh>();

        MixerBypassDemo demo = t.gameObject.AddComponent<MixerBypassDemo>();
        demo.source = routed;
        demo.secondSource = unrouted;
        demo.mixerGroup = sfx;
        demo.routedMeter = routedFill;
        demo.unroutedMeter = unroutedFill;
        demo.routedText = routedText;
        demo.unroutedText = unroutedText;
        demo.groupText = groupText;
    }

    // ------------------------------------------------------------------ slide 9: the five bugs

    /// <summary>Bug 1: a looping 3D source, and an ears object that starts switched off. The source
    /// is playing and the level meter moves; there is simply nothing listening.</summary>
    public static void BuildBugNoListener(Transform t)
    {
        W09Kit.Camera(t);

        AudioMixerGroup sfx = W09Kit.Group("SFX");
        AudioSource fire = W09Kit.Emitter(t, "Fire", "fire_loop", new Vector3(-2.6f, 1f, 0f),
                                          true, 1.5f, 12f, sfx, DemoKit.Orange, true);

        // The ears are built here rather than by W09Kit.Listener, because they must start inactive:
        // switching the AudioListener off is the bug.
        GameObject earGo = LabKit.Child(t, "EarsOff", new Vector3(3.4f, 1f, 0f));
        AudioListener ears = earGo.AddComponent<AudioListener>();
        W09Kit.Icon(earGo.transform, "Marker", LabKit.Circle, new Color(0.55f, 0.78f, 0.98f, 0.9f),
                    Vector3.zero, 0.45f, 20);
        W09Kit.Ring(earGo.transform, "Range", Vector3.zero, 0.75f, new Color(0.55f, 0.78f, 0.98f, 0.35f), 19);
        earGo.SetActive(false);

        W09Kit.Label(t, "EARS", new Vector3(3.4f, 2.2f, 0f), DemoKit.Dim);
        TextMesh earState = W09Kit.Label(t, "listener OFF", new Vector3(3.4f, 0.1f, 0f), DemoKit.Pink)
                                   .GetComponent<TextMesh>();
        TextMesh headline = W09Kit.Label(t, "NO LISTENER", new Vector3(0f, 4.3f, 0f), DemoKit.Pink)
                                   .GetComponent<TextMesh>();

        Transform levelFill;
        SpriteRenderer unused;
        W09Kit.Meter(t, "LevelMeter", new Vector3(-2.6f, -0.45f, 0f), 4.2f, DemoKit.Orange, out levelFill, out unused);
        TextMesh levelText = W09Kit.Label(t, "source", new Vector3(-2.6f, -1.25f, 0f), DemoKit.Dim)
                                    .GetComponent<TextMesh>();

        BugNoListenerDemo demo = t.gameObject.AddComponent<BugNoListenerDemo>();
        demo.source = fire;
        demo.listener = ears;
        demo.earObject = earGo;
        demo.headline = headline;
        demo.earState = earState;
        demo.levelMeter = levelFill;
        demo.levelText = levelText;
    }

    /// <summary>Bug 2: a 3D source asked for Min 14 and Max 10 - the radii the wrong way round. Unity
    /// will not keep Max inside Min (it raises Max to Min), so the fade region collapses to nothing:
    /// the pink Min ring is where the volume is still full, the blue ring is the 10 that was asked
    /// for and never used, and the source cuts off dead at 14 units instead of fading.</summary>
    public static void BuildBugMinDistance(Transform t)
    {
        W09Kit.Camera(t);

        Vector3 sourcePos = new Vector3(-6f, 0.2f, 0f);
        AudioMixerGroup sfx = W09Kit.Group("SFX");
        AudioSource fire = W09Kit.Emitter(t, "Fire", "fire_loop", sourcePos, true, 14f, 10f, sfx, DemoKit.Orange, true);

        W09Kit.Ring(t, "MinRing", sourcePos, 14f, DemoKit.Pink, 2);
        W09Kit.Ring(t, "MaxRing", sourcePos, 10f, DemoKit.Blue, 3);
        DemoKit.Visual(t, "Path", LabKit.White, DemoKit.Dark, new Vector2(0f, 0.2f), new Vector2(17.2f, 0.05f), 1);

        W09Kit.Label(t, "MIN 14", new Vector3(6.4f, 3.95f, 0f), DemoKit.Pink);
        W09Kit.Label(t, "MAX 10", new Vector3(2.3f, 3.95f, 0f), DemoKit.Blue);

        AudioListener ears = W09Kit.Listener(t, new Vector3(-7.4f, 0.2f, 0f));

        Transform levelFill;
        SpriteRenderer unused;
        W09Kit.Meter(t, "LevelMeter", new Vector3(-5.6f, 4.35f, 0f), 5.4f, DemoKit.Orange, out levelFill, out unused);
        TextMesh levelText = W09Kit.Label(t, "ears", new Vector3(-5.6f, 3.45f, 0f), DemoKit.Dim)
                                    .GetComponent<TextMesh>();

        BugMinDistanceDemo demo = t.gameObject.AddComponent<BugMinDistanceDemo>();
        demo.source = fire;
        demo.listener = ears;
        demo.mixerGroup = sfx;
        demo.levelMeter = levelFill;
        demo.levelText = levelText;
    }

    /// <summary>Bug 3: the slider is real, the source is not in the mixer. Output None, so the SFX
    /// group volume never reaches it - until O routes it in.</summary>
    public static void BuildBugOutputNone(Transform t)
    {
        W09Kit.Camera(t);
        W09Kit.Listener(t, new Vector3(-7.3f, 3.85f, 0f), "");

        AudioMixerGroup sfx = W09Kit.Group("SFX");
        // Pings rather than a loop: the coin has to be audible whenever you nudge the slider, but
        // a coin on repeat is exactly the noise this slide warns about.
        AudioSource coin = W09Kit.Emitter(t, "Coin", "coin", new Vector3(-2.8f, 1f, 0f),
                                          false, 1f, 10f, null, DemoKit.Pink);
        // Below the halo, not across its bottom arc.
        TextMesh outputText = W09Kit.Label(t, "OUTPUT: NONE", new Vector3(-2.8f, -0.75f, 0f), DemoKit.Pink)
                                     .GetComponent<TextMesh>();

        TextMesh sliderText = W09Kit.Label(t, "SFX   0.0 dB", new Vector3(3.6f, 2.25f, 0f), DemoKit.Orange)
                                     .GetComponent<TextMesh>();
        W09Kit.Label(t, "the mixer slider", new Vector3(3.6f, 0.72f, 0f), DemoKit.Dim);

        Transform sliderFill, sliderKnob;
        W09Kit.Slider(t, "SfxSlider", new Vector3(3.6f, 1.45f, 0f), 4.8f, DemoKit.Green, out sliderFill, out sliderKnob);

        Transform levelFill;
        SpriteRenderer unused;
        W09Kit.Meter(t, "LevelMeter", new Vector3(3.6f, -0.05f, 0f), 4.8f, DemoKit.Orange, out levelFill, out unused);
        TextMesh levelText = W09Kit.Label(t, "ears", new Vector3(3.6f, -0.85f, 0f), DemoKit.Dim)
                                    .GetComponent<TextMesh>();

        BugOutputNoneDemo demo = t.gameObject.AddComponent<BugOutputNoneDemo>();
        demo.source = coin;
        demo.mixerGroup = sfx;
        demo.sliderFill = sliderFill;
        demo.sliderKnob = sliderKnob;
        demo.sliderText = sliderText;
        demo.outputText = outputText;
        demo.levelMeter = levelFill;
        demo.levelText = levelText;
    }

    /// <summary>Bug 4: the same footstep clip fired every frame with Play(). T swaps in the real
    /// manager (0.4 s throttle, PlayOneShot, random pitch) so the difference is audible.</summary>
    public static void BuildBugMachineGun(Transform t)
    {
        W09Kit.Camera(t);
        W09Kit.Listener(t, new Vector3(-7.6f, 4.4f, 0f), "");

        AudioMixerGroup sfx = W09Kit.Group("SFX");
        AudioSource steps = W09Kit.Source(t, "StepSource", W09Kit.Clip("footstep_dirt_1"),
                                          new Vector3(0f, 0.2f, 0f), false, 1f, 10f, sfx, false, 1f);
        W09Kit.Icon(steps.transform, "Speaker", LabKit.Circle, DemoKit.Green, Vector3.zero, 0.6f, 10);
        W09Kit.Label(steps.transform, "footstep_dirt_1", new Vector3(0f, 1f, 0f), DemoKit.Green);

        TextMesh modeText = W09Kit.Label(t, "BROKEN: Play() every frame", new Vector3(0f, 4.35f, 0f), DemoKit.Pink)
                                   .GetComponent<TextMesh>();
        TextMesh rateText = W09Kit.Label(t, "fire rate   0 steps/s", new Vector3(0f, 3.5f, 0f), DemoKit.Orange)
                                   .GetComponent<TextMesh>();
        TextMesh verdictText = W09Kit.Label(t, "a machine gun, not footsteps", new Vector3(0f, 1.95f, 0f), DemoKit.Dim)
                                      .GetComponent<TextMesh>();

        Transform rateFill;
        SpriteRenderer unused;
        W09Kit.Meter(t, "RateMeter", new Vector3(0f, 2.75f, 0f), 9f, DemoKit.Orange, out rateFill, out unused);

        BugMachineGunDemo demo = t.gameObject.AddComponent<BugMachineGunDemo>();
        demo.source = steps;
        demo.mixerGroup = sfx;
        demo.rateMeter = rateFill;
        demo.modeText = modeText;
        demo.rateText = rateText;
        demo.verdictText = verdictText;
    }

    /// <summary>Bug 5: Space fires twelve one-shots at full volume at once - a summed gain of 12, so
    /// the output clips. C switches to the fix: at most four voices at a quarter of the volume.</summary>
    public static void BuildBugCrackling(Transform t)
    {
        W09Kit.Camera(t);
        W09Kit.Listener(t, new Vector3(-7.6f, 3.9f, 0f), "");

        AudioMixerGroup sfx = W09Kit.Group("SFX");
        AudioSource hit = W09Kit.Source(t, "HitSource", W09Kit.Clip("hit"), new Vector3(0f, 0.4f, 0f),
                                        false, 1f, 10f, sfx, false, 1f);
        W09Kit.Icon(hit.transform, "Speaker", LabKit.Circle, DemoKit.Green, Vector3.zero, 0.6f, 10);
        W09Kit.Label(hit.transform, "hit", new Vector3(0f, -1.1f, 0f), DemoKit.Green);

        TextMesh modeText = W09Kit.Label(t, "12 one-shots x 1.0 at once", new Vector3(0f, 4.35f, 0f), DemoKit.Pink)
                                   .GetComponent<TextMesh>();
        TextMesh voiceText = W09Kit.Label(t, "voices 0 of 12", new Vector3(0f, 3.55f, 0f), DemoKit.Orange)
                                    .GetComponent<TextMesh>();
        TextMesh gainText = W09Kit.Label(t, "summed gain 0.0  clean", new Vector3(0f, 2.85f, 0f), DemoKit.Dim)
                                   .GetComponent<TextMesh>();

        Transform levelFill;
        SpriteRenderer unused;
        W09Kit.Meter(t, "LevelMeter", new Vector3(0f, 2.05f, 0f), 9f, DemoKit.Orange, out levelFill, out unused);

        GameObject clipMark = W09Kit.Icon(t, "ClipMark", LabKit.Circle, DemoKit.Pink,
                                          new Vector3(4.9f, 2.05f, 0f), 0.45f, 20);
        W09Kit.Label(clipMark.transform, "CLIP", new Vector3(0f, 0.45f, 0f), DemoKit.Pink);
        clipMark.SetActive(false);

        BugCracklingDemo demo = t.gameObject.AddComponent<BugCracklingDemo>();
        demo.source = hit;
        demo.mixerGroup = sfx;
        demo.levelMeter = levelFill;
        demo.modeText = modeText;
        demo.voiceText = voiceText;
        demo.gainText = gainText;
        demo.clipMark = clipMark;
    }

    // ------------------------------------------------------------------ slide 10: the walkaround

    /// <summary>The lab brief in one screen: a campfire and a coin as 3D sources, the player's own
    /// 2D footsteps, and a 2D BGM track - four routing decisions sounding at the same time.</summary>
    public static void BuildWalkaround(Transform t)
    {
        W09Kit.Camera(t);

        GameObject player = DemoKit.Character(t, "Player", new Vector2(0f, 0f), new Vector2(1.15f, 1.15f), 6);

        AudioListener ears = W09Kit.Listener(t, Vector3.zero, "");
        Transform marker = ears.transform.Find("Marker");
        if (marker != null) marker.gameObject.SetActive(false);   // the player sprite is the marker here

        AudioMixerGroup sfx = W09Kit.Group("SFX");
        AudioMixerGroup bgmGroup = W09Kit.Group("BGM");

        AudioSource fire = W09Kit.Emitter(t, "Campfire", "fire_loop", new Vector3(-6f, 2f, 0f),
                                          true, 2f, 30f, sfx, DemoKit.Orange, true);
        AudioSource coin = W09Kit.Emitter(t, "Coin", "coin", new Vector3(6f, -1f, 0f),
                                          true, 1f, 15f, sfx, DemoKit.Green);

        AudioClip[] stepClips = W09Kit.Clips("footstep_dirt_1", "footstep_dirt_2", "footstep_dirt_3");
        AudioSource steps = W09Kit.Source(t, "Steps", stepClips.Length > 0 ? stepClips[0] : null,
                                          Vector3.zero, false, 1f, 10f, sfx, false, 0.9f);

        AudioSource bgm = W09Kit.Source(t, "Bgm", W09Kit.Clip("bgm_loop"), Vector3.zero,
                                        false, 1f, 10f, bgmGroup, true, 0.7f);
        W09Kit.Icon(t, "BgmIcon", LabKit.Circle, DemoKit.Blue, new Vector3(6.4f, 3.4f, 0f), 0.5f, 10);
        W09Kit.Label(t, "bgm 2D", new Vector3(6.4f, 3.85f, 0f), DemoKit.Blue);

        W09Kit.Label(t, "MIX", new Vector3(2.6f, 3.55f, 0f), DemoKit.Dim);
        Transform mixFill;
        SpriteRenderer unused;
        W09Kit.Meter(t, "MixMeter", new Vector3(2.6f, 3.05f, 0f), 4.4f, DemoKit.Orange, out mixFill, out unused);
        TextMesh nearText = W09Kit.Label(t, "campfire", new Vector3(2.6f, 2.1f, 0f), DemoKit.Orange)
                                    .GetComponent<TextMesh>();

        WalkaroundDemo demo = t.gameObject.AddComponent<WalkaroundDemo>();
        demo.player = player.transform;
        demo.source = steps;
        demo.fireSource = fire;
        demo.coinSource = coin;
        demo.bgmSource = bgm;
        demo.stepClips = stepClips;
        demo.listener = ears;
        demo.mixerGroup = sfx;
        demo.mixMeter = mixFill;
        demo.nearText = nearText;
    }
}
