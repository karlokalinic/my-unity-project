using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_speech.html")]
	public class Speech
	{
		private struct RichTextTag
		{
			public string openTag;

			public string closeTag;

			public bool usesParameters;

			public RichTextTag(string tag)
			{
				if (tag.Contains("="))
				{
					usesParameters = true;
					openTag = "<" + tag;
					closeTag = "</" + tag.Substring(0, tag.Length - 1) + ">";
				}
				else
				{
					usesParameters = false;
					openTag = "<" + tag + ">";
					closeTag = "</" + tag + ">";
				}
			}
		}

		private struct RichTextTagInstance
		{
			public int startIndex;

			public int endIndex;

			public string openText;

			public string closeText;

			public RichTextTagInstance(string _openText, string _closeText, int _startIndex)
			{
				startIndex = _startIndex;
				endIndex = 0;
				openText = _openText;
				closeText = _closeText;
			}

			public RichTextTagInstance(RichTextTagInstance instance, int _endIndex)
			{
				startIndex = instance.startIndex;
				openText = instance.openText;
				closeText = instance.closeText;
				endIndex = _endIndex;
			}

			public bool IsValid()
			{
				if (!string.IsNullOrEmpty(openText) && !string.IsNullOrEmpty(closeText) && endIndex > startIndex)
				{
					return true;
				}
				return false;
			}
		}

		public SpeechLog log;

		public bool isAlive;

		public bool continueFromSpeech;

		protected int gapIndex = -1;

		protected int continueIndex = -1;

		protected List<SpeechGap> speechGaps = new List<SpeechGap>();

		protected float endTime;

		protected float continueTime;

		protected float minSkipTime;

		protected bool preventSkipping;

		protected bool usingRichText;

		protected bool isSkippable;

		protected bool pauseGap;

		protected bool holdForever;

		protected float scrollAmount;

		protected float pauseEndTime;

		protected bool pauseIsIndefinite;

		protected AudioSource audioSource;

		protected bool isRTL;

		private List<RichTextTagInstance> richTextTagInstances = new List<RichTextTagInstance>();

		protected int currentCharIndex;

		protected float minDisplayTime;

		protected string realName;

		public string displayText { get; protected set; }

		public bool isBackground { get; protected set; }

		public bool hasAudio { get; protected set; }

		public Char speaker { get; protected set; }

		public string SpeakerName
		{
			get
			{
				return log.speakerName;
			}
		}

		public int LineID
		{
			get
			{
				return log.lineID;
			}
		}

		public string FullText
		{
			get
			{
				return log.fullText;
			}
		}

		public Speech(Char _speaker, string _message, int lineID, bool _isBackground, bool _noAnimation, bool _preventSkipping = false)
		{
			log.Clear();
			log.lineID = lineID;
			isRTL = KickStarter.runtimeLanguages.LanguageReadsRightToLeft(Options.GetLanguageName());
			isBackground = _isBackground;
			preventSkipping = _preventSkipping;
			realName = string.Empty;
			if (_speaker != null)
			{
				speaker = _speaker;
				speaker.isTalking = !_noAnimation;
				log.speakerName = (realName = _speaker.name);
				if ((bool)_speaker.GetComponent<Player>() && (KickStarter.settingsManager.playerSwitching == PlayerSwitching.Allow || !KickStarter.speechManager.usePlayerRealName))
				{
					log.speakerName = "Player";
				}
				Hotspot component = _speaker.GetComponent<Hotspot>();
				if (component != null && !string.IsNullOrEmpty(component.hotspotName))
				{
					log.speakerName = (realName = component.hotspotName);
				}
				if (KickStarter.speechManager.resetExpressionsEachLine)
				{
					_speaker.ClearExpression();
				}
				if (!_noAnimation)
				{
					if (KickStarter.speechManager.lipSyncMode == LipSyncMode.Off)
					{
						speaker.isLipSyncing = false;
					}
					else if (KickStarter.speechManager.lipSyncMode == LipSyncMode.Salsa2D || KickStarter.speechManager.lipSyncMode == LipSyncMode.FromSpeechText || KickStarter.speechManager.lipSyncMode == LipSyncMode.ReadPamelaFile || KickStarter.speechManager.lipSyncMode == LipSyncMode.ReadSapiFile || KickStarter.speechManager.lipSyncMode == LipSyncMode.ReadPapagayoFile)
					{
						speaker.StartLipSync(KickStarter.dialog.GenerateLipSyncShapes(KickStarter.speechManager.lipSyncMode, lineID, speaker, Options.GetVoiceLanguageName(), _message));
					}
					else if (KickStarter.speechManager.lipSyncMode == LipSyncMode.RogoLipSync)
					{
						RogoLipSyncIntegration.Play(_speaker, lineID, Options.GetVoiceLanguageName());
					}
				}
			}
			else
			{
				if ((bool)speaker)
				{
					speaker.isTalking = false;
				}
				speaker = null;
				log.speakerName = (realName = "Narrator");
			}
			if (CanScroll() && KickStarter.speechManager != null && KickStarter.speechManager.textScrollSpeed <= 0f)
			{
				ACDebug.LogWarning("Text Scroll Speed must be greater than zero - please amend your Speech Manager");
			}
			if (lineID > -1 && !string.IsNullOrEmpty(log.speakerName) && KickStarter.speechManager.searchAudioFiles)
			{
				AudioClip speechAudioClip = KickStarter.runtimeLanguages.GetSpeechAudioClip(lineID, speaker);
				if (speechAudioClip != null)
				{
					audioSource = null;
					if (speaker != null)
					{
						if (!_noAnimation && KickStarter.speechManager.lipSyncMode == LipSyncMode.FaceFX)
						{
							FaceFXIntegration.Play(speaker, log.speakerName + lineID, speechAudioClip);
						}
						if ((bool)speaker.speechAudioSource)
						{
							audioSource = speaker.speechAudioSource;
							speaker.SetSpeechVolume(Options.optionsData.speechVolume);
						}
						else
						{
							ACDebug.LogWarning(speaker.name + " has no audio source component!", speaker);
						}
					}
					else
					{
						audioSource = KickStarter.dialog.GetNarratorAudioSource();
						if (audioSource == null)
						{
							ACDebug.LogWarning("Cannot play audio for speech line '" + _message + "' as there is no AudioSource - assign a new 'Default Sound' in the Scene Manager.");
						}
					}
					if (audioSource != null)
					{
						audioSource.clip = speechAudioClip;
						audioSource.loop = false;
						audioSource.Play();
						hasAudio = true;
					}
				}
			}
			InitSpeech(_message, true);
			KickStarter.eventManager.Call_OnStartSpeech(this, speaker, log.fullText, log.lineID);
			if (CanScroll())
			{
				KickStarter.eventManager.Call_OnStartSpeechScroll(this, speaker, log.fullText, log.lineID);
			}
		}

		public Speech(string _message)
		{
			if (Application.isPlaying)
			{
				_message = DetermineGaps(_message);
				_message = DetermineRichTextTags(_message, KickStarter.dialog.richTextTags);
			}
			displayText = _message;
		}

		public Speech(Char _speaker, string _message)
		{
			log.Clear();
			if (Application.isPlaying)
			{
				_message = DetermineGaps(_message);
				log.textWithRichTextTags = _message;
				_message = DetermineRichTextTags(_message, KickStarter.dialog.richTextTags);
			}
			speaker = _speaker;
			displayText = _message;
			log.fullText = _message;
			log.lineID = -1;
			if (_speaker != null)
			{
				log.speakerName = (realName = _speaker.name);
				if ((bool)_speaker.GetComponent<Player>() && (KickStarter.settingsManager.playerSwitching == PlayerSwitching.Allow || !KickStarter.speechManager.usePlayerRealName))
				{
					log.speakerName = "Player";
				}
				Hotspot component = _speaker.GetComponent<Hotspot>();
				if (component != null && !string.IsNullOrEmpty(component.hotspotName))
				{
					log.speakerName = (realName = component.hotspotName);
				}
			}
			else
			{
				log.speakerName = (realName = "Narrator");
			}
		}

		public void UpdateVolume()
		{
			if ((bool)speaker)
			{
				speaker.SetSpeechVolume(Options.optionsData.speechVolume);
			}
		}

		public void UpdateDisplay()
		{
			if (minSkipTime > 0f)
			{
				minSkipTime -= Time.deltaTime;
			}
			if (minDisplayTime > 0f)
			{
				minDisplayTime -= Time.deltaTime;
			}
			if (pauseEndTime > 0f)
			{
				pauseEndTime -= Time.deltaTime;
			}
			if (pauseGap)
			{
				if (pauseIsIndefinite || !(pauseEndTime <= 0f))
				{
					return;
				}
				EndPause();
			}
			else if (hasAudio && !audioSource.isPlaying)
			{
				if (endTime > 0f)
				{
					endTime -= Time.deltaTime;
				}
			}
			else if (!hasAudio && (!CanScroll() || scrollAmount >= 1f) && endTime > 0f)
			{
				endTime -= Time.deltaTime;
			}
			if (hasAudio && !audioSource.isPlaying && speaker != null && speaker.isTalking)
			{
				speaker.StopSpeaking();
			}
			if (CanScroll())
			{
				if (scrollAmount < 1f)
				{
					if (pauseGap)
					{
						return;
					}
					scrollAmount += KickStarter.speechManager.textScrollSpeed * Time.deltaTime / 2f / (float)log.fullText.Length;
					if (scrollAmount >= 1f)
					{
						StopScrolling();
					}
					int num = ((!isRTL) ? ((int)(scrollAmount * (float)log.fullText.Length)) : ((int)((1f - scrollAmount) * (float)log.fullText.Length)));
					if (num == 0 || num != currentCharIndex)
					{
						currentCharIndex = num;
						displayText = GetTextPortion(log.fullText, currentCharIndex);
						if (!hasAudio)
						{
							KickStarter.dialog.PlayScrollAudio(speaker);
						}
					}
					if (gapIndex >= 0 && speechGaps.Count > gapIndex && HasPassedIndex(speechGaps[gapIndex].characterIndex))
					{
						SetPauseGap();
					}
					else if (continueIndex >= 0 && HasPassedIndex(continueIndex))
					{
						continueIndex = -1;
						continueFromSpeech = true;
					}
					return;
				}
				if (isRTL)
				{
					displayText = GetTextPortion(log.fullText, 0);
				}
				else
				{
					displayText = GetTextPortion(log.fullText, log.fullText.Length);
				}
			}
			else
			{
				if (gapIndex >= 0 && speechGaps.Count >= gapIndex)
				{
					if (gapIndex == speechGaps.Count)
					{
						displayText = log.fullText;
					}
					else
					{
						float num2 = speechGaps[gapIndex].waitTime;
						if (isRTL)
						{
							displayText = log.fullText.Substring(speechGaps[gapIndex].characterIndex);
						}
						else
						{
							displayText = log.fullText.Substring(0, speechGaps[gapIndex].characterIndex);
						}
						if (num2 >= 0f)
						{
							pauseEndTime = num2;
							pauseGap = true;
							speechGaps[gapIndex].CallEvent(this);
						}
						else if (speechGaps[gapIndex].expressionID >= 0)
						{
							speaker.SetExpression(speechGaps[gapIndex].expressionID);
							gapIndex++;
						}
						else
						{
							pauseIsIndefinite = true;
							pauseGap = true;
						}
					}
				}
				else
				{
					displayText = log.fullText;
				}
				if (continueIndex >= 0 && continueTime > 0f)
				{
					continueFromSpeech = true;
				}
			}
			if (!(endTime <= 0f) || !(minDisplayTime <= 0f))
			{
				return;
			}
			if (KickStarter.speechManager.displayForever)
			{
				if (isBackground)
				{
					EndMessage();
				}
				else if ((!hasAudio || !audioSource.isPlaying) && !KickStarter.speechManager.playAnimationForever && speaker != null && speaker.isTalking)
				{
					speaker.StopSpeaking();
				}
			}
			else if (!(speaker == null) || !KickStarter.speechManager.displayNarrationForever)
			{
				EndMessage();
			}
		}

		public void EndPause()
		{
			pauseEndTime = 0f;
			pauseGap = false;
			pauseIsIndefinite = false;
			gapIndex++;
			if (CanScroll())
			{
				KickStarter.eventManager.Call_OnStartSpeechScroll(this, speaker, log.fullText, log.lineID);
			}
		}

		public bool HasConditions(SpeechMenuLimit speechMenuLimit, SpeechMenuType speechMenuType, string limitToCharacters, SpeechProximityLimit speechProximityLimit = SpeechProximityLimit.NoLimit, float speechProximityDistance = 0f)
		{
			if (speaker != null && speechProximityDistance > 0f)
			{
				switch (speechProximityLimit)
				{
				case SpeechProximityLimit.LimitByDistanceToCamera:
				{
					float num2 = Vector3.Distance(speaker.transform.position, KickStarter.CameraMain.transform.position);
					if (num2 > speechProximityDistance)
					{
						return false;
					}
					break;
				}
				case SpeechProximityLimit.LimitByDistanceToPlayer:
					if (KickStarter.player != null)
					{
						float num = Vector3.Distance(speaker.transform.position, KickStarter.player.transform.position);
						if (num > speechProximityDistance)
						{
							return false;
						}
					}
					break;
				}
			}
			if (!limitToCharacters.StartsWith(";"))
			{
				limitToCharacters = ";" + limitToCharacters;
			}
			if (!limitToCharacters.EndsWith(";"))
			{
				limitToCharacters += ";";
			}
			if (speechMenuLimit == SpeechMenuLimit.All || (speechMenuLimit == SpeechMenuLimit.BlockingOnly && !isBackground) || (speechMenuLimit == SpeechMenuLimit.BackgroundOnly && isBackground))
			{
				if (speechMenuType == SpeechMenuType.All || (speechMenuType == SpeechMenuType.CharactersOnly && speaker != null) || (speechMenuType == SpeechMenuType.NarrationOnly && speaker == null))
				{
					return true;
				}
				if (speechMenuType == SpeechMenuType.SpecificCharactersOnly && speaker != null)
				{
					if (limitToCharacters.Contains(";" + GetSpeaker() + ";"))
					{
						return true;
					}
					if (limitToCharacters.Contains(";Player;") && speaker != null && speaker.IsPlayer)
					{
						return true;
					}
				}
				else if (speechMenuType == SpeechMenuType.AllExceptSpecificCharacters && GetSpeakingCharacter() != null)
				{
					if (limitToCharacters.Contains(";" + GetSpeaker() + ";"))
					{
						return false;
					}
					if (limitToCharacters.Contains(";Player;") && speaker != null && speaker.IsPlayer)
					{
						return false;
					}
					return true;
				}
			}
			return false;
		}

		public void UpdateInput()
		{
			if (!isSkippable)
			{
				return;
			}
			if (pauseGap && !IsBackgroundSpeech())
			{
				if (SkipSpeechInput())
				{
					if (speechGaps[gapIndex].waitTime < 0f)
					{
						KickStarter.playerInput.ResetMouseClick();
						EndPause();
					}
					else if (KickStarter.speechManager.allowSpeechSkipping)
					{
						KickStarter.playerInput.ResetMouseClick();
						EndPause();
					}
				}
			}
			else if ((KickStarter.speechManager.displayForever && !IsBackgroundSpeech()) || (KickStarter.speechManager.displayNarrationForever && !IsBackgroundSpeech() && speaker == null))
			{
				if (!SkipSpeechInput() || (holdForever && log.textWithRichTextTags == displayText))
				{
					return;
				}
				KickStarter.playerInput.ResetMouseClick();
				if (KickStarter.stateHandler.gameState == GameState.Cutscene)
				{
					if (KickStarter.speechManager.endScrollBeforeSkip && CanScroll() && displayText != log.textWithRichTextTags)
					{
						StopScrolling();
						if (speechGaps != null && speechGaps.Count > gapIndex)
						{
							for (int i = gapIndex; i < speechGaps.Count; i++)
							{
								if (gapIndex >= 0)
								{
									speechGaps[i].CallEvent(this);
								}
							}
							for (int num = speechGaps.Count - 1; num >= gapIndex; num--)
							{
								if (num >= 0 && speechGaps[num].expressionID >= 0)
								{
									speaker.SetExpression(speechGaps[num].expressionID);
									break;
								}
							}
						}
						if (continueIndex >= 0)
						{
							continueIndex = -1;
							continueFromSpeech = true;
						}
					}
					else
					{
						EndMessage(true);
					}
				}
				else
				{
					ACDebug.LogWarning("Cannot skip the line " + log.fullText + " because it is not background speech but the gameState is not a Cutscene! Either mark it as background speech, or initiate a scripted cutscene with AC.KickStarter.stateHandler.StartCutscene ();");
				}
			}
			else
			{
				if (!SkipSpeechInput() || ((!KickStarter.speechManager.allowSpeechSkipping || IsBackgroundSpeech()) && (!KickStarter.speechManager.allowSpeechSkipping || !KickStarter.speechManager.allowGameplaySpeechSkipping || !IsBackgroundSpeech()) && (!KickStarter.speechManager.displayForever || !KickStarter.speechManager.allowGameplaySpeechSkipping || !IsBackgroundSpeech()) && (!KickStarter.speechManager.displayNarrationForever || !(speaker == null) || !KickStarter.speechManager.allowGameplaySpeechSkipping || !IsBackgroundSpeech())))
				{
					return;
				}
				KickStarter.playerInput.ResetMouseClick();
				if (KickStarter.stateHandler.gameState != GameState.Cutscene && (!KickStarter.speechManager.allowGameplaySpeechSkipping || !KickStarter.stateHandler.IsInGameplay()))
				{
					return;
				}
				if (KickStarter.speechManager.endScrollBeforeSkip && CanScroll() && displayText != log.textWithRichTextTags)
				{
					if (speechGaps.Count > 0 && speechGaps.Count > gapIndex)
					{
						while (gapIndex < speechGaps.Count && speechGaps[gapIndex].waitTime >= 0f)
						{
							speechGaps[gapIndex].CallEvent(this);
							gapIndex++;
						}
						if (gapIndex == speechGaps.Count)
						{
							ExtendTime();
							StopScrolling();
							return;
						}
						if (isRTL)
						{
							displayText = log.fullText.Substring(speechGaps[gapIndex].characterIndex);
						}
						else
						{
							displayText = log.fullText.Substring(0, speechGaps[gapIndex].characterIndex);
						}
						SetPauseGap();
					}
					else
					{
						ExtendTime();
						StopScrolling();
					}
				}
				else
				{
					EndMessage(true);
				}
			}
		}

		public void EndBackgroundSpeechAudio(Char newSpeaker)
		{
			if (isBackground && hasAudio && speaker != null && speaker != newSpeaker && (bool)speaker.speechAudioSource)
			{
				speaker.speechAudioSource.Stop();
			}
		}

		public void EndSpeechAudio()
		{
			if (audioSource != null)
			{
				audioSource.Stop();
			}
		}

		public string GetSpeaker(int languageNumber = 0)
		{
			if ((bool)speaker)
			{
				return speaker.GetName(languageNumber);
			}
			return string.Empty;
		}

		public Color GetColour()
		{
			if ((bool)speaker)
			{
				return speaker.speechColor;
			}
			return Color.white;
		}

		public Char GetSpeakingCharacter()
		{
			return speaker;
		}

		public bool IsPaused()
		{
			return pauseGap;
		}

		public Sprite GetPortraitSprite()
		{
			if (speaker != null)
			{
				CursorIconBase portrait = speaker.GetPortrait();
				if (portrait != null && portrait.texture != null)
				{
					if (IsAnimating())
					{
						if (speaker.isLipSyncing)
						{
							return portrait.GetAnimatedSprite(speaker.GetLipSyncFrame());
						}
						return portrait.GetAnimatedSprite(speaker.isTalking);
					}
					return portrait.GetSprite();
				}
			}
			return null;
		}

		public Texture GetPortrait()
		{
			if ((bool)speaker && (bool)speaker.GetPortrait().texture)
			{
				return speaker.GetPortrait().texture;
			}
			return null;
		}

		public bool IsAnimating()
		{
			if ((bool)speaker && speaker.GetPortrait().isAnimated)
			{
				return true;
			}
			return false;
		}

		public Rect GetAnimatedRect()
		{
			if (speaker != null && speaker.GetPortrait() != null)
			{
				if (speaker.isLipSyncing)
				{
					return speaker.GetPortrait().GetAnimatedRect(speaker.GetLipSyncFrame());
				}
				if (speaker.isTalking)
				{
					return speaker.GetPortrait().GetAnimatedRect();
				}
				return speaker.GetPortrait().GetAnimatedRect(0);
			}
			return new Rect(0f, 0f, 0f, 0f);
		}

		public void ReplaceDisplayText(string newDisplayText, bool resetScrollAmount = true)
		{
			if (!string.IsNullOrEmpty(newDisplayText))
			{
				InitSpeech(newDisplayText, resetScrollAmount);
			}
		}

		public bool MenuCanShow(Menu menu)
		{
			if (menu != null)
			{
				return HasConditions(menu.speechMenuLimit, menu.speechMenuType, menu.limitToCharacters, menu.speechProximityLimit, menu.speechProximityDistance);
			}
			return false;
		}

		protected void ExtendTime()
		{
			if (CanScroll() && !KickStarter.speechManager.scrollingTextFactorsLength && !hasAudio)
			{
				float b = (1f - scrollAmount) * KickStarter.speechManager.screenTimeFactor * GetLengthWithoutRichText(log.fullText);
				endTime = Mathf.Max(endTime, b);
			}
		}

		protected void StopScrolling()
		{
			scrollAmount = 1f;
			displayText = log.textWithRichTextTags;
			if (holdForever)
			{
				continueFromSpeech = true;
			}
			KickStarter.eventManager.Call_OnEndSpeechScroll(this, speaker, log.fullText, log.lineID);
			KickStarter.eventManager.Call_OnCompleteSpeechScroll(this, speaker, log.fullText, log.lineID);
		}

		protected void SetPauseGap()
		{
			scrollAmount = (float)speechGaps[gapIndex].characterIndex / (float)log.fullText.Length;
			if (isRTL)
			{
				scrollAmount = 1f - scrollAmount;
			}
			float waitTime = speechGaps[gapIndex].waitTime;
			pauseGap = true;
			pauseIsIndefinite = false;
			if (speechGaps[gapIndex].pauseIsIndefinite)
			{
				pauseEndTime = 0f;
				pauseIsIndefinite = true;
			}
			else if (waitTime >= 0f)
			{
				pauseEndTime = waitTime;
				speechGaps[gapIndex].CallEvent(this);
			}
			else if (speechGaps[gapIndex].expressionID >= 0)
			{
				pauseEndTime = 0f;
				speaker.SetExpression(speechGaps[gapIndex].expressionID);
			}
			else
			{
				pauseEndTime = 0f;
			}
			if (pauseEndTime > 0f || pauseIsIndefinite)
			{
				KickStarter.eventManager.Call_OnEndSpeechScroll(this, speaker, log.fullText, log.lineID);
			}
		}

		protected string DetermineGaps(string _text)
		{
			speechGaps.Clear();
			continueIndex = -1;
			if (!string.IsNullOrEmpty(_text))
			{
				string[] speechEventTokenKeys = KickStarter.dialog.SpeechEventTokenKeys;
				for (int i = 0; i < _text.Length; i++)
				{
					string text = _text.Substring(0, i);
					string text2 = _text.Substring(i);
					if (text2.StartsWith("[continue]"))
					{
						continueIndex = i;
						_text = text + text2.Substring("[continue]".Length);
						CorrectPreviousGaps(continueIndex, 10);
						i = -1;
					}
					else if (text2.StartsWith("[hold]"))
					{
						if (continueIndex == -1)
						{
							continueIndex = i;
						}
						_text = text + text2.Substring("[hold]".Length);
						holdForever = true;
						CorrectPreviousGaps(continueIndex, 6);
						i = -1;
					}
					else if (text2.StartsWith("[expression:") && speaker != null)
					{
						int num = text2.IndexOf("]");
						string expressionLabel = text2.Substring(12, num - 12);
						int expressionID = speaker.GetExpressionID(expressionLabel);
						speechGaps.Add(new SpeechGap(i, expressionID));
						_text = text + text2.Substring(num + 1);
						i = -1;
					}
					else if (text2.StartsWith("[wait]"))
					{
						speechGaps.Add(new SpeechGap(i, true));
						_text = text + text2.Substring("[wait]".Length);
						i = -1;
					}
					else if (text2.StartsWith("[wait:"))
					{
						int num2 = text2.IndexOf("]");
						string text3 = text2.Substring(6, num2 - 6);
						speechGaps.Add(new SpeechGap(i, FloatParse(text3)));
						_text = text + text2.Substring(num2 + 1);
						i = -1;
					}
					else
					{
						if (speechEventTokenKeys == null)
						{
							continue;
						}
						string[] array = speechEventTokenKeys;
						foreach (string text4 in array)
						{
							if (!string.IsNullOrEmpty(text4))
							{
								string text5 = "[" + text4 + ":";
								if (text2.StartsWith(text5))
								{
									int num3 = text2.IndexOf("]");
									string tokenValue = text2.Substring(text5.Length, num3 - text5.Length);
									speechGaps.Add(new SpeechGap(i, text4, tokenValue));
									string text6 = KickStarter.eventManager.Call_OnRequestSpeechTokenReplacement(this, text4, tokenValue);
									_text = text + text6 + text2.Substring(num3 + 1);
									i = -1;
								}
							}
						}
					}
				}
			}
			if (speechGaps.Count > 1)
			{
				if (isRTL)
				{
					speechGaps.Sort((SpeechGap b, SpeechGap a) => a.characterIndex.CompareTo(b.characterIndex));
				}
				else
				{
					speechGaps.Sort((SpeechGap a, SpeechGap b) => a.characterIndex.CompareTo(b.characterIndex));
				}
			}
			return _text;
		}

		protected string DetermineRichTextTags(string _text, string[] tagNames)
		{
			if (!CanScroll())
			{
				return _text;
			}
			List<RichTextTag> list = new List<RichTextTag>();
			foreach (string text in tagNames)
			{
				if (!string.IsNullOrEmpty(text))
				{
					RichTextTag item = new RichTextTag(text);
					if (!list.Contains(item))
					{
						list.Add(item);
					}
				}
			}
			richTextTagInstances = new List<RichTextTagInstance>();
			if (!string.IsNullOrEmpty(_text))
			{
				for (int j = 0; j < _text.Length; j++)
				{
					string text2 = _text.Substring(0, j);
					string text3 = _text.Substring(j);
					foreach (RichTextTag item2 in list)
					{
						if (text3.StartsWith(item2.openTag))
						{
							int num = text3.IndexOf(">") + 1;
							if (num > 1)
							{
								string text4 = text3.Substring(0, num);
								_text = text2 + text3.Substring(text4.Length);
								CorrectPreviousGaps(j, text4.Length);
								richTextTagInstances.Add(new RichTextTagInstance(text4, item2.closeTag, j));
								j = -1;
								usingRichText = true;
							}
						}
						else
						{
							if (!text3.StartsWith(item2.closeTag))
							{
								continue;
							}
							_text = text2 + text3.Substring(item2.closeTag.Length);
							CorrectPreviousGaps(j, item2.closeTag.Length);
							for (int num2 = richTextTagInstances.Count - 1; num2 >= 0; num2--)
							{
								if (!richTextTagInstances[num2].IsValid() && richTextTagInstances[num2].closeText == item2.closeTag)
								{
									richTextTagInstances[num2] = new RichTextTagInstance(richTextTagInstances[num2], j);
								}
							}
							j = -1;
						}
					}
				}
			}
			return _text;
		}

		protected string FindSpeakerTag(string _message, string _speakerName)
		{
			if (!string.IsNullOrEmpty(_message) && _message.Contains("[speaker]"))
			{
				_message = _message.Replace("[speaker]", _speakerName);
			}
			return _message;
		}

		protected void CorrectPreviousGaps(int minCharIndex, int offset)
		{
			if (speechGaps.Count <= 0)
			{
				return;
			}
			for (int i = 0; i < speechGaps.Count; i++)
			{
				if (speechGaps[i].characterIndex > minCharIndex)
				{
					SpeechGap value = speechGaps[i];
					value.characterIndex -= offset;
					speechGaps[i] = value;
				}
			}
		}

		protected float FloatParse(string text)
		{
			float result = 0f;
			if (!string.IsNullOrEmpty(text))
			{
				float.TryParse(text, out result);
			}
			return result;
		}

		protected bool IsBackgroundSpeech()
		{
			return isBackground;
		}

		protected bool CanScroll()
		{
			if (speaker == null)
			{
				return KickStarter.speechManager.scrollNarration;
			}
			return KickStarter.speechManager.scrollSubtitles;
		}

		protected void EndMessage(bool forceOff = false)
		{
			if (holdForever)
			{
				continueFromSpeech = true;
				return;
			}
			endTime = 0f;
			isSkippable = false;
			if ((bool)speaker)
			{
				speaker.StopSpeaking();
			}
			EndSpeechAudio();
			if (!forceOff && gapIndex >= 0 && gapIndex < speechGaps.Count)
			{
				gapIndex++;
				return;
			}
			isAlive = false;
			KickStarter.stateHandler.UpdateAllMaxVolumes();
		}

		protected bool SkipSpeechInput()
		{
			if (minSkipTime > 0f || preventSkipping)
			{
				return false;
			}
			if (KickStarter.speechManager.canSkipWithMouseClicks && (KickStarter.playerInput.GetMouseState() == MouseState.SingleClick || KickStarter.playerInput.GetMouseState() == MouseState.RightClick))
			{
				return true;
			}
			if (KickStarter.playerInput.InputGetButtonDown("SkipSpeech"))
			{
				return true;
			}
			return false;
		}

		protected void InitSpeech(string _message, bool resetScrollAmount)
		{
			gapIndex = -1;
			continueIndex = -1;
			speechGaps = new List<SpeechGap>();
			endTime = 0f;
			continueTime = 0f;
			minSkipTime = 0f;
			usingRichText = false;
			isSkippable = false;
			pauseGap = false;
			holdForever = false;
			if (resetScrollAmount)
			{
				scrollAmount = 0f;
			}
			pauseEndTime = 0f;
			pauseIsIndefinite = false;
			richTextTagInstances = new List<RichTextTagInstance>();
			currentCharIndex = 0;
			minDisplayTime = 0f;
			if (Application.isPlaying)
			{
				_message = FindSpeakerTag(_message, realName);
				_message = DetermineGaps(_message);
				log.textWithRichTextTags = _message;
				_message = DetermineRichTextTags(_message, KickStarter.dialog.richTextTags);
			}
			gapIndex = ((speechGaps.Count <= 0) ? (-1) : 0);
			float num = 0f;
			if (hasAudio)
			{
				num = KickStarter.speechManager.screenTimeFactor * 5f;
			}
			else if (!CanScroll() || KickStarter.speechManager.scrollingTextFactorsLength)
			{
				float num2 = GetLengthWithoutRichText(_message) - GetTotalWaitTokenDuration();
				if (num2 < 0f)
				{
					num2 = 0.1f;
				}
				num = KickStarter.speechManager.screenTimeFactor * num2;
			}
			else
			{
				num = KickStarter.speechManager.screenTimeFactor * 5f;
			}
			num = Mathf.Max(num, 0.1f);
			log.fullText = _message;
			if (!CanScroll())
			{
				if (continueIndex > 0)
				{
					continueTime = (float)continueIndex / KickStarter.speechManager.textScrollSpeed;
				}
				if (speechGaps.Count > 0)
				{
					displayText = log.fullText.Substring(0, speechGaps[0].characterIndex);
				}
				else
				{
					displayText = log.fullText;
				}
			}
			else
			{
				displayText = string.Empty;
			}
			isAlive = true;
			isSkippable = true;
			pauseGap = false;
			endTime = num;
			minSkipTime = KickStarter.speechManager.skipThresholdTime;
			minDisplayTime = Mathf.Max(0f, KickStarter.speechManager.minimumDisplayTime);
			if (hasAudio && KickStarter.speechManager.syncSubtitlesToAudio && !KickStarter.speechManager.displayForever && (!(speaker == null) || !KickStarter.speechManager.displayNarrationForever))
			{
				minDisplayTime = 0f;
				endTime = 0.1f;
			}
			if (endTime <= 0f)
			{
				EndMessage();
			}
		}

		protected bool HasPassedIndex(int indexToCheck)
		{
			if (isRTL)
			{
				return currentCharIndex <= indexToCheck;
			}
			return currentCharIndex >= indexToCheck;
		}

		protected string GetTextPortion(string fullText, int index)
		{
			if (index <= 0)
			{
				if (!isRTL)
				{
					return string.Empty;
				}
				index = 0;
			}
			if (index > fullText.Length)
			{
				index = fullText.Length;
			}
			if (!usingRichText)
			{
				if (isRTL)
				{
					return fullText.Substring(index);
				}
				return fullText.Substring(0, index);
			}
			if (isRTL)
			{
				string text = fullText.Substring(index);
				int length = text.Length;
				int num = fullText.Length - length;
				string text2 = string.Empty;
				for (int num2 = fullText.Length; num2 >= num; num2--)
				{
					for (int num3 = richTextTagInstances.Count - 1; num3 >= 0; num3--)
					{
						RichTextTagInstance richTextTagInstance = richTextTagInstances[num3];
						if (richTextTagInstance.IsValid())
						{
							if (richTextTagInstance.startIndex == num2)
							{
								text = text.Insert(num2 - index, richTextTagInstance.openText);
							}
							else if (richTextTagInstance.endIndex == num2)
							{
								if (richTextTagInstance.startIndex < num)
								{
									text2 += richTextTagInstance.openText;
								}
								text = text.Insert(num2 - index, richTextTagInstance.closeText);
							}
						}
					}
				}
				return text2 + text;
			}
			string text3 = fullText.Substring(0, index);
			for (int num4 = index; num4 >= 0; num4--)
			{
				for (int num5 = richTextTagInstances.Count - 1; num5 >= 0; num5--)
				{
					RichTextTagInstance richTextTagInstance2 = richTextTagInstances[num5];
					if (richTextTagInstance2.IsValid())
					{
						if (richTextTagInstance2.endIndex == num4)
						{
							text3 = text3.Insert(num4, richTextTagInstance2.closeText);
						}
						else if (richTextTagInstance2.startIndex == num4)
						{
							if (richTextTagInstance2.endIndex > index)
							{
								text3 += richTextTagInstance2.closeText;
							}
							text3 = text3.Insert(num4, richTextTagInstance2.openText);
						}
					}
				}
			}
			return text3;
		}

		protected float GetLengthWithoutRichText(string _message)
		{
			_message = _message.Replace("[var:", string.Empty);
			_message = _message.Replace("[localvar:", string.Empty);
			_message = _message.Replace("[wait:", string.Empty);
			_message = _message.Replace("[continue:", string.Empty);
			_message = _message.Replace("[expression:", string.Empty);
			_message = _message.Replace("[paramlabel:", string.Empty);
			_message = _message.Replace("[param:", string.Empty);
			return _message.Length;
		}

		protected float GetTotalWaitTokenDuration()
		{
			if (speechGaps != null && (float)speechGaps.Count > 0f)
			{
				float num = 0f;
				{
					foreach (SpeechGap speechGap in speechGaps)
					{
						if (speechGap.waitTime > 0f)
						{
							num += speechGap.waitTime;
						}
					}
					return num;
				}
			}
			return 0f;
		}
	}
}
