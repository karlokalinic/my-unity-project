using UnityEngine;

namespace nightowl.HoloShaderPack
{
	public class CodeExample10 : MonoBehaviour
	{
		public Material Material;

		public float switchDelay = 2f;

		private float currentTime;

		private void Update()
		{
			currentTime += Time.deltaTime;
			if (currentTime > switchDelay)
			{
				currentTime -= switchDelay;
				Vector4 vector = Material.GetVector("_ScanLineSettings");
				vector.z = Random.value * 30f;
				Material.SetVector("_ScanLineSettings", vector);
			}
		}
	}
}
