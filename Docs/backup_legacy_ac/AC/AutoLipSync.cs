using UnityEngine;

namespace AC
{
	[RequireComponent(typeof(AudioSource))]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_auto_lip_sync.html")]
	public class AutoLipSync : MonoBehaviour
	{
		public Transform jawBone;

		public Coord coordinateToAffect;

		public float rotationFactor = 10f;

		public bool isAdditive = true;

		protected float volume;

		protected float bin = 0.04f;

		protected int width = 64;

		protected float output;

		protected float[] array;

		protected Quaternion jawRotation;

		protected AudioSource _audio;

		protected Quaternion originalRotation;

		protected Char _character;

		protected void Awake()
		{
			_audio = GetComponent<AudioSource>();
			_character = GetComponent<Char>();
			array = new float[width];
			originalRotation = jawBone.localRotation;
		}

		protected void LateUpdate()
		{
			if (_audio.isPlaying)
			{
				_audio.GetOutputData(array, 0);
				float num = 0f;
				for (int i = 0; i < width; i++)
				{
					float num2 = Mathf.Abs(array[i]);
					num += num2;
				}
				num /= (float)width;
				if (Options.GetSpeechVolume() > 0f)
				{
					num /= Options.GetSpeechVolume();
				}
				if (Mathf.Abs(num - volume) > bin)
				{
					volume = num;
				}
				volume = Mathf.Clamp01(volume * 2f);
				volume *= 0.3f;
				output = Mathf.Lerp(output, volume, Time.deltaTime * Mathf.Abs(rotationFactor));
			}
			else
			{
				output = 0f;
			}
			if (_character != null && !_character.isTalking && Mathf.Approximately(output, 0f))
			{
				return;
			}
			jawRotation = ((!isAdditive) ? originalRotation : jawBone.localRotation);
			if (coordinateToAffect == Coord.W)
			{
				if (rotationFactor < 0f)
				{
					jawRotation.w += output;
				}
				else
				{
					jawRotation.w -= output;
				}
			}
			else if (coordinateToAffect == Coord.X)
			{
				if (rotationFactor < 0f)
				{
					jawRotation.x += output;
				}
				else
				{
					jawRotation.x -= output;
				}
			}
			else if (coordinateToAffect == Coord.Y)
			{
				if (rotationFactor < 0f)
				{
					jawRotation.y += output;
				}
				else
				{
					jawRotation.y -= output;
				}
			}
			else if (coordinateToAffect == Coord.Z)
			{
				if (rotationFactor < 0f)
				{
					jawRotation.z += output;
				}
				else
				{
					jawRotation.z -= output;
				}
			}
			jawBone.localRotation = jawRotation;
		}
	}
}
