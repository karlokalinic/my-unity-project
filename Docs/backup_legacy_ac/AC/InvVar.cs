using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class InvVar : GVar
	{
		public bool limitToCategories;

		public List<int> categoryIDs = new List<int>();

		public InvVar(int[] idArray)
		{
			val = 0;
			floatVal = 0f;
			textVal = string.Empty;
			type = VariableType.Boolean;
			id = 0;
			popUps = null;
			textValLineID = -1;
			popUpsLineID = -1;
			vector3Val = Vector3.zero;
			popUpID = 0;
			foreach (int num in idArray)
			{
				if (id == num)
				{
					id++;
				}
			}
			label = "Property " + (id + 1);
		}

		public InvVar(int _id, VariableType _type)
		{
			val = 0;
			floatVal = 0f;
			textVal = string.Empty;
			type = _type;
			id = _id;
			popUps = null;
			textValLineID = -1;
			popUpsLineID = -1;
			label = string.Empty;
			vector3Val = Vector3.zero;
			popUpID = 0;
		}

		public InvVar(InvVar assetVar)
		{
			val = assetVar.val;
			floatVal = assetVar.floatVal;
			textVal = assetVar.textVal;
			type = assetVar.type;
			id = assetVar.id;
			label = assetVar.label;
			link = assetVar.link;
			pmVar = assetVar.pmVar;
			popUps = assetVar.popUps;
			updateLinkOnStart = assetVar.updateLinkOnStart;
			categoryIDs = assetVar.categoryIDs;
			limitToCategories = assetVar.limitToCategories;
			textValLineID = assetVar.textValLineID;
			popUpsLineID = assetVar.popUpsLineID;
			vector3Val = assetVar.vector3Val;
			popUpID = assetVar.popUpID;
		}

		public void TransferValues(InvVar invVar, bool addValues = false)
		{
			if (addValues)
			{
				val += invVar.val;
				floatVal += invVar.floatVal;
			}
			else
			{
				val = invVar.val;
				floatVal = invVar.floatVal;
			}
			textVal = invVar.textVal;
			textValLineID = invVar.textValLineID;
			vector3Val = invVar.vector3Val;
			popUpsLineID = invVar.popUpsLineID;
			popUpID = invVar.popUpID;
		}

		public string GetDisplayValue(int languageNumber = 0)
		{
			switch (type)
			{
			case VariableType.Integer:
				return val.ToString();
			case VariableType.Float:
				return floatVal.ToString();
			case VariableType.Boolean:
				return (val != 1) ? "False" : "True";
			case VariableType.PopUp:
				if (runtimeTranslations == null || runtimeTranslations.Length == 0)
				{
					CreateRuntimeTranslations();
				}
				return GetPopUpForIndex(val, languageNumber);
			case VariableType.String:
				if (languageNumber > 0)
				{
					return KickStarter.runtimeLanguages.GetTranslation(textVal, textValLineID, languageNumber);
				}
				return textVal;
			case VariableType.Vector3:
				return "(" + vector3Val.x + ", " + vector3Val.y + ", " + vector3Val.z + ")";
			default:
				return string.Empty;
			}
		}

		public override string GetTranslatableString(int index)
		{
			return GetPopUpsString();
		}

		public override int GetTranslationID(int index)
		{
			return popUpsLineID;
		}
	}
}
