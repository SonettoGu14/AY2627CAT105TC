using System.Collections;
using UnityEngine;

/// <summary>
/// Tiny one-shot "pop" effect, used to demonstrate the W4 slide
/// "Instantiate and Destroy": we spawn it, animate it, then destroy it.
/// </summary>
public class Burst2D : MonoBehaviour
{
    public float lifetime = 0.35f;
    public float startScale = 0.2f;
    public float endScale = 0.9f;

    private SpriteRenderer spriteRenderer;
    private float elapsed;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        transform.localScale = Vector3.one * startScale;
        StartCoroutine(Play());
    }

    private IEnumerator Play()
    {
        Color startColor = spriteRenderer != null ? spriteRenderer.color : Color.white;
        while (elapsed < lifetime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / lifetime);

            transform.localScale = Vector3.one * Mathf.Lerp(startScale, endScale, t);
            if (spriteRenderer != null)
            {
                Color c = startColor;
                c.a = 1f - t;
                spriteRenderer.color = c;
            }
            yield return null;
        }
        Destroy(gameObject);
    }
}
