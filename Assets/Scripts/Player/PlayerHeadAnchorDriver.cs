using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(ProceduralHumanoidRig))]
public class PlayerHeadAnchorDriver : MonoBehaviour
{
    [SerializeField] private ProceduralHumanoidRig rig;
    [SerializeField] private PlayerHumanoidVisualDriver humanoidVisualDriver;
    [SerializeField] private Transform headAnchor;
    [SerializeField] private bool followPhysicalHead;
    [SerializeField] private bool preferRenderedHumanoidHead = true;
    [SerializeField] private Vector3 localOffset = new Vector3(0f, 0.06f, 0.03f);
    [SerializeField] private Vector3 renderedHumanoidEyeOffset = new Vector3(0f, 0.055f, 0.085f);

    private Transform resolvedProceduralHead;
    private Transform resolvedVisualHead;

    public void Configure(ProceduralHumanoidRig rigComponent, Transform anchor, bool usePhysicalHead, Vector3 offset)
    {
        rig = rigComponent;
        headAnchor = anchor;
        followPhysicalHead = usePhysicalHead;
        localOffset = offset;
        EnsureHumanoidVisualDriver();
        ResolveHeadBones();
    }

    private void Awake()
    {
        if (rig == null)
        {
            rig = GetComponent<ProceduralHumanoidRig>();
        }

        EnsureHumanoidVisualDriver();
        ResolveHeadBones();
    }

    private void Start()
    {
        if (humanoidVisualDriver != null)
        {
            humanoidVisualDriver.ResolveNow();
        }
        ResolveHeadBones();
    }

    private void LateUpdate()
    {
        if (headAnchor == null)
        {
            return;
        }

        Transform sourceHead = ResolveCurrentHead();
        if (sourceHead == null)
        {
            return;
        }

        Vector3 worldOffset;
        if (sourceHead == resolvedVisualHead)
        {
            // Humanoid FBX bone axes are importer-defined. Use actor axes for eye placement
            // so the first-person camera cannot end up behind/inside the skull on a valid avatar.
            worldOffset = transform.TransformVector(renderedHumanoidEyeOffset);
        }
        else
        {
            worldOffset = sourceHead.TransformVector(localOffset);
        }

        headAnchor.position = sourceHead.position + worldOffset;
        headAnchor.rotation = sourceHead.rotation;
    }

    private Transform ResolveCurrentHead()
    {
        if (preferRenderedHumanoidHead && resolvedVisualHead != null && resolvedVisualHead.gameObject.activeInHierarchy)
        {
            return resolvedVisualHead;
        }

        if (resolvedProceduralHead == null)
        {
            ResolveHeadBones();
        }

        return resolvedProceduralHead;
    }

    private void EnsureHumanoidVisualDriver()
    {
        if (humanoidVisualDriver != null)
        {
            return;
        }

        humanoidVisualDriver = GetComponent<PlayerHumanoidVisualDriver>();
        if (humanoidVisualDriver == null)
        {
            humanoidVisualDriver = gameObject.AddComponent<PlayerHumanoidVisualDriver>();
        }
    }

    private void ResolveHeadBones()
    {
        resolvedProceduralHead = null;
        resolvedVisualHead = null;

        if (rig != null)
        {
            rig.EnsureBuilt();
            resolvedProceduralHead = rig.GetBone("Head", !followPhysicalHead);
        }

        if (preferRenderedHumanoidHead && humanoidVisualDriver != null)
        {
            humanoidVisualDriver.ResolveNow();
            resolvedVisualHead = humanoidVisualDriver.VisualHead;
        }
    }
}
