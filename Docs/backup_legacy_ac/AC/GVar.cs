using System;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class GVar : ITranslatable
	{
		public string label;

		public int id;

		public VariableType type;

		public int val;

		public float floatVal;

		public string textVal;

		public string[] popUps;

		public Vector3 vector3Val;

		public VarLink link;

		public string pmVar;

		public bool updateLinkOnStart;

		public bool canTranslate = true;

		public int popUpID;

		public int textValLineID = -1;

		public int popUpsLineID = -1;

		private float backupFloatVal;

		private int backupVal;

		protected string[] runtimeTranslations;

		public int IntegerValue
		{
			get
			{
				return val;
			}
			set
			{
				int num = val;
				val = value;
				if (num != val)
				{
					KickStarter.eventManager.Call_OnVariableChange(this);
				}
			}
		}

		public bool BooleanValue
		{
			get
			{
				return val == 1;
			}
			set
			{
				int num = val;
				val = (value ? 1 : 0);
				if (num != val)
				{
					KickStarter.eventManager.Call_OnVariableChange(this);
				}
			}
		}

		public float FloatValue
		{
			get
			{
				return floatVal;
			}
			set
			{
				float a = floatVal;
				floatVal = value;
				if (!Mathf.Approximately(a, floatVal))
				{
					KickStarter.eventManager.Call_OnVariableChange(this);
				}
			}
		}

		public string TextValue
		{
			get
			{
				return textVal;
			}
			set
			{
				string text = textVal;
				textVal = value;
				if (text != textVal)
				{
					KickStarter.eventManager.Call_OnVariableChange(this);
				}
			}
		}

		public Vector3 Vector3Value
		{
			get
			{
				return vector3Val;
			}
			set
			{
				Vector3 vector = vector3Val;
				vector3Val = value;
				if (vector != vector3Val)
				{
					KickStarter.eventManager.Call_OnVariableChange(this);
				}
			}
		}

		public GVar()
		{
		}

		public GVar(int[] idArray)
		{
			val = 0;
			floatVal = 0f;
			textVal = string.Empty;
			type = VariableType.Boolean;
			link = VarLink.None;
			pmVar = string.Empty;
			popUps = null;
			updateLinkOnStart = false;
			backupVal = 0;
			backupFloatVal = 0f;
			textValLineID = -1;
			popUpsLineID = -1;
			canTranslate = true;
			vector3Val = Vector3.zero;
			popUpID = 0;
			AssignUniqueID(idArray);
			label = "Variable " + (id + 1);
		}

		public GVar(GVar assetVar)
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
			backupVal = assetVar.val;
			backupFloatVal = assetVar.floatVal;
			textValLineID = assetVar.textValLineID;
			popUpsLineID = assetVar.popUpsLineID;
			canTranslate = assetVar.canTranslate;
			vector3Val = assetVar.vector3Val;
			popUpID = assetVar.popUpID;
		}

		public int AssignUniqueID(int[] idArray)
		{
			id = 0;
			if (idArray != null)
			{
				foreach (int num in idArray)
				{
					if (id == num)
					{
						id++;
					}
				}
			}
			return id;
		}

		public void Download(VariableLocation _location = VariableLocation.Global, Variables _variables = null)
		{
			if (_location == VariableLocation.Local)
			{
				return;
			}
			if (link == VarLink.PlaymakerVariable && !string.IsNullOrEmpty(pmVar))
			{
				if (!PlayMakerIntegration.IsDefinePresent())
				{
					return;
				}
				if (_location != VariableLocation.Component)
				{
					_variables = null;
				}
				if (_location != VariableLocation.Component || !(_variables == null))
				{
					switch (type)
					{
					case VariableType.Integer:
					case VariableType.PopUp:
						SetValue(PlayMakerIntegration.GetInt(pmVar, _variables));
						break;
					case VariableType.Boolean:
					{
						bool flag = PlayMakerIntegration.GetBool(pmVar, _variables);
						SetValue(flag ? 1 : 0);
						break;
					}
					case VariableType.String:
						SetStringValue(PlayMakerIntegration.GetString(pmVar, _variables));
						break;
					case VariableType.Float:
						SetFloatValue(PlayMakerIntegration.GetFloat(pmVar, _variables));
						break;
					case VariableType.Vector3:
						SetVector3Value(PlayMakerIntegration.GetVector3(pmVar, _variables));
						break;
					}
				}
			}
			else if (link == VarLink.CustomScript)
			{
				KickStarter.eventManager.Call_OnDownloadVariable(this, _variables);
			}
		}

		public void Upload(VariableLocation _location = VariableLocation.Global, Variables _variables = null)
		{
			if (_location == VariableLocation.Local)
			{
				return;
			}
			if (link == VarLink.PlaymakerVariable && !string.IsNullOrEmpty(pmVar))
			{
				if (!PlayMakerIntegration.IsDefinePresent())
				{
					return;
				}
				if (_location != VariableLocation.Component)
				{
					_variables = null;
				}
				if (_location != VariableLocation.Component || !(_variables == null))
				{
					switch (type)
					{
					case VariableType.Integer:
					case VariableType.PopUp:
						PlayMakerIntegration.SetInt(pmVar, val, _variables);
						break;
					case VariableType.Boolean:
						PlayMakerIntegration.SetBool(pmVar, val == 1, _variables);
						break;
					case VariableType.String:
						PlayMakerIntegration.SetString(pmVar, textVal, _variables);
						break;
					case VariableType.Float:
						PlayMakerIntegration.SetFloat(pmVar, floatVal, _variables);
						break;
					case VariableType.Vector3:
						PlayMakerIntegration.SetVector3(pmVar, vector3Val, _variables);
						break;
					}
				}
			}
			else if (link == VarLink.OptionsData)
			{
				Options.SavePrefs();
			}
			else if (link == VarLink.CustomScript)
			{
				KickStarter.eventManager.Call_OnUploadVariable(this, _variables);
			}
		}

		public void BackupValue()
		{
			backupVal = val;
			backupFloatVal = floatVal;
		}

		public void RestoreBackupValue()
		{
			val = backupVal;
			floatVal = backupFloatVal;
		}

		public void SetStringValue(string newValue, int newLineID = -1)
		{
			TextValue = newValue;
			if (type == VariableType.String && newLineID >= 0)
			{
				textValLineID = newLineID;
				CreateRuntimeTranslations();
			}
		}

		public void SetFloatValue(float newValue, SetVarMethod setVarMethod = SetVarMethod.SetValue)
		{
			float num = floatVal;
			switch (setVarMethod)
			{
			case SetVarMethod.IncreaseByValue:
				FloatValue = num + newValue;
				break;
			case SetVarMethod.SetAsRandom:
				FloatValue = UnityEngine.Random.Range(0f, newValue);
				break;
			default:
				FloatValue = newValue;
				break;
			}
		}

		public void SetVector3Value(Vector3 newValue)
		{
			Vector3Value = newValue;
		}

		public void SetValue(int newValue, SetVarMethod setVarMethod = SetVarMethod.SetValue)
		{
			switch (setVarMethod)
			{
			case SetVarMethod.IncreaseByValue:
				newValue = IntegerValue + newValue;
				break;
			case SetVarMethod.SetAsRandom:
				newValue = UnityEngine.Random.Range(0, newValue);
				break;
			}
			if (type == VariableType.Boolean)
			{
				BooleanValue = newValue > 0;
				return;
			}
			if (type == VariableType.PopUp)
			{
				newValue = Mathf.Clamp(newValue, 0, GetNumPopUpValues() - 1);
			}
			IntegerValue = newValue;
		}

		public void CreateRuntimeTranslations()
		{
			runtimeTranslations = null;
			if (!HasTranslations())
			{
				return;
			}
			if (type == VariableType.String && canTranslate)
			{
				runtimeTranslations = KickStarter.runtimeLanguages.GetTranslations(textValLineID);
			}
			else
			{
				if (type != VariableType.PopUp)
				{
					return;
				}
				int lineID = -1;
				if (popUpID > 0)
				{
					PopUpLabelData popUpLabelData = KickStarter.variablesManager.GetPopUpLabelData(popUpID);
					if (popUpLabelData != null && popUpLabelData.CanTranslate())
					{
						lineID = popUpLabelData.LineID;
						canTranslate = true;
					}
				}
				else if (canTranslate)
				{
					lineID = popUpsLineID;
				}
				runtimeTranslations = KickStarter.runtimeLanguages.GetTranslations(lineID);
			}
		}

		public string[] GetTranslations()
		{
			return runtimeTranslations;
		}

		public void CopyFromVariable(GVar oldVar, VariableLocation oldLocation)
		{
			if (oldLocation == VariableLocation.Global)
			{
				oldVar.Download(oldLocation);
			}
			if (type == VariableType.Integer || type == VariableType.Boolean || type == VariableType.PopUp)
			{
				int newValue = oldVar.val;
				if (oldVar.type == VariableType.Float)
				{
					newValue = (int)oldVar.floatVal;
				}
				else if (oldVar.type == VariableType.String)
				{
					float result = 0f;
					float.TryParse(oldVar.textVal, out result);
					newValue = (int)result;
				}
				if (type == VariableType.PopUp && oldVar.HasTranslations())
				{
					runtimeTranslations = oldVar.GetTranslations();
				}
				else
				{
					runtimeTranslations = null;
				}
				SetValue(newValue);
			}
			else if (type == VariableType.Float)
			{
				float result2 = oldVar.floatVal;
				if (oldVar.type == VariableType.Integer || oldVar.type == VariableType.Boolean || oldVar.type == VariableType.PopUp)
				{
					result2 = oldVar.val;
				}
				else if (oldVar.type == VariableType.String)
				{
					float.TryParse(oldVar.textVal, out result2);
				}
				SetFloatValue(result2);
			}
			else if (type == VariableType.String)
			{
				string value = oldVar.GetValue();
				textVal = value;
				if (oldVar.HasTranslations())
				{
					runtimeTranslations = oldVar.GetTranslations();
				}
				else
				{
					runtimeTranslations = null;
				}
			}
			else if (type == VariableType.Vector3)
			{
				Vector3 vector = oldVar.vector3Val;
				vector3Val = vector;
			}
		}

		public string GetValue(int languageNumber = 0)
		{
			if (!canTranslate)
			{
				languageNumber = 0;
			}
			if (type == VariableType.Integer)
			{
				return val.ToString();
			}
			if (type == VariableType.PopUp)
			{
				return GetPopUpForIndex(val, languageNumber);
			}
			if (type == VariableType.String)
			{
				if (languageNumber > 0 && runtimeTranslations != null && runtimeTranslations.Length >= languageNumber)
				{
					return runtimeTranslations[languageNumber - 1];
				}
				return textVal;
			}
			if (type == VariableType.Float)
			{
				return floatVal.ToString();
			}
			if (type == VariableType.Vector3)
			{
				return "(" + vector3Val.x + ", " + vector3Val.y + ", " + vector3Val.z + ")";
			}
			if (val == 0)
			{
				return "False";
			}
			return "True";
		}

		public string GetPopUpsString()
		{
			if (popUpID > 0)
			{
				PopUpLabelData popUpLabelData = KickStarter.variablesManager.GetPopUpLabelData(popUpID);
				if (popUpLabelData != null)
				{
					return popUpLabelData.GetPopUpsString();
				}
				return string.Empty;
			}
			string text = string.Empty;
			string[] array = popUps;
			foreach (string text2 in array)
			{
				text = text + text2 + "]";
			}
			if (text.Length > 0)
			{
				return text.Substring(0, text.Length - 1);
			}
			return string.Empty;
		}

		public bool HasTranslations()
		{
			if (type == VariableType.String || type == VariableType.PopUp)
			{
				return canTranslate;
			}
			return false;
		}

		public bool IsGlobalVariable()
		{
			foreach (GVar globalVar in KickStarter.runtimeVariables.globalVars)
			{
				if (globalVar == this)
				{
					return true;
				}
			}
			return false;
		}

		public int GetNumPopUpValues()
		{
			if (popUpID <= 0)
			{
				if (popUps != null)
				{
					return popUps.Length;
				}
			}
			else if (KickStarter.variablesManager != null)
			{
				PopUpLabelData popUpLabelData = KickStarter.variablesManager.GetPopUpLabelData(popUpID);
				if (popUpLabelData != null)
				{
					return popUpLabelData.Length;
				}
			}
			return 0;
		}

		public string GetPopUpForIndex(int index, int language = 0)
		{
			if (index >= 0)
			{
				if (language > 0 && runtimeTranslations != null && runtimeTranslations.Length >= language)
				{
					string text = runtimeTranslations[language - 1];
					string[] array = text.Split("]"[0]);
					if (index < array.Length)
					{
						return array[index];
					}
				}
				else if (popUpID > 0)
				{
					PopUpLabelData popUpLabelData = KickStarter.variablesManager.GetPopUpLabelData(popUpID);
					if (popUpLabelData != null)
					{
						return popUpLabelData.GetValue(index);
					}
				}
				else if (popUps != null && index < popUps.Length)
				{
					return popUps[index];
				}
			}
			return string.Empty;
		}

		public virtual string GetTranslatableString(int index)
		{
			if (type == VariableType.String)
			{
				return textVal;
			}
			return GetPopUpsString();
		}

		public virtual int GetTranslationID(int index)
		{
			if (type == VariableType.String)
			{
				return textValLineID;
			}
			return popUpsLineID;
		}
	}
}
