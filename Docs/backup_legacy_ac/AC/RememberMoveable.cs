using UnityEngine;

namespace AC
{
	[AddComponentMenu("Adventure Creator/Save system/Remember Moveable")]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_remember_moveable.html")]
	public class RememberMoveable : Remember
	{
		public AC_OnOff startState;

		private bool loadedData;

		private void Awake()
		{
			if (loadedData || !KickStarter.settingsManager || !GameIsPlaying())
			{
				return;
			}
			if ((bool)GetComponent<DragBase>())
			{
				if (startState == AC_OnOff.On)
				{
					GetComponent<DragBase>().TurnOn();
				}
				else
				{
					GetComponent<DragBase>().TurnOff();
				}
			}
			if (startState == AC_OnOff.On)
			{
				base.gameObject.layer = LayerMask.NameToLayer(KickStarter.settingsManager.hotspotLayer);
			}
			else
			{
				base.gameObject.layer = LayerMask.NameToLayer(KickStarter.settingsManager.deactivatedLayer);
			}
		}

		public override string SaveData()
		{
			MoveableData moveableData = new MoveableData();
			moveableData.objectID = constantID;
			moveableData.savePrevented = savePrevented;
			if (base.gameObject.layer == LayerMask.NameToLayer(KickStarter.settingsManager.hotspotLayer))
			{
				moveableData.isOn = true;
			}
			else
			{
				moveableData.isOn = false;
			}
			if ((bool)GetComponent<Moveable_Drag>())
			{
				Moveable_Drag component = GetComponent<Moveable_Drag>();
				moveableData.trackValue = component.trackValue;
				moveableData.revolutions = component.revolutions;
			}
			moveableData.LocX = base.transform.position.x;
			moveableData.LocY = base.transform.position.y;
			moveableData.LocZ = base.transform.position.z;
			moveableData.RotX = base.transform.eulerAngles.x;
			moveableData.RotY = base.transform.eulerAngles.y;
			moveableData.RotZ = base.transform.eulerAngles.z;
			moveableData.ScaleX = base.transform.localScale.x;
			moveableData.ScaleY = base.transform.localScale.y;
			moveableData.ScaleZ = base.transform.localScale.z;
			if ((bool)GetComponent<Moveable>())
			{
				moveableData = GetComponent<Moveable>().SaveData(moveableData);
			}
			return Serializer.SaveScriptData<MoveableData>(moveableData);
		}

		public override void LoadData(string stringData)
		{
			MoveableData moveableData = Serializer.LoadScriptData<MoveableData>(stringData);
			if (moveableData == null)
			{
				loadedData = false;
				return;
			}
			base.SavePrevented = moveableData.savePrevented;
			if (savePrevented)
			{
				return;
			}
			if ((bool)GetComponent<DragBase>())
			{
				if (moveableData.isOn)
				{
					GetComponent<DragBase>().TurnOn();
				}
				else
				{
					GetComponent<DragBase>().TurnOff();
				}
			}
			if (moveableData.isOn)
			{
				base.gameObject.layer = LayerMask.NameToLayer(KickStarter.settingsManager.hotspotLayer);
			}
			else
			{
				base.gameObject.layer = LayerMask.NameToLayer(KickStarter.settingsManager.deactivatedLayer);
			}
			base.transform.position = new Vector3(moveableData.LocX, moveableData.LocY, moveableData.LocZ);
			base.transform.eulerAngles = new Vector3(moveableData.RotX, moveableData.RotY, moveableData.RotZ);
			base.transform.localScale = new Vector3(moveableData.ScaleX, moveableData.ScaleY, moveableData.ScaleZ);
			if ((bool)GetComponent<Moveable_Drag>())
			{
				Moveable_Drag component = GetComponent<Moveable_Drag>();
				component.LetGo(true);
				if (component.dragMode == DragMode.LockToTrack && component.track != null)
				{
					component.trackValue = moveableData.trackValue;
					component.revolutions = moveableData.revolutions;
					component.StopAutoMove();
					component.track.SetPositionAlong(moveableData.trackValue, component);
				}
			}
			if ((bool)GetComponent<Moveable>())
			{
				GetComponent<Moveable>().LoadData(moveableData);
			}
			loadedData = true;
		}
	}
}
