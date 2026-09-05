using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class PitchBlackEncounterPresetTests
{
    [Test]
    public void AbominableSnowmanPreset_UsesOppositeRoomAndDedicatedAssetPath()
    {
        GameObject host = new GameObject("EncounterPresetTest");
        try
        {
            RakeEncounterController controller = host.AddComponent<RakeEncounterController>();
            Type presetType = typeof(RakeEncounterController).GetNestedType(
                "EncounterPreset",
                BindingFlags.NonPublic);
            MethodInfo configurePreset = typeof(RakeEncounterController).GetMethod(
                "ConfigurePreset",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.IsNotNull(presetType);
            Assert.IsNotNull(configurePreset);

            object snowmanPreset = Enum.Parse(presetType, "AbominableSnowman");
            configurePreset.Invoke(controller, new[] { snowmanPreset });

            Assert.AreEqual(
                "ThirdParty/AbominableSnowman/AbominableSnowman",
                ReadPrivate<string>(controller, "resourcePath"));
            Assert.AreEqual("The Abominable Snowman", ReadPrivate<string>(controller, "creatureDisplayName"));
            Assert.IsTrue(ReadPrivate<bool>(controller, "carveBoardingHouseBackWall"));

            Vector3 roomOffset = ReadPrivate<Vector3>(controller, "roomOffset");
            Vector3 depthAxis = ReadPrivate<Vector3>(controller, "configuredDepthAxis");
            Assert.Greater(roomOffset.z, 0f, "Snowman room must be placed on the opposite Z side of the boarding house.");
            Assert.Greater(depthAxis.z, 0.9f, "Snowman encounter must progress away from the opposite doorway.");
            Assert.Less(ReadPrivate<float>(controller, "doorOpenAngleY"), 0f, "Opposite door swing should be mirrored.");
            Assert.Greater(ReadPrivate<float>(controller, "creatureHeight"), 2.3f);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(host);
        }
    }

    [Test]
    public void RakePreset_RemainsOnOriginalSideWithOriginalAssetPath()
    {
        GameObject host = new GameObject("RakePresetTest");
        try
        {
            RakeEncounterController controller = host.AddComponent<RakeEncounterController>();
            Type presetType = typeof(RakeEncounterController).GetNestedType(
                "EncounterPreset",
                BindingFlags.NonPublic);
            MethodInfo configurePreset = typeof(RakeEncounterController).GetMethod(
                "ConfigurePreset",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.IsNotNull(presetType);
            Assert.IsNotNull(configurePreset);

            object rakePreset = Enum.Parse(presetType, "Rake");
            configurePreset.Invoke(controller, new[] { rakePreset });

            Assert.AreEqual("ThirdParty/TheRake/TheRake", ReadPrivate<string>(controller, "resourcePath"));
            Assert.IsFalse(ReadPrivate<bool>(controller, "carveBoardingHouseBackWall"));
            Assert.Less(ReadPrivate<Vector3>(controller, "roomOffset").z, 0f);
            Assert.Less(ReadPrivate<Vector3>(controller, "configuredDepthAxis").z, -0.9f);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(host);
        }
    }

    private static T ReadPrivate<T>(RakeEncounterController controller, string fieldName)
    {
        FieldInfo field = typeof(RakeEncounterController).GetField(
            fieldName,
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(field, $"Missing private field '{fieldName}'.");
        return (T)field.GetValue(controller);
    }
}
