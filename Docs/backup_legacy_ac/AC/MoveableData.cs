using System;

namespace AC
{
	[Serializable]
	public class MoveableData : RememberData
	{
		public bool isOn;

		public float trackValue;

		public int revolutions;

		public float LocX;

		public float LocY;

		public float LocZ;

		public bool doEulerRotation;

		public float RotW;

		public float RotX;

		public float RotY;

		public float RotZ;

		public float ScaleX;

		public float ScaleY;

		public float ScaleZ;

		public bool inWorldSpace;
	}
}
