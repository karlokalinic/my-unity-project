using System;
using UnityEngine;

namespace AC
{
	[AddComponentMenu("Adventure Creator/3rd-party/Variable linking example")]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_variable_linking_example.html")]
	public class VariableLinkingExample : MonoBehaviour
	{
		public int myCustomInteger = 2;

		public int variableIDToSyncWith;

		private void OnEnable()
		{
			EventManager.OnDownloadVariable = (EventManager.Delegate_OnVariableUpload)Delegate.Combine(EventManager.OnDownloadVariable, new EventManager.Delegate_OnVariableUpload(OnDownload));
			EventManager.OnUploadVariable = (EventManager.Delegate_OnVariableUpload)Delegate.Combine(EventManager.OnUploadVariable, new EventManager.Delegate_OnVariableUpload(OnUpload));
		}

		private void OnDisable()
		{
			EventManager.OnDownloadVariable = (EventManager.Delegate_OnVariableUpload)Delegate.Remove(EventManager.OnDownloadVariable, new EventManager.Delegate_OnVariableUpload(OnDownload));
			EventManager.OnUploadVariable = (EventManager.Delegate_OnVariableUpload)Delegate.Remove(EventManager.OnUploadVariable, new EventManager.Delegate_OnVariableUpload(OnUpload));
		}

		private void OnDownload(GVar variable, Variables variables)
		{
			if (variable.id == variableIDToSyncWith)
			{
				variable.IntegerValue = myCustomInteger;
				Debug.Log("DOWNLOADED : " + myCustomInteger);
			}
		}

		private void OnUpload(GVar variable, Variables variables)
		{
			if (variable.id == variableIDToSyncWith)
			{
				myCustomInteger = variable.IntegerValue;
				Debug.Log("UPLOADED : " + myCustomInteger);
			}
		}
	}
}
