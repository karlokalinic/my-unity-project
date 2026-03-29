using System;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class TrackSnapData
	{
		[SerializeField]
		protected float positionAlong;

		[SerializeField]
		protected float width;

		[SerializeField]
		protected int id;

		public float PositionAlong
		{
			get
			{
				return positionAlong;
			}
		}

		public float Width
		{
			get
			{
				return width;
			}
		}

		public int ID
		{
			get
			{
				return id;
			}
		}

		public TrackSnapData(float _positionAlong, int[] idArray)
		{
			positionAlong = _positionAlong;
			width = 0.1f;
			id = 0;
			if (idArray == null || idArray.Length <= 0)
			{
				return;
			}
			foreach (int num in idArray)
			{
				if (id == num)
				{
					id++;
				}
			}
		}

		public float GetDistanceFrom(float trackValue)
		{
			float num = positionAlong - trackValue;
			if (Mathf.Abs(num) > width)
			{
				return float.PositiveInfinity;
			}
			return num;
		}

		public void MoveTo(Moveable_Drag draggable, float speed)
		{
			draggable.AutoMoveAlongTrack(positionAlong, speed, true, 1, ID);
		}

		public bool IsWithinRegion(float trackValue)
		{
			if (GetDistanceFrom(trackValue) <= width)
			{
				return true;
			}
			return false;
		}
	}
}
