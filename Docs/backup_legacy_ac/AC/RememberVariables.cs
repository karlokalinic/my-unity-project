using UnityEngine;

namespace AC
{
	[RequireComponent(typeof(Variables))]
	[AddComponentMenu("Adventure Creator/Save system/Remember Variables")]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_remember_variables.html")]
	public class RememberVariables : Remember
	{
		private bool loadedData;

		private Variables variables;

		public bool LoadedData
		{
			get
			{
				return loadedData;
			}
		}

		private Variables Variables
		{
			get
			{
				if (variables == null)
				{
					variables = GetComponent<Variables>();
				}
				return variables;
			}
		}

		public override string SaveData()
		{
			VariablesData variablesData = new VariablesData();
			foreach (GVar var in Variables.vars)
			{
				var.Download(VariableLocation.Component);
			}
			variablesData.variablesData = SaveSystem.CreateVariablesData(Variables.vars, false, VariableLocation.Component);
			return Serializer.SaveScriptData<VariablesData>(variablesData);
		}

		public override void LoadData(string stringData)
		{
			VariablesData variablesData = Serializer.LoadScriptData<VariablesData>(stringData);
			if (variablesData == null)
			{
				loadedData = false;
				return;
			}
			base.SavePrevented = variablesData.savePrevented;
			if (savePrevented)
			{
				return;
			}
			Variables.vars = SaveSystem.UnloadVariablesData(variablesData.variablesData, Variables.vars);
			foreach (GVar var in Variables.vars)
			{
				var.Upload(VariableLocation.Component);
			}
			loadedData = true;
		}
	}
}
