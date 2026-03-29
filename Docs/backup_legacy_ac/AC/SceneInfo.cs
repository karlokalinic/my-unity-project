using UnityEngine;

namespace AC
{
	public class SceneInfo
	{
		public string name;

		public int number;

		public bool IsNull
		{
			get
			{
				if (string.IsNullOrEmpty(name) && number == -1)
				{
					return true;
				}
				return false;
			}
		}

		public SceneInfo()
		{
			number = UnityVersionHandler.GetCurrentSceneNumber();
			name = UnityVersionHandler.GetCurrentSceneName();
		}

		public SceneInfo(string _name, int _number)
		{
			number = _number;
			name = _name;
		}

		public SceneInfo(string _name)
		{
			name = _name;
			number = -1;
		}

		public SceneInfo(int _number)
		{
			number = _number;
			name = string.Empty;
		}

		public SceneInfo(ChooseSceneBy chooseSceneBy, string _name, int _number)
		{
			if (chooseSceneBy == ChooseSceneBy.Number || string.IsNullOrEmpty(_name))
			{
				number = _number;
				name = string.Empty;
			}
			else
			{
				name = _name;
				number = -1;
			}
		}

		public bool IsValid()
		{
			if (string.IsNullOrEmpty(name) && number < 0)
			{
				return false;
			}
			return true;
		}

		public bool Matches(SceneInfo _sceneInfo)
		{
			if (_sceneInfo != null)
			{
				if (number == _sceneInfo.number && number >= 0)
				{
					if (!string.IsNullOrEmpty(name) && name == _sceneInfo.name)
					{
						return true;
					}
					if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(_sceneInfo.name))
					{
						return true;
					}
				}
				else if ((number == -1 || _sceneInfo.number == -1) && !string.IsNullOrEmpty(name))
				{
					return name == _sceneInfo.name;
				}
			}
			return false;
		}

		public bool IsCurrentActive()
		{
			SceneInfo sceneInfo = new SceneInfo(UnityVersionHandler.GetCurrentSceneName(), UnityVersionHandler.GetCurrentSceneNumber());
			return Matches(sceneInfo);
		}

		public string GetLabel()
		{
			if (!string.IsNullOrEmpty(name))
			{
				return name;
			}
			return number.ToString();
		}

		public void LoadLevel(bool forceReload = false)
		{
			if (!string.IsNullOrEmpty(name))
			{
				UnityVersionHandler.OpenScene(name, forceReload);
			}
			else
			{
				UnityVersionHandler.OpenScene(number, forceReload);
			}
		}

		public void AddLevel()
		{
			if (!string.IsNullOrEmpty(name))
			{
				UnityVersionHandler.OpenScene(name, false, true);
			}
			else
			{
				UnityVersionHandler.OpenScene(number, false, true);
			}
		}

		public bool CloseLevel()
		{
			if (!string.IsNullOrEmpty(name))
			{
				return UnityVersionHandler.CloseScene(name);
			}
			return UnityVersionHandler.CloseScene(number);
		}

		public AsyncOperation LoadLevelASync()
		{
			return UnityVersionHandler.LoadLevelAsync(number, name);
		}
	}
}
