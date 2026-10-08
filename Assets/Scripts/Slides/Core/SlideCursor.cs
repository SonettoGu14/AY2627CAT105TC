/// Pure slide/step state machine - no Unity types, so it is unit-testable.
public class SlideCursor
{
    private readonly int[] blocksPerSlide;
    public SlideCursor(int[] blocksPerSlide) { this.blocksPerSlide = blocksPerSlide ?? new int[0]; }
    public int SlideCount => blocksPerSlide.Length;
    public int SlideIndex { get; private set; }
    public int RevealedBlocks { get; private set; }
    public int BlocksAt(int i) => (i >= 0 && i < SlideCount) ? blocksPerSlide[i] : 0;
    public bool StepsExhausted => RevealedBlocks >= BlocksAt(SlideIndex);

    public void JumpTo(int i) { SlideIndex = Clamp(i); RevealedBlocks = BlocksAt(SlideIndex); }
    public void NextSlide()   { SlideIndex = Clamp(SlideIndex + 1); RevealedBlocks = 0; }
    public void PrevSlide()   { SlideIndex = Clamp(SlideIndex - 1); RevealedBlocks = BlocksAt(SlideIndex); }

    public bool StepForward()
    {
        if (!StepsExhausted) { RevealedBlocks++; return true; }
        if (SlideIndex < SlideCount - 1) { NextSlide(); return true; }
        return false;
    }

    public bool StepBack()
    {
        if (RevealedBlocks > 0) { RevealedBlocks--; return true; }
        if (SlideIndex > 0) { PrevSlide(); return true; }
        return false;
    }

    private int Clamp(int i) => SlideCount == 0 ? 0 : (i < 0 ? 0 : (i > SlideCount - 1 ? SlideCount - 1 : i));
}
