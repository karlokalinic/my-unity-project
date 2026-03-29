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
    [SerializeField] private float dialogueShotFov = 44f;
    [SerializeField] private float blendInSeconds = 0.45f;
    [SerializeField] private float blendOutSeconds = 0.32f;
    [SerializeField] private bool useDynamicTwoShot = true;
    [SerializeField] private float twoShotDistance = 3.35f;
    [SerializeField] private float twoShotSideOffset = 1.55f;
    [SerializeField] private float twoShotHeight = 1.45f;
    [SerializeField] private float twoShotFocusLift = 0.08f;
    [SerializeField] private float playerFocusHeight = 1.42f;
    [SerializeField] private float npcFocusHeight = 1.44f;
    [SerializeField] private float playerFaceLerpSpeed = 9f;

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
        HolstinCameraRig rig = ResolveCameraRig(interactor);
        EnsureCameraAnchor();
        UpdateDialogueAnchor(interactor, rig);

        int pauseToken = GameplayPauseFacade.PushPause();
        if (interactor != null)
        {
            interactor.SetBusy(true);
        }

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
                UpdateDialogueAnchor(interactor, rig);
                AlignInteractorToNpc(interactor);
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

    private void UpdateDialogueAnchor(PlayerInteraction interactor, HolstinCameraRig rig)
    {
        if (cameraAnchor == null)
        {
            return;
        }

        if (!useDynamicTwoShot || interactor == null)
        {
            AlignAnchorRotation();
            return;
        }

        Vector3 playerFocus = interactor.transform.position + Vector3.up * playerFocusHeight;
        Transform npcTransform = lookAtTarget != null ? lookAtTarget : transform;
        Vector3 npcFocus = npcTransform.position + Vector3.up * npcFocusHeight;

        Vector3 pair = npcFocus - playerFocus;
        pair.y = 0f;
        if (pair.sqrMagnitude < 0.01f)
        {
            AlignAnchorRotation();
            return;
        }

        Vector3 pairForward = pair.normalized;
        Vector3 side = Vector3.Cross(Vector3.up, pairForward).normalized;
        Vector3 center = Vector3.Lerp(playerFocus, npcFocus, 0.5f);

        if (rig != null && rig.CameraTransform != null)
        {
            Vector3 currentOffset = rig.CameraTransform.position - center;
            if (Vector3.Dot(currentOffset, side) < 0f)
            {
                side = -side;
            }
        }

        Vector3 shotPosition = center +
                               (Vector3.up * twoShotHeight) +
                               (side * twoShotSideOffset) -
                               (pairForward * twoShotDistance);
        cameraAnchor.position = shotPosition;

        Vector3 lookPoint = center + Vector3.up * twoShotFocusLift;
        Vector3 lookDirection = lookPoint - shotPosition;
        if (lookDirection.sqrMagnitude > 0.0001f)
        {
            cameraAnchor.rotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
        }
    }

    private void AlignInteractorToNpc(PlayerInteraction interactor)
    {
        if (interactor == null)
        {
            return;
        }

        Transform npcTransform = lookAtTarget != null ? lookAtTarget : transform;
        Vector3 direction = npcTransform.position - interactor.transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.0001f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        float t = 1f - Mathf.Exp(-Mathf.Max(0.01f, playerFaceLerpSpeed) * Time.deltaTime);
        interactor.transform.rotation = Quaternion.Slerp(interactor.transform.rotation, targetRotation, t);
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
