#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class SampleSceneLocomotionControllerBuilder
{
    private const string Ual1Path = "Assets/Game/Art/Animations/Locomotion/UAL1_Standard.fbx";
    private const string Ual2Path = "Assets/Game/Art/Animations/Locomotion/UAL2_Standard.fbx";
    private const string ControllerPath = "Assets/Resources/Animators/SampleSceneLocomotion.controller";

    private static readonly string[] IdleCandidates =
    {
        "Armature|Idle_Loop",
        "__preview__Armature|Idle_Loop",
        "Armature|Idle_No_Loop",
        "__preview__Armature|Idle_No_Loop"
    };

    private static readonly string[] LegacyIdleCandidates =
    {
        "Armature|Idle_Loop",
        "__preview__Armature|Idle_Loop"
    };

    private static readonly string[] WalkCandidates =
    {
        "Armature|Walk_Loop",
        "__preview__Armature|Walk_Loop"
    };

    private static readonly string[] JogCandidates =
    {
        "Armature|Jog_Fwd_Loop",
        "__preview__Armature|Jog_Fwd_Loop"
    };

    private static readonly string[] SprintCandidates =
    {
        "Armature|Sprint_Loop",
        "__preview__Armature|Sprint_Loop"
    };

    private static readonly string[] TalkCandidates =
    {
        "Armature|Idle_Talking_Loop",
        "__preview__Armature|Idle_Talking_Loop"
    };

    [MenuItem("Tools/Holstin/Animation/Build Sample Scene Locomotion Controller", false, 140)]
    public static void BuildMenu()
    {
        BuildController();
    }

    public static void BuildController()
    {
        EnsureFolder("Assets/Resources");
        EnsureFolder("Assets/Resources/Animators");

        Dictionary<string, AnimationClip> clips = LoadClipMap(Ual1Path);
        Dictionary<string, AnimationClip> ual2Clips = LoadClipMap(Ual2Path);
        AnimationClip idle = ResolveClip(clips, IdleCandidates) ?? ResolveClip(ual2Clips, IdleCandidates);
        AnimationClip legacyIdle = ResolveClip(clips, LegacyIdleCandidates) ?? idle;
        AnimationClip walk = ResolveClip(clips, WalkCandidates) ?? ResolveClip(ual2Clips, WalkCandidates);
        AnimationClip jog = ResolveClip(clips, JogCandidates) ?? ResolveClip(ual2Clips, JogCandidates);
        AnimationClip sprint = ResolveClip(clips, SprintCandidates) ?? ResolveClip(ual2Clips, SprintCandidates);
        AnimationClip talk = ResolveClip(clips, TalkCandidates) ?? ResolveClip(ual2Clips, TalkCandidates) ?? legacyIdle;

        if (idle == null || walk == null || jog == null || sprint == null)
        {
            throw new InvalidOperationException(
                "Missing locomotion clips in UAL1_Standard.fbx. Expected Idle/Walk/Jog/Sprint clips.");
        }

        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (controller != null)
        {
            AssetDatabase.DeleteAsset(ControllerPath);
            AssetDatabase.Refresh();
        }

        controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);

        controller.parameters = Array.Empty<AnimatorControllerParameter>();
        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        controller.AddParameter("IsTalking", AnimatorControllerParameterType.Bool);
        controller.AddParameter("IdlePose", AnimatorControllerParameterType.Float);

        AnimatorControllerLayer layer = controller.layers != null && controller.layers.Length > 0
            ? controller.layers[0]
            : new AnimatorControllerLayer();
        layer.name = "Base Layer";
        layer.defaultWeight = 1f;
        AnimatorStateMachine stateMachine = layer.stateMachine;
        if (stateMachine == null)
        {
            stateMachine = new AnimatorStateMachine
            {
                name = "SampleSceneLocomotionSM"
            };
            AssetDatabase.AddObjectToAsset(stateMachine, controller);
        }

        ClearStateMachine(stateMachine);
        layer.stateMachine = stateMachine;
        controller.layers = new[] { layer };

        BlendTree locomotionBlend = new BlendTree
        {
            name = "LocomotionBlend",
            blendType = BlendTreeType.Simple1D,
            blendParameter = "Speed",
            useAutomaticThresholds = false
        };

        AssetDatabase.AddObjectToAsset(locomotionBlend, controller);
        locomotionBlend.AddChild(idle, 0f);
        locomotionBlend.AddChild(walk, 2f);
        locomotionBlend.AddChild(jog, 4.4f);
        locomotionBlend.AddChild(sprint, 6.2f);

        AnimatorState locomotionState = stateMachine.AddState("Locomotion");
        locomotionState.motion = locomotionBlend;
        locomotionState.writeDefaultValues = true;

        AnimatorState talkState = stateMachine.AddState("Talk");
        talkState.motion = talk;
        talkState.writeDefaultValues = true;

        stateMachine.defaultState = locomotionState;

        AnimatorStateTransition locomotionToTalk = locomotionState.AddTransition(talkState);
        locomotionToTalk.hasExitTime = false;
        locomotionToTalk.hasFixedDuration = true;
        locomotionToTalk.duration = 0.12f;
        locomotionToTalk.offset = 0f;
        locomotionToTalk.AddCondition(AnimatorConditionMode.If, 0f, "IsTalking");

        AnimatorStateTransition talkToLocomotion = talkState.AddTransition(locomotionState);
        talkToLocomotion.hasExitTime = false;
        talkToLocomotion.hasFixedDuration = true;
        talkToLocomotion.duration = 0.15f;
        talkToLocomotion.offset = 0f;
        talkToLocomotion.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsTalking");

        EditorUtility.SetDirty(controller);
        EditorUtility.SetDirty(stateMachine);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"SAMPLE_SCENE_LOCOMOTION_CONTROLLER_READY: {ControllerPath}");
    }

    private static void ClearStateMachine(AnimatorStateMachine stateMachine)
    {
        if (stateMachine == null)
        {
            return;
        }

        ChildAnimatorState[] states = stateMachine.states;
        for (int i = states.Length - 1; i >= 0; i--)
        {
            AnimatorState state = states[i].state;
            stateMachine.RemoveState(state);
            if (state != null)
            {
                UnityEngine.Object.DestroyImmediate(state, true);
            }
        }

        ChildAnimatorStateMachine[] childStateMachines = stateMachine.stateMachines;
        for (int i = childStateMachines.Length - 1; i >= 0; i--)
        {
            AnimatorStateMachine child = childStateMachines[i].stateMachine;
            stateMachine.RemoveStateMachine(child);
            if (child != null)
            {
                UnityEngine.Object.DestroyImmediate(child, true);
            }
        }

        stateMachine.anyStateTransitions = Array.Empty<AnimatorStateTransition>();
        stateMachine.entryTransitions = Array.Empty<AnimatorTransition>();
    }

    private static Dictionary<string, AnimationClip> LoadClipMap(string path)
    {
        UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
        Dictionary<string, AnimationClip> map = new Dictionary<string, AnimationClip>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < assets.Length; i++)
        {
            if (assets[i] is not AnimationClip clip)
            {
                continue;
            }

            map[clip.name] = clip;
        }

        return map;
    }

    private static AnimationClip ResolveClip(Dictionary<string, AnimationClip> clips, IEnumerable<string> candidates)
    {
        foreach (string candidate in candidates)
        {
            if (clips.TryGetValue(candidate, out AnimationClip clip) && clip != null)
            {
                return clip;
            }
        }

        return null;
    }

    private static void EnsureFolder(string folderPath)
    {
        if (AssetDatabase.IsValidFolder(folderPath))
        {
            return;
        }

        string normalized = folderPath.Replace('\\', '/');
        string parent = Path.GetDirectoryName(normalized)?.Replace('\\', '/') ?? "Assets";
        string name = Path.GetFileName(normalized);
        if (!AssetDatabase.IsValidFolder(parent))
        {
            EnsureFolder(parent);
        }

        AssetDatabase.CreateFolder(parent, name);
    }
}
#endif
