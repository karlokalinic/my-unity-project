using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.XPath;
using UnityEngine;
using UnityEngine.Audio;

namespace AC
{
	public class AdvGame : ScriptableObject
	{
		public static List<Action> copiedActions = new List<Action>();

		private static References references = null;

		private static string tokenStart;

		private static int tokenIndex;

		private static int tokenValueStartIndex;

		private static int tokenValueEndIndex;

		private static string queuedCloneAnimSuffix = " - Queued Clone";

		public static void SetMixerVolume(AudioMixerGroup audioMixerGroup, string parameter, float volume)
		{
			if (audioMixerGroup != null && KickStarter.settingsManager.volumeControl == VolumeControl.AudioMixerGroups)
			{
				float value = ((!(volume > 0f)) ? (-80f) : (Mathf.Log10(volume) * 20f));
				audioMixerGroup.audioMixer.SetFloat(parameter, value);
			}
		}

		public static void AssignMixerGroup(AudioSource audioSource, SoundType soundType)
		{
			if (!(audioSource != null) || !(KickStarter.settingsManager != null) || KickStarter.settingsManager.volumeControl != VolumeControl.AudioMixerGroups || audioSource.outputAudioMixerGroup != null)
			{
				return;
			}
			switch (soundType)
			{
			case SoundType.Music:
				if ((bool)KickStarter.settingsManager.musicMixerGroup)
				{
					audioSource.outputAudioMixerGroup = KickStarter.settingsManager.musicMixerGroup;
				}
				else
				{
					ACDebug.LogWarning("Cannot assign " + audioSource.gameObject.name + " a music AudioMixerGroup!");
				}
				break;
			case SoundType.SFX:
				if ((bool)KickStarter.settingsManager.sfxMixerGroup)
				{
					audioSource.outputAudioMixerGroup = KickStarter.settingsManager.sfxMixerGroup;
				}
				else
				{
					ACDebug.LogWarning("Cannot assign " + audioSource.gameObject.name + " a sfx AudioMixerGroup!");
				}
				break;
			case SoundType.Speech:
				if ((bool)KickStarter.settingsManager.speechMixerGroup)
				{
					audioSource.outputAudioMixerGroup = KickStarter.settingsManager.speechMixerGroup;
				}
				else
				{
					ACDebug.LogWarning("Cannot assign " + audioSource.gameObject.name + " a speech AudioMixerGroup!");
				}
				break;
			}
		}

		public static int GetAnimLayerInt(AnimLayer animLayer)
		{
			int num = (int)animLayer;
			if (num > 4)
			{
				num++;
			}
			return num;
		}

		public static References GetReferences()
		{
			if (references == null)
			{
				references = (References)Resources.Load("References");
			}
			return references;
		}

		public static RuntimeActionList RunActionListAsset(ActionListAsset actionListAsset, int parameterID = -1, int parameterValue = 0)
		{
			if (parameterID >= 0 && actionListAsset != null && actionListAsset.NumParameters > 0)
			{
				ActionParameter parameter = actionListAsset.GetParameter(parameterID);
				if (parameter != null)
				{
					if (parameter.IsIntegerBased())
					{
						parameter.intValue = parameterValue;
					}
					else
					{
						ACDebug.LogWarning("Cannot update " + actionListAsset.name + "'s parameter '" + parameter.label + "' because it's value is not integer-based.", actionListAsset);
					}
				}
			}
			return RunActionListAsset(actionListAsset, null, 0, false, true);
		}

		public static RuntimeActionList RunActionListAsset(ActionListAsset actionListAsset, GameObject parameterValue)
		{
			if (actionListAsset != null && actionListAsset.NumParameters > 1)
			{
				ActionParameter actionParameter = actionListAsset.GetParameters()[0];
				if (actionParameter.parameterType == ParameterType.GameObject)
				{
					actionParameter.gameObject = parameterValue;
				}
				else
				{
					ACDebug.LogWarning("Cannot update " + actionListAsset.name + "'s parameter '" + actionParameter.label + "' because it is not a GameObject!", actionListAsset);
				}
			}
			return RunActionListAsset(actionListAsset, null, 0, false, true);
		}

		public static RuntimeActionList RunActionListAsset(ActionListAsset actionListAsset, int i, bool addToSkipQueue)
		{
			return RunActionListAsset(actionListAsset, null, i, false, addToSkipQueue);
		}

		public static RuntimeActionList RunActionListAsset(ActionListAsset actionListAsset, Conversation endConversation)
		{
			return RunActionListAsset(actionListAsset, endConversation, 0, false, true);
		}

		public static RuntimeActionList RunActionListAsset(ActionListAsset actionListAsset, Conversation endConversation, int i, bool doSkip, bool addToSkipQueue)
		{
			if (KickStarter.actionListAssetManager == null)
			{
				ACDebug.LogWarning("Cannot run an ActionList asset file without the presence of the Action List Asset Manager component - is this an AC scene?");
				return null;
			}
			if (actionListAsset != null && actionListAsset.actions.Count > 0)
			{
				int num = 0;
				foreach (ActiveList activeList in KickStarter.actionListAssetManager.activeLists)
				{
					if (activeList.IsFor(actionListAsset) && activeList.IsRunning())
					{
						num++;
					}
				}
				GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("RuntimeActionList"));
				gameObject.name = actionListAsset.name;
				if (num > 0)
				{
					gameObject.name = gameObject.name + " " + num;
				}
				RuntimeActionList component = gameObject.GetComponent<RuntimeActionList>();
				component.DownloadActions(actionListAsset, endConversation, i, doSkip, addToSkipQueue);
				return component;
			}
			return null;
		}

		public static RuntimeActionList SkipActionListAsset(ActionListAsset actionListAsset)
		{
			return RunActionListAsset(actionListAsset, null, 0, true, false);
		}

		public static RuntimeActionList SkipActionListAsset(ActionListAsset actionListAsset, int i, Conversation endConversation = null)
		{
			return RunActionListAsset(actionListAsset, endConversation, i, true, false);
		}

		public static double CalculateFormula(string formula)
		{
			try
			{
				return (double)new XPathDocument(new StringReader("<r/>")).CreateNavigator().Evaluate(string.Format("number({0})", new Regex("([\\+\\-\\*])").Replace(formula, " ${1} ").Replace("/", " div ").Replace("%", " mod ")));
			}
			catch
			{
				ACDebug.LogWarning("Cannot compute formula: " + formula);
				return 0.0;
			}
		}

		public static string CombineLanguageString(string string1, string string2, int langugeIndex, bool separateWithSpace = true)
		{
			if (string.IsNullOrEmpty(string1))
			{
				return string2;
			}
			if (string.IsNullOrEmpty(string2))
			{
				return string1;
			}
			if (KickStarter.runtimeLanguages.LanguageReadsRightToLeft(langugeIndex))
			{
				if (separateWithSpace)
				{
					return string2 + " " + string1;
				}
				return string2 + string1;
			}
			if (separateWithSpace)
			{
				return string1 + " " + string2;
			}
			return string1 + string2;
		}

		public static string ConvertTokens(string _text)
		{
			return ConvertTokens(_text, Options.GetLanguage());
		}

		public static string ConvertTokens(string _text, int languageNumber, LocalVariables localVariables = null, List<ActionParameter> parameters = null)
		{
			if (!Application.isPlaying)
			{
				return _text;
			}
			if (localVariables == null)
			{
				localVariables = KickStarter.localVariables;
			}
			if (!string.IsNullOrEmpty(_text))
			{
				if (KickStarter.runtimeVariables != null && KickStarter.eventManager != null && KickStarter.runtimeVariables.TextEventTokenKeys != null)
				{
					string[] textEventTokenKeys = KickStarter.runtimeVariables.TextEventTokenKeys;
					string[] array = textEventTokenKeys;
					foreach (string text in array)
					{
						if (string.IsNullOrEmpty(text))
						{
							continue;
						}
						tokenStart = "[" + text + ":";
						if (_text.Contains(tokenStart))
						{
							int startIndex = _text.IndexOf(tokenStart) + tokenStart.Length;
							int num = _text.Substring(startIndex).IndexOf("]");
							if (num > 0)
							{
								string text2 = _text.Substring(startIndex, num);
								string oldValue = tokenStart + text2 + "]";
								string newValue = KickStarter.eventManager.Call_OnRequestTextTokenReplacement(text, text2);
								_text = _text.Replace(oldValue, newValue);
							}
						}
					}
				}
				for (int num2 = 1; num2 > 0; num2--)
				{
					tokenStart = "[line:";
					tokenIndex = _text.IndexOf(tokenStart);
					if (tokenIndex >= 0)
					{
						tokenValueStartIndex = tokenIndex + tokenStart.Length;
						tokenValueEndIndex = _text.Substring(tokenValueStartIndex).IndexOf("]");
						if (tokenValueEndIndex > 0)
						{
							string text3 = _text.Substring(tokenValueStartIndex, tokenValueEndIndex);
							int result = -1;
							if (int.TryParse(text3, out result))
							{
								string currentLanguageText = KickStarter.runtimeLanguages.GetCurrentLanguageText(result);
								if (!string.IsNullOrEmpty(currentLanguageText))
								{
									string oldValue2 = tokenStart + text3 + "]";
									_text = _text.Replace(oldValue2, currentLanguageText);
									num2 = 2;
								}
							}
						}
					}
					if (parameters != null)
					{
						tokenStart = "[param:";
						tokenIndex = _text.IndexOf(tokenStart);
						if (tokenIndex >= 0)
						{
							tokenValueStartIndex = tokenIndex + tokenStart.Length;
							tokenValueEndIndex = _text.Substring(tokenValueStartIndex).IndexOf("]");
							if (tokenValueEndIndex > 0)
							{
								string text4 = _text.Substring(tokenValueStartIndex, tokenValueEndIndex);
								int result2 = -1;
								if (int.TryParse(text4, out result2))
								{
									foreach (ActionParameter parameter in parameters)
									{
										if (parameter.ID == result2)
										{
											string oldValue3 = tokenStart + text4 + "]";
											_text = _text.Replace(oldValue3, parameter.GetSaveData());
											num2 = 2;
										}
									}
								}
							}
						}
					}
					tokenStart = "[var:";
					tokenIndex = _text.IndexOf(tokenStart);
					if (tokenIndex >= 0)
					{
						tokenValueStartIndex = tokenIndex + tokenStart.Length;
						tokenValueEndIndex = _text.Substring(tokenValueStartIndex).IndexOf("]");
						if (tokenValueEndIndex > 0)
						{
							string text5 = _text.Substring(tokenValueStartIndex, tokenValueEndIndex);
							int result3 = -1;
							if (int.TryParse(text5, out result3))
							{
								GVar variable = GlobalVariables.GetVariable(result3, true);
								if (variable != null)
								{
									string oldValue4 = tokenStart + text5 + "]";
									_text = _text.Replace(oldValue4, variable.GetValue(languageNumber));
									num2 = 2;
								}
							}
						}
					}
					tokenStart = "[Var:";
					tokenIndex = _text.IndexOf(tokenStart);
					if (tokenIndex >= 0)
					{
						tokenValueStartIndex = tokenIndex + tokenStart.Length;
						tokenValueEndIndex = _text.Substring(tokenValueStartIndex).IndexOf("]");
						if (tokenValueEndIndex > 0)
						{
							string text6 = _text.Substring(tokenValueStartIndex, tokenValueEndIndex);
							int result4 = -1;
							if (int.TryParse(text6, out result4))
							{
								GVar variable2 = GlobalVariables.GetVariable(result4, true);
								if (variable2 != null)
								{
									string oldValue5 = tokenStart + text6 + "]";
									_text = _text.Replace(oldValue5, variable2.GetValue(languageNumber));
									num2 = 2;
								}
							}
						}
					}
					tokenStart = "[localvar:";
					tokenIndex = _text.IndexOf(tokenStart);
					if (tokenIndex >= 0)
					{
						tokenValueStartIndex = tokenIndex + tokenStart.Length;
						tokenValueEndIndex = _text.Substring(tokenValueStartIndex).IndexOf("]");
						if (tokenValueEndIndex > 0)
						{
							string text7 = _text.Substring(tokenValueStartIndex, tokenValueEndIndex);
							int result5 = -1;
							if (int.TryParse(text7, out result5))
							{
								GVar variable3 = LocalVariables.GetVariable(result5, localVariables);
								if (variable3 != null)
								{
									string oldValue6 = tokenStart + text7 + "]";
									_text = _text.Replace(oldValue6, variable3.GetValue(languageNumber));
									num2 = 2;
								}
							}
						}
					}
					tokenStart = "[compvar:";
					tokenIndex = _text.IndexOf(tokenStart);
					if (tokenIndex >= 0)
					{
						int num3 = tokenIndex + 9;
						int num4 = _text.Substring(num3).IndexOf(":");
						if (num4 > 0)
						{
							string text8 = _text.Substring(num3, num4);
							int result6 = 0;
							if (int.TryParse(text8, out result6) && result6 != 0)
							{
								Variables variables = Serializer.returnComponent<Variables>(result6);
								if (variables != null)
								{
									tokenValueStartIndex = num3 + num4 + 1;
									tokenValueEndIndex = _text.Substring(tokenValueStartIndex).IndexOf("]");
									if (tokenValueEndIndex > 0)
									{
										string text9 = _text.Substring(tokenValueStartIndex, tokenValueEndIndex);
										int result7 = -1;
										if (int.TryParse(text9, out result7))
										{
											GVar variable4 = variables.GetVariable(result7);
											if (variable4 != null)
											{
												string oldValue7 = tokenStart + text8 + ":" + text9 + "]";
												_text = _text.Replace(oldValue7, variable4.GetValue(languageNumber));
												num2 = 2;
											}
										}
									}
								}
							}
						}
					}
					if (parameters != null)
					{
						tokenStart = "[paramlabel:";
						tokenIndex = _text.IndexOf(tokenStart);
						if (tokenIndex >= 0)
						{
							tokenValueStartIndex = tokenIndex + tokenStart.Length;
							tokenValueEndIndex = _text.Substring(tokenValueStartIndex).IndexOf("]");
							if (tokenValueEndIndex > 0)
							{
								string text10 = _text.Substring(tokenValueStartIndex, tokenValueEndIndex);
								int result8 = -1;
								if (int.TryParse(text10, out result8))
								{
									foreach (ActionParameter parameter2 in parameters)
									{
										if (parameter2.ID == result8)
										{
											string oldValue8 = tokenStart + text10 + "]";
											_text = _text.Replace(oldValue8, parameter2.GetLabel());
											num2 = 2;
										}
									}
								}
							}
						}
					}
				}
				if ((bool)KickStarter.runtimeVariables)
				{
					_text = KickStarter.runtimeVariables.ConvertCustomTokens(_text);
				}
			}
			return _text;
		}

		public static Vector3 GetScreenDirection(Vector3 originWorldPosition, Vector3 targetWorldPosition)
		{
			Vector3 vector = KickStarter.CameraMain.WorldToScreenPoint(originWorldPosition);
			Vector3 vector2 = KickStarter.CameraMain.WorldToScreenPoint(targetWorldPosition);
			Vector3 result = vector2 - vector;
			result.z = result.y;
			result.y = 0f;
			return result;
		}

		public static Vector3 GetScreenNavMesh(Vector3 targetWorldPosition)
		{
			SettingsManager settingsManager = GetReferences().settingsManager;
			Vector3 pos = KickStarter.CameraMain.WorldToScreenPoint(targetWorldPosition);
			Ray ray = KickStarter.CameraMain.ScreenPointToRay(pos);
			RaycastHit hitInfo = default(RaycastHit);
			if ((bool)settingsManager && Physics.Raycast(ray, out hitInfo, settingsManager.navMeshRaycastLength, 1 << LayerMask.NameToLayer(settingsManager.navMeshLayer)))
			{
				return hitInfo.point;
			}
			return targetWorldPosition;
		}

		public static Matrix4x4 SetVanishingPoint(Camera _camera, Vector2 perspectiveOffset, bool accountForFOV = false)
		{
			Matrix4x4 projectionMatrix = _camera.projectionMatrix;
			float num = 2f * _camera.nearClipPlane / projectionMatrix.m00;
			float num2 = 2f * _camera.nearClipPlane / projectionMatrix.m11;
			float num3 = 0f - num / 2f + perspectiveOffset.x;
			float right = num3 + num;
			float num4 = 0f - num2 / 2f + perspectiveOffset.y;
			float top = num4 + num2;
			Matrix4x4 result = PerspectiveOffCenter(num3, right, num4, top, _camera.nearClipPlane, _camera.farClipPlane);
			if (accountForFOV)
			{
				float num5 = 1f / Mathf.Tan(_camera.fieldOfView / 114.59156f);
				float value = num5 / _camera.aspect;
				result[0, 0] = value;
				result[1, 1] = num5;
			}
			return result;
		}

		private static Matrix4x4 PerspectiveOffCenter(float left, float right, float bottom, float top, float near, float far)
		{
			float value = 2f * near / (right - left);
			float value2 = 2f * near / (top - bottom);
			float value3 = (right + left) / (right - left);
			float value4 = (top + bottom) / (top - bottom);
			float value5 = (0f - (far + near)) / (far - near);
			float value6 = (0f - 2f * far * near) / (far - near);
			float value7 = -1f;
			Matrix4x4 result = default(Matrix4x4);
			result[0, 0] = value;
			result[0, 1] = 0f;
			result[0, 2] = value3;
			result[0, 3] = 0f;
			result[1, 0] = 0f;
			result[1, 1] = value2;
			result[1, 2] = value4;
			result[1, 3] = 0f;
			result[2, 0] = 0f;
			result[2, 1] = 0f;
			result[2, 2] = value5;
			result[2, 3] = value6;
			result[3, 0] = 0f;
			result[3, 1] = 0f;
			result[3, 2] = value7;
			result[3, 3] = 0f;
			return result;
		}

		public static string UniqueName(string name)
		{
			if ((bool)GameObject.Find(name))
			{
				string result = name;
				for (int i = 2; i < 20; i++)
				{
					result = name + i;
					if (!GameObject.Find(result))
					{
						break;
					}
				}
				return result;
			}
			return name;
		}

		public static string GetName(string resourceName)
		{
			int num = resourceName.IndexOf("/");
			if (num > 0)
			{
				return resourceName.Remove(0, num + 1);
			}
			return resourceName;
		}

		public static Rect GUIBox(float centre_x, float centre_y, float size)
		{
			return GUIRect(centre_x, centre_y, size, size);
		}

		public static Rect GUIBox(Vector2 posVector, float size)
		{
			return GUIRect(posVector.x / (float)ACScreen.width, ((float)ACScreen.height - posVector.y) / (float)ACScreen.height, size, size);
		}

		public static Rect GUIRect(float centre_x, float centre_y, float width, float height)
		{
			return new Rect((float)ACScreen.width * centre_x - (float)ACScreen.width * width / 2f, (float)ACScreen.height * centre_y - (float)ACScreen.width * height / 2f, (float)ACScreen.width * width, (float)ACScreen.width * height);
		}

		private static void AddAnimClip(Animation _animation, int layer, AnimationClip clip, AnimationBlendMode blendMode, WrapMode wrapMode, Transform mixingBone)
		{
			if (clip != null && _animation != null)
			{
				string newName = clip.name;
				_animation.AddClip(clip, newName);
				if (mixingBone != null)
				{
					_animation[newName].AddMixingTransform(mixingBone);
				}
				if ((bool)_animation[clip.name])
				{
					_animation[newName].layer = layer;
					_animation[newName].normalizedTime = 0f;
					_animation[newName].blendMode = blendMode;
					_animation[newName].wrapMode = wrapMode;
					_animation[newName].enabled = true;
				}
			}
		}

		public static void PlayAnimClipFrame(Animation _animation, int layer, AnimationClip clip, AnimationBlendMode blendMode, WrapMode wrapMode, float fadeTime, Transform mixingBone, float normalisedFrame)
		{
			if (clip != null)
			{
				AddAnimClip(_animation, layer, clip, blendMode, wrapMode, mixingBone);
				_animation[clip.name].normalizedTime = normalisedFrame;
				_animation[clip.name].speed *= 1f;
				_animation.Play(clip.name);
				CleanUnusedClips(_animation);
			}
		}

		public static void PlayAnimClip(Animation _animation, int layer, AnimationClip clip, AnimationBlendMode blendMode = AnimationBlendMode.Blend, WrapMode wrapMode = WrapMode.ClampForever, float fadeTime = 0f, Transform mixingBone = null, bool reverse = false)
		{
			if (clip != null)
			{
				AddAnimClip(_animation, layer, clip, blendMode, wrapMode, mixingBone);
				if (reverse)
				{
					_animation[clip.name].speed *= -1f;
				}
				_animation.CrossFade(clip.name, fadeTime);
				CleanUnusedClips(_animation);
			}
		}

		public static void CleanUnusedClips(Animation _animation)
		{
			List<string> list = new List<string>();
			foreach (AnimationState item in _animation)
			{
				if (!_animation[item.name].enabled)
				{
					if (item.name.Contains(queuedCloneAnimSuffix))
					{
						list.Add(item.name.Replace(queuedCloneAnimSuffix, string.Empty));
					}
					else
					{
						list.Add(item.name);
					}
				}
			}
			foreach (string item2 in list)
			{
				_animation.RemoveClip(item2);
			}
		}

		public static float Lerp(float from, float to, float t)
		{
			if (t < 0f || t > 1f)
			{
				return from + (to - from) * t;
			}
			return Mathf.Lerp(from, to, t);
		}

		public static Vector3 Lerp(Vector3 from, Vector3 to, float t)
		{
			if (t < 0f || t > 1f)
			{
				return from + (to - from) * t;
			}
			return Vector3.Lerp(from, to, t);
		}

		public static Quaternion Lerp(Quaternion from, Quaternion to, float t)
		{
			if (t < 0f || t > 1f)
			{
				Vector3 eulerAngles = from.eulerAngles;
				Vector3 eulerAngles2 = to.eulerAngles;
				if (eulerAngles.x - eulerAngles2.x > 180f)
				{
					eulerAngles2.x -= 360f;
				}
				else if (eulerAngles.x - eulerAngles2.x > 180f)
				{
					eulerAngles2.x += 360f;
				}
				if (eulerAngles.y - eulerAngles2.y < -180f)
				{
					eulerAngles2.y -= 360f;
				}
				else if (eulerAngles.y - eulerAngles2.y > 180f)
				{
					eulerAngles2.y += 360f;
				}
				if (eulerAngles.z - eulerAngles2.z > 180f)
				{
					eulerAngles2.z -= 360f;
				}
				else if (eulerAngles.z - eulerAngles2.z > 180f)
				{
					eulerAngles2.z += 360f;
				}
				return Quaternion.Euler(Lerp(eulerAngles, eulerAngles2, t));
			}
			return Quaternion.Lerp(from, to, t);
		}

		public static float Interpolate(float startT, float deltaT, MoveMethod moveMethod, AnimationCurve timeCurve = null)
		{
			switch (moveMethod)
			{
			case MoveMethod.Smooth:
			case MoveMethod.Curved:
				return -0.5f * (Mathf.Cos((float)Math.PI * (Time.time - startT) / deltaT) - 1f);
			case MoveMethod.EaseIn:
				return 1f - Mathf.Cos((Time.time - startT) / deltaT * ((float)Math.PI / 2f));
			case MoveMethod.EaseOut:
				return Mathf.Sin((Time.time - startT) / deltaT * ((float)Math.PI / 2f));
			case MoveMethod.CustomCurve:
			{
				if (timeCurve == null || timeCurve.length == 0)
				{
					return 1f;
				}
				float time = timeCurve[0].time;
				float time2 = timeCurve[timeCurve.length - 1].time;
				return timeCurve.Evaluate((time2 - time) * (Time.time - startT) / deltaT + time);
			}
			default:
				return (Time.time - startT) / deltaT;
			}
		}

		public static float Interpolate(float weight, MoveMethod moveMethod, AnimationCurve timeCurve = null)
		{
			switch (moveMethod)
			{
			case MoveMethod.Smooth:
			case MoveMethod.Curved:
				return -0.5f * (Mathf.Cos((float)Math.PI * weight) - 1f);
			case MoveMethod.EaseIn:
				return 1f - Mathf.Cos(weight * ((float)Math.PI / 2f));
			case MoveMethod.EaseOut:
				return Mathf.Sin(weight * ((float)Math.PI / 2f));
			case MoveMethod.CustomCurve:
			{
				if (timeCurve == null || timeCurve.length == 0)
				{
					return 1f;
				}
				float time = timeCurve[0].time;
				float time2 = timeCurve[timeCurve.length - 1].time;
				return timeCurve.Evaluate((time2 - time) * weight + time);
			}
			default:
				return weight;
			}
		}

		public static void DrawTextEffect(Rect rect, string text, GUIStyle style, Color outColor, Color inColor, float size, TextEffects textEffects)
		{
			if (GetReferences().menuManager != null && GetReferences().menuManager.scaleTextEffects)
			{
				size = ACScreen.safeArea.width / 200f / size;
			}
			int num = 0;
			string text2 = text;
			if (text2 == null)
			{
				return;
			}
			while (num < text.Length && text.IndexOf("<color=", num) > 0)
			{
				int num2 = text2.IndexOf("<color=", num);
				int num3 = 0;
				if (text2.IndexOf(">", num2) > 0)
				{
					num3 = text2.IndexOf(">", num2);
				}
				if (num3 > 0)
				{
					text2 = text2.Substring(0, num2) + "<color=black>" + text2.Substring(num3 + 1);
				}
				num = num2 + num;
			}
			if (textEffects == TextEffects.Outline || textEffects == TextEffects.OutlineAndShadow)
			{
				DrawTextOutline(rect, text, style, outColor, inColor, size, text2);
			}
			if (textEffects == TextEffects.Shadow || textEffects == TextEffects.OutlineAndShadow)
			{
				DrawTextShadow(rect, text, style, outColor, inColor, size, text2);
			}
		}

		private static void DrawTextShadow(Rect rect, string text, GUIStyle style, Color outColor, Color inColor, float size, string effectText = "")
		{
			GUIStyle gUIStyle = new GUIStyle(style);
			Color color = GUI.color;
			if (effectText.Length == 0)
			{
				effectText = text;
			}
			if (style.normal.background != null)
			{
				GUI.Label(rect, string.Empty, style);
			}
			style.normal.background = null;
			outColor.a = GUI.color.a;
			style.normal.textColor = outColor;
			GUI.color = outColor;
			rect.x += size;
			GUI.Label(rect, effectText, style);
			rect.y += size;
			GUI.Label(rect, effectText, style);
			rect.x -= size;
			rect.y -= size;
			style.normal.textColor = inColor;
			GUI.color = color;
			GUI.Label(rect, text, style);
			style = gUIStyle;
		}

		private static void DrawTextOutline(Rect rect, string text, GUIStyle style, Color outColor, Color inColor, float size, string effectText = "")
		{
			float num = size * 0.5f;
			GUIStyle gUIStyle = new GUIStyle(style);
			Color color = GUI.color;
			if (effectText.Length == 0)
			{
				effectText = text;
			}
			if (style.normal.background != null)
			{
				GUI.Label(rect, string.Empty, style);
			}
			style.normal.background = null;
			outColor.a = GUI.color.a;
			style.normal.textColor = outColor;
			GUI.color = outColor;
			rect.x -= num;
			GUI.Label(rect, effectText, style);
			rect.y -= num;
			GUI.Label(rect, effectText, style);
			rect.x += num;
			GUI.Label(rect, effectText, style);
			rect.x += num;
			GUI.Label(rect, effectText, style);
			rect.y += num;
			GUI.Label(rect, effectText, style);
			rect.y += num;
			GUI.Label(rect, effectText, style);
			rect.x -= num;
			GUI.Label(rect, effectText, style);
			rect.x -= num;
			GUI.Label(rect, effectText, style);
			rect.x += num;
			rect.y -= num;
			style.normal.textColor = inColor;
			GUI.color = color;
			GUI.Label(rect, text, style);
			style = gUIStyle;
		}

		public static string PrepareStringForSaving(string _string)
		{
			_string = _string.Replace("|", "*PIPE*");
			_string = _string.Replace(":", "*COLON*");
			return _string;
		}

		public static string PrepareStringForLoading(string _string)
		{
			_string = _string.Replace("*PIPE*", "|");
			_string = _string.Replace("*COLON*", ":");
			return _string;
		}

		public static float SignedAngle(Vector2 from, Vector2 to)
		{
			float num = Vector2.Angle(from, to);
			float num2 = Mathf.Sign(from.x * to.y - from.y * to.x);
			return num * num2;
		}

		public static Vector3 GetCharLookVector(CharDirection direction, Char _character = null)
		{
			Vector3 forward = KickStarter.CameraMain.transform.forward;
			forward = new Vector3(forward.x, 0f, forward.z).normalized;
			if (SceneSettings.IsTopDown())
			{
				forward = -KickStarter.CameraMain.transform.forward;
			}
			else if (SceneSettings.CameraPerspective == CameraPerspective.TwoD)
			{
				forward = KickStarter.CameraMain.transform.up;
			}
			Vector3 vector = new Vector3(KickStarter.CameraMain.transform.right.x, 0f, KickStarter.CameraMain.transform.right.z);
			if (!KickStarter.settingsManager.IsInFirstPerson())
			{
				if (SceneSettings.CameraPerspective == CameraPerspective.TwoD)
				{
					vector -= new Vector3(0f, 0f, 0.01f);
				}
				else
				{
					vector -= forward * 0.01f;
				}
			}
			if (_character != null)
			{
				forward = _character.TransformForward;
				vector = _character.TransformRight;
			}
			Vector3 result = Vector3.zero;
			switch (direction)
			{
			case CharDirection.Down:
				result = -forward;
				break;
			case CharDirection.Left:
				result = -vector;
				break;
			case CharDirection.Right:
				result = vector;
				break;
			case CharDirection.Up:
				result = forward;
				break;
			case CharDirection.DownLeft:
				result = (-forward - vector).normalized;
				break;
			case CharDirection.DownRight:
				result = (-forward + vector).normalized;
				break;
			case CharDirection.UpLeft:
				result = (forward - vector).normalized;
				break;
			case CharDirection.UpRight:
				result = (forward + vector).normalized;
				break;
			}
			if (SceneSettings.IsTopDown())
			{
				return result;
			}
			if (SceneSettings.CameraPerspective == CameraPerspective.TwoD && _character == null)
			{
				return new Vector3(result.x, 0f, result.y).normalized;
			}
			return result;
		}
	}
}
