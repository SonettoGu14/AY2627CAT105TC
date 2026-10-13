using NUnit.Framework;

/// <summary>
/// The slide paging state machine. It is deliberately engine-free, so it is testable without Unity.
///
/// It pages only: a slide is shown whole. There is no bullet-by-bullet reveal to test, because the
/// decks have never used one.
/// </summary>
public class SlideCursorTests
{
    [Test] public void StartsOnTheFirstSlide()
    { var c = new SlideCursor(3); Assert.AreEqual(0, c.SlideIndex); Assert.AreEqual(3, c.SlideCount); }

    [Test] public void NextSlideAdvancesOneAtATime()
    { var c = new SlideCursor(3); c.NextSlide(); Assert.AreEqual(1, c.SlideIndex);
      c.NextSlide(); Assert.AreEqual(2, c.SlideIndex); }

    [Test] public void NextSlideStopsAtTheLastSlide()
    { var c = new SlideCursor(3); c.NextSlide(); c.NextSlide(); c.NextSlide(); c.NextSlide();
      Assert.AreEqual(2, c.SlideIndex); }

    [Test] public void PrevSlideGoesBackOneAtATime()
    { var c = new SlideCursor(3); c.NextSlide(); c.NextSlide(); c.PrevSlide(); Assert.AreEqual(1, c.SlideIndex); }

    [Test] public void PrevSlideStopsAtTheFirstSlide()
    { var c = new SlideCursor(3); c.NextSlide(); c.PrevSlide(); c.PrevSlide(); c.PrevSlide();
      Assert.AreEqual(0, c.SlideIndex); }

    [Test] public void JumpToGoesStraightToASlide()
    { var c = new SlideCursor(5); c.JumpTo(3); Assert.AreEqual(3, c.SlideIndex); }

    [Test] public void JumpToClampsIntoRange()
    { var c = new SlideCursor(3); c.JumpTo(99); Assert.AreEqual(2, c.SlideIndex);
      c.JumpTo(-5); Assert.AreEqual(0, c.SlideIndex); }

    [Test] public void ASingleSlideDeckNeverMoves()
    { var c = new SlideCursor(1); c.NextSlide(); Assert.AreEqual(0, c.SlideIndex);
      c.PrevSlide(); Assert.AreEqual(0, c.SlideIndex); }

    [Test] public void AnEmptyDeckStaysOnIndexZero()
    { var c = new SlideCursor(0); Assert.AreEqual(0, c.SlideCount);
      c.NextSlide(); Assert.AreEqual(0, c.SlideIndex); c.PrevSlide(); Assert.AreEqual(0, c.SlideIndex); }

    [Test] public void ANegativeCountIsTreatedAsEmpty()
    { var c = new SlideCursor(-4); Assert.AreEqual(0, c.SlideCount);
      c.NextSlide(); Assert.AreEqual(0, c.SlideIndex); }
}
