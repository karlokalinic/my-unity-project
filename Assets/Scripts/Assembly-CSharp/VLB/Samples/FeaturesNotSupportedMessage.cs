using UnityEngine;
using UnityEngine.UI;

namespace VLB.Samples
{
	[RequireComponent(typeof(Text))]
	public class FeaturesNotSupportedMessage : MonoBehaviour
	{
		private void Start()
		{
			Text component = GetComponent<Text>();
			component.text = ((!Noise3D.isSupported) ? Noise3D.isNotSupportedString : string.Empty);
		}
	}
}
