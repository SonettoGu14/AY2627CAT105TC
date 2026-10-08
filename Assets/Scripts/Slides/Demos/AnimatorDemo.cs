using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Slides 15-21, one Animator idea per demo:
///   Basics      - what the Animator manages
///   States      - a State is a clip; one is playing
///   Default     - exactly one State is the default (the orange one)
///   Transitions - a Transition switches States when its conditions hold
///   ExitTime    - Exit Time decides WHEN a transition may fire
///   Duration    - Transition Duration blends the two clips
///   Vars        - float / int / bool / trigger
///   BoolTrigger - a bool stays set; a trigger clears itself
///   Scripting   - animator.SetBool / SetTrigger from code
///   Play        - animator.Play cuts; a transition blends
/// </summary>
public class AnimatorDemo : DemoBase
{
    public enum Mode { Basics, States, Default, Transitions, ExitTime, Duration, Vars, BoolTrigger, Scripting, Play }

    public Mode mode = Mode.Basics;

    public Animator animator;
    public string[] stateNames = { "A", "B", "C" };
    public string defaultState = "A";
    public float walkThreshold = 0.05f;
    public float runThreshold = 0.6f;
    public float transitionExitTime = 0.9f;
    public float transitionDuration = 0.6f;

    private float speed;
    private float lastCodeCall;

    public override string Title
    {
        get
        {
            switch (mode)
            {
                case Mode.Basics: return "the Animator manages the order and logic of animation";
                case Mode.States: return "a State holds one clip; the Animator is in one State";
                case Mode.Default: return "exactly one State is the default (the orange one)";
                case Mode.Transitions: return "a Transition switches States when its conditions are met";
                case Mode.ExitTime: return "Exit Time decides when a transition is allowed to fire";
                case Mode.Duration: return "Transition Duration blends the two clips";
                case Mode.Vars: return "Animator variables: float / int / bool / trigger";
                case Mode.BoolTrigger: return "a bool stays set; a trigger clears itself";
                case Mode.Scripting: return "changing Animator variables from code";
                default: return "animator.Play(..) cuts; a transition blends";
            }
        }
    }

    public override string Keys
    {
        get
        {
            switch (mode)
            {
                case Mode.Basics: return "-";
                case Mode.States: return "A / D  change Speed (the state follows)";
                case Mode.Default: return "D  jump back to the default State";
                case Mode.Transitions: return "A / D  change Speed past the thresholds";
                case Mode.ExitTime: return "Space  request the switch (it waits for Exit Time)";
                case Mode.Duration: return "Space  request the switch (watch the blend)";
                case Mode.Vars: return "1 float   2 int   3 bool   4 trigger";
                case Mode.BoolTrigger: return "B  toggle the bool      T  fire the trigger";
                case Mode.Scripting: return "B  SetBool      T  SetTrigger";
                default: return "A / D  animator.Play (cut)      Space  a transition (blend)";
            }
        }
    }

    public override void OnActivate()
    {
        speed = 0f;
        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
            animator.SetBool("Flag", false);
            animator.ResetTrigger("Trigger");
            animator.ResetTrigger("Switch");
        }
    }

    private void Update()
    {
        if (animator == null) return;

        switch (mode)
        {
            case Mode.States:
            case Mode.Transitions:
                if (Input.GetKey(KeyCode.A)) speed = Mathf.Max(0f, speed - 0.8f * Time.deltaTime);
                if (Input.GetKey(KeyCode.D)) speed = Mathf.Min(1f, speed + 0.8f * Time.deltaTime);
                animator.SetFloat("Speed", speed);
                break;

            case Mode.Default:
                if (Input.GetKeyDown(KeyCode.D)) animator.Play(defaultState, 0, 0f);
                break;

            case Mode.ExitTime:
                if (Input.GetKeyDown(KeyCode.Space)) { animator.SetTrigger("Switch"); lastCodeCall = Time.time; }
                break;

            case Mode.Duration:
                if (Input.GetKeyDown(KeyCode.Space)) { animator.SetTrigger("Switch"); lastCodeCall = Time.time; }
                break;

            case Mode.Vars:
                if (Input.GetKey(KeyCode.Alpha1)) animator.SetFloat("Speed", Mathf.Repeat(animator.GetFloat("Speed") + 0.5f * Time.deltaTime, 1f));
                if (Input.GetKeyDown(KeyCode.Alpha2)) animator.SetInteger("Level", (animator.GetInteger("Level") + 1) % 4);
                if (Input.GetKeyDown(KeyCode.Alpha3)) animator.SetBool("Flag", !animator.GetBool("Flag"));
                if (Input.GetKeyDown(KeyCode.Alpha4)) animator.SetTrigger("Trigger");
                break;

            case Mode.BoolTrigger:
                if (Input.GetKeyDown(KeyCode.B)) animator.SetBool("Flag", !animator.GetBool("Flag"));
                if (Input.GetKeyDown(KeyCode.T)) animator.SetTrigger("Trigger");
                break;

            case Mode.Scripting:
                if (Input.GetKeyDown(KeyCode.B)) { animator.SetBool("Flag", !animator.GetBool("Flag")); lastCodeCall = Time.time; }
                if (Input.GetKeyDown(KeyCode.T)) { animator.SetTrigger("Trigger"); lastCodeCall = Time.time; }
                break;

            case Mode.Play:
                if (Input.GetKeyDown(KeyCode.A)) animator.Play(stateNames[0], 0, 0f);
                if (Input.GetKeyDown(KeyCode.D)) animator.Play(stateNames[1], 0, 0f);
                if (Input.GetKeyDown(KeyCode.Space)) animator.SetTrigger("Switch");
                break;
        }
    }

    public override void Readout(List<string> lines)
    {
        switch (mode)
        {
            case Mode.Basics:
                lines.Add("controller = " + (animator.runtimeAnimatorController != null ? animator.runtimeAnimatorController.name : "-"));
                lines.Add("states     = " + stateNames.Length);
                lines.Add("parameters = " + animator.parameterCount);
                lines.Add("state now  = " + Current());
                lines.Add("");
                lines.Add("States  - which clip is playing");
                lines.Add("Transitions - when to move between them");
                lines.Add("Variables   - the conditions those transitions read");
                lines.Add("Layers      - separate state machines you can blend");
                break;

            case Mode.States:
                lines.Add("Speed      = " + F(speed, 2));
                lines.Add("state now  = " + Current());
                lines.Add("normalizedTime = " + F(animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1f, 2));
                lines.Add("");
                lines.Add("Each State plays one AnimationClip. The Animator");
                lines.Add("reaches a State and that clip plays.");
                break;

            case Mode.Default:
                lines.Add("default State = " + defaultState + "   (the orange one)");
                lines.Add("state now     = " + Current());
                lines.Add("");
                for (int i = 0; i < stateNames.Length; i++)
                    lines.Add((stateNames[i] == defaultState ? "  [start]" : "        ") + "  " + stateNames[i]);
                lines.Add("");
                lines.Add("An Animator has exactly one starting State; it");
                lines.Add("plays that the moment the game starts.");
                break;

            case Mode.Transitions:
                lines.Add("Speed = " + F(speed, 2));
                lines.Add("   Idle -> Walk  when Speed > " + F(walkThreshold, 2));
                lines.Add("   Walk -> Run   when Speed > " + F(runThreshold, 2));
                lines.Add("state now = " + Current());
                lines.Add("");
                lines.Add("A Transition is the arrow between two States.");
                lines.Add("It fires when every condition under it is true.");
                break;

            case Mode.ExitTime:
            {
                float norm = animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1f;
                bool pending = Input.GetKeyDown(KeyCode.Space) || Time.time - lastCodeCall < 1.5f;
                lines.Add("transition Exit Time = " + F(transitionExitTime, 2));
                lines.Add("this clip is at      = " + F(norm, 2));
                lines.Add("requested            = " + B(pending));
                lines.Add("waiting?             = " + B(pending && norm < transitionExitTime));
                lines.Add("state now            = " + Current());
                lines.Add("");
                lines.Add("Exit Time is how far through the CURRENT clip the");
                lines.Add("transition must be before it may fire - it is a");
                lines.Add("delay, not a condition.");
                break;
            }

            case Mode.Duration:
                lines.Add("transition Duration = " + F(transitionDuration, 2) + " s");
                lines.Add("in a transition now = " + B(animator.IsInTransition(0)));
                lines.Add("clips playing now   = " + animator.GetCurrentAnimatorClipInfoCount(0));
                lines.Add("state now           = " + Current());
                lines.Add("");
                lines.Add("Duration blends the two clips: 0 cuts instantly,");
                lines.Add("a larger value cross-fades. That is why the");
                lines.Add("clip count reads 2 mid-transition.");
                break;

            case Mode.Vars:
                lines.Add("float   Speed   = " + F(animator.GetFloat("Speed"), 2));
                lines.Add("int     Level   = " + animator.GetInteger("Level"));
                lines.Add("bool    Flag    = " + B(animator.GetBool("Flag")));
                lines.Add("trigger Trigger = " + B(animator.GetBool("Trigger")));
                lines.Add("");
                lines.Add("float - a number (0.37)");
                lines.Add("int   - a counter (2)");
                lines.Add("bool  - a switch that stays where you put it");
                lines.Add("trigger - true for one moment, then clears");
                break;

            case Mode.BoolTrigger:
                lines.Add("bool    Flag    = " + B(animator.GetBool("Flag")));
                lines.Add("trigger Trigger = " + B(animator.GetBool("Trigger")));
                lines.Add("");
                lines.Add("Press B: the bool stays true until you press it again.");
                lines.Add("Press T: the trigger reads true for an instant, the");
                lines.Add("Animator consumes it, and it is false again.");
                break;

            case Mode.Scripting:
                lines.Add("the last call:");
                lines.Add(Time.time - lastCodeCall < 1.2f
                    ? (animator.GetBool("Flag") ? "   animator.SetBool(\"Flag\", true)" : "   animator.SetBool(\"Flag\", false)")
                    : "   animator.SetTrigger(\"Trigger\")");
                lines.Add("");
                lines.Add("Flag    = " + B(animator.GetBool("Flag")));
                lines.Add("Trigger = " + B(animator.GetBool("Trigger")));
                lines.Add("");
                lines.Add("Note `animator`, not `Animator`: this is the");
                lines.Add("component on THIS object - the class name would");
                lines.Add("refer to the type, not an instance.");
                break;

            default:
                lines.Add("state now         = " + Current());
                lines.Add("in a transition   = " + B(animator.IsInTransition(0)));
                lines.Add("clips playing now = " + animator.GetCurrentAnimatorClipInfoCount(0));
                lines.Add("");
                lines.Add("animator.Play(name, 0, 0) jumps straight to a");
                lines.Add("State with no blending - right for 2D frame");
                lines.Add("animation. A Transition blends the two clips.");
                break;
        }
    }

    private string Current()
    {
        for (int i = 0; i < stateNames.Length; i++)
            if (animator.GetCurrentAnimatorStateInfo(0).IsName(stateNames[i])) return stateNames[i] + "  (clip)";
        return "(transition)";
    }
}
