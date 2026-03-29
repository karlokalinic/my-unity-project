using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace AC
{
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_runtime_variables.html")]
	public class RuntimeVariables : MonoBehaviour
	{
		public List<GVar> globalVars = new List<GVar>();

		protected List<CustomToken> customTokens = new List<CustomToken>();

		protected List<SpeechLog> speechLines = new List<SpeechLog>();

		protected string[] textEventTokenKeys = new string[0];

		public string[] TextEventTokenKeys
		{
			get
			{
				return textEventTokenKeys;
			}
			set
			{
				textEventTokenKeys = value;
			}
		}

		public void OnStart()
		{
			TransferFromManager();
			AssignOptionsLinkedVariabes();
			LinkAllValues();
		}

		public SpeechLog[] GetSpeechLog()
		{
			return speechLines.ToArray();
		}

		public void ClearSpeechLog()
		{
			speechLines.Clear();
		}

		public void AddToSpeechLog(SpeechLog _line)
		{
			int lineID = _line.lineID;
			if (lineID >= 0)
			{
				foreach (SpeechLog speechLine in speechLines)
				{
					if (speechLine.lineID == lineID)
					{
						speechLines.Remove(speechLine);
						break;
					}
				}
			}
			speechLines.Add(_line);
		}

		public void SetCustomToken(int _ID, string _replacementText)
		{
			CustomToken item = new CustomToken(_ID, _replacementText);
			for (int i = 0; i < customTokens.Count; i++)
			{
				if (customTokens[i].ID == _ID)
				{
					customTokens.RemoveAt(i);
					break;
				}
			}
			customTokens.Add(item);
		}

		public void ClearCustomTokens()
		{
			customTokens.Clear();
		}

		public string ConvertCustomTokens(string _text)
		{
			if (_text.Contains("[token:"))
			{
				foreach (CustomToken customToken in customTokens)
				{
					string text = "[token:" + customToken.ID + "]";
					if (_text.Contains(text))
					{
						_text = _text.Replace(text, customToken.replacementText);
					}
				}
			}
			return _text;
		}

		public void AssignCustomTokensFromString(string tokenData)
		{
			if (!string.IsNullOrEmpty(tokenData))
			{
				customTokens.Clear();
				string[] array = tokenData.Split("|"[0]);
				string[] array2 = array;
				foreach (string text in array2)
				{
					string[] array3 = text.Split(":"[0]);
					int result = 0;
					int.TryParse(array3[0], out result);
					string text2 = array3[1];
					customTokens.Add(new CustomToken(result, AdvGame.PrepareStringForLoading(text2)));
				}
			}
		}

		public void AssignOptionsLinkedVariabes()
		{
			if ((bool)AdvGame.GetReferences() && (bool)AdvGame.GetReferences().variablesManager && Options.optionsData != null && Options.optionsData.linkedVariables != string.Empty)
			{
				SaveSystem.AssignVariables(Options.optionsData.linkedVariables, true);
			}
		}

		public MainData SaveMainData(MainData mainData)
		{
			GlobalVariables.DownloadAll();
			mainData.runtimeVariablesData = SaveSystem.CreateVariablesData(GlobalVariables.GetAllVars(), false, VariableLocation.Global);
			mainData.customTokenData = GetCustomTokensAsString();
			return mainData;
		}

		public void AssignFromPreset(VarPreset varPreset, bool ignoreOptionLinked = false)
		{
			foreach (GVar globalVar in globalVars)
			{
				foreach (PresetValue presetValue in varPreset.presetValues)
				{
					if (globalVar.id == presetValue.id && (!ignoreOptionLinked || globalVar.link != VarLink.OptionsData))
					{
						globalVar.val = presetValue.val;
						globalVar.floatVal = presetValue.floatVal;
						globalVar.textVal = presetValue.textVal;
						globalVar.vector3Val = presetValue.vector3Val;
						globalVar.Upload();
					}
				}
			}
		}

		public void AssignFromPreset(int varPresetID, bool ignoreOptionLinked = false)
		{
			if (KickStarter.variablesManager.varPresets == null)
			{
				return;
			}
			foreach (VarPreset varPreset in KickStarter.variablesManager.varPresets)
			{
				if (varPreset.ID == varPresetID)
				{
					AssignFromPreset(varPreset, ignoreOptionLinked);
					break;
				}
			}
		}

		public VarPreset GetPreset(int varPresetID)
		{
			if (KickStarter.variablesManager.varPresets == null)
			{
				return null;
			}
			foreach (VarPreset varPreset in KickStarter.variablesManager.varPresets)
			{
				if (varPreset.ID == varPresetID)
				{
					return varPreset;
				}
			}
			return null;
		}

		protected string GetCustomTokensAsString()
		{
			if (customTokens != null)
			{
				StringBuilder stringBuilder = new StringBuilder();
				foreach (CustomToken customToken in customTokens)
				{
					stringBuilder.Append(customToken.ID.ToString());
					stringBuilder.Append(":");
					stringBuilder.Append(customToken.GetSafeReplacementText());
					stringBuilder.Append("|");
				}
				if (customTokens.Count > 0)
				{
					stringBuilder.Remove(stringBuilder.Length - 1, 1);
				}
				return stringBuilder.ToString();
			}
			return string.Empty;
		}

		protected void TransferFromManager()
		{
			if (!AdvGame.GetReferences() || !AdvGame.GetReferences().variablesManager)
			{
				return;
			}
			VariablesManager variablesManager = AdvGame.GetReferences().variablesManager;
			globalVars.Clear();
			foreach (GVar var in variablesManager.vars)
			{
				globalVars.Add(new GVar(var));
			}
			foreach (GVar globalVar in globalVars)
			{
				globalVar.CreateRuntimeTranslations();
			}
		}

		protected void LinkAllValues()
		{
			foreach (GVar globalVar in globalVars)
			{
				if (globalVar.link == VarLink.PlaymakerVariable || globalVar.link == VarLink.CustomScript)
				{
					if (globalVar.updateLinkOnStart)
					{
						globalVar.Download();
					}
					else
					{
						globalVar.Upload();
					}
				}
			}
		}
	}
}
