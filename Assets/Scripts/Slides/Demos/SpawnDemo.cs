using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// W5 slides 4-5 and 7-8 — creating and removing objects:
///   Instances   - a prefab is one saved template; everything you drop in the scene is a copy of it
///   Instantiate - Instantiate(original, position, rotation, parent) makes a GameObject from code
///   Destroy     - Destroy(go) removes it now, Destroy(go, t) removes it t seconds from now
///
/// The prefab demo is honest about its limits: "edit the prefab and every instance changes" is an
/// Editor behaviour, so at runtime it can only show one template producing many identical copies.
/// </summary>
public class SpawnDemo : DemoBase
{
    public enum Mode { Instances, Instantiate, Destroy }

    public Mode mode = Mode.Instantiate;

    public GameObject template;              // an inactive child: the "prefab"
    public Transform sourceMark;             // Instances: the labelled original
    public SpriteRenderer sourceSprite;
    public float spawnX = -3f;
    public float rowY = 1.6f;
    public float stepX = 1.2f;

    private readonly List<GameObject> live = new List<GameObject>();
    private readonly List<float> deathAt = new List<float>();   // -1 = not scheduled
    private int made;
    private const int MaxLive = 6;

    public override string Title
    {
        get
        {
            switch (mode)
            {
                case Mode.Instances:   return "A prefab: one saved template, many instances";
                case Mode.Instantiate: return "Instantiate makes a GameObject from code";
                default:               return "Destroy: now, or after a delay";
            }
        }
    }

    public override string Keys
    {
        get
        {
            switch (mode)
            {
                case Mode.Instances:   return "A  add an instance        S  clear";
                case Mode.Instantiate: return "Space  Instantiate one        S  clear";
                default:               return "Space  spawn        A  Destroy(go)        D  Destroy(go, 1f)        S  clear";
            }
        }
    }

    public override void OnActivate() { Clear(); if (mode == Mode.Instances) SpawnOne(); }
    public override void OnDeactivate() { Clear(); }

    void Update()
    {
        if (mode == Mode.Instances && Input.GetKeyDown(KeyCode.A)) SpawnOne();
        if (mode != Mode.Instances && Input.GetKeyDown(KeyCode.Space)) SpawnOne();
        if (mode == Mode.Destroy && Input.GetKeyDown(KeyCode.A)) DestroyOne(0f);
        if (mode == Mode.Destroy && Input.GetKeyDown(KeyCode.D)) DestroyOne(1f);
        if (Input.GetKeyDown(KeyCode.S)) Clear();
        Expire();
        if (sourceSprite != null && mode == Mode.Instances)
            sourceSprite.color = new Color(0.55f, 0.6f, 0.7f, 1f);   // the template reads as a template, not a copy
    }

    private void SpawnOne()
    {
        if (template == null || live.Count >= MaxLive) return;
        int i = live.Count;
        GameObject go = Instantiate(template, transform);
        go.name = (mode == Mode.Instances ? "Instance_" : "Spawned_") + (++made);
        go.transform.localPosition = new Vector3(spawnX + i * stepX, rowY, 0f);
        go.SetActive(true);
        live.Add(go);
        deathAt.Add(-1f);
    }

    private void DestroyOne(float delay)
    {
        if (live.Count == 0) return;
        int idx = 0;
        if (delay <= 0f)
        {
            Object.Destroy(live[idx]);
            live.RemoveAt(idx);
            deathAt.RemoveAt(idx);
        }
        else
        {
            deathAt[idx] = Time.time + delay;
        }
    }

    private void Expire()
    {
        for (int i = live.Count - 1; i >= 0; i--)
            if (deathAt[i] > 0f && Time.time >= deathAt[i])
            {
                Object.Destroy(live[i]);
                live.RemoveAt(i);
                deathAt.RemoveAt(i);
            }
    }

    private void Clear()
    {
        for (int i = 0; i < live.Count; i++) if (live[i] != null) Object.Destroy(live[i]);
        live.Clear();
        deathAt.Clear();
        made = 0;
    }

    public override void Readout(List<string> lines)
    {
        switch (mode)
        {
            case Mode.Instances:
                lines.Add("A prefab lives in the Project, not in the scene.");
                lines.Add("Every object you drop in is an INSTANCE of it.");
                lines.Add("");
                lines.Add("instances alive = " + live.Count);
                lines.Add("edit-the-prefab-and-all-change is Editor-only;");
                lines.Add("at runtime you can only show one template -> many copies.");
                break;

            case Mode.Instantiate:
                lines.Add("Instantiate(original, position, rotation, parent)");
                lines.Add("");
                lines.Add("original = " + (template != null ? template.name : "(none)"));
                lines.Add("position = new Vector3(x, " + F(rowY, 1) + "f, 0f)");
                lines.Add("rotation = Quaternion.identity");
                lines.Add("parent   = this demo");
                lines.Add("");
                lines.Add("spawned = " + live.Count + " / " + MaxLive);
                break;

            default:
                lines.Add("Destroy(Object gameObject, float t = 0f)");
                lines.Add("");
                if (live.Count == 0) lines.Add("(nothing alive - press Space)");
                for (int i = 0; i < live.Count; i++)
                {
                    string tag = deathAt[i] > 0f
                        ? "Destroy(go, 1f) in " + F(Mathf.Max(0f, deathAt[i] - Time.time), 1) + "s"
                        : "alive";
                    lines.Add("  " + live[i].name + "   " + tag);
                }
                break;
        }
    }
}
