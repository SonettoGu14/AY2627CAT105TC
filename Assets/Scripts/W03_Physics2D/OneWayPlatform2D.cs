using System.Collections;
using UnityEngine;

/// <summary>
/// W3 slide "PlatformEffector2D component": a platform you can jump onto from below.
/// With a normal collider, jumping up from underneath is blocked. The effector makes
/// the collider one-way. Remember to tick "Used By Effector" on the collider - the
/// script does it for you here, but it is a classic "why doesn't it work?" mistake.
///
/// Hold S for a moment to drop through the platform (a very common platformer feature).
/// </summary>
[RequireComponent(typeof(PlatformEffector2D))]
[RequireComponent(typeof(Collider2D))]
public class OneWayPlatform2D : MonoBehaviour
{
    public bool allowDropThrough = true;
    public float dropThroughTime = 0.35f;

    private Collider2D platformCollider;
    private PlatformEffector2D effector;
    private bool dropping;

    private void Awake()
    {
        platformCollider = GetComponent<Collider2D>();
        effector = GetComponent<PlatformEffector2D>();

        platformCollider.usedByEffector = true;   // <- the checkbox the slide mentions
        effector.useOneWay = true;
        effector.useOneWayGrouping = false;
        effector.surfaceArc = 170f;               // slightly less than 180 so the player can be pushed off the edge
    }

    private void Update()
    {
        if (!allowDropThrough || dropping)
        {
            return;
        }
        if (Input.GetKey(KeyCode.S) && Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(DropThrough());
        }
    }

    private IEnumerator DropThrough()
    {
        dropping = true;
        platformCollider.enabled = false;
        yield return new WaitForSeconds(dropThroughTime);
        platformCollider.enabled = true;
        dropping = false;
    }
}
