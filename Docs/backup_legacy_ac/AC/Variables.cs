using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[AddComponentMenu("Adventure Creator/Logic/Variables")]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_variables.html")]
	public class Variables : MonoBehaviour, ITranslatable
	{
		public List<GVar> vars = new List<GVar>();

		protected void Start()
		{
			if ((bool)KickStarter.runtimeLanguages)
			{
				foreach (GVar var in vars)
				{
					var.CreateRuntimeTranslations();
				}
			}
			RememberVariables component = GetComponent<RememberVariables>();
			if (component != null && component.LoadedData)
			{
				return;
			}
			foreach (GVar var2 in vars)
			{
				if (var2.updateLinkOnStart)
				{
					var2.Download(VariableLocation.Component);
				}
				else
				{
					var2.Upload(VariableLocation.Component);
				}
			}
		}

		public GVar GetVariable(int _id)
		{
			foreach (GVar var in vars)
			{
				if (var.id == _id)
				{
					var.Download(VariableLocation.Component, this);
					return var;
				}
			}
			return null;
		}

		public GVar GetVariable(int _id, VariableType _type)
		{
			GVar variable = GetVariable(_id);
			if (variable.type == _type)
			{
				variable.Download(VariableLocation.Component, this);
				return variable;
			}
			return null;
		}

		public GVar GetVariable(string _name)
		{
			foreach (GVar var in vars)
			{
				if (var.label == _name)
				{
					var.Download(VariableLocation.Component, this);
					return var;
				}
			}
			return null;
		}

		public GVar GetVariable(string _name, VariableType _type)
		{
			GVar variable = GetVariable(_name);
			if (variable != null && variable.type == _type)
			{
				variable.Download(VariableLocation.Component, this);
				return variable;
			}
			return null;
		}

		public string GetTranslatableString(int index)
		{
			return vars[index].GetTranslatableString(index);
		}

		public int GetTranslationID(int index)
		{
			return vars[index].GetTranslationID(index);
		}
	}
}
