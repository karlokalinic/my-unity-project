using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace AC
{
	[AddComponentMenu("Adventure Creator/Save system/Remember Footstep Sounds")]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_remember_footstep_sounds.html")]
	public class RememberFootstepSounds : Remember
	{
		public override string SaveData()
		{
			FootstepSoundData footstepSoundData = new FootstepSoundData();
			footstepSoundData.objectID = constantID;
			footstepSoundData.savePrevented = savePrevented;
			if ((bool)GetComponent<FootstepSounds>())
			{
				FootstepSounds component = GetComponent<FootstepSounds>();
				footstepSoundData.walkSounds = SoundsToString(component.footstepSounds);
				footstepSoundData.runSounds = SoundsToString(component.runSounds);
			}
			return Serializer.SaveScriptData<FootstepSoundData>(footstepSoundData);
		}

		public override void LoadData(string stringData)
		{
			FootstepSoundData footstepSoundData = Serializer.LoadScriptData<FootstepSoundData>(stringData);
			if (footstepSoundData == null)
			{
				return;
			}
			base.SavePrevented = footstepSoundData.savePrevented;
			if (!savePrevented && (bool)GetComponent<FootstepSounds>())
			{
				FootstepSounds component = GetComponent<FootstepSounds>();
				AudioClip[] array = StringToSounds(footstepSoundData.walkSounds);
				if (array != null && array.Length > 0)
				{
					component.footstepSounds = array;
				}
				AudioClip[] array2 = StringToSounds(footstepSoundData.runSounds);
				if (array2 != null && array2.Length > 0)
				{
					component.runSounds = array2;
				}
			}
		}

		private AudioClip[] StringToSounds(string dataString)
		{
			if (string.IsNullOrEmpty(dataString))
			{
				return null;
			}
			List<AudioClip> list = new List<AudioClip>();
			string[] array = dataString.Split("|"[0]);
			foreach (string text in array)
			{
				AudioClip audioClip = AssetLoader.RetrieveAudioClip(text);
				if (audioClip != null)
				{
					list.Add(audioClip);
				}
			}
			return list.ToArray();
		}

		private string SoundsToString(AudioClip[] audioClips)
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < audioClips.Length; i++)
			{
				if (audioClips[i] != null)
				{
					stringBuilder.Append(AssetLoader.GetAssetInstanceID(audioClips[i]));
					if (i < audioClips.Length - 1)
					{
						stringBuilder.Append("|");
					}
				}
			}
			return stringBuilder.ToString();
		}
	}
}
