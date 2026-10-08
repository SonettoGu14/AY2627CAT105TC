using UnityEngine;

/// <summary>
/// W3 review: Physics2D.Raycast + LayerMask, visualised.
///
/// Two rays are drawn every frame with a LineRenderer:
///   * a short DOWN ray  -> the ground-check pattern ("can I jump?")
///   * a FORWARD ray     -> the "is there a wall in front of me?" pattern from the slide
///
/// The forward ray uses a LayerMask, so you can show live that a ray only sees the
/// layers you put in the mask. Green = clear, red = hit.
/// </summary>
public class RaycastDemo2D : MonoBehaviour
{
    [Header("Rays")]
    public float forwardDistance = 3.0f;
    public float downDistance = 0.7f;
    public LayerMask obstacleMask = ~0;      // set to "Ground + Wall" in the scene
    public SpriteRenderer facingSprite;      // used to decide which way "forward" is

    [Header("Visuals")]
    public LineRenderer forwardLine;
    public LineRenderer downLine;
    public Color clearColor = new Color(0.35f, 0.95f, 0.45f);
    public Color hitColor = new Color(0.95f, 0.35f, 0.35f);

    private float forwardHitDistance;
    private bool forwardHit;
    private string lastReport;

    public bool ForwardHit { get { return forwardHit; } }
    public float ForwardHitDistance { get { return forwardHitDistance; } }

    private void Start()
    {
        Material mat = new Material(Shader.Find("Sprites/Default"));
        if (forwardLine != null) { forwardLine.material = mat; forwardLine.positionCount = 2; forwardLine.widthMultiplier = 0.05f; }
        if (downLine != null) { downLine.material = mat; downLine.positionCount = 2; downLine.widthMultiplier = 0.05f; }
    }

    private void Update()
    {
        Vector2 origin = transform.position;

        // ---- forward ray -----------------------------------------------------------------
        float sign = (facingSprite != null && facingSprite.flipX) ? -1f : 1f;
        Vector2 direction = new Vector2(sign, 0f);
        Vector2 forwardOrigin = origin + Vector2.up * 0.5f;      // cast at chest height, not at the feet
        RaycastHit2D forward = Physics2D.Raycast(forwardOrigin, direction, forwardDistance, obstacleMask);
        forwardHit = forward.collider != null;
        forwardHitDistance = forwardHit ? forward.distance : forwardDistance;
        DrawLine(forwardLine, forwardOrigin, forwardOrigin + direction * forwardHitDistance, forwardHit);

        // ---- downward (ground) ray -------------------------------------------------------
        RaycastHit2D down = Physics2D.Raycast(origin, Vector2.down, downDistance, obstacleMask);
        DrawLine(downLine, origin, origin + Vector2.down * (down.collider != null ? down.distance : downDistance), down.collider != null);

        // ---- HUD readout -----------------------------------------------------------------
        string report = "Forward ray    " + (forwardHit
            ? "HIT " + forward.collider.name + " @ " + forward.distance.ToString("0.00") +
              "  (layer: " + LayerMask.LayerToName(forward.collider.gameObject.layer) + ")"
            : "clear (" + forwardDistance.ToString("0.0") + "m)");
        if (report != lastReport)
        {
            lastReport = report;
            if (LabHud.Instance != null) LabHud.Instance.SetRayInfo(report);
        }
    }

    private void DrawLine(LineRenderer line, Vector2 a, Vector2 b, bool hit)
    {
        if (line == null)
        {
            return;
        }
        line.SetPosition(0, new Vector3(a.x, a.y, -0.5f));
        line.SetPosition(1, new Vector3(b.x, b.y, -0.5f));
        Color c = hit ? hitColor : clearColor;
        line.startColor = c;
        line.endColor = c;
    }
}
