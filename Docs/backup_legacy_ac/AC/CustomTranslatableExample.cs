using UnityEngine;

namespace AC
{
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_custom_translatable_example.html")]
	public class CustomTranslatableExample : MonoBehaviour, ITranslatable
	{
		public string myCustomText;

		public int myCustomLineID = -1;

		public string GetTranslatableString(int index)
		{
			return myCustomText;
		}

		public int GetTranslationID(int index)
		{
			return myCustomLineID;
		}
	}
}
