using System;
using System.Collections.Generic;

namespace AC
{
	[Serializable]
	public class SingleLevelData
	{
		public List<ScriptData> allScriptData;

		public List<TransformData> allTransformData;

		public int sceneNumber;

		public int navMesh;

		public int playerStart;

		public int sortingMap;

		public int tintMap;

		public int onLoadCutscene;

		public int onStartCutscene;

		public string activeLists;

		public string localVariablesData;

		public SingleLevelData()
		{
			allScriptData = new List<ScriptData>();
			allTransformData = new List<TransformData>();
		}

		public bool DataMatchesScene(SingleLevelData otherLevelData)
		{
			if (otherLevelData.sceneNumber == sceneNumber)
			{
				return true;
			}
			return false;
		}
	}
}
