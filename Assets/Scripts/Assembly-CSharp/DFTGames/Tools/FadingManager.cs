using UnityEngine;

namespace DFTGames.Tools
{
	public class FadingManager : MonoBehaviour
	{
		public float fadingTime;

		public float fadingAmount;

		public bool fadeOut = true;

		public Material oldMat;

		public int matIdx;

		public Color oldColor;

		private Renderer myRenderer;

		private Color tmpColor;

		private float delta;

		private bool done;

		private Material[] mats;

		private void Start()
		{
			myRenderer = GetComponent<Renderer>();
			if (myRenderer == null)
			{
				Debug.LogError("Can't get my renderer! Object: " + base.gameObject.name);
				Object.Destroy(this);
			}
			mats = myRenderer.materials;
			for (int i = 0; i < mats.Length; i++)
			{
				tmpColor = mats[i].color;
				tmpColor.a = ((!fadeOut) ? fadingAmount : 1f);
				mats[i].color = tmpColor;
			}
			myRenderer.materials = mats;
			delta = (1f - fadingAmount) / fadingTime;
		}

		public void GoAway()
		{
			for (int i = 0; i < mats.Length; i++)
			{
				tmpColor = mats[i].color;
				if (fadeOut)
				{
					tmpColor.a = fadingAmount;
					mats[i].color = tmpColor;
				}
				else if (i == matIdx)
				{
					mats[i] = oldMat;
					mats[i].color = oldColor;
				}
			}
			myRenderer.materials = mats;
			done = true;
			Resources.UnloadUnusedAssets();
			Object.Destroy(this);
		}

		private void Update()
		{
			if (done)
			{
				Resources.UnloadUnusedAssets();
				Object.Destroy(this);
			}
			mats = myRenderer.materials;
			for (int i = 0; i < mats.Length; i++)
			{
				tmpColor = mats[i].color;
				if (fadeOut)
				{
					FadeOut(i);
				}
				else
				{
					FadeIn(i);
				}
			}
			myRenderer.materials = mats;
		}

		private void FadeOut(int i)
		{
			if (tmpColor.a < fadingAmount)
			{
				tmpColor.a = fadingAmount;
				mats[i].color = tmpColor;
				done = true;
			}
			else
			{
				tmpColor.a -= delta * Time.deltaTime;
				mats[i].color = tmpColor;
			}
		}

		private void FadeIn(int i)
		{
			if (i == matIdx)
			{
				if (tmpColor.a >= 1f)
				{
					mats[i] = oldMat;
					mats[i].color = oldColor;
					done = true;
				}
				else
				{
					tmpColor.a += delta * Time.deltaTime;
					mats[i].color = tmpColor;
				}
			}
		}
	}
}
