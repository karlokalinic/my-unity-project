using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_global_variables.html")]
	public class GlobalVariables : MonoBehaviour
	{
		public static List<GVar> GetAllVars()
		{
			if ((bool)KickStarter.runtimeVariables)
			{
				return KickStarter.runtimeVariables.globalVars;
			}
			return null;
		}

		public static void BackupAll()
		{
			if (!KickStarter.runtimeVariables)
			{
				return;
			}
			foreach (GVar globalVar in KickStarter.runtimeVariables.globalVars)
			{
				globalVar.BackupValue();
			}
		}

		public static void UploadAll()
		{
			if (!KickStarter.runtimeVariables)
			{
				return;
			}
			foreach (GVar globalVar in KickStarter.runtimeVariables.globalVars)
			{
				globalVar.Upload();
			}
		}

		public static void DownloadAll()
		{
			if (!KickStarter.runtimeVariables)
			{
				return;
			}
			foreach (GVar globalVar in KickStarter.runtimeVariables.globalVars)
			{
				globalVar.Download();
			}
		}

		public static GVar GetVariable(int _id, bool synchronise = false)
		{
			if ((bool)KickStarter.runtimeVariables)
			{
				foreach (GVar globalVar in KickStarter.runtimeVariables.globalVars)
				{
					if (globalVar.id == _id)
					{
						if (synchronise)
						{
							globalVar.Download();
						}
						return globalVar;
					}
				}
			}
			return null;
		}

		public static GVar GetVariable(string _name, bool synchronise = false)
		{
			if ((bool)KickStarter.runtimeVariables)
			{
				foreach (GVar globalVar in KickStarter.runtimeVariables.globalVars)
				{
					if (globalVar.label == _name)
					{
						if (synchronise)
						{
							globalVar.Download();
						}
						return globalVar;
					}
				}
			}
			return null;
		}

		public static int GetIntegerValue(int _id, bool synchronise = true)
		{
			GVar variable = GetVariable(_id, synchronise);
			if (variable != null)
			{
				return variable.val;
			}
			ACDebug.LogWarning("Variable with ID=" + _id + " not found!");
			return 0;
		}

		public static bool GetBooleanValue(int _id, bool synchronise = true)
		{
			GVar variable = GetVariable(_id, synchronise);
			if (variable != null)
			{
				if (variable.val == 1)
				{
					return true;
				}
				return false;
			}
			ACDebug.LogWarning("Variable with ID=" + _id + " not found!");
			return false;
		}

		public static string GetStringValue(int _id, bool synchronise = true, int languageNumber = 0)
		{
			GVar variable = GetVariable(_id, synchronise);
			if (variable != null)
			{
				return variable.GetValue(languageNumber);
			}
			ACDebug.LogWarning("Variable with ID=" + _id + " not found!");
			return string.Empty;
		}

		public static float GetFloatValue(int _id, bool synchronise = true)
		{
			GVar variable = GetVariable(_id, synchronise);
			if (variable != null)
			{
				return variable.floatVal;
			}
			ACDebug.LogWarning("Variable with ID=" + _id + " not found!");
			return 0f;
		}

		public static Vector3 GetVector3Value(int _id, bool synchronise = true)
		{
			GVar variable = GetVariable(_id, synchronise);
			if (variable != null)
			{
				return variable.vector3Val;
			}
			ACDebug.LogWarning("Variable with ID=" + _id + " not found!");
			return Vector3.zero;
		}

		public static string GetPopupValue(int _id, bool synchronise = true, int languageNumber = 0)
		{
			GVar variable = GetVariable(_id, synchronise);
			if (variable != null)
			{
				return variable.GetPopUpForIndex(variable.val, languageNumber);
			}
			ACDebug.LogWarning("Variable with ID=" + _id + " not found!");
			return string.Empty;
		}

		public static void SetIntegerValue(int _id, int _value, bool synchronise = true)
		{
			GVar variable = GetVariable(_id);
			if (variable != null)
			{
				variable.SetValue(_value);
				if (synchronise)
				{
					variable.Upload();
				}
			}
		}

		public static void SetBooleanValue(int _id, bool _value, bool synchronise = true)
		{
			GVar variable = GetVariable(_id);
			if (variable != null)
			{
				variable.SetValue(_value ? 1 : 0);
				if (synchronise)
				{
					variable.Upload();
				}
			}
		}

		public static void SetStringValue(int _id, string _value, bool synchronise = true)
		{
			GVar variable = GetVariable(_id);
			if (variable != null)
			{
				variable.SetStringValue(_value);
				if (synchronise)
				{
					variable.Upload();
				}
			}
		}

		public static void SetFloatValue(int _id, float _value, bool synchronise = true)
		{
			GVar variable = GetVariable(_id);
			if (variable != null)
			{
				variable.SetFloatValue(_value);
				if (synchronise)
				{
					variable.Upload();
				}
			}
		}

		public static void SetVector3Value(int _id, Vector3 _value, bool synchronise = true)
		{
			GVar variable = GetVariable(_id);
			if (variable != null)
			{
				variable.SetVector3Value(_value);
				if (synchronise)
				{
					variable.Upload();
				}
			}
		}

		public static void SetPopupValue(int _id, int _value, bool synchronise = true)
		{
			SetIntegerValue(_id, _value, synchronise);
		}
	}
}
