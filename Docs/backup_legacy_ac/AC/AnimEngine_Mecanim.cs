using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	public class AnimEngine_Mecanim : AnimEngine
	{
		public override void Declare(Char _character)
		{
			character = _character;
			turningStyle = TurningStyle.RootMotion;
			base.updateHeadAlways = character != null && character.ikHeadTurning;
		}

		public override void CharSettingsGUI()
		{
		}

		public override void CharExpressionsGUI()
		{
		}

		public override PlayerData SavePlayerData(PlayerData playerData, Player player)
		{
			playerData.playerWalkAnim = player.moveSpeedParameter;
			playerData.playerTalkAnim = player.talkParameter;
			playerData.playerRunAnim = player.turnParameter;
			return playerData;
		}

		public override void LoadPlayerData(PlayerData playerData, Player player)
		{
			player.moveSpeedParameter = playerData.playerWalkAnim;
			player.talkParameter = playerData.playerTalkAnim;
			player.turnParameter = playerData.playerRunAnim;
		}

		public override NPCData SaveNPCData(NPCData npcData, NPC npc)
		{
			npcData.walkAnim = npc.moveSpeedParameter;
			npcData.talkAnim = npc.talkParameter;
			npcData.runAnim = npc.turnParameter;
			return npcData;
		}

		public override void LoadNPCData(NPCData npcData, NPC npc)
		{
			npc.moveSpeedParameter = npcData.walkAnim;
			npc.talkParameter = npcData.talkAnim;
			npc.turnParameter = npcData.runAnim;
		}

		public override void ActionSpeechGUI(ActionSpeech action, Char speaker)
		{
		}

		public override void ActionSpeechRun(ActionSpeech action)
		{
			if (!(character.GetAnimator() == null))
			{
				if (!string.IsNullOrEmpty(action.headClip2D))
				{
					character.GetAnimator().CrossFade(action.headClip2D, 0.1f, character.headLayer);
				}
				if (!string.IsNullOrEmpty(action.mouthClip2D))
				{
					character.GetAnimator().CrossFade(action.mouthClip2D, 0.1f, character.mouthLayer);
				}
			}
		}

		public override void ActionSpeechSkip(ActionSpeech action)
		{
		}

		public override void ActionCharAnimGUI(ActionCharAnim action, List<ActionParameter> parameters = null)
		{
		}

		public override void ActionCharAnimAssignValues(ActionCharAnim action, List<ActionParameter> parameters)
		{
			if (action.methodMecanim == AnimMethodCharMecanim.ChangeParameterValue)
			{
				switch (action.mecanimParameterType)
				{
				case MecanimParameterType.Bool:
				{
					BoolValue field = ((!(action.parameterValue <= 0f)) ? BoolValue.True : BoolValue.False);
					field = action.AssignBoolean(parameters, action.parameterValueParameterID, field);
					action.parameterValue = ((field != BoolValue.True) ? 0f : 1f);
					break;
				}
				case MecanimParameterType.Int:
					action.parameterValue = action.AssignInteger(parameters, action.parameterValueParameterID, (int)action.parameterValue);
					break;
				case MecanimParameterType.Float:
					action.parameterValue = action.AssignFloat(parameters, action.parameterValueParameterID, action.parameterValue);
					break;
				}
			}
		}

		public override float ActionCharAnimRun(ActionCharAnim action)
		{
			return ActionCharAnimProcess(action, false);
		}

		public override void ActionCharAnimSkip(ActionCharAnim action)
		{
			ActionCharAnimProcess(action, true);
		}

		protected float ActionCharAnimProcess(ActionCharAnim action, bool isSkipping)
		{
			if (action.methodMecanim == AnimMethodCharMecanim.SetStandard)
			{
				if (action.mecanimCharParameter == MecanimCharParameter.MoveSpeedFloat)
				{
					if (!string.IsNullOrEmpty(action.parameterName))
					{
						character.moveSpeedParameter = action.parameterName;
					}
					if (action.changeSpeed)
					{
						character.walkSpeedScale = action.newSpeed;
						character.runSpeedScale = action.parameterValue;
					}
					if (action.changeSound)
					{
						if (action.standard == AnimStandard.Walk)
						{
							character.walkSound = action.newSound;
						}
						else if (action.standard == AnimStandard.Run)
						{
							character.runSound = action.newSound;
						}
					}
				}
				else if (action.mecanimCharParameter == MecanimCharParameter.TalkBool)
				{
					character.talkParameter = action.parameterName;
				}
				else if (action.mecanimCharParameter == MecanimCharParameter.TurnFloat)
				{
					character.turnParameter = action.parameterName;
				}
				return 0f;
			}
			if (character.GetAnimator() == null)
			{
				return 0f;
			}
			if (!action.isRunning)
			{
				action.isRunning = true;
				if (action.methodMecanim == AnimMethodCharMecanim.ChangeParameterValue)
				{
					if (!string.IsNullOrEmpty(action.parameterName))
					{
						if (action.mecanimParameterType == MecanimParameterType.Float)
						{
							character.GetAnimator().SetFloat(action.parameterName, action.parameterValue);
						}
						else if (action.mecanimParameterType == MecanimParameterType.Int)
						{
							character.GetAnimator().SetInteger(action.parameterName, (int)action.parameterValue);
						}
						else if (action.mecanimParameterType == MecanimParameterType.Bool)
						{
							bool value = ((action.parameterValue > 0f) ? true : false);
							character.GetAnimator().SetBool(action.parameterName, value);
						}
						else if (action.mecanimParameterType == MecanimParameterType.Trigger && (!isSkipping || action.parameterValue < 1f))
						{
							character.GetAnimator().SetTrigger(action.parameterName);
						}
					}
				}
				else if (action.methodMecanim == AnimMethodCharMecanim.PlayCustom && !string.IsNullOrEmpty(action.clip2D))
				{
					string text = action.clip2D;
					if (action.includeDirection)
					{
						text += character.GetSpriteDirection();
					}
					character.GetAnimator().CrossFade(text, action.fadeTime, action.layerInt);
					if (action.willWait)
					{
						return action.defaultPauseTime;
					}
				}
			}
			else if (action.methodMecanim == AnimMethodCharMecanim.PlayCustom && !string.IsNullOrEmpty(action.clip2D))
			{
				if (character.GetAnimator().GetCurrentAnimatorStateInfo(action.layerInt).normalizedTime < 0.98f)
				{
					return action.defaultPauseTime / 6f;
				}
				action.isRunning = false;
				return 0f;
			}
			return 0f;
		}

		public override bool ActionCharHoldPossible()
		{
			return true;
		}

		public override void ActionAnimGUI(ActionAnim action, List<ActionParameter> parameters)
		{
		}

		public override string ActionAnimLabel(ActionAnim action)
		{
			string text = string.Empty;
			if ((bool)action.animator)
			{
				text = action.animator.name;
				if (action.methodMecanim == AnimMethodMecanim.ChangeParameterValue && action.parameterName != string.Empty)
				{
					text = text + " - " + action.parameterName;
				}
				else if (action.methodMecanim == AnimMethodMecanim.BlendShape)
				{
					text += " - Shapekey";
				}
			}
			return text;
		}

		public override void ActionAnimAssignValues(ActionAnim action, List<ActionParameter> parameters)
		{
			action.runtimeAnimator = action.AssignFile(parameters, action.parameterID, action.constantID, action.animator);
			action.runtimeShapeObject = action.AssignFile(parameters, action.parameterID, action.constantID, action.shapeObject);
			if (action.methodMecanim == AnimMethodMecanim.ChangeParameterValue)
			{
				switch (action.mecanimParameterType)
				{
				case MecanimParameterType.Bool:
				{
					BoolValue field = ((!(action.parameterValue <= 0f)) ? BoolValue.True : BoolValue.False);
					field = action.AssignBoolean(parameters, action.parameterValueParameterID, field);
					action.parameterValue = ((field != BoolValue.True) ? 0f : 1f);
					break;
				}
				case MecanimParameterType.Int:
					action.parameterValue = action.AssignInteger(parameters, action.parameterValueParameterID, (int)action.parameterValue);
					break;
				case MecanimParameterType.Float:
					action.parameterValue = action.AssignFloat(parameters, action.parameterValueParameterID, action.parameterValue);
					break;
				}
			}
		}

		public override float ActionAnimRun(ActionAnim action)
		{
			return ActionAnimProcess(action, false);
		}

		public override void ActionAnimSkip(ActionAnim action)
		{
			if (action.methodMecanim == AnimMethodMecanim.BlendShape)
			{
				if (action.runtimeShapeObject != null)
				{
					action.runtimeShapeObject.Change(action.shapeKey, action.shapeValue, action.fadeTime);
				}
			}
			else
			{
				ActionAnimProcess(action, true);
			}
		}

		protected float ActionAnimProcess(ActionAnim action, bool isSkipping)
		{
			if (!action.isRunning)
			{
				action.isRunning = true;
				if (action.methodMecanim == AnimMethodMecanim.ChangeParameterValue && (bool)action.runtimeAnimator && !string.IsNullOrEmpty(action.parameterName))
				{
					if (action.mecanimParameterType == MecanimParameterType.Float)
					{
						action.runtimeAnimator.SetFloat(action.parameterName, action.parameterValue);
					}
					else if (action.mecanimParameterType == MecanimParameterType.Int)
					{
						action.runtimeAnimator.SetInteger(action.parameterName, (int)action.parameterValue);
					}
					else if (action.mecanimParameterType == MecanimParameterType.Bool)
					{
						bool value = ((action.parameterValue > 0f) ? true : false);
						action.runtimeAnimator.SetBool(action.parameterName, value);
					}
					else if (action.mecanimParameterType == MecanimParameterType.Trigger && (!isSkipping || action.parameterValue < 1f))
					{
						action.runtimeAnimator.SetTrigger(action.parameterName);
					}
					return 0f;
				}
				if (action.methodMecanim == AnimMethodMecanim.PlayCustom && (bool)action.runtimeAnimator)
				{
					if (!string.IsNullOrEmpty(action.clip2D))
					{
						try
						{
							action.runtimeAnimator.CrossFade(action.clip2D, action.fadeTime, action.layerInt);
						}
						catch
						{
						}
						if (action.willWait)
						{
							return action.defaultPauseTime;
						}
					}
				}
				else if (action.methodMecanim == AnimMethodMecanim.BlendShape && action.shapeKey > -1 && action.runtimeShapeObject != null)
				{
					action.runtimeShapeObject.Change(action.shapeKey, action.shapeValue, action.fadeTime);
					if (action.willWait)
					{
						return action.fadeTime;
					}
				}
			}
			else
			{
				if (action.methodMecanim == AnimMethodMecanim.BlendShape && action.runtimeShapeObject != null)
				{
					action.isRunning = false;
					return 0f;
				}
				if (action.methodMecanim == AnimMethodMecanim.PlayCustom && (bool)action.runtimeAnimator && !string.IsNullOrEmpty(action.clip2D))
				{
					if (action.runtimeAnimator.GetCurrentAnimatorStateInfo(action.layerInt).normalizedTime < 1f)
					{
						return action.defaultPauseTime / 6f;
					}
					action.isRunning = false;
					return 0f;
				}
			}
			return 0f;
		}

		public override void ActionCharRenderGUI(ActionCharRender action)
		{
		}

		public override float ActionCharRenderRun(ActionCharRender action)
		{
			if (action.renderLock_scale == RenderLock.Set)
			{
				character.lockScale = true;
				float num = (float)action.scale / 100f;
				if (character.spriteChild != null)
				{
					character.spriteScale = num;
				}
				else
				{
					character.transform.localScale = new Vector3(num, num, num);
				}
			}
			else if (action.renderLock_scale == RenderLock.Release)
			{
				character.lockScale = false;
			}
			return 0f;
		}

		public override void PlayIdle()
		{
			if (!(character.GetAnimator() == null))
			{
				MoveCharacter();
				AnimTalk(character.GetAnimator());
				if (!string.IsNullOrEmpty(character.turnParameter))
				{
					character.GetAnimator().SetFloat(character.turnParameter, character.GetTurnFloat());
				}
				if (character.IsPlayer && !string.IsNullOrEmpty(character.jumpParameter))
				{
					character.GetAnimator().SetBool(character.jumpParameter, character.IsJumping);
				}
			}
		}

		public override void PlayWalk()
		{
			if (!(character.GetAnimator() == null))
			{
				MoveCharacter();
				AnimTalk(character.GetAnimator());
				if (!string.IsNullOrEmpty(character.turnParameter))
				{
					character.GetAnimator().SetFloat(character.turnParameter, character.GetTurnFloat());
				}
				if (character.IsPlayer && !string.IsNullOrEmpty(character.jumpParameter))
				{
					character.GetAnimator().SetBool(character.jumpParameter, character.IsJumping);
				}
			}
		}

		protected void MoveCharacter()
		{
			if (!string.IsNullOrEmpty(character.moveSpeedParameter))
			{
				character.GetAnimator().SetFloat(character.moveSpeedParameter, character.GetMoveSpeed(true));
			}
		}

		public override void PlayRun()
		{
			if (!(character.GetAnimator() == null))
			{
				MoveCharacter();
				AnimTalk(character.GetAnimator());
				if (!string.IsNullOrEmpty(character.turnParameter))
				{
					character.GetAnimator().SetFloat(character.turnParameter, character.GetTurnFloat());
				}
				if (character.IsPlayer && !string.IsNullOrEmpty(character.jumpParameter))
				{
					character.GetAnimator().SetBool(character.jumpParameter, character.IsJumping);
				}
			}
		}

		public override void PlayTalk()
		{
			PlayIdle();
		}

		protected void AnimTalk(Animator animator)
		{
			if (!string.IsNullOrEmpty(character.talkParameter))
			{
				animator.SetBool(character.talkParameter, character.isTalking);
			}
			if (!string.IsNullOrEmpty(character.phonemeParameter) && character.LipSyncGameObject())
			{
				animator.SetInteger(character.phonemeParameter, character.GetLipSyncFrame());
			}
			if (!string.IsNullOrEmpty(character.expressionParameter) && character.useExpressions)
			{
				animator.SetInteger(character.expressionParameter, character.GetExpressionID());
			}
		}

		public override void PlayVertical()
		{
			if (!(character.GetAnimator() == null))
			{
				if (!string.IsNullOrEmpty(character.verticalMovementParameter))
				{
					character.GetAnimator().SetFloat(character.verticalMovementParameter, character.GetHeightChange());
				}
				if (!string.IsNullOrEmpty(character.isGroundedParameter))
				{
					character.GetAnimator().SetBool(character.isGroundedParameter, character.IsGrounded(true));
				}
			}
		}

		public override void PlayJump()
		{
			if (!(character.GetAnimator() == null) && character.IsPlayer)
			{
				Player player = (Player)character;
				if (!string.IsNullOrEmpty(player.jumpParameter))
				{
					character.GetAnimator().SetBool(player.jumpParameter, true);
				}
				AnimTalk(character.GetAnimator());
			}
		}

		public override void TurnHead(Vector2 angles)
		{
			if (!(character.GetAnimator() == null))
			{
				if (!string.IsNullOrEmpty(character.headYawParameter))
				{
					character.GetAnimator().SetFloat(character.headYawParameter, angles.x);
				}
				if (!string.IsNullOrEmpty(character.headPitchParameter))
				{
					character.GetAnimator().SetFloat(character.headPitchParameter, angles.y);
				}
			}
		}

		public override void OnSetExpression()
		{
			if (character.mapExpressionsToShapeable && character.GetShapeable() != null)
			{
				if (character.CurrentExpression != null)
				{
					character.GetShapeable().SetActiveKey(character.expressionGroupID, character.CurrentExpression.label, 100f, 0.2f, MoveMethod.Smooth, null);
				}
				else
				{
					character.GetShapeable().DisableAllKeys(character.expressionGroupID, 0.2f, MoveMethod.Smooth, null);
				}
			}
		}
	}
}
