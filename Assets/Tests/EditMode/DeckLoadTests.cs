using NUnit.Framework; using UnityEditor; using UnityEngine;
public class DeckLoadTests
{
    static SlideDeckData Load()
    {
        var ta = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Slides/W04_L4.json");
        Assert.IsNotNull(ta, "Assets/Slides/W04_L4.json is missing");
        var deck = JsonUtility.FromJson<SlideDeckData>(ta.text);
        Assert.IsNotNull(deck, "the deck JSON did not deserialise");
        return deck;
    }

    [Test] public void ParsesTheRealW04Deck()
    {
        var d = Load();
        Assert.AreEqual("W04_L4", d.key);
        Assert.AreEqual("Animations and Camera", d.title);
        Assert.AreEqual("Lecture 4", d.subtitle);
        Assert.AreEqual(28, d.slides.Length);
    }

    [Test] public void SlideIndexesAreOneBasedAndContiguous()
    {
        var d = Load();
        for (int i = 0; i < d.slides.Length; i++) Assert.AreEqual(i + 1, d.slides[i].index);
    }

    [Test] public void TitleSlidesCarryTheSubtitleAndNoBlocks()
    {
        var d = Load();
        foreach (var s in new[] { d.slides[0], d.slides[27] })
        {
            Assert.AreEqual("title", s.layout);
            Assert.AreEqual("Lecture 4", s.subtitle);
            Assert.AreEqual(0, s.blocks.Length);
        }
    }

    [Test] public void CodeBlocksSurviveDeserialisation()
    {
        var d = Load();
        int code = 0;
        foreach (var s in d.slides) foreach (var b in s.blocks) if (b.kind == "code") code++;
        Assert.AreEqual(4, code, "expected the 4 animator.* code blocks");
    }
}
