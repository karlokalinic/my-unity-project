using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	public class AnimEngine_Sprites2DToolkit : AnimEngine
	{
		public override void Declare(Char _character)
		{
			character = _character;
			turningStyle = TurningStyle.Linear;
			isSpriteBased = true;
		}

		public override void CharSettingsGUI()
		{
		}

		public override PlayerData SavePlayerData(PlayerData playerData, Player player)
		{
			playerData.playerIdleAnim = player.idleAnimSprite;
			playerData.playerWalkAnim = player.walkAnimSprite;
			playerData.playerRunAnim = player.runAnimSprite;
			playerData.playerTalkAnim = player.talkAnimSprite;
			return playerData;
		}

		public override void LoadPlayerData(PlayerData playerData, Player player)
		{
			player.idleAnimSprite = playerData.playerIdleAnim;
			player.walkAnimSprite = playerData.playerWalkAnim;
			player.talkAnimSprite = playerData.playerTalkAnim;
			player.runAnimSprite = playerData.playerRunAnim;
		}

		public override NPCData SaveNPCData(NPCData npcData, NPC npc)
		{
			npcData.idleAnim = npc.idleAnimSprite;
			npcData.walkAnim = npc.walkAnimSprite;
			npcData.talkAnim = npc.talkAnimSprite;
			npcData.runAnim = npc.runAnimSprite;
			return npcData;
		}

		public override void LoadNPCData(NPCData npcData, NPC npc)
		{
			npc.idleAnimSprite = npcData.idleAnim;
			npc.walkAnimSprite = npcData.walkAnim;
			npc.talkAnimSprite = npcData.talkAnim;
			npc.runAnimSprite = npcData.runAnim;
		}

		public override void ActionCharAnimGUI(ActionCharAnim action, List<ActionParameter> parameters = null)
		{
		}

		public override float ActionCharAnimRun(ActionCharAnim action)
		{
			string text = action.clip2D;
			if (action.includeDirection)
			{
				text += character.GetSpriteDirection();
			}
			if (!action.isRunning)
			{
				action.isRunning = true;
				if (action.method == ActionCharAnim.AnimMethodChar.PlayCustom && action.clip2D != string.Empty)
				{
					character.charState = CharState.Custom;
					if (action.playMode == AnimPlayMode.Loop)
					{
						tk2DIntegration.PlayAnimation(character.spriteChild, text, true, WrapMode.Loop);
						action.willWait = false;
					}
					else
					{
						tk2DIntegration.PlayAnimation(character.spriteChild, text, true, WrapMode.Once);
					}
				}
				else if (action.method == ActionCharAnim.AnimMethodChar.ResetToIdle)
				{
					character.ResetBaseClips();
				}
				else if (action.method == ActionCharAnim.AnimMethodChar.SetStandard)
				{
					if (action.clip2D != string.Empty)
					{
						if (action.standard == AnimStandard.Idle)
						{
							character.idleAnimSprite = action.clip2D;
						}
						else if (action.standard == AnimStandard.Walk)
						{
							character.walkAnimSprite = action.clip2D;
						}
						else if (action.standard == AnimStandard.Talk)
						{
							character.talkAnimSprite = action.clip2D;
						}
						else if (action.standard == AnimStandard.Run)
						{
							character.runAnimSprite = action.clip2D;
						}
					}
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
							if (action.newSound != null)
							{
								character.walkSound = action.newSound;
							}
							else
							{
								character.walkSound = null;
							}
						}
						else if (action.standard == AnimStandard.Run)
						{
							if (action.newSound != null)
							{
								character.runSound = action.newSound;
							}
							else
							{
								character.runSound = null;
							}
						}
					}
				}
				if (action.willWait && !string.IsNullOrEmpty(action.clip2D) && action.method == ActionCharAnim.AnimMethodChar.PlayCustom)
				{
					return action.defaultPauseTime;
				}
			}
			else if ((bool)character.spriteChild && action.clip2D != string.Empty)
			{
				if (!tk2DIntegration.IsAnimationPlaying(character.spriteChild, action.clip2D))
				{
					action.isRunning = false;
					return 0f;
				}
				return action.defaultPauseTime / 6f;
			}
			return 0f;
		}

		public override void ActionCharAnimSkip(ActionCharAnim action)
		{
			string text = action.clip2D;
			if (action.includeDirection)
			{
				text += character.GetSpriteDirection();
			}
			if (action.method == ActionCharAnim.AnimMethodChar.PlayCustom && action.clip2D != string.Empty)
			{
				if (!action.willWait || action.playMode == AnimPlayMode.Loop)
				{
					character.charState = CharState.Custom;
					if (action.playMode == AnimPlayMode.Loop)
					{
						tk2DIntegration.PlayAnimation(character.spriteChild, text, true, WrapMode.Loop);
						action.willWait = false;
					}
					else
					{
						tk2DIntegration.PlayAnimation(character.spriteChild, text, true, WrapMode.Once);
					}
				}
				else if (action.playMode == AnimPlayMode.PlayOnce)
				{
					character.charState = CharState.Idle;
				}
			}
			else if (action.method == ActionCharAnim.AnimMethodChar.ResetToIdle)
			{
				character.ResetBaseClips();
			}
			else
			{
				if (action.method != ActionCharAnim.AnimMethodChar.SetStandard)
				{
					return;
				}
				if (!string.IsNullOrEmpty(action.clip2D))
				{
					if (action.standard == AnimStandard.Idle)
					{
						character.idleAnimSprite = action.clip2D;
					}
					else if (action.standard == AnimStandard.Walk)
					{
						character.walkAnimSprite = action.clip2D;
					}
					else if (action.standard == AnimStandard.Talk)
					{
						character.talkAnimSprite = action.clip2D;
					}
					else if (action.standard == AnimStandard.Run)
					{
						character.runAnimSprite = action.clip2D;
					}
				}
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
				if (!action.changeSound)
				{
					return;
				}
				if (action.standard == AnimStandard.Walk)
				{
					if (action.newSound != null)
					{
						character.walkSound = action.newSound;
					}
					else
					{
						character.walkSound = null;
					}
				}
				else if (action.standard == AnimStandard.Run)
				{
					if (action.newSound != null)
					{
						character.runSound = action.newSound;
					}
					else
					{
						character.runSound = null;
					}
				}
			}
		}

		public override void ActionAnimGUI(ActionAnim action, List<ActionParameter> parameters)
		{
		}

		public override string ActionAnimLabel(ActionAnim action)
		{
			string text = string.Empty;
			if ((bool)action._anim2D)
			{
				text = action._anim2D.name;
				if (action.method == AnimMethod.PlayCustom && !string.IsNullOrEmpty(action.clip2D))
				{
					text = text + " - " + action.clip2D;
				}
			}
			return text;
		}

		public override void ActionAnimAssignValues(ActionAnim action, List<ActionParameter> parameters)
		{
			action.runtimeAnim2D = action.AssignFile(parameters, action.parameterID, action.constantID, action._anim2D);
		}

		public override float ActionAnimRun(ActionAnim action)
		{
			if (!action.isRunning)
			{
				action.isRunning = true;
				if (action.runtimeAnim2D != null && !string.IsNullOrEmpty(action.clip2D))
				{
					if (action.method == AnimMethod.PlayCustom)
					{
						if (action.wrapMode2D == ActionAnim.WrapMode2D.Loop)
						{
							tk2DIntegration.PlayAnimation(action.runtimeAnim2D, action.clip2D, true, WrapMode.Loop);
						}
						else if (action.wrapMode2D == ActionAnim.WrapMode2D.PingPong)
						{
							tk2DIntegration.PlayAnimation(action.runtimeAnim2D, action.clip2D, true, WrapMode.PingPong);
						}
						else
						{
							tk2DIntegration.PlayAnimation(action.runtimeAnim2D, action.clip2D, true, WrapMode.Once);
						}
						if (action.willWait)
						{
							return action.defaultPauseTime;
						}
					}
					else if (action.method == AnimMethod.StopCustom)
					{
						tk2DIntegration.StopAnimation(action.runtimeAnim2D);
					}
					else if (action.method == AnimMethod.BlendShape)
					{
						action.ReportWarning("BlendShapes are not available for 2D animation.");
						return 0f;
					}
				}
			}
			else if (action.runtimeAnim2D != null && !string.IsNullOrEmpty(action.clip2D))
			{
				if (tk2DIntegration.IsAnimationPlaying(action.runtimeAnim2D, action.clip2D))
				{
					return Time.deltaTime;
				}
				action.isRunning = false;
			}
			return 0f;
		}

		public override void ActionAnimSkip(ActionAnim action)
		{
			if (action.isRunning)
			{
				return;
			}
			action.isRunning = true;
			if (!(action.runtimeAnim2D != null) || string.IsNullOrEmpty(action.clip2D))
			{
				return;
			}
			if (action.method == AnimMethod.PlayCustom)
			{
				if (action.wrapMode2D == ActionAnim.WrapMode2D.Loop)
				{
					tk2DIntegration.PlayAnimation(action.runtimeAnim2D, action.clip2D, true, WrapMode.Loop);
				}
				else if (action.wrapMode2D == ActionAnim.WrapMode2D.PingPong)
				{
					tk2DIntegration.PlayAnimation(action.runtimeAnim2D, action.clip2D, true, WrapMode.PingPong);
				}
				else
				{
					tk2DIntegration.PlayAnimation(action.runtimeAnim2D, action.clip2D, true, WrapMode.Once);
				}
			}
			else if (action.method == AnimMethod.StopCustom)
			{
				tk2DIntegration.StopAnimation(action.runtimeAnim2D);
			}
		}

		public override void ActionCharRenderGUI(ActionCharRender action)
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
			return 0f;
		}

		public override void PlayIdle()
		{
			PlayStandardAnim(character.idleAnimSprite, true);
		}

		public override void PlayWalk()
		{
			PlayStandardAnim(character.walkAnimSprite, true);
		}

		public override void PlayRun()
		{
			PlayStandardAnim(character.runAnimSprite, true);
		}

		public override void PlayTalk()
		{
			if (character.LipSyncGameObject())
			{
				PlayStandardAnim(character.talkAnimSprite, true, character.GetLipSyncFrame());
			}
			else
			{
				PlayStandardAnim(character.talkAnimSprite, true);
			}
		}

		private void PlayStandardAnim(string clip, bool includeDirection)
		{
			PlayStandardAnim(clip, includeDirection, -1);
		}

		private void PlayStandardAnim(string clip, bool includeDirection, int frame)
		{
			if (clip != string.Empty && character != null)
			{
				string text = clip;
				if (includeDirection)
				{
					text += character.GetSpriteDirection();
				}
				if (!tk2DIntegration.PlayAnimation(character.spriteChild, text, frame))
				{
					tk2DIntegration.PlayAnimation(character.spriteChild, clip, frame);
				}
			}
		}
	}
}
