using UnityEngine;

namespace AC
{
	public class RogoLipSyncIntegration
	{
		public static bool IsDefinePresent()
		{
			return false;
		}

		public static Object GetObjectToPing(string fullName)
		{
			return null;
		}

		public static void Play(Char speaker, int lineID, string language)
		{
			if (!(speaker == null))
			{
				ACDebug.LogError("The 'RogoLipSyncIsPresent' preprocessor define must be declared in the Player Settings.");
			}
		}

		public static void Stop(Char speaker)
		{
			if (!(speaker == null))
			{
			}
		}
	}
}
