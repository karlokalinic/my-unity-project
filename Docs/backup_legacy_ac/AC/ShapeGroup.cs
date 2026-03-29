using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ShapeGroup
	{
		public string label = string.Empty;

		public int ID;

		public List<ShapeKey> shapeKeys = new List<ShapeKey>();

		protected ShapeKey activeKey;

		protected SkinnedMeshRenderer smr;

		protected float startTime;

		protected float changeTime;

		protected AnimationCurve timeCurve;

		protected MoveMethod moveMethod;

		public ShapeGroup(int[] idArray)
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

		public void SetSMR(SkinnedMeshRenderer _smr)
		{
			smr = _smr;
		}

		public int GetActiveKeyID()
		{
			if (activeKey != null && shapeKeys.Contains(activeKey))
			{
				return activeKey.ID;
			}
			return -1;
		}

		public float GetActiveKeyValue()
		{
			if (activeKey != null && shapeKeys.Contains(activeKey))
			{
				return activeKey.targetValue;
			}
			return 0f;
		}

		public void SetActive(int _ID, float _value, float _changeTime, MoveMethod _moveMethod, AnimationCurve _timeCurve)
		{
			if (_changeTime < 0f)
			{
				return;
			}
			activeKey = null;
			foreach (ShapeKey shapeKey in shapeKeys)
			{
				if (shapeKey.ID == _ID)
				{
					activeKey = shapeKey;
					shapeKey.targetValue = _value;
				}
				else
				{
					shapeKey.targetValue = 0f;
				}
				shapeKey.ResetInitialValue();
			}
			moveMethod = _moveMethod;
			timeCurve = _timeCurve;
			changeTime = _changeTime;
			startTime = Time.time;
		}

		public void SetActive(string _label, float _value, float _changeTime, MoveMethod _moveMethod, AnimationCurve _timeCurve)
		{
			if (_changeTime < 0f)
			{
				return;
			}
			activeKey = null;
			foreach (ShapeKey shapeKey in shapeKeys)
			{
				if (shapeKey.label == _label)
				{
					activeKey = shapeKey;
					shapeKey.targetValue = _value;
				}
				else
				{
					shapeKey.targetValue = 0f;
				}
				shapeKey.ResetInitialValue();
			}
			moveMethod = _moveMethod;
			timeCurve = _timeCurve;
			changeTime = _changeTime;
			startTime = Time.time;
		}

		public void UpdateKeys()
		{
			if (smr == null)
			{
				return;
			}
			foreach (ShapeKey shapeKey in shapeKeys)
			{
				if (changeTime > 0f)
				{
					float value = Mathf.Lerp(shapeKey.InitialValue, shapeKey.targetValue, AdvGame.Interpolate(startTime, changeTime, moveMethod, timeCurve));
					shapeKey.SetValue(value, smr);
					if (startTime + changeTime < Time.time)
					{
						changeTime = 0f;
					}
				}
				else
				{
					shapeKey.SetValue(shapeKey.targetValue, smr);
				}
			}
		}
	}
}
