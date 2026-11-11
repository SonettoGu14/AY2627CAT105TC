using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

/// <summary>
/// The end-to-end contract for the W9 (audio) deck. Same shape as the W8 test - every key the demo
/// map names must exist on the stage and activating it must select it - plus the two things that are
/// specific to audio:
///
///   * exactly ONE AudioListener may be active at a time. Every W9 demo brings its own camera and
///     its own ears, and the shared stage camera carries the listener the other weeks use, so a
///     mistake here is the classic "two listeners" bug: Unity picks one and the mix is nonsense.
///   * a silent demo is the week's whole subject, so every AudioSource on the stage must actually
///     have a clip - a missing WAV reference would otherwise only show up as silence in class.
/// </summary>
public class W09SlidesPlayTests
{
    private const string SceneName = "W09Slides";

    [UnityTest]
    public IEnumerator EveryDemoKeyInTheMapActivates()
    {
        SceneManager.LoadScene(SceneName);
        yield return null;

        SlidePresenter presenter = Object.FindObjectOfType<SlidePresenter>();
        DemoStage stage = Object.FindObjectOfType<DemoStage>();
        SlideDemoMap map = Object.FindObjectOfType<SlideDemoMap>();
        Assert.IsNotNull(presenter, "no SlidePresenter in " + SceneName);
        Assert.IsNotNull(stage, "no DemoStage in " + SceneName);
        Assert.IsNotNull(map, "no SlideDemoMap in " + SceneName);

        SlideDeckPlayer player = presenter.player;
        Assert.IsNotNull(player, "the presenter has no deck player");
        Assert.AreEqual(map.perSlide.Length, player.Deck.slides.Length,
            "the demo map and the deck disagree on how many slides there are");

        int checkedSlides = 0, checkedDemos = 0;
        for (int i = 0; i < map.perSlide.Length; i++)
        {
            string csv = map.perSlide[i];
            if (string.IsNullOrEmpty(csv)) continue;
            checkedSlides++;

            string[] keys = csv.Split(',');
            for (int k = 0; k < keys.Length; k++)
            {
                checkedDemos++;
                presenter.SetDeckVisible(true);
                player.JumpTo(i);
                presenter.SetDeckVisible(false);
                stage.Jump(k);

                string want = keys[k].Trim();
                Assert.AreEqual(want, stage.ActiveKey,
                    "slide " + (i + 1) + " asked for demo '" + want + "' but the stage selected '" +
                    stage.ActiveKey + "' - the key is probably a typo, or the scene needs rebuilding");
                yield return null;
            }
        }

        Assert.AreEqual(8, checkedSlides, "the W9 deck has demos on slides 3-10");
        Assert.AreEqual(17, checkedDemos, "the W9 deck has 17 demo slots");

        presenter.SetDeckVisible(true);
    }

    /// <summary>While a W9 demo runs, it must be the only thing listening: its own camera is on, the
    /// shared stage camera (and the listener it carries) is off. And while the deck is up, nothing
    /// should be listening - the demo roots are inactive.</summary>
    [UnityTest]
    public IEnumerator OnlyOneAudioListenerIsActiveWhileADemoRuns()
    {
        SceneManager.LoadScene(SceneName);
        yield return null;

        SlidePresenter presenter = Object.FindObjectOfType<SlidePresenter>();
        DemoStage stage = Object.FindObjectOfType<DemoStage>();
        SlideDemoMap map = Object.FindObjectOfType<SlideDemoMap>();
        Assert.IsNotNull(presenter, "no SlidePresenter in " + SceneName);
        Assert.IsNotNull(stage, "no DemoStage in " + SceneName);
        Assert.IsNotNull(map, "no SlideDemoMap in " + SceneName);

        // While the deck is up the demo roots are inactive, so the only listener left is the one on the
        // shared stage camera - and it hears nothing, because every source belongs to a demo.
        presenter.SetDeckVisible(true);
        yield return null;
        AudioListener[] idle = Object.FindObjectsOfType<AudioListener>();
        Assert.AreEqual(1, idle.Length, "only the shared stage camera should listen while the deck is up");
        Assert.AreSame(stage.stageCamera, idle[0].GetComponent<Camera>(),
            "the listener that stays active while the deck is up should be the shared stage camera's");

        for (int i = 0; i < map.perSlide.Length; i++)
        {
            string csv = map.perSlide[i];
            if (string.IsNullOrEmpty(csv)) continue;

            string[] keys = csv.Split(',');
            for (int k = 0; k < keys.Length; k++)
            {
                presenter.SetDeckVisible(true);
                presenter.player.JumpTo(i);
                presenter.SetDeckVisible(false);
                stage.Jump(k);
                yield return null;

                AudioListener[] listeners = Object.FindObjectsOfType<AudioListener>();
                // bug_no_listener IS the bug it demonstrates: it starts with its EARS object
                // inactive on purpose (L brings it back), so it is the one demo that must hear
                // nothing. Every other demo must be the only thing listening.
                int want = stage.ActiveKey == "bug_no_listener" ? 0 : 1;
                Assert.AreEqual(want, listeners.Length,
                    "demo '" + stage.ActiveKey + "' found " + listeners.Length +
                    " active AudioListener(s), expected " + want);
                if (want == 0) continue;
                // stage.Index is the position within THIS slide's key list, not a slot index - find
                // the slot by key instead.
                Transform root = null;
                foreach (DemoStage.Slot slot in stage.slots)
                    if (slot.key == stage.ActiveKey) { root = slot.root.transform; break; }
                Assert.IsNotNull(root, "no stage slot for '" + stage.ActiveKey + "'");
                Assert.IsTrue(listeners[0].transform.IsChildOf(root),
                    "the active listener '" + listeners[0].name + "' (path " + Path(listeners[0].transform) +
                    ") belongs to another demo than '" + stage.ActiveKey + "'" +
                    "; expected under " + Path(root));
            }
        }

        presenter.SetDeckVisible(true);
    }

    /// <summary>Every AudioSource on the stage must have a clip, and every demo must bring its own
    /// camera (that is what keeps the sharing stage camera - with its listener - switched off).</summary>
    [UnityTest]
    public IEnumerator EverySourceHasAClipAndEveryDemoItsOwnCamera()
    {
        SceneManager.LoadScene(SceneName);
        yield return null;

        DemoStage stage = Object.FindObjectOfType<DemoStage>();
        Assert.IsNotNull(stage, "no DemoStage in " + SceneName);
        Assert.AreEqual(17, stage.slots.Length, "the W9 stage should carry 17 demos");

        int sources = 0;
        foreach (DemoStage.Slot slot in stage.slots)
        {
            Assert.IsNotNull(slot.camera, "demo '" + slot.key + "' has no camera of its own");
            foreach (AudioSource source in slot.root.GetComponentsInChildren<AudioSource>(true))
            {
                sources++;
                Assert.IsNotNull(source.clip,
                    "demo '" + slot.key + "' has an AudioSource with no clip - " +
                    "a missing Assets/Audio/W09 wav, or the scene needs rebuilding");
            }
        }
        Assert.IsTrue(sources >= 17, "the W9 demos should carry at least one AudioSource each, found " + sources);
    }

    /// <summary>The deck itself: the canvas must be wired to the W9 JSON (title + page count), the
    /// footstep page must show its code as text rather than as a picture nobody renders, and the
    /// "this page has a demo" badge must count the keys in the map.</summary>
    [UnityTest]
    public IEnumerator TheDeckShowsTheW09TitleAndTheFootstepCode()
    {
        SceneManager.LoadScene(SceneName);
        yield return null;

        SlidePresenter presenter = Object.FindObjectOfType<SlidePresenter>();
        SlideView view = Object.FindObjectOfType<SlideView>();
        Assert.IsNotNull(presenter, "no SlidePresenter in " + SceneName);
        Assert.IsNotNull(view, "no SlideView in " + SceneName);
        SlideDeckPlayer player = presenter.player;
        Assert.IsNotNull(player, "the presenter has no deck player");
        Assert.AreEqual(11, player.Deck.slides.Length, "the W9 deck has 11 slides");

        presenter.SetDeckVisible(true);
        player.JumpTo(0);
        yield return null;
        Assert.AreEqual(player.Deck.slides[0].title, view.titleText.text, "slide 1 should show the deck title");
        Assert.AreEqual("1 / 11", view.pageText.text, "the page label should say 1 / 11");

        player.JumpTo(6);   // slide 7, the footstep manager worked example
        yield return null;
        string body = view.bodyText.text;
        StringAssert.Contains("PlayOneShot", body, "the footstep page should show its script as code");
        StringAssert.Contains("Random.Range(0.9f, 1.1f)", body, "the pitch randomisation line is missing");
        StringAssert.Contains("nextStep = Time.time + 0.4f", body, "the throttle line is missing");

        player.JumpTo(3);   // slide 4: one demo, badge says DEMO
        yield return null;
        Assert.IsTrue(view.demoBadge.activeSelf, "slide 4 has a demo, so the badge should be up");
        Assert.AreEqual("\u2022 DEMO", view.demoBadgeText.text);
        player.JumpTo(4);   // slide 5: three demos
        yield return null;
        Assert.AreEqual("\u2022 3 DEMOS", view.demoBadgeText.text);
        player.JumpTo(1);   // slide 2: agenda, no demo
        yield return null;
        Assert.IsFalse(view.demoBadge.activeSelf, "the agenda has no demo, so no badge");

        presenter.SetDeckVisible(true);
    }

    private static string Path(Transform t)
    {
        string path = t.name;
        for (Transform p = t.parent; p != null; p = p.parent) path = p.name + "/" + path;
        return path;
    }
}
