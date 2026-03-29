using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionSpeech : Action, ITranslatable
	{
		public int constantID;

		public int parameterID = -1;

		public int messageParameterID = -1;

		public bool isPlayer;

		public Char speaker;

		public string messageText;

		public int lineID;

		public int[] multiLineIDs;

		public bool isBackground;

		public bool noAnimation;

		public AnimationClip headClip;

		public AnimationClip mouthClip;

		public bool play2DHeadAnim;

		public string headClip2D = string.Empty;

		public int headLayer;

		public bool play2DMouthAnim;

		public string mouthClip2D = string.Empty;

		public int mouthLayer;

		public float waitTimeOffset;

		protected bool stopAction;

		protected int splitIndex;

		protected bool splitDelay;

		protected Char runtimeSpeaker;

		protected Speech speech;

		protected LocalVariables localVariables;

		protected bool runActionListInBackground;

		protected List<ActionParameter> ownParameters = new List<ActionParameter>();

		public static string[] stringSeparators = new string[2] { "\n", "\\n" };

		public Char Speaker
		{
			get
			{
				if (Application.isPlaying)
				{
					return runtimeSpeaker;
				}
				return speaker;
			}
		}

		public ActionSpeech()
		{
			isDisplayed = true;
			category = ActionCategory.Dialogue;
			title = "Play speech";
			description = "Makes a Character talk, or – if no Character is specified – displays a message. Subtitles only appear if they are enabled from the Options menu. A 'thinking' effect can be produced by opting to not play any animation.";
			lineID = -1;
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			if (parameters != null)
			{
				ownParameters = parameters;
			}
			runtimeSpeaker = AssignFile(parameters, parameterID, constantID, speaker);
			if (runtimeSpeaker != null && runtimeSpeaker is Player && KickStarter.settingsManager.playerSwitching == PlayerSwitching.Allow && KickStarter.player != null)
			{
				ConstantID component = speaker.GetComponent<ConstantID>();
				ConstantID component2 = KickStarter.player.GetComponent<ConstantID>();
				if ((component == null && component2 != null) || (component != null && component2 == null) || (component != null && component2 != null && component.constantID != component2.constantID))
				{
					Player player = runtimeSpeaker as Player;
					foreach (PlayerPrefab player2 in KickStarter.settingsManager.players)
					{
						if (player2 == null || !(player2.playerOb == player))
						{
							continue;
						}
						if (player.associatedNPCPrefab != null)
						{
							ConstantID component3 = player.associatedNPCPrefab.GetComponent<ConstantID>();
							if (component3 != null)
							{
								runtimeSpeaker = AssignFile(parameters, parameterID, component3.constantID, runtimeSpeaker);
							}
						}
						break;
					}
				}
			}
			messageText = AssignString(parameters, messageParameterID, messageText);
			if (isPlayer)
			{
				runtimeSpeaker = KickStarter.player;
			}
		}

		public override void AssignParentList(ActionList actionList)
		{
			if (actionList != null)
			{
				localVariables = UnityVersionHandler.GetLocalVariablesOfGameObject(actionList.gameObject);
				runActionListInBackground = actionList.actionListType == ActionListType.RunInBackground;
			}
			if (localVariables == null)
			{
				localVariables = KickStarter.localVariables;
			}
			base.AssignParentList(actionList);
		}

		public override float Run()
		{
			if (KickStarter.speechManager == null)
			{
				Log("No Speech Manager present");
				return 0f;
			}
			if ((bool)KickStarter.dialog && (bool)KickStarter.stateHandler)
			{
				if (!isRunning)
				{
					stopAction = false;
					isRunning = true;
					splitDelay = false;
					splitIndex = 0;
					StartSpeech();
					if (isBackground)
					{
						if (KickStarter.speechManager.separateLines)
						{
							string[] array = messageText.Split(stringSeparators, StringSplitOptions.None);
							if (array != null && array.Length > 1)
							{
								LogWarning("Cannot separate multiple speech lines when 'Is Background?' is checked - will only play '" + array[0] + "'");
							}
						}
						isRunning = false;
						return 0f;
					}
					return base.defaultPauseTime;
				}
				if (stopAction || (speech != null && speech.continueFromSpeech))
				{
					if (speech != null)
					{
						speech.continueFromSpeech = false;
					}
					isRunning = false;
					return 0f;
				}
				if (speech == null || !speech.isAlive)
				{
					if (KickStarter.speechManager.separateLines)
					{
						if (splitDelay)
						{
							splitDelay = false;
							StartSpeech();
							return base.defaultPauseTime;
						}
						splitIndex++;
						string[] array2 = messageText.Split(stringSeparators, StringSplitOptions.None);
						if (array2.Length > splitIndex)
						{
							if (KickStarter.speechManager.separateLinePause > 0f)
							{
								splitDelay = true;
								return KickStarter.speechManager.separateLinePause;
							}
							splitDelay = false;
							StartSpeech();
							return base.defaultPauseTime;
						}
					}
					if (waitTimeOffset <= 0f)
					{
						isRunning = false;
						return 0f;
					}
					stopAction = true;
					return waitTimeOffset;
				}
				return base.defaultPauseTime;
			}
			return 0f;
		}

		public override void Skip()
		{
			KickStarter.dialog.KillDialog(true, true, SpeechMenuLimit.All, SpeechMenuType.All, string.Empty);
			SpeechLog line = new SpeechLog
			{
				lineID = lineID,
				fullText = messageText
			};
			if ((bool)runtimeSpeaker)
			{
				line.speakerName = runtimeSpeaker.name;
				if (!noAnimation)
				{
					runtimeSpeaker.isTalking = false;
					if (runtimeSpeaker.GetAnimEngine() != null)
					{
						runtimeSpeaker.GetAnimEngine().ActionSpeechSkip(this);
					}
				}
			}
			KickStarter.runtimeVariables.AddToSpeechLog(line);
		}

		public string GetTranslatableString(int index)
		{
			if (KickStarter.speechManager.separateLines)
			{
				return GetSpeechArray()[index];
			}
			return messageText;
		}

		public int GetTranslationID(int index)
		{
			if (index == 0)
			{
				return lineID;
			}
			return multiLineIDs[index - 1];
		}

		protected string[] GetSpeechArray()
		{
			string text = messageText.Replace("\\n", "\n");
			return text.Split(stringSeparators, StringSplitOptions.None);
		}

		protected void StartSpeech()
		{
			string translation = messageText;
			int num = lineID;
			int language = Options.GetLanguage();
			if (language > 0)
			{
				translation = KickStarter.runtimeLanguages.GetTranslation(translation, lineID, language);
			}
			translation = translation.Replace("\\n", "\n");
			if (KickStarter.speechManager.separateLines)
			{
				string[] array = messageText.Replace("\\n", "\n").Split(stringSeparators, StringSplitOptions.None);
				if (array.Length > 1)
				{
					translation = array[splitIndex];
					if (splitIndex > 0)
					{
						num = ((multiLineIDs == null || multiLineIDs.Length <= splitIndex - 1) ? (-1) : multiLineIDs[splitIndex - 1]);
					}
					if (language > 0)
					{
						translation = KickStarter.runtimeLanguages.GetTranslation(translation, num, language);
					}
				}
			}
			if (!string.IsNullOrEmpty(translation))
			{
				translation = AdvGame.ConvertTokens(translation, language, localVariables, ownParameters);
				speech = KickStarter.dialog.StartDialog(runtimeSpeaker, translation, isBackground || runActionListInBackground, num, noAnimation);
				if (runtimeSpeaker != null && !noAnimation && speech != null && runtimeSpeaker.GetAnimEngine() != null)
				{
					runtimeSpeaker.GetAnimEngine().ActionSpeechRun(this);
				}
			}
		}

		public static ActionSpeech CreateNew(Char charToSpeak, string subtitleText, int translationID = -1, bool waitUntilFinish = true)
		{
			ActionSpeech actionSpeech = ScriptableObject.CreateInstance<ActionSpeech>();
			actionSpeech.speaker = charToSpeak;
			actionSpeech.messageText = subtitleText;
			actionSpeech.isBackground = !waitUntilFinish;
			actionSpeech.lineID = translationID;
			return actionSpeech;
		}

		public static ActionSpeech CreateNew(Char characterToSpeak, string subtitleText, int[] translationIDs, bool waitUntilFinish = true)
		{
			ActionSpeech actionSpeech = ScriptableObject.CreateInstance<ActionSpeech>();
			actionSpeech.speaker = characterToSpeak;
			actionSpeech.messageText = subtitleText;
			actionSpeech.isBackground = !waitUntilFinish;
			actionSpeech.lineID = -1;
			if (translationIDs != null && translationIDs.Length > 0)
			{
				actionSpeech.multiLineIDs = new int[translationIDs.Length - 1];
				for (int i = 0; i < translationIDs.Length; i++)
				{
					if (i == 0)
					{
						actionSpeech.lineID = translationIDs[i];
					}
					else
					{
						actionSpeech.multiLineIDs[i - 1] = translationIDs[i];
					}
				}
			}
			return actionSpeech;
		}
	}
}
