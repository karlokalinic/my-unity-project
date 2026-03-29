using UnityEngine;

namespace nightowl.HoloShaderPack
{
	public class DissolveFlicker : MonoBehaviour
	{
		public Material Material;

		public float MinStrength;

		public float MaxStrength = 1f;

		public float MaxStep = 0.05f;

		public float TickDelay = 0.1f;

		private float timer;

		private void Update()
		{
			timer += Time.deltaTime;
			if (timer >= TickDelay)
			{
				timer -= TickDelay;
				UpdateDissolve();
			}
		}

		private void UpdateDissolve()
		{
			Vector4 vector = Material.GetVector("_DistortionSettings");
			vector.x += (Random.value - 0.5f) * MaxStep;
			if (vector.x < MinStrength)
			{
				vector.x = MinStrength;
			}
			if (vector.x > MaxStrength)
			{
				vector.x = MaxStrength;
			}
			Material.SetVector("_DistortionSettings", vector);
		}
	}
}
