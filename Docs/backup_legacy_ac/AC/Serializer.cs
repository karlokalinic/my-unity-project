using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace AC
{
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_serializer.html")]
	public class Serializer : MonoBehaviour
	{
		public static T returnComponent<T>(int constantID, GameObject sceneObject = null) where T : Component
		{
			if (constantID != 0)
			{
				T[] ownSceneComponents = UnityVersionHandler.GetOwnSceneComponents<T>(sceneObject);
				T[] array = ownSceneComponents;
				for (int i = 0; i < array.Length; i++)
				{
					T result = array[i];
					ConstantID[] components = result.GetComponents<ConstantID>();
					if (components == null)
					{
						continue;
					}
					ConstantID[] array2 = components;
					foreach (ConstantID constantID2 in array2)
					{
						if (constantID2.constantID == constantID)
						{
							return result;
						}
					}
				}
			}
			return (T)null;
		}

		public static ConstantID returnConstantID(int constantID)
		{
			if (constantID != 0)
			{
				List<ConstantID> constantIDs = KickStarter.stateHandler.ConstantIDs;
				foreach (ConstantID item in constantIDs)
				{
					if (item.constantID == constantID)
					{
						return item;
					}
				}
			}
			return null;
		}

		public static T GetGameObjectComponent<T>(int constantID, GameObject gameObject) where T : Component
		{
			if (constantID != 0 && gameObject != null)
			{
				T[] componentsInChildren = gameObject.GetComponentsInChildren<T>();
				T[] array = componentsInChildren;
				for (int i = 0; i < array.Length; i++)
				{
					T result = array[i];
					ConstantID[] components = result.GetComponents<ConstantID>();
					if (components == null)
					{
						continue;
					}
					ConstantID[] array2 = components;
					foreach (ConstantID constantID2 in array2)
					{
						if (constantID2.constantID == constantID)
						{
							return result;
						}
					}
				}
			}
			return (T)null;
		}

		public static T[] returnComponents<T>(int constantID, GameObject gameObject = null) where T : Component
		{
			if (constantID != 0)
			{
				List<T> list = new List<T>();
				T[] ownSceneComponents = UnityVersionHandler.GetOwnSceneComponents<T>(gameObject);
				T[] array = ownSceneComponents;
				for (int i = 0; i < array.Length; i++)
				{
					T item = array[i];
					if (!item.GetComponent<ConstantID>())
					{
						continue;
					}
					ConstantID[] components = item.GetComponents<ConstantID>();
					ConstantID[] array2 = components;
					foreach (ConstantID constantID2 in array2)
					{
						if (constantID2.constantID == constantID && !list.Contains(item))
						{
							list.Add(item);
						}
					}
				}
				return list.ToArray();
			}
			return null;
		}

		public static int GetConstantID(GameObject _gameObject)
		{
			if (_gameObject != null)
			{
				if ((bool)_gameObject.GetComponent<ConstantID>())
				{
					if (_gameObject.GetComponent<ConstantID>().constantID != 0)
					{
						return _gameObject.GetComponent<ConstantID>().constantID;
					}
					ACDebug.LogWarning("GameObject " + _gameObject.name + " was not saved because it does not have a Constant ID number.", _gameObject);
				}
				else
				{
					ACDebug.LogWarning("GameObject " + _gameObject.name + " was not saved because it does not have a 'Constant ID' script - please exit Play mode and attach one to it.", _gameObject);
				}
			}
			return 0;
		}

		public static int GetConstantID(Transform _transform)
		{
			if (_transform != null)
			{
				if ((bool)_transform.GetComponent<ConstantID>())
				{
					if (_transform.GetComponent<ConstantID>().constantID != 0)
					{
						return _transform.GetComponent<ConstantID>().constantID;
					}
					ACDebug.LogWarning("GameObject " + _transform.gameObject.name + " was not saved because it does not have a Constant ID number.", _transform);
				}
				else
				{
					ACDebug.LogWarning("GameObject " + _transform.gameObject.name + " was not saved because it does not have a 'Constant ID' script - please exit Play mode and attach one to it.", _transform);
				}
			}
			return 0;
		}

		public static string SerializeObject<T>(object dataObject, bool addMethodName = false, iFileFormatHandler fileFormatHandler = null)
		{
			if (fileFormatHandler == null)
			{
				fileFormatHandler = SaveSystem.FileFormatHandler;
			}
			string text = SaveSystem.FileFormatHandler.SerializeObject<T>(dataObject);
			if (text != string.Empty && addMethodName)
			{
				text = fileFormatHandler.GetSaveMethod() + text;
			}
			return text;
		}

		public static T DeserializeObject<T>(string dataString, iFileFormatHandler fileFormatHandler = null)
		{
			if (fileFormatHandler == null)
			{
				fileFormatHandler = SaveSystem.FileFormatHandler;
			}
			if (string.IsNullOrEmpty(dataString))
			{
				return default(T);
			}
			if (dataString.Contains("<?xml") || dataString.Contains("xml version"))
			{
				fileFormatHandler = new FileFormatHandler_Xml();
			}
			if (dataString.StartsWith(fileFormatHandler.GetSaveMethod()))
			{
				dataString = dataString.Remove(0, fileFormatHandler.GetSaveMethod().ToCharArray().Length);
			}
			T val = fileFormatHandler.DeserializeObject<T>(dataString);
			if (val != null && val is T)
			{
				return val;
			}
			return default(T);
		}

		public static Paths RestorePathData(Paths path, string pathData)
		{
			if (pathData == null)
			{
				return null;
			}
			path.affectY = true;
			path.pathType = AC_PathType.ForwardOnly;
			path.nodePause = 0f;
			path.nodes = new List<Vector3>();
			if (pathData.Length > 0)
			{
				string[] array = pathData.Split("|"[0]);
				string[] array2 = array;
				foreach (string text in array2)
				{
					string[] array3 = text.Split(":"[0]);
					float result = 0f;
					float.TryParse(array3[0], out result);
					float result2 = 0f;
					float.TryParse(array3[1], out result2);
					float result3 = 0f;
					float.TryParse(array3[2], out result3);
					path.nodes.Add(new Vector3(result, result2, result3));
				}
			}
			return path;
		}

		public static string CreatePathData(Paths path)
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (Vector3 node in path.nodes)
			{
				stringBuilder.Append(node.x.ToString());
				stringBuilder.Append(":");
				stringBuilder.Append(node.y.ToString());
				stringBuilder.Append(":");
				stringBuilder.Append(node.z.ToString());
				stringBuilder.Append("|");
			}
			if (path.nodes.Count > 0)
			{
				stringBuilder.Remove(stringBuilder.Length - 1, 1);
			}
			return stringBuilder.ToString();
		}

		public static string SaveScriptData<T>(object dataObject) where T : RememberData
		{
			return SerializeObject<T>(dataObject);
		}

		public static T LoadScriptData<T>(string dataString) where T : RememberData
		{
			iFileFormatHandler fileFormatHandler = SaveSystem.FileFormatHandler;
			if (dataString.StartsWith(fileFormatHandler.GetSaveMethod()))
			{
				dataString = dataString.Remove(0, fileFormatHandler.GetSaveMethod().ToCharArray().Length);
			}
			return fileFormatHandler.LoadScriptData<T>(dataString);
		}

		public static OptionsData DeserializeOptionsData(string dataString)
		{
			iFileFormatHandler optionsFileFormatHandler = SaveSystem.OptionsFileFormatHandler;
			if (dataString.StartsWith(optionsFileFormatHandler.GetSaveMethod()))
			{
				dataString = dataString.Remove(0, optionsFileFormatHandler.GetSaveMethod().ToCharArray().Length);
			}
			else if (dataString.StartsWith("XML") || dataString.StartsWith("Json") || dataString.StartsWith("Binary"))
			{
				return new OptionsData();
			}
			return DeserializeObject<OptionsData>(dataString);
		}
	}
}
