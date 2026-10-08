using UnityEngine; using UnityEngine.EventSystems;
/// Maps keys to slide actions while the deck is visible; inert while the game is showing.
public class SlideInput : MonoBehaviour
{
    public SlideDeckPlayer player; public SlidePresenter presenter;
    void Update()
    {
        if (presenter == null || player == null) return;
        if (Input.GetKeyDown(KeyCode.Tab)) { presenter.Toggle(); return; }
        if (!presenter.DeckVisible) return;
        if (Input.GetKeyDown(KeyCode.RightArrow))   player.NextSlide();
        if (Input.GetKeyDown(KeyCode.LeftArrow))    player.PrevSlide();
        if (Input.GetKeyDown(KeyCode.Backspace))    player.StepBack();
        // The mouse shortcut must not fire when the click landed on UI (e.g. the Toggle button).
        bool pointerOverUi = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        if (Input.GetKeyDown(KeyCode.Space) || (!pointerOverUi && Input.GetMouseButtonDown(0))) player.StepForward();
    }
}
