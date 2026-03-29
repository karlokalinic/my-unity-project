using System;

namespace AC
{
	[Serializable]
	public class InvInteraction
	{
		public ActionListAsset actionList;

		public CursorIcon icon;

		public InvInteraction(CursorIcon _icon)
		{
			icon = _icon;
			actionList = null;
		}
	}
}
