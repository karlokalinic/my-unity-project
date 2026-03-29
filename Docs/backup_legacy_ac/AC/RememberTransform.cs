using UnityEngine;

namespace AC
{
	[AddComponentMenu("Adventure Creator/Save system/Remember Transform")]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_remember_transform.html")]
	public class RememberTransform : ConstantID
	{
		public bool saveParent;

		public bool saveScenePresence;

		public int linkedPrefabID;

		public GlobalLocal transformSpace;

		private bool savePrevented;

		public bool SavePrevented
		{
			get
			{
				return savePrevented;
			}
			set
			{
				savePrevented = value;
			}
		}

		public void OnSpawn()
		{
			if (linkedPrefabID != 0)
			{
				int instanceID = GetInstanceID();
				ConstantID[] components = GetComponents<ConstantID>();
				ConstantID[] array = components;
				foreach (ConstantID constantID in array)
				{
					constantID.constantID = instanceID;
				}
				ACDebug.Log("Spawned new instance of " + base.gameObject.name + ", given new ID: " + instanceID, this);
			}
		}

		public TransformData SaveTransformData()
		{
			TransformData transformData = new TransformData();
			transformData.objectID = constantID;
			transformData.savePrevented = savePrevented;
			switch (transformSpace)
			{
			case GlobalLocal.Global:
			{
				transformData.LocX = base.transform.position.x;
				transformData.LocY = base.transform.position.y;
				transformData.LocZ = base.transform.position.z;
				Char component = base.transform.GetComponent<Char>();
				if (component != null)
				{
					transformData.RotX = component.TransformRotation.eulerAngles.x;
					transformData.RotY = component.TransformRotation.eulerAngles.y;
					transformData.RotZ = component.TransformRotation.eulerAngles.z;
				}
				else
				{
					transformData.RotX = base.transform.eulerAngles.x;
					transformData.RotY = base.transform.eulerAngles.y;
					transformData.RotZ = base.transform.eulerAngles.z;
				}
				break;
			}
			case GlobalLocal.Local:
				transformData.LocX = base.transform.localPosition.x;
				transformData.LocY = base.transform.localPosition.y;
				transformData.LocZ = base.transform.localPosition.z;
				transformData.RotX = base.transform.localEulerAngles.x;
				transformData.RotY = base.transform.localEulerAngles.y;
				transformData.RotZ = base.transform.localEulerAngles.z;
				break;
			}
			transformData.ScaleX = base.transform.localScale.x;
			transformData.ScaleY = base.transform.localScale.y;
			transformData.ScaleZ = base.transform.localScale.z;
			transformData.bringBack = saveScenePresence;
			transformData.linkedPrefabID = (saveScenePresence ? linkedPrefabID : 0);
			if (saveParent)
			{
				Transform parent = base.transform.parent;
				if (parent == null)
				{
					transformData.parentID = 0;
					return transformData;
				}
				while (parent.parent != null)
				{
					parent = parent.parent;
					Char component2 = parent.GetComponent<Char>();
					if (component2 != null)
					{
						if ((component2 is Player || ((bool)component2.GetComponent<ConstantID>() && component2.GetComponent<ConstantID>().constantID != 0)) && (base.transform.parent == component2.leftHandBone || base.transform.parent == component2.rightHandBone))
						{
							if (component2.IsPlayer)
							{
								transformData.parentIsPlayer = true;
								transformData.parentIsNPC = false;
								transformData.parentID = 0;
							}
							else
							{
								transformData.parentIsPlayer = false;
								transformData.parentIsNPC = true;
								transformData.parentID = component2.GetComponent<ConstantID>().constantID;
							}
							if (base.transform.parent == component2.leftHandBone)
							{
								transformData.heldHand = Hand.Left;
							}
							else
							{
								transformData.heldHand = Hand.Right;
							}
							return transformData;
						}
						break;
					}
				}
				if ((bool)base.transform.parent.GetComponent<ConstantID>() && base.transform.parent.GetComponent<ConstantID>().constantID != 0)
				{
					transformData.parentID = base.transform.parent.GetComponent<ConstantID>().constantID;
				}
				else
				{
					transformData.parentID = 0;
					ACDebug.LogWarning("Could not save " + base.name + "'s parent since it has no Constant ID", this);
				}
			}
			return transformData;
		}

		public void LoadTransformData(TransformData data)
		{
			if (data == null)
			{
				return;
			}
			savePrevented = data.savePrevented;
			if (savePrevented)
			{
				return;
			}
			if (data.parentIsPlayer)
			{
				if ((bool)KickStarter.player)
				{
					if (data.heldHand == Hand.Left)
					{
						base.transform.parent = KickStarter.player.leftHandBone;
					}
					else
					{
						base.transform.parent = KickStarter.player.rightHandBone;
					}
				}
			}
			else if (data.parentID != 0)
			{
				ConstantID constantID = Serializer.returnComponent<ConstantID>(data.parentID);
				if (constantID != null)
				{
					if (data.parentIsNPC && (bool)constantID.GetComponent<NPC>())
					{
						if (data.heldHand == Hand.Left)
						{
							base.transform.parent = constantID.GetComponent<NPC>().leftHandBone;
						}
						else
						{
							base.transform.parent = constantID.GetComponent<NPC>().rightHandBone;
						}
					}
					else
					{
						base.transform.parent = constantID.gameObject.transform;
					}
				}
			}
			else if (data.parentID == 0 && saveParent)
			{
				base.transform.parent = null;
			}
			switch (transformSpace)
			{
			case GlobalLocal.Global:
				base.transform.position = new Vector3(data.LocX, data.LocY, data.LocZ);
				base.transform.eulerAngles = new Vector3(data.RotX, data.RotY, data.RotZ);
				break;
			case GlobalLocal.Local:
				base.transform.localPosition = new Vector3(data.LocX, data.LocY, data.LocZ);
				base.transform.localEulerAngles = new Vector3(data.RotX, data.RotY, data.RotZ);
				break;
			}
			base.transform.localScale = new Vector3(data.ScaleX, data.ScaleY, data.ScaleZ);
		}
	}
}
