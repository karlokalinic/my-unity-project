using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace AC
{
	[ExecuteInEditMode]
	[AddComponentMenu("Adventure Creator/Save system/Constant ID")]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_constant_i_d.html")]
	public class ConstantID : MonoBehaviour
	{
		public int constantID;

		public bool retainInPrefab;

		public AutoManual autoManual;

		public virtual string SaveData()
		{
			return string.Empty;
		}

		public virtual void LoadData(string stringData)
		{
		}

		public virtual void LoadData(string stringData, bool restoringSaveFile)
		{
			LoadData(stringData);
		}

		protected bool GameIsPlaying()
		{
			return true;
		}

		protected bool[] StringToBoolArray(string _string)
		{
			if (_string == null || _string == string.Empty || _string.Length == 0)
			{
				return null;
			}
			string[] array = _string.Split("|"[0]);
			List<bool> list = new List<bool>();
			string[] array2 = array;
			foreach (string text in array2)
			{
				if (text == "False")
				{
					list.Add(false);
				}
				else
				{
					list.Add(true);
				}
			}
			return list.ToArray();
		}

		protected int[] StringToIntArray(string _string)
		{
			if (_string == null || _string == string.Empty || _string.Length == 0)
			{
				return null;
			}
			string[] array = _string.Split("|"[0]);
			List<int> list = new List<int>();
			string[] array2 = array;
			foreach (string s in array2)
			{
				list.Add(int.Parse(s));
			}
			return list.ToArray();
		}

		protected float[] StringToFloatArray(string _string)
		{
			if (_string == null || _string == string.Empty || _string.Length == 0)
			{
				return null;
			}
			string[] array = _string.Split("|"[0]);
			List<float> list = new List<float>();
			string[] array2 = array;
			foreach (string s in array2)
			{
				list.Add(float.Parse(s));
			}
			return list.ToArray();
		}

		protected string[] StringToStringArray(string _string)
		{
			if (_string == null || _string == string.Empty || _string.Length == 0)
			{
				return null;
			}
			string[] array = _string.Split("|"[0]);
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = AdvGame.PrepareStringForLoading(array[i]);
			}
			return array;
		}

		protected string ArrayToString<T>(T[] _list)
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < _list.Length; i++)
			{
				T val = _list[i];
				string text = AdvGame.PrepareStringForSaving(val.ToString());
				stringBuilder.Append(text + "|");
			}
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Remove(stringBuilder.Length - 1, 1);
			}
			return stringBuilder.ToString();
		}

		private void OnEnable()
		{
			if ((bool)KickStarter.stateHandler)
			{
				KickStarter.stateHandler.Register(this);
			}
		}

		private void Start()
		{
			if ((bool)KickStarter.stateHandler)
			{
				KickStarter.stateHandler.Register(this);
			}
		}

		private void OnDisable()
		{
			if ((bool)KickStarter.stateHandler)
			{
				KickStarter.stateHandler.Unregister(this);
			}
		}
	}
}
