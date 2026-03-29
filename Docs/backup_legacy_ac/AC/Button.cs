using System;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class Button
	{
		public Interaction interaction;

		public ActionListAsset assetFile;

		public GameObject customScriptObject;

		public string customScriptFunction = string.Empty;

		public bool isDisabled;

		public int invID;

		public int iconID = -1;

		public SelectItemMode selectItemMode;

		public PlayerAction playerAction;

		public bool setProximity;

		public float proximity = 1f;

		public bool faceAfter;

		public bool isBlocking;

		public int parameterID = -1;

		public int invParameterID = -1;

		public bool IsButtonModified()
		{
			if (interaction != null || assetFile != null || customScriptObject != null || customScriptFunction != string.Empty || isDisabled || playerAction != PlayerAction.DoNothing || setProximity || !Mathf.Approximately(proximity, 1f) || faceAfter || isBlocking)
			{
				return true;
			}
			return false;
		}

		public void CopyButton(Button _button)
		{
			interaction = _button.interaction;
			assetFile = _button.assetFile;
			customScriptObject = _button.customScriptObject;
			customScriptFunction = _button.customScriptFunction;
			isDisabled = _button.isDisabled;
			invID = _button.invID;
			iconID = _button.iconID;
			playerAction = _button.playerAction;
			setProximity = _button.setProximity;
			proximity = _button.proximity;
			faceAfter = _button.faceAfter;
			isBlocking = _button.isBlocking;
			parameterID = _button.parameterID;
			invParameterID = _button.invParameterID;
		}

		public string GetFullLabel(Hotspot _hotspot, int _language)
		{
			if (_hotspot == null)
			{
				return string.Empty;
			}
			if (_hotspot.lookButton == this)
			{
				string labelFromID = KickStarter.cursorManager.GetLabelFromID(KickStarter.cursorManager.lookCursor_ID, _language);
				return AdvGame.CombineLanguageString(labelFromID, _hotspot.GetName(_language), _language);
			}
			if (_hotspot.useButtons.Contains(this))
			{
				string labelFromID2 = KickStarter.cursorManager.GetLabelFromID(iconID, _language);
				return AdvGame.CombineLanguageString(labelFromID2, _hotspot.GetName(_language), _language);
			}
			if (_hotspot.invButtons.Contains(this))
			{
				InvItem item = KickStarter.runtimeInventory.GetItem(invID);
				string hotspotPrefixLabel = KickStarter.runtimeInventory.GetHotspotPrefixLabel(item, item.GetLabel(_language), _language);
				return AdvGame.CombineLanguageString(hotspotPrefixLabel, _hotspot.GetName(_language), _language);
			}
			return string.Empty;
		}
	}
}
