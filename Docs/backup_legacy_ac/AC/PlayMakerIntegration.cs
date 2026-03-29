using UnityEngine;

namespace AC
{
	public class PlayMakerIntegration
	{
		public static bool IsDefinePresent()
		{
			return false;
		}

		public static bool HasFSM(GameObject gameObject)
		{
			return false;
		}

		public static void CallEvent(GameObject linkedObject, string eventName, string fsmName)
		{
		}

		public static void CallEvent(GameObject linkedObject, string eventName)
		{
		}

		public static int GetInt(string _name, Variables _variables)
		{
			return 0;
		}

		public static bool GetBool(string _name, Variables _variables)
		{
			return false;
		}

		public static string GetString(string _name, Variables _variables)
		{
			return string.Empty;
		}

		public static float GetFloat(string _name, Variables _variables)
		{
			return 0f;
		}

		public static Vector3 GetVector3(string _name, Variables _variables)
		{
			return Vector3.zero;
		}

		public static void SetInt(string _name, int _val, Variables _variables)
		{
		}

		public static void SetBool(string _name, bool _val, Variables _variables)
		{
		}

		public static void SetString(string _name, string _val, Variables _variables)
		{
		}

		public static void SetFloat(string _name, float _val, Variables _variables)
		{
		}

		public static void SetVector3(string _name, Vector3 _val, Variables _variables)
		{
		}
	}
}
