using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Slides 11-14, one animation idea per demo:
///   Clip         - an AnimationClip plays on an Animator (play / pause / scrub)
///   Animator     - the Animator component is what plays the clip
///   Keyframes    - an animation is just values recorded at keyframes
///   Channels     - one clip can animate position, colour AND your own variables
///   OneAtATime   - only one animation plays on an object at a time
///   Priority     - the animation overrides what the code writes (the classic "bug")
/// </summary>
public class AnimationDemo : DemoBase
{
    public enum Mode { Clip, AnimatorComponent, Keyframes, Channels, OneAtATime, Priority }

    public Mode mode = Mode.Clip;

    public Animator animator;
    public Transform target;               // the object that is animated
    public SpriteRenderer sprite;
    public string stateA = "A";
    public string stateB = "B";
    public float clipLength = 1f;
    public float[] keyTimes = { 0f, 0.5f, 1f };
    public string[] keyLabels = { "start", "peak", "back" };
    public float restartAt;

    /// <summary>Animated by the "channels" clip - a clip can drive your own fields too.</summary>
    public float animValue;

    private Vector3 codeWants;
    private int highlight;

    public override string Title
    {
        get
        {
            switch (mode)
            {
                case Mode.Clip: return "an AnimationClip plays on an Animator";
                case Mode.AnimatorComponent: return "the Animator component is what plays the clip";
                case Mode.Keyframes: return "an animation is values recorded at keyframes";
                case Mode.Channels: return "one clip can animate position, colour and your own variables";
                case Mode.OneAtATime: return "only one animation plays on an object at a time";
                default: return "the animation overrides the code (the classic 'bug')";
            }
        }
    }

    public override string Keys
    {
        get
        {
            switch (mode)
            {
                case Mode.Clip: return "Space  pause / play      A / D  scrub the clip";
                case Mode.AnimatorComponent: return "-";
                case Mode.Keyframes: return "Space  restart the clip";
                case Mode.Channels: return "1 / 2 / 3  highlight a channel";
                case Mode.OneAtATime: return "A / D  play clip A or clip B";
                default: return "A / D  write localPosition from code";
            }
        }
    }

    public override void OnActivate()
    {
        if (animator != null) animator.speed = 1f;
        if (mode == Mode.Priority && target != null) codeWants = target.localPosition;
    }

    private void Update()
    {
        if (target == null) return;

        switch (mode)
        {
            case Mode.Clip:
                if (animator != null)
                {
                    if (Input.GetKeyDown(KeyCode.Space)) animator.speed = animator.speed > 0f ? 0f : 1f;
                    if (Input.GetKey(KeyCode.A)) Scrub(-0.4f * Time.deltaTime);
                    if (Input.GetKey(KeyCode.D)) Scrub(0.4f * Time.deltaTime);
                }
                break;

            case Mode.Keyframes:
                if (Input.GetKeyDown(KeyCode.Space) && animator != null) animator.Play(stateA, 0, 0f);
                break;

            case Mode.Channels:
                if (Input.GetKeyDown(KeyCode.Alpha1)) highlight = 0;
                if (Input.GetKeyDown(KeyCode.Alpha2)) highlight = 1;
                if (Input.GetKeyDown(KeyCode.Alpha3)) highlight = 2;
                break;

            case Mode.OneAtATime:
                if (Input.GetKeyDown(KeyCode.A) && animator != null) animator.Play(stateA, 0, 0f);
                if (Input.GetKeyDown(KeyCode.D) && animator != null) animator.Play(stateB, 0, 0f);
                break;

            case Mode.Priority:
            {
                float h = Input.GetAxisRaw("Horizontal");
                Vector3 p = target.localPosition;
                p.x += h * 4f * Time.deltaTime;
                target.localPosition = p;
                codeWants = p;
                break;
            }
        }
    }

    private void Scrub(float delta)
    {
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        float t = Mathf.Repeat(info.normalizedTime + delta, 1f);
        animator.Play(info.shortNameHash, 0, t);
    }

    public override void Readout(List<string> lines)
    {
        switch (mode)
        {
            case Mode.Clip:
            {
                AnimatorStateInfo info = animator != null ? animator.GetCurrentAnimatorStateInfo(0) : default(AnimatorStateInfo);
                lines.Add("animator.speed      = " + F(animator != null ? animator.speed : 0f, 1));
                lines.Add("clip normalizedTime = " + F(Mathf.Repeat(info.normalizedTime, 1f), 2));
                lines.Add("clips playing now   = " + (animator != null ? animator.GetCurrentAnimatorClipInfoCount(0) : 0));
                lines.Add("");
                lines.Add("The clip is an asset under Assets/Animations.");
                lines.Add("The Animator is the component that plays it.");
                break;
            }

            case Mode.AnimatorComponent:
                lines.Add("animator.runtimeAnimatorController = " +
                          (animator != null && animator.runtimeAnimatorController != null ? animator.runtimeAnimatorController.name : "-"));
                lines.Add("layer count     = " + (animator != null ? animator.layerCount : 0));
                lines.Add("parameter count = " + (animator != null ? animator.parameterCount : 0));
                lines.Add("current state   = " + CurrentStateName());
                lines.Add("");
                lines.Add("No Animation component is involved: the");
                lines.Add("Animator owns playback through its Controller.");
                break;

            case Mode.Keyframes:
            {
                AnimatorStateInfo info = animator != null ? animator.GetCurrentAnimatorStateInfo(0) : default(AnimatorStateInfo);
                float t = Mathf.Repeat(info.normalizedTime, 1f) * clipLength;
                lines.Add("clip time = " + F(t, 2) + " s   (length " + F(clipLength, 2) + " s)");
                lines.Add("");
                for (int i = 0; i < keyTimes.Length; i++)
                    lines.Add((t >= keyTimes[i] && (i == keyTimes.Length - 1 || t < keyTimes[i + 1]) ? "> " : "  ")
                              + F(keyTimes[i], 2) + "s   " + keyLabels[i]);
                lines.Add("");
                lines.Add("Unity only stores the values you set at the");
                lines.Add("keyframes; everything between is interpolated.");
                break;
            }

            case Mode.Channels:
                lines.Add((highlight == 0 ? "> " : "  ") + "position.y = " + F(target.localPosition.y, 2));
                lines.Add((highlight == 1 ? "> " : "  ") + "colour.r   = " + F(sprite != null ? sprite.color.r : 0f, 2));
                lines.Add((highlight == 2 ? "> " : "  ") + "animValue  = " + F(animValue, 2));
                lines.Add("");
                lines.Add("All three channels live in ONE clip. Any value");
                lines.Add("you can edit in the Inspector can be animated -");
                lines.Add("including fields on your own scripts.");
                break;

            case Mode.OneAtATime:
                lines.Add("current state     = " + CurrentStateName());
                lines.Add("clips playing now = " + (animator != null ? animator.GetCurrentAnimatorClipInfoCount(0) : 0));
                lines.Add("");
                lines.Add("1 means exactly one clip is driving the object.");
                lines.Add("A second clip would fight the first - so Unity");
                lines.Add("lets only one play (transitions blend two).");
                break;

            default:
                lines.Add("code set localPosition = " + V(codeWants));
                lines.Add("object shows           = " + V(target.localPosition));
                lines.Add("");
                lines.Add("The animation is writing the same channel, and");
                lines.Add("the animation wins. Changing it from code has");
                lines.Add("no visible effect - a very common 'bug'.");
                break;
        }
    }

    private string CurrentStateName()
    {
        if (animator == null) return "-";
        if (animator.GetCurrentAnimatorStateInfo(0).IsName(stateA)) return stateA;
        if (animator.GetCurrentAnimatorStateInfo(0).IsName(stateB)) return stateB;
        return "(other)";
    }
}
