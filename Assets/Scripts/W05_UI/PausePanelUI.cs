using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// W5 slide "Canvas Group": a whole panel switched off at once.
///
/// The pause panel is the honest use of it - alpha, interactable and blocksRaycasts all have to
/// change together, and doing that by hand on every child is exactly what the Canvas Group saves
/// you from.
///
/// The player controller is disabled while paused on purpose: Time.timeScale = 0 stops FixedUpdate
/// but NOT Update, so a controller left enabled keeps reading input and would fire a jump the
/// moment the game resumes. (The slides scene relies on the same fact.)
/// </summary>
public class PausePanelUI : MonoBehaviour
{
    public CanvasGroup panel;
    public Button button;
    public PlayerController2D player;
    public KeyCode key = KeyCode.P;

    private bool paused;

    void Awake()
    {
        if (button != null) button.onClick.AddListener(Toggle);
        Apply();
    }

    void Update()
    {
        if (Input.GetKeyDown(key)) Toggle();
    }

    public void Toggle() { SetPaused(!paused); }

    public void SetPaused(bool value)
    {
        paused = value;
        Time.timeScale = paused ? 0f : 1f;
        if (player != null) player.enabled = !paused;
        Apply();
    }

    private void Apply()
    {
        if (panel == null) return;
        panel.alpha = paused ? 1f : 0f;
        panel.interactable = paused;
        panel.blocksRaycasts = paused;
    }

    public bool IsPaused { get { return paused; } }
}
