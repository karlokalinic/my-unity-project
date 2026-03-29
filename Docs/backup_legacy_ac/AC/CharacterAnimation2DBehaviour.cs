using System;
using UnityEngine;
using UnityEngine.Playables;

namespace AC
{
	[Serializable]
	public class CharacterAnimation2DBehaviour : PlayableBehaviour
	{
		protected CharDirection charDirection;

		protected bool turnInstantly;

		protected CharacterAnimation2DShot activeShot;

		protected bool forceDirection;

		protected PathSpeed moveSpeed;

		protected Vector3 lastFramePosition;

		protected Char character;

		public override void OnBehaviourPause(Playable playable, FrameData info)
		{
			if (Application.isPlaying)
			{
				SetOverrideState(false);
				lastFramePosition = Vector3.zero;
			}
		}

		public void Init(PathSpeed _moveSpeed, bool _turnInstantly, bool _forceDirection, CharDirection _charDirection, CharacterAnimation2DShot _activeShot)
		{
			moveSpeed = _moveSpeed;
			forceDirection = _forceDirection;
			charDirection = _charDirection;
			turnInstantly = _turnInstantly;
			activeShot = _activeShot;
		}

		public override void ProcessFrame(Playable playable, FrameData info, object playerData)
		{
			if (!Application.isPlaying)
			{
				return;
			}
			character = playerData as Char;
			if (character == null)
			{
				GameObject gameObject = playerData as GameObject;
				if (gameObject != null)
				{
					character = gameObject.GetComponent<Char>();
				}
			}
			if (!(character != null))
			{
				return;
			}
			if (character.GetAnimEngine() == null)
			{
				ACDebug.LogWarning(string.Concat("The 2D character animation track requires that ", character, "'s has an animation engine."));
				return;
			}
			if (!character.GetAnimEngine().isSpriteBased)
			{
				ACDebug.LogWarning(string.Concat("The 2D character animation track requires that ", character, "'s animation is sprite-based."));
				return;
			}
			if (character.turn2DCharactersIn3DSpace)
			{
				ACDebug.LogWarning(string.Concat("For the 2D character animation track to work, ", character, "'s 'Turn root object in 3D?' must be unchecked."));
				return;
			}
			if (forceDirection)
			{
				Vector3 charLookVector = AdvGame.GetCharLookVector(charDirection);
				character.SetLookDirection(charLookVector, turnInstantly);
			}
			if (lastFramePosition != Vector3.zero)
			{
				Vector3 vector = character.transform.position - lastFramePosition;
				if (Mathf.Approximately(vector.sqrMagnitude, 0f))
				{
					character.GetAnimEngine().PlayIdle();
					SetOverrideState(false);
				}
				else
				{
					SetOverrideState(true);
					switch (moveSpeed)
					{
					case PathSpeed.Walk:
						character.GetAnimEngine().PlayWalk();
						break;
					case PathSpeed.Run:
						character.GetAnimEngine().PlayRun();
						break;
					}
					if (!forceDirection)
					{
						Vector3 direction = new Vector3(vector.x, 0f, vector.y);
						character.SetLookDirection(direction, turnInstantly);
					}
				}
			}
			lastFramePosition = character.transform.position;
		}

		private void SetOverrideState(bool enable)
		{
			if (character != null)
			{
				if (enable)
				{
					character.ActiveCharacterAnimation2DShot = activeShot;
				}
				else if (character.ActiveCharacterAnimation2DShot == activeShot)
				{
					character.ActiveCharacterAnimation2DShot = null;
				}
			}
		}
	}
}
