using UnityEngine;

#if ENABLE_INPUT_SYSTEM && !UNITY_DISABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[DisallowMultipleComponent]
public class RuntimeGraphicsQualityController : MonoBehaviour
{
    public enum RenderQualityOption
    {
        Low = 0,
        Medium = 1,
        High = 2,
        Ultra = 3
    }

    public enum TextureQualityOption
    {
        K2 = 0,
        K4 = 1
    }

    private const string RenderQualityPrefKey = "graphics.render_quality";
    private const string TextureQualityPrefKey = "graphics.texture_quality";

    [SerializeField] private RenderQualityOption defaultRenderQuality = RenderQualityOption.High;
    [SerializeField] private TextureQualityOption defaultTextureQuality = TextureQualityOption.K2;
    [SerializeField] private bool verboseLogging = true;

    private RenderQualityOption currentRenderQuality;
    private TextureQualityOption currentQuality;

    private void Awake()
    {
        int savedRenderValue = PlayerPrefs.GetInt(RenderQualityPrefKey, (int)defaultRenderQuality);
        RenderQualityOption renderQuality = ResolveRenderQuality(savedRenderValue);
        ApplyRenderQuality(renderQuality, savePreference: false);

        int savedValue = PlayerPrefs.GetInt(TextureQualityPrefKey, (int)defaultTextureQuality);
        TextureQualityOption resolved = savedValue == (int)TextureQualityOption.K4
            ? TextureQualityOption.K4
            : TextureQualityOption.K2;

        ApplyTextureQuality(resolved, savePreference: false);
    }

    private void Update()
    {
        if (Set2KPressed())
        {
            SetTextureQuality2K();
            return;
        }

        if (Set4KPressed())
        {
            SetTextureQuality4K();
            return;
        }

        if (SetRenderLowPressed())
        {
            SetRenderQuality(RenderQualityOption.Low);
            return;
        }

        if (SetRenderHighPressed())
        {
            SetRenderQuality(RenderQualityOption.High);
        }
    }

    public void SetTextureQuality2K()
    {
        ApplyTextureQuality(TextureQualityOption.K2, savePreference: true);
    }

    public void SetTextureQuality4K()
    {
        ApplyTextureQuality(TextureQualityOption.K4, savePreference: true);
    }

    public TextureQualityOption GetCurrentQuality()
    {
        return currentQuality;
    }

    public RenderQualityOption GetCurrentRenderQuality()
    {
        return currentRenderQuality;
    }

    public void SetRenderQuality(RenderQualityOption quality)
    {
        ApplyRenderQuality(quality, savePreference: true);
    }

    private void ApplyTextureQuality(TextureQualityOption quality, bool savePreference)
    {
        currentQuality = quality;
        QualitySettings.globalTextureMipmapLimit = quality == TextureQualityOption.K2 ? 1 : 0;

        if (savePreference)
        {
            PlayerPrefs.SetInt(TextureQualityPrefKey, (int)quality);
            PlayerPrefs.Save();
        }

        if (verboseLogging)
        {
            string label = quality == TextureQualityOption.K2 ? "2K" : "4K";
            Debug.Log($"RuntimeGraphicsQualityController: Texture profile set to {label}. F6=2K, F7=4K");
        }
    }

    private void ApplyRenderQuality(RenderQualityOption quality, bool savePreference)
    {
        currentRenderQuality = quality;

        int qualityLevel = ResolveQualityLevelIndex(quality);
        if (qualityLevel >= 0 && qualityLevel < QualitySettings.names.Length)
        {
            QualitySettings.SetQualityLevel(qualityLevel, applyExpensiveChanges: true);
        }

        if (savePreference)
        {
            PlayerPrefs.SetInt(RenderQualityPrefKey, (int)quality);
            PlayerPrefs.Save();
        }

        if (verboseLogging)
        {
            Debug.Log($"RuntimeGraphicsQualityController: Render quality set to {quality}.");
        }
    }

    private static RenderQualityOption ResolveRenderQuality(int value)
    {
        return value switch
        {
            (int)RenderQualityOption.Low => RenderQualityOption.Low,
            (int)RenderQualityOption.Medium => RenderQualityOption.Medium,
            (int)RenderQualityOption.High => RenderQualityOption.High,
            (int)RenderQualityOption.Ultra => RenderQualityOption.Ultra,
            _ => RenderQualityOption.High
        };
    }

    private static int ResolveQualityLevelIndex(RenderQualityOption quality)
    {
        string[] names = QualitySettings.names;
        if (names == null || names.Length == 0)
        {
            return 0;
        }

        if (names.Length == 1)
        {
            return 0;
        }

        if (names.Length == 2)
        {
            // This project only defines Mobile (0) + PC (1). Keep anything above Low on PC.
            return quality == RenderQualityOption.Low ? 0 : 1;
        }

        int max = names.Length - 1;
        return quality switch
        {
            RenderQualityOption.Low => 0,
            RenderQualityOption.Medium => Mathf.Clamp(Mathf.RoundToInt(max * 0.34f), 0, max),
            RenderQualityOption.High => Mathf.Clamp(Mathf.RoundToInt(max * 0.67f), 0, max),
            RenderQualityOption.Ultra => max,
            _ => max
        };
    }

    private static bool Set2KPressed()
    {
#if ENABLE_INPUT_SYSTEM && !UNITY_DISABLE_INPUT_SYSTEM
        Keyboard keyboard = Keyboard.current;
        return keyboard != null && keyboard.f6Key.wasPressedThisFrame;
#else
        return false;
#endif
    }

    private static bool Set4KPressed()
    {
#if ENABLE_INPUT_SYSTEM && !UNITY_DISABLE_INPUT_SYSTEM
        Keyboard keyboard = Keyboard.current;
        return keyboard != null && keyboard.f7Key.wasPressedThisFrame;
#else
        return false;
#endif
    }

    private static bool SetRenderLowPressed()
    {
#if ENABLE_INPUT_SYSTEM && !UNITY_DISABLE_INPUT_SYSTEM
        Keyboard keyboard = Keyboard.current;
        return keyboard != null && keyboard.f8Key.wasPressedThisFrame;
#else
        return false;
#endif
    }

    private static bool SetRenderHighPressed()
    {
#if ENABLE_INPUT_SYSTEM && !UNITY_DISABLE_INPUT_SYSTEM
        Keyboard keyboard = Keyboard.current;
        return keyboard != null && keyboard.f9Key.wasPressedThisFrame;
#else
        return false;
#endif
    }
}
