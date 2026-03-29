using UnityEngine;

/// <summary>
/// Core NPC identity: name, faction, narrative role. Replaces the old NPCPlaceholder stub.
/// Attach to any NPC to give them a trackable identity for dialogue, reputation, and quest systems.
/// </summary>
public class NpcIdentity : MonoBehaviour
{
    [SerializeField] private string npcName = "Resident";
    [SerializeField] private string factionId = "boarding_house";
    [SerializeField] [TextArea(2, 4)] private string narrativeRole = "Knows something they should not.";
    [SerializeField] private Transform lookTarget;
    [SerializeField] private float turnSpeed = 3.5f;
    private Rigidbody body;

    public string NpcName => npcName;
    public string FactionId => factionId;
    public string NarrativeRole => narrativeRole;

    public void Configure(string newName, string newRole, Transform target, string faction = "boarding_house")
    {
        npcName = newName;
        narrativeRole = newRole;
        lookTarget = target;
        factionId = faction;
    }

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (lookTarget == null) return;

        Vector3 direction = lookTarget.position - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.01f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        if (body != null && !body.isKinematic)
        {
            bool rotationFullyFrozen = (body.constraints & RigidbodyConstraints.FreezeRotationX) != 0 &&
                                       (body.constraints & RigidbodyConstraints.FreezeRotationY) != 0 &&
                                       (body.constraints & RigidbodyConstraints.FreezeRotationZ) != 0;
            if (rotationFullyFrozen)
            {
                return;
            }

            Quaternion nextRotation = Quaternion.Slerp(body.rotation, targetRotation, turnSpeed * Time.deltaTime);
            body.MoveRotation(nextRotation);
            return;
        }

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
    }
}
