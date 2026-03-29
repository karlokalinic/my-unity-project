using System;

namespace AC
{
	[Serializable]
	public class ActionEnd
	{
		public ResultAction resultAction;

		public int skipAction;

		public Action skipActionActual;

		public Cutscene linkedCutscene;

		public ActionListAsset linkedAsset;

		public ActionEnd(bool stopAfter = false)
		{
			resultAction = (stopAfter ? ResultAction.Stop : ResultAction.Continue);
			skipAction = -1;
			skipActionActual = null;
			linkedCutscene = null;
			linkedAsset = null;
		}

		public ActionEnd(ActionEnd _actionEnd)
		{
			resultAction = _actionEnd.resultAction;
			skipAction = _actionEnd.skipAction;
			skipActionActual = _actionEnd.skipActionActual;
			linkedCutscene = _actionEnd.linkedCutscene;
			linkedAsset = _actionEnd.linkedAsset;
		}

		public ActionEnd(int _skipAction)
		{
			resultAction = ResultAction.Continue;
			skipAction = _skipAction;
			skipActionActual = null;
			linkedCutscene = null;
			linkedAsset = null;
		}

		public ActionEnd(Action actionAfter)
		{
			resultAction = ResultAction.Skip;
			skipActionActual = actionAfter;
		}

		public ActionEnd(Cutscene cutsceneAfter)
		{
			linkedCutscene = cutsceneAfter;
		}

		public ActionEnd(ActionListAsset actionListAssetAfter)
		{
			linkedAsset = actionListAssetAfter;
		}
	}
}
