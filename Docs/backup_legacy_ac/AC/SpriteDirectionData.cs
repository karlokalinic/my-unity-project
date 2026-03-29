using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class SpriteDirectionData
	{
		private enum Directions
		{
			None = 0,
			Four = 1,
			Eight = 2,
			Custom = 3
		}

		[Serializable]
		public struct SpriteDirection
		{
			public string suffix;

			public float angle;

			public float maxAngle;

			public SpriteDirection(string suffix, float angle)
			{
				this.suffix = suffix;
				this.angle = angle;
				maxAngle = 0f;
			}

			public SpriteDirection(string suffix, float angle, float maxAngle)
			{
				this.suffix = suffix;
				this.angle = angle;
				this.maxAngle = maxAngle;
			}
		}

		[SerializeField]
		private Directions directions = Directions.Four;

		[SerializeField]
		private bool down = true;

		[SerializeField]
		private bool left = true;

		[SerializeField]
		private bool right = true;

		[SerializeField]
		private bool up = true;

		[SerializeField]
		private bool downLeft;

		[SerializeField]
		private bool upLeft;

		[SerializeField]
		private bool upRight;

		[SerializeField]
		private bool downRight;

		[SerializeField]
		private SpriteDirection[] spriteDirections = new SpriteDirection[0];

		[SerializeField]
		private bool isUpgraded;

		public bool IsUpgraded
		{
			get
			{
				return isUpgraded;
			}
		}

		public SpriteDirection[] SpriteDirections
		{
			get
			{
				return spriteDirections;
			}
		}

		public SpriteDirectionData(bool doDirections, bool doDiagonals)
		{
			isUpgraded = true;
			if (!doDirections)
			{
				directions = Directions.None;
			}
			else
			{
				directions = ((!doDiagonals) ? Directions.Four : Directions.Eight);
			}
			spriteDirections = new SpriteDirection[0];
			CalcDirections();
		}

		public bool HasDirections()
		{
			return spriteDirections.Length > 0;
		}

		private void CalcDirections()
		{
			if (directions == Directions.None)
			{
				spriteDirections = new SpriteDirection[0];
				down = (left = (right = (up = (downLeft = (upLeft = (upRight = (downRight = false)))))));
				return;
			}
			if (directions == Directions.Four)
			{
				down = (left = (right = (up = true)));
				downLeft = (upLeft = (upRight = (downRight = false)));
			}
			else if (directions == Directions.Eight)
			{
				down = (left = (right = (up = true)));
				downLeft = (upLeft = (upRight = (downRight = true)));
			}
			List<SpriteDirection> list = new List<SpriteDirection>();
			list.Clear();
			if (down)
			{
				list.Add(new SpriteDirection("D", 0f));
			}
			if (downLeft)
			{
				list.Add(new SpriteDirection("DL", 45f));
			}
			if (left)
			{
				list.Add(new SpriteDirection("L", 90f));
			}
			if (upLeft)
			{
				list.Add(new SpriteDirection("UL", 135f));
			}
			if (up)
			{
				list.Add(new SpriteDirection("U", 180f));
			}
			if (upRight)
			{
				list.Add(new SpriteDirection("UR", 225f));
			}
			if (right)
			{
				list.Add(new SpriteDirection("R", 270f));
			}
			if (downRight)
			{
				list.Add(new SpriteDirection("DR", 315f));
			}
			if (list.Count > 1)
			{
				for (int i = 0; i < list.Count; i++)
				{
					int index = ((i != list.Count - 1) ? (i + 1) : 0);
					float angle = list[i].angle;
					float num = list[index].angle;
					if (num < angle)
					{
						num += 360f;
					}
					float num2 = (angle + num) / 2f;
					if (num2 > 360f)
					{
						num2 -= 360f;
					}
					list[i] = new SpriteDirection(list[i].suffix, list[i].angle, num2);
				}
			}
			spriteDirections = list.ToArray();
		}

		public string GetDirectionalSuffix(float angle)
		{
			if (HasDirections())
			{
				if (angle > 360f)
				{
					angle -= 360f;
				}
				if (angle < 0f)
				{
					angle += 360f;
				}
				if (spriteDirections.Length <= 1)
				{
					return spriteDirections[0].suffix;
				}
				int num = 0;
				while (num < spriteDirections.Length)
				{
					if (num < spriteDirections.Length - 1)
					{
						if (spriteDirections[num + 1].maxAngle < spriteDirections[num].maxAngle && spriteDirections[num + 1].maxAngle > angle)
						{
							return spriteDirections[num + 1].suffix;
						}
						if (spriteDirections[num].maxAngle < angle)
						{
							num++;
							continue;
						}
					}
					else if (spriteDirections[num].maxAngle < angle && spriteDirections[0].maxAngle + 180f < angle)
					{
						return spriteDirections[0].suffix;
					}
					return spriteDirections[num].suffix;
				}
			}
			return string.Empty;
		}
	}
}
