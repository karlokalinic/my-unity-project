using UnityEngine;

public class NPCPlaceholder : MonoBehaviour
{
    [SerializeField] private string npcName = "Resident";
    [SerializeField] [TextArea(2, 4)] private string narrativeRole = "Knows something they should not.";
    [SerializeField] private Transform lookTarget;
    [SerializeField] private float turnSpeed = 3.5f;
    [SerializeField] private bool revealOnStart = false;


    public void Configure(string newName, string newRole, Transform target)
    {
        npcName = newName;
        narrativeRole = newRole;
        lookTarget = target;
    }

    private void Start()
    {
        if (revealOnStart)
        {
            Debug.Log($"NPC: {npcName}\n{narrativeRole}");
        }
    }

    private void Update()
    {
        if (lookTarget == null)
        {
            return;
        }

        Vector3 direction = lookTarget.position - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.01f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
    }
}
