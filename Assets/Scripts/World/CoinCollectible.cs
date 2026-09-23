using System;
using UnityEngine;

/// <summary>
/// Local, frame-responsive physical coin pickup. The visible cylinder mesh and its convex
/// trigger collider describe the same object; collection never waits on a network request.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody))]
public sealed class CoinCollectible : MonoBehaviour
{
    [SerializeField] private string currencyId = "coins";
    [SerializeField] private int value = 1;
    [SerializeField] private float spinDegreesPerSecond = 115f;
    [SerializeField] private float bobAmplitude = 0.08f;
    [SerializeField] private float bobFrequency = 1.55f;

    public static event Action<CoinCollectible, int> Collected;

    private Vector3 baseLocalPosition;
    private float phase;
    private bool collected;

    public int Value => value;

    public void Configure(int newValue, float phaseOffset)
    {
        value = Mathf.Max(1, newValue);
        phase = phaseOffset;
        baseLocalPosition = transform.localPosition;
    }

    private void Awake()
    {
        Rigidbody body = GetComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;
        body.interpolation = RigidbodyInterpolation.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

        baseLocalPosition = transform.localPosition;
    }

    private void Update()
    {
        if (collected)
        {
            return;
        }

        float time = Time.time + phase;
        transform.Rotate(Vector3.up, spinDegreesPerSecond * Time.deltaTime, Space.World);

        Vector3 local = baseLocalPosition;
        local.y += Mathf.Sin(time * bobFrequency * Mathf.PI * 2f) * bobAmplitude;
        transform.localPosition = local;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (collected || other == null)
        {
            return;
        }

        PlayerMover player = other.GetComponentInParent<PlayerMover>();
        if (player == null)
        {
            return;
        }

        CurrencyWallet wallet = player.GetComponent<CurrencyWallet>();
        if (wallet == null)
        {
            Debug.LogError("[CoinCollectible] Player has no CurrencyWallet; coin was not consumed.", this);
            return;
        }

        collected = true;
        wallet.Add(currencyId, value);
        Collected?.Invoke(this, value);

        Collider[] colliders = GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] != null)
            {
                colliders[i].enabled = false;
            }
        }

        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                renderers[i].enabled = false;
            }
        }

        Destroy(gameObject);
    }
}
