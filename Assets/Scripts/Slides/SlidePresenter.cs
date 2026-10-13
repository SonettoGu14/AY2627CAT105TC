using TMPro; using UnityEngine;
public class SlidePresenter : MonoBehaviour
{
    public GameObject slidesRoot;       // deck canvas root
    public SlideDeckPlayer player;      // which slide we are on
    public DemoStage demoStage;         // the demos behind the deck
    public SlideDemoMap demoMap;        // which demos belong to which slide
    public TMP_Text toggleLabel;
    public string hideLabel = "Hide slides  (Tab)", showLabel = "Show slides  (Tab)";
    public bool DeckVisible { get; private set; } = true;

    void Start() => SetDeckVisible(true);

    public void Toggle() => SetDeckVisible(!DeckVisible);

    public void SetDeckVisible(bool visible)
    {
        DeckVisible = visible;
        if (slidesRoot != null) slidesRoot.SetActive(visible);

        // Show the demos belonging to the CURRENT slide, or none. Deactivating the demo roots is
        // also what makes the frozen game's input genuinely inert: Time.timeScale alone does not
        // stop Update(), so an active PlayerController2D would keep latching Space into
        // `jumpQueued` while the deck is up and fire a jump the moment the deck hid.
        if (demoStage != null)
        {
            if (visible) demoStage.Hide();
            else demoStage.Show(demoMap != null && player != null ? demoMap.For(player.SlideIndex) : "");
        }

        Time.timeScale = visible ? 0f : 1f;
        if (toggleLabel != null) toggleLabel.text = visible ? hideLabel : showLabel;
    }
}
