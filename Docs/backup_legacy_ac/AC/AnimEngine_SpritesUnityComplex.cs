using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	public class AnimEngine_SpritesUnityComplex : AnimEngine
	{
		public override void Declare(Char _character)
		{
			character = _character;
			turningStyle = TurningStyle.Linear;
			isSpriteBased = true;
			_character.frameFlipping = AC_2DFrameFlipping.None;
			base.updateHeadAlways = true;
		}

		public override void CharSettingsGUI()
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

		protected float ActionCharAnimProcess(ActionCharAnim action, bool isSkipping)
		{
			if (action.methodMecanim == AnimMethodCharMecanim.SetStandard)
			{
				if (!string.IsNullOrEmpty(action.parameterName))
				{
					if (action.mecanimCharParameter == MecanimCharParameter.MoveSpeedFloat)
					{
						character.moveSpeedParameter = action.parameterName;
					}
					else if (action.mecanimCharParameter == MecanimCharParameter.TalkBool)
					{
						character.talkParameter = action.parameterName;
					}
					else if (action.mecanimCharParameter == MecanimCharParameter.TurnFloat)
					{
						character.turnParameter = action.parameterName;
					}
				}
				if (action.mecanimCharParameter == MecanimCharParameter.MoveSpeedFloat)
				{
					if (action.changeSpeed)
					{
						if (action.standard == AnimStandard.Walk)
						{
							character.walkSpeedScale = action.newSpeed;
						}
						else if (action.standard == AnimStandard.Run)
						{
							character.runSpeedScale = action.newSpeed;
						}
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
							bool value = false;
							if (action.parameterValue > 0f)
							{
								value = true;
							}
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
					character.GetAnimator().CrossFade(action.clip2D, action.fadeTime, action.layerInt);
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

		public override void ActionCharAnimSkip(ActionCharAnim action)
		{
			ActionCharAnimProcess(action, true);
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
			}
			return text;
		}

		public override void ActionAnimAssignValues(ActionAnim action, List<ActionParameter> parameters)
		{
			action.runtimeAnimator = action.AssignFile(parameters, action.parameterID, action.constantID, action.animator);
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
						bool value = false;
						if (action.parameterValue > 0f)
						{
							value = true;
						}
						action.runtimeAnimator.SetBool(action.parameterName, value);
					}
					else if (action.mecanimParameterType == MecanimParameterType.Trigger && (!isSkipping || action.parameterValue < 1f))
					{
						action.runtimeAnimator.SetTrigger(action.parameterName);
					}
					return 0f;
				}
				if (action.methodMecanim == AnimMethodMecanim.PlayCustom && (bool)action.runtimeAnimator && !string.IsNullOrEmpty(action.clip2D))
				{
					if (isSkipping)
					{
						action.runtimeAnimator.CrossFade(action.clip2D, action.fadeTime, action.layerInt);
						if (action.willWait)
						{
							return action.defaultPauseTime;
						}
					}
					else
					{
						action.runtimeAnimator.CrossFade(action.clip2D, 0f, action.layerInt);
					}
				}
			}
			else if (action.methodMecanim == AnimMethodMecanim.PlayCustom && (bool)action.runtimeAnimator && !string.IsNullOrEmpty(action.clip2D))
			{
				if (action.runtimeAnimator.GetCurrentAnimatorStateInfo(action.layerInt).normalizedTime < 1f)
				{
					return action.defaultPauseTime / 6f;
				}
				action.isRunning = false;
				return 0f;
			}
			return 0f;
		}

		public override void ActionAnimSkip(ActionAnim action)
		{
			ActionAnimProcess(action, true);
		}

		public override void ActionCharRenderGUI(ActionCharRender action, List<ActionParameter> parameters)
		{
		}

		public override float ActionCharRenderRun(ActionCharRender action)
		{
			if (action.renderLock_scale == RenderLock.Set)
			{
				character.lockScale = true;
				character.spriteScale = (float)action.scale / 100f;
			}
			else if (action.renderLock_scale == RenderLock.Release)
			{
				character.lockScale = false;
			}
			if (action.renderLock_direction == RenderLock.Set)
			{
				character.SetSpriteDirection(action.direction);
			}
			else if (action.renderLock_direction == RenderLock.Release)
			{
				character.lockDirection = false;
			}
			if (action.renderLock_sortingMap != RenderLock.NoChange && (bool)character.GetComponentInChildren<FollowSortingMap>())
			{
				FollowSortingMap[] componentsInChildren = character.GetComponentsInChildren<FollowSortingMap>();
				SortingMap sortingMap = ((action.renderLock_sortingMap != RenderLock.Set) ? KickStarter.sceneSettings.sortingMap : action.RuntimeSortingMap);
				FollowSortingMap[] array = componentsInChildren;
				foreach (FollowSortingMap followSortingMap in array)
				{
					followSortingMap.SetSortingMap(sortingMap);
				}
			}
			return 0f;
		}

		public override void PlayIdle()
		{
			if (!(character.GetAnimator() == null))
			{
				if (!string.IsNullOrEmpty(character.moveSpeedParameter))
				{
					character.GetAnimator().SetFloat(character.moveSpeedParameter, character.GetMoveSpeed());
				}
				AnimTalk(character.GetAnimator());
				if (!string.IsNullOrEmpty(character.turnParameter))
				{
					character.GetAnimator().SetFloat(character.turnParameter, 0f);
				}
				SetDirection(character.GetAnimator());
			}
		}

		public override void PlayWalk()
		{
			if (!(character.GetAnimator() == null))
			{
				if (!string.IsNullOrEmpty(character.moveSpeedParameter))
				{
					character.GetAnimator().SetFloat(character.moveSpeedParameter, character.GetMoveSpeed());
				}
				if (!string.IsNullOrEmpty(character.turnParameter))
				{
					character.GetAnimator().SetFloat(character.turnParameter, 0f);
				}
				AnimTalk(character.GetAnimator());
				SetDirection(character.GetAnimator());
			}
		}

		public override void PlayRun()
		{
			if (!(character.GetAnimator() == null))
			{
				if (!string.IsNullOrEmpty(character.moveSpeedParameter))
				{
					character.GetAnimator().SetFloat(character.moveSpeedParameter, character.GetMoveSpeed());
				}
				if (!string.IsNullOrEmpty(character.turnParameter))
				{
					character.GetAnimator().SetFloat(character.turnParameter, 0f);
				}
				AnimTalk(character.GetAnimator());
				SetDirection(character.GetAnimator());
			}
		}

		public override void PlayTalk()
		{
			PlayIdle();
		}

		public override void PlayTurnLeft()
		{
			if (!(character.GetAnimator() == null))
			{
				if (!string.IsNullOrEmpty(character.turnParameter))
				{
					character.GetAnimator().SetFloat(character.turnParameter, -1f);
				}
				AnimTalk(character.GetAnimator());
				if (!string.IsNullOrEmpty(character.moveSpeedParameter))
				{
					character.GetAnimator().SetFloat(character.moveSpeedParameter, 0f);
				}
				SetDirection(character.GetAnimator());
			}
		}

		public override void PlayTurnRight()
		{
			if (!(character.GetAnimator() == null))
			{
				if (!string.IsNullOrEmpty(character.turnParameter))
				{
					character.GetAnimator().SetFloat(character.turnParameter, 1f);
				}
				AnimTalk(character.GetAnimator());
				if (!string.IsNullOrEmpty(character.moveSpeedParameter))
				{
					character.GetAnimator().SetFloat(character.moveSpeedParameter, 0f);
				}
				SetDirection(character.GetAnimator());
			}
		}

		public override void PlayVertical()
		{
			if (!(character.GetAnimator() == null) && !string.IsNullOrEmpty(character.verticalMovementParameter))
			{
				character.GetAnimator().SetFloat(character.verticalMovementParameter, character.GetHeightChange());
			}
		}

		public override void TurnHead(Vector2 angles)
		{
			if (!string.IsNullOrEmpty(character.headYawParameter))
			{
				float num = angles.x * 57.29578f;
				float num2 = character.GetSpriteAngle() + num;
				if (num2 > 360f)
				{
					num2 -= 360f;
				}
				if (num2 < 0f)
				{
					num2 += 360f;
				}
				if (character.angleSnapping != AngleSnapping.None)
				{
					num2 = character.FlattenSpriteAngle(num2, character.angleSnapping);
				}
				character.GetAnimator().SetFloat(character.headYawParameter, num2);
			}
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

		protected void SetDirection(Animator animator)
		{
			if (!string.IsNullOrEmpty(character.directionParameter))
			{
				animator.SetInteger(character.directionParameter, character.GetSpriteDirectionInt());
			}
			if (!string.IsNullOrEmpty(character.angleParameter))
			{
				animator.SetFloat(character.angleParameter, character.GetSpriteAngle());
			}
		}
	}
}
