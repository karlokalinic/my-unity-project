using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace AC
{
	public class FileFormatHandler_Binary : iFileFormatHandler
	{
		public string GetSaveMethod()
		{
			return "Binary";
		}

		public string GetSaveExtension()
		{
			return ".save";
		}

		public string SerializeObject<T>(object dataObject)
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			MemoryStream memoryStream = new MemoryStream();
			binaryFormatter.Serialize(memoryStream, dataObject);
			return Convert.ToBase64String(memoryStream.GetBuffer());
		}

		public T DeserializeObject<T>(string dataString)
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			MemoryStream serializationStream = new MemoryStream(Convert.FromBase64String(dataString));
			return (T)binaryFormatter.Deserialize(serializationStream);
		}

		public string SerializeAllRoomData(List<SingleLevelData> dataObjects)
		{
			return SerializeObject<List<SingleLevelData>>(dataObjects);
		}

		public List<SingleLevelData> DeserializeAllRoomData(string dataString)
		{
			return DeserializeObject<List<SingleLevelData>>(dataString);
		}

		public T LoadScriptData<T>(string dataString) where T : RememberData
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			MemoryStream serializationStream = new MemoryStream(Convert.FromBase64String(dataString));
			return binaryFormatter.Deserialize(serializationStream) as T;
		}
	}
}
