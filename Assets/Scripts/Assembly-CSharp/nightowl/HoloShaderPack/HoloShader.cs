using UnityEngine;

namespace nightowl.HoloShaderPack
{
	public class HoloShader
	{
		public enum Rim
		{
			Off = 0,
			Rim = 1,
			Inverted = 2
		}

		public enum Scanline
		{
			Off = 0,
			World = 1,
			Local = 2,
			Face = 3
		}

		public enum Noise
		{
			Off = 0,
			On = 1,
			World = 2,
			Local = 3,
			Face = 4
		}

		public enum Distortion
		{
			Off = 0,
			Distirtion = 1,
			Dissolve = 2
		}

		private static string[] RIM = new string[3] { "RIM_OFF", "RIM_ON", "RIM_INVERTED" };

		private static string[] SCANLINE = new string[4] { "SCANLINES_OFF", "SCANLINES_ON_WORLD", "SCANLINES_ON_LOCAL", "SCANLINES_ON_FACE" };

		private static string[] NOISE = new string[5] { "NOISE_OFF", "NOISE_ON", "NOISE_ON_WORLD", "NOISE_ON_LOCAL", "NOISE_ON_FACE" };

		private static string[] DISTORTION = new string[3] { "DISTORTION_OFF", "DISTORTION_ON", "DISTORTION_ON_DISSOLVE" };

		public static void EnableRim(Material material, Rim type)
		{
			for (int i = 0; i < RIM.Length; i++)
			{
				material.DisableKeyword(RIM[i]);
			}
			material.EnableKeyword(RIM[(int)type]);
		}

		public static void EnableScanline(Material material, Scanline type)
		{
			for (int i = 0; i < SCANLINE.Length; i++)
			{
				material.DisableKeyword(SCANLINE[i]);
			}
			material.EnableKeyword(SCANLINE[(int)type]);
		}

		public static void EnableNoise(Material material, Noise type)
		{
			for (int i = 0; i < NOISE.Length; i++)
			{
				material.DisableKeyword(NOISE[i]);
			}
			material.EnableKeyword(NOISE[(int)type]);
		}

		public static void EnableDistortion(Material material, Distortion type)
		{
			for (int i = 0; i < DISTORTION.Length; i++)
			{
				material.DisableKeyword(DISTORTION[i]);
			}
			material.EnableKeyword(DISTORTION[(int)type]);
		}
	}
}
