using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionCharFollow : Action
	{
		public enum FollowType
		{
			StartFollowing = 0,
			StopFollowing = 1
		}

		public int npcToMoveParameterID = -1;

		public int charToFollowParameterID = -1;

		public int npcToMoveID;

		public int charToFollowID;

		public NPC npcToMove;

		protected NPC runtimeNpcToMove;

		public Char charToFollow;

		protected Char runtimeCharToFollow;

		public bool followPlayer;

		public bool faceWhenIdle;

		public float updateFrequency = 2f;

		public float followDistance = 1f;

		public float followDistanceMax = 15f;

		public FollowType followType;

		public bool randomDirection;

		public ActionCharFollow()
		{
			isDisplayed = true;
			category = ActionCategory.Character;
			title = "NPC follow";
			description = "Makes an NPC follow another Character, whether it be a fellow NPC or the Player. If they exceed a maximum distance from their target, they will run towards them. Note that making an NPC move via another Action will make them stop following anyone.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			runtimeNpcToMove = AssignFile(parameters, npcToMoveParameterID, npcToMoveID, npcToMove);
			runtimeCharToFollow = AssignFile(parameters, charToFollowParameterID, charToFollowID, charToFollow);
		}

		public override float Run()
		{
			if ((bool)runtimeNpcToMove)
			{
				if (followType == FollowType.StopFollowing)
				{
					runtimeNpcToMove.StopFollowing();
					return 0f;
				}
				if (followPlayer || (runtimeCharToFollow != null && runtimeCharToFollow != runtimeNpcToMove))
				{
					runtimeNpcToMove.FollowAssign(runtimeCharToFollow, followPlayer, updateFrequency, followDistance, followDistanceMax, faceWhenIdle, randomDirection);
				}
			}
			return 0f;
		}

		public static ActionCharFollow CreateNew_Start(NPC npcToMove, Char characterToFollow, float minimumDistance, float maximumDistance, float updateFrequency = 2f, bool randomisePosition = false, bool faceCharacterWhenIdle = false)
		{
			ActionCharFollow actionCharFollow = ScriptableObject.CreateInstance<ActionCharFollow>();
			actionCharFollow.followType = FollowType.StartFollowing;
			actionCharFollow.npcToMove = npcToMove;
			actionCharFollow.charToFollow = characterToFollow;
			actionCharFollow.followDistance = minimumDistance;
			actionCharFollow.followDistanceMax = maximumDistance;
			actionCharFollow.updateFrequency = updateFrequency;
			actionCharFollow.randomDirection = randomisePosition;
			actionCharFollow.faceWhenIdle = faceCharacterWhenIdle;
			return actionCharFollow;
		}

		public static ActionCharFollow CreateNew_Stop(NPC npcToMove)
		{
			ActionCharFollow actionCharFollow = ScriptableObject.CreateInstance<ActionCharFollow>();
			actionCharFollow.followType = FollowType.StopFollowing;
			actionCharFollow.npcToMove = npcToMove;
			return actionCharFollow;
		}
	}
}
