using UnityEngine;

/// <summary>
/// Owns every slide demo: their world anchors, their cameras, and which one is live.
///
/// Each demo is built on its own stage (see SlideDeckBuilder) and only one is enabled at a time.
/// When the deck is hidden, SlidePresenter asks the stage to show the demos belonging to the
/// current slide; `,` / `.` (or a digit) move between a slide's demos without going back to the slides.
/// </summary>
public class DemoStage : MonoBehaviour
{
    [System.Serializable]
    public class Slot
    {
        public string key;          // e.g. "sr_color"
        public GameObject root;     // the demo's hierarchy, inactive by default
        public Vector2 anchor;      // where the stage camera looks for this demo
        public float orthoSize = 5f;
        public Camera camera;       // optional: a demo that needs its own camera (the platformer)
    }

    public Slot[] slots;
    public Camera stageCamera;

    public DemoBase Active { get; private set; }
    public string ActiveKey { get; private set; }
    public bool IsShowing { get; private set; }

    private string[] currentKeys = new string[0];
    private int currentIndex;

    public int Count { get { return currentKeys.Length; } }
    public int Index { get { return currentKeys.Length == 0 ? 0 : currentIndex; } }

    /// <summary>Show the first of this slide's demos. <paramref name="csv"/> is a comma list of demo keys.</summary>
    public void Show(string csv)
    {
        currentKeys = string.IsNullOrEmpty(csv)
            ? new string[0]
            : csv.Split(new[] { ',', ';' }, System.StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < currentKeys.Length; i++) currentKeys[i] = currentKeys[i].Trim();
        currentIndex = 0;
        Apply();
    }

    public void Hide()
    {
        DeactivateAll();
        IsShowing = false;
        Active = null;
        ActiveKey = null;
        currentKeys = new string[0];
        currentIndex = 0;
        // Keep a camera alive so the scene always renders something (the deck covers it anyway).
        if (stageCamera != null) stageCamera.gameObject.SetActive(true);
    }

    public void Next() { if (currentKeys.Length > 1) { currentIndex = (currentIndex + 1) % currentKeys.Length; Apply(); } }
    public void Previous() { if (currentKeys.Length > 1) { currentIndex = (currentIndex - 1 + currentKeys.Length) % currentKeys.Length; Apply(); } }
    public void Jump(int zeroBased) { if (zeroBased >= 0 && zeroBased < currentKeys.Length) { currentIndex = zeroBased; Apply(); } }

    private void Apply()
    {
        if (currentKeys.Length == 0) { Hide(); return; }
        IsShowing = true;

        Slot slot = Find(currentKeys[currentIndex]);
        if (slot == null)
        {
            Debug.LogWarning("[DemoStage] no demo built for key '" + currentKeys[currentIndex] + "'");
            return;
        }

        DeactivateAll();
        ActiveKey = slot.key;
        if (slot.root != null) slot.root.SetActive(true);

        // exactly one camera for the whole rig
        if (stageCamera != null) stageCamera.gameObject.SetActive(slot.camera == null);
        if (slot.camera != null) slot.camera.gameObject.SetActive(true);
        if (stageCamera != null && slot.camera == null)
        {
            stageCamera.transform.position = new Vector3(slot.anchor.x, slot.anchor.y, -10f);
            stageCamera.orthographicSize = slot.orthoSize;
        }

        Active = slot.root != null ? slot.root.GetComponentInChildren<DemoBase>(true) : null;
        if (Active != null) Active.OnActivate();
    }

    private void DeactivateAll()
    {
        if (Active != null) { Active.OnDeactivate(); Active = null; }
        if (slots != null)
        {
            foreach (Slot s in slots)
            {
                if (s == null) continue;
                if (s.root != null) s.root.SetActive(false);
                if (s.camera != null) s.camera.gameObject.SetActive(false);
            }
        }
    }

    private Slot Find(string key)
    {
        if (slots == null) return null;
        foreach (Slot s in slots) if (s != null && s.key == key) return s;
        return null;
    }
}
