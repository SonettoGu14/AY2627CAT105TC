using System; using UnityEngine;
public class SlideDeckPlayer : MonoBehaviour
{
    public TextAsset deckJson;
    public SlideDeckData Deck { get; private set; }
    public SlideCursor Cursor { get; private set; }
    public int SlideIndex => Cursor?.SlideIndex ?? 0;
    public int RevealedBlocks => Cursor?.RevealedBlocks ?? 0;
    public event Action Changed;

    void Awake() { Reload(); }
    public void Reload()
    {
        if (deckJson == null) { Debug.LogError("[Slides] no deck JSON assigned"); return; }
        try { Deck = JsonUtility.FromJson<SlideDeckData>(deckJson.text); }
        catch (Exception) { Debug.LogError("[Slides] deck JSON did not parse: " + deckJson.name); return; }
        if (Deck == null || Deck.slides == null) { Debug.LogError("[Slides] deck JSON did not parse"); return; }
        var counts = new int[Deck.slides.Length];
        for (int i = 0; i < counts.Length; i++) counts[i] = Deck.slides[i].blocks?.Length ?? 0;
        Cursor = new SlideCursor(counts);
        Changed?.Invoke();
    }
    public void NextSlide()   { if (Cursor == null) return; Cursor.NextSlide();   Changed?.Invoke(); }
    public void PrevSlide()   { if (Cursor == null) return; Cursor.PrevSlide();   Changed?.Invoke(); }
    public void JumpTo(int i) { if (Cursor == null) return; Cursor.JumpTo(i);     Changed?.Invoke(); }
    public void StepForward(){ if (Cursor == null) return; if (Cursor.StepForward()) Changed?.Invoke(); }
    public void StepBack()   { if (Cursor == null) return; if (Cursor.StepBack())    Changed?.Invoke(); }
}
