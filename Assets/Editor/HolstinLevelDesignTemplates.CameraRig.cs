
// ============================================================
// FILE: HolstinLevelDesignTemplates.CameraRig.cs
// Player rig, camera, interaction UI
// ============================================================
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public static partial class HolstinLevelDesignTemplates
{
    private static void EnsureCoreRig(Vector3 spawnPosition)
    {
        EnsureSceneRootGroups();

        PlayerMover existingPlayer = Object.FindAnyObjectByType<PlayerMover>();
        GameObject player = existingPlayer != null ? existingPlayer.gameObject : GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.transform.position = spawnPosition;
        if (sceneCoreRoot != null && player.transform.parent != sceneCoreRoot)
            player.transform.SetParent(sceneCoreRoot, true);

        if (existingPlayer == null)
        {
            Collider c = player.GetComponent<Collider>();
            if (c != null) Object.DestroyImmediate(c);
        }

        CharacterController cc = EnsureComponent<CharacterController>(player);
        if (cc == null)
        {
            Debug.LogError("Failed to create CharacterController on Player during template generation.");
            return;
        }
        cc.center = new Vector3(0f, 0f, 0f); cc.height = 2f; cc.radius = 0.34f;
        cc.stepOffset = 0.35f; cc.slopeLimit = 45f; cc.skinWidth = 0.03f;

        Renderer pr = player.GetComponent<Renderer>();
        if (pr != null) pr.sharedMaterial = accentMaterial ?? GetMaterial(ref accentMaterial, "Accent", new Color(0.38f, 0.4f, 0.44f));

        PlayerMover mover       = EnsureComponent<PlayerMover>(player);
        PlayerInteraction inter = EnsureComponent<PlayerInteraction>(player);
        if (player.GetComponent<PlayerInventory>() == null)
        {
            player.AddComponent<PlayerInventory>();
        }

        Transform headAnchor = player.transform.Find("HeadAnchor");
        if (headAnchor == null) { headAnchor = new GameObject("HeadAnchor").transform; headAnchor.SetParent(player.transform); }
        headAnchor.localPosition = new Vector3(0f, 1.62f, 0f);
        headAnchor.localRotation = Quaternion.identity;

        HolstinCameraRig rig = Object.FindAnyObjectByType<HolstinCameraRig>();
        GameObject rigGO = rig != null ? rig.gameObject : new GameObject("HolstinCameraRig");
        if (rig == null) rig = rigGO.AddComponent<HolstinCameraRig>();
        if (sceneCoreRoot != null && rigGO.transform.parent != sceneCoreRoot)
            rigGO.transform.SetParent(sceneCoreRoot, true);

        Camera existingCam = Camera.main ?? Object.FindAnyObjectByType<Camera>();
        GameObject camGO = existingCam != null ? existingCam.gameObject : new GameObject("Main Camera");
        camGO.name = "Main Camera"; camGO.tag = "MainCamera";
        if (camGO.transform.parent == null) camGO.transform.SetParent(rigGO.transform);
        Camera cam = EnsureComponent<Camera>(camGO);
        cam.nearClipPlane = 0.03f; cam.farClipPlane = 500f; cam.clearFlags = CameraClearFlags.Skybox;
        UniversalAdditionalCameraData camData = cam.GetUniversalAdditionalCameraData();
        if (camData != null)
        {
            camData.renderPostProcessing = true;
            camData.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            camData.antialiasingQuality = AntialiasingQuality.High;
        }

        if (camGO.GetComponent<AudioListener>() == null) camGO.AddComponent<AudioListener>();

        Transform previewRoot = camGO.transform.Find("InspectPreviewRoot");
        if (previewRoot == null) { previewRoot = new GameObject("InspectPreviewRoot").transform; previewRoot.SetParent(camGO.transform); }
        previewRoot.localPosition = Vector3.zero; previewRoot.localRotation = Quaternion.identity;

        InspectItemViewer viewer = EnsureComponent<InspectItemViewer>(rigGO);
        viewer.SetPreviewRoot(previewRoot);

        InteractionPromptUI promptUI = EnsureInteractionPromptUI();
        DialoguePanelUI dialoguePanel = EnsureDialoguePanelUI();
        rig.Configure(player.transform, headAnchor, camGO.transform, cam);
        mover.SetCameraForwardSource(camGO.transform);
        inter.Configure(cam, rig, viewer);

        HolstinSceneContext context = Object.FindAnyObjectByType<HolstinSceneContext>();
        GameObject contextObject = context != null ? context.gameObject : new GameObject("HolstinSceneContext");
        if (sceneCoreRoot != null && contextObject.transform.parent != sceneCoreRoot)
        {
            contextObject.transform.SetParent(sceneCoreRoot, true);
        }

        if (context == null)
        {
            context = contextObject.AddComponent<HolstinSceneContext>();
        }

        context.Configure(rig, promptUI, viewer, mover, dialoguePanel);
        context.ResolveMissingReferences();
    }

    private static InteractionPromptUI EnsureInteractionPromptUI()
    {
        EnsureSceneRootGroups();
        InteractionPromptUI promptUI = Object.FindAnyObjectByType<InteractionPromptUI>();
        GameObject canvasGO = promptUI != null
            ? promptUI.gameObject
            : new GameObject("InteractionPromptUI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(InteractionPromptUI));
        if (sceneUiRoot != null && canvasGO.transform.parent != sceneUiRoot)
            canvasGO.transform.SetParent(sceneUiRoot, false);

        Canvas canvas = EnsureComponent<Canvas>(canvasGO);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.pixelPerfect = false;

        CanvasScaler scaler = EnsureComponent<CanvasScaler>(canvasGO);
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        if (canvasGO.GetComponent<GraphicRaycaster>() == null) canvasGO.AddComponent<GraphicRaycaster>();

        TMP_FontAsset font = TMP_Settings.defaultFontAsset
                          ?? Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");

        CreatePromptPanel(canvasGO.transform, "ContextPrompt",    new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f,  28f), new Vector2(560f, 64f),  font, 22, TextAlignmentOptions.Center);
        CreatePromptPanel(canvasGO.transform, "TransientMessage", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -28f), new Vector2(720f, 120f), font, 20, TextAlignmentOptions.Center);
        return canvasGO.GetComponent<InteractionPromptUI>();
    }

    private static DialoguePanelUI EnsureDialoguePanelUI()
    {
        EnsureSceneRootGroups();
        DialoguePanelUI dialoguePanel = Object.FindAnyObjectByType<DialoguePanelUI>();
        GameObject dialogueObject = dialoguePanel != null
            ? dialoguePanel.gameObject
            : new GameObject("DialoguePanelUI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(DialoguePanelUI));

        if (sceneUiRoot != null && dialogueObject.transform.parent != sceneUiRoot)
        {
            dialogueObject.transform.SetParent(sceneUiRoot, false);
        }

        return dialogueObject.GetComponent<DialoguePanelUI>();
    }

    private static void CreatePromptPanel(Transform parent, string panelName, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPos, Vector2 sizeDelta, TMP_FontAsset font, float fontSize, TextAlignmentOptions alignment)
    {
        Transform existing = parent.Find(panelName);
        GameObject panel = existing != null ? existing.gameObject : new GameObject(panelName, typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
        panel.transform.SetParent(parent, false);
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax; rt.pivot = pivot;
        rt.anchoredPosition = anchoredPos; rt.sizeDelta = sizeDelta;
        CanvasGroup cg = EnsureComponent<CanvasGroup>(panel);
        cg.blocksRaycasts = false; cg.interactable = false;
        Image bg = EnsureComponent<Image>(panel);
        bg.color = new Color(0.05f, 0.06f, 0.08f, 0.78f);
        Transform et = panel.transform.Find("Text");
        GameObject textGO = et != null ? et.gameObject : new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textGO.transform.SetParent(panel.transform, false);
        RectTransform tr = textGO.GetComponent<RectTransform>();
        tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one;
        tr.offsetMin = new Vector2(16f, 10f); tr.offsetMax = new Vector2(-16f, -10f);
        Text legacy = textGO.GetComponent<Text>();
        if (legacy != null) Object.DestroyImmediate(legacy);
        TextMeshProUGUI tmp = EnsureComponent<TextMeshProUGUI>(textGO);
        if (font != null) tmp.font = font;
        tmp.fontSize = fontSize; tmp.alignment = alignment; tmp.color = Color.white;
        tmp.textWrappingMode = TextWrappingModes.Normal; tmp.overflowMode = TextOverflowModes.Overflow;
        tmp.raycastTarget = false;
        tmp.text = panelName == "ContextPrompt" ? "[E] Interact" : "Interaction feedback appears here.";
    }
}
