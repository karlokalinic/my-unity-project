using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

[DisallowMultipleComponent]
public class NpcDialogueController : MonoBehaviour
{
    [Header("Dialogue Camera")]
    [SerializeField] private Transform cameraAnchor;
    [SerializeField] private Transform lookAtTarget;
    [SerializeField] private float dialogueShotFov = 38f;
    [SerializeField] private float blendInSeconds = 0.45f;
    [SerializeField] private float blendOutSeconds = 0.32f;

    [Header("Timeline (Optional)")]
    [SerializeField] private PlayableDirector enterTimeline;
    [SerializeField] private PlayableDirector exitTimeline;
    [SerializeField] private bool timelineControlsCamera;

    private bool runningConversation;

    public bool IsRunningConversation => runningConversation;
    public Transform CameraAnchor => cameraAnchor;

    public void ConfigureCameraAnchor(Transform anchor)
    {
        cameraAnchor = anchor;
    }

    public void StartConversation(
        PlayerInteraction interactor,
        DialogueNodeData node,
        Action<DialogueSelectionResult> onSelection)
    {
        if (!Application.isPlaying || runningConversation)
        {
            return;
        }

        StartCoroutine(ConversationRoutine(interactor, node, onSelection));
    }

    private IEnumerator ConversationRoutine(
        PlayerInteraction interactor,
        DialogueNodeData node,
        Action<DialogueSelectionResult> onSelection)
    {
        runningConversation = true;
        EnsureCameraAnchor();
        AlignAnchorRotation();

        int pauseToken = GameplayPauseFacade.PushPause();
        if (interactor != null)
        {
            interactor.SetBusy(true);
        }

        HolstinCameraRig rig = ResolveCameraRig(interactor);
        yield return PlayEnterCinematic(rig);

        DialoguePanelUI panel = HolstinFeedback.ResolveDialoguePanel();
        bool picked = false;
        DialogueSelectionResult selection = ResolveLeaveFallback(node);
        if (panel != null)
        {
            panel.ShowChoiceDialogue(node, result =>
            {
                selection = result;
                picked = true;
            });

            while (panel != null && panel.IsShowing && !picked)
            {
                yield return null;
            }
        }
        else
        {
            picked = true;
        }

        onSelection?.Invoke(selection);

        yield return PlayExitCinematic(rig);
        if (interactor != null)
        {
            interactor.SetBusy(false);
        }

        GameplayPauseFacade.PopPause(pauseToken);
        runningConversation = false;
    }

    private IEnumerator PlayEnterCinematic(HolstinCameraRig rig)
    {
        if (enterTimeline != null)
        {
            enterTimeline.Play();
            while (enterTimeline.state == PlayState.Playing)
            {
                yield return null;
            }
        }

        if (!timelineControlsCamera && rig != null)
        {
            rig.BeginDialogueShot(cameraAnchor, dialogueShotFov, blendInSeconds);
            float timer = Mathf.Max(0.05f, blendInSeconds);
            while (timer > 0f)
            {
                timer -= Time.deltaTime;
                yield return null;
            }
        }
    }

    private IEnumerator PlayExitCinematic(HolstinCameraRig rig)
    {
        if (!timelineControlsCamera && rig != null)
        {
            rig.EndDialogueShot(blendOutSeconds);
            float timer = Mathf.Max(0.05f, blendOutSeconds);
            while (timer > 0f)
            {
                timer -= Time.deltaTime;
                yield return null;
            }
        }

        if (exitTimeline != null)
        {
            exitTimeline.Play();
            while (exitTimeline.state == PlayState.Playing)
            {
                yield return null;
            }
        }
    }

    private HolstinCameraRig ResolveCameraRig(PlayerInteraction interactor)
    {
        if (HolstinSceneContext.TryGet(out HolstinSceneContext context) && context.CameraRig != null)
        {
            return context.CameraRig;
        }

        if (interactor != null)
        {
            HolstinCameraRig fromInteractor = interactor.GetComponent<HolstinCameraRig>();
            if (fromInteractor != null)
            {
                return fromInteractor;
            }
        }

        return FindAnyObjectByType<HolstinCameraRig>();
    }

    private void EnsureCameraAnchor()
    {
        if (cameraAnchor != null)
        {
            return;
        }

        Transform existing = transform.Find("DialogueCameraAnchor");
        if (existing == null)
        {
            GameObject anchorObject = new GameObject("DialogueCameraAnchor");
            anchorObject.transform.SetParent(transform, false);
            existing = anchorObject.transform;
        }

        existing.localPosition = new Vector3(0.65f, 1.52f, 1.35f);
        existing.localRotation = Quaternion.Euler(10f, 200f, 0f);
        cameraAnchor = existing;
    }

    private void AlignAnchorRotation()
    {
        if (cameraAnchor == null)
        {
            return;
        }

        Transform lookTarget = lookAtTarget != null ? lookAtTarget : transform;
        Vector3 direction = lookTarget.position + Vector3.up * 1.35f - cameraAnchor.position;
        if (direction.sqrMagnitude < 0.0001f)
        {
            return;
        }

        cameraAnchor.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
    }

    private static DialogueSelectionResult ResolveLeaveFallback(DialogueNodeData node)
    {
        if (node?.Choices == null || node.Choices.Length == 0)
        {
            return new DialogueSelectionResult(-1, null);
        }

        for (int i = 0; i < node.Choices.Length; i++)
        {
            DialogueChoiceData choice = node.Choices[i];
            if (choice != null && choice.IsLeave)
            {
                return new DialogueSelectionResult(i, choice);
            }
        }

        int fallbackIndex = node.Choices.Length - 1;
        return new DialogueSelectionResult(fallbackIndex, node.Choices[fallbackIndex]);
    }
}
