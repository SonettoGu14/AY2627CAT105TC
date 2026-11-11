using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

/// <summary>
/// The end-to-end contract for the W8 deck: every key its demo map names must exist on the demo
/// stage, and activating it must actually select it. This is the test that catches a typo in
/// <c>W08_L8.demos.json</c> - a mistyped key does not throw, it just leaves a slide blank.
/// </summary>
public class W08SlidesPlayTests
{
    private const string SceneName = "W08Slides";

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

        Assert.AreEqual(10, checkedSlides, "the W8 deck should have demos on slides 4-13");
        Assert.AreEqual(18, checkedDemos, "the W8 deck has 18 demo slots");

        presenter.SetDeckVisible(true);
    }

    /// <summary>
    /// The badge is the global "this page has a live demo" rule: one demo -> "• DEMO", three ->
    /// "• 3 DEMOS", no demo -> hidden. It is driven by the same demo map the presenter uses.
    /// </summary>
    [UnityTest]
    public IEnumerator TheBadgeMarksSlidesThatHaveDemos()
    {
        SceneManager.LoadScene(SceneName);
        yield return null;

        SlidePresenter presenter = Object.FindObjectOfType<SlidePresenter>();
        SlideView view = Object.FindObjectOfType<SlideView>();
        Assert.IsNotNull(presenter, "no SlidePresenter in " + SceneName);
        Assert.IsNotNull(view, "no SlideView in " + SceneName);
        Assert.IsNotNull(presenter.player, "the presenter has no deck player");
        Assert.IsNotNull(view.demoBadge, "the scene has no demo badge - rebuild it with Build W08Slides");

        presenter.SetDeckVisible(true);

        presenter.player.JumpTo(3);                 // slide 4: mesh_anatomy
        yield return null;
        Assert.IsTrue(view.demoBadge.activeSelf, "slide 4 has a demo and should show the badge");
        Assert.AreEqual("\u2022 DEMO", view.demoBadgeText.text);

        presenter.player.JumpTo(7);                 // slide 8: three demos
        yield return null;
        Assert.AreEqual("\u2022 3 DEMOS", view.demoBadgeText.text,
            "the badge should count the demos on the slide");

        presenter.player.JumpTo(0);                 // cover: no demo
        yield return null;
        Assert.IsFalse(view.demoBadge.activeSelf, "the cover has no demo and should hide the badge");
    }
}
