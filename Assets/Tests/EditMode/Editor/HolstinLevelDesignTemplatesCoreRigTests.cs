using System.Reflection;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HolstinLevelDesignTemplatesCoreRigTests
{
    [Test]
    public void EnsureCoreRig_DoesNotAttachBroadRpgSystemsByDefault()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        MethodInfo ensureCoreRig = typeof(HolstinLevelDesignTemplates).GetMethod(
            "EnsureCoreRig",
            BindingFlags.Static | BindingFlags.NonPublic);
        Assert.IsNotNull(ensureCoreRig, "EnsureCoreRig method should exist.");

        ensureCoreRig.Invoke(null, new object[] { new Vector3(0f, 1f, 0f) });

        PlayerMover playerMover = Object.FindAnyObjectByType<PlayerMover>();
        Assert.IsNotNull(playerMover, "PlayerMover should be created.");

        GameObject player = playerMover.gameObject;
        Assert.IsNotNull(player.GetComponent<CharacterController>());
        Assert.IsNotNull(player.GetComponent<PlayerInteraction>());
        Assert.IsNotNull(Object.FindAnyObjectByType<HolstinCameraRig>());
        Assert.IsNotNull(Object.FindAnyObjectByType<HolstinSceneContext>());
        Assert.IsNotNull(Object.FindAnyObjectByType<InspectItemViewer>());
        Assert.IsNotNull(Object.FindAnyObjectByType<InteractionPromptUI>());
        Assert.IsNotNull(Object.FindAnyObjectByType<DialoguePanelUI>());

        Assert.IsNull(player.GetComponent<CharacterStats>());
        Assert.IsNull(player.GetComponent<InventorySystem>());
        Assert.IsNull(player.GetComponent<SkillSystem>());
        Assert.IsNull(player.GetComponent<ReputationSystem>());
        Assert.IsNull(player.GetComponent<CurrencyWallet>());
        Assert.IsNull(player.GetComponent<Damageable>());
        Assert.IsNull(player.GetComponent<RealTimeCombat>());
        Assert.IsNull(player.GetComponent<ExperienceSystem>());
        Assert.IsNull(player.GetComponent<ChoiceHistoryTracker>());
        Assert.IsNull(player.GetComponent<ProceduralHumanoidRig>());
        Assert.IsNull(player.GetComponent<ActiveRagdollMotor>());
        Assert.IsNull(player.GetComponent<DeathRagdollController>());
        Assert.IsNull(player.GetComponent<PlayerAnimationController>());
        Assert.IsNull(player.GetComponent<PlayerReachController>());

        Assert.IsNull(Object.FindAnyObjectByType<DifficultyManager>());
        Assert.IsNull(Object.FindAnyObjectByType<TurnBasedCombatManager>());
        Assert.IsNull(Object.FindAnyObjectByType<HealthStaminaHUD>());
        Assert.IsNull(Object.FindAnyObjectByType<InventoryPanelUI>());
        Assert.IsNull(Object.FindAnyObjectByType<TurnBasedCombatUI>());
        Assert.IsNull(Object.FindAnyObjectByType<ShopWindowUI>());
        Assert.IsNull(Object.FindAnyObjectByType<CombatReticleUI>());

    }
}
