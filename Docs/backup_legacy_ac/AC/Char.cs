using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace AC
{
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_char.html")]
	public class Char : MonoBehaviour, ITranslatable
	{
		public CharState charState;

		private AnimEngine animEngine;

		private LerpUtils.FloatLerp turnFloatLerp = new LerpUtils.FloatLerp();

		private LerpUtils.FloatLerp newAngleLerp = new LerpUtils.FloatLerp();

		private LerpUtils.FloatLerp newAngleLinearLerp = new LerpUtils.FloatLerp(true);

		private LerpUtils.Vector2Lerp targetHeadAnglesLerp = new LerpUtils.Vector2Lerp();

		private LerpUtils.FloatLerp headTurnWeightLerp = new LerpUtils.FloatLerp();

		private LerpUtils.Vector2Lerp actualHeadAnglesLerp = new LerpUtils.Vector2Lerp();

		private LerpUtils.FloatLerp nonFacingFactorLerp = new LerpUtils.FloatLerp();

		private LerpUtils.FloatLerp wallReductionLerp = new LerpUtils.FloatLerp();

		protected LerpUtils.FloatLerp moveSpeedLerp = new LerpUtils.FloatLerp();

		private LerpUtils.Vector3Lerp exactPositionLerp = new LerpUtils.Vector3Lerp();

		public AnimationEngine animationEngine = AnimationEngine.SpritesUnity;

		public string customAnimationClass = string.Empty;

		public MotionControl motionControl;

		public TalkingAnimation talkingAnimation;

		public bool separateTalkingLayer;

		public string speechLabel = string.Empty;

		public int lineID = -1;

		public int displayLineID = -1;

		public Color speechColor = Color.white;

		public CursorIconBase portraitIcon = new CursorIconBase();

		public bool useExpressions;

		public List<Expression> expressions = new List<Expression>();

		public Transform speechMenuPlacement;

		public bool mapExpressionsToShapeable;

		public int expressionGroupID;

		public float lipSyncBlendShapeSpeedFactor = 1f;

		private Expression currentExpression;

		protected Quaternion newRotation;

		private float prevHeight;

		private float prevHeight2;

		private float heightChange;

		private Shapeable shapeable;

		private LipSyncTexture lipSyncTexture;

		private List<LipSyncShape> lipSyncShapes = new List<LipSyncShape>();

		public bool isLipSyncing;

		public string phonemeParameter = string.Empty;

		public int lipSyncGroupID;

		public Transform leftHandBone;

		public Transform rightHandBone;

		private GameObject leftHandHeldObject;

		private GameObject rightHandHeldObject;

		public AnimationClip idleAnim;

		public AnimationClip walkAnim;

		public AnimationClip runAnim;

		public AnimationClip talkAnim;

		public AnimationClip turnLeftAnim;

		public AnimationClip turnRightAnim;

		public AnimationClip headLookLeftAnim;

		public AnimationClip headLookRightAnim;

		public AnimationClip headLookUpAnim;

		public AnimationClip headLookDownAnim;

		private Animation _animation;

		public Transform upperBodyBone;

		public Transform leftArmBone;

		public Transform rightArmBone;

		public Transform neckBone;

		public float animCrossfadeSpeed = 0.2f;

		private Vector3 exactDestination;

		public int groundCheckLayerMask = 1;

		public string moveSpeedParameter = "Speed";

		public string verticalMovementParameter = string.Empty;

		public string isGroundedParameter;

		public string jumpParameter = "Jump";

		public string turnParameter = string.Empty;

		public string talkParameter = "IsTalking";

		public string directionParameter = "Direction";

		public string angleParameter = "Angle";

		public string headYawParameter = string.Empty;

		public string headPitchParameter = string.Empty;

		public string expressionParameter = string.Empty;

		public float rootTurningFactor;

		public int headLayer = 1;

		public int mouthLayer = 2;

		public Animator customAnimator;

		private bool animatorIsOnRoot;

		public float turningAngleThreshold = 4f;

		private Animator _animator;

		public bool turn2DCharactersIn3DSpace = true;

		public Transform spriteChild;

		public RotateSprite3D rotateSprite3D;

		public string idleAnimSprite = "idle";

		public string walkAnimSprite = "walk";

		public string runAnimSprite = "run";

		public string talkAnimSprite = "talk";

		public bool lockScale;

		public float spriteScale = 1f;

		public bool lockDirection;

		public string spriteDirection = "D";

		private FollowSortingMap followSortingMap;

		private float spriteAngle;

		public bool doDirections = true;

		public bool crossfadeAnims;

		public bool doDiagonals;

		public bool isTalking;

		public AC_2DFrameFlipping frameFlipping;

		public bool flipCustomAnims;

		private Vector3 originalScale;

		private bool flipFrames;

		public SpriteDirectionData _spriteDirectionData;

		public AngleSnapping angleSnapping;

		public float walkSpeedScale = 2f;

		public float runSpeedScale = 6f;

		public float reverseSpeedFactor = 1f;

		public float turnSpeed = 7f;

		public float acceleration = 6f;

		public float deceleration;

		public bool turnBeforeWalking;

		public float runDistanceThreshold = 1f;

		public bool antiGlideMode;

		public bool retroPathfinding;

		protected float pathfindUpdateTime;

		protected bool isJumping;

		private float sortingMapScale = 1f;

		private bool isReversing;

		protected float turnFloat;

		private string currentSpriteName = string.Empty;

		private SpriteRenderer _spriteRenderer;

		private bool isTurningBeforeWalking;

		private float lastDist;

		private bool isExactLerping;

		private Vector3 newVel;

		private float nonFacingFactor = 1f;

		private Paths ownPath;

		private Quaternion actualRotation;

		private Vector3 actualForward = Vector3.forward;

		private Vector3 actualRight = Vector3.right;

		public bool ignoreGravity;

		public bool freezeRigidbodyWhenIdle;

		public bool useRigidbodyForMovement = true;

		public bool useRigidbody2DForMovement;

		protected Rigidbody _rigidbody;

		protected Rigidbody2D _rigidbody2D;

		protected float originalGravityScale = 1f;

		protected Collider _collider;

		private CapsuleCollider capsuleCollider;

		private CapsuleCollider[] capsuleColliders;

		protected CharacterController _characterController;

		public bool doWallReduction;

		public string wallLayer = "Default";

		public float wallDistance = 0.5f;

		public bool wallReductionOnlyParameter = true;

		private float wallReductionFactor = 1f;

		private Vector3 wallRayOrigin = Vector3.zero;

		private float wallRayForward;

		public AudioClip walkSound;

		public AudioClip runSound;

		public AudioClip textScrollClip;

		public Sound soundChild;

		protected AudioSource audioSource;

		public AudioSource speechAudioSource;

		protected Paths activePath;

		protected float moveSpeed;

		protected Vector3 moveDirection;

		protected int targetNode;

		protected bool pausePath;

		protected Vector3 lookDirection;

		private float pausePathTime;

		private ActionList nodeActionList;

		private int prevNode;

		private int lastPathPrevNode;

		private int lastPathTargetNode;

		private Paths lastPathActivePath;

		protected bool tankTurning;

		private Vector2 targetHeadAngles;

		private Vector2 actualHeadAngles;

		private float headTurnWeight;

		[Range(0f, 1f)]
		public float headIKTurnFactor = 1f;

		[Range(0f, 1f)]
		public float bodyIKTurnFactor;

		[Range(0f, 1f)]
		public float eyesIKTurnFactor = 1f;

		public Transform headTurnTarget;

		public Vector3 headTurnTargetOffset;

		public HeadFacing headFacing;

		public bool ikHeadTurning;

		public float headTurnSpeed = 4f;

		private Vector3 defaultExactDestination = new Vector3(0f, 0f, 1234.5f);

		private Sound speechSound;

		private bool isPlayer;

		private bool isUnderTimelineControl;

		private CharacterAnimation2DShot activeCharacterAnimation2DShot;

		public bool isRunning { get; set; }

		public bool IsPlayer
		{
			get
			{
				return isPlayer;
			}
		}

		public Expression CurrentExpression
		{
			get
			{
				return currentExpression;
			}
		}

		private bool CanPhysicallyRotate
		{
			get
			{
				if (animEngine != null && animEngine.isSpriteBased && !turn2DCharactersIn3DSpace && motionControl != MotionControl.Manual)
				{
					return false;
				}
				return true;
			}
		}

		public Quaternion TransformRotation
		{
			get
			{
				if (CanPhysicallyRotate)
				{
					return base.transform.rotation;
				}
				return actualRotation;
			}
			set
			{
				if (CanPhysicallyRotate)
				{
					base.transform.rotation = value;
				}
				actualRotation = value;
				actualForward = actualRotation * Vector3.forward;
				actualRight = actualRotation * Vector3.right;
			}
		}

		public Vector3 TransformForward
		{
			get
			{
				if (CanPhysicallyRotate)
				{
					return base.transform.forward;
				}
				return actualForward;
			}
		}

		public Vector3 TransformRight
		{
			get
			{
				if (CanPhysicallyRotate)
				{
					return base.transform.right;
				}
				return actualRight;
			}
		}

		public bool IsJumping
		{
			get
			{
				return isJumping;
			}
		}

		public SpriteDirectionData spriteDirectionData
		{
			get
			{
				if (_spriteDirectionData == null || !_spriteDirectionData.IsUpgraded)
				{
					_spriteDirectionData = new SpriteDirectionData(doDirections, doDiagonals);
				}
				return _spriteDirectionData;
			}
		}

		public CharacterAnimation2DShot ActiveCharacterAnimation2DShot
		{
			get
			{
				return activeCharacterAnimation2DShot;
			}
			set
			{
				activeCharacterAnimation2DShot = value;
			}
		}

		private bool AnimationControlledByAnimationShot
		{
			get
			{
				return activeCharacterAnimation2DShot != null;
			}
		}

		protected void _Awake()
		{
			newRotation = TransformRotation;
			isTalking = false;
			exactDestination = defaultExactDestination;
			if ((bool)GetComponent<CharacterController>())
			{
				_characterController = GetComponent<CharacterController>();
				wallRayOrigin = _characterController.center;
				wallRayForward = _characterController.radius;
			}
			else if ((bool)GetComponent<CapsuleCollider>())
			{
				CapsuleCollider component = GetComponent<CapsuleCollider>();
				wallRayOrigin = component.center;
				wallRayForward = component.radius;
			}
			else if ((bool)GetComponent<CircleCollider2D>())
			{
				CircleCollider2D component2 = GetComponent<CircleCollider2D>();
				wallRayOrigin = component2.offset;
				wallRayForward = component2.radius;
			}
			else if ((bool)GetComponent<BoxCollider2D>())
			{
				BoxCollider2D component3 = GetComponent<BoxCollider2D>();
				wallRayOrigin = component3.bounds.center;
				wallRayForward = component3.bounds.size.x / 2f;
			}
			isPlayer = this is Player;
			ownPath = GetComponent<Paths>();
			if (ownPath == null)
			{
				ownPath = base.gameObject.AddComponent<Paths>();
			}
			if ((bool)GetComponentInChildren<FollowSortingMap>())
			{
				base.transform.localScale = Vector3.one;
			}
			originalScale = base.transform.localScale;
			charState = CharState.Idle;
			shapeable = GetShapeable();
			lipSyncTexture = GetComponentInChildren<LipSyncTexture>();
			if ((bool)GetComponent<Sound>())
			{
				speechSound = GetComponent<Sound>();
			}
			ResetAnimationEngine();
			ResetBaseClips();
			_animator = GetAnimator();
			_animation = GetAnimation();
			SetAntiGlideState();
			if ((bool)spriteChild && (bool)spriteChild.gameObject.GetComponent<SpriteRenderer>())
			{
				_spriteRenderer = spriteChild.gameObject.GetComponent<SpriteRenderer>();
				if (((Vector2)spriteChild.localPosition).magnitude > 0f)
				{
					ACDebug.LogWarning("The sprite child of '" + base.gameObject.name + "' is not positioned at (0,0,0) - is this correct?", base.gameObject);
				}
			}
			if ((bool)spriteChild && (bool)spriteChild.GetComponent<FollowSortingMap>())
			{
				followSortingMap = spriteChild.GetComponent<FollowSortingMap>();
			}
			else if ((bool)GetComponentInChildren<FollowSortingMap>())
			{
				followSortingMap = GetComponentInChildren<FollowSortingMap>();
			}
			if (speechAudioSource == null && (bool)GetComponent<AudioSource>())
			{
				speechAudioSource = GetComponent<AudioSource>();
			}
			if ((bool)soundChild && (bool)soundChild.gameObject.GetComponent<AudioSource>())
			{
				audioSource = soundChild.gameObject.GetComponent<AudioSource>();
			}
			if ((bool)GetComponent<Rigidbody>())
			{
				_rigidbody = GetComponent<Rigidbody>();
			}
			else if ((bool)GetComponent<Rigidbody2D>())
			{
				_rigidbody2D = GetComponent<Rigidbody2D>();
				originalGravityScale = _rigidbody2D.gravityScale;
				if (originalGravityScale <= 0f)
				{
					originalGravityScale = 1f;
				}
				if (SceneSettings.CameraPerspective != CameraPerspective.TwoD)
				{
					ACDebug.LogWarning("In order to move a sprite-based character (" + base.gameObject.name + ") in 3D, there must not be a Rigidbody2D component on the base.", base.gameObject);
				}
				else if (antiGlideMode)
				{
					_rigidbody2D.isKinematic = true;
					_rigidbody2D = null;
					ACDebug.LogWarning("The use of character " + base.gameObject.name + "'s Rigidbody2D component is disabled as it conflicts with the 'Only move when sprite changes feature.", base.gameObject);
				}
			}
			PhysicsUpdate();
			if ((bool)GetComponent<Collider>())
			{
				_collider = GetComponent<Collider>();
				if (_collider is CapsuleCollider)
				{
					capsuleCollider = _collider as CapsuleCollider;
				}
			}
			capsuleColliders = GetComponentsInChildren<CapsuleCollider>();
			AdvGame.AssignMixerGroup(speechAudioSource, SoundType.Speech);
			AdvGame.AssignMixerGroup(audioSource, SoundType.SFX);
			displayLineID = lineID;
		}

		private void OnEnable()
		{
			if ((bool)KickStarter.stateHandler)
			{
				KickStarter.stateHandler.Register(this);
			}
		}

		private void Start()
		{
			if ((bool)KickStarter.stateHandler)
			{
				KickStarter.stateHandler.Register(this);
			}
		}

		private void OnDisable()
		{
			if ((bool)KickStarter.stateHandler)
			{
				KickStarter.stateHandler.Unregister(this);
			}
		}

		public virtual void _Update()
		{
			CalculateHeadTurn();
			UpdateWallReductionFactor();
			CalcHeightChange();
			if (spriteChild != null && KickStarter.settingsManager != null)
			{
				PrepareSpriteChild(SceneSettings.IsTopDown(), SceneSettings.IsUnity2D());
			}
			AnimUpdate();
			SpeedUpdate();
			PathUpdate();
			PhysicsUpdate();
			if (!antiGlideMode)
			{
				MoveUpdate();
			}
		}

		public void _LateUpdate()
		{
			if (antiGlideMode)
			{
				MoveUpdate();
				if ((bool)spriteChild && KickStarter.settingsManager != null)
				{
					PrepareSpriteChild(SceneSettings.IsTopDown(), SceneSettings.IsUnity2D());
				}
			}
			if ((bool)spriteChild && KickStarter.settingsManager != null)
			{
				UpdateSpriteChild(SceneSettings.IsTopDown(), SceneSettings.IsUnity2D());
			}
			UpdateScale();
		}

		public virtual void _FixedUpdate()
		{
			MoveRigidbody();
			DoTurn(true);
		}

		private void OnAnimatorIK(int layerIndex)
		{
			if (ikHeadTurning)
			{
				if (headTurnWeight > 0f && headTurnTarget != null)
				{
					Vector3 lookAtPosition = CalculateIKHeadTurnPosition();
					_animator.SetLookAtPosition(lookAtPosition);
				}
				_animator.SetLookAtWeight(headTurnWeight, bodyIKTurnFactor, headIKTurnFactor, eyesIKTurnFactor);
			}
		}

		private Vector3 CalculateIKHeadTurnPosition()
		{
			Vector3 position = base.transform.position;
			if (neckBone != null)
			{
				position += neckBone.position - base.transform.position;
			}
			else if (_characterController != null)
			{
				position += new Vector3(0f, _characterController.height * base.transform.localScale.y * 0.8f, 0f);
			}
			else if (capsuleCollider != null)
			{
				position += new Vector3(0f, capsuleCollider.height * base.transform.localScale.y * 0.8f, 0f);
			}
			Vector3 vector = Vector3.RotateTowards(TransformForward, TransformRight, actualHeadAngles.x * ((float)Math.PI / 180f), 1f);
			return position + vector + Vector3.up * Mathf.Asin(actualHeadAngles.y * ((float)Math.PI / 180f));
		}

		public void RecalculateActivePathfind()
		{
			if (activePath != null && !pausePath && activePath == ownPath && pathfindUpdateTime >= 0f)
			{
				Vector3 point = activePath.nodes[activePath.nodes.Count - 1];
				MoveToPoint(point, isRunning, true);
			}
		}

		protected void PathUpdate()
		{
			if (!activePath || activePath.nodes.Count <= 0)
			{
				return;
			}
			if (pausePath)
			{
				if (nodeActionList != null)
				{
					if (!KickStarter.actionListManager.IsListRunning(nodeActionList))
					{
						SetNextNodes(true);
					}
				}
				else if (Time.time > pausePathTime)
				{
					SetNextNodes(true);
				}
				return;
			}
			if (targetNode == -1 || targetNode >= activePath.nodes.Count)
			{
				ACDebug.LogWarning("Invalid node target - cannot update pathfinding on " + base.name, this);
				return;
			}
			if (pathfindUpdateTime > 0f)
			{
				pathfindUpdateTime -= Time.deltaTime;
				if (pathfindUpdateTime <= 0f)
				{
					pathfindUpdateTime = 0f;
					RecalculateActivePathfind();
				}
			}
			Vector3 vector = activePath.nodes[targetNode] - base.transform.position;
			Vector3 direction = new Vector3(vector.x, 0f, vector.z);
			if (SceneSettings.IsUnity2D())
			{
				vector.z = 0f;
				SetMoveDirection(vector);
				direction = new Vector3(vector.x, 0f, vector.y);
				SetLookDirection(direction, false);
			}
			else if (activePath.affectY)
			{
				SetMoveDirection(vector);
				SetLookDirection(direction, false);
			}
			else
			{
				SetLookDirection(direction, false);
				SetMoveDirectionAsForward();
			}
			if (isRunning && vector.magnitude > 0f && vector.magnitude < runDistanceThreshold && WillStopAtNextNode())
			{
				isRunning = false;
			}
			float num = KickStarter.settingsManager.GetDestinationThreshold();
			if (isRunning && GetMotionControl() == MotionControl.Automatic)
			{
				float num2 = (1f - runSpeedScale / walkSpeedScale) / 20f;
				num2 *= Mathf.Clamp(GetDeceleration(), 1f, 20f);
				num2 += runSpeedScale / walkSpeedScale;
				num *= num2;
			}
			if (retroPathfinding)
			{
				num = 0.01f;
			}
			float magnitude = vector.magnitude;
			if ((!SceneSettings.IsUnity2D() || !(magnitude < num)) && (!activePath.affectY || !(magnitude < num)) && (activePath.affectY || !(direction.magnitude < num)))
			{
				return;
			}
			KickStarter.eventManager.Call_OnCharacterReachNode(this, activePath, targetNode);
			if (activePath.nodeCommands.Count > targetNode)
			{
				NodeCommand nodeCommand = activePath.nodeCommands[targetNode];
				nodeCommand.SetParameter(activePath.commandSource, base.gameObject);
				if (activePath.commandSource == ActionListSource.InScene && nodeCommand.cutscene != null && nodeCommand.pausesCharacter)
				{
					PausePath(activePath.nodePause, nodeCommand.cutscene);
				}
				else if (activePath.commandSource == ActionListSource.AssetFile && nodeCommand.actionListAsset != null && nodeCommand.pausesCharacter)
				{
					PausePath(activePath.nodePause, nodeCommand.actionListAsset);
				}
				else if (activePath.nodePause > 0f)
				{
					PausePath(activePath.nodePause);
				}
				else
				{
					SetNextNodes();
				}
			}
			else if (activePath.nodePause > 0f)
			{
				PausePath(activePath.nodePause);
			}
			else
			{
				SetNextNodes();
			}
		}

		private void SpeedUpdate()
		{
			if (AnimationControlledByAnimationShot)
			{
				moveSpeed = moveSpeedLerp.Update(moveSpeed, GetTargetSpeed(), acceleration);
			}
			else if (charState == CharState.Move)
			{
				lastDist = float.PositiveInfinity;
				Accelerate();
			}
			else if (charState == CharState.Decelerate || charState == CharState.Custom)
			{
				Decelerate();
			}
			else if (charState == CharState.Idle)
			{
				if (moveSpeed > 0f)
				{
					lastDist = float.PositiveInfinity;
					moveSpeed = 0f;
				}
				isExactLerping = false;
			}
		}

		private void PhysicsUpdate()
		{
			if (GetMotionControl() == MotionControl.Manual || GetMotionControl() == MotionControl.JustTurning)
			{
				return;
			}
			if ((bool)_rigidbody)
			{
				if (ignoreGravity)
				{
					_rigidbody.useGravity = false;
				}
				else if (charState == CharState.Custom && moveSpeed < 0.01f)
				{
					_rigidbody.useGravity = false;
				}
				else if ((bool)activePath && activePath.affectY)
				{
					_rigidbody.useGravity = false;
				}
				else
				{
					_rigidbody.useGravity = true;
				}
			}
			else if ((bool)_rigidbody2D)
			{
				if (ignoreGravity)
				{
					_rigidbody2D.gravityScale = 0f;
				}
				else if (charState == CharState.Custom && moveSpeed < 0.01f)
				{
					_rigidbody2D.gravityScale = 0f;
				}
				else if ((bool)activePath && activePath.affectY)
				{
					_rigidbody2D.gravityScale = 0f;
				}
				else
				{
					_rigidbody2D.gravityScale = originalGravityScale;
				}
			}
		}

		private void AnimUpdate()
		{
			if (animEngine == null)
			{
				return;
			}
			if (isTalking)
			{
				ProcessLipSync();
			}
			if (IsMovingHead() || animEngine.updateHeadAlways)
			{
				AnimateHeadTurn();
			}
			if (AnimationControlledByAnimationShot)
			{
				return;
			}
			if (isJumping)
			{
				animEngine.PlayJump();
				StopStandardAudio();
			}
			else if (charState == CharState.Idle || charState == CharState.Decelerate)
			{
				if (IsTurning())
				{
					if (turnFloat < 0f)
					{
						animEngine.PlayTurnLeft();
					}
					else
					{
						animEngine.PlayTurnRight();
					}
				}
				else if (isTalking && (talkingAnimation == TalkingAnimation.Standard || animationEngine == AnimationEngine.Custom))
				{
					animEngine.PlayTalk();
				}
				else
				{
					animEngine.PlayIdle();
				}
				StopStandardAudio();
			}
			else if (charState == CharState.Move)
			{
				if (isRunning)
				{
					animEngine.PlayRun();
				}
				else
				{
					animEngine.PlayWalk();
				}
				PlayStandardAudio();
			}
			else
			{
				StopStandardAudio();
			}
			animEngine.PlayVertical();
		}

		private void MoveUpdate()
		{
			if ((bool)animEngine && GetMotionControl() == MotionControl.Automatic)
			{
				RootMotionType rootMotionType = GetRootMotionType();
				if (Mathf.Approximately(moveSpeed, 0f))
				{
					newVel = Vector3.zero;
				}
				if (moveSpeed > 0f && rootMotionType != RootMotionType.ThreeD)
				{
					newVel = moveDirection * moveSpeed * walkSpeedScale * sortingMapScale;
					if (isReversing)
					{
						newVel *= reverseSpeedFactor;
					}
					if (SceneSettings.IsTopDown())
					{
						float magnitude = newVel.magnitude;
						if (magnitude > 0f)
						{
							float num = Mathf.Abs(Vector3.Dot(newVel.normalized, Vector3.forward));
							float num2 = magnitude * (1f - num) + magnitude * KickStarter.sceneSettings.GetVerticalReductionFactor() * num;
							newVel *= num2 / magnitude;
						}
					}
					else if (SceneSettings.IsUnity2D())
					{
						newVel.z = 0f;
						float magnitude2 = newVel.magnitude;
						if (magnitude2 > 0f)
						{
							float num3 = Mathf.Abs(Vector3.Dot(newVel.normalized, Vector3.up));
							float num4 = magnitude2 * (1f - num3) + magnitude2 * KickStarter.sceneSettings.GetVerticalReductionFactor() * num3;
							newVel *= num4 / magnitude2;
						}
					}
					else
					{
						newVel *= GetNonFacingReductionFactor();
						newVel *= ((!doWallReduction || wallReductionOnlyParameter) ? 1f : wallReductionFactor);
					}
					newVel *= KickStarter.playerInput.GetDragMovementSlowDown();
					bool flag = false;
					if (rootMotionType == RootMotionType.TwoD)
					{
						if (spriteDirection == "L" || spriteDirection == "R")
						{
							newVel.x = 0f;
						}
						else if (spriteDirection == "U" || spriteDirection == "D")
						{
							newVel.y = 0f;
						}
						else
						{
							newVel = Vector3.zero;
						}
					}
					else if (antiGlideMode && _spriteRenderer != null && _spriteRenderer.sprite != null)
					{
						string text = _spriteRenderer.sprite.name;
						if (text == currentSpriteName)
						{
							flag = true;
						}
						else
						{
							currentSpriteName = text;
						}
					}
					if (!flag)
					{
						float num5 = ((!antiGlideMode) ? Time.deltaTime : Time.fixedDeltaTime);
						if (num5 > 0f)
						{
							if (!DoRigidbodyMovement() && !DoRigidbodyMovement2D())
							{
								if ((bool)_characterController)
								{
									if (!ignoreGravity)
									{
										if (IsGrounded())
										{
											newVel.y = (0f - _characterController.stepOffset) / Time.deltaTime;
										}
										else
										{
											newVel += Physics.gravity;
										}
									}
									_characterController.Move(newVel * num5);
								}
								else if (retroPathfinding && IsMovingAlongPath())
								{
									float num6 = ((!isRunning) ? walkSpeedScale : runSpeedScale) * sortingMapScale;
									float num7 = Mathf.Abs(Vector2.Dot((GetTargetPosition() - base.transform.position).normalized, Vector2.up));
									float num8 = 1f - num7 + KickStarter.sceneSettings.GetVerticalReductionFactor() * num7;
									num6 *= num8;
									Vector3 position = Vector3.MoveTowards(base.transform.position, GetTargetPosition(), num6 * num5);
									base.transform.position = position;
								}
								else
								{
									base.transform.position += newVel * num5;
								}
							}
							if (antiGlideMode && KickStarter.mainCamera != null && KickStarter.mainCamera.attachedCamera != null)
							{
								KickStarter.mainCamera.attachedCamera._Update();
							}
						}
					}
				}
				else if ((bool)_characterController && rootMotionType != RootMotionType.ThreeD && !_characterController.isGrounded && !ignoreGravity)
				{
					_characterController.Move(Physics.gravity * Time.deltaTime);
				}
			}
			Turn(false);
			DoTurn();
		}

		private Vector3 CalcForce(Vector3 targetVelocity, float verticalSpeed, float mass, Vector3 rigidbodyVelocity)
		{
			targetVelocity.y += verticalSpeed;
			Vector3 vector = targetVelocity - rigidbodyVelocity;
			return mass * vector / Time.deltaTime * Time.fixedDeltaTime / 0.02f;
		}

		private void MoveRigidbody()
		{
			if (GetMotionControl() == MotionControl.Manual || GetMotionControl() == MotionControl.JustTurning)
			{
				return;
			}
			if (DoRigidbodyMovement())
			{
				if (GetRootMotionType() == RootMotionType.None)
				{
					if (_rigidbody.isKinematic)
					{
						_rigidbody.MovePosition(base.transform.position + newVel * Time.deltaTime);
					}
					else
					{
						Vector3 force = CalcForce(newVel, _rigidbody.linearVelocity.y, _rigidbody.mass, _rigidbody.linearVelocity);
						_rigidbody.AddForce(force);
					}
				}
				bool flag = false;
				bool flag2 = false;
				bool flag3 = false;
				if (freezeRigidbodyWhenIdle)
				{
					flag = (flag2 = (flag3 = !isJumping && (charState == CharState.Custom || charState == CharState.Idle)));
				}
				else
				{
					flag = (_rigidbody.constraints & RigidbodyConstraints.FreezePositionX) != 0;
					flag2 = (_rigidbody.constraints & RigidbodyConstraints.FreezePositionY) != 0;
					flag3 = (_rigidbody.constraints & RigidbodyConstraints.FreezePositionZ) != 0;
				}
				_rigidbody.constraints = (RigidbodyConstraints)(0x70 | (flag ? 2 : 0) | (flag2 ? 4 : 0) | (flag3 ? 8 : 0));
			}
			else
			{
				if (!DoRigidbodyMovement2D())
				{
					return;
				}
				if (GetRootMotionType() == RootMotionType.None)
				{
					if (_rigidbody2D.isKinematic)
					{
						_rigidbody2D.MovePosition(base.transform.position + newVel * Time.deltaTime);
					}
					else
					{
						Vector3 vector = CalcForce(newVel, 0f, _rigidbody2D.mass, _rigidbody2D.linearVelocity);
						_rigidbody2D.AddForce(vector);
					}
				}
				bool flag4 = false;
				bool flag5 = false;
				if (freezeRigidbodyWhenIdle)
				{
					flag4 = (flag5 = !isJumping && (charState == CharState.Custom || charState == CharState.Idle));
				}
				else
				{
					flag4 = (_rigidbody2D.constraints & RigidbodyConstraints2D.FreezePositionX) != 0;
					flag5 = (_rigidbody2D.constraints & RigidbodyConstraints2D.FreezePositionY) != 0;
				}
				_rigidbody2D.constraints = (RigidbodyConstraints2D)(4 | (flag4 ? 1 : 0) | (flag5 ? 2 : 0));
			}
		}

		protected float GetTargetSpeed()
		{
			if (animatorIsOnRoot)
			{
				if (isRunning)
				{
					return runSpeedScale;
				}
				return walkSpeedScale;
			}
			if (AnimationControlledByAnimationShot)
			{
				if (isRunning)
				{
					return runSpeedScale / walkSpeedScale;
				}
				return 1f;
			}
			if (isRunning)
			{
				return moveDirection.magnitude * runSpeedScale / walkSpeedScale;
			}
			return moveDirection.magnitude;
		}

		private Vector3 GetSmartPosition(Vector3 targetPoint)
		{
			if (SceneSettings.IsUnity2D())
			{
				targetPoint.z = base.transform.position.z;
			}
			else
			{
				targetPoint.y = base.transform.position.y;
			}
			return targetPoint;
		}

		protected void AccurateAcc(float targetSpeed, bool canStop)
		{
			if (IsTurningBeforeWalking())
			{
				return;
			}
			float num = 3f * ((!isRunning) ? 1f : (runSpeedScale / walkSpeedScale)) / GetDeceleration();
			float num2 = Vector3.Distance(GetSmartPosition(exactDestination), base.transform.position);
			if (canStop && num2 <= 0f)
			{
				charState = CharState.Idle;
				base.transform.position = GetSmartPosition(exactDestination);
				moveSpeed = 0f;
				isExactLerping = false;
				exactDestination = defaultExactDestination;
				return;
			}
			if (canStop && (num2 < num / 4f || num2 > lastDist))
			{
				isExactLerping = true;
				base.transform.position = exactPositionLerp.Update(base.transform.position, GetSmartPosition(exactDestination), GetDeceleration());
			}
			else
			{
				isExactLerping = false;
			}
			if (num2 < lastDist)
			{
				float num3 = 1f;
				if (num2 < num)
				{
					isRunning = false;
					num3 = num2 / num;
				}
				moveSpeed = moveSpeedLerp.Update(moveSpeed, targetSpeed * num3, (!canStop) ? acceleration : GetDeceleration());
			}
			if (canStop)
			{
				moveDirection = GetSmartPosition(exactDestination) - base.transform.position;
				lastDist = num2;
			}
		}

		protected virtual void Accelerate()
		{
			if (AccurateDestination() && WillStopAtNextNode())
			{
				AccurateAcc(GetTargetSpeed(), false);
			}
			else
			{
				moveSpeed = moveSpeedLerp.Update(moveSpeed, GetTargetSpeed(), acceleration);
			}
		}

		private void Decelerate()
		{
			if (animEngine != null && animEngine.turningStyle == TurningStyle.Linear)
			{
				moveSpeed = moveSpeedLerp.Update(moveSpeed, 0f, GetDeceleration() * 3f);
			}
			else if (AccurateDestination() && !CanBeDirectControlled() && charState == CharState.Decelerate)
			{
				AccurateAcc(moveSpeed, true);
			}
			else
			{
				exactDestination = defaultExactDestination;
				moveSpeed = moveSpeedLerp.Update(moveSpeed, 0f, GetDeceleration());
			}
			if (moveSpeed <= 0f)
			{
				moveSpeed = 0f;
				if (charState != CharState.Custom)
				{
					charState = CharState.Idle;
				}
			}
		}

		public bool AccurateDestination()
		{
			if (retroPathfinding)
			{
				return false;
			}
			if (motionControl == MotionControl.Automatic && KickStarter.settingsManager.experimentalAccuracy && KickStarter.settingsManager.destinationAccuracy >= 1f && (this is NPC || KickStarter.settingsManager.movementMethod != MovementMethod.StraightToCursor) && exactDestination != defaultExactDestination)
			{
				return true;
			}
			return false;
		}

		private float GetDeceleration()
		{
			if (deceleration <= 0f)
			{
				return acceleration;
			}
			return deceleration;
		}

		public void Teleport(Vector3 _position, bool recalculateActivePathFind = false)
		{
			bool flag = false;
			if (_characterController != null)
			{
				flag = _characterController.enabled;
				_characterController.enabled = false;
			}
			base.transform.position = _position;
			if (flag)
			{
				_characterController.enabled = true;
			}
			if (recalculateActivePathFind)
			{
				RecalculateActivePathfind();
			}
			SendMessage("OnTeleport", SendMessageOptions.DontRequireReceiver);
		}

		public void SetRotation(Quaternion _rotation)
		{
			TransformRotation = _rotation;
			SetLookDirection(TransformForward, true);
		}

		public void SetRotation(float angle)
		{
			TransformRotation = Quaternion.AngleAxis(angle, Vector3.up);
			SetLookDirection(TransformForward, true);
		}

		public void StopTurning()
		{
			SetLookDirection(TransformForward, true);
			newRotation = TransformRotation;
			StopTankTurning();
			newAngleLerp.Reset();
			newAngleLinearLerp.Reset();
		}

		public virtual void StopTankTurning()
		{
		}

		private void Turn(bool isInstant)
		{
			if (lookDirection == Vector3.zero)
			{
				return;
			}
			if (turnSpeed < 0f)
			{
				isInstant = true;
			}
			if (retroPathfinding && IsMovingAlongPath() && !IsTurningBeforeWalking())
			{
				isInstant = true;
			}
			if (isInstant)
			{
				turnFloat = 0f;
				Quaternion targetRotation = GetTargetRotation();
				if (motionControl != MotionControl.Manual)
				{
					TransformRotation = targetRotation;
				}
				newRotation = targetRotation;
				if (KickStarter.settingsManager != null && (bool)spriteChild)
				{
					PrepareSpriteChild(SceneSettings.IsTopDown(), SceneSettings.IsUnity2D());
				}
				SendMessage("OnSnapRotate", SendMessageOptions.DontRequireReceiver);
				return;
			}
			float num = Mathf.Atan2(lookDirection.x, lookDirection.z);
			float num2 = Mathf.Atan2(TransformForward.x, TransformForward.z);
			float num3 = num - num2;
			if (Mathf.Approximately(num3, 0f))
			{
				turnFloat = turnFloatLerp.Update(turnFloat, 0f, turnSpeed);
				newRotation = TransformRotation;
				return;
			}
			if (num3 < -(float)Math.PI)
			{
				num += (float)Math.PI * 2f;
				num3 += (float)Math.PI * 2f;
			}
			else if (num3 > (float)Math.PI)
			{
				num -= (float)Math.PI * 2f;
				num3 -= (float)Math.PI * 2f;
			}
			if (retroPathfinding && IsTurningBeforeWalking() && SceneSettings.IsUnity2D() && num * num2 < 0f && Mathf.Abs(num3) > (float)Math.PI / 2f)
			{
				if (num2 < 0f)
				{
					num -= (float)Math.PI * 2f;
					num3 -= (float)Math.PI * 2f;
				}
				else
				{
					num += (float)Math.PI * 2f;
					num3 += (float)Math.PI * 2f;
				}
			}
			float num4 = 0f;
			if (num3 > 0f)
			{
				num4 = Mathf.Min(num3 / 2f, 1f);
			}
			else if (num3 < 0f)
			{
				num4 = Mathf.Max(num3 / 2f, -1f);
			}
			turnFloat = turnFloatLerp.Update(turnFloat, turnSpeed * num4, turnSpeed);
			if ((bool)animEngine && animEngine.turningStyle == TurningStyle.Linear)
			{
				float num5 = newAngleLinearLerp.Update(num2, num, turnSpeed * GetScriptTurningFactor());
				newRotation = Quaternion.AngleAxis(num5 * 57.29578f, Vector3.up);
			}
			else
			{
				float num6 = newAngleLerp.Update(num2, num, turnSpeed * GetScriptTurningFactor());
				newRotation = Quaternion.AngleAxis(num6 * 57.29578f, Vector3.up);
			}
		}

		public float GetAngleDifference()
		{
			float num = Mathf.Atan2(lookDirection.x, lookDirection.z);
			float num2 = Mathf.Atan2(TransformForward.x, TransformForward.z);
			return num - num2;
		}

		private float GetNonFacingReductionFactor()
		{
			if ((bool)activePath)
			{
				Vector3 zero = Vector3.zero;
				if (SceneSettings.IsUnity2D())
				{
					Vector2 vector = GetTargetPosition() - base.transform.position;
					zero = new Vector3(vector.x, 0f, vector.y);
				}
				else
				{
					zero = GetSmartPosition(GetTargetPosition()) - base.transform.position;
				}
				float num = Vector3.Dot(TransformForward, zero.normalized);
				num *= num;
				return Mathf.Clamp01(num);
			}
			return 1f;
		}

		private void UpdateWallReductionFactor()
		{
			if (SceneSettings.CameraPerspective == CameraPerspective.TwoD)
			{
				Vector2 vector = TransformForward;
				if (SceneSettings.IsUnity2D())
				{
					vector = new Vector2(TransformForward.x, TransformForward.z);
				}
				Vector2 vector2 = (Vector2)base.transform.position + (Vector2)wallRayOrigin + vector * wallRayForward;
				RaycastHit2D raycastHit2D = UnityVersionHandler.Perform2DRaycast(vector2, vector, wallDistance, 1 << LayerMask.NameToLayer(wallLayer));
				if (raycastHit2D.collider != null)
				{
					wallReductionFactor = wallReductionLerp.Update(wallReductionFactor, (raycastHit2D.point - vector2).magnitude / wallDistance, 10f);
				}
				else
				{
					wallReductionFactor = wallReductionLerp.Update(wallReductionFactor, 1f, 10f);
				}
			}
			else
			{
				Vector3 vector3 = base.transform.position + wallRayOrigin + TransformForward * wallRayForward;
				RaycastHit hitInfo;
				if (Physics.Raycast(vector3, TransformForward, out hitInfo, wallDistance, 1 << LayerMask.NameToLayer(wallLayer)))
				{
					wallReductionFactor = wallReductionLerp.Update(wallReductionFactor, (hitInfo.point - vector3).magnitude / wallDistance, 10f);
				}
				else
				{
					wallReductionFactor = wallReductionLerp.Update(wallReductionFactor, 1f, 10f);
				}
			}
		}

		private float GetScriptTurningFactor()
		{
			if (_animator != null && _animator.applyRootMotion)
			{
				return 1f - rootTurningFactor;
			}
			return 1f;
		}

		private void DoTurn(bool fromFixedUpdate = false)
		{
			if (GetMotionControl() != MotionControl.Manual && !(lookDirection == Vector3.zero) && fromFixedUpdate == DoRigidbodyMovement() && !(GetScriptTurningFactor() <= 0f))
			{
				if (fromFixedUpdate && GetScriptTurningFactor() >= 1f)
				{
					_rigidbody.MoveRotation(newRotation);
				}
				else
				{
					TransformRotation = newRotation;
				}
			}
		}

		public void SetLookDirection(Vector3 _direction, bool isInstant)
		{
			lookDirection = new Vector3(_direction.x, 0f, _direction.z);
			if (KickStarter.settingsManager.rotationsAffectedByVerticalReduction && SceneSettings.IsUnity2D() && KickStarter.settingsManager.movementMethod == MovementMethod.PointAndClick && _direction != TransformForward)
			{
				lookDirection.z /= KickStarter.sceneSettings.GetVerticalReductionFactor();
			}
			if (isInstant)
			{
				Turn(isInstant);
			}
		}

		public void SetMoveDirection(Vector3 _direction)
		{
			if (_direction != Vector3.zero)
			{
				Quaternion quaternion = Quaternion.LookRotation(_direction, Vector3.up);
				moveDirection = quaternion * Vector3.forward;
				moveDirection.Normalize();
			}
		}

		public void SetMoveDirectionAsForward()
		{
			isReversing = false;
			moveDirection = TransformForward;
			if (SceneSettings.IsUnity2D())
			{
				moveDirection = new Vector3(moveDirection.x, moveDirection.z, 0f);
			}
			moveDirection.Normalize();
		}

		public void SetMoveDirectionAsBackward()
		{
			isReversing = true;
			moveDirection = -TransformForward;
			if (SceneSettings.IsUnity2D())
			{
				moveDirection = new Vector3(moveDirection.x, moveDirection.z, 0f);
			}
			moveDirection.Normalize();
		}

		private void SetAntiGlideState()
		{
			if (!animEngine.isSpriteBased || GetRootMotionType() != RootMotionType.None)
			{
				antiGlideMode = false;
			}
		}

		public Animation GetAnimation()
		{
			if (_animation == null)
			{
				_animation = GetComponent<Animation>();
			}
			return _animation;
		}

		public Animator GetAnimator()
		{
			if (_animator == null)
			{
				animatorIsOnRoot = false;
				if ((bool)spriteChild && (bool)spriteChild.GetComponent<Animator>())
				{
					_animator = spriteChild.GetComponent<Animator>();
				}
				else if (customAnimator != null)
				{
					_animator = customAnimator;
					animatorIsOnRoot = customAnimator.gameObject == base.gameObject;
				}
				else
				{
					_animator = GetComponent<Animator>();
					if (_animator != null)
					{
						animatorIsOnRoot = true;
					}
				}
			}
			return _animator;
		}

		public void ResetAnimator()
		{
			_animator = null;
		}

		private RootMotionType GetRootMotionType()
		{
			if (charState == CharState.Decelerate)
			{
			}
			if (_animator == null || !_animator.applyRootMotion)
			{
				return RootMotionType.None;
			}
			if (animEngine.isSpriteBased)
			{
				return RootMotionType.TwoD;
			}
			return RootMotionType.ThreeD;
		}

		public Vector3 GetMoveDirection()
		{
			return moveDirection;
		}

		private void SetNextNodes(bool justResumedPath = false)
		{
			lastDist = float.PositiveInfinity;
			pausePath = false;
			nodeActionList = null;
			int num = targetNode;
			if (IsPlayer && KickStarter.stateHandler.IsInGameplay())
			{
				targetNode = activePath.GetNextNode(targetNode, prevNode, true);
			}
			else
			{
				targetNode = activePath.GetNextNode(targetNode, prevNode, false);
			}
			prevNode = num;
			if (targetNode == 0 && activePath.pathType == AC_PathType.Loop && activePath.teleportToStart)
			{
				Teleport(activePath.transform.position);
				if (activePath.nodes.Count > 1)
				{
					SetLookDirection(activePath.nodes[1] - activePath.nodes[0], true);
				}
				SetNextNodes();
			}
			else if (targetNode == -1)
			{
				EndPath();
			}
			else if (justResumedPath && turnBeforeWalking)
			{
				TurnBeforeWalking();
			}
		}

		public bool WillStopAtNextNode()
		{
			if ((bool)activePath && activePath.WillStopAtNextNode(targetNode))
			{
				return true;
			}
			return false;
		}

		public bool IsPathfinding()
		{
			if (activePath != null && activePath == ownPath)
			{
				return true;
			}
			return false;
		}

		public void EndPath(Paths optionalPath)
		{
			if (optionalPath != null && activePath != null && activePath != optionalPath)
			{
				return;
			}
			StopTurning();
			if (activePath != null)
			{
				KickStarter.eventManager.Call_OnCharacterEndPath(this, activePath);
			}
			if (IsPathfinding())
			{
				activePath.nodes.Clear();
			}
			else
			{
				lastPathPrevNode = prevNode;
				lastPathTargetNode = targetNode;
				lastPathActivePath = activePath;
			}
			activePath = null;
			targetNode = 0;
			pathfindUpdateTime = 0f;
			if (charState == CharState.Move)
			{
				if (retroPathfinding)
				{
					Halt();
				}
				else
				{
					charState = CharState.Decelerate;
				}
			}
		}

		public void StartDecelerating()
		{
			if (retroPathfinding)
			{
				moveSpeed = 0f;
				exactDestination = defaultExactDestination;
				if (charState == CharState.Move || charState == CharState.Decelerate)
				{
					charState = CharState.Idle;
				}
			}
			else
			{
				charState = CharState.Decelerate;
			}
		}

		public void EndPath()
		{
			EndPath(null);
		}

		public void ResumeLastPath()
		{
			if (lastPathActivePath != null)
			{
				SetPath(lastPathActivePath, lastPathTargetNode, lastPathPrevNode);
			}
		}

		protected void SetLastPath(Paths _lastPathActivePath, int _lastPathTargetNode, int _lastPathPrevNode)
		{
			lastPathActivePath = _lastPathActivePath;
			lastPathTargetNode = _lastPathTargetNode;
			lastPathPrevNode = _lastPathPrevNode;
		}

		public void Halt(bool haltTurning = true)
		{
			if (!IsPathfinding())
			{
				lastPathPrevNode = prevNode;
				lastPathTargetNode = targetNode;
				lastPathActivePath = activePath;
			}
			activePath = null;
			targetNode = 0;
			moveSpeed = 0f;
			if (haltTurning)
			{
				StopTurning();
			}
			exactDestination = defaultExactDestination;
			if (charState == CharState.Move || charState == CharState.Decelerate)
			{
				charState = CharState.Idle;
			}
		}

		public void ForceIdle()
		{
			charState = CharState.Idle;
		}

		protected void ReverseDirection()
		{
			int num = targetNode;
			targetNode = prevNode;
			prevNode = num;
		}

		private void PausePath(float pauseTime)
		{
			StartDecelerating();
			pausePath = true;
			pausePathTime = Time.time + pauseTime;
			nodeActionList = null;
		}

		private void PausePath(float pauseTime, Cutscene pauseCutscene)
		{
			StartDecelerating();
			pausePath = true;
			if (pauseTime > 0f)
			{
				pausePathTime = Time.time + pauseTime + 1f;
				StartCoroutine(DelayPathCutscene(pauseTime, pauseCutscene));
			}
			else
			{
				pausePathTime = 0f;
				pauseCutscene.Interact();
				nodeActionList = pauseCutscene;
			}
		}

		private IEnumerator DelayPathCutscene(float pauseTime, Cutscene pauseCutscene)
		{
			yield return new WaitForSeconds(pauseTime);
			pausePathTime = 0f;
			pauseCutscene.Interact();
			nodeActionList = pauseCutscene;
		}

		private void PausePath(float pauseTime, ActionListAsset pauseAsset)
		{
			StartDecelerating();
			pausePath = true;
			if (pauseTime > 0f)
			{
				pausePathTime = Time.time + pauseTime + 1f;
				StartCoroutine(DelayPathActionList(pauseTime, pauseAsset));
			}
			else
			{
				pausePathTime = 0f;
				nodeActionList = AdvGame.RunActionListAsset(pauseAsset);
			}
		}

		private IEnumerator DelayPathActionList(float pauseTime, ActionListAsset pauseAsset)
		{
			yield return new WaitForSeconds(pauseTime);
			pausePathTime = 0f;
			nodeActionList = AdvGame.RunActionListAsset(pauseAsset);
		}

		private void TurnBeforeWalking()
		{
			if (retroPathfinding && (charState == CharState.Move || charState == CharState.Decelerate))
			{
				charState = CharState.Idle;
			}
			Vector3 vector = activePath.nodes[1] - base.transform.position;
			if (SceneSettings.IsUnity2D())
			{
				SetLookDirection(new Vector3(vector.x, 0f, vector.y), false);
			}
			else
			{
				SetLookDirection(new Vector3(vector.x, 0f, vector.z), false);
			}
			isTurningBeforeWalking = true;
			Turn(false);
		}

		public bool IsTurningBeforeWalking()
		{
			if (IsTurning() && isTurningBeforeWalking && !Mathf.Approximately(turnSpeed, 0f))
			{
				return true;
			}
			isTurningBeforeWalking = false;
			return false;
		}

		private bool CanTurnBeforeMoving()
		{
			if (turnBeforeWalking && IsPathfinding() && targetNode <= 1 && activePath.nodes.Count > 1)
			{
				return true;
			}
			return false;
		}

		private void SetPath(Paths pathOb, PathSpeed _speed, int _targetNode, int _prevNode)
		{
			if (pathOb != activePath)
			{
				pausePath = false;
				nodeActionList = null;
			}
			activePath = pathOb;
			targetNode = _targetNode;
			prevNode = _prevNode;
			exactDestination = ((pathOb.pathType != AC_PathType.ReverseOnly) ? pathOb.nodes[pathOb.nodes.Count - 1] : pathOb.transform.position);
			if (CanTurnBeforeMoving())
			{
				TurnBeforeWalking();
			}
			if ((bool)pathOb)
			{
				if (_speed == PathSpeed.Run)
				{
					isRunning = true;
				}
				else
				{
					isRunning = false;
				}
			}
			if (charState == CharState.Custom)
			{
				charState = CharState.Idle;
			}
			pathfindUpdateTime = 0f;
			if (pathOb != null)
			{
				KickStarter.eventManager.Call_OnCharacterSetPath(this, pathOb);
			}
		}

		public void SetPath(Paths pathOb, PathSpeed _speed)
		{
			if (pathOb != null && pathOb.nodes.Count > 0)
			{
				if (pathOb.nodes.Count != 1 || !(pathOb.nodes[0] == base.transform.position))
				{
					int num = ((pathOb == ownPath) ? 1 : 0);
					SetPath(pathOb, _speed, num, 0);
				}
			}
			else
			{
				SetPath(pathOb, _speed, 0, 0);
			}
		}

		public void SetPath(Paths pathOb)
		{
			if (!(pathOb != null))
			{
				return;
			}
			if (pathOb.nodes.Count > 0)
			{
				int num = ((pathOb == ownPath) ? 1 : 0);
				if (pathOb.pathType == AC_PathType.ReverseOnly)
				{
					num = pathOb.nodes.Count - 1;
				}
				SetPath(pathOb, pathOb.pathSpeed, num, 0);
			}
			else
			{
				SetPath(pathOb, pathOb.pathSpeed, 0, 0);
			}
		}

		public void SetPath(Paths pathOb, int _targetNode, int _prevNode)
		{
			if (pathOb != null)
			{
				SetPath(pathOb, pathOb.pathSpeed, _targetNode, _prevNode);
			}
		}

		public void SetPath(Paths pathOb, int _targetNode, int _prevNode, bool affectY)
		{
			if ((bool)pathOb)
			{
				SetPath(pathOb, pathOb.pathSpeed, _targetNode, _prevNode);
				activePath.affectY = affectY;
			}
		}

		public Paths GetPath()
		{
			return activePath;
		}

		public int GetTargetNode()
		{
			return targetNode;
		}

		public int GetPreviousNode()
		{
			return prevNode;
		}

		protected Paths GetLastPath()
		{
			return lastPathActivePath;
		}

		protected int GetLastTargetNode()
		{
			return lastPathTargetNode;
		}

		protected int GetLastPrevNode()
		{
			return lastPathPrevNode;
		}

		public void MoveToPoint(Vector3 point, bool run = false, bool usePathfinding = false)
		{
			if (KickStarter.navigationManager == null)
			{
				usePathfinding = false;
			}
			if (usePathfinding)
			{
				Vector3[] array = null;
				array = KickStarter.navigationManager.navigationEngine.GetPointsArray(base.transform.position, point, this);
				MoveAlongPoints(array, run);
			}
			else
			{
				List<Vector3> list = new List<Vector3>();
				list.Add(point);
				MoveAlongPoints(list.ToArray(), run, false);
			}
		}

		public void MoveAlongPoints(Vector3[] pointData, bool run, bool allowUpdating = true)
		{
			if (pointData.Length == 0)
			{
				return;
			}
			Paths paths = ownPath;
			if ((bool)paths)
			{
				paths.BuildNavPath(pointData);
				if (run)
				{
					SetPath(paths, PathSpeed.Run);
				}
				else
				{
					SetPath(paths, PathSpeed.Walk);
				}
				if (allowUpdating)
				{
					pathfindUpdateTime = Mathf.Max(0f, KickStarter.settingsManager.pathfindUpdateFrequency);
				}
				else
				{
					pathfindUpdateTime = -1f;
				}
			}
			else
			{
				ACDebug.LogWarning(base.name + " cannot pathfind without a Paths component", base.gameObject);
			}
		}

		public void ResetBaseClips()
		{
			if ((bool)spriteChild && (bool)spriteChild.GetComponent<Animation>())
			{
				List<string> list = new List<string>();
				foreach (AnimationState item in spriteChild.GetComponent<Animation>())
				{
					if ((idleAnim == null || item.name != idleAnim.name) && (walkAnim == null || item.name != walkAnim.name) && (runAnim == null || item.name != runAnim.name))
					{
						list.Add(item.name);
					}
				}
				foreach (string item2 in list)
				{
					spriteChild.GetComponent<Animation>().RemoveClip(item2);
				}
			}
			if (!_animation)
			{
				return;
			}
			List<string> list2 = new List<string>();
			foreach (AnimationState item3 in _animation)
			{
				if ((idleAnim == null || item3.name != idleAnim.name) && (walkAnim == null || item3.name != walkAnim.name) && (runAnim == null || item3.name != runAnim.name))
				{
					list2.Add(item3.name);
				}
			}
			foreach (string item4 in list2)
			{
				_animation.RemoveClip(item4);
			}
		}

		public float GetSpriteAngle()
		{
			return spriteAngle;
		}

		private string GetSpriteDirectionSuffix(bool ignoreFrameFlipping = false)
		{
			if (ignoreFrameFlipping && flipFrames)
			{
				if (spriteDirection.Contains("L"))
				{
					return spriteDirection.Replace("L", "R");
				}
				if (spriteDirection.Contains("R"))
				{
					return spriteDirection.Replace("R", "L");
				}
			}
			return spriteDirection;
		}

		public string GetSpriteDirection(bool ignoreFrameFlipping = false)
		{
			return "_" + GetSpriteDirectionSuffix(ignoreFrameFlipping);
		}

		public int GetSpriteDirectionInt(bool ignoreFrameFlipping = false)
		{
			switch (GetSpriteDirectionSuffix(ignoreFrameFlipping))
			{
			case "D":
				return 0;
			case "L":
				return 1;
			case "R":
				return 2;
			case "U":
				return 3;
			case "DL":
				return 4;
			case "DR":
				return 5;
			case "UL":
				return 6;
			case "UR":
				return 7;
			default:
				return 0;
			}
		}

		public void SetSpriteDirection(CharDirection direction)
		{
			lockDirection = true;
			switch (direction)
			{
			case CharDirection.Down:
				spriteDirection = "D";
				break;
			case CharDirection.Left:
				spriteDirection = "L";
				break;
			case CharDirection.Right:
				spriteDirection = "R";
				break;
			case CharDirection.Up:
				spriteDirection = "U";
				break;
			case CharDirection.DownLeft:
				spriteDirection = "DL";
				break;
			case CharDirection.DownRight:
				spriteDirection = "DR";
				break;
			case CharDirection.UpLeft:
				spriteDirection = "UL";
				break;
			case CharDirection.UpRight:
				spriteDirection = "UR";
				break;
			}
			UpdateFrameFlipping();
		}

		private void CalcHeightChange()
		{
			float y = base.transform.position.y;
			if (!Mathf.Approximately(y, prevHeight) && !Mathf.Approximately(y, prevHeight2) && !Mathf.Approximately(prevHeight, prevHeight2))
			{
				heightChange = y - prevHeight;
			}
			else
			{
				heightChange = 0f;
			}
			prevHeight2 = prevHeight;
			prevHeight = y;
		}

		private void StopStandardAudio()
		{
			if ((bool)audioSource && audioSource.isPlaying && (((bool)runSound && audioSource.clip == runSound) || ((bool)walkSound && audioSource.clip == walkSound)))
			{
				audioSource.Stop();
			}
		}

		private void PlayStandardAudio()
		{
			if (!audioSource)
			{
				return;
			}
			if (isRunning && (bool)runSound)
			{
				if ((!audioSource.isPlaying || !(audioSource.clip == runSound)) && IsGrounded())
				{
					audioSource.loop = false;
					audioSource.clip = runSound;
					audioSource.Play();
					if (KickStarter.eventManager != null)
					{
						KickStarter.eventManager.Call_OnPlayFootstepSound(this, null, false, audioSource, runSound);
					}
				}
			}
			else if ((bool)walkSound && (!audioSource.isPlaying || !(audioSource.clip == walkSound)) && IsGrounded())
			{
				audioSource.loop = false;
				audioSource.clip = walkSound;
				audioSource.Play();
				if (KickStarter.eventManager != null)
				{
					KickStarter.eventManager.Call_OnPlayFootstepSound(this, null, true, audioSource, walkSound);
				}
			}
		}

		private void ResetAnimationEngine()
		{
			string text = "AnimEngine";
			if (animationEngine == AnimationEngine.Custom)
			{
				if (!string.IsNullOrEmpty(customAnimationClass))
				{
					text = customAnimationClass;
				}
			}
			else
			{
				text = text + "_" + animationEngine;
			}
			if (!(animEngine == null) && !(animEngine.ToString() != text))
			{
				return;
			}
			try
			{
				animEngine = (AnimEngine)ScriptableObject.CreateInstance(text);
				if (animEngine != null)
				{
					animEngine.Declare(this);
				}
			}
			catch
			{
			}
		}

		public void InitSpriteChild()
		{
			if (animEngine == null)
			{
				ResetAnimationEngine();
			}
			PrepareSpriteChild(SceneSettings.IsTopDown(), SceneSettings.IsUnity2D());
			UpdateSpriteChild(SceneSettings.IsTopDown(), SceneSettings.IsUnity2D());
		}

		protected void PrepareSpriteChild(bool isTopDown, bool isUnity2D)
		{
			float num = 0f;
			float num2 = 0f;
			if (isTopDown || isUnity2D)
			{
				num = Vector3.Dot(Vector3.forward, TransformForward.normalized);
				num2 = Vector3.Dot(Vector3.right, TransformForward.normalized);
			}
			else
			{
				num = Vector3.Dot(KickStarter.mainCamera.ForwardVector().normalized, TransformForward.normalized);
				num2 = Vector3.Dot(KickStarter.mainCamera.RightVector().normalized, TransformForward.normalized);
			}
			spriteAngle = Mathf.Atan(num2 / num) * 57.29578f;
			if (num > 0f)
			{
				spriteAngle += 180f;
			}
			else if (num2 > 0f)
			{
				spriteAngle += 360f;
			}
			if (Mathf.Approximately(spriteAngle, 360f))
			{
				spriteAngle = 0f;
			}
			if (charState == CharState.Custom && !flipCustomAnims)
			{
				flipFrames = false;
			}
			else
			{
				if (!lockDirection)
				{
					spriteDirection = spriteDirectionData.GetDirectionalSuffix(spriteAngle);
				}
				UpdateFrameFlipping();
			}
			if (angleSnapping != AngleSnapping.None)
			{
				spriteAngle = FlattenSpriteAngle(spriteAngle, angleSnapping);
			}
		}

		private void UpdateFrameFlipping()
		{
			if (!spriteDirectionData.HasDirections())
			{
				flipFrames = false;
			}
			else if (frameFlipping == AC_2DFrameFlipping.LeftMirrorsRight && spriteDirection.Contains("L"))
			{
				spriteDirection = spriteDirection.Replace("L", "R");
				flipFrames = true;
			}
			else if (frameFlipping == AC_2DFrameFlipping.RightMirrorsLeft && spriteDirection.Contains("R"))
			{
				spriteDirection = spriteDirection.Replace("R", "L");
				flipFrames = true;
			}
			else
			{
				flipFrames = false;
			}
		}

		public float FlattenSpriteAngle(float angle, AngleSnapping _angleSnapping)
		{
			switch (_angleSnapping)
			{
			case AngleSnapping.FortyFiveDegrees:
				if (angle > 337.5f)
				{
					return 360f;
				}
				if (angle > 292.5f)
				{
					return 315f;
				}
				if (angle > 247.5f)
				{
					return 270f;
				}
				if (angle > 202.5f)
				{
					return 225f;
				}
				if (angle > 157.5f)
				{
					return 180f;
				}
				if (angle > 112.5f)
				{
					return 135f;
				}
				if (angle > 67.5f)
				{
					return 90f;
				}
				if (angle > 22.5f)
				{
					return 45f;
				}
				return 0f;
			case AngleSnapping.NinetyDegrees:
				if (angle > 315f)
				{
					return 360f;
				}
				if (angle > 225f)
				{
					return 270f;
				}
				if (angle > 135f)
				{
					return 180f;
				}
				if (angle > 45f)
				{
					return 90f;
				}
				return 0f;
			default:
				return angle;
			}
		}

		protected void UpdateSpriteChild(bool isTopDown, bool isUnity2D)
		{
			if (frameFlipping != AC_2DFrameFlipping.None && ((flipFrames && spriteChild.localScale.x > 0f) || (!flipFrames && spriteChild.localScale.x < 0f)))
			{
				spriteChild.localScale = new Vector3(0f - spriteChild.localScale.x, spriteChild.localScale.y, spriteChild.localScale.z);
			}
			if (isTopDown)
			{
				if ((bool)animEngine && !animEngine.isSpriteBased)
				{
					spriteChild.rotation = TransformRotation;
					spriteChild.RotateAround(base.transform.position, Vector3.right, 90f);
				}
				else
				{
					spriteChild.rotation = Quaternion.Euler(90f, 0f, spriteChild.localEulerAngles.z);
				}
			}
			else if (isUnity2D)
			{
				spriteChild.rotation = Quaternion.Euler(0f, 0f, spriteChild.localEulerAngles.z);
			}
			else if (rotateSprite3D == RotateSprite3D.RelativePositionToCamera)
			{
				Vector3 normalized = (base.transform.position - KickStarter.mainCamera.transform.position).normalized;
				spriteChild.forward = normalized;
			}
			else if (rotateSprite3D == RotateSprite3D.CameraFacingDirection)
			{
				spriteChild.rotation = Quaternion.Euler(spriteChild.rotation.eulerAngles.x, KickStarter.mainCamera.transform.rotation.eulerAngles.y, spriteChild.rotation.eulerAngles.z);
			}
			else if (rotateSprite3D == RotateSprite3D.FullCameraRotation)
			{
				spriteChild.rotation = Quaternion.Euler(KickStarter.mainCamera.transform.rotation.eulerAngles.x, KickStarter.mainCamera.transform.rotation.eulerAngles.y, KickStarter.mainCamera.transform.rotation.eulerAngles.z);
			}
		}

		protected void UpdateScale()
		{
			if (!followSortingMap)
			{
				return;
			}
			if (!lockScale)
			{
				spriteScale = followSortingMap.GetLocalScale();
			}
			if (spriteScale > 0f)
			{
				base.transform.localScale = originalScale * spriteScale;
				if (lockScale)
				{
					sortingMapScale = spriteScale;
				}
				else
				{
					sortingMapScale = followSortingMap.GetLocalSpeed();
				}
			}
		}

		public void SetSorting(int order)
		{
			if ((bool)followSortingMap)
			{
				followSortingMap.LockSortingOrder(order);
			}
			else if ((bool)GetComponentInChildren<Renderer>())
			{
				GetComponentInChildren<Renderer>().sortingOrder = order;
			}
		}

		public void SetSorting(string layer)
		{
			if ((bool)followSortingMap)
			{
				followSortingMap.LockSortingLayer(layer);
			}
			else if ((bool)GetComponentInChildren<Renderer>())
			{
				GetComponentInChildren<Renderer>().sortingLayerName = layer;
			}
		}

		public void ReleaseSorting()
		{
			if ((bool)followSortingMap)
			{
				followSortingMap.lockSorting = false;
			}
		}

		public float GetMoveSpeed(bool reduceNonFacing = false)
		{
			if (isExactLerping && charState == CharState.Decelerate)
			{
				return 0f;
			}
			if (isReversing)
			{
				return (0f - moveSpeed) * reverseSpeedFactor;
			}
			if (reduceNonFacing)
			{
				float nonFacingReductionFactor = GetNonFacingReductionFactor();
				nonFacingFactor = nonFacingFactorLerp.Update(nonFacingFactor, nonFacingReductionFactor, 10f);
			}
			return moveSpeed * ((!reduceNonFacing) ? 1f : nonFacingFactor) * ((!doWallReduction) ? 1f : wallReductionFactor);
		}

		public virtual void SetHeadTurnTarget(Transform _headTurnTarget, Vector3 _headTurnTargetOffset, bool isInstant, HeadFacing _headFacing = HeadFacing.Manual)
		{
			if (_headFacing != HeadFacing.Hotspot || headFacing != HeadFacing.Manual)
			{
				bool flag = headTurnTarget != _headTurnTarget || headTurnTargetOffset != _headTurnTargetOffset || headFacing == HeadFacing.None;
				headTurnTarget = _headTurnTarget;
				headTurnTargetOffset = _headTurnTargetOffset;
				headFacing = _headFacing;
				if (isInstant)
				{
					CalculateHeadTurn();
					SnapHeadMovement();
				}
				if (flag || isInstant)
				{
					KickStarter.eventManager.Call_OnSetHeadTurnTarget(this, headTurnTarget, headTurnTargetOffset, isInstant);
				}
			}
		}

		public void ClearHeadTurnTarget(bool isInstant, HeadFacing _headFacing)
		{
			if (headFacing == _headFacing)
			{
				ClearHeadTurnTarget(isInstant);
			}
		}

		public void ClearHeadTurnTarget(bool isInstant)
		{
			headFacing = HeadFacing.None;
			if (isInstant)
			{
				targetHeadAngles = Vector2.zero;
				SnapHeadMovement();
				headTurnWeight = 0f;
			}
			KickStarter.eventManager.Call_OnClearHeadTurnTarget(this, isInstant);
		}

		public void SnapHeadMovement()
		{
			headTurnWeight = 1f;
			actualHeadAngles = targetHeadAngles;
			AnimateHeadTurn();
		}

		public bool IsMovingHead()
		{
			if (Mathf.Abs(actualHeadAngles.x - targetHeadAngles.x) < 0.1f && Mathf.Abs(actualHeadAngles.y - targetHeadAngles.y) < 0.1f)
			{
				return false;
			}
			return true;
		}

		public Shapeable GetShapeable()
		{
			Shapeable shapeable = GetComponent<Shapeable>();
			if (shapeable == null)
			{
				shapeable = GetComponentInChildren<Shapeable>();
			}
			return shapeable;
		}

		public Vector3 GetHeadTurnTarget()
		{
			if (headFacing != HeadFacing.None && headTurnTarget != null)
			{
				return headTurnTarget.position + headTurnTargetOffset;
			}
			return Vector3.zero;
		}

		private void CalculateHeadTurn()
		{
			if (headFacing == HeadFacing.None || headTurnTarget == null)
			{
				targetHeadAngles = targetHeadAnglesLerp.Update(targetHeadAngles, Vector2.zero, headTurnSpeed);
				headTurnWeight = headTurnWeightLerp.Update(headTurnWeight, 0f, headTurnSpeed);
				return;
			}
			headTurnWeight = headTurnWeightLerp.Update(headTurnWeight, 1f, headTurnSpeed);
			Vector3 vector = ((!SceneSettings.IsUnity2D()) ? (headTurnTarget.position + headTurnTargetOffset - base.transform.position) : (new Vector3(headTurnTarget.position.x - base.transform.position.x, 0f, headTurnTarget.position.y - base.transform.position.y) + headTurnTargetOffset));
			vector.y = 0f;
			targetHeadAngles.x = Vector3.Angle(TransformForward, vector);
			targetHeadAngles.x = Mathf.Min(targetHeadAngles.x, (!animEngine.isSpriteBased) ? 70f : 100f);
			Vector3 lhs = Vector3.Cross(TransformForward, vector);
			float num = Vector3.Dot(lhs, Vector2.up);
			if (num < 0f)
			{
				targetHeadAngles.x *= -1f;
			}
			Vector3 vector2 = headTurnTarget.position + headTurnTargetOffset;
			if (neckBone != null)
			{
				vector2 -= neckBone.position;
			}
			else
			{
				vector2 -= base.transform.position;
				if (_characterController != null)
				{
					vector2 -= new Vector3(0f, _characterController.height * base.transform.localScale.y * 0.8f, 0f);
				}
				else if (capsuleCollider != null)
				{
					vector2 -= new Vector3(0f, capsuleCollider.height * base.transform.localScale.y * 0.8f, 0f);
				}
			}
			targetHeadAngles.y = Vector3.Angle(vector2, vector);
			targetHeadAngles.y = Mathf.Min(targetHeadAngles.y, 50f);
			if (vector2.y < vector.y)
			{
				targetHeadAngles.y *= -1f;
			}
			if (!ikHeadTurning)
			{
				targetHeadAngles.x /= 60f;
				targetHeadAngles.y *= Vector3.Dot(TransformForward, vector.normalized) / 2f + 0.5f;
				targetHeadAngles.y /= 30f;
			}
		}

		private void AnimateHeadTurn()
		{
			if (!ikHeadTurning)
			{
				GetAnimEngine().TurnHead(actualHeadAngles);
			}
			float num = ((!Mathf.Approximately(headTurnWeight, 0f) || !(KickStarter.stateHandler != null) || !KickStarter.stateHandler.IsInGameplay()) ? 1.25f : 0.75f);
			actualHeadAngles = actualHeadAnglesLerp.Update(actualHeadAngles, targetHeadAngles, headTurnSpeed * num);
		}

		public bool HoldObject(GameObject objectToHold, Hand hand)
		{
			if (objectToHold == null)
			{
				return false;
			}
			Transform transform;
			if (hand == Hand.Left)
			{
				transform = leftHandBone;
				leftHandHeldObject = objectToHold;
			}
			else
			{
				transform = rightHandBone;
				rightHandHeldObject = objectToHold;
			}
			if ((bool)transform)
			{
				objectToHold.transform.parent = transform;
				objectToHold.transform.localPosition = Vector3.zero;
				return true;
			}
			ACDebug.Log("Cannot parent object - no hand bone found.", base.gameObject);
			return false;
		}

		public void ReleaseHeldObjects()
		{
			if (leftHandHeldObject != null && leftHandHeldObject.transform.IsChildOf(base.transform))
			{
				leftHandHeldObject.transform.parent = null;
			}
			if (rightHandHeldObject != null && rightHandHeldObject.transform.IsChildOf(base.transform))
			{
				rightHandHeldObject.transform.parent = null;
			}
		}

		public Vector3 GetSpeechWorldPosition()
		{
			Vector3 position = base.transform.position;
			if (speechMenuPlacement != null)
			{
				position = speechMenuPlacement.position;
			}
			else if ((bool)_collider && _collider is CapsuleCollider)
			{
				CapsuleCollider capsuleCollider = (CapsuleCollider)_collider;
				float num = capsuleCollider.height * base.transform.localScale.y;
				if (_spriteRenderer != null)
				{
					num *= spriteChild.localScale.y;
				}
				if (SceneSettings.IsTopDown())
				{
					position.z += num;
				}
				else
				{
					position.y += num;
				}
			}
			else if (_characterController != null)
			{
				float num2 = _characterController.height * base.transform.localScale.y;
				if (SceneSettings.IsTopDown())
				{
					position.z += num2;
				}
				else
				{
					position.y += num2;
				}
			}
			else if (spriteChild != null)
			{
				if (_spriteRenderer != null)
				{
					position.y = _spriteRenderer.bounds.extents.y + _spriteRenderer.bounds.center.y;
				}
				else if ((bool)spriteChild.GetComponent<Renderer>())
				{
					position.y = spriteChild.GetComponent<Renderer>().bounds.extents.y + spriteChild.GetComponent<Renderer>().bounds.center.y;
				}
			}
			return position;
		}

		public Vector2 GetSpeechScreenPosition(bool keepWithinScreen = true)
		{
			Vector3 speechWorldPosition = GetSpeechWorldPosition();
			Vector3 vector = KickStarter.CameraMain.WorldToViewportPoint(speechWorldPosition);
			if (vector.z < 0f)
			{
				if (!keepWithinScreen)
				{
					vector.x = 5f;
				}
				else
				{
					vector.x = 0.5f - vector.x;
				}
			}
			return new Vector3(vector.x, 1f - vector.y, vector.z);
		}

		private bool DoRigidbodyMovement()
		{
			if ((bool)_rigidbody && useRigidbodyForMovement && followSortingMap == null)
			{
				if (retroPathfinding && IsMovingAlongPath())
				{
					return false;
				}
				return true;
			}
			return false;
		}

		private bool DoRigidbodyMovement2D()
		{
			if ((bool)_rigidbody2D && !antiGlideMode && useRigidbody2DForMovement)
			{
				if (retroPathfinding && IsMovingAlongPath())
				{
					return false;
				}
				return true;
			}
			return false;
		}

		private float GetTargetDistance()
		{
			if (activePath != null && activePath.nodes.Count > targetNode)
			{
				return (activePath.nodes[targetNode] - base.transform.position).magnitude;
			}
			return 0f;
		}

		public void AfterLoad()
		{
			headFacing = HeadFacing.None;
			lockDirection = false;
			lockScale = false;
			isLipSyncing = false;
			lipSyncShapes.Clear();
			ReleaseSorting();
		}

		public void StartLipSync(List<LipSyncShape> _lipSyncShapes)
		{
			lipSyncShapes = _lipSyncShapes;
			isLipSyncing = true;
		}

		public int GetLipSyncFrame()
		{
			if (isTalking && lipSyncShapes.Count > 0)
			{
				return lipSyncShapes[0].frame;
			}
			return 0;
		}

		public float GetLipSyncNormalised()
		{
			if (lipSyncShapes.Count > 0)
			{
				return (float)lipSyncShapes[0].frame / (float)(KickStarter.speechManager.phonemes.Count - 1);
			}
			return 0f;
		}

		public bool LipSyncGameObject()
		{
			if (isLipSyncing && KickStarter.speechManager.lipSyncOutput == LipSyncOutput.PortraitAndGameObject)
			{
				return true;
			}
			return false;
		}

		private void ProcessLipSync()
		{
			if (lipSyncShapes.Count <= 0 || !(Time.time > lipSyncShapes[0].timeIndex))
			{
				return;
			}
			if (KickStarter.speechManager.lipSyncOutput == LipSyncOutput.PortraitAndGameObject && (bool)shapeable)
			{
				if (lipSyncShapes.Count > 1)
				{
					float num = lipSyncShapes[1].timeIndex - lipSyncShapes[0].timeIndex;
					shapeable.SetActiveKey(lipSyncGroupID, lipSyncShapes[1].frame, 100f, num * lipSyncBlendShapeSpeedFactor, MoveMethod.Smooth, null);
				}
				else
				{
					shapeable.SetActiveKey(lipSyncGroupID, 0, 100f, 0.2f, MoveMethod.Smooth, null);
				}
			}
			else if (KickStarter.speechManager.lipSyncOutput == LipSyncOutput.GameObjectTexture && (bool)lipSyncTexture)
			{
				lipSyncTexture.SetFrame(lipSyncShapes[0].frame);
			}
			lipSyncShapes.RemoveAt(0);
		}

		public void StopSpeaking()
		{
			isTalking = false;
			if ((bool)_animation && !isLipSyncing)
			{
				foreach (AnimationState item in _animation)
				{
					if (item.layer == 7)
					{
						item.normalizedTime = 1f;
						item.weight = 0f;
					}
				}
			}
			if (shapeable != null && KickStarter.speechManager.lipSyncOutput == LipSyncOutput.PortraitAndGameObject)
			{
				shapeable.DisableAllKeys(lipSyncGroupID, 0.1f, MoveMethod.Curved, null);
			}
			else if (lipSyncTexture != null && KickStarter.speechManager.lipSyncOutput == LipSyncOutput.GameObjectTexture)
			{
				lipSyncTexture.SetFrame(0);
			}
			if (KickStarter.speechManager.lipSyncMode == LipSyncMode.RogoLipSync)
			{
				RogoLipSyncIntegration.Stop(this);
			}
			else if (KickStarter.speechManager.lipSyncMode == LipSyncMode.FaceFX)
			{
				FaceFXIntegration.Stop(this);
			}
			lipSyncShapes.Clear();
			isLipSyncing = false;
			if ((bool)speechAudioSource)
			{
				speechAudioSource.Stop();
			}
		}

		public int GetExpressionID(string expressionLabel)
		{
			if (expressionLabel == "None" || expressionLabel == "none")
			{
				return 99;
			}
			foreach (Expression expression in expressions)
			{
				if (expression.label == expressionLabel)
				{
					return expression.ID;
				}
			}
			return -1;
		}

		public int GetExpressionID()
		{
			if (currentExpression != null)
			{
				return currentExpression.ID;
			}
			return 0;
		}

		public void ClearExpression()
		{
			if (currentExpression != null)
			{
				currentExpression = null;
				if ((bool)animEngine)
				{
					animEngine.OnSetExpression();
				}
			}
			if (portraitIcon != null)
			{
				portraitIcon.Reset();
			}
			for (int i = 0; i < expressions.Count; i++)
			{
				if (expressions[i].portraitIcon != null)
				{
					expressions[i].portraitIcon.Reset();
				}
			}
		}

		public void SetExpression(int ID)
		{
			currentExpression = null;
			if (ID == 99)
			{
				if ((bool)animEngine)
				{
					animEngine.OnSetExpression();
				}
				return;
			}
			foreach (Expression expression in expressions)
			{
				if (expression.ID != ID)
				{
					continue;
				}
				if (currentExpression != expression)
				{
					currentExpression = expression;
					if ((bool)animEngine)
					{
						animEngine.OnSetExpression();
					}
				}
				return;
			}
			ACDebug.LogWarning("Cannot find expression with ID=" + ID + " on character " + base.gameObject.name, base.gameObject);
		}

		public CursorIconBase GetPortrait()
		{
			if (useExpressions && currentExpression != null && currentExpression.portraitIcon.texture != null)
			{
				return currentExpression.portraitIcon;
			}
			return portraitIcon;
		}

		public bool IsGrounded(bool reportError = false)
		{
			if (_characterController != null)
			{
				return _characterController.isGrounded;
			}
			if (_collider != null && _collider.enabled)
			{
				if (this.capsuleCollider != null && this.capsuleCollider.direction != 1)
				{
					return Physics.CheckCapsule(base.transform.position + new Vector3(0f, _collider.bounds.size.x / 2f, 0f), base.transform.position + new Vector3(0f, _collider.bounds.size.x / 4f, 0f), _collider.bounds.size.y * 0.45f, groundCheckLayerMask);
				}
				return Physics.CheckCapsule(base.transform.position + new Vector3(0f, _collider.bounds.size.y - _collider.bounds.size.x / 2f, 0f), base.transform.position + new Vector3(0f, _collider.bounds.size.x / 4f, 0f), _collider.bounds.size.x * 0.45f, groundCheckLayerMask);
			}
			if (_rigidbody != null && Mathf.Abs(_rigidbody.linearVelocity.y) > 0.1f)
			{
				return false;
			}
			if (spriteChild != null && GetAnimEngine().isSpriteBased)
			{
				return !isJumping;
			}
			if (KickStarter.settingsManager.movementMethod == MovementMethod.FirstPerson && capsuleColliders != null)
			{
				CapsuleCollider[] array = capsuleColliders;
				foreach (CapsuleCollider capsuleCollider in array)
				{
					if (!capsuleCollider.isTrigger && capsuleCollider.enabled)
					{
						return Physics.CheckCapsule(base.transform.position + new Vector3(0f, _collider.bounds.size.y - _collider.bounds.size.x / 2f, 0f), base.transform.position + new Vector3(0f, _collider.bounds.size.x / 4f, 0f), _collider.bounds.size.x / 2f, groundCheckLayerMask);
					}
				}
			}
			if (reportError)
			{
				ACDebug.LogWarning("Cannot determine if character '" + base.name + "' is grounded - consider adding a Capsule Collider component to them.", this);
			}
			return false;
		}

		public virtual bool CanBeDirectControlled()
		{
			return false;
		}

		public MotionControl GetMotionControl()
		{
			return motionControl;
		}

		public Vector3 GetTargetPosition(bool wantFinalDestination = false)
		{
			if ((bool)activePath)
			{
				if (wantFinalDestination)
				{
					if (activePath.nodes.Count > 0)
					{
						return activePath.nodes[activePath.nodes.Count - 1];
					}
				}
				else if (targetNode >= 0 && activePath.nodes.Count > targetNode)
				{
					return activePath.nodes[targetNode];
				}
			}
			return base.transform.position;
		}

		public Quaternion GetTargetRotation()
		{
			return Quaternion.LookRotation(lookDirection, Vector3.up);
		}

		public Quaternion GetFrameRotation()
		{
			return newRotation;
		}

		public AnimEngine GetAnimEngine()
		{
			if (animEngine == null)
			{
				ResetAnimationEngine();
			}
			else if ((animationEngine != AnimationEngine.Custom || string.IsNullOrEmpty(customAnimationClass) || !animEngine.ToString().EndsWith(customAnimationClass + ")")) && (animationEngine == AnimationEngine.Custom || !animEngine.ToString().EndsWith(animationEngine.ToString() + ")")))
			{
				ResetAnimationEngine();
			}
			return animEngine;
		}

		public void SetAnimEngine(AnimationEngine _animationEngine, string customClassName = "")
		{
			animationEngine = _animationEngine;
			if (animationEngine == AnimationEngine.Custom)
			{
				customAnimationClass = customClassName;
			}
			ResetAnimationEngine();
		}

		public float GetHeightChange()
		{
			return heightChange;
		}

		public bool IsReversing()
		{
			return isReversing;
		}

		public virtual float GetTurnFloat()
		{
			return turnFloat;
		}

		public bool IsTurning()
		{
			if (lookDirection == Vector3.zero || Quaternion.Angle(Quaternion.LookRotation(lookDirection), TransformRotation) < turningAngleThreshold)
			{
				return false;
			}
			return true;
		}

		public bool IsMovingAlongPath()
		{
			if (activePath != null && !pausePath)
			{
				return true;
			}
			return false;
		}

		public string GetName(int languageNumber = 0)
		{
			string text = base.gameObject.name;
			if (!string.IsNullOrEmpty(speechLabel))
			{
				text = speechLabel;
				if (languageNumber > 0)
				{
					return KickStarter.runtimeLanguages.GetTranslation(text, displayLineID, languageNumber);
				}
			}
			return text;
		}

		public void SetName(string newName, int _lineID)
		{
			speechLabel = newName;
			if (_lineID >= 0)
			{
				displayLineID = _lineID;
			}
			else
			{
				displayLineID = lineID;
			}
		}

		public void SetSpeechVolume(float volume)
		{
			if (speechSound != null)
			{
				speechSound.SetVolume(volume);
			}
			else if (speechAudioSource != null && KickStarter.settingsManager.volumeControl != VolumeControl.AudioMixerGroups)
			{
				speechAudioSource.volume = volume;
			}
		}

		public Speech GetCurrentSpeech()
		{
			foreach (Speech speech in KickStarter.dialog.speechList)
			{
				if (speech.speaker == this)
				{
					return speech;
				}
			}
			return null;
		}

		private void OnApplicationQuit()
		{
			if (portraitIcon != null)
			{
				portraitIcon.ClearCache();
			}
			foreach (Expression expression in expressions)
			{
				if (expression.portraitIcon != null)
				{
					expression.portraitIcon.ClearCache();
				}
			}
		}

		public void OnEnterTimeline(PlayableDirector director, int trackIndex)
		{
			if (!isUnderTimelineControl)
			{
				isUnderTimelineControl = true;
				KickStarter.eventManager.Call_OnCharacterTimeline(this, director, trackIndex, true);
			}
		}

		public void OnExitTimeline(PlayableDirector director, int trackIndex)
		{
			if (isUnderTimelineControl)
			{
				isUnderTimelineControl = false;
				KickStarter.eventManager.Call_OnCharacterTimeline(this, director, trackIndex, false);
			}
		}

		public string GetTranslatableString(int index)
		{
			return speechLabel;
		}

		public int GetTranslationID(int index)
		{
			return lineID;
		}
	}
}
