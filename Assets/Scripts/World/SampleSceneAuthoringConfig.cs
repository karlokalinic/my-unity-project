using UnityEngine;

[DisallowMultipleComponent]
public class SampleSceneAuthoringConfig : MonoBehaviour
{
    [Header("Spawn Points")]
    [SerializeField] private Vector3 playerSpawnPosition = new Vector3(-7f, 1.2f, 0f);
    [SerializeField] private Vector3 playerSpawnEuler = Vector3.zero;
    [SerializeField] private Vector3 friendlyNpcSpawnPosition = new Vector3(-4.6f, 1.2f, 1.8f);
    [SerializeField] private Vector3 hostileNpcSpawnPosition = new Vector3(12.2f, 4.5f, 0.4f);

    [Header("Ground")]
    [SerializeField] private string groundObjectName = "InteractableGround";
    [SerializeField] private Vector3 groundPosition = new Vector3(0f, -0.01f, 0f);
    [SerializeField] private Vector3 groundScale = new Vector3(34f, 0.2f, 22f);

    [Header("Interior Root")]
    [SerializeField] private string interiorRootName = "SampleInteriorShell";
    [SerializeField] private Vector3 interiorOrigin = Vector3.zero;

    [Header("Interior Shell Dimensions")]
    [SerializeField] private float roomHalfX = 10.7f;
    [SerializeField] private float roomHalfZ = 6.7f;
    [SerializeField] private float wallHeight = 3.4f;
    [SerializeField] private float wallThickness = 0.22f;
    [SerializeField] private float ceilingThickness = 0.14f;
    [SerializeField] private float frontDoorWidth = 2.6f;
    [SerializeField] private float frontDoorHeight = 2.35f;
    [SerializeField] private float floorFinishYOffset = 0.01f;

    [Header("Alignment")]
    [SerializeField] private float actorGroundYOffset = 0.005f;
    [SerializeField] private float visualGroundOffset = 0.008f;

    public Vector3 PlayerSpawnPosition => playerSpawnPosition;
    public Quaternion PlayerSpawnRotation => Quaternion.Euler(playerSpawnEuler);
    public Vector3 FriendlyNpcSpawnPosition => friendlyNpcSpawnPosition;
    public Vector3 HostileNpcSpawnPosition => hostileNpcSpawnPosition;

    public string GroundObjectName => string.IsNullOrWhiteSpace(groundObjectName) ? "InteractableGround" : groundObjectName.Trim();
    public Vector3 GroundPosition => groundPosition;
    public Vector3 GroundScale => new Vector3(
        Mathf.Max(1f, groundScale.x),
        Mathf.Max(0.05f, groundScale.y),
        Mathf.Max(1f, groundScale.z));

    public string InteriorRootName => string.IsNullOrWhiteSpace(interiorRootName) ? "SampleInteriorShell" : interiorRootName.Trim();
    public Vector3 InteriorOrigin => interiorOrigin;

    public float RoomHalfX => Mathf.Max(2f, roomHalfX);
    public float RoomHalfZ => Mathf.Max(2f, roomHalfZ);
    public float WallHeight => Mathf.Max(2f, wallHeight);
    public float WallThickness => Mathf.Max(0.05f, wallThickness);
    public float CeilingThickness => Mathf.Max(0.05f, ceilingThickness);
    public float FrontDoorWidth => Mathf.Max(1f, frontDoorWidth);
    public float FrontDoorHeight => Mathf.Max(1.6f, frontDoorHeight);
    public float FloorFinishYOffset => Mathf.Clamp(floorFinishYOffset, 0f, 0.2f);

    public float ActorGroundYOffset => Mathf.Clamp(actorGroundYOffset, 0f, 0.08f);
    public float VisualGroundOffset => Mathf.Clamp(visualGroundOffset, 0f, 0.08f);
}
