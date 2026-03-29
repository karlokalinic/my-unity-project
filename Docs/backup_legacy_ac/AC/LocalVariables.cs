using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_local_variables.html")]
	public class LocalVariables : MonoBehaviour, ITranslatable
	{
		[HideInInspector]
		public List<GVar> localVars = new List<GVar>();

		[HideInInspector]
		public List<VarPreset> varPresets = new List<VarPreset>();

		public void OnStart()
		{
			foreach (GVar localVar in localVars)
			{
				localVar.CreateRuntimeTranslations();
			}
		}

		public void BackupAllValues()
		{
			foreach (GVar localVar in localVars)
			{
				localVar.BackupValue();
			}
		}

		public void AssignFromPreset(VarPreset varPreset)
		{
			foreach (GVar localVar in localVars)
			{
				foreach (PresetValue presetValue in varPreset.presetValues)
				{
					if (localVar.id == presetValue.id)
					{
						localVar.val = presetValue.val;
						localVar.floatVal = presetValue.floatVal;
						localVar.textVal = presetValue.textVal;
						localVar.vector3Val = presetValue.vector3Val;
					}
				}
			}
		}

		public void AssignFromPreset(int varPresetID)
		{
			if (varPresets == null)
			{
				return;
			}
			foreach (VarPreset varPreset in varPresets)
			{
				if (varPreset.ID == varPresetID)
				{
					AssignFromPreset(varPreset);
					break;
				}
			}
		}

		public VarPreset GetPreset(int varPresetID)
		{
			if (varPresets == null)
			{
				return null;
			}
			foreach (VarPreset varPreset in varPresets)
			{
				if (varPreset.ID == varPresetID)
				{
					return varPreset;
				}
			}
			return null;
		}

		public static GVar GetVariable(int _id, LocalVariables localVariables = null)
		{
			if (localVariables == null)
			{
				localVariables = KickStarter.localVariables;
			}
			if ((bool)localVariables)
			{
				foreach (GVar localVar in localVariables.localVars)
				{
					if (localVar.id == _id)
					{
						return localVar;
					}
				}
			}
			return null;
		}

		public static GVar GetVariable(string _name, LocalVariables localVariables = null)
		{
			if (localVariables == null)
			{
				localVariables = KickStarter.localVariables;
			}
			if ((bool)localVariables)
			{
				foreach (GVar localVar in localVariables.localVars)
				{
					if (localVar.label == _name)
					{
						return localVar;
					}
				}
			}
			return null;
		}

		public static List<GVar> GetAllVars()
		{
			if ((bool)KickStarter.localVariables)
			{
				return KickStarter.localVariables.localVars;
			}
			return null;
		}

		public static int GetIntegerValue(int _id)
		{
			return GetVariable(_id).val;
		}

		public static bool GetBooleanValue(int _id)
		{
			if (GetVariable(_id).val == 1)
			{
				return true;
			}
			return false;
		}

		public static string GetStringValue(int _id, int lanugageNumber = 0)
		{
			return GetVariable(_id).GetValue(lanugageNumber);
		}

		public static float GetFloatValue(int _id)
		{
			return GetVariable(_id).floatVal;
		}

		public static Vector3 GetVector3Value(int _id)
		{
			return GetVariable(_id).vector3Val;
		}

		public static string GetPopupValue(int _id, int languageNumber = 0)
		{
			return GetVariable(_id).GetValue(languageNumber);
		}

		public static void SetIntegerValue(int _id, int _value)
		{
			GetVariable(_id).SetValue(_value);
		}

		public static void SetBooleanValue(int _id, bool _value)
		{
			if (_value)
			{
				GetVariable(_id).SetValue(1);
			}
			else
			{
				GetVariable(_id).SetValue(0);
			}
		}

		public static void SetStringValue(int _id, string _value)
		{
			GetVariable(_id).SetStringValue(_value);
		}

		public static void SetFloatValue(int _id, float _value)
		{
			GetVariable(_id).SetFloatValue(_value);
		}

		public static void SetVector3Value(int _id, Vector3 _value)
		{
			GetVariable(_id).SetVector3Value(_value);
		}

		public static void SetPopupValue(int _id, int _value)
		{
			GetVariable(_id).SetValue(_value);
		}

		public string GetTranslatableString(int index)
		{
			return localVars[index].GetTranslatableString(index);
		}

		public int GetTranslationID(int index)
		{
			return localVars[index].GetTranslationID(index);
		}
	}
}
