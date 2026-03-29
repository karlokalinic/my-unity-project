using UnityEngine;
using UnityEngine.AI;

namespace AC
{
	[AddComponentMenu("Adventure Creator/Navigation/NavMeshAgent Integration")]
	[RequireComponent(typeof(NavMeshAgent))]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_nav_mesh_agent_integration.html")]
	public class NavMeshAgentIntegration : MonoBehaviour
	{
		public bool useACForSpeedValues;

		public float runSpeedFactor = 2f;

		public bool useACForTurning = true;

		private float originalSpeed;

		private NavMeshAgent navMeshAgent;

		private Char _char;

		private bool disableDuringGameplay;

		private Vector3 targetPosition;

		private void Awake()
		{
			navMeshAgent = GetComponent<NavMeshAgent>();
			originalSpeed = navMeshAgent.speed;
			_char = GetComponent<Char>();
		}

		private void Start()
		{
			if (_char == null)
			{
				ACDebug.LogWarning("A 'Player' or 'NPC' component must be attached to " + base.gameObject.name + " for the NavMeshAgentIntegration script to work.", base.gameObject);
			}
			else
			{
				SetMotionControl();
				if (_char.IsPlayer && KickStarter.settingsManager != null && KickStarter.settingsManager.movementMethod != MovementMethod.PointAndClick)
				{
					disableDuringGameplay = true;
				}
			}
			if (KickStarter.sceneSettings != null && KickStarter.sceneSettings.navigationMethod != AC_NavigationMethod.UnityNavigation)
			{
				ACDebug.LogWarning("For the NavMeshAgentIntegration script to work, your scene's pathfinding method must be set to 'Unity Navigation'");
			}
		}

		private void OnTeleport()
		{
			targetPosition = _char.GetTargetPosition();
			navMeshAgent.Warp(base.transform.position);
		}

		private void Update()
		{
			if (disableDuringGameplay)
			{
				if ((bool)KickStarter.stateHandler && KickStarter.stateHandler.gameState == GameState.Cutscene)
				{
					navMeshAgent.enabled = true;
					SetMotionControl();
					SetCharacterPosition();
				}
				else
				{
					navMeshAgent.enabled = false;
					_char.motionControl = MotionControl.Automatic;
				}
			}
			else
			{
				SetMotionControl();
				SetCharacterPosition();
			}
		}

		private void SetCharacterPosition()
		{
			if (_char != null && !_char.IsTurningBeforeWalking())
			{
				if ((bool)_char.GetPath() || _char.charState == CharState.Idle)
				{
					targetPosition = _char.GetTargetPosition();
				}
				if (_char.isRunning && Vector3.Distance(targetPosition, base.transform.position) < navMeshAgent.stoppingDistance && _char.WillStopAtNextNode())
				{
					_char.isRunning = false;
				}
				if (useACForSpeedValues)
				{
					navMeshAgent.speed = ((!_char.isRunning) ? _char.walkSpeedScale : _char.runSpeedScale);
				}
				else
				{
					navMeshAgent.speed = ((!_char.isRunning) ? originalSpeed : (originalSpeed * runSpeedFactor));
				}
				if (navMeshAgent.isOnNavMesh)
				{
					navMeshAgent.SetDestination(targetPosition);
				}
			}
		}

		private void SetMotionControl()
		{
			_char.motionControl = (useACForTurning ? MotionControl.JustTurning : MotionControl.Manual);
		}
	}
}
