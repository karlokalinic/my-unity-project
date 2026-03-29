using System;

namespace AC
{
	[Serializable]
	public class Ingredient
	{
		public int itemID;

		public int amount;

		public int slotNumber;

		public Ingredient()
		{
			itemID = 0;
			amount = 1;
			slotNumber = 1;
		}
	}
}
