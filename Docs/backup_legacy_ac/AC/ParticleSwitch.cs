using UnityEngine;

namespace AC
{
	[AddComponentMenu("Adventure Creator/Misc/Particle switch")]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_particle_switch.html")]
	public class ParticleSwitch : MonoBehaviour
	{
		public bool enableOnStart;

		protected ParticleSystem _particleSystem;

		protected ParticleSystem ParticleSystem
		{
			get
			{
				if (_particleSystem == null)
				{
					_particleSystem = GetComponent<ParticleSystem>();
					if (_particleSystem == null)
					{
						ACDebug.LogWarning("No Particle System attached to Particle Switch!", this);
					}
				}
				return _particleSystem;
			}
		}

		protected void Awake()
		{
			Switch(enableOnStart);
		}

		public void TurnOn()
		{
			Switch(true);
		}

		public void TurnOff()
		{
			Switch(false);
		}

		public void Pause()
		{
			if (ParticleSystem != null && !ParticleSystem.isPaused)
			{
				ParticleSystem.Pause();
			}
		}

		public void Interact()
		{
			if (ParticleSystem != null)
			{
				ParticleSystem.Emit(ParticleSystem.main.maxParticles);
			}
		}

		protected void Switch(bool turnOn)
		{
			if (ParticleSystem != null)
			{
				if (turnOn)
				{
					ParticleSystem.Play();
				}
				else
				{
					ParticleSystem.Stop();
				}
			}
		}
	}
}
