using NUnit.Framework;
using UnityEngine;

public class PressureManagerTests
{
    [Test]
    public void SetStage_ClampsToRange_AndUpdatesSliceState()
    {
        GameObject stateGo = new GameObject("PressureTests_State");
        SliceState state = stateGo.AddComponent<SliceState>();

        GameObject pressureGo = new GameObject("PressureTests_Manager");
        PressureManager manager = pressureGo.AddComponent<PressureManager>();
        manager.Configure(state, false);

        manager.SetStage(99, "test");
        Assert.AreEqual(3, state.PressureStage);
        Assert.AreEqual(3, manager.CurrentStage);

        manager.SetStage(-5, "test");
        Assert.AreEqual(0, state.PressureStage);
        Assert.AreEqual(0, manager.CurrentStage);

        Object.DestroyImmediate(pressureGo);
        Object.DestroyImmediate(stateGo);
    }

    [Test]
    public void MilestoneRule_AdvancesPressureStage()
    {
        GameObject stateGo = new GameObject("PressureTests_State");
        SliceState state = stateGo.AddComponent<SliceState>();

        GameObject pressureGo = new GameObject("PressureTests_Manager");
        PressureManager manager = pressureGo.AddComponent<PressureManager>();
        manager.Configure(state, false);

        InfectionDirector.NotifyMilestoneGlobal("inspect_exterior_note");
        Assert.AreEqual(1, manager.CurrentStage);

        InfectionDirector.NotifyMilestoneGlobal("unlock_interior_gate");
        Assert.AreEqual(2, manager.CurrentStage);

        InfectionDirector.NotifyMilestoneGlobal("console_service_unlock");
        Assert.AreEqual(3, manager.CurrentStage);

        Object.DestroyImmediate(pressureGo);
        Object.DestroyImmediate(stateGo);
    }

    [Test]
    public void InfectionBridge_MapsPressureToNodeStage()
    {
        GameObject stateGo = new GameObject("PressureTests_State");
        SliceState state = stateGo.AddComponent<SliceState>();

        GameObject nodeGo = new GameObject("PressureTests_Node");
        InfectionNode node = nodeGo.AddComponent<InfectionNode>();
        node.Configure("node", InfectionStage.Dormant, null);

        GameObject pressureGo = new GameObject("PressureTests_Manager");
        PressureManager manager = pressureGo.AddComponent<PressureManager>();
        manager.Configure(state, true);

        manager.SetStage(1, "test");
        Assert.AreEqual(InfectionStage.Active, node.CurrentStage);

        manager.SetStage(3, "test");
        Assert.AreEqual(InfectionStage.Overrun, node.CurrentStage);

        Object.DestroyImmediate(pressureGo);
        Object.DestroyImmediate(nodeGo);
        Object.DestroyImmediate(stateGo);
    }
}
