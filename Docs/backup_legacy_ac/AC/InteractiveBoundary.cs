using System;
using UnityEngine;

namespace AC
{
	[AddComponentMenu("Adventure Creator/Hotspots/Interactive boundary")]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_interactive_boundary.html")]
	public class InteractiveBoundary : MonoBehaviour
	{
		protected bool forcePresence;

		protected bool playerIsPresent;

		public bool PlayerIsPresent
		{
			get
			{
				if (forcePresence)
				{
					return true;
				}
				return playerIsPresent;
			}
		}

		public bool ForcePresence
		{
			set
			{
				forcePresence = value;
			}
		}

		protected void OnEnable()
		{
			EventManager.OnSetPlayer = (EventManager.Delegate_SetPlayer)Delegate.Combine(EventManager.OnSetPlayer, new EventManager.Delegate_SetPlayer(OnSwitchPlayer));
		}

		protected void OnDisable()
		{
			EventManager.OnSetPlayer = (EventManager.Delegate_SetPlayer)Delegate.Remove(EventManager.OnSetPlayer, new EventManager.Delegate_SetPlayer(OnSwitchPlayer));
		}

		protected void OnSwitchPlayer(Player player)
		{
			playerIsPresent = false;
		}

		protected void OnTriggerEnter(Collider other)
		{
			if (KickStarter.player != null && other.gameObject == KickStarter.player.gameObject)
			{
				playerIsPresent = true;
			}
		}

		protected void OnTriggerExit(Collider other)
		{
			if (KickStarter.player != null && other.gameObject == KickStarter.player.gameObject)
			{
				playerIsPresent = false;
			}
		}

		protected void OnTriggerStay2D(Collider2D other)
		{
			if (KickStarter.player != null && other.gameObject == KickStarter.player.gameObject)
			{
				playerIsPresent = true;
			}
		}

		protected void OnTriggerExit2D(Collider2D other)
		{
			if (KickStarter.player != null && other.gameObject == KickStarter.player.gameObject)
			{
				playerIsPresent = false;
			}
		}
	}
}
