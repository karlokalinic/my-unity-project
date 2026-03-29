using System;
using System.Collections.Generic;
using UnityEngine;

public class RFX1_TransformMotion : MonoBehaviour
{
	public enum RFX4_SimulationSpace
	{
		Local = 0,
		World = 1
	}

	public class RFX1_CollisionInfo : EventArgs
	{
		public RaycastHit Hit;
	}

	public float Distance = 30f;

	public float Speed = 1f;

	public float TimeDelay;

	public float RandomMoveRadius;

	public float RandomMoveSpeedScale;

	public GameObject Target;

	public LayerMask CollidesWith = -1;

	public GameObject[] EffectsOnCollision;

	public float CollisionOffset;

	public float DestroyTimeDelay = 5f;

	public bool CollisionEffectInWorldSpace = true;

	public GameObject[] DeactivatedObjectsOnCollision;

	[HideInInspector]
	public float HUE = -1f;

	[HideInInspector]
	public List<GameObject> CollidedInstances;

	private Vector3 startPosition;

	private Vector3 startPositionLocal;

	private Transform t;

	private Transform targetT;

	private Vector3 oldPos;

	private bool isCollided;

	private bool isOutDistance;

	private Quaternion startQuaternion;

	private float currentDelay;

	private const float RayCastTolerance = 0.15f;

	private bool isInitialized;

	private bool dropFirstFrameForFixUnityBugWithParticles;

	private Vector3 randomTimeOffset;

	public event EventHandler<RFX1_CollisionInfo> CollisionEnter;

	private void Start()
	{
		t = base.transform;
		if (Target != null)
		{
			targetT = Target.transform;
		}
		startQuaternion = t.rotation;
		startPositionLocal = t.localPosition;
		startPosition = t.position;
		oldPos = t.TransformPoint(startPositionLocal);
		Initialize();
		isInitialized = true;
	}

	private void OnEnable()
	{
		if (isInitialized)
		{
			Initialize();
		}
	}

	private void OnDisable()
	{
		if (isInitialized)
		{
			Initialize();
		}
	}

	private void Initialize()
	{
		isCollided = false;
		isOutDistance = false;
		currentDelay = 0f;
		startQuaternion = t.rotation;
		t.localPosition = startPositionLocal;
		oldPos = t.TransformPoint(startPositionLocal);
		OnCollisionDeactivateBehaviour(true);
		dropFirstFrameForFixUnityBugWithParticles = true;
		randomTimeOffset = UnityEngine.Random.insideUnitSphere * 10f;
	}

	private void Update()
	{
		if (!dropFirstFrameForFixUnityBugWithParticles)
		{
			UpdateWorldPosition();
		}
		else
		{
			dropFirstFrameForFixUnityBugWithParticles = false;
		}
	}

	private void UpdateWorldPosition()
	{
		currentDelay += Time.deltaTime;
		if (currentDelay < TimeDelay)
		{
			return;
		}
		Vector3 vector = Vector3.zero;
		if (RandomMoveRadius > 0f)
		{
			vector = GetRadiusRandomVector() * RandomMoveRadius;
			if (Target != null)
			{
				if (targetT == null)
				{
					targetT = Target.transform;
				}
				float num = Vector3.Distance(t.position, targetT.position) / Vector3.Distance(startPosition, targetT.position);
				vector *= num;
			}
		}
		Vector3 vector2 = Vector3.zero;
		Vector3 vector3 = Vector3.zero;
		if (!isCollided && !isOutDistance)
		{
			if (Target == null)
			{
				Vector3 vector4 = (Vector3.forward + vector) * Speed * Time.deltaTime;
				vector2 = t.localRotation * vector4;
				vector3 = startQuaternion * vector4;
			}
			else
			{
				Vector3 normalized = (targetT.position - t.position).normalized;
				Vector3 vector5 = (normalized + vector) * Speed * Time.deltaTime;
				vector2 = vector5;
				vector3 = vector5;
			}
		}
		float magnitude = (t.localPosition + vector2 - startPositionLocal).magnitude;
		Debug.DrawRay(t.position, vector3.normalized * (Distance - magnitude));
		RaycastHit hitInfo;
		if (!isCollided && Physics.Raycast(t.position, vector3.normalized, out hitInfo, Distance, CollidesWith) && vector2.magnitude + 0.15f > hitInfo.distance)
		{
			isCollided = true;
			t.position = hitInfo.point;
			oldPos = t.position;
			OnCollisionBehaviour(hitInfo);
			OnCollisionDeactivateBehaviour(false);
		}
		else if (!isOutDistance && magnitude + 0.15f > Distance)
		{
			isOutDistance = true;
			OnCollisionDeactivateBehaviour(false);
			if (Target == null)
			{
				t.localPosition = startPositionLocal + t.localRotation * (Vector3.forward + vector) * Distance;
			}
			else
			{
				Vector3 normalized2 = (targetT.position - t.position).normalized;
				t.position = startPosition + normalized2 * Distance;
			}
			oldPos = t.position;
		}
		else
		{
			t.position = oldPos + vector3;
			oldPos = t.position;
		}
	}

	private Vector3 GetRadiusRandomVector()
	{
		float num = Time.time * RandomMoveSpeedScale + randomTimeOffset.x;
		float x = Mathf.Sin(num / 7f + Mathf.Cos(num / 2f)) * Mathf.Cos(num / 5f + Mathf.Sin(num));
		num = Time.time * RandomMoveSpeedScale + randomTimeOffset.y;
		float y = Mathf.Cos(num / 8f + Mathf.Sin(num / 2f)) * Mathf.Sin(Mathf.Sin(num / 1.2f) + num * 1.2f);
		num = Time.time * RandomMoveSpeedScale + randomTimeOffset.z;
		float z = Mathf.Cos(num * 0.7f + Mathf.Cos(num * 0.5f)) * Mathf.Cos(Mathf.Sin(num * 0.8f) + num * 0.3f);
		return new Vector3(x, y, z);
	}

	private void OnCollisionBehaviour(RaycastHit hit)
	{
		EventHandler<RFX1_CollisionInfo> eventHandler = this.CollisionEnter;
		if (eventHandler != null)
		{
			eventHandler(this, new RFX1_CollisionInfo
			{
				Hit = hit
			});
		}
		CollidedInstances.Clear();
		GameObject[] effectsOnCollision = EffectsOnCollision;
		foreach (GameObject original in effectsOnCollision)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(original, hit.point + hit.normal * CollisionOffset, default(Quaternion));
			CollidedInstances.Add(gameObject);
			if (HUE > -0.9f)
			{
				RFX1_EffectSettingColor rFX1_EffectSettingColor = gameObject.AddComponent<RFX1_EffectSettingColor>();
				RFX1_ColorHelper.HSBColor hsbColor = RFX1_ColorHelper.ColorToHSV(rFX1_EffectSettingColor.Color);
				hsbColor.H = HUE;
				rFX1_EffectSettingColor.Color = RFX1_ColorHelper.HSVToColor(hsbColor);
			}
			gameObject.transform.LookAt(hit.point + hit.normal + hit.normal * CollisionOffset);
			if (!CollisionEffectInWorldSpace)
			{
				gameObject.transform.parent = base.transform;
			}
			UnityEngine.Object.Destroy(gameObject, DestroyTimeDelay);
		}
	}

	private void OnCollisionDeactivateBehaviour(bool active)
	{
		GameObject[] deactivatedObjectsOnCollision = DeactivatedObjectsOnCollision;
		foreach (GameObject gameObject in deactivatedObjectsOnCollision)
		{
			if (gameObject != null)
			{
				gameObject.SetActive(active);
			}
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (!Application.isPlaying)
		{
			t = base.transform;
			Gizmos.color = Color.blue;
			Gizmos.DrawLine(t.position, t.position + t.forward * Distance);
		}
	}
}
