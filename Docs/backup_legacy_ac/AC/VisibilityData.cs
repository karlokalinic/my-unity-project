using System;

namespace AC
{
	[Serializable]
	public class VisibilityData : RememberData
	{
		public bool isOn;

		public bool isFading;

		public bool isFadingIn;

		public float fadeTime;

		public float fadeStartTime;

		public float fadeAlpha;

		public bool useDefaultTintMap;

		public int tintMapID;

		public float tintIntensity;

		public float colourR;

		public float colourG;

		public float colourB;

		public float colourA;
	}
}
