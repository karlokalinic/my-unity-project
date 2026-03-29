using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class VariablesManager : ScriptableObject
	{
		public List<GVar> vars = new List<GVar>();

		public List<VarPreset> varPresets = new List<VarPreset>();

		public bool updateRuntime = true;

		public List<PopUpLabelData> popUpLabelData = new List<PopUpLabelData>();

		public GVar GetVariable(int _id)
		{
			foreach (GVar var in vars)
			{
				if (var.id == _id)
				{
					return var;
				}
			}
			return null;
		}

		public PopUpLabelData GetPopUpLabelData(int ID)
		{
			if (ID > 0)
			{
				foreach (PopUpLabelData popUpLabelDatum in popUpLabelData)
				{
					if (popUpLabelDatum.ID == ID)
					{
						return popUpLabelDatum;
					}
				}
			}
			return null;
		}
	}
}
