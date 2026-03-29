using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	public class AnimEngine_Legacy : AnimEngine
	{
		public static string lastClipName = string.Empty;

		public static Dictionary<int, string> animationStateDict = new Dictionary<int, string>();

		public override void CharSettingsGUI()
		{
		}

		public override void CharExpressionsGUI()
		{
		}

		public override PlayerData SavePlayerData(PlayerData playerData, Player player)
		{
			playerData.playerIdleAnim = AssetLoader.GetAssetInstanceID(player.idleAnim);
			playerData.playerWalkAnim = AssetLoader.GetAssetInstanceID(player.walkAnim);
			playerData.playerRunAnim = AssetLoader.GetAssetInstanceID(player.runAnim);
			playerData.playerTalkAnim = AssetLoader.GetAssetInstanceID(player.talkAnim);
			return playerData;
		}

		public override void LoadPlayerData(PlayerData playerData, Player player)
		{
			player.idleAnim = AssetLoader.RetrieveAsset(player.idleAnim, playerData.playerIdleAnim);
			player.walkAnim = AssetLoader.RetrieveAsset(player.walkAnim, playerData.playerWalkAnim);
			player.talkAnim = AssetLoader.RetrieveAsset(player.talkAnim, playerData.playerTalkAnim);
			player.runAnim = AssetLoader.RetrieveAsset(player.runAnim, playerData.playerRunAnim);
		}

		public override NPCData SaveNPCData(NPCData npcData, NPC npc)
		{
			npcData.idleAnim = AssetLoader.GetAssetInstanceID(npc.idleAnim);
			npcData.walkAnim = AssetLoader.GetAssetInstanceID(npc.walkAnim);
			npcData.runAnim = AssetLoader.GetAssetInstanceID(npc.runAnim);
			npcData.talkAnim = AssetLoader.GetAssetInstanceID(npc.talkAnim);
			return npcData;
		}

		public override void LoadNPCData(NPCData npcData, NPC npc)
		{
			npc.idleAnim = AssetLoader.RetrieveAsset(npc.idleAnim, npcData.idleAnim);
			npc.walkAnim = AssetLoader.RetrieveAsset(npc.walkAnim, npcData.walkAnim);
			npc.runAnim = AssetLoader.RetrieveAsset(npc.runAnim, npcData.talkAnim);
			npc.talkAnim = AssetLoader.RetrieveAsset(npc.talkAnim, npcData.runAnim);
		}

		public override void ActionCharAnimGUI(ActionCharAnim action, List<ActionParameter> parameters = null)
		{
		}

		public override float ActionCharAnimRun(ActionCharAnim action)
		{
			if (character == null)
			{
				return 0f;
			}
			Animation animation = character.GetAnimation();
			if (!action.isRunning)
			{
				action.isRunning = true;
				if (action.method == ActionCharAnim.AnimMethodChar.PlayCustom && (bool)action.clip)
				{
					AdvGame.CleanUnusedClips(animation);
					WrapMode wrapMode = WrapMode.Once;
					Transform mixingBone = null;
					if (action.layer == AnimLayer.Base)
					{
						character.charState = CharState.Custom;
						action.blendMode = AnimationBlendMode.Blend;
						action.playMode = (AnimPlayMode)action.playModeBase;
					}
					else if (action.layer == AnimLayer.UpperBody)
					{
						mixingBone = character.upperBodyBone;
					}
					else if (action.layer == AnimLayer.LeftArm)
					{
						mixingBone = character.leftArmBone;
					}
					else if (action.layer == AnimLayer.RightArm)
					{
						mixingBone = character.rightArmBone;
					}
					else if (action.layer == AnimLayer.Neck || action.layer == AnimLayer.Head || action.layer == AnimLayer.Face || action.layer == AnimLayer.Mouth)
					{
						mixingBone = character.neckBone;
					}
					if (action.playMode == AnimPlayMode.PlayOnceAndClamp)
					{
						wrapMode = WrapMode.ClampForever;
					}
					else if (action.playMode == AnimPlayMode.Loop)
					{
						wrapMode = WrapMode.Loop;
					}
					AdvGame.PlayAnimClip(animation, AdvGame.GetAnimLayerInt(action.layer), action.clip, action.blendMode, wrapMode, action.fadeTime, mixingBone);
				}
				else if (action.method == ActionCharAnim.AnimMethodChar.StopCustom && (bool)action.clip)
				{
					if (action.clip != character.idleAnim && action.clip != character.walkAnim)
					{
						animation.Blend(action.clip.name, 0f, action.fadeTime);
					}
				}
				else if (action.method == ActionCharAnim.AnimMethodChar.ResetToIdle)
				{
					character.ResetBaseClips();
					character.charState = CharState.Idle;
					AdvGame.CleanUnusedClips(animation);
				}
				else if (action.method == ActionCharAnim.AnimMethodChar.SetStandard)
				{
					if (action.clip != null)
					{
						if (action.standard == AnimStandard.Idle)
						{
							character.idleAnim = action.clip;
						}
						else if (action.standard == AnimStandard.Walk)
						{
							character.walkAnim = action.clip;
						}
						else if (action.standard == AnimStandard.Run)
						{
							character.runAnim = action.clip;
						}
						else if (action.standard == AnimStandard.Talk)
						{
							character.talkAnim = action.clip;
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
				if (action.willWait && (bool)action.clip)
				{
					if (action.method == ActionCharAnim.AnimMethodChar.PlayCustom)
					{
						return action.defaultPauseTime;
					}
					if (action.method == ActionCharAnim.AnimMethodChar.StopCustom)
					{
						return action.fadeTime;
					}
				}
				return 0f;
			}
			if ((bool)character.GetAnimation()[action.clip.name] && character.GetAnimation()[action.clip.name].normalizedTime < 1f && character.GetAnimation().IsPlaying(action.clip.name))
			{
				return action.defaultPauseTime;
			}
			action.isRunning = false;
			if (action.playMode == AnimPlayMode.PlayOnce)
			{
				character.GetAnimation().Blend(action.clip.name, 0f, action.fadeTime);
				if (action.layer == AnimLayer.Base && action.method == ActionCharAnim.AnimMethodChar.PlayCustom)
				{
					character.charState = CharState.Idle;
					character.ResetBaseClips();
				}
			}
			AdvGame.CleanUnusedClips(animation);
			return 0f;
		}

		public override void ActionCharAnimSkip(ActionCharAnim action)
		{
			if (character == null)
			{
				return;
			}
			Animation animation = character.GetAnimation();
			if (action.method == ActionCharAnim.AnimMethodChar.PlayCustom && (bool)action.clip)
			{
				if (action.layer == AnimLayer.Base)
				{
					character.charState = CharState.Custom;
					action.blendMode = AnimationBlendMode.Blend;
					action.playMode = (AnimPlayMode)action.playModeBase;
				}
				if (action.playMode == AnimPlayMode.PlayOnce)
				{
					if (action.layer == AnimLayer.Base && action.method == ActionCharAnim.AnimMethodChar.PlayCustom)
					{
						character.charState = CharState.Idle;
						character.ResetBaseClips();
					}
				}
				else
				{
					AdvGame.CleanUnusedClips(animation);
					WrapMode wrapMode = WrapMode.Once;
					Transform mixingBone = null;
					if (action.layer == AnimLayer.UpperBody)
					{
						mixingBone = character.upperBodyBone;
					}
					else if (action.layer == AnimLayer.LeftArm)
					{
						mixingBone = character.leftArmBone;
					}
					else if (action.layer == AnimLayer.RightArm)
					{
						mixingBone = character.rightArmBone;
					}
					else if (action.layer == AnimLayer.Neck || action.layer == AnimLayer.Head || action.layer == AnimLayer.Face || action.layer == AnimLayer.Mouth)
					{
						mixingBone = character.neckBone;
					}
					if (action.playMode == AnimPlayMode.PlayOnceAndClamp)
					{
						wrapMode = WrapMode.ClampForever;
					}
					else if (action.playMode == AnimPlayMode.Loop)
					{
						wrapMode = WrapMode.Loop;
					}
					AdvGame.PlayAnimClipFrame(animation, AdvGame.GetAnimLayerInt(action.layer), action.clip, action.blendMode, wrapMode, action.fadeTime, mixingBone, 1f);
				}
				AdvGame.CleanUnusedClips(animation);
			}
			else if (action.method == ActionCharAnim.AnimMethodChar.StopCustom && (bool)action.clip)
			{
				if (action.clip != character.idleAnim && action.clip != character.walkAnim)
				{
					animation.Blend(action.clip.name, 0f, 0f);
				}
			}
			else if (action.method == ActionCharAnim.AnimMethodChar.ResetToIdle)
			{
				character.ResetBaseClips();
				character.charState = CharState.Idle;
				AdvGame.CleanUnusedClips(animation);
			}
			else
			{
				if (action.method != ActionCharAnim.AnimMethodChar.SetStandard)
				{
					return;
				}
				if (action.clip != null)
				{
					if (action.standard == AnimStandard.Idle)
					{
						character.idleAnim = action.clip;
					}
					else if (action.standard == AnimStandard.Walk)
					{
						character.walkAnim = action.clip;
					}
					else if (action.standard == AnimStandard.Run)
					{
						character.runAnim = action.clip;
					}
					else if (action.standard == AnimStandard.Talk)
					{
						character.talkAnim = action.clip;
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

		public override bool ActionCharHoldPossible()
		{
			return true;
		}

		public override void ActionSpeechGUI(ActionSpeech action, Char speaker)
		{
		}

		public override void ActionSpeechRun(ActionSpeech action)
		{
			if (action.Speaker != null && action.Speaker.talkingAnimation == TalkingAnimation.CustomFace && ((bool)action.headClip || (bool)action.mouthClip))
			{
				AdvGame.CleanUnusedClips(action.Speaker.GetAnimation());
				if ((bool)action.headClip)
				{
					AdvGame.PlayAnimClip(action.Speaker.GetAnimation(), AdvGame.GetAnimLayerInt(AnimLayer.Head), action.headClip, AnimationBlendMode.Additive, WrapMode.Once, 0f, action.Speaker.neckBone);
				}
				if ((bool)action.mouthClip)
				{
					AdvGame.PlayAnimClip(action.Speaker.GetAnimation(), AdvGame.GetAnimLayerInt(AnimLayer.Mouth), action.mouthClip, AnimationBlendMode.Additive, WrapMode.Once, 0f, action.Speaker.neckBone);
				}
			}
		}

		public override void ActionSpeechSkip(ActionSpeech action)
		{
			if ((bool)action.Speaker && action.Speaker.talkingAnimation == TalkingAnimation.CustomFace && ((bool)action.headClip || (bool)action.mouthClip))
			{
				AdvGame.CleanUnusedClips(action.Speaker.GetAnimation());
				if ((bool)action.headClip)
				{
					AdvGame.PlayAnimClipFrame(action.Speaker.GetAnimation(), AdvGame.GetAnimLayerInt(AnimLayer.Head), action.headClip, AnimationBlendMode.Additive, WrapMode.Once, 0f, action.Speaker.neckBone, 1f);
				}
				if ((bool)action.mouthClip)
				{
					AdvGame.PlayAnimClipFrame(action.Speaker.GetAnimation(), AdvGame.GetAnimLayerInt(AnimLayer.Mouth), action.mouthClip, AnimationBlendMode.Additive, WrapMode.Once, 0f, action.Speaker.neckBone, 1f);
				}
			}
		}

		public override void ActionAnimGUI(ActionAnim action, List<ActionParameter> parameters)
		{
		}

		public override string ActionAnimLabel(ActionAnim action)
		{
			string text = string.Empty;
			if ((bool)action._anim)
			{
				text = action._anim.name;
				if (action.method == AnimMethod.PlayCustom && (bool)action.clip)
				{
					text = text + " - Play " + action.clip.name;
				}
				else if (action.method == AnimMethod.StopCustom && (bool)action.clip)
				{
					text = text + " - Stop " + action.clip.name;
				}
				else if (action.method == AnimMethod.BlendShape)
				{
					text += " - Shapekey";
				}
			}
			return text;
		}

		public override void ActionAnimAssignValues(ActionAnim action, List<ActionParameter> parameters)
		{
			action.runtimeAnim = action.AssignFile(parameters, action.parameterID, action.constantID, action._anim);
			action.runtimeShapeObject = action.AssignFile(parameters, action.parameterID, action.constantID, action.shapeObject);
		}

		public override float ActionAnimRun(ActionAnim action)
		{
			if (!action.isRunning)
			{
				action.isRunning = true;
				if (action.method == AnimMethod.PlayCustom && action.runtimeAnim != null && action.clip != null)
				{
					AdvGame.CleanUnusedClips(action.runtimeAnim);
					WrapMode wrapMode = WrapMode.Once;
					if (action.playMode == AnimPlayMode.PlayOnceAndClamp)
					{
						wrapMode = WrapMode.ClampForever;
					}
					else if (action.playMode == AnimPlayMode.Loop)
					{
						wrapMode = WrapMode.Loop;
					}
					AdvGame.PlayAnimClip(action.runtimeAnim, 0, action.clip, action.blendMode, wrapMode, action.fadeTime);
				}
				else if (action.method == AnimMethod.StopCustom && (bool)action.runtimeAnim && (bool)action.clip)
				{
					AdvGame.CleanUnusedClips(action.runtimeAnim);
					action.runtimeAnim.Blend(action.clip.name, 0f, action.fadeTime);
				}
				else if (action.method == AnimMethod.BlendShape && action.shapeKey > -1 && action.runtimeShapeObject != null)
				{
					action.runtimeShapeObject.Change(action.shapeKey, action.shapeValue, action.fadeTime);
					if (action.willWait)
					{
						return action.fadeTime;
					}
				}
				if (action.willWait)
				{
					return action.defaultPauseTime;
				}
			}
			else
			{
				if (action.method == AnimMethod.PlayCustom && (bool)action.runtimeAnim && (bool)action.clip)
				{
					if (!action.runtimeAnim.IsPlaying(action.clip.name))
					{
						action.isRunning = false;
						return 0f;
					}
					return action.defaultPauseTime;
				}
				if (action.method == AnimMethod.BlendShape && action.runtimeShapeObject != null)
				{
					action.isRunning = false;
					return 0f;
				}
			}
			return 0f;
		}

		public override void ActionAnimSkip(ActionAnim action)
		{
			if (action.method == AnimMethod.PlayCustom && (bool)action.runtimeAnim && (bool)action.clip)
			{
				AdvGame.CleanUnusedClips(action.runtimeAnim);
				WrapMode wrapMode = WrapMode.Once;
				if (action.playMode == AnimPlayMode.PlayOnceAndClamp)
				{
					wrapMode = WrapMode.ClampForever;
				}
				else if (action.playMode == AnimPlayMode.Loop)
				{
					wrapMode = WrapMode.Loop;
				}
				AdvGame.PlayAnimClipFrame(action.runtimeAnim, 0, action.clip, action.blendMode, wrapMode, 0f, null, 1f);
			}
			else if (action.method == AnimMethod.StopCustom && (bool)action.runtimeAnim && (bool)action.clip)
			{
				AdvGame.CleanUnusedClips(action.runtimeAnim);
				action.runtimeAnim.Blend(action.clip.name, 0f, 0f);
			}
			else if (action.method == AnimMethod.BlendShape && action.shapeKey > -1 && action.runtimeShapeObject != null)
			{
				action.runtimeShapeObject.Change(action.shapeKey, action.shapeValue, 0f);
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
			PlayStandardAnim(character.idleAnim, true, false);
		}

		public override void PlayWalk()
		{
			PlayStandardAnim(character.walkAnim, true, character.IsReversing());
		}

		public override void PlayRun()
		{
			PlayStandardAnim(character.runAnim, true, character.IsReversing());
		}

		public override void PlayTalk()
		{
			PlayStandardAnim(character.talkAnim, true, false);
		}

		public override void PlayJump()
		{
			if (character.IsPlayer)
			{
				Player player = (Player)character;
				if ((bool)player.jumpAnim)
				{
					PlayStandardAnim(player.jumpAnim, false, false);
				}
				else
				{
					PlayIdle();
				}
			}
			else
			{
				PlayIdle();
			}
		}

		public override void PlayTurnLeft()
		{
			if ((bool)character.turnLeftAnim)
			{
				PlayStandardAnim(character.turnLeftAnim, false, false);
			}
			else
			{
				PlayIdle();
			}
		}

		public override void PlayTurnRight()
		{
			if ((bool)character.turnRightAnim)
			{
				PlayStandardAnim(character.turnRightAnim, false, false);
			}
			else
			{
				PlayIdle();
			}
		}

		public override void TurnHead(Vector2 angles)
		{
			if (character == null)
			{
				return;
			}
			Animation animation = character.GetAnimation();
			if (animation == null)
			{
				return;
			}
			if ((bool)character.headLookLeftAnim && (bool)character.headLookRightAnim)
			{
				if (angles.x < 0f)
				{
					animation.Stop(character.headLookRightAnim.name);
					AdvGame.PlayAnimClipFrame(animation, AdvGame.GetAnimLayerInt(AnimLayer.Neck), character.headLookLeftAnim, AnimationBlendMode.Additive, WrapMode.ClampForever, 0f, character.neckBone, 1f);
					animation[character.headLookLeftAnim.name].weight = 0f - angles.x;
					animation[character.headLookLeftAnim.name].speed = 0f;
				}
				else if (angles.x > 0f)
				{
					animation.Stop(character.headLookLeftAnim.name);
					AdvGame.PlayAnimClipFrame(animation, AdvGame.GetAnimLayerInt(AnimLayer.Neck), character.headLookRightAnim, AnimationBlendMode.Additive, WrapMode.ClampForever, 0f, character.neckBone, 1f);
					animation[character.headLookRightAnim.name].weight = angles.x;
					animation[character.headLookRightAnim.name].speed = 0f;
				}
				else
				{
					animation.Stop(character.headLookLeftAnim.name);
					animation.Stop(character.headLookRightAnim.name);
				}
			}
			if ((bool)character.headLookUpAnim && (bool)character.headLookDownAnim)
			{
				if (angles.y < 0f)
				{
					animation.Stop(character.headLookUpAnim.name);
					AdvGame.PlayAnimClipFrame(animation, AdvGame.GetAnimLayerInt(AnimLayer.Neck) + 1, character.headLookDownAnim, AnimationBlendMode.Additive, WrapMode.ClampForever, 0f, character.neckBone, 1f);
					animation[character.headLookDownAnim.name].weight = 0f - angles.y;
					animation[character.headLookDownAnim.name].speed = 0f;
				}
				else if (angles.y > 0f)
				{
					animation.Stop(character.headLookDownAnim.name);
					AdvGame.PlayAnimClipFrame(animation, AdvGame.GetAnimLayerInt(AnimLayer.Neck) + 1, character.headLookUpAnim, AnimationBlendMode.Additive, WrapMode.ClampForever, 0f, character.neckBone, 1f);
					animation[character.headLookUpAnim.name].weight = angles.y;
					animation[character.headLookUpAnim.name].speed = 0f;
				}
				else
				{
					animation.Stop(character.headLookDownAnim.name);
					animation.Stop(character.headLookUpAnim.name);
				}
			}
		}

		protected void PlayStandardAnim(AnimationClip clip, bool doLoop, bool reverse)
		{
			if (character == null)
			{
				return;
			}
			Animation animation = character.GetAnimation();
			if (!(animation != null))
			{
				return;
			}
			AnimationState animationState = animation[NonAllocAnimationClipName(clip)];
			if (clip != null && animationState != null)
			{
				if (!animationState.enabled)
				{
					if (doLoop)
					{
						AdvGame.PlayAnimClip(animation, AdvGame.GetAnimLayerInt(AnimLayer.Base), clip, AnimationBlendMode.Blend, WrapMode.Loop, character.animCrossfadeSpeed, null, reverse);
					}
					else
					{
						AdvGame.PlayAnimClip(animation, AdvGame.GetAnimLayerInt(AnimLayer.Base), clip, AnimationBlendMode.Blend, WrapMode.Once, character.animCrossfadeSpeed, null, reverse);
					}
				}
			}
			else if (doLoop)
			{
				AdvGame.PlayAnimClip(animation, AdvGame.GetAnimLayerInt(AnimLayer.Base), clip, AnimationBlendMode.Blend, WrapMode.Loop, character.animCrossfadeSpeed, null, reverse);
			}
			else
			{
				AdvGame.PlayAnimClip(animation, AdvGame.GetAnimLayerInt(AnimLayer.Base), clip, AnimationBlendMode.Blend, WrapMode.Once, character.animCrossfadeSpeed, null, reverse);
			}
		}

		public static string NonAllocAnimationClipName(AnimationClip clip)
		{
			if (clip == null)
			{
				return string.Empty;
			}
			int instanceID = clip.GetInstanceID();
			if (instanceID >= 0)
			{
				if (animationStateDict.TryGetValue(instanceID, out lastClipName))
				{
					if (!string.IsNullOrEmpty(lastClipName))
					{
						return lastClipName;
					}
					lastClipName = clip.name;
					animationStateDict[instanceID] = lastClipName;
					return lastClipName;
				}
				lastClipName = clip.name;
				animationStateDict.Add(instanceID, lastClipName);
				return lastClipName;
			}
			return string.Empty;
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
