using NUnit.Framework;
public class SlideCursorTests
{
    [Test] public void StartsOnFirstSlideWithNothingRevealed()
    { var c = new SlideCursor(new[]{2,1}); Assert.AreEqual(0,c.SlideIndex); Assert.AreEqual(0,c.RevealedBlocks); }

    [Test] public void StepForwardRevealsThenRollsOver()
    {
        var c = new SlideCursor(new[]{2,1});
        Assert.IsTrue(c.StepForward()); Assert.AreEqual(1,c.RevealedBlocks);
        Assert.IsTrue(c.StepForward()); Assert.AreEqual(2,c.RevealedBlocks);
        Assert.IsTrue(c.StepForward()); Assert.AreEqual(1,c.SlideIndex); Assert.AreEqual(0,c.RevealedBlocks);
    }

    [Test] public void StepForwardAtTheVeryEndDoesNothing()
    {
        var c = new SlideCursor(new[]{1,1});
        c.StepForward(); c.StepForward(); c.StepForward(); // now last slide, all revealed
        Assert.AreEqual(1,c.SlideIndex); Assert.IsFalse(c.StepForward());
    }

    [Test] public void PrevSlideShowsTheWholeSlide()
    { var c = new SlideCursor(new[]{3,1}); c.NextSlide(); c.PrevSlide(); Assert.AreEqual(0,c.SlideIndex); Assert.AreEqual(3,c.RevealedBlocks); }

    [Test] public void StepBackFromAnEmptySlideLandsOnAPreviousSlideFullyRevealed()
    {
        var c = new SlideCursor(new[]{3,2});
        c.NextSlide();                                    // slide 1, nothing revealed
        Assert.IsTrue(c.StepBack());
        Assert.AreEqual(0,c.SlideIndex); Assert.AreEqual(3,c.RevealedBlocks);
    }

    [Test] public void JumpToClampsAndRevealsEverything()
    { var c = new SlideCursor(new[]{2,4,1}); c.JumpTo(99); Assert.AreEqual(2,c.SlideIndex); Assert.AreEqual(1,c.RevealedBlocks);
      c.JumpTo(-5); Assert.AreEqual(0,c.SlideIndex); Assert.AreEqual(2,c.RevealedBlocks); }

    [Test] public void ZeroBlockSlideIsImmediatelyExhausted()
    { var c = new SlideCursor(new[]{0,1}); Assert.IsTrue(c.StepsExhausted);
      Assert.IsTrue(c.StepForward()); Assert.AreEqual(1,c.SlideIndex); }

    [Test] public void NextSlideOnTheLastSlideChangesNothing()
    { var c = new SlideCursor(new[]{2,3}); c.NextSlide(); Assert.AreEqual(1,c.SlideIndex);
      c.StepForward(); Assert.AreEqual(1,c.RevealedBlocks);
      c.NextSlide();                                   // already last
      Assert.AreEqual(1,c.SlideIndex); Assert.AreEqual(1,c.RevealedBlocks); }

    [Test] public void PrevSlideOnTheFirstSlideChangesNothing()
    { var c = new SlideCursor(new[]{2,3}); c.StepForward(); Assert.AreEqual(1,c.RevealedBlocks);
      c.PrevSlide();                                   // already first
      Assert.AreEqual(0,c.SlideIndex); Assert.AreEqual(1,c.RevealedBlocks); }

    [Test] public void StepBackAtTheStartReturnsFalse()
    { var c = new SlideCursor(new[]{2,3}); Assert.IsFalse(c.StepBack());
      Assert.AreEqual(0,c.SlideIndex); Assert.AreEqual(0,c.RevealedBlocks); }
}
