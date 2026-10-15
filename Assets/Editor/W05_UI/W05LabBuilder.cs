using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// W5's lab STARTER - the scene a lab session begins from.
///
/// Deliberately minimal: a floor, two small platforms, the character (the W3 body with the W4
/// Animator on it) and a follow camera. No coins, no goal, no hazard, and **no UI at all** - not even
/// the lab HUD, so there is no title bar, input visualiser or key hints on screen. The UI is what the
/// students add during the session, which is why this scene exists separately from W05Playground,
/// where that UI is already finished.
///
/// So the week reads: W5Lab (start here) -> add UI -> W05Playground (what it looks like done).
/// </summary>
public static class W05LabBuilder
{
    [MenuItem("CAT105TC/W05 UI/Build W5 Lab (Starter)")]
    public static void BuildMenu()
    {
        Build();
    }

    public static void Build()
    {
        LabKit.SetupSharedAssets();
        CharacterRig.Build();                 // the character's clips and controllers

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        PlayerController2D player;
        BuildGameplay(null, out player, out Camera _);

        // No HUD. The starter is a blank slate: the lab HUD's title bar, input visualiser and key
        // hints are exactly the chrome that would be in the way while a class adds its own UI.
        EditorSceneManager.SaveScene(scene, LabKit.W05LabPath);
        LabKit.AddSceneToBuildSettings(LabKit.W05LabPath);
        AssetDatabase.SaveAssets();
        Debug.Log("[W05LabBuilder] W05Lab (starter) built.");
    }

    /// <summary>
    /// A floor and two platforms, the character, and a follow camera - nothing else. Kept public so
    /// a later week can start from the same body.
    /// </summary>
    public static void BuildGameplay(Transform parent, out PlayerController2D player, out Camera cam)
    {
        GameObject env = new GameObject("Environment");
        if (parent != null) env.transform.SetParent(parent, false);
        LabKit.MakeLight(parent);

        LabKit.Platform(env.transform, "Ground", 0f, -1f, 20f, 1f, LabKit.GroundColor, "Ground");
        LabKit.Platform(env.transform, "Platform_A", -4f, 1.4f, 3.5f, 0.5f, LabKit.PlatformColor, "Ground");
        LabKit.Platform(env.transform, "Platform_B", 4f, 2.8f, 3.5f, 0.5f, LabKit.PlatformColor, "Ground");

        GameObject playerGO = W03PlaygroundBuilder.CreatePlayer(parent, new Vector3(-7f, -0.5f, 0f),
                                                                new Vector3(-7f, -0.5f, 0f), out player);

        // the W4 animator, so the character looks alive while the students work on the UI
        GameObject visual = playerGO.transform.Find("Visual").gameObject;
        Animator animator = visual.AddComponent<Animator>();
        animator.runtimeAnimatorController =
            AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(CharacterRig.PlayerControllerPath);
        animator.applyRootMotion = false;
        player.animator = animator;

        cam = W03PlaygroundBuilder.MakeFollowCamera(parent, new Vector3(-7f, 1f, -10f), 5f, player.transform);
    }
}
