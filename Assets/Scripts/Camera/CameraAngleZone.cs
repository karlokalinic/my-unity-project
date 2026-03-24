using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CameraAngleZone : MonoBehaviour
{
    [SerializeField] private float isometricYaw = 45f;
    [SerializeField] private bool snapOnEnter;
    [SerializeField] private Color gizmoColor = new Color(0.35f, 0.8f, 1f, 0.2f);

    private HolstinCameraRig cameraRig;


    public void Configure(float yaw, bool snapInstantly = false)
    {
        isometricYaw = yaw;
        snapOnEnter = snapInstantly;
    }

    private void Reset()
    {
        Collider trigger = GetComponent<Collider>();
        trigger.isTrigger = true;
    }

    private void Awake()
    {
        ResolveCameraRig();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.GetComponent<PlayerMover>())
        {
            return;
        }

        ResolveCameraRig();
        if (cameraRig != null)
        {
            cameraRig.SetIsometricYaw(isometricYaw, snapOnEnter);
        }
    }

    private void ResolveCameraRig()
    {
        if (cameraRig != null)
        {
            return;
        }

        if (HolstinSceneContext.TryGet(out HolstinSceneContext context) && context.CameraRig != null)
        {
            cameraRig = context.CameraRig;
            return;
        }

        cameraRig = FindAnyObjectByType<HolstinCameraRig>();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Collider colliderComponent = GetComponent<Collider>();
        if (colliderComponent is BoxCollider box)
        {
            Matrix4x4 old = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(box.center, box.size);
            Gizmos.matrix = old;
        }
    }
}
