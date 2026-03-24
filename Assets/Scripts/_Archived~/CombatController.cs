using UnityEngine;

public class CombatController
{
    public void HandleCombat(
        Camera viewCamera,
        float fireDistance,
        LayerMask fireMask,
        float fireCooldown,
        float rigidbodyImpactForce,
        ref float nextFireTime)
    {
        if (GameplayPauseFacade.IsPaused)
        {
            return;
        }

        if (viewCamera == null)
        {
            return;
        }

        if (!InputReader.FireHeld() || Time.time < nextFireTime)
        {
            return;
        }

        nextFireTime = Time.time + fireCooldown;

        Ray ray = viewCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out RaycastHit hit, fireDistance, fireMask, QueryTriggerInteraction.Ignore))
        {
            if (hit.rigidbody != null)
            {
                hit.rigidbody.AddForceAtPosition(ray.direction * rigidbodyImpactForce, hit.point, ForceMode.Impulse);
            }

            SimpleEnemyAgent enemy = hit.collider.GetComponentInParent<SimpleEnemyAgent>();
            if (enemy != null)
            {
                enemy.RegisterHit();
            }
        }

        Debug.DrawRay(ray.origin, ray.direction * fireDistance, Color.red, 0.25f);
    }
}
