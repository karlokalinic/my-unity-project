using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(ProceduralHumanoidRig))]
public class PlayerHeadAnchorDriver : MonoBehaviour
{
    [SerializeField] private ProceduralHumanoidRig rig;
    [SerializeField] private Transform headAnchor;
    [SerializeField] private bool followPhysicalHead;
    [SerializeField] private Vector3 localOffset = new Vector3(0f, 0.06f, 0.03f);

    private Transform resolvedHead;

    public void Configure(ProceduralHumanoidRig rigComponent, Transform anchor, bool usePhysicalHead, Vector3 offset)
    {
        rig = rigComponent;
        headAnchor = anchor;
        followPhysicalHead = usePhysicalHead;
        localOffset = offset;
        ResolveHeadBone();
    }

    private void Awake()
    {
        if (rig == null)
        {
            rig = GetComponent<ProceduralHumanoidRig>();
        }

        ResolveHeadBone();
    }

    private void LateUpdate()
    {
        if (headAnchor == null)
        {
            return;
        }

        if (rig == null)
        {
            rig = GetComponent<ProceduralHumanoidRig>();
            if (rig == null)
            {
                return;
            }
        }

        if (resolvedHead == null)
        {
            ResolveHeadBone();
            if (resolvedHead == null)
            {
                return;
            }
        }

        Vector3 worldOffset = resolvedHead.TransformVector(localOffset);
        headAnchor.position = resolvedHead.position + worldOffset;
        headAnchor.rotation = resolvedHead.rotation;
    }

    private void ResolveHeadBone()
    {
        if (rig == null)
        {
            resolvedHead = null;
            return;
        }

        rig.EnsureBuilt();
        resolvedHead = rig.GetBone("Head", !followPhysicalHead);
    }
}
