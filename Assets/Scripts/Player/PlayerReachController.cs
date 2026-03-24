using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerReachController : MonoBehaviour
{
    [SerializeField] private PlayerAnimationController animationController;
    [SerializeField] private float reachDuration = 0.28f;
    [SerializeField] private float rotateSpeed = 10f;

    private bool reaching;

    public bool IsReaching => reaching;

    private void Awake()
    {
        if (animationController == null)
        {
            animationController = GetComponent<PlayerAnimationController>();
        }
    }

    public IEnumerator PerformReach(Vector3 worldTarget, float? overrideDuration = null)
    {
        float duration = Mathf.Max(0.08f, overrideDuration ?? reachDuration);
        reaching = true;
        animationController?.BeginReach(worldTarget);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            RotateTowardTarget(worldTarget);
            yield return null;
        }

        animationController?.EndReach();
        reaching = false;
    }

    private void RotateTowardTarget(Vector3 worldTarget)
    {
        Vector3 direction = worldTarget - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.0001f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
    }
}
