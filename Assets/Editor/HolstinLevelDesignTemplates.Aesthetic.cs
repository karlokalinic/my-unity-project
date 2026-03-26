// ============================================================
// FILE: HolstinLevelDesignTemplates.Aesthetic.cs
// Production-style atmosphere and post-processing pass
// ============================================================
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public static partial class HolstinLevelDesignTemplates
{
    private static Material atmosphereFogMaterial;
    private static Material dustParticleMaterial;

    [MenuItem("Tools/Holstin Level Design Templates/Apply Production Aesthetic Pass", false, 50)]
    public static void ApplyProductionAestheticPassToCurrentScene()
    {
        ApplyProductionAestheticPassInternal();
        FinalizeScene("Production aesthetic pass applied.");
    }

    private static void ApplyProductionAestheticPassInternal()
    {
        EnsureSceneRootGroups();
        ResetMaterialCache();

        ApplyGlobalLookAndLighting();
        EnsureAtmosphereCards();
        EnsureAtmosphericDust();
        EnsureProductionVolume();
        EnsureTemplateMoodLights();
    }

    private static void ApplyGlobalLookAndLighting()
    {
        EnsureDirectionalLight();
        Light[] lights = Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < lights.Length; i++)
        {
            Light lightComponent = lights[i];
            if (lightComponent == null || lightComponent.type != LightType.Directional)
            {
                continue;
            }

            lightComponent.intensity = 0.85f;
            lightComponent.color = new Color(0.85f, 0.90f, 0.98f);
            lightComponent.shadows = LightShadows.Soft;
            lightComponent.shadowStrength = 0.7f;
            lightComponent.shadowBias = 0.07f;
            lightComponent.shadowNormalBias = 0.25f;
            lightComponent.transform.rotation = Quaternion.Euler(42f, -48f, 0f);
        }

        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = new Color(0.19f, 0.24f, 0.28f, 1f);
        RenderSettings.fogDensity = 0.018f;

        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.19f, 0.21f, 0.25f, 1f);
        RenderSettings.ambientEquatorColor = new Color(0.13f, 0.14f, 0.17f, 1f);
        RenderSettings.ambientGroundColor = new Color(0.07f, 0.08f, 0.10f, 1f);
        RenderSettings.ambientIntensity = 1.1f;
        RenderSettings.reflectionIntensity = 0.35f;
    }

    private static void EnsureAtmosphereCards()
    {
        EnsureSceneRootGroups();
        Transform atmosphereRoot = EnsureChildGroup(sceneLightingRoot, "VS_Atmosphere");
        Vector3 center = ResolveAtmosphereCenter();
        Vector3[] offsets =
        {
            new Vector3(-18f, 2.4f, -9f),
            new Vector3(-8f, 1.8f, 12f),
            new Vector3(9f, 2.6f, -14f),
            new Vector3(20f, 2.2f, 8f),
            new Vector3(4f, 1.9f, 20f),
            new Vector3(-22f, 2.1f, 5f)
        };

        Material fogMaterial = GetAtmosphereFogMaterial();
        for (int i = 0; i < offsets.Length; i++)
        {
            string cardName = $"FogCard_{i}";
            Transform existing = atmosphereRoot.Find(cardName);
            GameObject card = existing != null ? existing.gameObject : GameObject.CreatePrimitive(PrimitiveType.Quad);
            card.name = cardName;
            card.transform.SetParent(atmosphereRoot, false);
            card.transform.position = center + offsets[i];
            card.transform.rotation = Quaternion.Euler(90f - (i * 4f), 40f + (i * 20f), 0f);
            card.transform.localScale = new Vector3(17f + i * 2f, 9f + i, 1f);

            Renderer rendererComponent = card.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.sharedMaterial = fogMaterial;
                rendererComponent.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                rendererComponent.receiveShadows = false;
                rendererComponent.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
                rendererComponent.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
            }

            Collider colliderComponent = card.GetComponent<Collider>();
            if (colliderComponent != null)
            {
                Object.DestroyImmediate(colliderComponent);
            }
        }
    }

    private static void EnsureAtmosphericDust()
    {
        EnsureSceneRootGroups();
        Transform atmosphereRoot = EnsureChildGroup(sceneLightingRoot, "VS_Atmosphere");
        Transform dustTransform = atmosphereRoot.Find("AtmosphericDust");
        GameObject dustRoot = dustTransform != null ? dustTransform.gameObject : new GameObject("AtmosphericDust");
        dustRoot.transform.SetParent(atmosphereRoot, false);
        dustRoot.transform.position = ResolveAtmosphereCenter() + new Vector3(0f, 2.8f, 0f);

        ParticleSystem particleSystem = EnsureComponent<ParticleSystem>(dustRoot);
        ParticleSystem.MainModule main = particleSystem.main;
        main.loop = true;
        main.playOnAwake = true;
        main.duration = 12f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(7f, 15f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.05f, 0.28f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.22f);
        main.maxParticles = 900;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = 0f;

        ParticleSystem.EmissionModule emission = particleSystem.emission;
        emission.enabled = true;
        emission.rateOverTime = 46f;

        ParticleSystem.ShapeModule shape = particleSystem.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(80f, 20f, 80f);

        ParticleSystem.ColorOverLifetimeModule colorOverLifetime = particleSystem.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(new Color(0.72f, 0.78f, 0.82f), 0f),
                new GradientColorKey(new Color(0.50f, 0.56f, 0.60f), 1f)
            },
            new[]
            {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(0.2f, 0.18f),
                new GradientAlphaKey(0.2f, 0.8f),
                new GradientAlphaKey(0f, 1f)
            });
        colorOverLifetime.color = gradient;

        ParticleSystem.NoiseModule noise = particleSystem.noise;
        noise.enabled = true;
        noise.strength = new ParticleSystem.MinMaxCurve(0.2f, 0.45f);
        noise.frequency = 0.28f;
        noise.scrollSpeed = 0.15f;

        ParticleSystemRenderer rendererComponent = EnsureComponent<ParticleSystemRenderer>(dustRoot);
        rendererComponent.renderMode = ParticleSystemRenderMode.Billboard;
        rendererComponent.alignment = ParticleSystemRenderSpace.View;
        rendererComponent.material = GetDustParticleMaterial();
        rendererComponent.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        rendererComponent.receiveShadows = false;

        if (!particleSystem.isPlaying)
        {
            particleSystem.Play();
        }
    }

    private static void EnsureProductionVolume()
    {
        EnsureSceneRootGroups();
        Transform volumeTransform = EnsureChildGroup(sceneLightingRoot, "VS_ProductionVolume");
        Volume volume = EnsureComponent<Volume>(volumeTransform.gameObject);
        volume.isGlobal = true;
        volume.priority = 25f;
        volume.weight = 1f;

        if (volume.profile == null || !volume.profile.name.StartsWith("VS_ProductionProfile_"))
        {
            VolumeProfile profile = ScriptableObject.CreateInstance<VolumeProfile>();
            profile.name = "VS_ProductionProfile_" + SceneManager.GetActiveScene().name;
            volume.profile = profile;
        }

        VolumeProfile activeProfile = volume.profile;
        ColorAdjustments colorAdjustments = EnsureVolumeOverride<ColorAdjustments>(activeProfile);
        colorAdjustments.postExposure.overrideState = true;
        colorAdjustments.postExposure.value = -0.24f;
        colorAdjustments.contrast.overrideState = true;
        colorAdjustments.contrast.value = 23f;
        colorAdjustments.saturation.overrideState = true;
        colorAdjustments.saturation.value = -18f;
        colorAdjustments.colorFilter.overrideState = true;
        colorAdjustments.colorFilter.value = new Color(0.90f, 0.96f, 1f, 1f);

        WhiteBalance whiteBalance = EnsureVolumeOverride<WhiteBalance>(activeProfile);
        whiteBalance.temperature.overrideState = true;
        whiteBalance.temperature.value = -7f;
        whiteBalance.tint.overrideState = true;
        whiteBalance.tint.value = -3f;

        LiftGammaGain liftGammaGain = EnsureVolumeOverride<LiftGammaGain>(activeProfile);
        liftGammaGain.lift.overrideState = true;
        liftGammaGain.lift.value = new Vector4(-0.04f, -0.03f, -0.01f, 0f);
        liftGammaGain.gamma.overrideState = true;
        liftGammaGain.gamma.value = new Vector4(0.98f, 0.98f, 0.98f, 0f);
        liftGammaGain.gain.overrideState = true;
        liftGammaGain.gain.value = new Vector4(0.01f, 0.00f, -0.02f, 0f);

        Bloom bloom = EnsureVolumeOverride<Bloom>(activeProfile);
        bloom.threshold.overrideState = true;
        bloom.threshold.value = 0.88f;
        bloom.intensity.overrideState = true;
        bloom.intensity.value = 0.52f;
        bloom.scatter.overrideState = true;
        bloom.scatter.value = 0.76f;
        bloom.tint.overrideState = true;
        bloom.tint.value = new Color(0.88f, 0.94f, 1f, 1f);

        Vignette vignette = EnsureVolumeOverride<Vignette>(activeProfile);
        vignette.intensity.overrideState = true;
        vignette.intensity.value = 0.34f;
        vignette.smoothness.overrideState = true;
        vignette.smoothness.value = 0.74f;
        vignette.rounded.overrideState = true;
        vignette.rounded.value = false;

        FilmGrain filmGrain = EnsureVolumeOverride<FilmGrain>(activeProfile);
        filmGrain.type.overrideState = true;
        filmGrain.type.value = FilmGrainLookup.Thin1;
        filmGrain.intensity.overrideState = true;
        filmGrain.intensity.value = 0.34f;
        filmGrain.response.overrideState = true;
        filmGrain.response.value = 0.84f;

        ChromaticAberration chromaticAberration = EnsureVolumeOverride<ChromaticAberration>(activeProfile);
        chromaticAberration.intensity.overrideState = true;
        chromaticAberration.intensity.value = 0.07f;
    }

    private static void EnsureTemplateMoodLights()
    {
        EnsureSceneRootGroups();
        Transform lightingRoot = EnsureChildGroup(sceneLightingRoot, "VS_MoodPracticals");

        Vector3 exterior = ResolveTemplateRootPosition("Template_Exterior_FogCourtyard", new Vector3(-18f, 0f, -6f));
        Vector3 interior = ResolveTemplateRootPosition("Template_Interior_BoardingHouse", new Vector3(14f, 0f, -2f));
        Vector3 underpass = ResolveTemplateRootPosition("Template_Underpass_Catacombs", new Vector3(12f, -5.5f, 10f));

        EnsureMoodPointLight(lightingRoot, "MoodLight_Exterior", exterior + new Vector3(9f, 4.2f, 6f), new Color(0.58f, 0.72f, 0.86f), 12f, 3.2f);
        EnsureMoodPointLight(lightingRoot, "MoodLight_Interior", interior + new Vector3(-2f, 5f, 0f), new Color(1f, 0.72f, 0.50f), 10f, 3.6f);
        EnsureMoodPointLight(lightingRoot, "MoodLight_Underpass", underpass + new Vector3(4f, 2f, 0f), new Color(0.48f, 0.66f, 0.86f), 11f, 3.8f);
    }

    private static void EnsureMoodPointLight(Transform root, string name, Vector3 worldPosition, Color color, float range, float intensity)
    {
        Transform existing = root.Find(name);
        GameObject lightObject = existing != null ? existing.gameObject : new GameObject(name);
        lightObject.transform.SetParent(root, false);
        lightObject.transform.position = worldPosition;

        Light lightComponent = EnsureComponent<Light>(lightObject);
        lightComponent.type = LightType.Point;
        lightComponent.range = range;
        lightComponent.intensity = intensity;
        lightComponent.color = color;
        lightComponent.shadows = LightShadows.None;

        TutorialAmbientMotion motion = EnsureComponent<TutorialAmbientMotion>(lightObject);
        SerializedObject so = new SerializedObject(motion);
        SerializedProperty bobAmplitude = so.FindProperty("bobAmplitude");
        if (bobAmplitude != null) bobAmplitude.vector3Value = new Vector3(0f, 0.02f, 0f);
        SerializedProperty bobSpeed = so.FindProperty("bobSpeed");
        if (bobSpeed != null) bobSpeed.floatValue = 1.08f;
        SerializedProperty pulseAmplitude = so.FindProperty("pulseAmplitude");
        if (pulseAmplitude != null) pulseAmplitude.floatValue = 0.22f;
        SerializedProperty pulseSpeed = so.FindProperty("pulseSpeed");
        if (pulseSpeed != null) pulseSpeed.floatValue = 1.35f;
        SerializedProperty targetLight = so.FindProperty("targetLight");
        if (targetLight != null) targetLight.objectReferenceValue = lightComponent;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static Vector3 ResolveAtmosphereCenter()
    {
        Vector3 exterior = ResolveTemplateRootPosition("Template_Exterior_FogCourtyard", new Vector3(-18f, 0f, -6f));
        Vector3 interior = ResolveTemplateRootPosition("Template_Interior_BoardingHouse", new Vector3(14f, 0f, -2f));
        Vector3 underpass = ResolveTemplateRootPosition("Template_Underpass_Catacombs", new Vector3(12f, -5.5f, 10f));
        return (exterior + interior + underpass) / 3f;
    }

    private static Vector3 ResolveTemplateRootPosition(string rootName, Vector3 fallback)
    {
        GameObject root = FindSceneObjectByName(rootName);
        return root != null ? root.transform.position : fallback;
    }

    private static Material GetAtmosphereFogMaterial()
    {
        if (atmosphereFogMaterial != null)
        {
            return atmosphereFogMaterial;
        }

        Shader shader = Shader.Find("Universal Render Pipeline/Unlit")
                     ?? Shader.Find("Unlit/Color")
                     ?? Shader.Find("Sprites/Default");
        atmosphereFogMaterial = new Material(shader) { name = "HolstinTemplate_AtmosphereFog" };
        SetMaterialColor(atmosphereFogMaterial, new Color(0.47f, 0.56f, 0.62f, 0.15f));
        if (atmosphereFogMaterial.HasProperty("_Surface")) atmosphereFogMaterial.SetFloat("_Surface", 1f);
        if (atmosphereFogMaterial.HasProperty("_Blend")) atmosphereFogMaterial.SetFloat("_Blend", 0f);
        if (atmosphereFogMaterial.HasProperty("_ZWrite")) atmosphereFogMaterial.SetFloat("_ZWrite", 0f);
        if (atmosphereFogMaterial.HasProperty("_Cull")) atmosphereFogMaterial.SetFloat("_Cull", 0f);
        atmosphereFogMaterial.enableInstancing = true;
        return atmosphereFogMaterial;
    }

    private static Material GetDustParticleMaterial()
    {
        if (dustParticleMaterial != null)
        {
            return dustParticleMaterial;
        }

        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit")
                     ?? Shader.Find("Particles/Standard Unlit")
                     ?? Shader.Find("Sprites/Default");
        dustParticleMaterial = new Material(shader) { name = "HolstinTemplate_DustParticle" };
        SetMaterialColor(dustParticleMaterial, new Color(0.86f, 0.88f, 0.90f, 0.12f));
        if (dustParticleMaterial.HasProperty("_Surface")) dustParticleMaterial.SetFloat("_Surface", 1f);
        if (dustParticleMaterial.HasProperty("_ZWrite")) dustParticleMaterial.SetFloat("_ZWrite", 0f);
        dustParticleMaterial.enableInstancing = true;
        return dustParticleMaterial;
    }

    private static T EnsureVolumeOverride<T>(VolumeProfile profile) where T : VolumeComponent
    {
        if (!profile.TryGet(out T component))
        {
            component = profile.Add<T>(true);
        }

        component.active = true;
        return component;
    }
}
