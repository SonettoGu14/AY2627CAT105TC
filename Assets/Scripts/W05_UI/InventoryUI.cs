using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// W5 slides "Instantiate" / "Destroy", inside the interface: a button opens a bag, and the slots
/// are Instantiate'd when it opens and Destroy'd when it closes.
///
/// Deliberately re-creates the slots every time rather than hiding them, because spawning and
/// removing them IS the lesson. The grid sits under a RectMask2D, so anything past the frame is
/// clipped, which is slide "Mask" doing its job.
/// </summary>
public class InventoryUI : MonoBehaviour
{
    public Button toggleButton;
    public CanvasGroup panel;
    public RectTransform grid;
    public GameObject slotTemplate;     // an inactive child: the "prefab"
    public TMP_Text coinsLabel;
    public int slotCount = 8;

    private readonly System.Collections.Generic.List<GameObject> live = new System.Collections.Generic.List<GameObject>();
    private bool open;

    void Awake()
    {
        if (toggleButton != null) toggleButton.onClick.AddListener(Toggle);
        if (panel != null) { panel.alpha = 0f; panel.interactable = false; panel.blocksRaycasts = false; }
    }

    void Update()
    {
        if (coinsLabel != null && LabHud.Instance != null)
            coinsLabel.text = "Coins  " + LabHud.Instance.Coins;
    }

    public void Toggle() { SetOpen(!open); }

    public void SetOpen(bool value)
    {
        open = value;
        if (panel != null)
        {
            panel.alpha = value ? 1f : 0f;
            panel.interactable = value;
            panel.blocksRaycasts = value;
        }
        if (value) SpawnSlots(); else ClearSlots();
    }

    /// <summary>W5: Instantiate - one template, slotCount copies, parented to the grid.</summary>
    private void SpawnSlots()
    {
        ClearSlots();
        if (slotTemplate == null || grid == null) return;
        for (int i = 0; i < slotCount; i++)
        {
            GameObject slot = Instantiate(slotTemplate, grid);
            slot.name = "Slot_" + i;
            slot.SetActive(true);
            live.Add(slot);
        }
    }

    /// <summary>W5: Destroy - and the right way to remove UI that was built at runtime.</summary>
    private void ClearSlots()
    {
        for (int i = 0; i < live.Count; i++) if (live[i] != null) Destroy(live[i]);
        live.Clear();
    }

    public int LiveSlots { get { return live.Count; } }
    public bool IsOpen { get { return open; } }
}
