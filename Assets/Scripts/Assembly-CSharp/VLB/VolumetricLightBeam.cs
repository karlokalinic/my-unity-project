using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace VLB
{
	[ExecuteInEditMode]
	[DisallowMultipleComponent]
	[SelectionBase]
	[HelpURL("http://saladgamer.com/vlb-doc/comp-lightbeam/")]
	public class VolumetricLightBeam : MonoBehaviour
	{
		public enum ColorMode
		{
			Flat = 0,
			Gradient = 1
		}

		public enum AttenuationEquation
		{
			Linear = 0,
			Quadratic = 1,
			Blend = 2
		}

		public bool colorFromLight = true;

		public ColorMode colorMode;

		[ColorUsage(true, true)]
		[FormerlySerializedAs("colorValue")]
		public Color color = Consts.FlatColor;

		public Gradient colorGradient;

		[FormerlySerializedAs("alphaOutside")]
		[Range(0f, 1f)]
		public float alpha = 1f;

		[FormerlySerializedAs("angleFromLight")]
		public bool spotAngleFromLight = true;

		[Range(0.1f, 179.9f)]
		public float spotAngle = 35f;

		[FormerlySerializedAs("radiusStart")]
		public float coneRadiusStart = 0.1f;

		public int geomSides = 18;

		public bool geomCap;

		public bool fadeEndFromLight = true;

		public AttenuationEquation attenuationEquation = AttenuationEquation.Quadratic;

		[Range(0f, 1f)]
		public float attenuationCustomBlending = 0.5f;

		public float fadeStart;

		public float fadeEnd = 3f;

		public float depthBlendDistance = 2f;

		public float cameraClippingDistance = 0.5f;

		[Range(0f, 1f)]
		public float glareFrontal = 0.5f;

		[Range(0f, 1f)]
		public float glareBehind = 0.5f;

		[Obsolete("Use 'glareFrontal' instead")]
		public float boostDistanceInside = 0.5f;

		[Obsolete("This property has been merged with 'fresnelPow'")]
		public float fresnelPowInside = 6f;

		[FormerlySerializedAs("fresnelPowOutside")]
		public float fresnelPow = 8f;

		public bool noiseEnabled;

		[Range(0f, 1f)]
		public float noiseIntensity = 0.5f;

		public bool noiseScaleUseGlobal = true;

		[Range(0.01f, 2f)]
		public float noiseScaleLocal = 0.5f;

		public bool noiseVelocityUseGlobal = true;

		public Vector3 noiseVelocityLocal = Consts.NoiseVelocityDefault;

		[SerializeField]
		private int pluginVersion = -1;

		[FormerlySerializedAs("trackChangesDuringPlaytime")]
		[SerializeField]
		private bool _TrackChangesDuringPlaytime;

		[SerializeField]
		private int _SortingLayerID;

		[SerializeField]
		private int _SortingOrder;

		private BeamGeometry m_BeamGeom;

		private Coroutine m_CoPlaytimeUpdate;

		private Light _CachedLight;

		public float coneAngle
		{
			get
			{
				return Mathf.Atan2(coneRadiusEnd - coneRadiusStart, fadeEnd) * 57.29578f * 2f;
			}
		}

		public float coneRadiusEnd
		{
			get
			{
				return fadeEnd * Mathf.Tan(spotAngle * ((float)Math.PI / 180f) * 0.5f);
			}
		}

		public float coneVolume
		{
			get
			{
				float num = coneRadiusStart;
				float num2 = coneRadiusEnd;
				return (float)Math.PI / 3f * (num * num + num * num2 + num2 * num2) * fadeEnd;
			}
		}

		public float coneApexOffsetZ
		{
			get
			{
				float num = coneRadiusStart / coneRadiusEnd;
				return (num != 1f) ? (fadeEnd * num / (1f - num)) : 0f;
			}
		}

		public float attenuationLerpLinearQuad
		{
			get
			{
				if (attenuationEquation == AttenuationEquation.Linear)
				{
					return 0f;
				}
				if (attenuationEquation == AttenuationEquation.Quadratic)
				{
					return 1f;
				}
				return attenuationCustomBlending;
			}
		}

		public int sortingLayerID
		{
			get
			{
				return _SortingLayerID;
			}
			set
			{
				_SortingLayerID = value;
				if ((bool)m_BeamGeom)
				{
					m_BeamGeom.sortingLayerID = value;
				}
			}
		}

		public string sortingLayerName
		{
			get
			{
				return SortingLayer.IDToName(sortingLayerID);
			}
			set
			{
				sortingLayerID = SortingLayer.NameToID(value);
			}
		}

		public int sortingOrder
		{
			get
			{
				return _SortingOrder;
			}
			set
			{
				_SortingOrder = value;
				if ((bool)m_BeamGeom)
				{
					m_BeamGeom.sortingOrder = value;
				}
			}
		}

		public bool trackChangesDuringPlaytime
		{
			get
			{
				return _TrackChangesDuringPlaytime;
			}
			set
			{
				_TrackChangesDuringPlaytime = value;
				StartPlaytimeUpdateIfNeeded();
			}
		}

		public bool isCurrentlyTrackingChanges
		{
			get
			{
				return m_CoPlaytimeUpdate != null;
			}
		}

		public bool hasGeometry
		{
			get
			{
				return m_BeamGeom != null;
			}
		}

		public Bounds bounds
		{
			get
			{
				return (!(m_BeamGeom != null)) ? new Bounds(Vector3.zero, Vector3.zero) : m_BeamGeom.meshRenderer.bounds;
			}
		}

		public string meshStats
		{
			get
			{
				Mesh mesh = ((!m_BeamGeom) ? null : m_BeamGeom.coneMesh);
				if ((bool)mesh)
				{
					return string.Format("Cone angle: {0:0.0} degrees\nMesh: {1} vertices, {2} triangles", coneAngle, mesh.vertexCount, mesh.triangles.Length / 3);
				}
				return "no mesh available";
			}
		}

		private Light lightSpotAttached
		{
			get
			{
				if (_CachedLight == null)
				{
					_CachedLight = GetComponent<Light>();
				}
				if ((bool)_CachedLight && _CachedLight.type == LightType.Spot)
				{
					return _CachedLight;
				}
				return null;
			}
		}

		public void SetClippingPlane(Plane planeWS)
		{
			if ((bool)m_BeamGeom)
			{
				m_BeamGeom.SetClippingPlane(planeWS);
			}
		}

		public void SetClippingPlaneOff()
		{
			if ((bool)m_BeamGeom)
			{
				m_BeamGeom.SetClippingPlaneOff();
			}
		}

		public float GetInsideBeamFactor(Vector3 posWS)
		{
			Vector3 aVector = base.transform.InverseTransformPoint(posWS);
			if (aVector.z < 0f)
			{
				return -1f;
			}
			Vector2 vector = new Vector2(aVector.xy().magnitude, aVector.z + coneApexOffsetZ);
			Vector2 normalized = vector.normalized;
			float f = coneAngle * ((float)Math.PI / 180f) / 2f;
			return Mathf.Clamp((Mathf.Abs(Mathf.Sin(f)) - Mathf.Abs(normalized.x)) / 0.1f, -1f, 1f);
		}

		[Obsolete("Use 'GenerateGeometry()' instead")]
		public void Generate()
		{
			GenerateGeometry();
		}

		public void GenerateGeometry()
		{
			pluginVersion = 1370;
			ValidateProperties();
			if (m_BeamGeom == null)
			{
				Shader beamShader = Config.Instance.beamShader;
				if (!beamShader)
				{
					Debug.LogError("Invalid BeamShader set in VLB Config");
					return;
				}
				m_BeamGeom = Utils.NewWithComponent<BeamGeometry>("Beam Geometry");
				m_BeamGeom.Initialize(this, beamShader);
			}
			m_BeamGeom.RegenerateMesh();
			m_BeamGeom.visible = base.enabled;
		}

		public void UpdateAfterManualPropertyChange()
		{
			ValidateProperties();
			if ((bool)m_BeamGeom)
			{
				m_BeamGeom.UpdateMaterialAndBounds();
			}
		}

		private void Start()
		{
			GenerateGeometry();
		}

		private void OnEnable()
		{
			if ((bool)m_BeamGeom)
			{
				m_BeamGeom.visible = true;
			}
			StartPlaytimeUpdateIfNeeded();
		}

		private void OnDisable()
		{
			if ((bool)m_BeamGeom)
			{
				m_BeamGeom.visible = false;
			}
			m_CoPlaytimeUpdate = null;
		}

		private void StartPlaytimeUpdateIfNeeded()
		{
			if (Application.isPlaying && trackChangesDuringPlaytime && m_CoPlaytimeUpdate == null)
			{
				m_CoPlaytimeUpdate = StartCoroutine(CoPlaytimeUpdate());
			}
		}

		private IEnumerator CoPlaytimeUpdate()
		{
			while (trackChangesDuringPlaytime && base.enabled)
			{
				UpdateAfterManualPropertyChange();
				yield return null;
			}
			m_CoPlaytimeUpdate = null;
		}

		private void OnDestroy()
		{
			if ((bool)m_BeamGeom)
			{
				UnityEngine.Object.DestroyImmediate(m_BeamGeom.gameObject);
			}
			m_BeamGeom = null;
		}

		private void AssignPropertiesFromSpotLight(Light lightSpot)
		{
			if ((bool)lightSpot && lightSpot.type == LightType.Spot)
			{
				if (fadeEndFromLight)
				{
					fadeEnd = lightSpot.range;
				}
				if (spotAngleFromLight)
				{
					spotAngle = lightSpot.spotAngle;
				}
				if (colorFromLight)
				{
					colorMode = ColorMode.Flat;
					color = lightSpot.color;
				}
			}
		}

		private void ClampProperties()
		{
			alpha = Mathf.Clamp01(alpha);
			attenuationCustomBlending = Mathf.Clamp01(attenuationCustomBlending);
			fadeEnd = Mathf.Max(0.01f, fadeEnd);
			fadeStart = Mathf.Clamp(fadeStart, 0f, fadeEnd - 0.01f);
			spotAngle = Mathf.Clamp(spotAngle, 0.1f, 179.9f);
			coneRadiusStart = Mathf.Max(coneRadiusStart, 0f);
			depthBlendDistance = Mathf.Max(depthBlendDistance, 0f);
			cameraClippingDistance = Mathf.Max(cameraClippingDistance, 0f);
			geomSides = Mathf.Clamp(geomSides, 3, 256);
			fresnelPow = Mathf.Max(0f, fresnelPow);
			glareBehind = Mathf.Clamp01(glareBehind);
			glareFrontal = Mathf.Clamp01(glareFrontal);
			noiseIntensity = Mathf.Clamp01(noiseIntensity);
		}

		private void ValidateProperties()
		{
			AssignPropertiesFromSpotLight(lightSpotAttached);
			ClampProperties();
		}
	}
}
