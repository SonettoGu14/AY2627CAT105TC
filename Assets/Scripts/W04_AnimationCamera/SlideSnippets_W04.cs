using UnityEngine;

/// <summary>
/// The exact code from the W4 lecture ("Animations and 2D Art"), gathered in one place so
/// it can be opened live during the class. Nothing here is attached to a GameObject.
///
/// Method names say which slide they come from.
/// </summary>
public static class SlideSnippets_W04
{
    /// <summary>Slide "Handling Sprites in Code": swap the image on a SpriteRenderer.</summary>
    public static void ChangeSprite(SpriteRenderer renderer, Sprite next)
    {
        renderer.sprite = next;
    }

    /// <summary>Slide "Animator: Scripting" - bool and trigger.</summary>
    public static void AnimatorFromCode(Animator animator, bool isRunning)
    {
        animator.SetBool("Running", isRunning);   // bool    -> like a light switch
        animator.SetTrigger("Jump");              // trigger -> true for a single instant
    }

    /// <summary>Slide "Animator: Scripting" - Play is for when you don't need a smooth blend.</summary>
    public static void PlayState(Animator animator, string stateName)
    {
        animator.Play(stateName, 0, 0f);          // (state, layer, normalizedTime)
    }

    /// <summary>Slide "The camera follows the character".</summary>
    public static void CameraFollow(Transform cameraTransform, Transform playerTransform, Vector3 offset)
    {
        // 1. record the offset once:   offset = camera.position - player.position;
        // 2. every frame:              camera.position = player.position + offset;
        cameraTransform.position = playerTransform.position + offset;
    }

    /// <summary>Slide "Field Modifier" - the three modifiers that change the Inspector.</summary>
    public class FieldModifierExample
    {
        [SerializeField] private float shownThoughPrivate;   // shown in the Inspector
        [HideInInspector] public bool hiddenThoughPublic;    // hidden in the Inspector
        [Range(0f, 10f)] public float shownAsSlider;         // shown as a slider
    }
}
