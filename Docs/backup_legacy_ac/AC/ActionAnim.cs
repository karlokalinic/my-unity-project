using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionAnim : Action
	{
		public enum WrapMode2D
		{
			Once = 0,
			Loop = 1,
			PingPong = 2
		}

		public int parameterID = -1;

		public int constantID;

		public Animation _anim;

		public Animation runtimeAnim;

		public AnimationClip clip;

		public float fadeTime;

		public Transform _anim2D;

		public Transform runtimeAnim2D;

		public Animator animator;

		public Animator runtimeAnimator;

		public string clip2D;

		public int clip2DParameterID = -1;

		public WrapMode2D wrapMode2D;

		public int layerInt;

		public Shapeable shapeObject;

		public Shapeable runtimeShapeObject;

		public int shapeKey;

		public float shapeValue;

		public bool isPlayer;

		public AnimMethodMecanim methodMecanim;

		public MecanimParameterType mecanimParameterType;

		public string parameterName;

		public int parameterNameID = -1;

		public float parameterValue;

		public int parameterValueParameterID = -1;

		public AnimMethod method;

		public AnimationBlendMode blendMode;

		public AnimPlayMode playMode;

		public AnimationEngine animationEngine;

		public string customClassName;

		public AnimEngine animEngine;

		public ActionAnim()
		{
			isDisplayed = true;
			category = ActionCategory.Object;
			title = "Animate";
			description = "Causes a GameObject to play or stop an animation, or modify a Blend Shape. The available options will differ depending on the chosen animation engine.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			if (animEngine == null)
			{
				ResetAnimationEngine();
			}
			if (animEngine != null)
			{
				animEngine.ActionAnimAssignValues(this, parameters);
			}
			parameterName = AssignString(parameters, parameterNameID, parameterName);
			clip2D = AssignString(parameters, clip2DParameterID, clip2D);
			if (method == AnimMethod.BlendShape && isPlayer && (bool)KickStarter.player)
			{
				runtimeShapeObject = KickStarter.player.GetComponent<Shapeable>();
			}
		}

		public override float Run()
		{
			if (method == AnimMethod.BlendShape && isPlayer && runtimeShapeObject == null)
			{
				LogWarning("Cannot BlendShape Player since cannot find Shapeable script on Player.");
			}
			if (animEngine != null)
			{
				return animEngine.ActionAnimRun(this);
			}
			LogWarning("Could not create animation engine!");
			return 0f;
		}

		public override void Skip()
		{
			if (animEngine != null)
			{
				animEngine.ActionAnimSkip(this);
			}
		}

		public void ReportWarning(string message, UnityEngine.Object context = null)
		{
			LogWarning(message, context);
		}

		protected void ResetAnimationEngine()
		{
			string empty = string.Empty;
			empty = ((animationEngine != AnimationEngine.Custom) ? ("AnimEngine_" + animationEngine) : customClassName);
			if (!string.IsNullOrEmpty(empty) && (animEngine == null || animEngine.ToString() != empty))
			{
				animEngine = (AnimEngine)ScriptableObject.CreateInstance(empty);
			}
		}

		public static ActionAnim CreateNew_SpritesUnity_PlayCustom(Animator animator, string clipName, int layerIndex, float transitionTime = 0f, bool waitUntilFinish = false)
		{
			ActionAnim actionAnim = ScriptableObject.CreateInstance<ActionAnim>();
			actionAnim.animationEngine = AnimationEngine.SpritesUnity;
			actionAnim.animator = animator;
			actionAnim.clip2D = clipName;
			actionAnim.layerInt = layerIndex;
			actionAnim.fadeTime = transitionTime;
			actionAnim.willWait = waitUntilFinish;
			return actionAnim;
		}

		public static ActionAnim CreateNew_SpritesUnityComplex_PlayCustom(Animator animator, string clipName, int layerIndex, float transitionTime = 0f, bool waitUntilFinish = false)
		{
			ActionAnim actionAnim = ScriptableObject.CreateInstance<ActionAnim>();
			actionAnim.animationEngine = AnimationEngine.SpritesUnityComplex;
			actionAnim.methodMecanim = AnimMethodMecanim.PlayCustom;
			actionAnim.animator = animator;
			actionAnim.clip2D = clipName;
			actionAnim.layerInt = layerIndex;
			actionAnim.fadeTime = transitionTime;
			actionAnim.willWait = waitUntilFinish;
			return actionAnim;
		}

		public static ActionAnim CreateNew_SpritesUnityComplex_ChangeParameterValue(Animator animator, string parameterName, int newValue)
		{
			ActionAnim actionAnim = ScriptableObject.CreateInstance<ActionAnim>();
			actionAnim.animationEngine = AnimationEngine.SpritesUnityComplex;
			actionAnim.methodMecanim = AnimMethodMecanim.ChangeParameterValue;
			actionAnim.animator = animator;
			actionAnim.parameterName = parameterName;
			actionAnim.mecanimParameterType = MecanimParameterType.Int;
			actionAnim.parameterValue = newValue;
			return actionAnim;
		}

		public static ActionAnim CreateNew_SpritesUnityComplex_ChangeParameterValue(Animator animator, string parameterName, float newValue)
		{
			ActionAnim actionAnim = ScriptableObject.CreateInstance<ActionAnim>();
			actionAnim.animationEngine = AnimationEngine.SpritesUnityComplex;
			actionAnim.methodMecanim = AnimMethodMecanim.ChangeParameterValue;
			actionAnim.animator = animator;
			actionAnim.parameterName = parameterName;
			actionAnim.mecanimParameterType = MecanimParameterType.Float;
			actionAnim.parameterValue = newValue;
			return actionAnim;
		}

		public static ActionAnim CreateNew_SpritesUnityComplex_ChangeParameterValue(Animator animator, string parameterName, bool newValue)
		{
			ActionAnim actionAnim = ScriptableObject.CreateInstance<ActionAnim>();
			actionAnim.animationEngine = AnimationEngine.SpritesUnityComplex;
			actionAnim.methodMecanim = AnimMethodMecanim.ChangeParameterValue;
			actionAnim.animator = animator;
			actionAnim.parameterName = parameterName;
			actionAnim.mecanimParameterType = MecanimParameterType.Bool;
			actionAnim.parameterValue = ((!newValue) ? 0f : 1f);
			return actionAnim;
		}

		public static ActionAnim CreateNew_SpritesUnityComplex_ChangeParameterValue(Animator animator, string parameterName)
		{
			ActionAnim actionAnim = ScriptableObject.CreateInstance<ActionAnim>();
			actionAnim.animationEngine = AnimationEngine.SpritesUnityComplex;
			actionAnim.methodMecanim = AnimMethodMecanim.ChangeParameterValue;
			actionAnim.animator = animator;
			actionAnim.parameterName = parameterName;
			actionAnim.mecanimParameterType = MecanimParameterType.Trigger;
			actionAnim.parameterValue = 0f;
			return actionAnim;
		}

		public static ActionAnim CreateNew_Mecanim_PlayCustom(Animator animator, string clipName, int layerIndex, float transitionTime = 0f, bool waitUntilFinish = false)
		{
			ActionAnim actionAnim = ScriptableObject.CreateInstance<ActionAnim>();
			actionAnim.animationEngine = AnimationEngine.Mecanim;
			actionAnim.methodMecanim = AnimMethodMecanim.PlayCustom;
			actionAnim.animator = animator;
			actionAnim.clip2D = clipName;
			actionAnim.layerInt = layerIndex;
			actionAnim.fadeTime = transitionTime;
			actionAnim.willWait = waitUntilFinish;
			return actionAnim;
		}

		public static ActionAnim CreateNew_Mecanim_ChangeParameterValue(Animator animator, string parameterName, int newValue)
		{
			ActionAnim actionAnim = ScriptableObject.CreateInstance<ActionAnim>();
			actionAnim.animationEngine = AnimationEngine.Mecanim;
			actionAnim.methodMecanim = AnimMethodMecanim.ChangeParameterValue;
			actionAnim.animator = animator;
			actionAnim.parameterName = parameterName;
			actionAnim.mecanimParameterType = MecanimParameterType.Int;
			actionAnim.parameterValue = newValue;
			return actionAnim;
		}

		public static ActionAnim CreateNew_Mecanim_ChangeParameterValue(Animator animator, string parameterName, float newValue)
		{
			ActionAnim actionAnim = ScriptableObject.CreateInstance<ActionAnim>();
			actionAnim.animationEngine = AnimationEngine.Mecanim;
			actionAnim.methodMecanim = AnimMethodMecanim.ChangeParameterValue;
			actionAnim.animator = animator;
			actionAnim.parameterName = parameterName;
			actionAnim.mecanimParameterType = MecanimParameterType.Float;
			actionAnim.parameterValue = newValue;
			return actionAnim;
		}

		public static ActionAnim CreateNew_Mecanim_ChangeParameterValue(Animator animator, string parameterName, bool newValue)
		{
			ActionAnim actionAnim = ScriptableObject.CreateInstance<ActionAnim>();
			actionAnim.animationEngine = AnimationEngine.Mecanim;
			actionAnim.methodMecanim = AnimMethodMecanim.ChangeParameterValue;
			actionAnim.animator = animator;
			actionAnim.parameterName = parameterName;
			actionAnim.mecanimParameterType = MecanimParameterType.Bool;
			actionAnim.parameterValue = ((!newValue) ? 0f : 1f);
			return actionAnim;
		}

		public static ActionAnim CreateNew_Mecanim_ChangeParameterValue(Animator animator, string parameterName)
		{
			ActionAnim actionAnim = ScriptableObject.CreateInstance<ActionAnim>();
			actionAnim.animationEngine = AnimationEngine.Mecanim;
			actionAnim.methodMecanim = AnimMethodMecanim.ChangeParameterValue;
			actionAnim.animator = animator;
			actionAnim.parameterName = parameterName;
			actionAnim.mecanimParameterType = MecanimParameterType.Trigger;
			actionAnim.parameterValue = 0f;
			return actionAnim;
		}

		public static ActionAnim CreateNew_BlendShape(Shapeable shapeable, int shapeKey, float shapeValue, float transitionTime = 1f, bool waitUntilFinish = false)
		{
			ActionAnim actionAnim = ScriptableObject.CreateInstance<ActionAnim>();
			actionAnim.animationEngine = AnimationEngine.Mecanim;
			actionAnim.methodMecanim = AnimMethodMecanim.BlendShape;
			actionAnim.shapeObject = shapeable;
			actionAnim.shapeKey = shapeKey;
			actionAnim.shapeValue = shapeValue;
			actionAnim.fadeTime = transitionTime;
			actionAnim.willWait = waitUntilFinish;
			return actionAnim;
		}
	}
}
