using System;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionParameter
	{
		public string label = string.Empty;

		public int ID;

		public ParameterType parameterType;

		public int intValue = -1;

		public float floatValue;

		public string stringValue = string.Empty;

		public GameObject gameObject;

		public UnityEngine.Object objectValue;

		public Vector3 vector3Value;

		public GameObjectParameterReferences gameObjectParameterReferences;

		public Variables variables;

		public ActionParameter(int[] idArray)
		{
			label = string.Empty;
			ID = 0;
			intValue = -1;
			floatValue = 0f;
			stringValue = string.Empty;
			gameObject = null;
			objectValue = null;
			parameterType = ParameterType.GameObject;
			vector3Value = Vector3.zero;
			gameObjectParameterReferences = GameObjectParameterReferences.ReferencePrefab;
			variables = null;
			foreach (int num in idArray)
			{
				if (ID == num)
				{
					ID++;
				}
			}
			label = "Parameter " + (ID + 1);
		}

		public ActionParameter(int id)
		{
			label = string.Empty;
			ID = id;
			intValue = -1;
			floatValue = 0f;
			stringValue = string.Empty;
			gameObject = null;
			objectValue = null;
			parameterType = ParameterType.GameObject;
			vector3Value = Vector3.zero;
			gameObjectParameterReferences = GameObjectParameterReferences.ReferencePrefab;
			variables = null;
			label = "Parameter " + (ID + 1);
		}

		public ActionParameter(ActionParameter _actionParameter, bool alsoCopyValues = false)
		{
			label = _actionParameter.label;
			ID = _actionParameter.ID;
			parameterType = _actionParameter.parameterType;
			if (alsoCopyValues)
			{
				intValue = _actionParameter.intValue;
				floatValue = _actionParameter.floatValue;
				stringValue = _actionParameter.stringValue;
				gameObject = _actionParameter.gameObject;
				objectValue = _actionParameter.objectValue;
				vector3Value = _actionParameter.vector3Value;
				gameObjectParameterReferences = _actionParameter.gameObjectParameterReferences;
				variables = _actionParameter.variables;
			}
			else
			{
				intValue = -1;
				floatValue = 0f;
				stringValue = string.Empty;
				gameObject = null;
				objectValue = null;
				vector3Value = Vector3.zero;
				gameObjectParameterReferences = GameObjectParameterReferences.ReferencePrefab;
				variables = null;
			}
		}

		public void CopyValues(ActionParameter otherParameter)
		{
			intValue = otherParameter.intValue;
			floatValue = otherParameter.floatValue;
			stringValue = otherParameter.stringValue;
			gameObject = otherParameter.gameObject;
			objectValue = otherParameter.objectValue;
			vector3Value = otherParameter.vector3Value;
			gameObjectParameterReferences = otherParameter.gameObjectParameterReferences;
			variables = otherParameter.variables;
		}

		public void Reset()
		{
			intValue = -1;
			floatValue = 0f;
			stringValue = string.Empty;
			gameObject = null;
			objectValue = null;
			vector3Value = Vector3.zero;
			gameObjectParameterReferences = GameObjectParameterReferences.ReferencePrefab;
			variables = null;
		}

		public bool IsIntegerBased()
		{
			if (parameterType == ParameterType.GameObject || parameterType == ParameterType.GlobalVariable || parameterType == ParameterType.Integer || parameterType == ParameterType.Boolean || parameterType == ParameterType.InventoryItem || parameterType == ParameterType.Document || parameterType == ParameterType.LocalVariable || parameterType == ParameterType.ComponentVariable)
			{
				return true;
			}
			return false;
		}

		public void SetValue(int _value)
		{
			intValue = _value;
			floatValue = 0f;
			stringValue = string.Empty;
			gameObject = null;
			objectValue = null;
			vector3Value = Vector3.zero;
			variables = null;
		}

		public void SetValue(float _value)
		{
			floatValue = _value;
			stringValue = string.Empty;
			intValue = -1;
			gameObject = null;
			objectValue = null;
			vector3Value = Vector3.zero;
			variables = null;
		}

		public void SetValue(string _value)
		{
			stringValue = AdvGame.ConvertTokens(_value);
			floatValue = 0f;
			intValue = -1;
			gameObject = null;
			objectValue = null;
			vector3Value = Vector3.zero;
			variables = null;
		}

		public void SetValue(Vector3 _value)
		{
			stringValue = string.Empty;
			floatValue = 0f;
			intValue = -1;
			gameObject = null;
			objectValue = null;
			vector3Value = _value;
			variables = null;
		}

		public void SetValue(GameObject _object)
		{
			gameObject = _object;
			floatValue = 0f;
			stringValue = string.Empty;
			intValue = -1;
			objectValue = null;
			vector3Value = Vector3.zero;
			variables = null;
		}

		public void SetValue(UnityEngine.Object _object)
		{
			gameObject = null;
			floatValue = 0f;
			stringValue = string.Empty;
			intValue = -1;
			objectValue = _object;
			vector3Value = Vector3.zero;
			variables = null;
		}

		public void SetValue(GameObject _object, int _value)
		{
			gameObject = _object;
			floatValue = 0f;
			stringValue = string.Empty;
			intValue = _value;
			objectValue = null;
			vector3Value = Vector3.zero;
			variables = null;
		}

		public void SetValue(Variables _variables, int _value)
		{
			gameObject = null;
			floatValue = 0f;
			stringValue = string.Empty;
			intValue = _value;
			objectValue = null;
			vector3Value = Vector3.zero;
			variables = _variables;
		}

		public GVar GetVariable()
		{
			switch (parameterType)
			{
			case ParameterType.GlobalVariable:
				return GlobalVariables.GetVariable(intValue, true);
			case ParameterType.LocalVariable:
				return LocalVariables.GetVariable(intValue);
			case ParameterType.ComponentVariable:
				if (variables != null)
				{
					return variables.GetVariable(intValue);
				}
				break;
			}
			return null;
		}

		public string GetLabel()
		{
			switch (parameterType)
			{
			case ParameterType.GameObject:
				if (gameObject != null)
				{
					Hotspot component = gameObject.GetComponent<Hotspot>();
					if ((bool)component)
					{
						return component.GetName(Options.GetLanguage());
					}
					Char component2 = gameObject.GetComponent<Char>();
					if ((bool)component2)
					{
						return component2.GetName(Options.GetLanguage());
					}
					return gameObject.name;
				}
				return string.Empty;
			case ParameterType.InventoryItem:
			{
				InvItem item = KickStarter.inventoryManager.GetItem(intValue);
				if (item != null)
				{
					return item.GetLabel(Options.GetLanguage());
				}
				return GetSaveData();
			}
			case ParameterType.Document:
			{
				Document document = KickStarter.inventoryManager.GetDocument(intValue);
				if (document != null)
				{
					return KickStarter.runtimeLanguages.GetTranslation(document.title, document.titleLineID, Options.GetLanguage());
				}
				return GetSaveData();
			}
			case ParameterType.GlobalVariable:
			{
				GVar variable2 = GetVariable();
				if (variable2 != null)
				{
					return variable2.label;
				}
				return GetSaveData();
			}
			case ParameterType.LocalVariable:
			{
				GVar variable3 = GetVariable();
				if (variable3 != null)
				{
					return variable3.label;
				}
				return GetSaveData();
			}
			case ParameterType.ComponentVariable:
			{
				GVar variable = GetVariable();
				if (variable != null)
				{
					return variable.label;
				}
				return GetSaveData();
			}
			default:
				return GetSaveData();
			}
		}

		public string GetSaveData()
		{
			switch (parameterType)
			{
			case ParameterType.Float:
				return floatValue.ToString();
			case ParameterType.String:
				return AdvGame.PrepareStringForSaving(stringValue);
			case ParameterType.GameObject:
				if (gameObject != null)
				{
					if ((bool)gameObject.GetComponent<ConstantID>())
					{
						return gameObject.GetComponent<ConstantID>().constantID.ToString();
					}
					ACDebug.LogWarning("Could not save parameter data for '" + gameObject.name + "' as it has no Constant ID number.", gameObject);
				}
				return string.Empty;
			case ParameterType.UnityObject:
				if (objectValue != null)
				{
					return objectValue.name;
				}
				return string.Empty;
			case ParameterType.Vector3:
			{
				string text = vector3Value.x + "," + vector3Value.y + "," + vector3Value.z;
				return AdvGame.PrepareStringForSaving(text);
			}
			default:
				return intValue.ToString();
			}
		}

		public void LoadData(string dataString)
		{
			switch (parameterType)
			{
			case ParameterType.Float:
				floatValue = 0f;
				float.TryParse(dataString, out floatValue);
				break;
			case ParameterType.String:
				stringValue = AdvGame.PrepareStringForLoading(dataString);
				break;
			case ParameterType.GameObject:
			{
				gameObject = null;
				int result4 = 0;
				if (int.TryParse(dataString, out result4))
				{
					ConstantID constantID = Serializer.returnComponent<ConstantID>(result4);
					if (constantID != null)
					{
						gameObject = constantID.gameObject;
					}
				}
				break;
			}
			case ParameterType.UnityObject:
			{
				if (string.IsNullOrEmpty(dataString))
				{
					objectValue = null;
					break;
				}
				UnityEngine.Object[] array2 = Resources.LoadAll(string.Empty);
				UnityEngine.Object[] array3 = array2;
				foreach (UnityEngine.Object obj in array3)
				{
					if (obj.name == dataString)
					{
						objectValue = obj;
						break;
					}
				}
				break;
			}
			case ParameterType.Vector3:
				if (!string.IsNullOrEmpty(dataString))
				{
					dataString = AdvGame.PrepareStringForLoading(dataString);
					Vector3 vector = Vector3.zero;
					string[] array = dataString.Split(","[0]);
					if (array != null && array.Length == 3)
					{
						float result = 0f;
						float.TryParse(array[0], out result);
						float result2 = 0f;
						float.TryParse(array[1], out result2);
						float result3 = 0f;
						float.TryParse(array[2], out result3);
						vector = new Vector3(result, result2, result3);
					}
					vector3Value = vector;
				}
				break;
			default:
				intValue = 0;
				int.TryParse(dataString, out intValue);
				break;
			}
		}
	}
}
