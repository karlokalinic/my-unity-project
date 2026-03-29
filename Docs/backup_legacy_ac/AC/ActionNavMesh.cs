using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionNavMesh : Action
	{
		public int constantID;

		public int parameterID = -1;

		public int replaceConstantID;

		public int replaceParameterID = -1;

		public NavigationMesh newNavMesh;

		public SortingMap sortingMap;

		public PlayerStart playerStart;

		public Cutscene cutscene;

		public TintMap tintMap;

		public SceneSetting sceneSetting;

		public ChangeNavMeshMethod changeNavMeshMethod;

		public InvAction holeAction;

		public PolygonCollider2D hole;

		public PolygonCollider2D replaceHole;

		protected SceneSettings sceneSettings;

		protected NavigationMesh runtimeNewNavMesh;

		protected PolygonCollider2D runtimeHole;

		protected PolygonCollider2D runtimeReplaceHole;

		protected PlayerStart runtimePlayerStart;

		protected SortingMap runtimeSortingMap;

		protected TintMap runtimeTintMap;

		protected Cutscene runtimeCutscene;

		public ActionNavMesh()
		{
			isDisplayed = true;
			category = ActionCategory.Scene;
			title = "Change setting";
			description = "Changes any of the following scene parameters: NavMesh, Default PlayerStart, Sorting Map, Tint Map, Cutscene On Load, and Cutscene On Start. When the NavMesh is a Polygon Collider, this Action can also be used to add or remove holes from it.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			if (sceneSettings == null)
			{
				return;
			}
			switch (sceneSetting)
			{
			case SceneSetting.DefaultNavMesh:
				if (KickStarter.sceneSettings.navigationMethod == AC_NavigationMethod.PolygonCollider && changeNavMeshMethod == ChangeNavMeshMethod.ChangeNumberOfHoles)
				{
					runtimeHole = AssignFile(parameters, parameterID, constantID, hole);
					runtimeReplaceHole = AssignFile(parameters, replaceParameterID, replaceConstantID, replaceHole);
					runtimeNewNavMesh = null;
				}
				else
				{
					runtimeHole = null;
					runtimeReplaceHole = null;
					runtimeNewNavMesh = AssignFile(parameters, parameterID, constantID, newNavMesh);
				}
				break;
			case SceneSetting.DefaultPlayerStart:
				runtimePlayerStart = AssignFile(parameters, parameterID, constantID, playerStart);
				break;
			case SceneSetting.SortingMap:
				runtimeSortingMap = AssignFile(parameters, parameterID, constantID, sortingMap);
				break;
			case SceneSetting.TintMap:
				runtimeTintMap = AssignFile(parameters, parameterID, constantID, tintMap);
				break;
			case SceneSetting.OnStartCutscene:
			case SceneSetting.OnLoadCutscene:
				runtimeCutscene = AssignFile(parameters, parameterID, constantID, cutscene);
				break;
			}
		}

		public override void AssignParentList(ActionList actionList)
		{
			if (actionList != null)
			{
				sceneSettings = UnityVersionHandler.GetSceneSettingsOfGameObject(actionList.gameObject);
			}
			if (sceneSettings == null)
			{
				sceneSettings = KickStarter.sceneSettings;
			}
			base.AssignParentList(actionList);
		}

		public override float Run()
		{
			switch (sceneSetting)
			{
			case SceneSetting.DefaultNavMesh:
				if (sceneSettings.navigationMethod == AC_NavigationMethod.PolygonCollider && changeNavMeshMethod == ChangeNavMeshMethod.ChangeNumberOfHoles)
				{
					if (runtimeHole != null)
					{
						NavigationMesh navMesh = sceneSettings.navMesh;
						switch (holeAction)
						{
						case InvAction.Add:
							navMesh.AddHole(runtimeHole);
							break;
						case InvAction.Remove:
							navMesh.RemoveHole(runtimeHole);
							break;
						case InvAction.Replace:
							navMesh.AddHole(runtimeHole);
							navMesh.RemoveHole(runtimeReplaceHole);
							break;
						}
					}
				}
				else if (runtimeNewNavMesh != null)
				{
					NavigationMesh navMesh2 = sceneSettings.navMesh;
					navMesh2.TurnOff();
					runtimeNewNavMesh.TurnOn();
					sceneSettings.navMesh = runtimeNewNavMesh;
					runtimeNewNavMesh.TurnOff();
					runtimeNewNavMesh.TurnOn();
					if (runtimeNewNavMesh.GetComponent<ConstantID>() == null)
					{
						LogWarning("Warning: Changing to new NavMesh with no ConstantID - change will not be recognised by saved games.", runtimeNewNavMesh);
					}
				}
				foreach (Char character in KickStarter.stateHandler.Characters)
				{
					character.RecalculateActivePathfind();
				}
				break;
			case SceneSetting.DefaultPlayerStart:
				if (runtimePlayerStart != null)
				{
					sceneSettings.defaultPlayerStart = runtimePlayerStart;
					if (runtimePlayerStart.GetComponent<ConstantID>() == null)
					{
						LogWarning("Warning: Changing to new default PlayerStart with no ConstantID - change will not be recognised by saved games.", runtimePlayerStart);
					}
				}
				break;
			case SceneSetting.SortingMap:
				if (runtimeSortingMap != null)
				{
					sceneSettings.sortingMap = runtimeSortingMap;
					sceneSettings.UpdateAllSortingMaps();
					if (runtimeSortingMap.GetComponent<ConstantID>() == null)
					{
						LogWarning("Warning: Changing to new SortingMap with no ConstantID - change will not be recognised by saved games.", runtimeSortingMap);
					}
				}
				break;
			case SceneSetting.TintMap:
				if (runtimeTintMap != null)
				{
					sceneSettings.tintMap = runtimeTintMap;
					FollowTintMap[] array = UnityEngine.Object.FindObjectsOfType(typeof(FollowTintMap)) as FollowTintMap[];
					FollowTintMap[] array2 = array;
					foreach (FollowTintMap followTintMap in array2)
					{
						followTintMap.ResetTintMap();
					}
					if (runtimeTintMap.GetComponent<ConstantID>() == null)
					{
						LogWarning("Warning: Changing to new TintMap with no ConstantID - change will not be recognised by saved games.", runtimeTintMap);
					}
				}
				break;
			case SceneSetting.OnLoadCutscene:
				if (runtimeCutscene != null)
				{
					sceneSettings.cutsceneOnLoad = runtimeCutscene;
					if (sceneSettings.actionListSource == ActionListSource.AssetFile)
					{
						LogWarning("Warning: As the Scene Manager relies on asset files for its cutscenes, changes made with the 'Scene: Change setting' Action will not be felt.");
					}
					else if (runtimeCutscene.GetComponent<ConstantID>() == null)
					{
						LogWarning("Warning: Changing to Cutscene On Load with no ConstantID - change will not be recognised by saved games.", runtimeCutscene);
					}
				}
				break;
			case SceneSetting.OnStartCutscene:
				if (runtimeCutscene != null)
				{
					sceneSettings.cutsceneOnStart = runtimeCutscene;
					if (sceneSettings.actionListSource == ActionListSource.AssetFile)
					{
						LogWarning("Warning: As the Scene Manager relies on asset files for its cutscenes, changes made with the 'Scene: Change setting' Action will not be felt.");
					}
					else if (runtimeCutscene.GetComponent<ConstantID>() == null)
					{
						LogWarning("Warning: Changing to Cutscene On Start with no ConstantID - change will not be recognised by saved games.", runtimeCutscene);
					}
				}
				break;
			}
			return 0f;
		}

		public static ActionNavMesh CreateNew_ChangeDefaultNavMesh(NavigationMesh newNavMesh)
		{
			ActionNavMesh actionNavMesh = ScriptableObject.CreateInstance<ActionNavMesh>();
			actionNavMesh.sceneSetting = SceneSetting.DefaultNavMesh;
			actionNavMesh.changeNavMeshMethod = ChangeNavMeshMethod.ChangeNavMesh;
			actionNavMesh.newNavMesh = newNavMesh;
			return actionNavMesh;
		}

		public static ActionNavMesh CreateNew_AddNavMeshHole(PolygonCollider2D holeToAdd)
		{
			ActionNavMesh actionNavMesh = ScriptableObject.CreateInstance<ActionNavMesh>();
			actionNavMesh.sceneSetting = SceneSetting.DefaultNavMesh;
			actionNavMesh.changeNavMeshMethod = ChangeNavMeshMethod.ChangeNumberOfHoles;
			actionNavMesh.holeAction = InvAction.Add;
			actionNavMesh.hole = holeToAdd;
			return actionNavMesh;
		}

		public static ActionNavMesh CreateNew_RemoveNavMeshHole(PolygonCollider2D holeToRemove)
		{
			ActionNavMesh actionNavMesh = ScriptableObject.CreateInstance<ActionNavMesh>();
			actionNavMesh.sceneSetting = SceneSetting.DefaultNavMesh;
			actionNavMesh.changeNavMeshMethod = ChangeNavMeshMethod.ChangeNumberOfHoles;
			actionNavMesh.holeAction = InvAction.Remove;
			actionNavMesh.hole = holeToRemove;
			return actionNavMesh;
		}

		public static ActionNavMesh CreateNew_ChangeDefaultPlayerStart(PlayerStart newPlayerStart)
		{
			ActionNavMesh actionNavMesh = ScriptableObject.CreateInstance<ActionNavMesh>();
			actionNavMesh.sceneSetting = SceneSetting.DefaultPlayerStart;
			actionNavMesh.playerStart = newPlayerStart;
			return actionNavMesh;
		}

		public static ActionNavMesh CreateNew_ChangeSortingMap(SortingMap newSortingMap)
		{
			ActionNavMesh actionNavMesh = ScriptableObject.CreateInstance<ActionNavMesh>();
			actionNavMesh.sceneSetting = SceneSetting.SortingMap;
			actionNavMesh.sortingMap = newSortingMap;
			return actionNavMesh;
		}

		public static ActionNavMesh CreateNew_ChangeTintMap(TintMap newTintMap)
		{
			ActionNavMesh actionNavMesh = ScriptableObject.CreateInstance<ActionNavMesh>();
			actionNavMesh.sceneSetting = SceneSetting.TintMap;
			actionNavMesh.tintMap = newTintMap;
			return actionNavMesh;
		}

		public static ActionNavMesh CreateNew_ChangeCutsceneOnLoad(Cutscene newCutscene)
		{
			ActionNavMesh actionNavMesh = ScriptableObject.CreateInstance<ActionNavMesh>();
			actionNavMesh.sceneSetting = SceneSetting.OnLoadCutscene;
			actionNavMesh.cutscene = newCutscene;
			return actionNavMesh;
		}

		public static ActionNavMesh CreateNew_ChangeCutsceneOnStart(Cutscene newCutscene)
		{
			ActionNavMesh actionNavMesh = ScriptableObject.CreateInstance<ActionNavMesh>();
			actionNavMesh.sceneSetting = SceneSetting.OnStartCutscene;
			actionNavMesh.cutscene = newCutscene;
			return actionNavMesh;
		}
	}
}
