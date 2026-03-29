using UnityEngine;
using UnityEngine.Playables;

namespace AC
{
	public class CharacterAnimation2DShot : PlayableAsset
	{
		[SerializeField]
		protected bool forceDirection;

		[SerializeField]
		protected CharDirection charDirection = CharDirection.Down;

		[SerializeField]
		protected bool turnInstantly;

		[SerializeField]
		protected PathSpeed moveSpeed;

		public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
		{
			ScriptPlayable<CharacterAnimation2DBehaviour> scriptPlayable = ScriptPlayable<CharacterAnimation2DBehaviour>.Create(graph);
			CharacterAnimation2DBehaviour behaviour = scriptPlayable.GetBehaviour();
			behaviour.Init(moveSpeed, turnInstantly, forceDirection, charDirection, this);
			return scriptPlayable;
		}
	}
}
