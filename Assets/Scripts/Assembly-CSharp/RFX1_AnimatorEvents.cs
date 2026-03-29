using System;
using UnityEngine;

public class RFX1_AnimatorEvents : MonoBehaviour
{
	[Serializable]
	public class RFX1_EffectAnimatorProperty
	{
		public GameObject Prefab;

		public Transform BonePosition;

		public Transform BoneRotation;

		public float DestroyTime = 10f;

		[HideInInspector]
		public GameObject CurrentInstance;
	}

	public RFX1_EffectAnimatorProperty Effect1;

	public RFX1_EffectAnimatorProperty Effect2;

	public RFX1_EffectAnimatorProperty Effect3;

	public GameObject Target;

	[HideInInspector]
	public float HUE = -1f;

	[HideInInspector]
	public float Speed = -1f;

	private float oldHUE;

	private float oldSpeed;

	private void InstantiateEffect(RFX1_EffectAnimatorProperty effect)
	{
		if (effect.Prefab == null)
		{
			return;
		}
		effect.CurrentInstance = UnityEngine.Object.Instantiate(effect.Prefab, effect.BonePosition.position, effect.BoneRotation.rotation);
		if (HUE > -0.9f)
		{
			UpdateColor(effect);
		}
		if (Speed > -0.9f)
		{
			UpdateSpeed(effect);
		}
		if (Target != null)
		{
			RFX1_Target component = effect.CurrentInstance.GetComponent<RFX1_Target>();
			if (component != null)
			{
				component.Target = Target;
			}
		}
		if (effect.DestroyTime > 0.001f)
		{
			UnityEngine.Object.Destroy(effect.CurrentInstance, effect.DestroyTime);
		}
	}

	public void ActivateEffect1()
	{
		InstantiateEffect(Effect1);
	}

	public void ActivateEffect2()
	{
		InstantiateEffect(Effect2);
	}

	public void ActivateEffect3()
	{
		InstantiateEffect(Effect3);
	}

	private void LateUpdate()
	{
		UpdateInstance(Effect1);
		UpdateInstance(Effect2);
		UpdateInstance(Effect3);
	}

	private void UpdateInstance(RFX1_EffectAnimatorProperty effect)
	{
		if (effect.CurrentInstance != null && effect.BonePosition != null)
		{
			effect.CurrentInstance.transform.position = effect.BonePosition.position;
			if (HUE > -0.9f && Mathf.Abs(oldHUE - HUE) > 0.001f)
			{
				UpdateColor(effect);
			}
			if (Speed > -0.9f && Mathf.Abs(oldSpeed - Speed) > 0.001f)
			{
				UpdateSpeed(effect);
			}
		}
	}

	private void UpdateSpeed(RFX1_EffectAnimatorProperty effect)
	{
		oldSpeed = Speed;
	}

	private void UpdateColor(RFX1_EffectAnimatorProperty effect)
	{
		oldHUE = HUE;
		RFX1_EffectSettingColor rFX1_EffectSettingColor = effect.CurrentInstance.GetComponent<RFX1_EffectSettingColor>();
		if (rFX1_EffectSettingColor == null)
		{
			rFX1_EffectSettingColor = effect.CurrentInstance.AddComponent<RFX1_EffectSettingColor>();
		}
		RFX1_ColorHelper.HSBColor hsbColor = RFX1_ColorHelper.ColorToHSV(rFX1_EffectSettingColor.Color);
		hsbColor.H = HUE;
		rFX1_EffectSettingColor.Color = RFX1_ColorHelper.HSVToColor(hsbColor);
	}
}
