using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace AC
{
	[RequireComponent(typeof(Animator))]
	[AddComponentMenu("Adventure Creator/Save system/Remember Animator")]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_remember_animator.html")]
	public class RememberAnimator : Remember
	{
		[Serializable]
		private struct DefaultAnimParameter
		{
			public int intValue;

			public float floatValue;

			public DefaultAnimParameter(int _intValue)
			{
				intValue = _intValue;
				floatValue = 0f;
			}

			public DefaultAnimParameter(float _floatValue)
			{
				intValue = 0;
				floatValue = _floatValue;
			}
		}

		[SerializeField]
		private bool saveController;

		[SerializeField]
		private bool setDefaultParameterValues;

		[SerializeField]
		private List<DefaultAnimParameter> defaultAnimParameters = new List<DefaultAnimParameter>();

		private Animator _animator;

		private bool loadedData;

		private Animator Animator
		{
			get
			{
				if (_animator == null || !Application.isPlaying)
				{
					_animator = GetComponent<Animator>();
				}
				return _animator;
			}
		}

		private void Awake()
		{
			if (loadedData || !GameIsPlaying() || !setDefaultParameterValues)
			{
				return;
			}
			for (int i = 0; i < Animator.parameters.Length; i++)
			{
				if (i < defaultAnimParameters.Count)
				{
					string text = Animator.parameters[i].name;
					switch (Animator.parameters[i].type)
					{
					case AnimatorControllerParameterType.Bool:
						Animator.SetBool(text, defaultAnimParameters[i].intValue == 1);
						break;
					case AnimatorControllerParameterType.Float:
						Animator.SetFloat(text, defaultAnimParameters[i].floatValue);
						break;
					case AnimatorControllerParameterType.Int:
						Animator.SetInteger(text, defaultAnimParameters[i].intValue);
						break;
					}
				}
			}
		}

		public override string SaveData()
		{
			AnimatorData animatorData = new AnimatorData();
			animatorData.objectID = constantID;
			animatorData.savePrevented = savePrevented;
			if (saveController && Animator != null && Animator.runtimeAnimatorController != null)
			{
				animatorData.controllerID = AssetLoader.GetAssetInstanceID(Animator.runtimeAnimatorController);
			}
			animatorData.parameterData = ParameterValuesToString(Animator.parameters);
			animatorData.layerWeightData = LayerWeightsToString();
			animatorData.stateData = StatesToString();
			return Serializer.SaveScriptData<AnimatorData>(animatorData);
		}

		public override void LoadData(string stringData)
		{
			AnimatorData animatorData = Serializer.LoadScriptData<AnimatorData>(stringData);
			if (animatorData == null)
			{
				loadedData = false;
				return;
			}
			base.SavePrevented = animatorData.savePrevented;
			if (savePrevented)
			{
				return;
			}
			if (!string.IsNullOrEmpty(animatorData.controllerID) && Animator != null)
			{
				RuntimeAnimatorController runtimeAnimatorController = AssetLoader.RetrieveAsset(Animator.runtimeAnimatorController, animatorData.controllerID);
				if (runtimeAnimatorController != null)
				{
					_animator.runtimeAnimatorController = runtimeAnimatorController;
				}
			}
			StringToParameterValues(Animator.parameters, animatorData.parameterData);
			StringToLayerWeights(animatorData.layerWeightData);
			StringToStates(animatorData.stateData);
			loadedData = true;
		}

		private string ParameterValuesToString(AnimatorControllerParameter[] parameters)
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (AnimatorControllerParameter animatorControllerParameter in parameters)
			{
				switch (animatorControllerParameter.type)
				{
				case AnimatorControllerParameterType.Bool:
				{
					string value = ((!Animator.GetBool(animatorControllerParameter.name)) ? "0" : "1");
					stringBuilder.Append(value);
					break;
				}
				case AnimatorControllerParameterType.Float:
					stringBuilder.Append(Animator.GetFloat(animatorControllerParameter.name).ToString());
					break;
				case AnimatorControllerParameterType.Int:
					stringBuilder.Append(Animator.GetInteger(animatorControllerParameter.name).ToString());
					break;
				default:
					stringBuilder.Append("0");
					break;
				}
				stringBuilder.Append("|");
			}
			return stringBuilder.ToString();
		}

		private string LayerWeightsToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (Animator.layerCount > 1)
			{
				for (int i = 1; i < Animator.layerCount; i++)
				{
					stringBuilder.Append(Animator.GetLayerWeight(i).ToString());
					stringBuilder.Append("|");
				}
			}
			return stringBuilder.ToString();
		}

		private string StatesToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < Animator.layerCount; i++)
			{
				stringBuilder = ((!Animator.IsInTransition(i)) ? ProcessState(stringBuilder, Animator.GetCurrentAnimatorStateInfo(i)) : ProcessState(stringBuilder, Animator.GetNextAnimatorStateInfo(i)));
				stringBuilder.Append("|");
			}
			return stringBuilder.ToString();
		}

		private StringBuilder ProcessState(StringBuilder stateString, AnimatorStateInfo stateInfo)
		{
			int shortNameHash = stateInfo.shortNameHash;
			float num = stateInfo.normalizedTime;
			if (num > 1f)
			{
				num = ((!stateInfo.loop) ? 1f : (num % 1f));
			}
			stateString.Append(shortNameHash + "," + num);
			return stateString;
		}

		private void StringToParameterValues(AnimatorControllerParameter[] parameters, string valuesString)
		{
			if (string.IsNullOrEmpty(valuesString))
			{
				return;
			}
			string[] array = valuesString.Split("|"[0]);
			for (int i = 0; i < parameters.Length; i++)
			{
				if (i >= array.Length || array[i].Length <= 0)
				{
					continue;
				}
				string text = parameters[i].name;
				switch (parameters[i].type)
				{
				case AnimatorControllerParameterType.Bool:
					Animator.SetBool(text, array[i] == "1");
					break;
				case AnimatorControllerParameterType.Float:
				{
					float result2 = 0f;
					if (float.TryParse(array[i], out result2))
					{
						Animator.SetFloat(text, result2);
					}
					break;
				}
				case AnimatorControllerParameterType.Int:
				{
					int result = 0;
					if (int.TryParse(array[i], out result))
					{
						Animator.SetInteger(text, result);
					}
					break;
				}
				}
			}
		}

		private void StringToLayerWeights(string valuesString)
		{
			if (string.IsNullOrEmpty(valuesString) || Animator.layerCount <= 1)
			{
				return;
			}
			string[] array = valuesString.Split("|"[0]);
			for (int i = 1; i < Animator.layerCount; i++)
			{
				if (i < array.Length + 1 && array[i - 1].Length > 0)
				{
					float result = 1f;
					if (float.TryParse(array[i - 1], out result))
					{
						Animator.SetLayerWeight(i, result);
					}
				}
			}
		}

		private void StringToStates(string valuesString)
		{
			if (string.IsNullOrEmpty(valuesString))
			{
				return;
			}
			string[] array = valuesString.Split("|"[0]);
			for (int i = 0; i < Animator.layerCount; i++)
			{
				if (i >= array.Length || array[i].Length <= 0)
				{
					continue;
				}
				string[] array2 = array[i].Split(","[0]);
				if (array2.Length >= 2)
				{
					int result = 0;
					float result2 = 0f;
					if (int.TryParse(array2[0], out result) && float.TryParse(array2[1], out result2))
					{
						Animator.Play(result, i, result2);
					}
				}
			}
		}
	}
}
