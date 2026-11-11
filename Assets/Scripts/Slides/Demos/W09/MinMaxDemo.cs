using System.Collections.Generic;
using UnityEngine;


// ==============================================================================================
//  MIN / MAX DISTANCE: within Min the sound is at full volume, then it fades, and past Max a
//  linear rolloff leaves nothing. Move the ears to walk through the two radii.
// ==============================================================================================

public class MinMaxDemo : AudioDemoBase
{
    public Transform listenerTransform;
    public LineRenderer distanceLine;
    public float moveSpeed = 4f;

    public override string Title { get { return "Min and max distance: where a sound lives"; } }
    public override string Keys { get { return "Left / Right move the ears    SPACE fire    R centre"; } }

    public override void OnActivate()
    {
        if (listenerTransform != null) listenerTransform.localPosition = new Vector3(-7f, 0.4f, 0f);
        if (source != null) { source.Stop(); source.Play(); }     // one 5 s fire to start with
        UpdateLine();
    }

    public override void OnDeactivate()
    {
        if (source != null) source.Stop();
    }

    private void Update()
    {
        if (listenerTransform == null || source == null) return;

        Vector3 p = listenerTransform.localPosition;
        float dt = Time.deltaTime;
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) p.x -= moveSpeed * dt;
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) p.x += moveSpeed * dt;
        if (Input.GetKeyDown(KeyCode.R)) p.x = 0f;
        // The fire is a one-shot, not a loop: press again to hear it from wherever the ears are now.
        if (Input.GetKeyDown(KeyCode.Space) && source.clip != null) source.PlayOneShot(source.clip, 0.9f);
        p.x = Mathf.Clamp(p.x, -7f, 7f);
        p.y = 0.4f;
        listenerTransform.localPosition = p;
        UpdateLine();
    }

    private void UpdateLine()
    {
        if (distanceLine == null || source == null || listenerTransform == null) return;
        distanceLine.SetPosition(0, source.transform.position);
        distanceLine.SetPosition(1, listenerTransform.position);
    }

    public override void Readout(List<string> lines)
    {
        if (source == null || listenerTransform == null) return;

        float distance = Vector3.Distance(source.transform.position, listenerTransform.position);
        string state = distance <= source.minDistance ? "FULL VOLUME"
            : distance < source.maxDistance ? "FADING" : "SILENT";

        float left = 0f, right = 0f;
        if (listener != null) Ears(out left, out right);
        float level = Mathf.Max(left, right);

        lines.Add("distance  " + F(distance, 2) + " m      state  " + state);
        lines.Add("level     " + Level(level, 10) + "  " + Db(level));
        lines.Add("min " + F(source.minDistance, 1) + "   max " + F(source.maxDistance, 1)
                  + "   (" + source.rolloffMode + " rolloff)");
        lines.Add("inside min the volume stops growing");
        lines.Add("past max a linear curve has nothing left to give");
    }
}
