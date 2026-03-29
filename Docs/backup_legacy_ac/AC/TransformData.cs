using System;

namespace AC
{
	[Serializable]
	public class TransformData
	{
		public int objectID;

		public bool savePrevented;

		public float LocX;

		public float LocY;

		public float LocZ;

		public float RotX;

		public float RotY;

		public float RotZ;

		public float ScaleX;

		public float ScaleY;

		public float ScaleZ;

		public bool bringBack;

		public int linkedPrefabID;

		public int parentID;

		public bool parentIsNPC;

		public bool parentIsPlayer;

		public Hand heldHand;
	}
}
