using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	public class FileFormatHandler_Json : iFileFormatHandler
	{
		protected const string roomDelimiter = "|ROOMDELIMITER|";

		public string GetSaveMethod()
		{
			return "Json";
		}

		public string GetSaveExtension()
		{
			return ".savj";
		}

		public string SerializeObject<T>(object dataObject)
		{
			return JsonUtility.ToJson(dataObject);
		}

		public T DeserializeObject<T>(string dataString)
		{
			return (T)DeserializeObjectJson<T>(dataString);
		}

		protected object DeserializeObjectJson<T>(string jsonString)
		{
			object obj = JsonUtility.FromJson(jsonString, typeof(T));
			if (obj is VideoPlayerData && !jsonString.Contains("isPlaying"))
			{
				return null;
			}
			if (obj is AnimatorData && !jsonString.Contains("layerWeightData"))
			{
				return null;
			}
			if (obj is ColliderData && !jsonString.Contains("isOn"))
			{
				return null;
			}
			if (obj is ContainerData && !jsonString.Contains("_linkedIDs"))
			{
				return null;
			}
			if (obj is ConversationData && !jsonString.Contains("_optionStates"))
			{
				return null;
			}
			if (obj is FootstepSoundData && !jsonString.Contains("walkSounds"))
			{
				return null;
			}
			if (obj is HotspotData && !jsonString.Contains("buttonStates"))
			{
				return null;
			}
			if (obj is MaterialData && !jsonString.Contains("_materialIDs"))
			{
				return null;
			}
			if (obj is MoveableData && !jsonString.Contains("trackValue"))
			{
				return null;
			}
			if (obj is NameData && !jsonString.Contains("newName"))
			{
				return null;
			}
			if (obj is NavMesh2DData && !jsonString.Contains("_linkedIDs"))
			{
				return null;
			}
			if (obj is NPCData && !jsonString.Contains("isHeadTurning"))
			{
				return null;
			}
			if (obj is ShapeableData && !jsonString.Contains("_activeKeyIDs"))
			{
				return null;
			}
			if (obj is SoundData && !jsonString.Contains("isPlaying"))
			{
				return null;
			}
			if (obj is TimelineData && !jsonString.Contains("timelineAssetID"))
			{
				return null;
			}
			if (obj is TransformData && !jsonString.Contains("bringBack"))
			{
				return null;
			}
			if (obj is TriggerData && !jsonString.Contains("isOn"))
			{
				return null;
			}
			if (obj is VisibilityData && !jsonString.Contains("useDefaultTintMap"))
			{
				return null;
			}
			if (obj is ParticleSystemData && !jsonString.Contains("currentTime"))
			{
				return null;
			}
			return obj;
		}

		public string SerializeAllRoomData(List<SingleLevelData> dataObjects)
		{
			string text = string.Empty;
			if (dataObjects != null && dataObjects.Count > 0)
			{
				for (int i = 0; i < dataObjects.Count; i++)
				{
					text += SerializeObject<SingleLevelData>(dataObjects[i]);
					if (i < dataObjects.Count - 1)
					{
						text += "|ROOMDELIMITER|";
					}
				}
			}
			return text;
		}

		public List<SingleLevelData> DeserializeAllRoomData(string dataString)
		{
			List<SingleLevelData> list = new List<SingleLevelData>();
			string[] separator = new string[1] { "|ROOMDELIMITER|" };
			string[] array = dataString.Split(separator, StringSplitOptions.None);
			string[] array2 = array;
			foreach (string dataString2 in array2)
			{
				SingleLevelData item = DeserializeObject<SingleLevelData>(dataString2);
				list.Add(item);
			}
			return list;
		}

		public T LoadScriptData<T>(string dataString) where T : RememberData
		{
			return DeserializeObject<T>(dataString);
		}
	}
}
