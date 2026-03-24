using System.Collections.Generic;
using UnityEngine;

public class InspectItemViewer : MonoBehaviour
{
    [SerializeField] private Transform previewRoot;
    [SerializeField] private float dragRotateSpeed = 140f;
    [SerializeField] private float inspectMessageDuration = 4.5f;

    private InspectableItem currentItem;
    private GameObject previewInstance;
    private bool isInspecting;

    public bool IsInspecting => isInspecting;

    public void SetPreviewRoot(Transform root)
    {
        previewRoot = root;
    }

    public void BeginInspect(InspectableItem item)
    {
        if (item == null || previewRoot == null)
        {
            return;
        }

        EndInspect();

        currentItem = item;
        currentItem.SetWorldVisible(false);

        previewInstance = Instantiate(item.gameObject, previewRoot);
        previewInstance.name = item.name + "_Preview";
        previewInstance.transform.localPosition = item.PreviewLocalPosition;
        previewInstance.transform.localRotation = Quaternion.Euler(item.PreviewLocalEuler);
        previewInstance.transform.localScale = item.transform.lossyScale * item.PreviewScale;

        StripClone(previewInstance);
        isInspecting = true;

        string message = string.IsNullOrWhiteSpace(item.ItemDescription)
            ? item.ItemName
            : $"{item.ItemName}\n\n{item.ItemDescription}";
        HolstinFeedback.ShowMessage($"{message}\n\nHold LMB to rotate. Press E, Esc, or move to return.", inspectMessageDuration);
    }

    public void EndInspect()
    {
        if (previewInstance != null)
        {
            Destroy(previewInstance);
        }

        if (currentItem != null)
        {
            currentItem.SetWorldVisible(true);
        }

        currentItem = null;
        previewInstance = null;
        isInspecting = false;
    }

    private void OnDisable()
    {
        EndInspect();
    }

    private void OnDestroy()
    {
        EndInspect();
    }

    private void Update()
    {
        if (!isInspecting)
        {
            return;
        }

        if (previewInstance == null || currentItem == null || previewRoot == null)
        {
            EndInspect();
            return;
        }

        float yawDelta = currentItem.AutoRotateSpeed * Time.deltaTime;

        if (InputReader.InspectRotateHeld())
        {
            Vector2 look = InputReader.GetLookDelta();
            yawDelta = -look.x * dragRotateSpeed * Time.deltaTime;
            float pitchDelta = look.y * dragRotateSpeed * Time.deltaTime;
            previewInstance.transform.Rotate(previewRoot.up, yawDelta, Space.World);
            previewInstance.transform.Rotate(previewRoot.right, pitchDelta, Space.World);
            return;
        }

        previewInstance.transform.Rotate(previewRoot.up, yawDelta, Space.World);
    }

    private static void StripClone(GameObject cloneRoot)
    {
        foreach (Collider colliderComponent in cloneRoot.GetComponentsInChildren<Collider>(true))
        {
            Destroy(colliderComponent);
        }

        foreach (Rigidbody body in cloneRoot.GetComponentsInChildren<Rigidbody>(true))
        {
            Destroy(body);
        }

        List<MonoBehaviour> behavioursToDestroy = new List<MonoBehaviour>();
        foreach (MonoBehaviour behaviour in cloneRoot.GetComponentsInChildren<MonoBehaviour>(true))
        {
            behavioursToDestroy.Add(behaviour);
        }

        foreach (MonoBehaviour behaviour in behavioursToDestroy)
        {
            Destroy(behaviour);
        }
    }
}
