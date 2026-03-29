using System.Collections.Generic;

namespace AC
{
	public interface iFileFormatHandler
	{
		string GetSaveMethod();

		string GetSaveExtension();

		string SerializeObject<T>(object dataObject);

		T DeserializeObject<T>(string dataString);

		string SerializeAllRoomData(List<SingleLevelData> dataObjects);

		List<SingleLevelData> DeserializeAllRoomData(string dataString);

		T LoadScriptData<T>(string dataString) where T : RememberData;
	}
}
