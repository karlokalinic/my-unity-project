using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class IsometricFrontWallHider : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HolstinCameraRig cameraRig;
    [SerializeField] private Transform target;
    [SerializeField] private Camera sourceCamera;

    [Header("Cutaway")]
    [SerializeField] private LayerMask occluderMask = ~0;
    [SerializeField] private float castRadius = 0.22f;
    [SerializeField] private float targetFocusHeight = 1.3f;
    [SerializeField] private float minCastDistance = 0.5f;
    [SerializeField] private bool hideOnlyWallLikeObjects = true;
    [SerializeField] private bool hideOnlyIndoorRootRenderers = true;
    [SerializeField] private bool disableDuringFirstPersonTransition = true;
    [SerializeField] [Range(0f, 1f)] private float firstPersonTransitionDisableBlend = 0.08f;

    [Header("Indoor Gate")]
    [SerializeField] private Transform indoorRoot;
    [SerializeField] private Vector3 indoorBoundsCenter = new Vector3(0f, 1.6f, 0f);
    [SerializeField] private Vector3 indoorBoundsSize = new Vector3(21.4f, 3.6f, 13.4f);
    [SerializeField] private float indoorBoundsInsetX = 0.55f;
    [SerializeField] private float indoorBoundsInsetZ = 0.7f;
    [SerializeField] private bool requireCeilingAboveTarget = true;
    [SerializeField] private LayerMask indoorCeilingMask = ~0;
    [SerializeField] private float ceilingCheckDistance = 2.7f;

    private readonly HashSet<Renderer> hiddenRenderers = new HashSet<Renderer>();
    private readonly HashSet<Renderer> nextHiddenRenderers = new HashSet<Renderer>();
    private readonly Dictionary<Renderer, bool> initialEnabledState = new Dictionary<Renderer, bool>();
    private readonly List<Renderer> restoreBuffer = new List<Renderer>();

    public void Configure(HolstinCameraRig rig, Transform targetTransform, Camera camera, Transform interiorRoot = null)
    {
        cameraRig = rig;
        target = targetTransform;
        sourceCamera = camera;
        if (interiorRoot != null)
        {
            indoorRoot = interiorRoot;
        }
    }

    private void OnDisable()
    {
        RestoreAll();
    }

    private void LateUpdate()
    {
        ResolveReferences();
        UpdateOccluders();
    }

    private void ResolveReferences()
    {
        if (cameraRig == null)
        {
            cameraRig = GetComponent<HolstinCameraRig>();
            if (cameraRig == null)
            {
                cameraRig = FindAnyObjectByType<HolstinCameraRig>();
            }
        }

        if (target == null)
        {
            if (HolstinSceneContext.TryGet(out HolstinSceneContext context) && context != null)
            {
                target = context.PlayerTransform;
            }

            if (target == null)
            {
                PlayerMover mover = FindAnyObjectByType<PlayerMover>();
                if (mover != null)
                {
                    target = mover.transform;
                }
            }
        }

        if (sourceCamera == null)
        {
            sourceCamera = cameraRig != null ? cameraRig.ControlledCamera : null;
            if (sourceCamera == null)
            {
                sourceCamera = Camera.main;
            }
        }
    }

    private void UpdateOccluders()
    {
        if (target == null || sourceCamera == null)
        {
            RestoreAll();
            return;
        }

        if (cameraRig != null)
        {
            if (cameraRig.IsInFirstPerson)
            {
                RestoreAll();
                return;
            }

            if (disableDuringFirstPersonTransition &&
                cameraRig.FirstPersonBlend >= Mathf.Clamp01(firstPersonTransitionDisableBlend))
            {
                RestoreAll();
                return;
            }
        }

        if (!IsTargetInsideIndoorArea())
        {
            RestoreAll();
            return;
        }

        Vector3 focusPoint = target.position + (Vector3.up * targetFocusHeight);
        Vector3 toCamera = sourceCamera.transform.position - focusPoint;
        float distance = toCamera.magnitude;
        if (distance < minCastDistance)
        {
            RestoreAll();
            return;
        }

        nextHiddenRenderers.Clear();
        Vector3 direction = toCamera / distance;
        RaycastHit[] hits = Physics.SphereCastAll(
            focusPoint,
            Mathf.Max(0.01f, castRadius),
            direction,
            distance,
            occluderMask,
            QueryTriggerInteraction.Ignore);

        for (int i = 0; i < hits.Length; i++)
        {
            Collider hitCollider = hits[i].collider;
            if (hitCollider == null)
            {
                continue;
            }

            Transform hitTransform = hitCollider.transform;
            if (target != null && hitTransform.IsChildOf(target))
            {
                continue;
            }

            if (cameraRig != null && hitTransform.IsChildOf(cameraRig.transform))
            {
                continue;
            }

            Renderer renderer = hitCollider.GetComponentInParent<Renderer>();
            if (renderer == null)
            {
                continue;
            }

            if (hideOnlyIndoorRootRenderers && indoorRoot != null && !renderer.transform.IsChildOf(indoorRoot))
            {
                continue;
            }

            if (hideOnlyWallLikeObjects && !LooksLikeWall(renderer.gameObject.name))
            {
                continue;
            }

            nextHiddenRenderers.Add(renderer);
        }

        foreach (Renderer renderer in nextHiddenRenderers)
        {
            Hide(renderer);
        }

        restoreBuffer.Clear();
        foreach (Renderer renderer in hiddenRenderers)
        {
            if (!nextHiddenRenderers.Contains(renderer))
            {
                restoreBuffer.Add(renderer);
            }
        }

        for (int i = 0; i < restoreBuffer.Count; i++)
        {
            Restore(restoreBuffer[i]);
        }
    }

    private void Hide(Renderer renderer)
    {
        if (renderer == null || hiddenRenderers.Contains(renderer))
        {
            return;
        }

        initialEnabledState[renderer] = renderer.enabled;
        renderer.enabled = false;
        hiddenRenderers.Add(renderer);
    }

    private void Restore(Renderer renderer)
    {
        if (renderer == null)
        {
            hiddenRenderers.Remove(renderer);
            initialEnabledState.Remove(renderer);
            return;
        }

        bool initialEnabled = true;
        if (initialEnabledState.TryGetValue(renderer, out bool cached))
        {
            initialEnabled = cached;
        }

        renderer.enabled = initialEnabled;
        hiddenRenderers.Remove(renderer);
        initialEnabledState.Remove(renderer);
    }

    private void RestoreAll()
    {
        restoreBuffer.Clear();
        foreach (Renderer renderer in hiddenRenderers)
        {
            restoreBuffer.Add(renderer);
        }

        for (int i = 0; i < restoreBuffer.Count; i++)
        {
            Restore(restoreBuffer[i]);
        }
    }

    private static bool LooksLikeWall(string objectName)
    {
        if (string.IsNullOrWhiteSpace(objectName))
        {
            return false;
        }

        string key = objectName.ToLowerInvariant();
        return key.Contains("wall") ||
               key.Contains("ceiling") ||
               key.Contains("divider") ||
               key.Contains("windowframe") ||
               key.Contains("doorframe") ||
               key.Contains("quarantine") ||
               key.Contains("shortwall");
    }

    private bool IsTargetInsideIndoorArea()
    {
        if (target == null)
        {
            return false;
        }

        if (indoorRoot == null)
        {
            return false;
        }

        Vector3 localTarget = indoorRoot.InverseTransformPoint(target.position);
        Vector3 effectiveSize = indoorBoundsSize;
        effectiveSize.x = Mathf.Max(0.1f, effectiveSize.x - Mathf.Max(0f, indoorBoundsInsetX) * 2f);
        effectiveSize.z = Mathf.Max(0.1f, effectiveSize.z - Mathf.Max(0f, indoorBoundsInsetZ) * 2f);
        Bounds bounds = new Bounds(indoorBoundsCenter, effectiveSize);
        if (!bounds.Contains(localTarget))
        {
            return false;
        }

        if (!requireCeilingAboveTarget)
        {
            return true;
        }

        float castDistance = Mathf.Max(0.2f, ceilingCheckDistance);
        RaycastHit[] hits = Physics.RaycastAll(
            target.position + (Vector3.up * 0.12f),
            Vector3.up,
            castDistance,
            indoorCeilingMask,
            QueryTriggerInteraction.Ignore);

        if (hits == null || hits.Length == 0)
        {
            return false;
        }

        Array.Sort(hits, static (a, b) => a.distance.CompareTo(b.distance));
        for (int i = 0; i < hits.Length; i++)
        {
            Collider collider = hits[i].collider;
            if (collider == null)
            {
                continue;
            }

            Transform hitTransform = collider.transform;
            if (target != null && hitTransform.IsChildOf(target))
            {
                continue;
            }

            if (cameraRig != null && hitTransform.IsChildOf(cameraRig.transform))
            {
                continue;
            }

            if (LooksLikeOverheadSurface(collider, target.position, indoorRoot))
            {
                return true;
            }

            return false;
        }

        return false;
    }

    private static bool LooksLikeCeiling(string objectName)
    {
        if (string.IsNullOrWhiteSpace(objectName))
        {
            return false;
        }

        string key = objectName.ToLowerInvariant();
        return key.Contains("ceiling") ||
               key.Contains("roof") ||
               key.Contains("top");
    }

    private static bool LooksLikeOverheadSurface(Collider collider, Vector3 targetPosition, Transform root)
    {
        if (collider == null)
        {
            return false;
        }

        if (LooksLikeCeiling(collider.gameObject.name))
        {
            return true;
        }

        if (root == null || !collider.transform.IsChildOf(root))
        {
            return false;
        }

        Bounds bounds = collider.bounds;
        float minCeilingHeight = targetPosition.y + 1.15f;
        if (bounds.center.y <= minCeilingHeight)
        {
            return false;
        }

        float horizontalSpan = Mathf.Max(bounds.size.x, bounds.size.z);
        return bounds.size.y <= 0.65f && horizontalSpan >= 1.4f;
    }
}
