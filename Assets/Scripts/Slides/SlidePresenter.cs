using TMPro; using UnityEngine;
public class SlidePresenter : MonoBehaviour
{
    public GameObject slidesRoot;       // deck canvas root
    public TMP_Text toggleLabel;
    public string hideLabel = "Hide slides  (Tab)", showLabel = "Show slides  (Tab)";
    public bool DeckVisible { get; private set; } = true;

    void Start() => SetDeckVisible(true);

    public void Toggle() => SetDeckVisible(!DeckVisible);

    public void SetDeckVisible(bool visible)
    {
        DeckVisible = visible;
        if (slidesRoot != null) slidesRoot.SetActive(visible);
        Time.timeScale = visible ? 0f : 1f;          // freeze the game under the deck
        if (toggleLabel != null) toggleLabel.text = visible ? hideLabel : showLabel;
    }
}
