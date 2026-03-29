using System.Collections.Generic;
using UnityEngine;

namespace DFTGames.Tools
{
	[RequireComponent(typeof(Camera))]
	public abstract class FadeObstructorsBaseClass : MonoBehaviour
	{
		public Material transparentMaterial;

		public bool replicateTexture;

		public float fadingTime = 0.3f;

		public float transparenceValue = 0.3f;

		public bool ignoreTriggers = true;

		public Color fadingColorFullWhite = new Color(1f, 1f, 1f, 1f);

		public Color fadingColorToUse = new Color(1f, 1f, 1f, 0.3f);

		public LayerMask layersToFade = -1;

		public Transform playerTransform;

		public float offset = -0.5f;

		public string playerTag = "Player";

		protected Transform myTransform;

		protected Dictionary<int, ShaderData> modifiedShaders = new Dictionary<int, ShaderData>();

		public virtual void Start()
		{
			myTransform = base.transform;
			if (playerTransform == null)
			{
				playerTransform = GameObject.FindGameObjectWithTag(playerTag).GetComponent<Transform>();
			}
			if (playerTransform == null)
			{
				Debug.LogError("Player's transform not set and can't find any object in the scene with tag " + playerTag);
			}
		}
	}
}
