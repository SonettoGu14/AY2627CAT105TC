using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The Week 9 (audio) demo stage. Same contract as W08DemoBuilder: one spec per demo key, each
/// build action furnishes its own root and adds the demo component.
///
/// Every W09 demo owns a camera AND a listener (see W09Kit), so the shared "Demo Camera" - and the
/// single AudioListener the other weeks put on it - is switched off whenever a W09 demo is shown.
/// That is why the deck must not warm the demos up (SlidePresenter.warmupDemos would activate all
/// 17 roots for a frame, and 17 audio sources would all make a noise).
///
/// Builders are split in two so the concept demos (slides 3-6) and the practice demos (slides 7-10)
/// are separate files:
///   W09ConceptDemos   why_sound, audio_map, play_replace, oneshot_layer, pitch_vary,
///                     spatial_blend, min_max, rolloff
///   W09PracticeDemos  footsteps, mixer_groups, mixer_bypass, bug_no_listener, bug_min_distance,
///                     bug_output_none, bug_machine_gun, bug_crackling, walkaround
/// </summary>
internal static class W09DemoBuilder
{
    public static List<SlideDemoBuilder.Spec> Specs()
    {
        List<SlideDemoBuilder.Spec> specs = new List<SlideDemoBuilder.Spec>();

        specs.Add(D("why_sound", W09ConceptDemos.BuildWhySound));
        specs.Add(D("audio_map", W09ConceptDemos.BuildAudioMap));
        specs.Add(D("play_replace", W09ConceptDemos.BuildPlayReplace));
        specs.Add(D("oneshot_layer", W09ConceptDemos.BuildOneShotLayer));
        specs.Add(D("pitch_vary", W09ConceptDemos.BuildPitchVary));
        specs.Add(D("spatial_blend", W09ConceptDemos.BuildSpatialBlend));
        specs.Add(D("min_max", W09ConceptDemos.BuildMinMax));
        specs.Add(D("rolloff", W09ConceptDemos.BuildRolloff));

        specs.Add(D("footsteps", W09PracticeDemos.BuildFootsteps));
        specs.Add(D("mixer_groups", W09PracticeDemos.BuildMixerGroups));
        specs.Add(D("mixer_bypass", W09PracticeDemos.BuildMixerBypass));
        specs.Add(D("bug_no_listener", W09PracticeDemos.BuildBugNoListener));
        specs.Add(D("bug_min_distance", W09PracticeDemos.BuildBugMinDistance));
        specs.Add(D("bug_output_none", W09PracticeDemos.BuildBugOutputNone));
        specs.Add(D("bug_machine_gun", W09PracticeDemos.BuildBugMachineGun));
        specs.Add(D("bug_crackling", W09PracticeDemos.BuildBugCrackling));
        specs.Add(D("walkaround", W09PracticeDemos.BuildWalkaround));

        return specs;
    }

    private static SlideDemoBuilder.Spec D(string key, System.Action<Transform> build)
    {
        return new SlideDemoBuilder.Spec { key = key, orthoSize = W09Kit.ViewSize, build = build };
    }
}
