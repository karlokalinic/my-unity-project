using NUnit.Framework;
using UnityEngine;

public class MonsterCoinRuntimeContractsTests
{
    [Test]
    public void CreatureCompoundCollision_ReplacesGenericRootCollider_AtVisibleGround()
    {
        GameObject host = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        host.name = "MonsterPhysicsContract";
        host.transform.position = new Vector3(3f, 1f, 2f);
        host.transform.localScale = new Vector3(0.9f, 1f, 0.9f);

        try
        {
            Collider genericRoot = host.GetComponent<Collider>();
            Assert.IsNotNull(genericRoot);
            Assert.IsTrue(genericRoot.enabled);

            HorrorCreatureRuntimeUtility.ConfigureCompoundCollision(
                host,
                2.3f,
                1f,
                0f,
                true);
            HorrorCreatureRuntimeUtility.SetCompoundCollisionEnabled(host, true);

            Assert.IsFalse(genericRoot.enabled, "Generic capsule must not remain as an invisible blocker.");

            Transform collisionRoot = host.transform.Find("__CreatureCollision");
            Assert.IsNotNull(collisionRoot);
            Assert.That(collisionRoot.position.y, Is.EqualTo(0f).Within(0.001f));

            Collider[] physicalParts = collisionRoot.GetComponentsInChildren<Collider>(true);
            Assert.GreaterOrEqual(physicalParts.Length, 6);
            for (int i = 0; i < physicalParts.Length; i++)
            {
                Assert.IsTrue(physicalParts[i].enabled);
            }

            HorrorCreatureRuntimeUtility.SetCompoundCollisionEnabled(host, false);
            for (int i = 0; i < physicalParts.Length; i++)
            {
                Assert.IsFalse(physicalParts[i].enabled);
            }
        }
        finally
        {
            Object.DestroyImmediate(host);
        }
    }

    [Test]
    public void CoinConfigure_StoresPositiveCollectionValue()
    {
        GameObject coin = new GameObject("CoinContract");
        try
        {
            coin.AddComponent<Rigidbody>();
            CoinCollectible collectible = coin.AddComponent<CoinCollectible>();
            collectible.Configure(3, 0.25f);
            Assert.AreEqual(3, collectible.Value);

            collectible.Configure(0, 0f);
            Assert.AreEqual(1, collectible.Value);
        }
        finally
        {
            Object.DestroyImmediate(coin);
        }
    }
}
