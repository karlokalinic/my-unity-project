using NUnit.Framework;
using UnityEngine;

public class PlayerHumanoidMicroDetailTests
{
    [Test]
    public void AnatomicalDetail_InstallsOptionalEyeGazeCompanion()
    {
        GameObject actor = new GameObject("HumanoidMicroDetailContractTest");
        try
        {
            actor.AddComponent<ProceduralHumanoidRig>();
            PlayerAnatomicalDetailDriver anatomy = actor.AddComponent<PlayerAnatomicalDetailDriver>();

            Assert.IsNotNull(anatomy);
            Assert.IsNotNull(actor.GetComponent<PlayerHumanoidVisualDriver>());
            Assert.IsNotNull(actor.GetComponent<PlayerEyeGazeDetailDriver>());
        }
        finally
        {
            Object.DestroyImmediate(actor);
        }
    }

    [Test]
    public void DetailDrivers_AreUniquePerPlayerObject()
    {
        GameObject actor = new GameObject("HumanoidMicroDetailUniquenessTest");
        try
        {
            actor.AddComponent<ProceduralHumanoidRig>();
            actor.AddComponent<PlayerHumanoidVisualDriver>();
            actor.AddComponent<PlayerMicroMotionDetailDriver>();
            actor.AddComponent<PlayerAnatomicalDetailDriver>();
            actor.AddComponent<PlayerFootGroundingDetailDriver>();
            actor.AddComponent<PlayerFacialMicroMotion>();

            Assert.AreEqual(1, actor.GetComponents<PlayerHumanoidVisualDriver>().Length);
            Assert.AreEqual(1, actor.GetComponents<PlayerMicroMotionDetailDriver>().Length);
            Assert.AreEqual(1, actor.GetComponents<PlayerAnatomicalDetailDriver>().Length);
            Assert.AreEqual(1, actor.GetComponents<PlayerEyeGazeDetailDriver>().Length);
            Assert.AreEqual(1, actor.GetComponents<PlayerFootGroundingDetailDriver>().Length);
            Assert.AreEqual(1, actor.GetComponents<PlayerFacialMicroMotion>().Length);
        }
        finally
        {
            Object.DestroyImmediate(actor);
        }
    }
}
