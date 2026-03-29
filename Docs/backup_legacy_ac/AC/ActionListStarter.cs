using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[AddComponentMenu("Adventure Creator/Logic/ActionList starter")]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_action_list_starter.html")]
	public class ActionListStarter : SetParametersBase
	{
		[SerializeField]
		protected ActionListSource actionListSource;

		[SerializeField]
		protected ActionList actionList;

		[SerializeField]
		protected ActionListAsset actionListAsset;

		[SerializeField]
		protected bool runOnStart;

		[SerializeField]
		protected bool runOnLoad;

		[SerializeField]
		protected bool setParameters;

		[SerializeField]
		protected bool runMultipleTimes;

		[SerializeField]
		protected bool runInstantly;

		protected void OnEnable()
		{
			EventManager.OnStartScene += OnStartScene;
			EventManager.OnAfterChangeScene += OnAfterChangeScene;
		}

		protected void OnDisable()
		{
			EventManager.OnStartScene -= OnStartScene;
		}

		public override List<ActionListAsset> GetReferencedActionListAssets()
		{
			List<ActionListAsset> referencedActionListAssets = base.GetReferencedActionListAssets();
			if (actionListSource == ActionListSource.AssetFile && actionListAsset != null)
			{
				referencedActionListAssets.Add(actionListAsset);
			}
			return referencedActionListAssets;
		}

		[ContextMenu("Run now")]
		public void RunActionList()
		{
			if (Application.isPlaying)
			{
				RunActionLists();
			}
		}

		protected void OnStartScene()
		{
			if (runOnStart)
			{
				RunActionLists();
			}
		}

		protected void OnAfterChangeScene(LoadingGame loadingGame)
		{
			if (runOnLoad && loadingGame != LoadingGame.No)
			{
				RunActionLists();
			}
		}

		protected void RunActionLists()
		{
			switch (actionListSource)
			{
			case ActionListSource.InScene:
				if (actionList != null)
				{
					if (setParameters)
					{
						AssignParameterValues(actionList);
					}
					if (actionList.IsSkippable() && runInstantly)
					{
						actionList.Skip();
					}
					else
					{
						actionList.Interact();
					}
				}
				break;
			case ActionListSource.AssetFile:
				if (!(actionListAsset != null))
				{
					break;
				}
				if (setParameters && runMultipleTimes)
				{
					if (actionListAsset.canRunMultipleInstances)
					{
						for (int i = 0; i < successiveGUIData.Length + 1; i++)
						{
							AssignParameterValues(actionListAsset, i);
							if (actionListAsset.IsSkippable() && runInstantly)
							{
								AdvGame.SkipActionListAsset(actionListAsset);
							}
							else
							{
								actionListAsset.Interact();
							}
						}
					}
					else
					{
						ACDebug.LogWarning(string.Concat("Cannot set run multiple parameter configurations because the ActionList asset '", actionListAsset, "' has 'Can run multiple instances?' unchecked."), actionListAsset);
					}
				}
				else
				{
					if (setParameters)
					{
						AssignParameterValues(actionListAsset);
					}
					if (actionListAsset.IsSkippable() && runInstantly)
					{
						AdvGame.SkipActionListAsset(actionListAsset);
					}
					else
					{
						actionListAsset.Interact();
					}
				}
				break;
			}
		}
	}
}
