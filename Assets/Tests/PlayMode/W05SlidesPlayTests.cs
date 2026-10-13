using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

/// <summary>
/// The end-to-end contract for the W5 deck: every key its demo map names must exist on the demo
/// stage, and activating it must actually select it.
///
/// This is the test that catches a typo in `<deck>.demos.json`. A mistyped key does not throw - the
/// stage logs a warning and shows nothing - so a lecture slide would just be blank, and nobody would
/// notice until the lesson. It reads the built scene, which is committed, so it also fails if the
/// scene was never rebuilt after a map edit.
///
/// The W5 LAB has no automated test: its scripts live in Assembly-CSharp, which a test assembly
/// cannot reference (the same limitation as PlayerController2D). Its UI was verified by hand -
/// the bar eased to the right value, the bag Instantiated 8 slots and Destroyed them, the dialogue
/// zone raised its alpha, and pausing disabled the player and zeroed the clock.
/// </summary>
public class W05SlidesPlayTests
{
    private const string SceneName = "W05Slides";

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
                    "slide " + (i + 1) + " asked for demo '" + want + "' but the stage selected '" + stage.ActiveKey +
                    "' - the key is probably a typo, or the scene needs rebuilding");
            }
        }

        Assert.Greater(checkedSlides, 20, "the W5 deck should have demos on most slides");
        Assert.Greater(checkedDemos, 25, "the W5 deck should exercise at least its 25 new demos");

        presenter.SetDeckVisible(true);
    }
}
