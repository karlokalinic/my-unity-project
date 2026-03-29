using System;

namespace AC
{
	[Serializable]
	public class SelectiveLoad
	{
		public bool loadVariables;

		public bool loadInventory;

		public bool loadPlayer;

		public bool loadScene;

		public bool loadSubScenes;

		public bool loadSceneObjects;

		public SelectiveLoad()
		{
			loadVariables = true;
			loadPlayer = true;
			loadSceneObjects = true;
			loadScene = true;
			loadInventory = true;
			loadSubScenes = true;
		}
	}
}
