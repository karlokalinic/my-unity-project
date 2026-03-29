using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	public abstract class SetParametersBase : MonoBehaviour
	{
		[Serializable]
		public struct GUIData
		{
			public List<ActionParameter> fromParameters;

			public List<int> parameterIDs;

			public GUIData(List<ActionParameter> _fromParameters, List<int> _parameterIDs)
			{
				fromParameters = new List<ActionParameter>();
				if (_fromParameters != null)
				{
					foreach (ActionParameter _fromParameter in _fromParameters)
					{
						fromParameters.Add(new ActionParameter(_fromParameter, true));
					}
				}
				parameterIDs = new List<int>();
				if (_parameterIDs == null)
				{
					return;
				}
				foreach (int _parameterID in _parameterIDs)
				{
					parameterIDs.Add(_parameterID);
				}
			}

			public GUIData(GUIData guiData)
			{
				fromParameters = new List<ActionParameter>();
				if (guiData.fromParameters != null)
				{
					foreach (ActionParameter fromParameter in guiData.fromParameters)
					{
						fromParameters.Add(new ActionParameter(fromParameter, true));
					}
				}
				parameterIDs = new List<int>();
				if (guiData.parameterIDs == null)
				{
					return;
				}
				foreach (int parameterID in guiData.parameterIDs)
				{
					parameterIDs.Add(parameterID);
				}
			}
		}

		[SerializeField]
		protected GUIData initialGUIData = new GUIData(new List<ActionParameter>(), new List<int>());

		[SerializeField]
		protected GUIData[] successiveGUIData = new GUIData[0];

		public virtual List<ActionListAsset> GetReferencedActionListAssets()
		{
			List<ActionListAsset> existingList = new List<ActionListAsset>();
			existingList = GetAssetsFromParameterGUIData(initialGUIData, existingList);
			if (successiveGUIData != null)
			{
				GUIData[] array = successiveGUIData;
				foreach (GUIData guiData in array)
				{
					existingList = GetAssetsFromParameterGUIData(guiData, existingList);
				}
			}
			return existingList;
		}

		protected void AssignParameterValues(ActionList _actionList, int runIndex = 0)
		{
			if (_actionList != null && _actionList.source == ActionListSource.InScene && _actionList.useParameters && _actionList.parameters != null)
			{
				BulkAssignParameterValues(_actionList.parameters, GetFromParameters(runIndex), false, false);
			}
			else if (_actionList != null && _actionList.source == ActionListSource.AssetFile && _actionList.assetFile != null && _actionList.assetFile.NumParameters > 0)
			{
				if (_actionList.syncParamValues)
				{
					BulkAssignParameterValues(_actionList.assetFile.GetParameters(), GetFromParameters(runIndex), false, true);
				}
				else
				{
					BulkAssignParameterValues(_actionList.parameters, GetFromParameters(runIndex), true, false);
				}
			}
		}

		protected void AssignParameterValues(ActionListAsset _actionListAsset, int runIndex = 0)
		{
			if (_actionListAsset != null && _actionListAsset.NumParameters > 0)
			{
				BulkAssignParameterValues(_actionListAsset.GetParameters(), GetFromParameters(runIndex), false, true);
			}
		}

		protected List<ActionParameter> GetFromParameters(int index)
		{
			if (index <= 0)
			{
				return initialGUIData.fromParameters;
			}
			return successiveGUIData[index - 1].fromParameters;
		}

		protected List<ActionListAsset> GetAssetsFromParameterGUIData(GUIData guiData, List<ActionListAsset> existingList)
		{
			if (guiData.fromParameters != null)
			{
				foreach (ActionParameter fromParameter in guiData.fromParameters)
				{
					if (fromParameter.parameterType == ParameterType.UnityObject && fromParameter.objectValue != null && fromParameter.objectValue is ActionListAsset)
					{
						ActionListAsset item = (ActionListAsset)fromParameter.objectValue;
						existingList.Add(item);
					}
				}
			}
			return existingList;
		}

		public static void BulkAssignParameterValues(List<ActionParameter> externalParameters, List<ActionParameter> fromParameters, bool sendingToAsset, bool _isAssetFile)
		{
			for (int i = 0; i < externalParameters.Count; i++)
			{
				if (fromParameters.Count <= i)
				{
					continue;
				}
				if (externalParameters[i].parameterType == ParameterType.String)
				{
					externalParameters[i].SetValue(fromParameters[i].stringValue);
				}
				else if (externalParameters[i].parameterType == ParameterType.Float)
				{
					externalParameters[i].SetValue(fromParameters[i].floatValue);
				}
				else if (externalParameters[i].parameterType == ParameterType.UnityObject)
				{
					externalParameters[i].SetValue(fromParameters[i].objectValue);
				}
				else if (externalParameters[i].parameterType == ParameterType.Vector3)
				{
					externalParameters[i].SetValue(fromParameters[i].vector3Value);
				}
				else if (externalParameters[i].parameterType == ParameterType.ComponentVariable)
				{
					externalParameters[i].SetValue(fromParameters[i].variables, fromParameters[i].intValue);
				}
				else if (externalParameters[i].parameterType != ParameterType.GameObject)
				{
					externalParameters[i].SetValue(fromParameters[i].intValue);
				}
				else if (sendingToAsset)
				{
					if (_isAssetFile)
					{
						if (fromParameters[i].gameObject != null)
						{
							if (fromParameters[i].gameObjectParameterReferences == GameObjectParameterReferences.ReferencePrefab)
							{
								externalParameters[i].SetValue(fromParameters[i].gameObject);
							}
							else if (fromParameters[i].gameObjectParameterReferences == GameObjectParameterReferences.ReferenceSceneInstance)
							{
								int value = 0;
								if ((bool)fromParameters[i].gameObject && (bool)fromParameters[i].gameObject.GetComponent<ConstantID>())
								{
									value = fromParameters[i].gameObject.GetComponent<ConstantID>().constantID;
								}
								else
								{
									ACDebug.LogWarning(fromParameters[i].gameObject.name + " requires a ConstantID script component!", fromParameters[i].gameObject);
								}
								externalParameters[i].SetValue(fromParameters[i].gameObject, value);
							}
						}
						else
						{
							externalParameters[i].SetValue(fromParameters[i].intValue);
						}
					}
					else if (fromParameters[i].gameObject != null)
					{
						int value2 = 0;
						if ((bool)fromParameters[i].gameObject && (bool)fromParameters[i].gameObject.GetComponent<ConstantID>())
						{
							value2 = fromParameters[i].gameObject.GetComponent<ConstantID>().constantID;
						}
						else
						{
							ACDebug.LogWarning(fromParameters[i].gameObject.name + " requires a ConstantID script component!", fromParameters[i].gameObject);
						}
						externalParameters[i].SetValue(fromParameters[i].gameObject, value2);
					}
					else
					{
						externalParameters[i].SetValue(fromParameters[i].intValue);
					}
				}
				else if (fromParameters[i].gameObject != null)
				{
					externalParameters[i].SetValue(fromParameters[i].gameObject);
				}
				else
				{
					externalParameters[i].SetValue(fromParameters[i].intValue);
				}
			}
		}

		public static GUIData SyncLists(List<ActionParameter> externalParameters, GUIData guiData)
		{
			List<ActionParameter> list = new List<ActionParameter>();
			List<int> list2 = new List<int>();
			foreach (ActionParameter externalParameter in externalParameters)
			{
				bool flag = false;
				for (int i = 0; i < guiData.fromParameters.Count; i++)
				{
					if (!flag && guiData.fromParameters[i].ID == externalParameter.ID)
					{
						list.Add(new ActionParameter(guiData.fromParameters[i], true));
						guiData.fromParameters.RemoveAt(i);
						if (guiData.parameterIDs != null && i < guiData.parameterIDs.Count)
						{
							list2.Add(guiData.parameterIDs[i]);
							guiData.parameterIDs.RemoveAt(i);
						}
						else
						{
							list2.Add(-1);
						}
						flag = true;
					}
				}
				if (!flag)
				{
					list.Add(new ActionParameter(externalParameter, true));
					list2.Add(-1);
				}
			}
			return new GUIData(list, list2);
		}
	}
}
