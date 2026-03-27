using NUnit.Framework;
using UnityEngine;

public class SliceStateTests
{
    [Test]
    public void FlagsAndCollections_WorkAsExpected()
    {
        GameObject go = new GameObject("SliceStateTests_State");
        SliceState state = go.AddComponent<SliceState>();

        state.SetCurrentObjective("objective_a");
        state.SetBoolFlag("door_open", true);
        state.SetIntFlag("pressure_hint", 2);
        state.SetStringFlag("chapter", "one");
        state.AcquireKeyItem("old_key");
        state.MarkMilestone("inspect_exterior_note");

        Assert.AreEqual("objective_a", state.CurrentObjectiveId);
        Assert.IsTrue(state.GetBoolFlag("door_open"));
        Assert.AreEqual(2, state.GetIntFlag("pressure_hint"));
        Assert.AreEqual("one", state.GetStringFlag("chapter"));
        Assert.IsTrue(state.HasKeyItem("old_key"));
        Assert.IsTrue(state.HasMilestone("inspect_exterior_note"));

        Object.DestroyImmediate(go);
    }

    [Test]
    public void CheckpointSaveAndRestore_RestoresTransform()
    {
        GameObject go = new GameObject("SliceStateTests_State");
        SliceState state = go.AddComponent<SliceState>();
        CharacterController controller = go.AddComponent<CharacterController>();

        Vector3 checkpointPosition = new Vector3(4f, 1f, -3f);
        Quaternion checkpointRotation = Quaternion.Euler(0f, 45f, 0f);
        state.SaveCheckpoint(checkpointPosition, checkpointRotation, "checkpoint_a");

        go.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

        bool restored = state.RestoreCheckpoint(go.transform, controller);
        Assert.IsTrue(restored);
        Assert.AreEqual(checkpointPosition, go.transform.position);
        Assert.AreEqual(checkpointRotation.eulerAngles.y, go.transform.rotation.eulerAngles.y, 0.1f);

        Object.DestroyImmediate(go);
    }
}
