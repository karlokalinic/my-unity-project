using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionBlendShape : Action
	{
		public int parameterID = -1;

		public int constantID;

		public Shapeable shapeObject;

		public int shapeGroupID;

		public int shapeKeyID;

		public float shapeValue;

		public bool isPlayer;

		public bool disableAllKeys;

		public float fadeTime;

		public MoveMethod moveMethod = MoveMethod.Smooth;

		public AnimationCurve timeCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

		protected Shapeable runtimeShapeObject;

		public ActionBlendShape()
		{
			isDisplayed = true;
			category = ActionCategory.Object;
			title = "Blend shape";
			description = "Animates a Skinned Mesh Renderer's blend shape by a chosen amount. If the Shapeable script attached to the renderer has grouped multiple shapes into a group, all other shapes in that group will be deactivated.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			runtimeShapeObject = AssignFile(parameters, parameterID, constantID, shapeObject);
			if (isPlayer && (bool)KickStarter.player)
			{
				runtimeShapeObject = KickStarter.player.GetShapeable();
			}
		}

		public override float Run()
		{
			if (isPlayer && runtimeShapeObject == null)
			{
				LogWarning("Cannot BlendShape Player since cannot find Shapeable script on Player.");
			}
			if (!isRunning)
			{
				isRunning = true;
				if (runtimeShapeObject != null)
				{
					DoShape(fadeTime);
					if (willWait)
					{
						return fadeTime;
					}
				}
				return 0f;
			}
			isRunning = false;
			return 0f;
		}

		public override void Skip()
		{
			DoShape(0f);
		}

		protected void DoShape(float _time)
		{
			if (runtimeShapeObject != null)
			{
				if (disableAllKeys)
				{
					runtimeShapeObject.DisableAllKeys(shapeGroupID, _time, moveMethod, timeCurve);
				}
				else
				{
					runtimeShapeObject.SetActiveKey(shapeGroupID, shapeKeyID, shapeValue, _time, moveMethod, timeCurve);
				}
			}
		}

		public static ActionBlendShape CreateNew_SetActiveKey(Shapeable shapeable, int groupID, int keyID, float newKeyValue, float transitionTime = 0f, MoveMethod moveMethod = MoveMethod.Linear, AnimationCurve timeCurve = null)
		{
			ActionBlendShape actionBlendShape = ScriptableObject.CreateInstance<ActionBlendShape>();
			actionBlendShape.disableAllKeys = false;
			actionBlendShape.shapeObject = shapeable;
			actionBlendShape.shapeGroupID = groupID;
			actionBlendShape.shapeKeyID = keyID;
			actionBlendShape.shapeValue = newKeyValue;
			actionBlendShape.fadeTime = transitionTime;
			actionBlendShape.moveMethod = moveMethod;
			actionBlendShape.timeCurve = timeCurve;
			return actionBlendShape;
		}

		public static ActionBlendShape CreateNew_DisableAllKeys(Shapeable shapeable, int groupID, float transitionTime = 0f, MoveMethod moveMethod = MoveMethod.Linear, AnimationCurve timeCurve = null)
		{
			ActionBlendShape actionBlendShape = ScriptableObject.CreateInstance<ActionBlendShape>();
			actionBlendShape.disableAllKeys = true;
			actionBlendShape.shapeObject = shapeable;
			actionBlendShape.shapeGroupID = groupID;
			actionBlendShape.fadeTime = transitionTime;
			actionBlendShape.moveMethod = moveMethod;
			actionBlendShape.timeCurve = timeCurve;
			return actionBlendShape;
		}
	}
}
