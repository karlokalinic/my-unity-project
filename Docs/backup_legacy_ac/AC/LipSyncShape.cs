using UnityEngine;

namespace AC
{
	public struct LipSyncShape
	{
		public int frame;

		public float timeIndex;

		public LipSyncShape(int _frame, float _timeIndex, float speed, float fps = 1f)
		{
			frame = _frame;
			timeIndex = _timeIndex / 15f / speed / fps + Time.time;
		}
	}
}
