using UnityEngine;

/// <summary>
/// Which demos belong to which slide. One entry per slide, a comma-separated list of demo keys:
///
///   perSlide[15] = "am_states,am_default"      // slide 16
///
/// SlideDeckBuilder fills this from Assets/Slides/&lt;key&gt;.demos.json when it builds the scene, so the
/// mapping can be edited without touching the builder. A slide with no entry simply shows nothing
/// when the deck is hidden.
/// </summary>
public class SlideDemoMap : MonoBehaviour
{
    public string[] perSlide;

    public string For(int slideIndex)
    {
        if (perSlide == null || slideIndex < 0 || slideIndex >= perSlide.Length) return "";
        return perSlide[slideIndex];
    }
}
