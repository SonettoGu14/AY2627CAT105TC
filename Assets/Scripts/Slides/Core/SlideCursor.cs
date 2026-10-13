/// <summary>
/// Pure slide-paging state machine - no Unity types, so it is unit-testable.
///
/// One slide at a time, and nothing else: a slide is shown **whole**. There is no bullet-by-bullet
/// reveal - the decks have never used one, so the cursor does not model one.
/// </summary>
public class SlideCursor
{
    private readonly int slideCount;

    public SlideCursor(int slideCount) { this.slideCount = slideCount < 0 ? 0 : slideCount; }

    public int SlideCount => slideCount;
    public int SlideIndex { get; private set; }

    /// <summary>Go straight to a slide, clamped into range. Used by the jump-to-slide grid.</summary>
    public void JumpTo(int i) { SlideIndex = Clamp(i); }

    // Clamp at the ends rather than wrapping: pressing right on the last slide must do nothing.
    public void NextSlide() { if (SlideIndex < slideCount - 1) SlideIndex++; }
    public void PrevSlide() { if (SlideIndex > 0) SlideIndex--; }

    private int Clamp(int i) => slideCount == 0 ? 0 : (i < 0 ? 0 : (i > slideCount - 1 ? slideCount - 1 : i));
}
