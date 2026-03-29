using System;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class MusicStorage
	{
		public int ID;

		public AudioClip audioClip;

		public float relativeVolume;

		public MusicStorage(int[] idArray)
		{
			ID = 0;
			audioClip = null;
			relativeVolume = 1f;
			if (idArray == null || idArray.Length <= 0)
			{
				return;
			}
			foreach (int num in idArray)
			{
				if (ID == num)
				{
					ID++;
				}
			}
		}
	}
}
