using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

/// <summary>
/// W4 lecture content: the character sprite animation.
///
/// Builds the five AnimationClips (Idle / Walk / Run / Jump / Fall) and the two
/// AnimatorControllers used by the W4 lab. This is week-4 material, which is why it
/// lives in W04_AnimationCamera and not in Common - the W3 lab deliberately has no
/// animation, because animation is not taught until W4.
///
/// Output: Assets/Animations/W04_AnimationCamera/
/// </summary>
public static class CharacterRig
{
    public const string AnimFolder = "Assets/Animations/W04_AnimationCamera";
    public const string PlayerControllerPath = AnimFolder + "/Player.controller";
    public const string PropertyControllerPath = AnimFolder + "/PropertyDemo.controller";

    private static Sprite Pose(string pose)
    {
        return LabKit.CharacterSprite(pose);
    }

    public static void Build()
    {
        AnimationClip idle = BuildSpriteClip("Player_Idle", 8f, true, Pose("idle"));
        AnimationClip walk = BuildSpriteClip("Player_Walk", 12f, true,
            Pose("walk0"), Pose("walk1"), Pose("walk2"), Pose("walk3"),
            Pose("walk4"), Pose("walk5"), Pose("walk6"), Pose("walk7"));
        AnimationClip run = BuildSpriteClip("Player_Run", 14f, true, Pose("run0"), Pose("run1"), Pose("run2"));
        AnimationClip jump = BuildSpriteClip("Player_Jump", 8f, false, Pose("jump"));
        AnimationClip fall = BuildSpriteClip("Player_Fall", 8f, false, Pose("fall"));

        BuildPlayerController(idle, walk, run, jump, fall);
        BuildPropertyController();
        BuildDemoAssets();
    }

    // ==================================================================================
    //  W04 slide-demo assets
    // ==================================================================================

    public const string DemoStatesControllerPath = AnimFolder + "/DemoStates.controller";
    public const string DemoExitControllerPath = AnimFolder + "/DemoExit.controller";

    /// <summary>
    /// Three visually distinct states (idle / walk / run poses), a controller that moves between
    /// them on `Speed`, a second controller whose one transition carries a 0.9 exit time and a 0.6
    /// duration (the transition demos read those numbers), and the clips the Animation demos need.
    /// </summary>
    /// <summary>
    /// Make sure the demo clips and controllers exist. SlideDemoBuilder calls this, so building the
    /// slides scene on its own never leaves an animator demo with a null controller - which would
    /// look like a demo that does nothing rather than like an error.
    /// </summary>
    public static void EnsureDemoAssets()
    {
        BuildDemoAssets();
    }

    private static void BuildDemoAssets()
    {
        AnimationClip a = BuildSpriteClip("Demo_A", 6f, true, Pose("idle"));
        AnimationClip b = BuildSpriteClip("Demo_B", 6f, true, Pose("walk0"), Pose("walk1"), Pose("walk2"), Pose("walk3"));
        AnimationClip c = BuildSpriteClip("Demo_C", 10f, true, Pose("run0"), Pose("run1"), Pose("run2"));

        // --- controller: A -(Speed>0.05)-> B -(Speed>0.6)-> C, and back ---
        AssetDatabase.DeleteAsset(DemoStatesControllerPath);
        AnimatorController states = AnimatorController.CreateAnimatorControllerAtPath(DemoStatesControllerPath);
        states.AddParameter("Speed", AnimatorControllerParameterType.Float);
        states.AddParameter("Flag", AnimatorControllerParameterType.Bool);
        states.AddParameter("Trigger", AnimatorControllerParameterType.Trigger);
        states.AddParameter("Switch", AnimatorControllerParameterType.Trigger);
        AnimatorStateMachine sm = states.layers[0].stateMachine;
        AnimatorState sA = sm.AddState("A", new Vector3(240, 0, 0));
        AnimatorState sB = sm.AddState("B", new Vector3(240, 110, 0));
        AnimatorState sC = sm.AddState("C", new Vector3(240, 220, 0));
        sA.motion = a; sB.motion = b; sC.motion = c;
        sm.defaultState = sA;
        Link(sA, sB, AnimatorConditionMode.Greater, 0.05f, "Speed");
        Link(sB, sA, AnimatorConditionMode.Less, 0.05f, "Speed");
        Link(sB, sC, AnimatorConditionMode.Greater, 0.60f, "Speed");
        Link(sC, sB, AnimatorConditionMode.Less, 0.60f, "Speed");

        // --- controller: one transition with a long Exit Time and a real Duration ---
        AssetDatabase.DeleteAsset(DemoExitControllerPath);
        AnimatorController exit = AnimatorController.CreateAnimatorControllerAtPath(DemoExitControllerPath);
        exit.AddParameter("Switch", AnimatorControllerParameterType.Trigger);
        AnimatorStateMachine esm = exit.layers[0].stateMachine;
        AnimatorState wait = esm.AddState("Wait", new Vector3(240, 0, 0));
        AnimatorState done = esm.AddState("Done", new Vector3(240, 130, 0));
        wait.motion = b; done.motion = c;
        esm.defaultState = wait;
        AnimatorStateTransition t = wait.AddTransition(done);
        t.hasExitTime = true;
        t.exitTime = 0.9f;            // must wait until the clip is 90% done
        t.duration = 0.6f;            // then cross-fade over 0.6 s
        t.hasFixedDuration = true;
        t.AddCondition(AnimatorConditionMode.If, 0f, "Switch");

        // --- clips the Animation demos point at ---
        AnimationClip keyframes = NewDemoClip();
        SetFloatCurve(keyframes, "", typeof(Transform), "m_LocalPosition.y", new[] { 0f, 0.5f, 1f }, new[] { 0f, 1.2f, 0f });
        FinishDemoClip(keyframes, "Demo_Keyframes", true);

        AnimationClip channels = NewDemoClip();
        SetFloatCurve(channels, "", typeof(Transform), "m_LocalPosition.y", new[] { 0f, 0.5f, 1f }, new[] { 0f, 0.9f, 0f });
        SetFloatCurve(channels, "", typeof(SpriteRenderer), "m_Color.r", new[] { 0f, 0.5f, 1f }, new[] { 1f, 0.25f, 1f });
        SetFloatCurve(channels, "", typeof(AnimationDemo), "animValue", new[] { 0f, 0.5f, 1f }, new[] { 0f, 8f, 0f });
        FinishDemoClip(channels, "Demo_Channels", true);

        AnimationClip priority = NewDemoClip();
        SetFloatCurve(priority, "", typeof(Transform), "m_LocalPosition.x", new[] { 0f, 0.5f, 1f }, new[] { -2f, 2f, -2f });
        FinishDemoClip(priority, "Demo_Priority", true);
    }

    private static AnimationClip NewDemoClip()
    {
        AnimationClip clip = new AnimationClip();
        clip.frameRate = 60;
        return clip;
    }

    private static AnimationClip FinishDemoClip(AnimationClip clip, string name, bool loop)
    {
        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
        string path = AnimFolder + "/" + name + ".anim";
        AssetDatabase.DeleteAsset(path);
        AssetDatabase.CreateAsset(clip, path);
        return AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
    }

    // ------------------------------------------------------------------ clips

    /// <summary>One AnimationClip made of a sprite flip-book: a single "m_Sprite" PPtr curve.</summary>
    private static AnimationClip BuildSpriteClip(string assetName, float fps, bool loop, params Sprite[] frames)
    {
        Directory.CreateDirectory(AnimFolder);
        string path = AnimFolder + "/" + assetName + ".anim";
        AssetDatabase.DeleteAsset(path);

        AnimationClip clip = new AnimationClip();
        clip.frameRate = fps;

        if (frames.Length > 0)
        {
            EditorCurveBinding binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
            ObjectReferenceKeyframe[] keys = new ObjectReferenceKeyframe[frames.Length];
            for (int i = 0; i < frames.Length; i++)
            {
                keys[i] = new ObjectReferenceKeyframe { time = i / fps, value = frames[i] };
            }
            AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);
        }

        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
        AssetDatabase.CreateAsset(clip, path);
        return AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
    }

    private static void SetFloatCurve(AnimationClip clip, string path, System.Type type, string property, float[] times, float[] values)
    {
        AnimationCurve curve = new AnimationCurve();
        for (int i = 0; i < times.Length; i++) curve.AddKey(times[i], values[i]);
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(path, type, property), curve);
    }

    /// <summary>The slide exercise: animate position, scale AND colour.</summary>
    private static AnimationClip BuildPropertyClip()
    {
        string path = AnimFolder + "/PropertyDemo.anim";
        AssetDatabase.DeleteAsset(path);
        AnimationClip clip = new AnimationClip();
        clip.frameRate = 30;

        // NOTE: the curves are bound to the child "Visual", not to the root, because
        // animating a transform clears the channels you did NOT key. Animating the child
        // keeps the parent - and the object's authored position - intact. That is the
        // "animation overrides code" slide, made concrete.
        SetFloatCurve(clip, "Visual", typeof(Transform), "m_LocalPosition.y", new[] { 0f, 0.5f, 1f }, new[] { 0f, 0.55f, 0f });
        SetFloatCurve(clip, "Visual", typeof(Transform), "m_LocalScale.x", new[] { 0f, 0.5f, 1f }, new[] { 0.9f, 1.25f, 0.9f });
        SetFloatCurve(clip, "Visual", typeof(Transform), "m_LocalScale.y", new[] { 0f, 0.5f, 1f }, new[] { 0.9f, 1.25f, 0.9f });
        SetFloatCurve(clip, "Visual", typeof(SpriteRenderer), "m_Color.r", new[] { 0f, 0.33f, 0.66f, 1f }, new[] { 1f, 0.35f, 0.35f, 1f });
        SetFloatCurve(clip, "Visual", typeof(SpriteRenderer), "m_Color.g", new[] { 0f, 0.33f, 0.66f, 1f }, new[] { 0.45f, 1f, 0.35f, 0.45f });
        SetFloatCurve(clip, "Visual", typeof(SpriteRenderer), "m_Color.b", new[] { 0f, 0.33f, 0.66f, 1f }, new[] { 0.35f, 0.35f, 1f, 0.35f });

        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
        AssetDatabase.CreateAsset(clip, path);
        return AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
    }

    // ------------------------------------------------------------------ controllers

    private static void BuildPlayerController(AnimationClip idle, AnimationClip walk, AnimationClip run, AnimationClip jump, AnimationClip fall)
    {
        AssetDatabase.DeleteAsset(PlayerControllerPath);
        AnimatorController ctrl = AnimatorController.CreateAnimatorControllerAtPath(PlayerControllerPath);
        ctrl.AddParameter("Speed", AnimatorControllerParameterType.Float);
        ctrl.AddParameter("VerticalSpeed", AnimatorControllerParameterType.Float);
        ctrl.AddParameter("IsGrounded", AnimatorControllerParameterType.Bool);
        ctrl.AddParameter("Jump", AnimatorControllerParameterType.Trigger);

        AnimatorStateMachine sm = ctrl.layers[0].stateMachine;
        AnimatorState sIdle = sm.AddState("Idle", new Vector3(280, 20, 0));
        AnimatorState sWalk = sm.AddState("Walk", new Vector3(280, 120, 0));
        AnimatorState sRun = sm.AddState("Run", new Vector3(280, 220, 0));
        AnimatorState sJump = sm.AddState("Jump", new Vector3(560, 120, 0));
        AnimatorState sFall = sm.AddState("Fall", new Vector3(560, 20, 0));
        sIdle.motion = idle;
        sWalk.motion = walk;
        sRun.motion = run;
        sJump.motion = jump;
        sFall.motion = fall;
        sm.defaultState = sIdle;

        Link(sIdle, sWalk, AnimatorConditionMode.Greater, 0.05f, "Speed");
        Link(sWalk, sIdle, AnimatorConditionMode.Less, 0.05f, "Speed");
        Link(sWalk, sRun, AnimatorConditionMode.Greater, 0.60f, "Speed");
        Link(sRun, sWalk, AnimatorConditionMode.Less, 0.60f, "Speed");
        Link(sFall, sIdle, AnimatorConditionMode.If, 0f, "IsGrounded");

        AnimatorStateTransition toJump = sm.AddAnyStateTransition(sJump);
        toJump.hasExitTime = false;
        toJump.duration = 0.02f;
        toJump.canTransitionToSelf = false;
        toJump.AddCondition(AnimatorConditionMode.If, 0f, "Jump");

        AnimatorStateTransition toFall = sm.AddAnyStateTransition(sFall);
        toFall.hasExitTime = false;
        toFall.duration = 0.05f;
        toFall.canTransitionToSelf = false;
        toFall.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsGrounded");
        toFall.AddCondition(AnimatorConditionMode.Less, -0.1f, "VerticalSpeed");
    }

    private static void BuildPropertyController()
    {
        AnimationClip clip = BuildPropertyClip();
        AssetDatabase.DeleteAsset(PropertyControllerPath);
        AnimatorController ctrl = AnimatorController.CreateAnimatorControllerAtPath(PropertyControllerPath);
        AnimatorState state = ctrl.layers[0].stateMachine.AddState("PropertyDemo");
        state.motion = clip;
        ctrl.layers[0].stateMachine.defaultState = state;
    }

    private static void Link(AnimatorState from, AnimatorState to, AnimatorConditionMode mode, float threshold, string parameter)
    {
        AnimatorStateTransition t = from.AddTransition(to);
        t.hasExitTime = false;
        t.duration = 0f;
        t.hasFixedDuration = true;
        t.AddCondition(mode, threshold, parameter);
    }
}
