using UnityEngine;

/// <summary>
/// W4 slide "Animator: Scripting" - the <c>animator.Play()</c> overload, used for
/// animations that don't need a smooth blend (exactly what the slide says: 2D frame
/// animation, one-shot effects).
///
/// The object it sits on has an Animator with a single looping state that animates
/// scale and colour - the "create an animation that changes the position, rotation,
/// scale and color of an object" exercise from the slide. Press P to replay it.
/// </summary>
public class PropertyAnimationDemo2D : MonoBehaviour
{
    public Animator animator;
    public string stateName = "PropertyDemo";

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P) && animator != null)
        {
            animator.Play(stateName, 0, 0f);   // (stateName, layer, normalizedTime)
            if (LabHud.Instance != null)
            {
                LabHud.Instance.SetStateText("animator.Play(\"" + stateName + "\", 0, 0)");
            }
        }
    }
}
