using UnityEngine;

namespace AC
{
	[RequireComponent(typeof(Player))]
	[AddComponentMenu("Adventure Creator/3rd-party/UFPS integration")]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_ultimate_f_p_s_integration.html")]
	public class UltimateFPSIntegration : MonoBehaviour
	{
		protected void Awake()
		{
		}

		protected void Start()
		{
			ACDebug.LogError("'UltimateFPSIsPresent' must be listed in your Unity Player Setting's 'Scripting define symbols' for AC's UFPS integration to work.", this);
		}
	}
}
