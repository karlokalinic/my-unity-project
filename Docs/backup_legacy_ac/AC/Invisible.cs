using UnityEngine;

namespace AC
{
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_invisible.html")]
	public class Invisible : MonoBehaviour
	{
		public enum ChildrenToAffect
		{
			None = 0,
			OnlyActive = 1,
			All = 2
		}

		public bool affectOwnGameObject = true;

		public ChildrenToAffect childrenToAffect;

		protected void Awake()
		{
			Renderer component = GetComponent<Renderer>();
			Renderer[] array = new Renderer[1] { component };
			if (childrenToAffect != ChildrenToAffect.None)
			{
				bool includeInactive = childrenToAffect == ChildrenToAffect.All;
				array = GetComponentsInChildren<Renderer>(includeInactive);
			}
			Renderer[] array2 = array;
			foreach (Renderer renderer in array2)
			{
				if (!(renderer != null) || !(renderer == component) || affectOwnGameObject)
				{
					renderer.enabled = false;
				}
			}
		}
	}
}
