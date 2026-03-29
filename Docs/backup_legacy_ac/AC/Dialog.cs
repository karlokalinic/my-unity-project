using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_dialog.html")]
	public class Dialog : MonoBehaviour
	{
		public List<Speech> speechList = new List<Speech>();

		public Sound narratorSound;

		[Range(0f, 1f)]
		public float conversationDelay = 0.3f;

		public string[] richTextTags = new string[4] { "b", "i", "size=", "color=" };

		protected AudioSource defaultAudioSource;

		protected AudioSource narratorAudioSource;

		protected string[] speechEventTokenKeys = new string[0];

		public string[] SpeechEventTokenKeys
		{
			get
			{
				return speechEventTokenKeys;
			}
			set
			{
				speechEventTokenKeys = value;
			}
		}

		public void OnAwake()
		{
			if ((bool)KickStarter.sceneSettings.defaultSound)
			{
				defaultAudioSource = KickStarter.sceneSettings.defaultSound.audioSource;
			}
		}

		public void _Update()
		{
			if (KickStarter.stateHandler.gameState != GameState.Paused)
			{
				for (int i = 0; i < speechList.Count; i++)
				{
					if (speechList[i].isAlive)
					{
						speechList[i].UpdateInput();
					}
				}
			}
			if (KickStarter.playerInput.InputGetButtonDown("EndConversation") && KickStarter.stateHandler.gameState == GameState.DialogOptions)
			{
				KickStarter.playerInput.EndConversation();
				KickStarter.actionListManager.OnEndConversation();
				KickStarter.actionListManager.SetCorrectGameState();
			}
		}

		public void _LateUpdate()
		{
			for (int i = 0; i < speechList.Count; i++)
			{
				speechList[i].UpdateDisplay();
				if (!speechList[i].isAlive)
				{
					EndSpeech(i);
					break;
				}
			}
		}

		protected void OnDestroy()
		{
			defaultAudioSource = null;
			narratorAudioSource = null;
		}

		public void UpdateSpeechVolumes()
		{
			for (int i = 0; i < speechList.Count; i++)
			{
				speechList[i].UpdateVolume();
			}
		}

		public Speech StartDialog(Char _speaker, string _text, bool isBackground = false, int lineID = -1, bool noAnimation = false, bool preventSkipping = false)
		{
			if (!KickStarter.runtimeLanguages.MarkLineAsSpoken(lineID))
			{
				return null;
			}
			if (!KickStarter.actionListManager.IsGameplayBlocked() && !KickStarter.stateHandler.IsInScriptedCutscene())
			{
				isBackground = true;
			}
			for (int i = 0; i < speechList.Count; i++)
			{
				if (speechList[i].GetSpeakingCharacter() == _speaker)
				{
					EndSpeech(i);
					i = 0;
				}
			}
			Speech speech = new Speech(_speaker, _text, lineID, isBackground, noAnimation, preventSkipping);
			speechList.Add(speech);
			KickStarter.runtimeVariables.AddToSpeechLog(speech.log);
			KickStarter.playerMenus.AssignSpeechToMenu(speech);
			if (speech.hasAudio)
			{
				if (KickStarter.speechManager.relegateBackgroundSpeechAudio)
				{
					EndBackgroundSpeechAudio(speech);
				}
				KickStarter.stateHandler.UpdateAllMaxVolumes();
			}
			return speech;
		}

		public Speech StartDialog(Char _speaker, int lineID, bool isBackground = false, bool noAnimation = false)
		{
			string empty = string.Empty;
			SpeechLine line = KickStarter.speechManager.GetLine(lineID);
			if (line != null)
			{
				empty = line.text;
				if (!KickStarter.runtimeLanguages.MarkLineAsSpoken(lineID))
				{
					return null;
				}
				for (int i = 0; i < speechList.Count; i++)
				{
					if (speechList[i].GetSpeakingCharacter() == _speaker)
					{
						EndSpeech(i);
						i = 0;
					}
				}
				Speech speech = new Speech(_speaker, empty, lineID, isBackground, noAnimation);
				speechList.Add(speech);
				KickStarter.runtimeVariables.AddToSpeechLog(speech.log);
				KickStarter.playerMenus.AssignSpeechToMenu(speech);
				if (speech.hasAudio)
				{
					if (KickStarter.speechManager.relegateBackgroundSpeechAudio)
					{
						EndBackgroundSpeechAudio(speech);
					}
					KickStarter.stateHandler.UpdateAllMaxVolumes();
				}
				return speech;
			}
			ACDebug.LogWarning("Cannot start dialog because the line ID " + lineID + " was not found in the Speech Manager.");
			return null;
		}

		public AudioSource GetNarratorAudioSource()
		{
			if (narratorAudioSource == null)
			{
				if (narratorSound != null)
				{
					narratorAudioSource = narratorSound.GetComponent<AudioSource>();
				}
				else
				{
					GameObject gameObject = new GameObject("Narrator");
					UnityVersionHandler.PutInFolder(gameObject, "_Sounds");
					narratorAudioSource = gameObject.AddComponent<AudioSource>();
					AdvGame.AssignMixerGroup(narratorAudioSource, SoundType.Speech);
					narratorAudioSource.spatialBlend = 0f;
					narratorSound = gameObject.AddComponent<Sound>();
					narratorSound.soundType = SoundType.Speech;
				}
			}
			narratorSound.SetMaxVolume();
			return narratorAudioSource;
		}

		public void PlayScrollAudio(Char _speaker)
		{
			AudioClip audioClip = KickStarter.speechManager.textScrollCLip;
			if (_speaker == null)
			{
				audioClip = KickStarter.speechManager.narrationTextScrollCLip;
			}
			else if (_speaker.textScrollClip != null)
			{
				audioClip = _speaker.textScrollClip;
			}
			if (!(audioClip != null))
			{
				return;
			}
			if ((bool)defaultAudioSource)
			{
				if (KickStarter.speechManager.playScrollAudioEveryCharacter || !defaultAudioSource.isPlaying)
				{
					defaultAudioSource.PlayOneShot(audioClip);
				}
			}
			else
			{
				ACDebug.LogWarning("Cannot play text scroll audio clip as no 'Default' sound prefab has been defined in the Scene Manager");
			}
		}

		public Speech GetLatestSpeech()
		{
			if (speechList.Count > 0)
			{
				return speechList[speechList.Count - 1];
			}
			return null;
		}

		public Speech GetLiveSpeechWithID(int lineID)
		{
			if (speechList.Count > 0)
			{
				foreach (Speech speech in speechList)
				{
					if (speech.LineID == lineID)
					{
						return speech;
					}
				}
			}
			return null;
		}

		public bool FoundAudio()
		{
			foreach (Speech speech in speechList)
			{
				if (speech.hasAudio)
				{
					return true;
				}
			}
			return false;
		}

		public bool IsAnySpeechPlaying(bool ignoreBackgroundSpeech = false)
		{
			if (speechList.Count > 0)
			{
				if (!ignoreBackgroundSpeech)
				{
					return true;
				}
				if (KickStarter.stateHandler.IsInGameplay())
				{
					return false;
				}
				for (int i = 0; i < speechList.Count; i++)
				{
					if (!speechList[i].isBackground)
					{
						return true;
					}
				}
			}
			return false;
		}

		public string GetSpeaker(int languageNumber = 0)
		{
			if (speechList.Count > 0)
			{
				return GetLatestSpeech().GetSpeaker(languageNumber);
			}
			return string.Empty;
		}

		public bool CharacterIsSpeaking(Char _char)
		{
			for (int i = 0; i < speechList.Count; i++)
			{
				if (speechList[i].GetSpeakingCharacter() == _char)
				{
					return true;
				}
			}
			return false;
		}

		public Char GetSpeakingCharacter()
		{
			if (speechList.Count > 0)
			{
				return GetLatestSpeech().GetSpeakingCharacter();
			}
			return null;
		}

		public bool AudioIsPlaying()
		{
			if (Options.optionsData != null && Options.optionsData.speechVolume > 0f)
			{
				for (int i = 0; i < speechList.Count; i++)
				{
					if (speechList[i].hasAudio && speechList[i].isAlive)
					{
						return true;
					}
				}
			}
			return false;
		}

		public void KillDialog(bool stopCharacter, bool forceMenusOff, SpeechMenuLimit speechMenuLimit = SpeechMenuLimit.All, SpeechMenuType speechMenuType = SpeechMenuType.All, string limitToCharacters = "")
		{
			bool flag = false;
			for (int i = 0; i < speechList.Count; i++)
			{
				if (speechList[i].HasConditions(speechMenuLimit, speechMenuType, limitToCharacters))
				{
					EndSpeech(i, stopCharacter);
					flag = true;
					i = 0;
				}
			}
			if (flag)
			{
				KickStarter.stateHandler.UpdateAllMaxVolumes();
				if (forceMenusOff)
				{
					KickStarter.playerMenus.ForceOffSubtitles();
				}
			}
		}

		public void KillDialog(Speech speech)
		{
			if (speech != null)
			{
				if (speechList.Contains(speech))
				{
					EndSpeech(speechList.IndexOf(speech), true);
				}
				else
				{
					ACDebug.Log("Cannot kill dialog '" + speech.log.fullText + "' because it is not in the speech list.");
				}
			}
			KickStarter.stateHandler.UpdateAllMaxVolumes();
		}

		public virtual List<LipSyncShape> GenerateLipSyncShapes(LipSyncMode _lipSyncMode, int lineID, Char _speaker, string language = "", string _message = "")
		{
			List<LipSyncShape> list = new List<LipSyncShape>();
			list.Add(new LipSyncShape(0, 0f, KickStarter.speechManager.lipSyncSpeed));
			TextAsset textAsset = null;
			if (_lipSyncMode == LipSyncMode.Salsa2D)
			{
				return list;
			}
			if (lineID > -1 && _speaker != null && KickStarter.speechManager.searchAudioFiles && KickStarter.speechManager.UseFileBasedLipSyncing())
			{
				textAsset = KickStarter.runtimeLanguages.GetSpeechLipsyncFile<TextAsset>(lineID, _speaker);
			}
			if (_lipSyncMode == LipSyncMode.ReadPamelaFile && textAsset != null)
			{
				string[] separator = new string[3] { "\r\n", "\r", "\n" };
				string[] array = textAsset.text.Split(separator, StringSplitOptions.None);
				bool flag = false;
				float result = 24f;
				string[] array2 = array;
				foreach (string text in array2)
				{
					if (!flag)
					{
						if (text.Contains("framespersecond:"))
						{
							string[] array3 = text.Split(':');
							float.TryParse(array3[1], out result);
						}
						else if (text.Contains("[Speech]"))
						{
							flag = true;
						}
					}
					else
					{
						if (!text.Contains(":"))
						{
							continue;
						}
						string[] array4 = text.Split(':');
						float result2 = 0f;
						float.TryParse(array4[0], out result2);
						string text2 = array4[1].ToLower().Substring(0, array4[1].Length - 1);
						bool flag2 = false;
						foreach (string phoneme in KickStarter.speechManager.phonemes)
						{
							string[] array5 = phoneme.ToLower().Split("/"[0]);
							if (flag2)
							{
								continue;
							}
							string[] array6 = array5;
							foreach (string text3 in array6)
							{
								if (text2.Contains(text3) && text2.Length == text3.Length)
								{
									int frame = KickStarter.speechManager.phonemes.IndexOf(phoneme);
									list.Add(new LipSyncShape(frame, result2, KickStarter.speechManager.lipSyncSpeed, result));
									flag2 = true;
								}
							}
						}
						if (!flag2)
						{
							list.Add(new LipSyncShape(0, result2, KickStarter.speechManager.lipSyncSpeed, result));
						}
					}
				}
			}
			else if (_lipSyncMode == LipSyncMode.ReadSapiFile && textAsset != null)
			{
				string[] separator2 = new string[3] { "\r\n", "\r", "\n" };
				string[] array7 = textAsset.text.Split(separator2, StringSplitOptions.None);
				string[] array8 = array7;
				foreach (string text4 in array8)
				{
					if (!text4.StartsWith("phn "))
					{
						continue;
					}
					string[] array9 = text4.Split(' ');
					float result3 = 0f;
					float.TryParse(array9[1], out result3);
					string text5 = ((!array9[4].EndsWith(" ")) ? array9[4].ToLower() : array9[4].ToLower().Substring(0, array9[4].Length - 1));
					bool flag3 = false;
					foreach (string phoneme2 in KickStarter.speechManager.phonemes)
					{
						string text6 = phoneme2.ToLower();
						if (!text6.Contains(text5))
						{
							continue;
						}
						string[] array10 = text6.Split("/"[0]);
						if (flag3)
						{
							continue;
						}
						string[] array11 = array10;
						foreach (string text7 in array11)
						{
							if (text7 == text5)
							{
								int frame2 = KickStarter.speechManager.phonemes.IndexOf(phoneme2);
								list.Add(new LipSyncShape(frame2, result3, KickStarter.speechManager.lipSyncSpeed, 60f));
								flag3 = true;
							}
						}
					}
					if (!flag3)
					{
						list.Add(new LipSyncShape(0, result3, KickStarter.speechManager.lipSyncSpeed, 60f));
					}
				}
			}
			else if (_lipSyncMode == LipSyncMode.ReadPapagayoFile && textAsset != null)
			{
				string[] separator3 = new string[3] { "\r\n", "\r", "\n" };
				string[] array12 = textAsset.text.Split(separator3, StringSplitOptions.None);
				string[] array13 = array12;
				foreach (string text8 in array13)
				{
					if (string.IsNullOrEmpty(text8) || text8.Contains("MohoSwitch"))
					{
						continue;
					}
					string[] array14 = text8.Split(' ');
					if (array14.Length != 2)
					{
						continue;
					}
					float result4 = 0f;
					if (!float.TryParse(array14[0], out result4))
					{
						continue;
					}
					string text9 = array14[1].ToLower().Substring(0, array14[1].Length);
					bool flag4 = false;
					if (text9.Contains("rest"))
					{
						continue;
					}
					foreach (string phoneme3 in KickStarter.speechManager.phonemes)
					{
						string[] array15 = phoneme3.ToLower().Split("/"[0]);
						if (flag4)
						{
							continue;
						}
						string[] array16 = array15;
						foreach (string text10 in array16)
						{
							if (text10 == text9)
							{
								int frame3 = KickStarter.speechManager.phonemes.IndexOf(phoneme3);
								list.Add(new LipSyncShape(frame3, result4, KickStarter.speechManager.lipSyncSpeed, 24f));
								flag4 = true;
								break;
							}
						}
					}
					if (!flag4 && !text9.Contains("etc"))
					{
						list.Add(new LipSyncShape(0, result4, KickStarter.speechManager.lipSyncSpeed, 24f));
					}
				}
			}
			else if (_lipSyncMode == LipSyncMode.FromSpeechText)
			{
				for (int num = 0; num < _message.Length; num++)
				{
					int num2 = Mathf.Min(5, _message.Length - num);
					for (int num3 = num2; num3 > 0; num3--)
					{
						string text11 = _message.Substring(num, num3);
						text11 = text11.ToLower();
						foreach (string phoneme4 in KickStarter.speechManager.phonemes)
						{
							string[] array17 = phoneme4.ToLower().Split("/"[0]);
							string[] array18 = array17;
							foreach (string text12 in array18)
							{
								if (text12 == text11)
								{
									int frame4 = KickStarter.speechManager.phonemes.IndexOf(phoneme4);
									list.Add(new LipSyncShape(frame4, num, KickStarter.speechManager.lipSyncSpeed));
									num += num3;
									num3 = Mathf.Min(5, _message.Length - num);
									break;
								}
							}
						}
					}
					list.Add(new LipSyncShape(0, num, KickStarter.speechManager.lipSyncSpeed));
				}
			}
			if (list.Count > 1)
			{
				list.Sort((LipSyncShape a, LipSyncShape b) => a.timeIndex.CompareTo(b.timeIndex));
			}
			return list;
		}

		public void EndSpeechByCharacter(Char character)
		{
			for (int i = 0; i < speechList.Count; i++)
			{
				if (speechList[i].GetSpeakingCharacter() == character)
				{
					EndSpeech(i, true);
					break;
				}
			}
		}

		protected void EndBackgroundSpeechAudio(Speech speech)
		{
			foreach (Speech speech2 in speechList)
			{
				if (speech2 != speech)
				{
					speech2.EndBackgroundSpeechAudio(speech.GetSpeakingCharacter());
				}
			}
		}

		protected void EndSpeech(int i, bool stopCharacter = false)
		{
			Speech speech = speechList[i];
			KickStarter.playerMenus.RemoveSpeechFromMenu(speech);
			if (stopCharacter)
			{
				if ((bool)speech.GetSpeakingCharacter())
				{
					speech.GetSpeakingCharacter().StopSpeaking();
				}
				else
				{
					speech.EndSpeechAudio();
				}
			}
			speech.isAlive = false;
			speechList.RemoveAt(i);
			if (speech.hasAudio)
			{
				KickStarter.stateHandler.UpdateAllMaxVolumes();
			}
			KickStarter.eventManager.Call_OnStopSpeech(speech, speech.GetSpeakingCharacter());
		}
	}
}
