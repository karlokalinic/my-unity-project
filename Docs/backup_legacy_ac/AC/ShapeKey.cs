using System;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ShapeKey
	{
		public int index;

		public string label = string.Empty;

		public int ID;

		public float value;

		public float targetValue;

		private float initialValue;

		public float InitialValue
		{
			get
			{
				return initialValue;
			}
		}

		public ShapeKey(int[] idArray)
		{
			ID = 0;
			foreach (int num in idArray)
			{
				if (ID == num)
				{
					ID++;
				}
			}
		}

		public void SetValue(float _value, SkinnedMeshRenderer smr)
		{
			value = _value;
			smr.SetBlendShapeWeight(index, value);
		}

		public void ResetInitialValue()
		{
			initialValue = value;
		}
	}
}
