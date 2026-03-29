using System;
using System.Collections.Generic;

namespace AC
{
	[Serializable]
	public class ActionSpeechWait : Action
	{
		public int constantID;

		public int parameterID = -1;

		public bool isPlayer;

		public Char speaker;

		protected Char runtimeSpeaker;

		public ActionSpeechWait()
		{
			isDisplayed = true;
			category = ActionCategory.Dialogue;
			title = "Wait for speech";
			description = "Waits until a particular character has stopped speaking.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
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
			if (isPlayer)
			{
				runtimeSpeaker = KickStarter.player;
			}
		}

		public override float Run()
		{
			if (runtimeSpeaker == null)
			{
				LogWarning("No speaker set.");
			}
			else if (!isRunning)
			{
				isRunning = true;
				if (KickStarter.dialog.CharacterIsSpeaking(runtimeSpeaker))
				{
					return base.defaultPauseTime;
				}
			}
			else
			{
				if (KickStarter.dialog.CharacterIsSpeaking(runtimeSpeaker))
				{
					return base.defaultPauseTime;
				}
				isRunning = false;
			}
			return 0f;
		}

		public override void Skip()
		{
		}
	}
}
