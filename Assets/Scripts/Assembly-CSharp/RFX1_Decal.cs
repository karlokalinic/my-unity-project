using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class RFX1_Decal : MonoBehaviour
{
	public bool UseWorldSpaceRotation;

	public bool UseRandomRotationAndScale = true;

	public float randomScalePercent = 20f;

	public bool IsScreenSpace = true;

	private ParticleSystem ps;

	private ParticleSystem.MainModule psMain;

	private MaterialPropertyBlock props;

	private MeshRenderer rend;

	private Vector3 startScale;

	private Vector3 worldRotation = new Vector3(0f, 0f, 0f);

	private void Awake()
	{
		startScale = base.transform.localScale;
	}

	private void OnEnable()
	{
		MeshRenderer component = GetComponent<MeshRenderer>();
		if (component == null)
		{
			return;
		}
		ps = GetComponent<ParticleSystem>();
		if (ps != null)
		{
			psMain = ps.main;
		}
		if (Camera.main.depthTextureMode != DepthTextureMode.Depth)
		{
			Camera.main.depthTextureMode = DepthTextureMode.Depth;
		}
		GetComponent<MeshRenderer>().reflectionProbeUsage = ReflectionProbeUsage.Off;
		if (!IsScreenSpace)
		{
			Material sharedMaterial = GetComponent<Renderer>().sharedMaterial;
			sharedMaterial.EnableKeyword("USE_QUAD_DECAL");
			sharedMaterial.SetInt("_ZTest1", 4);
			if (Application.isPlaying)
			{
				Vector3 localPosition = base.transform.localPosition;
				localPosition.z += 0.1f;
				base.transform.localPosition = localPosition;
				Vector3 localScale = base.transform.localScale;
				localScale.y = 0.001f;
				base.transform.localScale = localScale;
			}
		}
		else
		{
			Material sharedMaterial2 = GetComponent<Renderer>().sharedMaterial;
			sharedMaterial2.DisableKeyword("USE_QUAD_DECAL");
			sharedMaterial2.SetInt("_ZTest1", 5);
		}
		if (Application.isPlaying && UseRandomRotationAndScale && !UseWorldSpaceRotation)
		{
			base.transform.localRotation = Quaternion.Euler(Random.Range(0, 360), 90f, 90f);
			float num = Random.Range(startScale.x - startScale.x * randomScalePercent * 0.01f, startScale.x + startScale.x * randomScalePercent * 0.01f);
			base.transform.localScale = new Vector3(num, (!IsScreenSpace) ? 0.001f : startScale.y, num);
		}
	}

	private void LateUpdate()
	{
		Matrix4x4 worldToLocalMatrix = base.transform.worldToLocalMatrix;
		if (props == null)
		{
			props = new MaterialPropertyBlock();
		}
		if (rend == null)
		{
			rend = GetComponent<MeshRenderer>();
		}
		rend.GetPropertyBlock(props);
		props.SetMatrix("_InverseTransformMatrix", worldToLocalMatrix);
		rend.SetPropertyBlock(props);
		if (ps != null)
		{
			psMain.scalingMode = ParticleSystemScalingMode.Hierarchy;
		}
		if (UseWorldSpaceRotation)
		{
			base.transform.rotation = Quaternion.Euler(worldRotation);
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.matrix = Matrix4x4.TRS(base.transform.TransformPoint(Vector3.zero), base.transform.rotation, base.transform.lossyScale);
		Gizmos.color = new Color(1f, 1f, 1f, 1f);
		Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
	}
}
