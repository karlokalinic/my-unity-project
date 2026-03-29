using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

#if UNITY_EDITOR
using UnityEditor;
#endif

[DisallowMultipleComponent]
public class SampleSceneLocomotionAnimatorDriver : MonoBehaviour
{
    private const float WalkThreshold = 2f;
    private const float JogThreshold = 4.4f;
    private const float SprintThreshold = 6.2f;
    private const string LocomotionClipPathPrimary = "Assets/Game/Art/Animations/Locomotion/UAL1_Standard.fbx";
    private const string LocomotionClipPathSecondary = "Assets/Game/Art/Animations/Locomotion/UAL2_Standard.fbx";

    [SerializeField] private Transform modelRoot;
    [SerializeField] private float speedDamping = 12f;

    private Animator[] animators = Array.Empty<Animator>();
    private PlayerMover playerMover;
    private PlayerInteraction playerInteraction;
    private Rigidbody body;
    private NpcDialogueController npcDialogue;
    private SampleSceneSourceModelIdlePose fallbackIdlePose;
    private float smoothedSpeed;

    private readonly List<PlayableState> playableStates = new List<PlayableState>();

    private static bool clipCacheResolved;
    private static float nextClipResolveRetryTime;
    private static AnimationClip idleClip;
    private static AnimationClip walkClip;
    private static AnimationClip jogClip;
    private static AnimationClip sprintClip;
    private static AnimationClip talkClip;

    private struct PlayableState
    {
        public Animator Animator;
        public PlayableGraph Graph;
        public AnimationMixerPlayable Mixer;
    }

    public void Configure(Transform configuredModelRoot)
    {
        modelRoot = configuredModelRoot;
        CacheAnimators();
        CacheSources();
        RebuildPlayableGraphs();
    }

    private void Awake()
    {
        CacheSources();
        CacheAnimators();
        RebuildPlayableGraphs();
    }

    private void OnEnable()
    {
        CacheSources();
        CacheAnimators();
        RebuildPlayableGraphs();
    }

    private void OnDisable()
    {
        DestroyPlayableGraphs();
    }

    private void OnDestroy()
    {
        DestroyPlayableGraphs();
    }

    private void Update()
    {
        if (animators == null || animators.Length == 0)
        {
            CacheAnimators();
            if (animators == null || animators.Length == 0)
            {
                return;
            }
        }

        if (playableStates.Count == 0)
        {
            RebuildPlayableGraphs();
        }

        float targetSpeed = ResolvePlanarSpeed();
        float blend = 1f - Mathf.Exp(-Mathf.Max(0.01f, speedDamping) * Time.deltaTime);
        smoothedSpeed = Mathf.Lerp(smoothedSpeed, targetSpeed, blend);

        bool talking = ResolveDialogueTalking();
        UpdatePlayableWeights(smoothedSpeed, talking);
    }

    private void CacheSources()
    {
        playerMover = GetComponent<PlayerMover>();
        playerInteraction = GetComponent<PlayerInteraction>();
        body = GetComponent<Rigidbody>();
        npcDialogue = GetComponent<NpcDialogueController>();
        fallbackIdlePose = GetComponent<SampleSceneSourceModelIdlePose>();
    }

    private void CacheAnimators()
    {
        Transform root = modelRoot != null ? modelRoot : transform;
        Animator[] discovered = root.GetComponentsInChildren<Animator>(true);
        if (discovered == null || discovered.Length == 0)
        {
            animators = Array.Empty<Animator>();
            return;
        }

        Animator primary = ResolvePrimaryAnimator(discovered, root);
        animators = primary != null
            ? new[] { primary }
            : discovered;
    }

    private void RebuildPlayableGraphs()
    {
        DestroyPlayableGraphs();
        if (!TryResolveLocomotionClips())
        {
            SyncFallbackPoseState(true);
            return;
        }

        for (int i = 0; i < animators.Length; i++)
        {
            Animator animator = animators[i];
            if (animator == null)
            {
                continue;
            }

            PlayableGraph graph = PlayableGraph.Create($"SampleSceneLocomotion_{animator.name}_{i}");
            graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

            AnimationPlayableOutput output = AnimationPlayableOutput.Create(graph, "Animation", animator);
            AnimationMixerPlayable mixer = AnimationMixerPlayable.Create(graph, 5);

            AnimationClipPlayable idlePlayable = AnimationClipPlayable.Create(graph, idleClip);
            AnimationClipPlayable walkPlayable = AnimationClipPlayable.Create(graph, walkClip);
            AnimationClipPlayable jogPlayable = AnimationClipPlayable.Create(graph, jogClip);
            AnimationClipPlayable sprintPlayable = AnimationClipPlayable.Create(graph, sprintClip);
            AnimationClipPlayable talkPlayable = AnimationClipPlayable.Create(graph, talkClip ?? idleClip);

            ConfigureClipPlayable(idlePlayable);
            ConfigureClipPlayable(walkPlayable);
            ConfigureClipPlayable(jogPlayable);
            ConfigureClipPlayable(sprintPlayable);
            ConfigureClipPlayable(talkPlayable);

            graph.Connect(idlePlayable, 0, mixer, 0);
            graph.Connect(walkPlayable, 0, mixer, 1);
            graph.Connect(jogPlayable, 0, mixer, 2);
            graph.Connect(sprintPlayable, 0, mixer, 3);
            graph.Connect(talkPlayable, 0, mixer, 4);

            mixer.SetInputWeight(0, 1f);
            mixer.SetInputWeight(1, 0f);
            mixer.SetInputWeight(2, 0f);
            mixer.SetInputWeight(3, 0f);
            mixer.SetInputWeight(4, 0f);

            output.SetSourcePlayable(mixer);

            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            animator.runtimeAnimatorController = null;
            animator.enabled = true;
            animator.Rebind();
            if (animator.gameObject.activeInHierarchy)
            {
                animator.Update(0f);
            }

            graph.Play();
            playableStates.Add(new PlayableState
            {
                Animator = animator,
                Graph = graph,
                Mixer = mixer
            });
        }

        SyncFallbackPoseState(playableStates.Count == 0);
    }

    private static void ConfigureClipPlayable(AnimationClipPlayable playable)
    {
        playable.SetApplyFootIK(false);
        playable.SetApplyPlayableIK(false);
        playable.SetDuration(double.PositiveInfinity);
        playable.SetTime(0);
    }

    private void DestroyPlayableGraphs()
    {
        for (int i = 0; i < playableStates.Count; i++)
        {
            PlayableState state = playableStates[i];
            if (state.Graph.IsValid())
            {
                state.Graph.Destroy();
            }
        }

        playableStates.Clear();
        SyncFallbackPoseState(true);
    }

    private void UpdatePlayableWeights(float speed, bool talking)
    {
        if (playableStates.Count == 0)
        {
            return;
        }

        float idleWeight = 0f;
        float walkWeight = 0f;
        float jogWeight = 0f;
        float sprintWeight = 0f;
        float talkWeight = 0f;

        if (talking)
        {
            talkWeight = 1f;
        }
        else if (speed <= WalkThreshold)
        {
            float t = Mathf.Clamp01(speed / WalkThreshold);
            idleWeight = 1f - t;
            walkWeight = t;
        }
        else if (speed <= JogThreshold)
        {
            float t = Mathf.Clamp01((speed - WalkThreshold) / (JogThreshold - WalkThreshold));
            walkWeight = 1f - t;
            jogWeight = t;
        }
        else if (speed <= SprintThreshold)
        {
            float t = Mathf.Clamp01((speed - JogThreshold) / (SprintThreshold - JogThreshold));
            jogWeight = 1f - t;
            sprintWeight = t;
        }
        else
        {
            sprintWeight = 1f;
        }

        for (int i = 0; i < playableStates.Count; i++)
        {
            PlayableState state = playableStates[i];
            if (!state.Graph.IsValid() || !state.Mixer.IsValid())
            {
                continue;
            }

            state.Mixer.SetInputWeight(0, idleWeight);
            state.Mixer.SetInputWeight(1, walkWeight);
            state.Mixer.SetInputWeight(2, jogWeight);
            state.Mixer.SetInputWeight(3, sprintWeight);
            state.Mixer.SetInputWeight(4, talkWeight);
        }
    }

    private float ResolvePlanarSpeed()
    {
        if (playerMover != null)
        {
            return playerMover.CurrentPlanarSpeed;
        }

        if (body != null)
        {
            Vector3 planar = body.linearVelocity;
            planar.y = 0f;
            return planar.magnitude;
        }

        return 0f;
    }

    private bool ResolveDialogueTalking()
    {
        if (InputReader.CurrentContext != InputReader.InputContext.Dialogue)
        {
            return false;
        }

        if (npcDialogue != null && npcDialogue.IsRunningConversation)
        {
            return true;
        }

        if (playerInteraction != null)
        {
            DialoguePanelUI panel = HolstinFeedback.ResolveDialoguePanel();
            return panel != null && panel.IsShowing && playerInteraction.IsBusy;
        }

        return false;
    }

    private static bool TryResolveLocomotionClips()
    {
        if (clipCacheResolved && idleClip != null)
        {
            return true;
        }

        if (clipCacheResolved && idleClip == null && Time.realtimeSinceStartup < nextClipResolveRetryTime)
        {
            return false;
        }

#if UNITY_EDITOR
        Dictionary<string, AnimationClip> clips = new Dictionary<string, AnimationClip>(StringComparer.OrdinalIgnoreCase);
        MergeClipMap(clips, LoadClipMapFromAssetPath(LocomotionClipPathPrimary));
        MergeClipMap(clips, LoadClipMapFromAssetPath(LocomotionClipPathSecondary));

        if (clips.Count == 0)
        {
            TryInvokeEditorLocomotionBuilder();
            MergeClipMap(clips, LoadClipMapFromAssetPath(LocomotionClipPathPrimary));
            MergeClipMap(clips, LoadClipMapFromAssetPath(LocomotionClipPathSecondary));
        }

        if (clips.Count == 0)
        {
            MergeClipMap(clips, SearchProjectLocomotionClips());
        }

        if (clips.Count == 0)
        {
            clipCacheResolved = false;
            nextClipResolveRetryTime = Time.realtimeSinceStartup + 0.75f;
            return false;
        }

        Dictionary<string, AnimationClip> preferred = new Dictionary<string, AnimationClip>(StringComparer.OrdinalIgnoreCase);
        MergeClipMap(preferred, LoadClipMapFromAssetPath(LocomotionClipPathPrimary));
        MergeClipMap(preferred, LoadClipMapFromAssetPath(LocomotionClipPathSecondary));

        if (preferred.Count > 0)
        {
            clips = preferred;
        }

        idleClip = ResolveClip(clips, "Armature|Idle_Loop", "__preview__Armature|Idle_Loop");
        walkClip = ResolveClip(clips, "Armature|Walk_Loop", "__preview__Armature|Walk_Loop");
        jogClip = ResolveClip(clips, "Armature|Jog_Fwd_Loop", "__preview__Armature|Jog_Fwd_Loop");
        sprintClip = ResolveClip(clips, "Armature|Sprint_Loop", "__preview__Armature|Sprint_Loop");
        talkClip = ResolveClip(clips, "Armature|Idle_Talking_Loop", "__preview__Armature|Idle_Talking_Loop");

        idleClip ??= ResolveClipByKeywords(clips, new[] { "idle" }, new[] { "talk", "attack", "stab", "punch" });
        walkClip ??= ResolveClipByKeywords(clips, new[] { "walk" }, new[] { "back", "strafe", "left", "right" });
        jogClip ??= ResolveClipByKeywords(clips, new[] { "jog" }, new[] { "back", "strafe", "left", "right" });
        jogClip ??= ResolveClipByKeywords(clips, new[] { "run" }, new[] { "sprint", "back", "strafe", "left", "right" });
        sprintClip ??= ResolveClipByKeywords(clips, new[] { "sprint" }, new[] { "back", "strafe", "left", "right" });
        sprintClip ??= ResolveClipByKeywords(clips, new[] { "run" }, new[] { "back", "strafe", "left", "right" });
        talkClip ??= ResolveClipByKeywords(clips, new[] { "talk", "speak" }, Array.Empty<string>());

        idleClip ??= ResolveAnyUsableClip(clips);
        walkClip ??= idleClip;
        jogClip ??= walkClip;
        sprintClip ??= jogClip;
        talkClip ??= idleClip;

        if (idleClip != null)
        {
            clipCacheResolved = true;
            Debug.Log($"SampleSceneLocomotionAnimatorDriver clips: idle={idleClip.name}, walk={walkClip?.name}, jog={jogClip?.name}, sprint={sprintClip?.name}, talk={talkClip?.name}");
        }
        else
        {
            clipCacheResolved = false;
            nextClipResolveRetryTime = Time.realtimeSinceStartup + 0.75f;
        }
#else
        clipCacheResolved = false;
        nextClipResolveRetryTime = Time.realtimeSinceStartup + 1f;
#endif

        return idleClip != null;
    }

#if UNITY_EDITOR
    private static Dictionary<string, AnimationClip> LoadClipMapFromAssetPath(string path)
    {
        Dictionary<string, AnimationClip> map = new Dictionary<string, AnimationClip>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(path))
        {
            return map;
        }

        UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
        for (int i = 0; i < assets.Length; i++)
        {
            if (assets[i] is AnimationClip clip && clip != null)
            {
                map[clip.name] = clip;
            }
        }

        return map;
    }

    private static void MergeClipMap(Dictionary<string, AnimationClip> target, Dictionary<string, AnimationClip> source)
    {
        if (target == null || source == null)
        {
            return;
        }

        foreach (KeyValuePair<string, AnimationClip> kv in source)
        {
            if (!target.ContainsKey(kv.Key))
            {
                target[kv.Key] = kv.Value;
            }
        }
    }

    private static Dictionary<string, AnimationClip> SearchProjectLocomotionClips()
    {
        Dictionary<string, AnimationClip> map = new Dictionary<string, AnimationClip>(StringComparer.OrdinalIgnoreCase);
        string[] searchRoots = { "Assets/Game/Art", "Assets/Scenes", "Assets/Resources", "Assets/Avatar" };
        string[] guids = AssetDatabase.FindAssets("t:AnimationClip", searchRoots);
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip == null)
            {
                continue;
            }

            string key = clip.name.ToLowerInvariant();
            bool looksLikeLocomotion = key.Contains("idle") ||
                                       key.Contains("walk") ||
                                       key.Contains("jog") ||
                                       key.Contains("run") ||
                                       key.Contains("sprint") ||
                                       key.Contains("talk") ||
                                       key.Contains("speak");
            if (!looksLikeLocomotion)
            {
                continue;
            }

            map[clip.name] = clip;
        }

        return map;
    }

    private static void TryInvokeEditorLocomotionBuilder()
    {
        Type builderType = Type.GetType("SampleSceneLocomotionControllerBuilder");
        if (builderType == null)
        {
            System.Reflection.Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                System.Reflection.Assembly assembly = assemblies[i];
                if (assembly == null)
                {
                    continue;
                }

                builderType = assembly.GetType("SampleSceneLocomotionControllerBuilder");
                if (builderType != null)
                {
                    break;
                }
            }
        }

        if (builderType == null)
        {
            return;
        }

        System.Reflection.MethodInfo buildMethod = builderType.GetMethod(
            "BuildController",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        buildMethod?.Invoke(null, null);
    }
#endif

    private static AnimationClip ResolveClip(Dictionary<string, AnimationClip> clips, params string[] candidates)
    {
        for (int i = 0; i < candidates.Length; i++)
        {
            string candidate = candidates[i];
            if (clips.TryGetValue(candidate, out AnimationClip clip) && clip != null)
            {
                return clip;
            }
        }

        return null;
    }

    private static AnimationClip ResolveClipByKeywords(
        Dictionary<string, AnimationClip> clips,
        string[] includeKeywords,
        string[] excludeKeywords)
    {
        AnimationClip best = null;
        int bestScore = int.MinValue;
        foreach (KeyValuePair<string, AnimationClip> pair in clips)
        {
            AnimationClip clip = pair.Value;
            if (clip == null)
            {
                continue;
            }

            string key = pair.Key.ToLowerInvariant();
            int includeCount = 0;
            for (int i = 0; i < includeKeywords.Length; i++)
            {
                string include = includeKeywords[i];
                if (!string.IsNullOrWhiteSpace(include) && key.Contains(include))
                {
                    includeCount++;
                }
            }

            if (includeCount == 0)
            {
                continue;
            }

            bool excluded = false;
            for (int i = 0; i < excludeKeywords.Length; i++)
            {
                string exclude = excludeKeywords[i];
                if (!string.IsNullOrWhiteSpace(exclude) && key.Contains(exclude))
                {
                    excluded = true;
                    break;
                }
            }

            if (excluded)
            {
                continue;
            }

            int score = includeCount * 100;
            if (key.Contains("__preview__"))
            {
                score -= 8;
            }

            if (score > bestScore)
            {
                bestScore = score;
                best = clip;
            }
        }

        return best;
    }

    private static AnimationClip ResolveAnyUsableClip(Dictionary<string, AnimationClip> clips)
    {
        AnimationClip fallback = null;
        foreach (KeyValuePair<string, AnimationClip> pair in clips)
        {
            AnimationClip clip = pair.Value;
            if (clip == null)
            {
                continue;
            }

            if (!pair.Key.Contains("__preview__", StringComparison.OrdinalIgnoreCase))
            {
                return clip;
            }

            fallback ??= clip;
        }

        return fallback;
    }

    private void SyncFallbackPoseState(bool useFallback)
    {
        if (fallbackIdlePose == null)
        {
            return;
        }

        if (useFallback)
        {
            fallbackIdlePose.SetNeutralMode(ShouldUseNeutralFallbackPose());
        }

        fallbackIdlePose.enabled = useFallback;
    }

    private bool ShouldUseNeutralFallbackPose()
    {
        if (playerMover != null)
        {
            return false;
        }

        if (body == null || body.isKinematic)
        {
            return true;
        }

        const RigidbodyConstraints horizontalFreeze = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
        bool horizontalLocked = (body.constraints & horizontalFreeze) == horizontalFreeze;
        return horizontalLocked;
    }

    private static Animator ResolvePrimaryAnimator(Animator[] candidates, Transform preferredRoot)
    {
        Animator best = null;
        int bestScore = int.MinValue;
        for (int i = 0; i < candidates.Length; i++)
        {
            Animator candidate = candidates[i];
            if (candidate == null)
            {
                continue;
            }

            int score = ScoreAnimator(candidate, preferredRoot);
            if (score <= bestScore)
            {
                continue;
            }

            bestScore = score;
            best = candidate;
        }

        return best;
    }

    private static int ScoreAnimator(Animator animator, Transform preferredRoot)
    {
        if (animator == null)
        {
            return int.MinValue;
        }

        int score = 0;
        if (animator.transform == preferredRoot)
        {
            score += 1200;
        }

        if (animator.avatar != null && animator.avatar.isValid)
        {
            score += animator.isHuman ? 420 : 160;
        }

        if (animator.runtimeAnimatorController != null)
        {
            score += 120;
        }

        SkinnedMeshRenderer[] skinnedRenderers = animator.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        score += Mathf.Min(skinnedRenderers.Length, 64) * 12;

        int depthFromRoot = ResolveDepthFromRoot(preferredRoot, animator.transform);
        if (depthFromRoot >= 0)
        {
            score += Mathf.Max(0, 80 - (depthFromRoot * 7));
        }

        if (!animator.gameObject.activeInHierarchy)
        {
            score -= 80;
        }

        return score;
    }

    private static int ResolveDepthFromRoot(Transform root, Transform target)
    {
        if (root == null || target == null)
        {
            return -1;
        }

        int depth = 0;
        Transform walker = target;
        while (walker != null)
        {
            if (walker == root)
            {
                return depth;
            }

            depth++;
            walker = walker.parent;
        }

        return -1;
    }
}
