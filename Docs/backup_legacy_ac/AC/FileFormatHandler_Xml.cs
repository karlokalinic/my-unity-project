using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace AC
{
	public class FileFormatHandler_Xml : iFileFormatHandler
	{
		public string GetSaveMethod()
		{
			return "XML";
		}

		public string GetSaveExtension()
		{
			return ".savx";
		}

		public string SerializeObject<T>(object dataObject)
		{
			string text = null;
			MemoryStream stream = new MemoryStream();
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
			XmlTextWriter xmlTextWriter = new XmlTextWriter(stream, Encoding.UTF8);
			xmlSerializer.Serialize(xmlTextWriter, dataObject);
			stream = (MemoryStream)xmlTextWriter.BaseStream;
			return UTF8ByteArrayToString(stream.ToArray());
		}

		public T DeserializeObject<T>(string dataString)
		{
			if (!dataString.Contains("<?xml") && !dataString.Contains("xml version"))
			{
				return default(T);
			}
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
			MemoryStream stream = new MemoryStream(StringToUTF8ByteArray(dataString));
			try
			{
				string text = typeof(T).ToString();
				if (text.StartsWith("AC."))
				{
					text = text.Substring(3);
				}
				else if (text.Contains("[AC."))
				{
					int startIndex = text.IndexOf("[AC.") + 4;
					int num = text.Substring(startIndex).IndexOf("]");
					if (num > 1)
					{
						text = text.Substring(startIndex, num);
					}
				}
				if (dataString.Contains("</" + text + ">"))
				{
					object obj = xmlSerializer.Deserialize(stream);
					if (obj is T)
					{
						return (T)obj;
					}
				}
			}
			catch (Exception ex)
			{
				ACDebug.LogWarning("Could not XML deserialize datastring '" + dataString + "; Exception: " + ex);
			}
			return default(T);
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
			return DeserializeObject<T>(dataString);
		}

		protected string UTF8ByteArrayToString(byte[] characters)
		{
			UTF8Encoding uTF8Encoding = new UTF8Encoding();
			return uTF8Encoding.GetString(characters, 0, characters.Length);
		}

		protected byte[] StringToUTF8ByteArray(string pXmlString)
		{
			UTF8Encoding uTF8Encoding = new UTF8Encoding();
			return uTF8Encoding.GetBytes(pXmlString);
		}
	}
}
