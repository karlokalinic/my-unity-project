using System;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class PopUpLabelData : ITranslatable
	{
		[SerializeField]
		protected int id = 1;

		[SerializeField]
		protected string[] labels = new string[0];

		[SerializeField]
		protected int lineID = -1;

		[SerializeField]
		protected bool canTranslate;

		public int ID
		{
			get
			{
				return id;
			}
		}

		public int Length
		{
			get
			{
				return labels.Length;
			}
		}

		public int LineID
		{
			get
			{
				return lineID;
			}
		}

		public PopUpLabelData(int[] idArray, string[] existingLabels, int _lineID)
		{
			labels = new string[0];
			lineID = _lineID;
			id = 1;
			foreach (int num in idArray)
			{
				if (id == num)
				{
					id++;
				}
			}
			if (existingLabels != null && existingLabels.Length > 0)
			{
				labels = new string[existingLabels.Length];
				for (int j = 0; j < existingLabels.Length; j++)
				{
					labels[j] = existingLabels[j];
				}
			}
		}

		public string GetValue(int index)
		{
			if (index >= 0 && index < Length)
			{
				return labels[index];
			}
			return string.Empty;
		}

		public bool CanTranslate()
		{
			if (canTranslate)
			{
				return !string.IsNullOrEmpty(GetPopUpsString());
			}
			return false;
		}

		public string GetPopUpsString()
		{
			string text = string.Empty;
			string[] array = labels;
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

		public string GetTranslatableString(int index)
		{
			return GetPopUpsString();
		}

		public int GetTranslationID(int index)
		{
			return lineID;
		}
	}
}
