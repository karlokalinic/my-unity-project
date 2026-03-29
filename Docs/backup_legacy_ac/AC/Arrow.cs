using System;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class Arrow
	{
		public bool isPresent;

		public Cutscene linkedCutscene;

		public ActionListAsset linkedActionList;

		public Texture2D texture;

		public Rect rect;

		public Arrow()
		{
			isPresent = false;
		}

		public void Run(ActionListSource actionListSource)
		{
			switch (actionListSource)
			{
			case ActionListSource.AssetFile:
				if (linkedActionList != null)
				{
					linkedActionList.Interact();
				}
				break;
			case ActionListSource.InScene:
				if (linkedCutscene != null)
				{
					linkedCutscene.Interact();
				}
				break;
			}
		}

		public void Draw()
		{
			if ((bool)texture)
			{
				GUI.DrawTexture(rect, texture, ScaleMode.StretchToFill, true);
			}
		}
	}
}
