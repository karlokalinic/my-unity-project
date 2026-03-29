using UnityEngine;

namespace AC
{
	[RequireComponent(typeof(AudioSource))]
	[RequireComponent(typeof(Sound))]
	[AddComponentMenu("Adventure Creator/Save system/Remember Sound")]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_remember_sound.html")]
	public class RememberSound : Remember
	{
		public override string SaveData()
		{
			Sound component = GetComponent<Sound>();
			SoundData soundData = new SoundData();
			soundData.objectID = constantID;
			soundData.savePrevented = savePrevented;
			soundData = component.GetSaveData(soundData);
			return Serializer.SaveScriptData<SoundData>(soundData);
		}

		public override void LoadData(string stringData, bool restoringSaveFile = false)
		{
			SoundData soundData = Serializer.LoadScriptData<SoundData>(stringData);
			if (soundData == null)
			{
				return;
			}
			base.SavePrevented = soundData.savePrevented;
			if (!savePrevented)
			{
				Sound component = GetComponent<Sound>();
				if (!(component is Music) && (restoringSaveFile || !component.surviveSceneChange))
				{
					component.LoadData(soundData);
				}
			}
		}
	}
}
