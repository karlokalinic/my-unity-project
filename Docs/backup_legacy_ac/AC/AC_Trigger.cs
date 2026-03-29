using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	[AddComponentMenu("Adventure Creator/Logic/Trigger")]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_a_c___trigger.html")]
	public class AC_Trigger : ActionList
	{
		public TriggerDetects detects;

		public GameObject obToDetect;

		public string detectComponent = string.Empty;

		public int triggerType;

		public bool cancelInteractions;

		public TriggerReacts triggerReacts;

		public TriggerDetectionMethod detectionMethod;

		public bool detectsPlayer = true;

		public List<GameObject> obsToDetect = new List<GameObject>();

		public int gameObjectParameterID = -1;

		protected Collider2D _collider2D;

		protected Collider _collider;

		protected bool[] lastFrameWithins;

		protected void OnEnable()
		{
			if ((bool)KickStarter.stateHandler)
			{
				KickStarter.stateHandler.Register(this);
			}
			_collider2D = GetComponent<Collider2D>();
			_collider = GetComponent<Collider>();
			lastFrameWithins = ((!detectsPlayer) ? new bool[obsToDetect.Count] : new bool[obsToDetect.Count + 1]);
			if (_collider == null && _collider2D == null)
			{
				ACDebug.LogWarning("Trigger '" + base.gameObject.name + " cannot detect collisions because it has no Collider!", this);
			}
		}

		protected void Start()
		{
			if ((bool)KickStarter.stateHandler)
			{
				KickStarter.stateHandler.Register(this);
			}
		}

		protected void OnDisable()
		{
			if ((bool)KickStarter.stateHandler)
			{
				KickStarter.stateHandler.Unregister(this);
			}
		}

		public void _Update()
		{
			if (detectionMethod == TriggerDetectionMethod.TransformPosition)
			{
				for (int i = 0; i < obsToDetect.Count; i++)
				{
					ProcessObject(obsToDetect[i], i);
				}
				if (detectsPlayer && KickStarter.player != null)
				{
					ProcessObject(KickStarter.player.gameObject, lastFrameWithins.Length - 1);
				}
			}
		}

		protected void OnTriggerEnter(Collider other)
		{
			if (detectionMethod == TriggerDetectionMethod.RigidbodyCollision && triggerType == 0 && IsObjectCorrect(other.gameObject))
			{
				Interact(other.gameObject);
			}
		}

		protected void OnTriggerEnter2D(Collider2D other)
		{
			if (detectionMethod == TriggerDetectionMethod.RigidbodyCollision && triggerType == 0 && IsObjectCorrect(other.gameObject))
			{
				Interact(other.gameObject);
			}
		}

		protected void OnTriggerStay(Collider other)
		{
			if (detectionMethod == TriggerDetectionMethod.RigidbodyCollision && triggerType == 1 && IsObjectCorrect(other.gameObject))
			{
				Interact(other.gameObject);
			}
		}

		protected void OnTriggerStay2D(Collider2D other)
		{
			if (detectionMethod == TriggerDetectionMethod.RigidbodyCollision && triggerType == 1 && IsObjectCorrect(other.gameObject))
			{
				Interact(other.gameObject);
			}
		}

		protected void OnTriggerExit(Collider other)
		{
			if (detectionMethod == TriggerDetectionMethod.RigidbodyCollision && triggerType == 2 && IsObjectCorrect(other.gameObject))
			{
				Interact(other.gameObject);
			}
		}

		protected void OnTriggerExit2D(Collider2D other)
		{
			if (detectionMethod == TriggerDetectionMethod.RigidbodyCollision && triggerType == 2 && IsObjectCorrect(other.gameObject))
			{
				Interact(other.gameObject);
			}
		}

		public bool IsOn()
		{
			if ((bool)GetComponent<Collider>())
			{
				return GetComponent<Collider>().enabled;
			}
			if ((bool)GetComponent<Collider2D>())
			{
				return GetComponent<Collider2D>().enabled;
			}
			return false;
		}

		public void TurnOn()
		{
			if ((bool)GetComponent<Collider>())
			{
				GetComponent<Collider>().enabled = true;
			}
			else if ((bool)GetComponent<Collider2D>())
			{
				GetComponent<Collider2D>().enabled = true;
			}
			else
			{
				ACDebug.LogWarning("Cannot turn " + base.name + " on because it has no Collider component.", this);
			}
		}

		public void TurnOff()
		{
			if ((bool)GetComponent<Collider>())
			{
				GetComponent<Collider>().enabled = false;
			}
			else if ((bool)GetComponent<Collider2D>())
			{
				GetComponent<Collider2D>().enabled = false;
			}
			else
			{
				ACDebug.LogWarning("Cannot turn " + base.name + " off because it has no Collider component.", this);
			}
			if (lastFrameWithins != null)
			{
				for (int i = 0; i < lastFrameWithins.Length; i++)
				{
					lastFrameWithins[i] = false;
				}
			}
		}

		protected void Interact(GameObject collisionOb)
		{
			if (cancelInteractions)
			{
				KickStarter.playerInteraction.StopMovingToHotspot();
			}
			if (actionListType == ActionListType.PauseGameplay)
			{
				KickStarter.playerInteraction.DeselectHotspot();
			}
			KickStarter.eventManager.Call_OnRunTrigger(this, collisionOb);
			if (source == ActionListSource.InScene)
			{
				if (useParameters && parameters != null && parameters.Count >= 1)
				{
					if (parameters[0].parameterType == ParameterType.GameObject)
					{
						parameters[0].gameObject = collisionOb;
					}
					else
					{
						ACDebug.Log("Cannot set the value of parameter 0 ('" + parameters[0].label + "') as it is not of the type 'Game Object'.", this);
					}
				}
			}
			else if (source == ActionListSource.AssetFile && assetFile != null && assetFile.NumParameters > 0 && gameObjectParameterID >= 0)
			{
				ActionParameter actionParameter = null;
				actionParameter = ((!syncParamValues) ? GetParameter(gameObjectParameterID) : assetFile.GetParameter(gameObjectParameterID));
				if (actionParameter != null)
				{
					actionParameter.SetValue(collisionOb);
				}
			}
			Interact();
		}

		protected bool IsObjectCorrect(GameObject obToCheck)
		{
			if (KickStarter.stateHandler == null || KickStarter.stateHandler.gameState == GameState.Paused || obToCheck == null)
			{
				return false;
			}
			if (KickStarter.saveSystem.loadingGame != LoadingGame.No)
			{
				return false;
			}
			if (triggerReacts == TriggerReacts.OnlyDuringGameplay && KickStarter.stateHandler.gameState != GameState.Normal)
			{
				return false;
			}
			if (triggerReacts == TriggerReacts.OnlyDuringCutscenes && KickStarter.stateHandler.IsInGameplay())
			{
				return false;
			}
			if (KickStarter.stateHandler != null && KickStarter.stateHandler.AreTriggersDisabled())
			{
				return false;
			}
			if (detectionMethod == TriggerDetectionMethod.TransformPosition)
			{
				return true;
			}
			if (detects == TriggerDetects.Player)
			{
				if (KickStarter.player != null && obToCheck == KickStarter.player.gameObject)
				{
					return true;
				}
			}
			else if (detects == TriggerDetects.SetObject)
			{
				if (obToDetect != null && obToCheck == obToDetect)
				{
					return true;
				}
			}
			else if (detects == TriggerDetects.AnyObjectWithComponent)
			{
				if (!string.IsNullOrEmpty(detectComponent))
				{
					string[] array = detectComponent.Split(";"[0]);
					string[] array2 = array;
					foreach (string text in array2)
					{
						if (!string.IsNullOrEmpty(text) && (bool)obToCheck.GetComponent(text))
						{
							return true;
						}
					}
				}
			}
			else if (detects == TriggerDetects.AnyObjectWithTag)
			{
				if (!string.IsNullOrEmpty(detectComponent))
				{
					string[] array3 = detectComponent.Split(";"[0]);
					string[] array4 = array3;
					foreach (string text2 in array4)
					{
						if (!string.IsNullOrEmpty(text2) && obToCheck.tag == text2)
						{
							return true;
						}
					}
				}
			}
			else if (detects == TriggerDetects.AnyObject)
			{
				return true;
			}
			return false;
		}

		protected void ProcessObject(GameObject objectToCheck, int i)
		{
			if (objectToCheck != null)
			{
				bool thisFrameWithin = CheckForPoint(objectToCheck.transform.position);
				if (DetermineValidity(thisFrameWithin, i) && IsObjectCorrect(objectToCheck))
				{
					Interact(objectToCheck);
				}
			}
		}

		protected bool DetermineValidity(bool thisFrameWithin, int i)
		{
			bool result = false;
			switch (triggerType)
			{
			case 0:
				if (thisFrameWithin && !lastFrameWithins[i])
				{
					result = true;
				}
				break;
			case 1:
				result = thisFrameWithin;
				break;
			case 2:
				if (!thisFrameWithin && lastFrameWithins[i])
				{
					result = true;
				}
				break;
			}
			lastFrameWithins[i] = thisFrameWithin;
			return result;
		}

		protected bool CheckForPoint(Vector3 position)
		{
			if (_collider2D != null)
			{
				if (_collider2D.enabled)
				{
					return _collider2D.OverlapPoint(position);
				}
				return false;
			}
			if (_collider != null && _collider.enabled)
			{
				return _collider.bounds.Contains(position);
			}
			return false;
		}
	}
}
