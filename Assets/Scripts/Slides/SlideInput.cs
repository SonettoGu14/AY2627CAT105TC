using UnityEngine; using UnityEngine.EventSystems;
/// <summary>
/// Maps keys to slide actions while the deck is visible, and to demo selection while it is hidden.
/// The demo itself reads its own keys (the readout panel lists them).
/// </summary>
public class SlideInput : MonoBehaviour
{
    public SlideDeckPlayer player; public SlidePresenter presenter; public SlideNavigator navigator;
    public DemoStage demoStage;

    void Update()
    {
        if (presenter == null || player == null) return;
        if (Input.GetKeyDown(KeyCode.Tab)) { presenter.Toggle(); return; }

        if (!presenter.DeckVisible)
        {
            // the deck is hidden: the demos for this slide are showing
            if (demoStage != null && demoStage.IsShowing)
            {
                if (Input.GetKeyDown(KeyCode.Comma)) demoStage.Previous();
                if (Input.GetKeyDown(KeyCode.Period)) demoStage.Next();
                for (int i = 0; i < 9; i++)
                    if (Input.GetKeyDown(KeyCode.Alpha1 + i)) demoStage.Jump(i);
            }
            return;                       // everything else belongs to the demo
        }

        if (navigator != null && Input.GetKeyDown(KeyCode.G)) { navigator.ToggleJumpPanel(); return; }
        if (navigator != null && navigator.JumpPanelOpen) { if (Input.GetKeyDown(KeyCode.Escape)) navigator.ToggleJumpPanel(); return; }
        if (Input.GetKeyDown(KeyCode.RightArrow))   player.NextSlide();
        if (Input.GetKeyDown(KeyCode.LeftArrow))    player.PrevSlide();
        if (Input.GetKeyDown(KeyCode.Backspace))    player.StepBack();
        // The mouse shortcut must not fire when the click landed on UI (e.g. the Toggle button).
        bool pointerOverUi = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        if (Input.GetKeyDown(KeyCode.Space) || (!pointerOverUi && Input.GetMouseButtonDown(0))) player.StepForward();
    }
}
