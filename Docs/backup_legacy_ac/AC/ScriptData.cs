using System;

namespace AC
{
	[Serializable]
	public struct ScriptData
	{
		public int objectID;

		public string data;

		public ScriptData(int _objectID, string _data)
		{
			objectID = _objectID;
			data = _data;
		}
	}
}
