using NUnit.Framework; using UnityEditor; using UnityEngine;
public class DeckLoadTests
{
    static SlideDeckData Load(string path)
    {
        var ta = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
        Assert.IsNotNull(ta, path + " is missing");
        var deck = JsonUtility.FromJson<SlideDeckData>(ta.text);
        Assert.IsNotNull(deck, path + " did not deserialise");
        return deck;
    }

    static SlideDeckData Load() { return Load("Assets/Slides/W04_L4.json"); }

    [Test] public void ParsesTheRealW04Deck()
    {
        var d = Load();
        Assert.AreEqual("W04_L4", d.key);
        Assert.AreEqual("Animations and Camera", d.title);
        Assert.AreEqual("Lecture 4", d.subtitle);
        Assert.AreEqual(28, d.slides.Length);
    }

    [Test] public void SlideIndexesAreOneBasedAndContiguous()
    {
        var d = Load();
        for (int i = 0; i < d.slides.Length; i++) Assert.AreEqual(i + 1, d.slides[i].index);
    }

    [Test] public void TitleSlidesCarryTheSubtitleAndNoBlocks()
    {
        var d = Load();
        foreach (var s in new[] { d.slides[0], d.slides[27] })
        {
            Assert.AreEqual("title", s.layout);
            Assert.AreEqual("Lecture 4", s.subtitle);
            Assert.AreEqual(0, s.blocks.Length);
        }
    }

    [Test] public void CodeBlocksSurviveDeserialisation()
    {
        var d = Load();
        int code = 0;
        foreach (var s in d.slides) foreach (var b in s.blocks) if (b.kind == "code") code++;
        Assert.AreEqual(4, code, "expected the 4 animator.* code blocks");
    }

    // ------------------------------------------------------------------ W5

    [Test] public void ParsesTheRealW05Deck()
    {
        var d = Load("Assets/Slides/W05_L5.json");
        Assert.AreEqual("W05_L5", d.key);
        Assert.AreEqual("UI", d.title);
        Assert.AreEqual("Lecture 5", d.subtitle);
        Assert.AreEqual(34, d.slides.Length, "33 from the .pptx plus the inserted camera page");
    }

    /// <summary>
    /// The camera recap was added to the .pptx by hand. If someone re-converts from a .pptx that
    /// does not have it, this page disappears and slide 3 becomes "Prefab" again - so pin it.
    /// </summary>
    [Test] public void TheW05CameraPageIsStillSlideThree()
    {
        var d = Load("Assets/Slides/W05_L5.json");
        Assert.AreEqual("Camera: Perspective vs Orthographic", d.slides[2].title);
        Assert.AreEqual("content", d.slides[2].layout);
        Assert.IsTrue(d.slides[2].blocks.Length >= 6, "the page should carry the comparison bullets");
        Assert.AreEqual("Review: Animation System", d.slides[1].title);
        Assert.AreEqual("Prefab", d.slides[3].title);
    }

    /// <summary>
    /// The shipped font (LiberationSans SDF) has no CJK glyphs, so a non-ASCII character renders as
    /// an empty box. W5's deck is entirely English; this catches a stray Chinese character getting
    /// into a slide during authoring.
    /// </summary>
    [Test] public void TheW05DeckIsEntirelyAscii()
    {
        var d = Load("Assets/Slides/W05_L5.json");
        System.Text.RegularExpressions.Regex cjk = new System.Text.RegularExpressions.Regex(@"[\u4e00-\u9fff\u3040-\u30ff\uac00-\ud7af]");
        foreach (var s in d.slides)
            Assert.IsFalse(cjk.IsMatch(JsonUtility.ToJson(s)),
                "slide " + s.index + " contains a CJK character the font cannot draw");
    }

    /// <summary>The demo map must line up with the deck one-for-one, or a slide silently shows nothing.</summary>
    [Test] public void TheW05DemoMapHasOneEntryPerSlide()
    {
        var d = Load("Assets/Slides/W05_L5.json");
        var ta = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Slides/W05_L5.demos.json");
        Assert.IsNotNull(ta, "W05_L5.demos.json is missing");
        var map = JsonUtility.FromJson<DemoMap>(ta.text);
        Assert.IsNotNull(map);
        Assert.AreEqual(d.slides.Length, map.perSlide.Length,
            "the demo map must have exactly one entry per slide");

        // the pages that legitimately show nothing: cover, review, contents, the two repeated
        // Prefab pages, and The End
        int[] emptyExpected = { 1, 2, 6, 23, 24, 34 };
        foreach (int i in emptyExpected)
            Assert.AreEqual("", map.perSlide[i - 1], "slide " + i + " should have no demo");
    }

    [System.Serializable] private class DemoMap { public string[] perSlide; }

    // ------------------------------------------------------------------ W8

    [Test] public void ParsesTheRealW08Deck()
    {
        var d = Load("Assets/Slides/W08_L8.json");
        Assert.AreEqual("W08_L8", d.key);
        Assert.AreEqual("3D Rendering and Materials", d.title);
        StringAssert.StartsWith("Lecture 8", d.subtitle);
        Assert.AreEqual(15, d.slides.Length);
    }

    [Test] public void TheW08DeckIsEntirelyAscii()
    {
        var d = Load("Assets/Slides/W08_L8.json");
        System.Text.RegularExpressions.Regex cjk =
            new System.Text.RegularExpressions.Regex(@"[\u4e00-\u9fff\u3040-\u30ff\uac00-\ud7af]");
        foreach (var s in d.slides)
            Assert.IsFalse(cjk.IsMatch(JsonUtility.ToJson(s)),
                "slide " + s.index + " contains a CJK character the font cannot draw");
    }

    [Test] public void TheW08DemoMapHasOneEntryPerSlide()
    {
        var d = Load("Assets/Slides/W08_L8.json");
        var ta = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Slides/W08_L8.demos.json");
        Assert.IsNotNull(ta, "W08_L8.demos.json is missing");
        var map = JsonUtility.FromJson<DemoMap>(ta.text);
        Assert.IsNotNull(map);
        Assert.AreEqual(d.slides.Length, map.perSlide.Length,
            "the demo map must have exactly one entry per slide");

        // the cover, agenda, the "where it fits" page, summary and The End have no live demo
        foreach (int i in new[] { 1, 2, 3, 14, 15 })
            Assert.AreEqual("", map.perSlide[i - 1], "slide " + i + " should have no demo");

        Assert.AreEqual("mesh_anatomy", map.perSlide[3]);
        Assert.AreEqual("pbr_metallic,pbr_smoothness,pbr_normalmap", map.perSlide[7]);
        Assert.AreEqual("studio", map.perSlide[12]);
    }

    // ------------------------------------------------------------------ W9

    [Test] public void ParsesTheRealW09Deck()
    {
        var d = Load("Assets/Slides/W09_L9.json");
        Assert.AreEqual("W09_L9", d.key);
        Assert.AreEqual("Audio System", d.title);
        StringAssert.StartsWith("Lecture 9", d.subtitle);
        Assert.AreEqual(11, d.slides.Length);
    }

    [Test] public void TheW09DeckIsEntirelyAscii()
    {
        var d = Load("Assets/Slides/W09_L9.json");
        System.Text.RegularExpressions.Regex cjk =
            new System.Text.RegularExpressions.Regex(@"[\u4e00-\u9fff\u3040-\u30ff\uac00-\ud7af]");
        foreach (var s in d.slides)
            Assert.IsFalse(cjk.IsMatch(JsonUtility.ToJson(s)),
                "slide " + s.index + " contains a CJK character the font cannot draw");
    }

    /// <summary>
    /// Slide 7 (the footstep manager) was a code screenshot in the .pptx, and SlideView draws no
    /// images - so the picture was transcribed into code blocks by hand. If someone re-converts the
    /// deck, the image comes back and the page projects blank; pin the transcription instead.
    /// </summary>
    [Test] public void TheW09FootstepPageIsCodeNotAnImage()
    {
        var d = Load("Assets/Slides/W09_L9.json");
        var s = d.slides[6];
        Assert.AreEqual("Worked Example: A Footstep Manager", s.title);
        // JsonUtility cannot express null for a class field - "image": null (or a missing key)
        // deserialises as an empty SlideImage, so "no image" means "no image path".
        Assert.IsTrue(s.image == null || string.IsNullOrEmpty(s.image.path),
            "the screenshot should have been replaced by code blocks");
        Assert.IsTrue(s.blocks.Length >= 15, "the whole script should be transcribed");
        foreach (var b in s.blocks) Assert.AreEqual("code", b.kind);

        string code = "";
        foreach (var b in s.blocks) code += b.text + "\n";
        StringAssert.Contains("PlayOneShot", code);
        StringAssert.Contains("Random.Range(0.9f, 1.1f)", code);
        StringAssert.Contains("nextStep = Time.time + 0.4f", code);
    }

    [Test] public void TheW09DemoMapHasOneEntryPerSlide()
    {
        var d = Load("Assets/Slides/W09_L9.json");
        var ta = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Slides/W09_L9.demos.json");
        Assert.IsNotNull(ta, "W09_L9.demos.json is missing");
        var map = JsonUtility.FromJson<DemoMap>(ta.text);
        Assert.IsNotNull(map);
        Assert.AreEqual(d.slides.Length, map.perSlide.Length,
            "the demo map must have exactly one entry per slide");

        // cover, agenda and The End have no live demo
        foreach (int i in new[] { 1, 2, 11 })
            Assert.AreEqual("", map.perSlide[i - 1], "slide " + i + " should have no demo");

        Assert.AreEqual("why_sound", map.perSlide[2]);
        Assert.AreEqual("play_replace,oneshot_layer,pitch_vary", map.perSlide[4]);
        Assert.AreEqual("spatial_blend,min_max,rolloff", map.perSlide[5]);
        Assert.AreEqual("mixer_groups,mixer_bypass", map.perSlide[7]);
        Assert.AreEqual("bug_no_listener,bug_min_distance,bug_output_none,bug_machine_gun,bug_crackling",
                        map.perSlide[8]);
        Assert.AreEqual("walkaround", map.perSlide[9]);
    }
}
