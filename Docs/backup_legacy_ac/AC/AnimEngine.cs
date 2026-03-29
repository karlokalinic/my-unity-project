using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class AnimEngine : ScriptableObject
	{
		public TurningStyle turningStyle = TurningStyle.Script;

		public bool isSpriteBased;

		protected Char character;

		public bool updateHeadAlways { get; protected set; }

		public virtual void Declare(Char _character)
		{
			character = _character;
			turningStyle = TurningStyle.Script;
			isSpriteBased = false;
		}

		public virtual void CharSettingsGUI()
		{
		}

		public virtual void CharExpressionsGUI()
		{
		}

		public virtual PlayerData SavePlayerData(PlayerData playerData, Player player)
		{
			return playerData;
		}

		public virtual void LoadPlayerData(PlayerData playerData, Player player)
		{
		}

		public virtual NPCData SaveNPCData(NPCData npcData, NPC npc)
		{
			return npcData;
		}

		public virtual void LoadNPCData(NPCData npcData, NPC npc)
		{
		}

		public virtual void ActionCharAnimGUI(ActionCharAnim action, List<ActionParameter> parameters = null)
		{
		}

		public virtual void ActionCharAnimAssignValues(ActionCharAnim action, List<ActionParameter> parameters)
		{
		}

		public virtual float ActionCharAnimRun(ActionCharAnim action)
		{
			return 0f;
		}

		public virtual void ActionCharAnimSkip(ActionCharAnim action)
		{
			ActionCharAnimRun(action);
		}

		public virtual bool ActionCharHoldPossible()
		{
			return false;
		}

		public virtual void ActionSpeechGUI(ActionSpeech action, Char speaker)
		{
		}

		public virtual void ActionSpeechRun(ActionSpeech action)
		{
		}

		public virtual void ActionSpeechSkip(ActionSpeech action)
		{
			ActionSpeechRun(action);
		}

		public virtual void ActionAnimGUI(ActionAnim action, List<ActionParameter> parameters)
		{
		}

		public virtual string ActionAnimLabel(ActionAnim action)
		{
			return string.Empty;
		}

		public virtual void ActionAnimAssignValues(ActionAnim action, List<ActionParameter> parameters)
		{
		}

		public virtual float ActionAnimRun(ActionAnim action)
		{
			return 0f;
		}

		public virtual void ActionAnimSkip(ActionAnim action)
		{
			ActionAnimRun(action);
		}

		public virtual void ActionCharRenderGUI(ActionCharRender action, List<ActionParameter> parameters)
		{
			ActionCharRenderGUI(action);
		}

		public virtual void ActionCharRenderGUI(ActionCharRender action)
		{
		}

		public virtual float ActionCharRenderRun(ActionCharRender action)
		{
			return 0f;
		}

		public virtual void PlayIdle()
		{
		}

		public virtual void PlayWalk()
		{
		}

		public virtual void PlayRun()
		{
		}

		public virtual void PlayTalk()
		{
		}

		public virtual void PlayVertical()
		{
		}

		public virtual void PlayJump()
		{
			PlayIdle();
		}

		public virtual void PlayTurnLeft()
		{
			PlayIdle();
		}

		public virtual void PlayTurnRight()
		{
			PlayIdle();
		}

		public virtual void TurnHead(Vector2 angles)
		{
		}

		public virtual void OnSetExpression()
		{
		}
	}
}
