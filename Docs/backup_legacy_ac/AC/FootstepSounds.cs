using UnityEngine;

namespace AC
{
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_footstep_sounds.html")]
	[AddComponentMenu("Adventure Creator/Characters/Footstep sounds")]
	public class FootstepSounds : MonoBehaviour
	{
		public enum FootstepPlayMethod
		{
			Automatically = 0,
			ViaAnimationEvents = 1
		}

		public AudioClip[] footstepSounds;

		public AudioClip[] runSounds;

		public Sound soundToPlayFrom;

		public FootstepPlayMethod footstepPlayMethod = FootstepPlayMethod.ViaAnimationEvents;

		public Char character;

		public bool doGroundedCheck;

		public float pitchVariance;

		public float volumeVariance;

		public float walkSeparationTime = 0.5f;

		public float runSeparationTime = 0.25f;

		protected float originalRelativeSound = 1f;

		protected int lastIndex;

		protected AudioSource audioSource;

		protected float delayTime;

		protected void Awake()
		{
			if (soundToPlayFrom != null)
			{
				audioSource = soundToPlayFrom.GetComponent<AudioSource>();
			}
			if (audioSource == null)
			{
				audioSource = GetComponent<AudioSource>();
			}
			if (character == null)
			{
				character = GetComponent<Char>();
			}
			delayTime = walkSeparationTime / 2f;
			if (soundToPlayFrom != null)
			{
				originalRelativeSound = soundToPlayFrom.relativeVolume;
			}
		}

		protected void Update()
		{
			if (character == null || footstepPlayMethod == FootstepPlayMethod.ViaAnimationEvents)
			{
				return;
			}
			if (character.charState == CharState.Move && !character.IsJumping)
			{
				delayTime -= Time.deltaTime;
				if (delayTime <= 0f)
				{
					delayTime = ((!character.isRunning) ? walkSeparationTime : runSeparationTime);
					PlayFootstep();
				}
			}
			else
			{
				delayTime = walkSeparationTime / 2f;
			}
		}

		public void PlayFootstep()
		{
			if (audioSource != null && footstepSounds.Length > 0 && (character == null || character.charState == CharState.Move) && (!doGroundedCheck || !(character != null) || character.IsGrounded(true)))
			{
				bool flag = ((character.isRunning && runSounds.Length > 0) ? true : false);
				if (flag)
				{
					PlaySound(runSounds, flag);
				}
				else
				{
					PlaySound(footstepSounds, flag);
				}
			}
		}

		protected void PlaySound(AudioClip[] clips, bool isRunSound)
		{
			if (clips == null)
			{
				return;
			}
			if (clips.Length == 1)
			{
				PlaySound(clips[0], isRunSound);
				return;
			}
			int num = Random.Range(0, clips.Length - 1);
			if (num == lastIndex)
			{
				num++;
				if (num >= clips.Length)
				{
					num = 0;
				}
			}
			PlaySound(clips[num], isRunSound);
			lastIndex = num;
		}

		protected void PlaySound(AudioClip clip, bool isRunSound)
		{
			if (clip == null)
			{
				return;
			}
			audioSource.clip = clip;
			if (pitchVariance > 0f)
			{
				float pitch = 1f + Random.Range(0f - pitchVariance, pitchVariance);
				audioSource.pitch = pitch;
			}
			if (volumeVariance > 0f)
			{
				float num = 1f - Random.Range(0f, volumeVariance);
				if (soundToPlayFrom != null)
				{
					soundToPlayFrom.ChangeRelativeVolume(num * originalRelativeSound);
				}
				else
				{
					audioSource.volume = num;
				}
			}
			if (soundToPlayFrom != null)
			{
				soundToPlayFrom.Play(false);
				if (KickStarter.eventManager != null)
				{
					KickStarter.eventManager.Call_OnPlayFootstepSound(character, this, !isRunSound, soundToPlayFrom.audioSource, clip);
				}
				return;
			}
			audioSource.loop = false;
			audioSource.Play();
			if (KickStarter.eventManager != null)
			{
				KickStarter.eventManager.Call_OnPlayFootstepSound(character, this, !isRunSound, audioSource, clip);
			}
		}
	}
}
