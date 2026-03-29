using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[AddComponentMenu("Adventure Creator/Characters/Lipsync texture")]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_lip_sync_texture.html")]
	public class LipSyncTexture : MonoBehaviour
	{
		public SkinnedMeshRenderer skinnedMeshRenderer;

		public int materialIndex;

		public string propertyName = "_MainTex";

		public List<Texture2D> textures = new List<Texture2D>();

		public bool affectInLateUpdate;

		protected int thisFrameIndex = -1;

		protected void Awake()
		{
			LimitTextureArray();
		}

		protected void LateUpdate()
		{
			if (affectInLateUpdate)
			{
				SetFrame(thisFrameIndex, true);
			}
		}

		public void LimitTextureArray()
		{
			if (AdvGame.GetReferences() == null || AdvGame.GetReferences().speechManager == null)
			{
				return;
			}
			int count = AdvGame.GetReferences().speechManager.phonemes.Count;
			if (textures.Count == count)
			{
				return;
			}
			int count2 = textures.Count;
			if (count < count2)
			{
				textures.RemoveRange(count, count2 - count);
			}
			else if (count > count2)
			{
				for (int i = textures.Count; i < count; i++)
				{
					textures.Add(null);
				}
			}
		}

		public void SetFrame(int textureIndex, bool fromLateUpdate = false)
		{
			thisFrameIndex = textureIndex;
			if (textureIndex >= 0 && textures != null && textureIndex < textures.Count && affectInLateUpdate == fromLateUpdate && (bool)skinnedMeshRenderer)
			{
				if (materialIndex >= 0 && skinnedMeshRenderer.materials.Length > materialIndex)
				{
					skinnedMeshRenderer.materials[materialIndex].SetTexture(propertyName, textures[textureIndex]);
					return;
				}
				ACDebug.LogWarning("Cannot find material index " + materialIndex + " on SkinnedMeshRenderer " + skinnedMeshRenderer.gameObject.name, skinnedMeshRenderer);
			}
		}
	}
}
