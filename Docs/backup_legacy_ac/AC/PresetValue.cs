using System;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class PresetValue
	{
		public int id;

		public int val;

		public float floatVal;

		public string textVal;

		public Vector3 vector3Val;

		public PresetValue(GVar _gVar)
		{
			id = _gVar.id;
			val = _gVar.val;
			floatVal = _gVar.floatVal;
			textVal = _gVar.textVal;
			vector3Val = _gVar.vector3Val;
		}
	}
}
