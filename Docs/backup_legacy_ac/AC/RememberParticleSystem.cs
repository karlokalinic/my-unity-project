using UnityEngine;

namespace AC
{
	[AddComponentMenu("Adventure Creator/Save system/Remember ParticleSystem")]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_remember_particle_system.html")]
	public class RememberParticleSystem : Remember
	{
		public override string SaveData()
		{
			ParticleSystemData particleSystemData = new ParticleSystemData();
			particleSystemData.objectID = constantID;
			particleSystemData.savePrevented = savePrevented;
			ParticleSystem component = GetComponent<ParticleSystem>();
			if (component != null)
			{
				particleSystemData.isPlaying = component.isPlaying;
				particleSystemData.isPaused = component.isPaused;
				particleSystemData.currentTime = component.time;
			}
			return Serializer.SaveScriptData<ShapeableData>(particleSystemData);
		}

		public override void LoadData(string stringData)
		{
			ParticleSystemData particleSystemData = Serializer.LoadScriptData<ParticleSystemData>(stringData);
			if (particleSystemData == null)
			{
				return;
			}
			base.SavePrevented = particleSystemData.savePrevented;
			if (savePrevented)
			{
				return;
			}
			ParticleSystem component = GetComponent<ParticleSystem>();
			if (component != null)
			{
				component.time = particleSystemData.currentTime;
				if (particleSystemData.isPlaying)
				{
					component.Play();
				}
				else if (particleSystemData.isPaused)
				{
					component.Pause();
				}
				else
				{
					component.Stop();
				}
				component.time = particleSystemData.currentTime;
			}
		}
	}
}
