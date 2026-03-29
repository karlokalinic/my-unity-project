using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	public class AnimEngine_SpritesUnity : AnimEngine
	{
		protected string hideHeadClip = "HideHead";

		protected string headDirection;

		public override void Declare(Char _character)
		{
			character = _character;
			turningStyle = TurningStyle.Linear;
			isSpriteBased = true;
			base.updateHeadAlways = true;
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
				if (action.method == ActionCharAnim.AnimMethodChar.PlayCustom && !string.IsNullOrEmpty(action.clip2D))
				{
					if ((bool)character.GetAnimator())
					{
						character.charState = CharState.Custom;
						character.GetAnimator().CrossFade(text, action.fadeTime, action.layerInt);
						if (action.hideHead && character.talkingAnimation == TalkingAnimation.Standard && character.separateTalkingLayer)
						{
							PlayHeadAnim(hideHeadClip, false);
						}
					}
				}
				else if (action.method == ActionCharAnim.AnimMethodChar.ResetToIdle)
				{
					if (action.idleAfterCustom)
					{
						action.layerInt = 0;
						return action.defaultPauseTime;
					}
					character.ResetBaseClips();
					character.charState = CharState.Idle;
				}
				else if (action.method == ActionCharAnim.AnimMethodChar.SetStandard)
				{
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
				return 0f;
			}
			if ((bool)character.GetAnimator())
			{
				float length = character.GetAnimator().GetCurrentAnimatorStateInfo(action.layerInt).length;
				float num = (1f - character.GetAnimator().GetCurrentAnimatorStateInfo(action.layerInt).normalizedTime) * length;
				num -= 0.1f;
				if (num > 0f)
				{
					return num;
				}
				if (action.method == ActionCharAnim.AnimMethodChar.ResetToIdle)
				{
					character.ResetBaseClips();
					character.charState = CharState.Idle;
				}
				else if (action.idleAfter)
				{
					character.charState = CharState.Idle;
				}
				action.isRunning = false;
				return 0f;
			}
			action.isRunning = false;
			character.charState = CharState.Idle;
			return 0f;
		}

		public override void ActionCharAnimSkip(ActionCharAnim action)
		{
			if (action.method == ActionCharAnim.AnimMethodChar.SetStandard)
			{
				ActionCharAnimRun(action);
				return;
			}
			if (action.method == ActionCharAnim.AnimMethodChar.ResetToIdle)
			{
				character.ResetBaseClips();
				character.charState = CharState.Idle;
				return;
			}
			string text = action.clip2D;
			if (action.includeDirection)
			{
				text += character.GetSpriteDirection();
			}
			if (action.method == ActionCharAnim.AnimMethodChar.PlayCustom)
			{
				if (action.willWait && action.idleAfter)
				{
					character.charState = CharState.Idle;
				}
				else if ((bool)character.GetAnimator())
				{
					character.charState = CharState.Custom;
					character.GetAnimator().Play(text, action.layerInt, 0.8f);
				}
			}
		}

		public override void ActionSpeechGUI(ActionSpeech action, Char speaker)
		{
		}

		public override void ActionSpeechRun(ActionSpeech action)
		{
			if (action.Speaker.talkingAnimation != TalkingAnimation.CustomFace || !action.Speaker.GetAnimator())
			{
				return;
			}
			if (action.play2DHeadAnim && !string.IsNullOrEmpty(action.headClip2D))
			{
				try
				{
					action.Speaker.GetAnimator().Play(action.headClip2D, action.headLayer);
				}
				catch
				{
				}
			}
			if (action.play2DMouthAnim && !string.IsNullOrEmpty(action.mouthClip2D))
			{
				try
				{
					action.Speaker.GetAnimator().Play(action.mouthClip2D, action.mouthLayer);
				}
				catch
				{
				}
			}
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
				if (action.method == AnimMethod.PlayCustom && action.clip2D != string.Empty)
				{
					text = text + " - " + action.clip2D;
				}
			}
			return text;
		}

		public override void ActionAnimAssignValues(ActionAnim action, List<ActionParameter> parameters)
		{
			action.runtimeAnimator = action.AssignFile(parameters, action.parameterID, action.constantID, action.animator);
		}

		public override float ActionAnimRun(ActionAnim action)
		{
			if (!action.isRunning)
			{
				action.isRunning = true;
				if ((bool)action.runtimeAnimator && !string.IsNullOrEmpty(action.clip2D))
				{
					if (action.method == AnimMethod.PlayCustom)
					{
						action.runtimeAnimator.CrossFade(action.clip2D, action.fadeTime, action.layerInt);
						if (action.willWait)
						{
							return action.defaultPauseTime;
						}
					}
					else if (action.method == AnimMethod.BlendShape)
					{
						action.ReportWarning("BlendShapes not available for 2D animation.");
						return 0f;
					}
				}
			}
			else if ((bool)action.runtimeAnimator && !string.IsNullOrEmpty(action.clip2D))
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
			if ((bool)action.runtimeAnimator && !string.IsNullOrEmpty(action.clip2D) && action.method == AnimMethod.PlayCustom)
			{
				action.runtimeAnimator.Play(action.clip2D, action.layerInt, 0.8f);
			}
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
			PlayStandardAnim(character.idleAnimSprite, character.spriteDirectionData.HasDirections());
			PlaySeparateHead();
		}

		public override void PlayWalk()
		{
			PlayStandardAnim(character.walkAnimSprite, character.spriteDirectionData.HasDirections());
			PlaySeparateHead();
		}

		public override void PlayRun()
		{
			if (!string.IsNullOrEmpty(character.runAnimSprite))
			{
				PlayStandardAnim(character.runAnimSprite, character.spriteDirectionData.HasDirections());
			}
			else
			{
				PlayWalk();
			}
			PlaySeparateHead();
		}

		public override void PlayTalk()
		{
			if (string.IsNullOrEmpty(character.talkAnimSprite))
			{
				PlayIdle();
			}
			else if (character.talkingAnimation == TalkingAnimation.Standard && character.separateTalkingLayer)
			{
				PlayIdle();
			}
			else if (character.LipSyncGameObject() && (bool)character.GetAnimator())
			{
				PlayLipSync(false);
			}
			else
			{
				PlayStandardAnim(character.talkAnimSprite, character.spriteDirectionData.HasDirections());
			}
		}

		protected void PlaySeparateHead()
		{
			if (character.talkingAnimation != TalkingAnimation.Standard || !character.separateTalkingLayer)
			{
				return;
			}
			if (character.isTalking)
			{
				if (character.LipSyncGameObject() && (bool)character.GetAnimator())
				{
					PlayLipSync(true);
				}
				else
				{
					PlayHeadAnim(character.talkAnimSprite, character.spriteDirectionData.HasDirections());
				}
			}
			else
			{
				PlayHeadAnim(character.idleAnimSprite, character.spriteDirectionData.HasDirections());
			}
		}

		protected void PlayLipSync(bool onlyHead)
		{
			string text = character.talkAnimSprite;
			int num = (onlyHead ? character.headLayer : 0);
			if (character.spriteDirectionData.HasDirections())
			{
				text = ((num <= 0) ? (text + character.GetSpriteDirection()) : (text + headDirection));
			}
			character.GetAnimator().speed = 0f;
			try
			{
				character.GetAnimator().Play(text, num, character.GetLipSyncNormalised());
			}
			catch
			{
			}
			character.GetAnimator().speed = 1f;
		}

		public override void TurnHead(Vector2 angles)
		{
			if (character.lockDirection)
			{
				headDirection = character.GetSpriteDirection();
			}
			else if (character.talkingAnimation == TalkingAnimation.Standard && character.separateTalkingLayer)
			{
				float num = angles.x * 57.29578f;
				float angle = character.GetSpriteAngle() + num;
				headDirection = "_" + character.spriteDirectionData.GetDirectionalSuffix(angle);
			}
		}

		protected void PlayStandardAnim(string clip, bool includeDirection)
		{
			if ((bool)character && (bool)character.GetAnimator() && !string.IsNullOrEmpty(clip))
			{
				if (includeDirection)
				{
					clip += character.GetSpriteDirection();
				}
				PlayCharAnim(clip, 0);
			}
		}

		protected void PlayHeadAnim(string clip, bool includeDirection)
		{
			if ((bool)character && (bool)character.GetAnimator() && !string.IsNullOrEmpty(clip))
			{
				if (includeDirection)
				{
					clip += headDirection;
				}
				PlayCharAnim(clip, character.headLayer);
			}
		}

		protected void PlayCharAnim(string clip, int layer)
		{
			if (character.crossfadeAnims)
			{
				try
				{
					if (!character.GetAnimator().GetNextAnimatorStateInfo(layer).IsName(clip) && !character.GetAnimator().GetCurrentAnimatorStateInfo(layer).IsName(clip))
					{
						character.GetAnimator().CrossFade(clip, character.animCrossfadeSpeed, layer);
					}
					return;
				}
				catch
				{
					return;
				}
			}
			try
			{
				character.GetAnimator().Play(clip, layer);
			}
			catch
			{
			}
		}
	}
}
