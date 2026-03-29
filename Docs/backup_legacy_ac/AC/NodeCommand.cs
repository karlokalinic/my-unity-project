using System;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class NodeCommand
	{
		public Cutscene cutscene;

		public ActionListAsset actionListAsset;

		public int parameterID;

		public bool pausesCharacter = true;

		public NodeCommand()
		{
			cutscene = null;
			actionListAsset = null;
			parameterID = -1;
			pausesCharacter = true;
		}

		public void SetParameter(ActionListSource source, GameObject gameObject)
		{
			if (source == ActionListSource.InScene && cutscene != null)
			{
				if (parameterID >= 0 && cutscene.NumParameters > parameterID)
				{
					ActionParameter parameter = cutscene.GetParameter(parameterID);
					if (parameter != null)
					{
						parameter.SetValue(gameObject);
					}
				}
				if (!pausesCharacter)
				{
					cutscene.Interact();
				}
			}
			else
			{
				if (source != ActionListSource.AssetFile || !(actionListAsset != null))
				{
					return;
				}
				if (parameterID >= 0 && actionListAsset.NumParameters > parameterID)
				{
					int value = 0;
					if ((bool)gameObject.GetComponent<ConstantID>())
					{
						value = gameObject.GetComponent<ConstantID>().constantID;
					}
					else
					{
						ACDebug.LogWarning(gameObject.name + " requires a ConstantID script component!", gameObject);
					}
					ActionParameter parameter2 = actionListAsset.GetParameter(parameterID);
					if (parameter2 != null)
					{
						parameter2.SetValue(value);
					}
				}
				if (!pausesCharacter)
				{
					actionListAsset.Interact();
				}
			}
		}
	}
}
