using System;
using System.Collections.Generic;

namespace AC
{
	[Serializable]
	public class VarPreset
	{
		public string label;

		public int ID;

		public List<PresetValue> presetValues = new List<PresetValue>();

		public VarPreset(List<GVar> _vars, int[] idArray)
		{
			presetValues = new List<PresetValue>();
			presetValues.Clear();
			foreach (GVar _var in _vars)
			{
				presetValues.Add(new PresetValue(_var));
			}
			foreach (int num in idArray)
			{
				if (ID == num)
				{
					ID++;
				}
			}
			label = "New preset";
		}

		public void UpdateCollection(List<GVar> _vars)
		{
			foreach (GVar _var in _vars)
			{
				bool flag = false;
				foreach (PresetValue presetValue in presetValues)
				{
					if (presetValue.id == _var.id)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					presetValues.Add(new PresetValue(_var));
				}
			}
			for (int i = 0; i < presetValues.Count; i++)
			{
				bool flag2 = false;
				foreach (GVar _var2 in _vars)
				{
					if (presetValues[i].id == _var2.id)
					{
						flag2 = true;
						break;
					}
				}
				if (!flag2)
				{
					presetValues.RemoveAt(i);
				}
			}
		}

		public void UpdateCollection(GVar _var)
		{
			bool flag = false;
			foreach (PresetValue presetValue in presetValues)
			{
				if (presetValue.id == _var.id)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				presetValues.Add(new PresetValue(_var));
			}
		}

		public PresetValue GetPresetValue(GVar _var)
		{
			foreach (PresetValue presetValue2 in presetValues)
			{
				if (presetValue2.id == _var.id)
				{
					return presetValue2;
				}
			}
			PresetValue presetValue = new PresetValue(_var);
			presetValues.Add(presetValue);
			return presetValue;
		}

		public PresetValue GetPresetValue(int variableID)
		{
			foreach (PresetValue presetValue in presetValues)
			{
				if (presetValue.id == variableID)
				{
					return presetValue;
				}
			}
			return null;
		}
	}
}
