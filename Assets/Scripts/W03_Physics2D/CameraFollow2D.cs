using UnityEngine;

/// <summary>
/// W4 Lecture 4 - "The camera follows the character".
///
/// Slide idea:
///   1. record the offset between the camera and the character once, at the start
///   2. every frame, put the camera at character.position + offset
///
/// We also show the "better way" the slide hints at: smoothing the movement with
/// Vector3.SmoothDamp so the camera lags a little behind the player.
/// Run this from LateUpdate so the camera reads the player's final position for the frame.
/// </summary>
public class CameraFollow2D : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Offset (recorded at start if 'captureOffsetAtStart' is on)")]
    public bool captureOffsetAtStart = true;
    public Vector2 offset = new Vector2(0f, 1.5f);

    [Header("Smoothing")]
    public bool smooth = true;
    [Range(0.01f, 1f)] public float smoothTime = 0.15f;

    private Vector3 dampVelocity;

    private void Start()
    {
        if (captureOffsetAtStart && target != null)
        {
            // "Record the Z-distance between the camera and the character at the beginning."
            offset = transform.position - target.position;
        }
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        // Keep the camera's own Z (orthographic cameras don't care, but the Scene View likes it).
        Vector3 desired = new Vector3(
            target.position.x + offset.x,
            target.position.y + offset.y,
            transform.position.z);

        if (smooth)
        {
            transform.position = Vector3.SmoothDamp(transform.position, desired, ref dampVelocity, smoothTime);
        }
        else
        {
            transform.position = desired;
        }
    }
}
