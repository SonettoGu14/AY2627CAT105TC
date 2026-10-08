using TMPro; using UnityEngine;
public class SlidePresenter : MonoBehaviour
{
    public GameObject slidesRoot;       // deck canvas root
    public GameObject gameRoot;         // the gameplay root, deactivated while the deck is up
    public TMP_Text toggleLabel;
    public string hideLabel = "Hide slides  (Tab)", showLabel = "Show slides  (Tab)";
    public bool DeckVisible { get; private set; } = true;

    void Start() => SetDeckVisible(true);

    public void Toggle() => SetDeckVisible(!DeckVisible);

    public void SetDeckVisible(bool visible)
    {
        DeckVisible = visible;
        if (slidesRoot != null) slidesRoot.SetActive(visible);

        // Freezing with Time.timeScale alone is NOT enough. Update() still runs at timeScale 0 -
        // only FixedUpdate stops - so Update-based input would keep accumulating while the deck is
        // up. PlayerController2D latches Space into `jumpQueued` in Update and only consumes it in
        // FixedUpdate, so pressing Space to advance bullets would fire a jump the moment the deck
        // hid. Deactivating the gameplay root makes the game's Update genuinely inert. It is
        // invisible in practice because the deck's background covers the screen.
        if (gameRoot != null) gameRoot.SetActive(!visible);

        Time.timeScale = visible ? 0f : 1f;
        if (toggleLabel != null) toggleLabel.text = visible ? hideLabel : showLabel;
    }
}
