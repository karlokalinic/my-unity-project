using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class NarrativeZone : MonoBehaviour
{
    public static event Action<NarrativeZone, string> Revealed;

    [SerializeField] private string locationName = "Unnamed Location";
    [SerializeField] [TextArea(2, 5)] private string summary = "A story-relevant location.";
    [SerializeField] private bool revealOnEnter = true;
    [SerializeField] private bool revealOnlyOnce = true;
    [SerializeField] private float messageDuration = 4.5f;
    [SerializeField] private Color gizmoColor = new Color(1f, 0.7f, 0.2f, 0.15f);

    private bool hasRevealed;


    public void Configure(string newLocationName, string newSummary, bool autoReveal = true)
    {
        locationName = newLocationName;
        summary = newSummary;
        revealOnEnter = autoReveal;
    }

    private void Reset()
    {
        Collider colliderComponent = GetComponent<Collider>();
        colliderComponent.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!revealOnEnter || !other.GetComponent<PlayerMover>())
        {
            return;
        }

        Reveal();
    }

    public void Reveal()
    {
        if (revealOnlyOnce && hasRevealed)
        {
            return;
        }

        hasRevealed = true;

        string headline = string.IsNullOrWhiteSpace(locationName) ? "Location" : locationName;
        string body = string.IsNullOrWhiteSpace(summary) ? string.Empty : $"\n\n{summary}";
        string message = headline + body;

        if (Application.isPlaying)
        {
            HolstinFeedback.ShowMessage(message, messageDuration);
        }
        else
        {
            Debug.Log($"Narrative Location: {locationName}\n{summary}");
        }

        Revealed?.Invoke(this, message);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        if (TryGetComponent(out BoxCollider box))
        {
            Matrix4x4 old = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(box.center, box.size);
            Gizmos.matrix = old;
        }
        else if (TryGetComponent(out SphereCollider sphere))
        {
            Matrix4x4 old = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawSphere(sphere.center, sphere.radius);
            Gizmos.matrix = old;
        }
    }
}
