using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace AC
{
	public class CSVReader
	{
		private const string legacy_csvDelimiter = "|";

		private const string legacy_csvComma = ",";

		private const string legacy_csvTemp = "{{{$$}}}";

		private const string textSeparator = "\"";

		private const string fieldSeparator = ",";

		public static string[,] SplitCsvGrid(string csvText)
		{
			switch (ACEditorPrefs.CSVFormat)
			{
			case CSVFormat.Legacy:
			{
				csvText = csvText.Replace(",", "{{{$$}}}");
				csvText = csvText.Replace("|", ",");
				csvText = csvText.Replace("\r\n", "\n");
				csvText = csvText.Replace("\r", "\n");
				string[] separator2 = new string[1] { "\n" };
				string[] array4 = csvText.Split(separator2, StringSplitOptions.None);
				int num3 = 0;
				for (int m = 0; m < array4.Length; m++)
				{
					string[] array5 = array4[m].Split(","[0]);
					num3 = Mathf.Max(num3, array5.Length);
				}
				string[,] array6 = new string[num3 + 1, array4.Length + 1];
				for (int n = 0; n < array4.Length; n++)
				{
					string[] array7 = array4[n].Split(","[0]);
					for (int num4 = 0; num4 < array7.Length; num4++)
					{
						array6[num4, n] = array7[num4].Replace("{{{$$}}}", ",");
					}
				}
				return array6;
			}
			case CSVFormat.Standard:
			{
				csvText = csvText.Replace("\r\n", "\n");
				csvText = csvText.Replace("\r", "\n");
				string[] separator = new string[1] { "\n" };
				string[] array = csvText.Split(separator, StringSplitOptions.None);
				List<string[]> list = new List<string[]>();
				int num = array.Length;
				for (int i = 0; i < num; i++)
				{
					if (string.IsNullOrEmpty(array[i]))
					{
						continue;
					}
					Regex regex = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
					string[] array2 = regex.Split(array[i]);
					for (int j = 0; j < array2.Length; j++)
					{
						string text = array2[j];
						if (text.StartsWith("\"") && text.EndsWith("\""))
						{
							text = text.Substring(1, text.Length - 2);
							if (text.Contains("\"\""))
							{
								text = text.Replace("\"\"", "\"");
							}
						}
						array2[j] = text;
					}
					list.Add(array2);
				}
				num = list.Count + 1;
				int num2 = list[0].Length + 1;
				string[,] array3 = new string[num2, num];
				for (int k = 0; k < num - 1; k++)
				{
					for (int l = 0; l < num2 - 1; l++)
					{
						array3[l, k] = list[k][l];
					}
				}
				return array3;
			}
			default:
				return null;
			}
		}

		public static string CreateCSVGrid(List<string[]> contents)
		{
			StringBuilder stringBuilder = new StringBuilder();
			int count = contents.Count;
			for (int i = 0; i < count; i++)
			{
				switch (ACEditorPrefs.CSVFormat)
				{
				case CSVFormat.Legacy:
				{
					bool flag2 = false;
					int num2 = contents[i].Length;
					for (int k = 0; k < num2; k++)
					{
						string text2 = contents[i][k];
						if (text2.Contains("|"))
						{
							ACDebug.LogWarning("Skipping CSV export of text '" + text2 + "' on row #" + i + " because it contains the character '|'.");
							flag2 = true;
						}
					}
					if (!flag2)
					{
						stringBuilder.AppendLine(string.Join("|", contents[i]));
					}
					break;
				}
				case CSVFormat.Standard:
				{
					int num = contents[i].Length;
					for (int j = 0; j < num; j++)
					{
						string text = contents[i][j];
						bool flag = text.Contains(",") || text.Contains("\"");
						if (text.Contains("\""))
						{
							text = text.Replace("\"", "\"\"");
						}
						if (flag)
						{
							text = "\"" + text + "\"";
						}
						contents[i][j] = text;
					}
					stringBuilder.AppendLine(string.Join(",", contents[i]));
					break;
				}
				}
			}
			return stringBuilder.ToString();
		}
	}
}
