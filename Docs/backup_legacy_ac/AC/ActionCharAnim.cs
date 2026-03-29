using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionCharAnim : Action
	{
		public enum AnimMethodChar
		{
			PlayCustom = 0,
			StopCustom = 1,
			ResetToIdle = 2,
			SetStandard = 3
		}

		public int parameterID = -1;

		public int constantID;

		public AnimEngine editingAnimEngine;

		public bool isPlayer;

		public Char animChar;

		protected Char runtimeAnimChar;

		public AnimationClip clip;

		public string clip2D;

		public int clip2DParameterID = -1;

		public AnimMethodChar method;

		public AnimationBlendMode blendMode;

		public AnimLayer layer;

		public AnimStandard standard;

		public bool includeDirection;

		public bool changeSound;

		public AudioClip newSound;

		public int newSoundParameterID = -1;

		public int newSpeedParameterID = -1;

		public int layerInt;

		public bool idleAfter = true;

		public bool idleAfterCustom;

		public AnimPlayMode playMode;

		public AnimPlayModeBase playModeBase = AnimPlayModeBase.PlayOnceAndClamp;

		public float fadeTime;

		public bool changeSpeed;

		public float newSpeed;

		public AnimMethodCharMecanim methodMecanim;

		public MecanimCharParameter mecanimCharParameter;

		public MecanimParameterType mecanimParameterType;

		public string parameterName;

		public int parameterNameID = -1;

		public float parameterValue;

		public int parameterValueParameterID = -1;

		public bool hideHead;

		public bool doLoop;

		public Char RuntimeAnimChar
		{
			get
			{
				return runtimeAnimChar;
			}
		}

		public ActionCharAnim()
		{
			isDisplayed = true;
			category = ActionCategory.Character;
			title = "Animate";
			description = "Affects a Character's animation. Can play or stop a custom animation, change a standard animation (idle, walk or run), change a footstep sound, or revert the Character to idle.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			runtimeAnimChar = AssignFile(parameters, parameterID, constantID, animChar);
			newSound = (AudioClip)AssignObject<AudioClip>(parameters, newSoundParameterID, newSound);
			newSpeed = AssignFloat(parameters, newSpeedParameterID, newSpeed);
			parameterName = AssignString(parameters, parameterNameID, parameterName);
			clip2D = AssignString(parameters, clip2DParameterID, clip2D);
			if (isPlayer)
			{
				runtimeAnimChar = KickStarter.player;
			}
			if (runtimeAnimChar != null && runtimeAnimChar.GetAnimEngine() != null)
			{
				runtimeAnimChar.GetAnimEngine().ActionCharAnimAssignValues(this, parameters);
			}
		}

		public override float Run()
		{
			if (runtimeAnimChar != null)
			{
				if (runtimeAnimChar.GetAnimEngine() != null)
				{
					return runtimeAnimChar.GetAnimEngine().ActionCharAnimRun(this);
				}
				LogWarning("Could not create animation engine for " + runtimeAnimChar.name, runtimeAnimChar);
			}
			else
			{
				LogWarning("Could not create animation engine!");
			}
			return 0f;
		}

		public override void Skip()
		{
			if (runtimeAnimChar != null && runtimeAnimChar.GetAnimEngine() != null)
			{
				runtimeAnimChar.GetAnimEngine().ActionCharAnimSkip(this);
			}
		}

		public void ReportWarning(string message, UnityEngine.Object context = null)
		{
			LogWarning(message, context);
		}

		public static ActionCharAnim CreateNew_SpritesUnity_PlayCustom(Char characterToAnimate, string clipName, bool addDirectionalSuffix = false, int layerIndex = 0, float transitionTime = 0f, bool waitUntilFinish = true, bool returnToIdleAfter = true)
		{
			ActionCharAnim actionCharAnim = ScriptableObject.CreateInstance<ActionCharAnim>();
			actionCharAnim.animChar = characterToAnimate;
			actionCharAnim.method = AnimMethodChar.PlayCustom;
			actionCharAnim.clip2D = clipName;
			actionCharAnim.includeDirection = addDirectionalSuffix;
			actionCharAnim.layerInt = layerIndex;
			actionCharAnim.fadeTime = transitionTime;
			actionCharAnim.willWait = waitUntilFinish;
			actionCharAnim.idleAfter = returnToIdleAfter;
			return actionCharAnim;
		}

		public static ActionCharAnim CreateNew_SpritesUnity_SetStandard(Char characterToAnimate, AnimStandard standardToChange, string newStandardName, AudioClip newSound = null, float newSpeed = 0f)
		{
			ActionCharAnim actionCharAnim = ScriptableObject.CreateInstance<ActionCharAnim>();
			actionCharAnim.animChar = characterToAnimate;
			actionCharAnim.method = AnimMethodChar.SetStandard;
			actionCharAnim.standard = standardToChange;
			actionCharAnim.clip2D = newStandardName;
			if (newSound != null)
			{
				actionCharAnim.changeSound = true;
				actionCharAnim.newSound = newSound;
			}
			if (newSpeed > 0f)
			{
				actionCharAnim.changeSpeed = true;
				actionCharAnim.newSpeed = newSpeed;
			}
			return actionCharAnim;
		}

		public static ActionCharAnim CreateNew_SpritesUnity_ResetToIdle(Char characterToAnimate, bool waitForCustomAnimationToFinish = false)
		{
			ActionCharAnim actionCharAnim = ScriptableObject.CreateInstance<ActionCharAnim>();
			actionCharAnim.animChar = characterToAnimate;
			actionCharAnim.method = AnimMethodChar.ResetToIdle;
			actionCharAnim.idleAfterCustom = waitForCustomAnimationToFinish;
			return actionCharAnim;
		}

		public static ActionCharAnim CreateNew_SpritesUnityComplex_PlayCustom(Char characterToAnimate, string clipName, bool addDirectionalSuffix = false, int layerIndex = 0, float transitionTime = 0f, bool waitUntilFinish = true)
		{
			ActionCharAnim actionCharAnim = ScriptableObject.CreateInstance<ActionCharAnim>();
			actionCharAnim.animChar = characterToAnimate;
			actionCharAnim.methodMecanim = AnimMethodCharMecanim.PlayCustom;
			actionCharAnim.clip2D = clipName;
			actionCharAnim.includeDirection = addDirectionalSuffix;
			actionCharAnim.layerInt = layerIndex;
			actionCharAnim.fadeTime = transitionTime;
			actionCharAnim.willWait = waitUntilFinish;
			return actionCharAnim;
		}

		public static ActionCharAnim CreateNew_SpritesUnityComplex_ChangeParameterValue(Char characterToAnimate, string parameterName)
		{
			ActionCharAnim actionCharAnim = ScriptableObject.CreateInstance<ActionCharAnim>();
			actionCharAnim.animChar = characterToAnimate;
			actionCharAnim.methodMecanim = AnimMethodCharMecanim.ChangeParameterValue;
			actionCharAnim.parameterName = parameterName;
			actionCharAnim.mecanimParameterType = MecanimParameterType.Trigger;
			return actionCharAnim;
		}

		public static ActionCharAnim CreateNew_SpritesUnityComplex_ChangeParameterValue(Char characterToAnimate, string parameterName, int parameterValue)
		{
			ActionCharAnim actionCharAnim = ScriptableObject.CreateInstance<ActionCharAnim>();
			actionCharAnim.animChar = characterToAnimate;
			actionCharAnim.methodMecanim = AnimMethodCharMecanim.ChangeParameterValue;
			actionCharAnim.parameterName = parameterName;
			actionCharAnim.mecanimParameterType = MecanimParameterType.Int;
			actionCharAnim.parameterValue = parameterValue;
			return actionCharAnim;
		}

		public static ActionCharAnim CreateNew_SpritesUnityComplex_ChangeParameterValue(Char characterToAnimate, string parameterName, float parameterValue)
		{
			ActionCharAnim actionCharAnim = ScriptableObject.CreateInstance<ActionCharAnim>();
			actionCharAnim.animChar = characterToAnimate;
			actionCharAnim.methodMecanim = AnimMethodCharMecanim.ChangeParameterValue;
			actionCharAnim.parameterName = parameterName;
			actionCharAnim.mecanimParameterType = MecanimParameterType.Float;
			actionCharAnim.parameterValue = parameterValue;
			return actionCharAnim;
		}

		public static ActionCharAnim CreateNew_SpritesUnityComplex_ChangeParameterValue(Char characterToAnimate, string parameterName, bool parameterValue)
		{
			ActionCharAnim actionCharAnim = ScriptableObject.CreateInstance<ActionCharAnim>();
			actionCharAnim.animChar = characterToAnimate;
			actionCharAnim.methodMecanim = AnimMethodCharMecanim.ChangeParameterValue;
			actionCharAnim.parameterName = parameterName;
			actionCharAnim.mecanimParameterType = MecanimParameterType.Bool;
			actionCharAnim.parameterValue = ((!parameterValue) ? 0f : 1f);
			return actionCharAnim;
		}

		public static ActionCharAnim CreateNew_SpritesUnityComplex_SetStandard(Char characterToAnimate, MecanimCharParameter parameterToChange, string newParameterName)
		{
			ActionCharAnim actionCharAnim = ScriptableObject.CreateInstance<ActionCharAnim>();
			actionCharAnim.animChar = characterToAnimate;
			actionCharAnim.methodMecanim = AnimMethodCharMecanim.SetStandard;
			actionCharAnim.mecanimCharParameter = parameterToChange;
			actionCharAnim.parameterName = newParameterName;
			return actionCharAnim;
		}

		public static ActionCharAnim CreateNew_Mecanim_ChangeParameterValue(Char characterToAnimate, string parameterName)
		{
			ActionCharAnim actionCharAnim = ScriptableObject.CreateInstance<ActionCharAnim>();
			actionCharAnim.animChar = characterToAnimate;
			actionCharAnim.methodMecanim = AnimMethodCharMecanim.ChangeParameterValue;
			actionCharAnim.parameterName = parameterName;
			actionCharAnim.mecanimParameterType = MecanimParameterType.Trigger;
			return actionCharAnim;
		}

		public static ActionCharAnim CreateNew_Mecanim_ChangeParameterValue(Char characterToAnimate, string parameterName, int parameterValue)
		{
			ActionCharAnim actionCharAnim = ScriptableObject.CreateInstance<ActionCharAnim>();
			actionCharAnim.animChar = characterToAnimate;
			actionCharAnim.methodMecanim = AnimMethodCharMecanim.ChangeParameterValue;
			actionCharAnim.parameterName = parameterName;
			actionCharAnim.mecanimParameterType = MecanimParameterType.Int;
			actionCharAnim.parameterValue = parameterValue;
			return actionCharAnim;
		}

		public static ActionCharAnim CreateNew_Mecanim_ChangeParameterValue(Char characterToAnimate, string parameterName, float parameterValue)
		{
			ActionCharAnim actionCharAnim = ScriptableObject.CreateInstance<ActionCharAnim>();
			actionCharAnim.animChar = characterToAnimate;
			actionCharAnim.methodMecanim = AnimMethodCharMecanim.ChangeParameterValue;
			actionCharAnim.parameterName = parameterName;
			actionCharAnim.mecanimParameterType = MecanimParameterType.Float;
			actionCharAnim.parameterValue = parameterValue;
			return actionCharAnim;
		}

		public static ActionCharAnim CreateNew_Mecanim_ChangeParameterValue(Char characterToAnimate, string parameterName, bool parameterValue)
		{
			ActionCharAnim actionCharAnim = ScriptableObject.CreateInstance<ActionCharAnim>();
			actionCharAnim.animChar = characterToAnimate;
			actionCharAnim.methodMecanim = AnimMethodCharMecanim.ChangeParameterValue;
			actionCharAnim.parameterName = parameterName;
			actionCharAnim.mecanimParameterType = MecanimParameterType.Bool;
			actionCharAnim.parameterValue = ((!parameterValue) ? 0f : 1f);
			return actionCharAnim;
		}

		public static ActionCharAnim CreateNew_Mecanim_SetStandard(Char characterToAnimate, MecanimCharParameter parameterToChange, string newParameterName)
		{
			ActionCharAnim actionCharAnim = ScriptableObject.CreateInstance<ActionCharAnim>();
			actionCharAnim.animChar = characterToAnimate;
			actionCharAnim.methodMecanim = AnimMethodCharMecanim.SetStandard;
			actionCharAnim.mecanimCharParameter = parameterToChange;
			actionCharAnim.parameterName = newParameterName;
			return actionCharAnim;
		}

		public static ActionCharAnim CreateNew_Mecanim_PlayCustom(Char characterToAnimate, string clipName, int layerIndex = 0, float transitionTime = 0f, bool waitUntilFinish = true)
		{
			ActionCharAnim actionCharAnim = ScriptableObject.CreateInstance<ActionCharAnim>();
			actionCharAnim.animChar = characterToAnimate;
			actionCharAnim.methodMecanim = AnimMethodCharMecanim.PlayCustom;
			actionCharAnim.clip2D = clipName;
			actionCharAnim.includeDirection = false;
			actionCharAnim.layerInt = layerIndex;
			actionCharAnim.fadeTime = transitionTime;
			actionCharAnim.willWait = waitUntilFinish;
			return actionCharAnim;
		}
	}
}
