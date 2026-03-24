using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class InfectionNode : MonoBehaviour
{
    [Serializable]
    public class StagePayload
    {
        public InfectionStage stage = InfectionStage.Dormant;
        public GameObject[] activateObjects = Array.Empty<GameObject>();
        public GameObject[] deactivateObjects = Array.Empty<GameObject>();

        [Header("Visuals")]
        public Light[] lights = Array.Empty<Light>();
        public float lightIntensityMultiplier = 1f;
        public Renderer[] emissiveRenderers = Array.Empty<Renderer>();
        public Color emissiveColor = new Color(0.5f, 0.92f, 0.68f, 1f);
        public float emissiveIntensity = 0f;
        public Volume[] volumes = Array.Empty<Volume>();
        [Range(0f, 1f)] public float volumeWeight;

        [Header("Narrative")]
        [TextArea(2, 4)] public string narrativeOverride;
        public NarrativeZone[] revealNarrativeZones = Array.Empty<NarrativeZone>();
    }

    [SerializeField] private string nodeId = "district_node";
    [SerializeField] private InfectionStage initialStage = InfectionStage.Dormant;
    [SerializeField] private bool applyOnAwake = true;
    [SerializeField] private bool logTransitions;
    [SerializeField] private bool routeNarrativeToUI = true;
    [SerializeField] private float narrativeMessageDuration = 4f;
    [SerializeField] private StagePayload[] stagePayloads = Array.Empty<StagePayload>();

    private readonly Dictionary<Light, float> lightBaselineIntensity = new Dictionary<Light, float>();
    private readonly Dictionary<Renderer, Color> rendererBaselineEmission = new Dictionary<Renderer, Color>();
    private readonly Dictionary<Renderer, MaterialPropertyBlock> rendererPropertyBlocks = new Dictionary<Renderer, MaterialPropertyBlock>();
    private readonly HashSet<Volume> configuredVolumes = new HashSet<Volume>();

    private InfectionStage currentStage;
    private bool initialized;

    public event Action<InfectionNode, InfectionStage> StageChanged;

    public string NodeId => nodeId;
    public InfectionStage InitialStage => initialStage;
    public InfectionStage CurrentStage => currentStage;

    private void Awake()
    {
        InitializeIfNeeded();

        if (applyOnAwake)
        {
            SetStage(initialStage, true);
        }
        else
        {
            currentStage = initialStage;
        }
    }

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(nodeId))
        {
            nodeId = gameObject.name;
        }
    }

    public void Configure(string newNodeId, InfectionStage newInitialStage, StagePayload[] payloads)
    {
        nodeId = string.IsNullOrWhiteSpace(newNodeId) ? gameObject.name : newNodeId;
        initialStage = newInitialStage;
        stagePayloads = payloads ?? Array.Empty<StagePayload>();
        initialized = false;
        InitializeIfNeeded();
        SetStage(initialStage, true);
    }

    public void SetStage(InfectionStage stage, bool force = false)
    {
        InitializeIfNeeded();

        if (!force && stage == currentStage)
        {
            return;
        }

        currentStage = stage;
        ApplyStage(stage);
        StageChanged?.Invoke(this, stage);

        if (logTransitions)
        {
            Debug.Log($"InfectionNode {nodeId} -> {stage}");
        }
    }

    public bool TryAdvanceStage()
    {
        if (currentStage >= InfectionStage.Overrun)
        {
            return false;
        }

        SetStage(currentStage + 1);
        return true;
    }

    private void InitializeIfNeeded()
    {
        if (initialized)
        {
            return;
        }

        lightBaselineIntensity.Clear();
        rendererBaselineEmission.Clear();
        rendererPropertyBlocks.Clear();
        configuredVolumes.Clear();

        for (int i = 0; i < stagePayloads.Length; i++)
        {
            StagePayload payload = stagePayloads[i];
            if (payload == null)
            {
                continue;
            }

            for (int lightIndex = 0; lightIndex < payload.lights.Length; lightIndex++)
            {
                Light lightComponent = payload.lights[lightIndex];
                if (lightComponent != null && !lightBaselineIntensity.ContainsKey(lightComponent))
                {
                    lightBaselineIntensity.Add(lightComponent, lightComponent.intensity);
                }
            }

            for (int rendererIndex = 0; rendererIndex < payload.emissiveRenderers.Length; rendererIndex++)
            {
                Renderer rendererComponent = payload.emissiveRenderers[rendererIndex];
                if (rendererComponent == null || rendererBaselineEmission.ContainsKey(rendererComponent))
                {
                    continue;
                }

                Color emission = Color.black;
                if (rendererComponent.sharedMaterial != null && rendererComponent.sharedMaterial.HasProperty("_EmissionColor"))
                {
                    emission = rendererComponent.sharedMaterial.GetColor("_EmissionColor");
                }

                rendererBaselineEmission.Add(rendererComponent, emission);
                rendererPropertyBlocks.Add(rendererComponent, new MaterialPropertyBlock());
            }

            for (int volumeIndex = 0; volumeIndex < payload.volumes.Length; volumeIndex++)
            {
                Volume volume = payload.volumes[volumeIndex];
                if (volume != null)
                {
                    configuredVolumes.Add(volume);
                }
            }
        }

        initialized = true;
    }

    private void ApplyStage(InfectionStage stage)
    {
        ResetVisualsToBaseline();

        StagePayload payload = GetPayload(stage);
        if (payload == null)
        {
            return;
        }

        SetActiveState(payload.deactivateObjects, false);
        SetActiveState(payload.activateObjects, true);

        for (int i = 0; i < payload.lights.Length; i++)
        {
            Light lightComponent = payload.lights[i];
            if (lightComponent == null)
            {
                continue;
            }

            if (!lightBaselineIntensity.TryGetValue(lightComponent, out float baseIntensity))
            {
                baseIntensity = lightComponent.intensity;
            }

            lightComponent.intensity = baseIntensity * payload.lightIntensityMultiplier;
        }

        for (int i = 0; i < payload.emissiveRenderers.Length; i++)
        {
            Renderer rendererComponent = payload.emissiveRenderers[i];
            if (rendererComponent == null)
            {
                continue;
            }

            SetRendererEmission(rendererComponent, payload.emissiveColor * payload.emissiveIntensity);
        }

        for (int i = 0; i < payload.volumes.Length; i++)
        {
            Volume volume = payload.volumes[i];
            if (volume != null)
            {
                volume.weight = payload.volumeWeight;
            }
        }

        for (int i = 0; i < payload.revealNarrativeZones.Length; i++)
        {
            NarrativeZone zone = payload.revealNarrativeZones[i];
            if (zone != null)
            {
                zone.Reveal();
            }
        }

        if (!string.IsNullOrWhiteSpace(payload.narrativeOverride))
        {
            if (Application.isPlaying && routeNarrativeToUI)
            {
                HolstinFeedback.ShowMessage(payload.narrativeOverride, narrativeMessageDuration);
            }
            else
            {
                Debug.Log($"Infection narrative ({nodeId}/{stage}): {payload.narrativeOverride}");
            }
        }
    }

    private void ResetVisualsToBaseline()
    {
        foreach (KeyValuePair<Light, float> pair in lightBaselineIntensity)
        {
            if (pair.Key != null)
            {
                pair.Key.intensity = pair.Value;
            }
        }

        foreach (KeyValuePair<Renderer, Color> pair in rendererBaselineEmission)
        {
            if (pair.Key != null)
            {
                SetRendererEmission(pair.Key, pair.Value);
            }
        }

        foreach (Volume volume in configuredVolumes)
        {
            if (volume != null)
            {
                volume.weight = 0f;
            }
        }
    }

    private StagePayload GetPayload(InfectionStage stage)
    {
        for (int i = 0; i < stagePayloads.Length; i++)
        {
            StagePayload payload = stagePayloads[i];
            if (payload != null && payload.stage == stage)
            {
                return payload;
            }
        }

        return null;
    }

    private void SetRendererEmission(Renderer rendererComponent, Color emission)
    {
        if (!rendererPropertyBlocks.TryGetValue(rendererComponent, out MaterialPropertyBlock propertyBlock))
        {
            propertyBlock = new MaterialPropertyBlock();
            rendererPropertyBlocks.Add(rendererComponent, propertyBlock);
        }

        rendererComponent.GetPropertyBlock(propertyBlock);
        propertyBlock.SetColor("_EmissionColor", emission);
        rendererComponent.SetPropertyBlock(propertyBlock);
    }

    private static void SetActiveState(GameObject[] targets, bool active)
    {
        for (int i = 0; i < targets.Length; i++)
        {
            if (targets[i] != null)
            {
                targets[i].SetActive(active);
            }
        }
    }
}
