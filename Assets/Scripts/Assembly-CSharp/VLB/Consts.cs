using UnityEngine;

namespace VLB
{
	public static class Consts
	{
		private const string HelpUrlBase = "http://saladgamer.com/vlb-doc/";

		public const string HelpUrlBeam = "http://saladgamer.com/vlb-doc/comp-lightbeam/";

		public const string HelpUrlDustParticles = "http://saladgamer.com/vlb-doc/comp-dustparticles/";

		public const string HelpUrlDynamicOcclusion = "http://saladgamer.com/vlb-doc/comp-dynocclusion/";

		public const string HelpUrlConfig = "http://saladgamer.com/vlb-doc/config/";

		public static bool ProceduralObjectsVisibleInEditor = true;

		public static Color FlatColor = Color.white;

		public const VolumetricLightBeam.ColorMode ColorMode = VolumetricLightBeam.ColorMode.Flat;

		public const float Alpha = 1f;

		public const float SpotAngleDefault = 35f;

		public const float SpotAngleMin = 0.1f;

		public const float SpotAngleMax = 179.9f;

		public const float ConeRadiusStart = 0.1f;

		public const int GeomSidesDefault = 18;

		public const int GeomSidesMin = 3;

		public const int GeomSidesMax = 256;

		public const bool GeomCap = false;

		public const VolumetricLightBeam.AttenuationEquation AttenuationEquation = VolumetricLightBeam.AttenuationEquation.Quadratic;

		public const float AttenuationCustomBlending = 0.5f;

		public const float FadeStart = 0f;

		public const float FadeEnd = 3f;

		public const float FadeMinThreshold = 0.01f;

		public const float DepthBlendDistance = 2f;

		public const float CameraClippingDistance = 0.5f;

		public const float FresnelPowMaxValue = 10f;

		public const float FresnelPow = 8f;

		public const float GlareFrontal = 0.5f;

		public const float GlareBehind = 0.5f;

		public const float NoiseIntensityDefault = 0.5f;

		public const float NoiseScaleMin = 0.01f;

		public const float NoiseScaleMax = 2f;

		public const float NoiseScaleDefault = 0.5f;

		public static Vector3 NoiseVelocityDefault = new Vector3(0.07f, 0.18f, 0.05f);

		public static HideFlags ProceduralObjectsHideFlags
		{
			get
			{
				return (!ProceduralObjectsVisibleInEditor) ? HideFlags.HideAndDontSave : (HideFlags.DontSave | HideFlags.NotEditable);
			}
		}
	}
}
