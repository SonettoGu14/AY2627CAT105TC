using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Reports what the pointer is doing to ONE UI element, so a demo can show the interaction state
/// live. uGUI keeps its own current selection state private, and a disabled Button stops sending
/// onClick while the pointer handlers still fire, so a demo needs both facts: the hover/press flags
/// from here, and Selectable.IsInteractable for "Disabled".
///
/// Lives on the element that owns the raycast target (the Button's / Image's own GameObject).
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class UiPointerProbe : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public bool Hovered { get; private set; }
    public bool Pressed { get; private set; }

    /// <summary>How many clicks this element has actually received. Zero once it stops being a raycast target.</summary>
    public int Clicks { get; private set; }

    public void OnPointerEnter(PointerEventData e) { Hovered = true; }
    public void OnPointerExit(PointerEventData e) { Hovered = false; Pressed = false; }
    public void OnPointerDown(PointerEventData e) { Pressed = true; Clicks++; }
    public void OnPointerUp(PointerEventData e) { Pressed = false; }

    /// <summary>The state name a Button shows, from what the pointer is doing plus interactability.</summary>
    public string State(Selectable selectable)
    {
        if (selectable != null && !selectable.IsInteractable()) return "Disabled";
        if (Pressed) return "Pressed";
        if (Hovered) return "Highlighted";
        return "Normal";
    }
}
