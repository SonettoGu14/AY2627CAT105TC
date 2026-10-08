using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// PlayMode coverage for the slides runtime — the layer the EditMode tests cannot reach.
///
/// The regression these pin down: while the deck is up the demo roots must be INACTIVE.
/// `Time.timeScale = 0` does **not** stop `Update()`, so a demo left active keeps running its
/// Update and can latch input. That is exactly how "Space advances a bullet AND queues a jump"
/// reached the final review: eight task reviews never exercised the presenter against a live game.
///
/// `PlayerController2D` lives in `Assembly-CSharp`, which a test assembly cannot reference, so these
/// tests pin the mechanism the fix relies on — the demo root (which holds the game) is off while the
/// deck is up — rather than the controller itself.
/// </summary>
public class SlidePresenterPlayTests
{
    private const string TwoSlideDeck =
        "{\"key\":\"t\",\"title\":\"t\",\"subtitle\":\"\",\"source\":\"\",\"generated\":\"\"," +
        "\"slides\":[" +
        "{\"index\":1,\"layout\":\"content\",\"title\":\"one\",\"subtitle\":\"\",\"blocks\":[]}," +
        "{\"index\":2,\"layout\":\"content\",\"title\":\"two\",\"subtitle\":\"\"," +
        "\"blocks\":[{\"level\":0,\"text\":\"a\",\"kind\":\"bullet\"},{\"level\":0,\"text\":\"b\",\"kind\":\"bullet\"}]}" +
        "]}";

    private GameObject root;
    private GameObject deckCanvas;
    private GameObject demoRoot;
    private SlidePresenter presenter;
    private SlideDeckPlayer player;
    private DemoStage stage;

    [SetUp]
    public void SetUp()
    {
        Time.timeScale = 1f;

        root = new GameObject("test root");
        root.SetActive(false);                 // so Awake does not run before the fields are assigned

        deckCanvas = new GameObject("deck canvas");
        deckCanvas.transform.SetParent(root.transform, false);

        demoRoot = new GameObject("demo root");
        demoRoot.transform.SetParent(root.transform, false);
        demoRoot.SetActive(false);

        GameObject stageGo = new GameObject("demo stage");
        stageGo.transform.SetParent(root.transform, false);
        stage = stageGo.AddComponent<DemoStage>();
        stage.slots = new[]
        {
            new DemoStage.Slot { key = "d1", root = demoRoot, anchor = Vector2.zero, orthoSize = 5f }
        };

        player = root.AddComponent<SlideDeckPlayer>();
        player.deckJson = new TextAsset(TwoSlideDeck);

        SlideDemoMap map = root.AddComponent<SlideDemoMap>();
        map.perSlide = new[] { "", "d1", "d1" };

        presenter = root.AddComponent<SlidePresenter>();
        presenter.slidesRoot = deckCanvas;
        presenter.player = player;
        presenter.demoStage = stage;
        presenter.demoMap = map;

        root.SetActive(true);                  // Awake runs here: SlideDeckPlayer parses the deck
    }

    [TearDown]
    public void TearDown()
    {
        Time.timeScale = 1f;                   // never leave the editor frozen for the next test
        Object.Destroy(root);
    }

    [UnityTest]
    public IEnumerator HidingTheDeckActivatesThatSlidesDemoAndUnfreezes()
    {
        presenter.SetDeckVisible(true);
        Assert.AreEqual(0f, Time.timeScale, "showing the deck should freeze the game clock");
        Assert.IsTrue(deckCanvas.activeSelf);
        Assert.IsFalse(demoRoot.activeSelf, "no demo may be active while the deck is up");

        player.JumpTo(1);                      // slide 2 is the one with a demo
        presenter.SetDeckVisible(false);
        yield return null;

        Assert.AreEqual(1f, Time.timeScale, "hiding the deck must unfreeze the game");
        Assert.IsFalse(deckCanvas.activeSelf);
        Assert.IsTrue(stage.IsShowing);
        Assert.AreEqual("d1", stage.ActiveKey);
        Assert.IsTrue(demoRoot.activeSelf,
            "the current slide's demo root must be active - and the game's own root being off while the deck is up is what makes its Update inert");
    }

    [UnityTest]
    public IEnumerator ReturningToTheDeckKeepsTheSameSlideAndStep()
    {
        presenter.SetDeckVisible(false);
        player.JumpTo(1);
        player.StepForward();
        int slide = player.SlideIndex;
        int revealed = player.RevealedBlocks;

        presenter.SetDeckVisible(true);
        yield return null;

        Assert.AreEqual(slide, player.SlideIndex, "the deck must come back on the same slide");
        Assert.AreEqual(revealed, player.RevealedBlocks, "and on the same revealed bullet");
        Assert.IsFalse(demoRoot.activeSelf, "and with the demo switched off again");
        Assert.AreEqual(0f, Time.timeScale);
    }

    [UnityTest]
    public IEnumerator ASlideWithOneDemoIgnoresNextInsteadOfThrowing()
    {
        player.JumpTo(1);                      // pick the slide first: the deck chooses a demo when it hides
        presenter.SetDeckVisible(false);
        Assert.AreEqual(1, stage.Count);

        stage.Next();                          // one demo: Next is a no-op, not a crash
        Assert.AreEqual("d1", stage.ActiveKey);
        yield return null;
    }
}
