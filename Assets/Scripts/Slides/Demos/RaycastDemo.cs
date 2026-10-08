using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Slides 3-4, one parameter per demo:
///   Basic      - what a raycast is: emit a ray, get the first thing it touches
///   NoCollider - the emitter is a plain point (no collider); only the TARGET needs one
///   Origin     - the `origin` parameter
///   Direction  - the `direction` parameter
///   Distance   - the `distance` parameter
///   LayerMask  - the `layerMask` parameter
/// </summary>
public class RaycastDemo : DemoBase
{
    public enum Mode { Basic, NoCollider, Origin, Direction, Distance, LayerMask }

    public Mode mode = Mode.Basic;

    public Transform emitter;
    public Collider2D target;
    public LineRenderer line;
    public SpriteRenderer targetVisual;
    public SpriteRenderer emitterVisual;

    public float distance = 4f;
    public Vector2 direction = Vector2.right;
    public LayerMask mask = ~0;

    /// <summary>Bit index of the layer the target sits on, used by the LayerMask demo.</summary>
    public int targetLayerBit;

    private RaycastHit2D hit;

    public override string Title
    {
        get
        {
            switch (mode)
            {
                case Mode.Basic: return "Physics2D.Raycast stops at the first object it touches";
                case Mode.NoCollider: return "the ray's emitter needs no collider - only the target does";
                case Mode.Origin: return "Raycast parameter: origin";
                case Mode.Direction: return "Raycast parameter: direction";
                case Mode.Distance: return "Raycast parameter: distance";
                default: return "Raycast parameter: layerMask";
            }
        }
    }

    public override string Keys
    {
        get
        {
            switch (mode)
            {
                case Mode.Basic: return "A / D  move the wall";
                case Mode.NoCollider: return "T  remove / restore the target's collider";
                case Mode.Origin: return "W / S  move the ray's origin";
                case Mode.Direction: return "A / D  rotate the direction";
                case Mode.Distance: return "W / S  lengthen / shorten the ray";
                default: return "M  add / remove the target's layer from the mask";
            }
        }
    }

    public override void OnActivate()
    {
        if (emitterVisual != null) emitterVisual.color = new Color32(0x6F, 0xA8, 0xFF, 0xFF);
    }

    private void FixedUpdate()
    {
        if (emitter == null) return;

        switch (mode)
        {
            case Mode.Basic:
            case Mode.NoCollider:
                if (targetVisual != null)
                {
                    Vector3 p = targetVisual.transform.position;
                    if (Input.GetKey(KeyCode.A)) p.x -= 3f * Time.fixedDeltaTime;
                    if (Input.GetKey(KeyCode.D)) p.x += 3f * Time.fixedDeltaTime;
                    p.x = Mathf.Clamp(p.x, -2f, 6f);
                    targetVisual.transform.position = p;
                }
                break;

            case Mode.Origin:
            {
                Vector3 p = emitter.position;
                if (Input.GetKey(KeyCode.W)) p.y += 2f * Time.fixedDeltaTime;
                if (Input.GetKey(KeyCode.S)) p.y -= 2f * Time.fixedDeltaTime;
                p.y = Mathf.Clamp(p.y, -2.5f, 2.5f);
                emitter.position = p;
                break;
            }

            case Mode.Direction:
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                if (Input.GetKey(KeyCode.A)) angle += 60f * Time.fixedDeltaTime;
                if (Input.GetKey(KeyCode.D)) angle -= 60f * Time.fixedDeltaTime;
                angle = Mathf.Clamp(angle, -60f, 60f);
                direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
                break;
            }

            case Mode.Distance:
                if (Input.GetKey(KeyCode.W)) distance += 3f * Time.fixedDeltaTime;
                if (Input.GetKey(KeyCode.S)) distance -= 3f * Time.fixedDeltaTime;
                distance = Mathf.Clamp(distance, 0.5f, 9f);
                break;

            case Mode.LayerMask:
                if (Input.GetKeyDown(KeyCode.M)) mask = mask | (1 << targetLayerBit);
                break;
        }

        if (mode == Mode.LayerMask && Input.GetKeyDown(KeyCode.M))
        {
            // toggle: if the bit is in, take it out; otherwise put it in
            if ((mask.value & (1 << targetLayerBit)) != 0) mask = mask & ~(1 << targetLayerBit);
            else mask = mask | (1 << targetLayerBit);
        }

        if (mode == Mode.NoCollider && Input.GetKeyDown(KeyCode.T) && target != null)
            target.enabled = !target.enabled;

        Vector2 origin = emitter.position;
        hit = Physics2D.Raycast(origin, direction.normalized, distance, mask);

        float len = hit.collider != null ? hit.distance : distance;
        if (line != null)
        {
            line.positionCount = 2;
            line.SetPosition(0, new Vector3(origin.x, origin.y, -1f));
            line.SetPosition(1, new Vector3(origin.x + direction.normalized.x * len, origin.y + direction.normalized.y * len, -1f));
            Color c = hit.collider != null ? new Color32(0xFF, 0x6B, 0x6B, 0xFF) : new Color32(0x7C, 0xE3, 0x8B, 0xFF);
            line.startColor = c;
            line.endColor = c;
        }
    }

    public override void Readout(List<string> lines)
    {
        lines.Add("Physics2D.Raycast(origin, direction, distance, layerMask)");
        lines.Add("");
        lines.Add("origin    = " + V((Vector2)(emitter != null ? emitter.position : Vector3.zero)));
        lines.Add("direction = " + V(direction.normalized));
        lines.Add("distance  = " + F(distance, 1));
        lines.Add("layerMask = 0x" + mask.value.ToString("X8") + "   (layers: " + MaskNames() + ")");

        switch (mode)
        {
            case Mode.Basic:
                lines.Add("");
                lines.Add("hit       = " + (hit.collider != null ? hit.collider.name + " @ " + F(hit.distance, 2) : "nothing"));
                break;
            case Mode.NoCollider:
                lines.Add("");
                lines.Add("emitter collider = none (not needed)");
                lines.Add("target collider  = " + (target != null ? B(target.enabled) : "-"));
                lines.Add("hit              = " + (hit.collider != null ? "yes @ " + F(hit.distance, 2) : "no"));
                break;
            case Mode.Origin:
                lines.Add("");
                lines.Add("origin.y  = " + F(emitter != null ? emitter.position.y : 0f, 2) + "   (W / S)");
                lines.Add("hit       = " + (hit.collider != null ? hit.collider.name : "nothing"));
                break;
            case Mode.Direction:
                lines.Add("");
                lines.Add("angle     = " + F(Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg, 1) + " deg");
                lines.Add("hit       = " + (hit.collider != null ? hit.collider.name : "nothing"));
                break;
            case Mode.Distance:
                lines.Add("");
                lines.Add("distance  = " + F(distance, 2));
                lines.Add("hit       = " + (hit.collider != null ? hit.collider.name + " @" + F(hit.distance, 2)
                                                          : "nothing (the ray is too short)"));
                break;
            default:
                lines.Add("");
                lines.Add("target layer      = " + LayerMask.LayerToName(target != null ? target.gameObject.layer : 0));
                lines.Add("layer in the mask = " + B((mask.value & (1 << targetLayerBit)) != 0));
                lines.Add("hit               = " + (hit.collider != null ? "yes - the ray sees it" : "no - the mask skips it"));
                break;
        }
    }

    private string MaskNames()
    {
        int v = mask.value;
        var names = new List<string>();
        for (int i = 0; i < 32; i++)
        {
            if ((v & (1 << i)) == 0) continue;
            string n = LayerMask.LayerToName(i);
            names.Add(string.IsNullOrEmpty(n) ? i.ToString() : n);
        }
        return names.Count > 5 ? names.Count + " layers" : string.Join(",", names.ToArray());
    }
}
