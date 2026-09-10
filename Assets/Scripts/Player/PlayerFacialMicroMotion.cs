using System;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(160)]
[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerHumanoidVisualDriver))]
public sealed class PlayerFacialMicroMotion : MonoBehaviour
{
    private struct BlinkChannel
    {
        public SkinnedMeshRenderer renderer;
        public int blendShapeIndex;
        public float baseWeight;
    }

    [Header("References")]
    [SerializeField] private PlayerHumanoidVisualDriver visualDriver;
    [SerializeField] private DeathRagdollController ragdollController;

    [Header("Blink Timing")]
    [SerializeField] private Vector2 blinkIntervalRange = new Vector2(2.4f, 5.8f);
    [SerializeField] private float blinkDuration = 0.115f;
    [SerializeField] [Range(0f, 1f)] private float doubleBlinkChance = 0.12f;
    [SerializeField] private float doubleBlinkDelay = 0.16f;

    [Header("Death / Recovery")]
    [SerializeField] private float deathEyeCloseSpeed = 3.4f;
    [SerializeField] private float reviveEyeOpenSpeed = 1.8f;

    private readonly List<BlinkChannel> blinkChannels = new List<BlinkChannel>(4);
    private float nextBlinkAt;
    private float blinkElapsed = -1f;
    private float doubleBlinkAt = -1f;
    private float deathClosure;
    private bool resolved;

    public bool HasBlinkShapes => blinkChannels.Count > 0;

    private void Awake()
    {
        if (visualDriver == null)
        {
            visualDriver = GetComponent<PlayerHumanoidVisualDriver>();
        }
        if (ragdollController == null)
        {
            ragdollController = GetComponent<DeathRagdollController>();
        }
    }

    private void Start()
    {
        ResolveBlendShapes();
        ScheduleNextBlink();
    }

    private void LateUpdate()
    {
        if (!resolved)
        {
            ResolveBlendShapes();
            if (!resolved)
            {
                return;
            }
        }

        bool dead = ragdollController != null && ragdollController.RagdollActive;
        float closureSpeed = dead ? deathEyeCloseSpeed : reviveEyeOpenSpeed;
        deathClosure = Mathf.MoveTowards(
            deathClosure,
            dead ? 1f : 0f,
            Mathf.Max(0.01f, closureSpeed) * Time.deltaTime);

        float blinkWeight = dead ? 0f : UpdateBlinkEnvelope();
        float finalClosure = Mathf.Max(deathClosure, blinkWeight);
        ApplyBlinkWeight(finalClosure * 100f);
    }

    private void OnDisable()
    {
        RestoreBaseWeights();
    }

    public void ResolveBlendShapes()
    {
        RestoreBaseWeights();
        blinkChannels.Clear();
        resolved = false;

        Transform visualRoot = transform.Find("StoreModelVisual");
        if (visualRoot == null)
        {
            return;
        }

        SkinnedMeshRenderer[] renderers = visualRoot.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        for (int rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
        {
            SkinnedMeshRenderer renderer = renderers[rendererIndex];
            Mesh mesh = renderer != null ? renderer.sharedMesh : null;
            if (mesh == null)
            {
                continue;
            }

            for (int blendIndex = 0; blendIndex < mesh.blendShapeCount; blendIndex++)
            {
                string normalized = NormalizeBlendShapeName(mesh.GetBlendShapeName(blendIndex));
                if (!LooksLikeBlink(normalized))
                {
                    continue;
                }

                blinkChannels.Add(new BlinkChannel
                {
                    renderer = renderer,
                    blendShapeIndex = blendIndex,
                    baseWeight = renderer.GetBlendShapeWeight(blendIndex)
                });
            }
        }

        // No compatible facial morph is a valid asset configuration. Resolve once and become a no-op.
        resolved = true;
    }

    private float UpdateBlinkEnvelope()
    {
        float now = Time.time;
        if (blinkElapsed < 0f)
        {
            if (doubleBlinkAt > 0f && now >= doubleBlinkAt)
            {
                doubleBlinkAt = -1f;
                blinkElapsed = 0f;
            }
            else if (now >= nextBlinkAt)
            {
                blinkElapsed = 0f;
                if (UnityEngine.Random.value < doubleBlinkChance)
                {
                    doubleBlinkAt = now + Mathf.Max(0.05f, doubleBlinkDelay);
                }
                ScheduleNextBlink();
            }
            else
            {
                return 0f;
            }
        }

        float duration = Mathf.Max(0.05f, blinkDuration);
        blinkElapsed += Time.deltaTime;
        float t = Mathf.Clamp01(blinkElapsed / duration);

        float envelope;
        if (t < 0.38f)
        {
            envelope = Smooth01(t / 0.38f);
        }
        else
        {
            envelope = 1f - Smooth01((t - 0.38f) / 0.62f);
        }

        if (t >= 1f)
        {
            blinkElapsed = -1f;
        }

        return envelope;
    }

    private void ScheduleNextBlink()
    {
        float min = Mathf.Max(0.5f, Mathf.Min(blinkIntervalRange.x, blinkIntervalRange.y));
        float max = Mathf.Max(min + 0.05f, Mathf.Max(blinkIntervalRange.x, blinkIntervalRange.y));
        nextBlinkAt = Time.time + UnityEngine.Random.Range(min, max);
    }

    private void ApplyBlinkWeight(float closureWeight)
    {
        closureWeight = Mathf.Clamp(closureWeight, 0f, 100f);
        for (int i = 0; i < blinkChannels.Count; i++)
        {
            BlinkChannel channel = blinkChannels[i];
            if (channel.renderer == null || channel.blendShapeIndex < 0)
            {
                continue;
            }

            float weight = Mathf.Lerp(channel.baseWeight, 100f, closureWeight / 100f);
            channel.renderer.SetBlendShapeWeight(channel.blendShapeIndex, weight);
        }
    }

    private void RestoreBaseWeights()
    {
        for (int i = 0; i < blinkChannels.Count; i++)
        {
            BlinkChannel channel = blinkChannels[i];
            if (channel.renderer != null && channel.blendShapeIndex >= 0)
            {
                channel.renderer.SetBlendShapeWeight(channel.blendShapeIndex, channel.baseWeight);
            }
        }
    }

    private static bool LooksLikeBlink(string normalized)
    {
        if (string.IsNullOrEmpty(normalized))
        {
            return false;
        }

        return normalized.Contains("blink", StringComparison.Ordinal) ||
               normalized.Contains("eyeclose", StringComparison.Ordinal) ||
               normalized.Contains("eyelidclose", StringComparison.Ordinal);
    }

    private static string NormalizeBlendShapeName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        char[] buffer = new char[value.Length];
        int count = 0;
        for (int i = 0; i < value.Length; i++)
        {
            char c = value[i];
            if (!char.IsLetterOrDigit(c))
            {
                continue;
            }

            buffer[count++] = char.ToLowerInvariant(c);
        }

        return new string(buffer, 0, count);
    }

    private static float Smooth01(float value)
    {
        value = Mathf.Clamp01(value);
        return value * value * (3f - 2f * value);
    }
}
