using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace VLB
{
	[AddComponentMenu("")]
	[ExecuteInEditMode]
	[HelpURL("http://saladgamer.com/vlb-doc/comp-lightbeam/")]
	public class BeamGeometry : MonoBehaviour
	{
		private const int kNbSegments = 0;

		private VolumetricLightBeam m_Master;

		private Matrix4x4 m_ColorGradientMatrix;

		public MeshRenderer meshRenderer { get; private set; }

		public MeshFilter meshFilter { get; private set; }

		public Material material { get; private set; }

		public Mesh coneMesh { get; private set; }

		public bool visible
		{
			get
			{
				return meshRenderer.enabled;
			}
			set
			{
				meshRenderer.enabled = value;
			}
		}

		public int sortingLayerID
		{
			get
			{
				return meshRenderer.sortingLayerID;
			}
			set
			{
				meshRenderer.sortingLayerID = value;
			}
		}

		public int sortingOrder
		{
			get
			{
				return meshRenderer.sortingOrder;
			}
			set
			{
				meshRenderer.sortingOrder = value;
			}
		}

		private void Start()
		{
			if (!m_Master)
			{
				UnityEngine.Object.DestroyImmediate(base.gameObject);
			}
		}

		private void OnDestroy()
		{
			if ((bool)material)
			{
				UnityEngine.Object.DestroyImmediate(material);
				material = null;
			}
		}

		public void Initialize(VolumetricLightBeam master, Shader shader)
		{
			HideFlags proceduralObjectsHideFlags = Consts.ProceduralObjectsHideFlags;
			m_Master = master;
			base.transform.SetParent(master.transform, false);
			material = new Material(shader);
			material.hideFlags = proceduralObjectsHideFlags;
			meshRenderer = base.gameObject.GetOrAddComponent<MeshRenderer>();
			meshRenderer.hideFlags = proceduralObjectsHideFlags;
			meshRenderer.material = material;
			meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
			meshRenderer.receiveShadows = false;
			meshRenderer.lightProbeUsage = LightProbeUsage.Off;
			sortingLayerID = m_Master.sortingLayerID;
			sortingOrder = m_Master.sortingOrder;
			meshFilter = base.gameObject.GetOrAddComponent<MeshFilter>();
			meshFilter.hideFlags = proceduralObjectsHideFlags;
			base.gameObject.hideFlags = proceduralObjectsHideFlags;
		}

		public void RegenerateMesh()
		{
			base.gameObject.layer = Config.Instance.geometryLayerID;
			base.gameObject.tag = Config.Instance.geometryTag;
			if ((bool)coneMesh)
			{
				UnityEngine.Object.DestroyImmediate(coneMesh);
			}
			coneMesh = MeshGenerator.GenerateConeZ_Radius(1f, 1f, 1f, m_Master.geomSides, 0, m_Master.geomCap);
			coneMesh.hideFlags = Consts.ProceduralObjectsHideFlags;
			meshFilter.mesh = coneMesh;
			UpdateMaterialAndBounds();
		}

		private void ComputeBounds()
		{
			if ((bool)coneMesh)
			{
				float coneRadiusStart = m_Master.coneRadiusStart;
				float coneRadiusEnd = m_Master.coneRadiusEnd;
				float num = Mathf.Max(coneRadiusStart, coneRadiusEnd);
				float fadeEnd = m_Master.fadeEnd;
				Bounds bounds = new Bounds(new Vector3(0f, 0f, fadeEnd / 2f), new Vector3(num * 2f, num * 2f, fadeEnd));
				coneMesh.bounds = bounds;
			}
		}

		public void UpdateMaterialAndBounds()
		{
			float f = m_Master.coneAngle * ((float)Math.PI / 180f) / 2f;
			Vector2 vector = new Vector2(m_Master.coneRadiusStart, m_Master.coneRadiusEnd);
			material.SetVector("_ConeSlopeCosSin", new Vector2(Mathf.Cos(f), Mathf.Sin(f)));
			material.SetVector("_ConeRadius", vector);
			material.SetFloat("_ConeApexOffsetZ", m_Master.coneApexOffsetZ);
			if (m_Master.colorMode == VolumetricLightBeam.ColorMode.Gradient)
			{
				material.EnableKeyword("VLB_COLOR_GRADIENT_MATRIX");
				m_ColorGradientMatrix = m_Master.colorGradient.SampleInMatrix();
			}
			else
			{
				material.DisableKeyword("VLB_COLOR_GRADIENT_MATRIX");
				material.SetColor("_ColorFlat", m_Master.color);
			}
			material.SetFloat("_Alpha", m_Master.alpha);
			material.SetFloat("_AttenuationLerpLinearQuad", m_Master.attenuationLerpLinearQuad);
			material.SetFloat("_DistanceFadeStart", m_Master.fadeStart);
			material.SetFloat("_DistanceFadeEnd", m_Master.fadeEnd);
			material.SetFloat("_DistanceCamClipping", m_Master.cameraClippingDistance);
			material.SetFloat("_FresnelPow", m_Master.fresnelPow);
			material.SetFloat("_GlareBehind", m_Master.glareBehind);
			material.SetFloat("_GlareFrontal", m_Master.glareFrontal);
			if (m_Master.depthBlendDistance > 0f)
			{
				material.EnableKeyword("VLB_DEPTH_BLEND");
				material.SetFloat("_DepthBlendDistance", m_Master.depthBlendDistance);
			}
			else
			{
				material.DisableKeyword("VLB_DEPTH_BLEND");
			}
			if (m_Master.noiseEnabled && m_Master.noiseIntensity > 0f && Noise3D.isSupported)
			{
				Noise3D.LoadIfNeeded();
				material.EnableKeyword("VLB_NOISE_3D");
				material.SetVector("_NoiseLocal", new Vector4(m_Master.noiseVelocityLocal.x, m_Master.noiseVelocityLocal.y, m_Master.noiseVelocityLocal.z, m_Master.noiseScaleLocal));
				material.SetVector("_NoiseParam", new Vector3(m_Master.noiseIntensity, (!m_Master.noiseVelocityUseGlobal) ? 0f : 1f, (!m_Master.noiseScaleUseGlobal) ? 0f : 1f));
			}
			else
			{
				material.DisableKeyword("VLB_NOISE_3D");
			}
			ComputeBounds();
		}

		public void SetClippingPlane(Plane planeWS)
		{
			Vector3 normal = planeWS.normal;
			material.EnableKeyword("VLB_CLIPPING_PLANE");
			material.SetVector("_ClippingPlaneWS", new Vector4(normal.x, normal.y, normal.z, planeWS.distance));
		}

		public void SetClippingPlaneOff()
		{
			material.DisableKeyword("VLB_CLIPPING_PLANE");
		}

		private void OnWillRenderObject()
		{
			if (!m_Master)
			{
				return;
			}
			Camera current = Camera.current;
			if ((bool)material)
			{
				Vector3 normalized = base.transform.InverseTransformDirection(current.transform.forward).normalized;
				float w = ((!current.orthographic) ? m_Master.GetInsideBeamFactor(current.transform.position) : (-1f));
				material.SetVector("_CameraParams", new Vector4(normalized.x, normalized.y, normalized.z, w));
				if (m_Master.colorMode == VolumetricLightBeam.ColorMode.Gradient)
				{
					material.SetMatrix("_ColorGradientMatrix", m_ColorGradientMatrix);
				}
			}
			if (m_Master.depthBlendDistance > 0f)
			{
				current.depthTextureMode |= DepthTextureMode.Depth;
			}
		}
	}
}
