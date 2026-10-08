using System.Text;
using UnityEngine;

/// <summary>
/// Drives the W3 playground checklist shown on the HUD. Each <see cref="ConceptZone2D"/>
/// the player walks through ticks one topic off, awards points, and when all topics are
/// done the review is "complete".
/// </summary>
public class PlaygroundReview : MonoBehaviour
{
    public ConceptZone2D[] zones;
    public string[] titles;
    public int pointsPerConcept = 200;
    public int completionBonus = 1000;

    private bool completionAnnounced;

    private void Start()
    {
        Report();
    }

    public void Complete(ConceptZone2D zone)
    {
        if (LabHud.Instance != null)
        {
            LabHud.Instance.AddScore(pointsPerConcept);
            LabHud.Instance.SetStateText("Concept reviewed: " + zone.conceptTitle);
        }
        Report();
    }

    public void Report()
    {
        if (LabHud.Instance == null || zones == null)
        {
            return;
        }

        int done = 0;
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < zones.Length; i++)
        {
            bool isDone = zones[i] != null && zones[i].Done;
            if (isDone)
            {
                done += 1;
            }

            string label = (titles != null && i < titles.Length && !string.IsNullOrEmpty(titles[i]))
                ? titles[i]
                : (zones[i] != null ? zones[i].conceptTitle : "?");

            sb.Append(isDone ? "<color=#7CE38B>[x]</color> " : "[ ] ").Append(label).Append('\n');
        }

        LabHud.Instance.SetConcepts(done, zones.Length, sb.ToString());

        if (done >= zones.Length && !completionAnnounced)
        {
            completionAnnounced = true;
            LabHud.Instance.AddScore(completionBonus);
            LabHud.Instance.SetStateText("All W3 topics reviewed! +" + completionBonus);
        }
    }
}
