using System;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Samples an imported creature locomotion clip locally while NavMeshAgent owns translation.
/// Root motion is stripped so visual animation cannot fight gameplay movement.
/// </summary>
[DisallowMultipleComponent]
public sealed class HorrorCreatureLocomotionDriver : MonoBehaviour
{
    [SerializeField] private string resourcePath = "ThirdParty/TheRake/TheRake";
    [SerializeField] private float referenceSpeed = 3.25f;
    [SerializeField] private float minimumAnimatedSpeed = 0.08f;

    private NavMeshAgent agent;
    private Damageable damageable;
    private CharacterStats stats;
    private GameObject visualRoot;
    private AnimationClip locomotionClip;
    private Vector3 baseLocalPosition;
    private Quaternion baseLocalRotation;
    private Vector3 baseLocalScale;
    private float clipTime;

    public bool HasLocomotionClip => locomotionClip != null;

    public void Configure(GameObject visual, string path)
    {
        visualRoot = visual;
        if (!string.IsNullOrWhiteSpace(path))
        {
            resourcePath = path;
        }

        Resolve();
    }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        damageable = GetComponent<Damageable>();
        stats = GetComponent<CharacterStats>();

        if (damageable != null)
        {
            damageable.Died += HandleDied;
        }

        if (stats != null)
        {
            stats.Revived += HandleRevived;
        }
    }

    private void OnDestroy()
    {
        if (damageable != null)
        {
            damageable.Died -= HandleDied;
        }

        if (stats != null)
        {
            stats.Revived -= HandleRevived;
        }
    }

    private void Start()
    {
        Resolve();
    }

    private void Update()
    {
        if (visualRoot == null || locomotionClip == null)
        {
            return;
        }

        float speed = 0f;
        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            speed = agent.velocity.magnitude;
        }

        if (speed < minimumAnimatedSpeed)
        {
            return;
        }

        float speedRatio = Mathf.Clamp(speed / Mathf.Max(0.1f, referenceSpeed), 0.55f, 1.65f);
        clipTime += Time.deltaTime * speedRatio;
        float sampleTime = locomotionClip.length > 0.001f ? clipTime % locomotionClip.length : 0f;

        locomotionClip.SampleAnimation(visualRoot, sampleTime);

        // Imported clips can contain root translation/rotation. NavMeshAgent remains the
        // authoritative locomotion source, so restore the calibrated visual root afterward.
        Transform t = visualRoot.transform;
        t.localPosition = baseLocalPosition;
        t.localRotation = baseLocalRotation;
        t.localScale = baseLocalScale;
    }

    private void HandleDied()
    {
        if (visualRoot != null)
        {
            visualRoot.SetActive(false);
        }

        HorrorCreatureRuntimeUtility.SetCompoundCollisionEnabled(gameObject, false);
    }

    private void HandleRevived()
    {
        if (visualRoot != null)
        {
            visualRoot.SetActive(true);
        }

        HorrorCreatureRuntimeUtility.SetCompoundCollisionEnabled(gameObject, true);
    }

    private void Resolve()
    {
        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }

        if (visualRoot == null)
        {
            return;
        }

        Transform t = visualRoot.transform;
        baseLocalPosition = t.localPosition;
        baseLocalRotation = t.localRotation;
        baseLocalScale = t.localScale;

        Animator[] animators = visualRoot.GetComponentsInChildren<Animator>(true);
        for (int i = 0; i < animators.Length; i++)
        {
            if (animators[i] != null)
            {
                animators[i].enabled = false;
            }
        }

        locomotionClip = FindBestLocomotionClip(Resources.LoadAll<AnimationClip>(resourcePath));
    }

    private static AnimationClip FindBestLocomotionClip(AnimationClip[] clips)
    {
        if (clips == null || clips.Length == 0)
        {
            return null;
        }

        string[] priorities = { "run", "sprint", "charge", "walk" };
        for (int p = 0; p < priorities.Length; p++)
        {
            for (int i = 0; i < clips.Length; i++)
            {
                AnimationClip candidate = clips[i];
                if (candidate != null &&
                    candidate.name.IndexOf(priorities[p], StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return candidate;
                }
            }
        }

        for (int i = 0; i < clips.Length; i++)
        {
            if (clips[i] != null)
            {
                return clips[i];
            }
        }

        return null;
    }
}
