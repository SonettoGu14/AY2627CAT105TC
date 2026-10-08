using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// The blackboard next to a demo: what it teaches, the live values, the key hint, and which of the
/// slide's demos you are looking at ("demo 2 / 3").
///
/// Lives on the Chrome canvas so it is visible while the deck is hidden, and it hides itself while
/// the deck is up.
/// </summary>
public class DemoReadout : MonoBehaviour
{
    public DemoStage stage;
    public SlidePresenter presenter;
    public GameObject panel;
    public TMP_Text titleText;
    public TMP_Text readoutText;
    public TMP_Text keysText;
    public TMP_Text indexText;

    private readonly List<string> lines = new List<string>();

    private void Update()
    {
        bool show = presenter != null && !presenter.DeckVisible && stage != null && stage.IsShowing && stage.Active != null;
        if (panel != null && panel.activeSelf != show) panel.SetActive(show);
        if (!show) return;

        DemoBase demo = stage.Active;
        if (titleText != null) titleText.text = demo.Title;
        if (keysText != null) keysText.text = demo.Keys;
        if (indexText != null)
            indexText.text = stage.Count > 1 ? "demo " + (stage.Index + 1) + " / " + stage.Count + "    , .  switch" : "";

        lines.Clear();
        demo.Readout(lines);
        if (readoutText != null) readoutText.text = string.Join("\n", lines.ToArray());
    }
}
