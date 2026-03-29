using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[AddComponentMenu("Adventure Creator/Misc/Shapeable")]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_shapeable.html")]
	public class Shapeable : MonoBehaviour
	{
		public List<ShapeGroup> shapeGroups = new List<ShapeGroup>();

		protected SkinnedMeshRenderer skinnedMeshRenderer;

		private bool isChanging;

		private float targetShape;

		private float actualShape;

		private float originalShape;

		private int shapeKey;

		private float startTime;

		private float deltaTime;

		protected SkinnedMeshRenderer SkinnedMeshRenderer
		{
			get
			{
				if (skinnedMeshRenderer == null)
				{
					skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
					if (skinnedMeshRenderer == null)
					{
						skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
					}
					if (skinnedMeshRenderer == null)
					{
						ACDebug.LogWarning("No Skinned Mesh Renderer found on Shapeable GameObject!", this);
					}
				}
				return skinnedMeshRenderer;
			}
		}

		protected void Awake()
		{
			if (!(SkinnedMeshRenderer != null))
			{
				return;
			}
			foreach (ShapeGroup shapeGroup in shapeGroups)
			{
				shapeGroup.SetSMR(SkinnedMeshRenderer);
				foreach (ShapeKey shapeKey in shapeGroup.shapeKeys)
				{
					shapeKey.SetValue(0f, SkinnedMeshRenderer);
				}
			}
		}

		protected void LateUpdate()
		{
			foreach (ShapeGroup shapeGroup in shapeGroups)
			{
				shapeGroup.UpdateKeys();
			}
			if (isChanging)
			{
				actualShape = Mathf.Lerp(originalShape, targetShape, AdvGame.Interpolate(startTime, deltaTime, MoveMethod.Linear));
				if (Time.time > startTime + deltaTime)
				{
					isChanging = false;
					actualShape = targetShape;
				}
				if (SkinnedMeshRenderer != null)
				{
					SkinnedMeshRenderer.SetBlendShapeWeight(shapeKey, actualShape);
				}
			}
		}

		public void DisableAllKeys(int _groupID, float _deltaTime, MoveMethod _moveMethod, AnimationCurve _timeCurve)
		{
			foreach (ShapeGroup shapeGroup in shapeGroups)
			{
				if (shapeGroup.ID == _groupID)
				{
					shapeGroup.SetActive(-1, 0f, _deltaTime, _moveMethod, _timeCurve);
				}
			}
		}

		public void SetActiveKey(int _groupID, int _keyID, float _value, float _deltaTime, MoveMethod _moveMethod, AnimationCurve _timeCurve)
		{
			foreach (ShapeGroup shapeGroup in shapeGroups)
			{
				if (shapeGroup.ID == _groupID)
				{
					shapeGroup.SetActive(_keyID, _value, _deltaTime, _moveMethod, _timeCurve);
				}
			}
		}

		public void SetActiveKey(int _groupID, string _keyLabel, float _value, float _deltaTime, MoveMethod _moveMethod, AnimationCurve _timeCurve)
		{
			foreach (ShapeGroup shapeGroup in shapeGroups)
			{
				if (shapeGroup.ID == _groupID)
				{
					shapeGroup.SetActive(_keyLabel, _value, _deltaTime, _moveMethod, _timeCurve);
				}
			}
		}

		public ShapeGroup GetGroup(int ID)
		{
			foreach (ShapeGroup shapeGroup in shapeGroups)
			{
				if (shapeGroup.ID == ID)
				{
					return shapeGroup;
				}
			}
			return null;
		}

		public void Change(int _shapeKey, float _targetShape, float _deltaTime)
		{
			if (targetShape < 0f)
			{
				targetShape = 0f;
			}
			else if (targetShape > 100f)
			{
				targetShape = 100f;
			}
			isChanging = true;
			targetShape = _targetShape;
			deltaTime = _deltaTime;
			startTime = Time.time;
			shapeKey = _shapeKey;
			if (SkinnedMeshRenderer != null)
			{
				originalShape = SkinnedMeshRenderer.GetBlendShapeWeight(shapeKey);
			}
		}
	}
}
