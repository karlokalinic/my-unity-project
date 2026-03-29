using System;
using System.Collections.Generic;

namespace AC
{
	[Serializable]
	public class ActionCheck : Action
	{
		public ResultAction resultActionTrue;

		public int skipActionTrue = -1;

		public Action skipActionTrueActual;

		public Cutscene linkedCutsceneTrue;

		public ActionListAsset linkedAssetTrue;

		public ResultAction resultActionFail = ResultAction.Stop;

		public int skipActionFail = -1;

		public Action skipActionFailActual;

		public Cutscene linkedCutsceneFail;

		public ActionListAsset linkedAssetFail;

		public ActionCheck()
		{
			numSockets = 2;
		}

		public override ActionEnd End(List<Action> actions)
		{
			return ProcessResult(CheckCondition(), actions);
		}

		protected ActionEnd ProcessResult(bool result, List<Action> actions)
		{
			if (result)
			{
				return GenerateActionEnd(resultActionTrue, linkedAssetTrue, linkedCutsceneTrue, skipActionTrue, skipActionTrueActual, actions);
			}
			return GenerateActionEnd(resultActionFail, linkedAssetFail, linkedCutsceneFail, skipActionFail, skipActionFailActual, actions);
		}

		public virtual bool CheckCondition()
		{
			return false;
		}

		public void SetOutputs(ActionEnd actionEndOnPass, ActionEnd actionEndOnFail)
		{
			resultActionTrue = actionEndOnPass.resultAction;
			skipActionTrue = actionEndOnPass.skipAction;
			skipActionTrueActual = actionEndOnPass.skipActionActual;
			linkedCutsceneTrue = actionEndOnPass.linkedCutscene;
			linkedAssetTrue = actionEndOnPass.linkedAsset;
			resultActionFail = actionEndOnFail.resultAction;
			skipActionFail = actionEndOnFail.skipAction;
			skipActionFailActual = actionEndOnFail.skipActionActual;
			linkedCutsceneFail = actionEndOnFail.linkedCutscene;
			linkedAssetFail = actionEndOnFail.linkedAsset;
		}
	}
}
