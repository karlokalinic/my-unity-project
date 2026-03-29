using UnityEngine;

public class HerdSimCore : MonoBehaviour
{
	public HerdSimController _controller;

	public Transform _scanner;

	public Transform _collider;

	public Transform _model;

	public Renderer _renderer;

	public float _hitPoints = 100f;

	public int _type;

	public float _minSize = 1f;

	public float _avoidAngle = 0.35f;

	public float _avoidDistance;

	public float _avoidSpeed = 75f;

	public float _stopDistance;

	private float _rotateCounterR;

	private float _rotateCounterL;

	public bool _pushHalfTheTime;

	private bool _pushToggle;

	public float _pushDistance;

	public float _pushForce = 5f;

	private bool _scan;

	public Vector3 _roamingArea;

	public float _walkSpeed = 0.5f;

	public float _runSpeed = 1.5f;

	public float _damping;

	public int _idleProbablity = 20;

	public float _runChance = 0.1f;

	public Vector3 _waypoint;

	public float _speed;

	public float _targetSpeed;

	public int _mode;

	public Vector3 _startPosition;

	private bool _reachedWaypoint = true;

	private int _lerpCounter;

	public bool _scared;

	public Transform _scaredOf;

	public bool _eating;

	public Transform _food;

	public float _groundCheckInterval = 0.1f;

	public string _groundTag = "Ground";

	private Vector3 _ground;

	private Quaternion _groundRot;

	private bool _grounded;

	public float _maxGroundAngle = 45f;

	public float _maxFall = 3f;

	public float _fakeGravity = 5f;

	public LayerMask _herdLayerMask = -1;

	public HerdSimCore _leader;

	public Vector3 _leaderArea;

	public int _leaderSize;

	public float _leaderAreaMultiplier = 0.2f;

	public int _maxHerdSize = 25;

	public int _minHerdSize = 10;

	public float _herdDistance = 2f;

	private int _herdSize;

	public bool _dead;

	public Material _deadMaterial;

	public bool _scaryCorpse;

	public string _animIdle = "idle";

	public float _animIdleSpeed = 1f;

	public string _animSleep = "sleep";

	public float _animSleepSpeed = 1f;

	public string _animWalk = "walk";

	public float _animWalkSpeed = 1f;

	public string _animRun = "run";

	public float _animRunSpeed = 1f;

	public string _animDead = "dead";

	public float _animDeadSpeed = 1f;

	public float _idleToSleepSeconds = 1f;

	private float _sleepCounter;

	private bool _idle;

	private int _updateCounter;

	public int _updateDivisor = 1;

	private static int _updateNextSeed;

	private int _updateSeed = -1;

	private float _newDelta;

	public bool _enabled;

	public LayerMask _groundLayerMask = -1;

	public LayerMask _pushyLayerMask = -1;

	public string _herdSimLayerName = "HerdSim";

	private int _groundIndex = 25;

	private int _herdSimIndex = 26;

	private Transform _thisTR;

	public bool _lean;

	public AnimationClip _leanLeftAnimation;

	public AnimationClip _leanRightAnimation;

	private AnimationState _leanLeft;

	private AnimationState _leanRight;

	private float _leanRightTime;

	private float _leanLeftTime;

	private bool _avoiding;

	private bool _avoidingLeft;

	private bool _avoidingRight;

	public void Start()
	{
		_thisTR = base.transform;
		_enabled = true;
		_groundIndex = LayerMask.NameToLayer(_groundTag);
		_herdSimIndex = LayerMask.NameToLayer(_herdSimLayerName);
		if (_updateDivisor > 1)
		{
			int num = _updateDivisor - 1;
			_updateNextSeed++;
			_updateSeed = _updateNextSeed;
			_updateNextSeed %= num;
		}
		if (_groundTag == null)
		{
			_groundTag = "Ground";
		}
		Init();
		_startPosition = _thisTR.position;
		if (_pushDistance <= 0f)
		{
			_pushDistance = _avoidDistance * 0.25f;
		}
		if (_stopDistance <= 0f)
		{
			_stopDistance = _avoidDistance * 0.25f;
		}
		_ground = (_waypoint = _thisTR.position);
		float maxFall = _maxFall;
		_maxFall = 1000000f;
		GroundCheck();
		_maxFall = maxFall;
		if (_collider == null)
		{
			_collider = _thisTR.Find("Collider");
		}
		_herdSize = Random.Range(_minHerdSize, _maxHerdSize);
		if (_minSize < 1f)
		{
			_thisTR.localScale = Vector3.one * Random.Range(_minSize, 1f);
		}
		_model.GetComponent<Animation>()[_animIdle].speed = _animIdleSpeed;
		_model.GetComponent<Animation>()[_animDead].speed = _animDeadSpeed;
		_model.GetComponent<Animation>()[_animSleep].speed = _animSleepSpeed;
		LeanInit();
	}

	private void LeanInit()
	{
		if (_lean)
		{
			_leanLeft = _model.GetComponent<Animation>()[_leanLeftAnimation.name];
			_leanRight = _model.GetComponent<Animation>()[_leanRightAnimation.name];
			AnimationState leanRight = _leanRight;
			int layer = 10;
			_leanLeft.layer = layer;
			leanRight.layer = layer;
			AnimationState leanRight2 = _leanRight;
			AnimationBlendMode blendMode = AnimationBlendMode.Additive;
			_leanLeft.blendMode = blendMode;
			leanRight2.blendMode = blendMode;
			_leanRight.enabled = true;
			_leanLeft.enabled = true;
			AnimationState leanLeft = _leanLeft;
			float weight = 1f;
			_leanRight.weight = weight;
			leanLeft.weight = weight;
			AnimationState leanRight3 = _leanRight;
			WrapMode wrapMode = WrapMode.ClampForever;
			_leanLeft.wrapMode = wrapMode;
			leanRight3.wrapMode = wrapMode;
		}
	}

	private void Lean()
	{
		if (_lean)
		{
			float num = AngleAmount();
			if (_avoidingLeft || (!_avoiding && _mode != 0 && (double)num < 0.3))
			{
				_leanLeftTime = Mathf.Lerp(_leanLeftTime, 0f - num, _newDelta * 2f);
			}
			else
			{
				_leanLeftTime = Mathf.Lerp(_leanLeftTime, 0f, _newDelta);
			}
			if (_avoidingRight || (!_avoiding && _mode != 0 && (double)num > 0.3))
			{
				_leanRightTime = Mathf.Lerp(_leanRightTime, num, _newDelta * 2f);
			}
			else
			{
				_leanRightTime = Mathf.Lerp(_leanRightTime, 0f, _newDelta);
			}
			_leanLeft.normalizedTime = _leanLeftTime;
			_leanRight.normalizedTime = _leanRightTime;
		}
	}

	public void Disable(bool disableModel, bool disableCollider)
	{
		if (_enabled)
		{
			_enabled = false;
			CancelInvoke();
			if (disableModel)
			{
				_model.gameObject.SetActive(false);
			}
			if (disableCollider)
			{
				_collider.gameObject.SetActive(false);
			}
			_thisTR.GetComponent<HerdSimCore>().enabled = false;
			_model.GetComponent<Animation>().Stop();
		}
	}

	public void Enable()
	{
		if (!_enabled)
		{
			_enabled = true;
			Init();
			if (!_model.gameObject.activeInHierarchy)
			{
				_model.gameObject.SetActive(true);
			}
			if (!_collider.gameObject.activeInHierarchy)
			{
				_collider.gameObject.SetActive(true);
			}
			_thisTR.GetComponent<HerdSimCore>().enabled = true;
			_model.GetComponent<Animation>().Play();
		}
	}

	public void Damage(float d)
	{
		_hitPoints -= d;
		if (_hitPoints <= 0f)
		{
			Death();
		}
	}

	public void Effects()
	{
		if (_controller != null && _mode == 2 && _controller._runPS != null && _speed > 1f)
		{
			_controller._runPS.transform.position = _thisTR.position;
			_controller._runPS.Emit(1);
		}
		if (_dead && _controller != null && _controller._deadPS != null)
		{
			_controller._deadPS.transform.position = _collider.transform.position;
			_controller._deadPS.Emit(1);
		}
	}

	public void Death()
	{
		if (_dead)
		{
			return;
		}
		_dead = true;
		_mode = 0;
		CancelInvoke("Wander");
		CancelInvoke("WalkTimeOut");
		CancelInvoke("FindLeader");
		if (_leader != null)
		{
			if (_leader != this)
			{
				_leader._leaderSize--;
			}
			else
			{
				_leaderSize = 0;
			}
			_leader = null;
		}
		if (_deadMaterial != null)
		{
			_renderer.sharedMaterial = _deadMaterial;
		}
		_model.GetComponent<Animation>()[_animDead].speed = 1f;
		_model.GetComponent<Animation>().CrossFade(_animDead, 0.1f);
		if (_scaryCorpse)
		{
			InvokeRepeating("Corpse", 1f, 1f);
		}
	}

	public void Corpse()
	{
		Collider[] array = Physics.OverlapSphere(_thisTR.position, 10f);
		HerdSimCore herdSimCore = null;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].transform.parent != null)
			{
				herdSimCore = array[i].transform.parent.GetComponent<HerdSimCore>();
			}
			if (_scaryCorpse && herdSimCore != null && !herdSimCore._dead && herdSimCore._mode < 1)
			{
				herdSimCore.Scare(base.transform);
			}
		}
	}

	public void FindLeader()
	{
		if (_leader == this && _leaderSize <= 1)
		{
			_leader = null;
			_leaderSize = 0;
		}
		else
		{
			if (!(_leader != this))
			{
				return;
			}
			if (_leader != null && _leader._dead)
			{
				_leader = null;
			}
			_leaderSize = 0;
			Collider[] array = Physics.OverlapSphere(_thisTR.position, _herdDistance, _herdLayerMask);
			HerdSimCore herdSimCore = null;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].transform.parent != null)
				{
					herdSimCore = array[i].transform.parent.GetComponent<HerdSimCore>();
				}
				if (herdSimCore != null && herdSimCore != this && _type == herdSimCore._type)
				{
					if (_leader == null && herdSimCore._leader == null)
					{
						_leader = this;
						herdSimCore._leader = this;
						_leaderSize += 2;
						break;
					}
					if (_leader == null && herdSimCore._leader != null && herdSimCore._leader._leaderSize < herdSimCore._leader._herdSize)
					{
						_leader = herdSimCore._leader;
						_leader._leaderSize++;
						break;
					}
					if (_leader != null && herdSimCore._leader != _leader && herdSimCore._leader != null && herdSimCore._leader._leaderSize >= _leader._leaderSize && herdSimCore._leader._leaderSize < herdSimCore._leader._herdSize)
					{
						_leader._leaderSize--;
						herdSimCore._leader._leaderSize++;
						_leader = herdSimCore._leader;
						break;
					}
				}
			}
		}
	}

	public void Wander()
	{
		Vector3 waypoint = Vector3.zero;
		if (_leader == this)
		{
			_leaderArea = Vector3.one * ((float)_leaderSize * _leaderAreaMultiplier + 1f);
		}
		Vector3 zero = Vector3.zero;
		Vector3 zero2 = Vector3.zero;
		if (_leader != null && _leader != this)
		{
			zero = _leader._leaderArea;
			zero2 = _leader.transform.position;
		}
		else if (_controller == null)
		{
			zero = _roamingArea;
			zero2 = _startPosition;
		}
		else
		{
			zero = _controller._roamingArea;
			zero2 = _controller.transform.position;
		}
		waypoint.x = Random.Range(0f - zero.x, zero.x) + zero2.x;
		waypoint.z = Random.Range(0f - zero.z, zero.z) + zero2.z;
		if (_food != null)
		{
			waypoint = _food.position;
			_mode = 2;
		}
		else if (this != null)
		{
			if (_thisTR.position.x < 0f - zero.x + zero2.x || _thisTR.position.x > zero.x + zero2.x || _thisTR.position.z < 0f - zero.z + zero2.z || _thisTR.position.z > zero.z + zero2.z)
			{
				if (Random.value < 0.1f)
				{
					_mode = 2;
				}
				else
				{
					_mode = 1;
				}
				_waypoint = waypoint;
			}
			else if (_leader != null && _leader != this && Random.value < 0.75f)
			{
				_mode = 0;
			}
			else if (_reachedWaypoint)
			{
				_mode = Random.Range(-_idleProbablity, 2);
				if (_mode == 1 && Random.value < _runChance && (_leader == null || _leader == this))
				{
					_mode = 2;
				}
			}
		}
		if (_reachedWaypoint && _mode > 0)
		{
			_waypoint = waypoint;
			CancelInvoke("WalkTimeOut");
			Invoke("WalkTimeOut", 30f);
			_reachedWaypoint = false;
		}
		_waypoint.y = _collider.transform.position.y;
		_lerpCounter = 0;
	}

	public void Init()
	{
		if (_controller != null)
		{
			InvokeRepeating("Effects", 1f + Random.value, 0.1f);
		}
		InvokeRepeating("Wander", 1f + Random.value, 1f);
		InvokeRepeating("GroundCheck", _groundCheckInterval * Random.value + 1f, _groundCheckInterval);
		InvokeRepeating("FindLeader", Random.value * 3f, 3f);
	}

	private float AngleAmount()
	{
		Vector3 normalized = (_waypoint - base.transform.position).normalized;
		float num = Vector3.Dot(normalized, base.transform.right);
		float num2 = Vector3.Dot(normalized, base.transform.forward);
		if (num2 < 0f)
		{
			if (num < 0f)
			{
				num = -1f;
			}
			if (num > 0f)
			{
				num = 1f;
			}
		}
		return num;
	}

	public void AnimationHandler()
	{
		if (_dead)
		{
			return;
		}
		if (_mode == 1)
		{
			if (_speed > 0f)
			{
				_model.GetComponent<Animation>()[_animWalk].speed = _speed * _animWalkSpeed + 0.051f;
			}
			else
			{
				_model.GetComponent<Animation>()[_animWalk].speed = 0.1f;
			}
			_model.GetComponent<Animation>().CrossFade(_animWalk, 0.5f);
			_idle = false;
			return;
		}
		if (_mode == 2)
		{
			if (_speed > _runSpeed * 0.35f)
			{
				_model.GetComponent<Animation>().CrossFade(_animRun, 0.5f);
				_model.GetComponent<Animation>()[_animRun].speed = _speed * _animRunSpeed + 0.051f;
			}
			else
			{
				_model.GetComponent<Animation>().CrossFade(_animWalk, 0.5f);
				_model.GetComponent<Animation>()[_animWalk].speed = _speed * _animWalkSpeed + 0.051f;
			}
			_idle = false;
			return;
		}
		if (!_idle && _speed < 0.5f)
		{
			_sleepCounter = 0f;
			_model.GetComponent<Animation>().CrossFade(_animIdle, 1f);
			_idle = true;
		}
		if (_idle && _sleepCounter > _idleToSleepSeconds)
		{
			_model.GetComponent<Animation>().CrossFade(_animSleep, 1f);
		}
		else
		{
			_sleepCounter += _newDelta;
		}
	}

	public void Scare(Transform t)
	{
		if (_scaredOf == null)
		{
			_scaredOf = t;
		}
		_mode = 2;
		if (!_scared)
		{
			_scared = true;
			UnFlock();
			Invoke("EndScare", 3f);
		}
		else if (Vector3.Distance(_scaredOf.position, _thisTR.position) > Vector3.Distance(t.position, _thisTR.position))
		{
			_scaredOf = t;
		}
	}

	public void EndScare()
	{
		_scared = false;
		Wander();
		_reachedWaypoint = true;
	}

	public void Food(Transform t)
	{
		if (_food == null)
		{
			_food = t;
		}
	}

	public void Pushy()
	{
		RaycastHit hitInfo = default(RaycastHit);
		float num = 0f;
		Vector3 forward = _scanner.forward;
		if (_scan)
		{
			_scanner.Rotate(new Vector3(0f, 1000f * _newDelta, 0f));
		}
		else
		{
			_scanner.Rotate(new Vector3(0f, 250f * _newDelta, 0f));
		}
		if (Physics.Raycast(_collider.transform.position, forward, out hitInfo, _pushDistance, _pushyLayerMask))
		{
			Transform transform = hitInfo.transform;
			if (transform.gameObject.layer != _groundIndex || (transform.gameObject.layer == _groundIndex && Vector3.Angle(Vector3.up, hitInfo.normal) > _maxGroundAngle))
			{
				float distance = hitInfo.distance;
				num = (_pushDistance - distance) / _pushDistance;
				if (base.gameObject.layer != _herdSimIndex)
				{
					_thisTR.position -= forward * _newDelta * num * _pushForce;
				}
				else if (distance < _pushDistance * 0.5f)
				{
					_thisTR.position -= forward * _newDelta * (num - 0.5f) * _pushForce;
				}
				_scan = false;
			}
			else
			{
				_scan = true;
			}
		}
		else
		{
			_scan = true;
		}
	}

	public void UnFlock()
	{
		if (_leader != null && _leader != this)
		{
			_reachedWaypoint = true;
			_leader._leaderSize--;
			_leader = null;
			Wander();
		}
	}

	public void WalkTimeOut()
	{
		_reachedWaypoint = true;
		UnFlock();
		Wander();
	}

	public void Update()
	{
		if (_updateDivisor > 1)
		{
			_updateCounter++;
			if (_updateCounter != _updateSeed)
			{
				_updateCounter %= _updateDivisor;
				return;
			}
			_updateCounter %= _updateDivisor;
			_newDelta = Time.deltaTime * (float)_updateDivisor;
		}
		else
		{
			_newDelta = Time.deltaTime;
		}
		if ((!_pushHalfTheTime || _pushToggle) && _mode > 0)
		{
			Pushy();
		}
		_pushToggle = !_pushToggle;
		Vector3 position = _thisTR.position;
		position.y -= (_thisTR.position.y - _ground.y) * _newDelta * _fakeGravity;
		_thisTR.position = position;
		if (!_dead)
		{
			AnimationHandler();
			Vector3 zero = Vector3.zero;
			Quaternion b = Quaternion.identity;
			_model.transform.rotation = Quaternion.Slerp(_model.transform.rotation, _groundRot, _newDelta * 5f);
			Quaternion localRotation = _model.transform.localRotation;
			localRotation.eulerAngles = new Vector3(localRotation.eulerAngles.x, 0f, localRotation.eulerAngles.y);
			_model.transform.localRotation = localRotation;
			if (!_scared && _mode > 0)
			{
				zero = _waypoint - _thisTR.position;
				if (zero != Vector3.zero)
				{
					b = Quaternion.LookRotation(zero);
				}
			}
			else if (_scared && _scaredOf != null)
			{
				zero = _scaredOf.position - _thisTR.position;
				if (zero != Vector3.zero)
				{
					b = Quaternion.LookRotation(-zero);
				}
			}
			if ((_thisTR.position - _waypoint).sqrMagnitude < 10f)
			{
				if (_mode > 0)
				{
					_mode = 1;
				}
				_reachedWaypoint = true;
			}
			else
			{
				_eating = false;
			}
			if (_scared || (_leader != null && _leader != this && _leader._mode == 2))
			{
				_mode = 2;
			}
			else if (_eating)
			{
				_mode = 0;
			}
			if (_mode == 1)
			{
				if (_leader != this)
				{
					_targetSpeed = _walkSpeed;
				}
				else
				{
					_targetSpeed = _walkSpeed * 0.75f;
				}
			}
			else if (_mode == 2)
			{
				_targetSpeed = _runSpeed;
			}
			_speed = Mathf.Lerp(_speed, _targetSpeed, (float)_lerpCounter * _newDelta * 0.05f);
			_lerpCounter++;
			if (_speed > 0.01f && !Avoidance())
			{
				_thisTR.rotation = Quaternion.Slerp(_thisTR.rotation, b, _newDelta * _damping);
			}
			if (_mode == 1)
			{
				_targetSpeed = _walkSpeed;
			}
			else if (_mode == 2)
			{
				_targetSpeed = _runSpeed;
			}
			else if (_mode <= 0)
			{
				_targetSpeed = 0f;
			}
			_thisTR.rotation = Quaternion.Euler(0f, _thisTR.rotation.eulerAngles.y, 0f);
			Lean();
		}
		if (!_grounded)
		{
			_scared = false;
			UnFlock();
			Vector3 zero2 = Vector3.zero;
			zero2 = _thisTR.position;
			zero2.x -= (_thisTR.position.x - _ground.x) * _newDelta * 15f;
			zero2.z -= (_thisTR.position.z - _ground.z) * _newDelta * 15f;
			_thisTR.position = zero2;
		}
		else if (!_dead)
		{
			_thisTR.position += _thisTR.TransformDirection(Vector3.forward) * _speed * _newDelta;
		}
	}

	public void GroundCheck()
	{
		RaycastHit hitInfo = default(RaycastHit);
		if (Physics.Raycast(new Vector3(_thisTR.position.x, _collider.transform.position.y, _thisTR.position.z), -_thisTR.up, out hitInfo, _maxFall, _groundLayerMask))
		{
			_grounded = true;
			_groundRot = Quaternion.FromToRotation(_model.transform.up, hitInfo.normal) * _model.transform.rotation;
			_ground = hitInfo.point;
		}
		else
		{
			_grounded = false;
			_waypoint = _thisTR.position + _thisTR.right * 5f;
			_speed = 0f;
		}
	}

	private void NotAvoiding()
	{
		_avoidingRight = (_avoidingLeft = false);
	}

	public bool Avoidance()
	{
		bool flag = false;
		RaycastHit hitInfo = default(RaycastHit);
		float num = 0f;
		Vector3 forward = _model.transform.forward;
		Vector3 right = _model.transform.right;
		Transform transform = null;
		float num2 = Mathf.Clamp(_speed, 0.5f, 1f);
		Quaternion identity = Quaternion.identity;
		if (_mode == 0 && _speed < 0.21f)
		{
			return true;
		}
		if (_mode > 0 && _rotateCounterR == 0f && Physics.Raycast(_collider.transform.position, forward + right * (_avoidAngle + _rotateCounterL), out hitInfo, _avoidDistance, _pushyLayerMask))
		{
			transform = hitInfo.transform;
			if (transform.gameObject.layer != _groundIndex || (transform.gameObject.layer == _groundIndex && Vector3.Angle(Vector3.up, hitInfo.normal) > _maxGroundAngle))
			{
				_rotateCounterL += _newDelta;
				num = (_avoidDistance - hitInfo.distance) / _avoidDistance;
				identity = _thisTR.rotation;
				identity.eulerAngles = new Vector3(identity.eulerAngles.x, identity.eulerAngles.y - _avoidSpeed * _newDelta * num * _rotateCounterL * num2, identity.eulerAngles.z);
				_thisTR.rotation = identity;
				CancelInvoke("NotAvoiding");
				Invoke("NotAvoiding", 0.5f);
				_avoidingLeft = true;
				_avoidingRight = false;
				if (_rotateCounterL > 1.5f)
				{
					_rotateCounterL = 1.5f;
					_rotateCounterR = 0f;
					flag = true;
				}
			}
		}
		else if (_mode > 0 && _rotateCounterL == 0f && Physics.Raycast(_collider.transform.position, forward + right * (0f - (_avoidAngle + _rotateCounterR)), out hitInfo, _avoidDistance, _pushyLayerMask))
		{
			transform = hitInfo.transform;
			if (transform.gameObject.layer != _groundIndex || (transform.gameObject.layer == _groundIndex && Vector3.Angle(Vector3.up, hitInfo.normal) > _maxGroundAngle))
			{
				_rotateCounterR += _newDelta;
				num = (_avoidDistance - hitInfo.distance) / _avoidDistance;
				identity = _thisTR.rotation;
				identity.eulerAngles = new Vector3(identity.eulerAngles.x, identity.eulerAngles.y + _avoidSpeed * _newDelta * num * _rotateCounterR * num2, identity.eulerAngles.z);
				CancelInvoke("NotAvoiding");
				Invoke("NotAvoiding", 0.5f);
				_avoidingLeft = false;
				_avoidingRight = true;
				if (_rotateCounterR > 1.5f)
				{
					_rotateCounterR = 1.5f;
					_rotateCounterL = 0f;
					flag = true;
				}
			}
		}
		else
		{
			_rotateCounterL -= _newDelta;
			if (_rotateCounterL < 0f)
			{
				_rotateCounterL = 0f;
			}
			_rotateCounterR -= _newDelta;
			if (_rotateCounterR < 0f)
			{
				_rotateCounterR = 0f;
			}
		}
		if (Physics.Raycast(_collider.transform.position, forward + right * Random.Range(-0.1f, 0.1f), out hitInfo, _avoidDistance * 0.9f, _pushyLayerMask))
		{
			transform = hitInfo.transform;
			if (transform.gameObject.layer != _groundIndex || (transform.gameObject.layer == _groundIndex && Vector3.Angle(Vector3.up, hitInfo.normal) > _maxGroundAngle))
			{
				float distance = hitInfo.distance;
				num = (_avoidDistance - hitInfo.distance) / _avoidDistance;
				identity = _thisTR.rotation;
				if (_rotateCounterL > _rotateCounterR)
				{
					identity.eulerAngles = new Vector3(identity.eulerAngles.x, identity.eulerAngles.y - _avoidSpeed * _newDelta * num * _rotateCounterL, identity.eulerAngles.z);
				}
				else
				{
					identity.eulerAngles = new Vector3(identity.eulerAngles.x, identity.eulerAngles.y + _avoidSpeed * _newDelta * num * _rotateCounterR, identity.eulerAngles.z);
				}
				base.transform.rotation = identity;
				if (distance < _stopDistance * 0.5f)
				{
					_speed = -0.2f;
					flag = true;
				}
				if (distance < _stopDistance && _speed > 0.2f)
				{
					_speed -= _newDelta * (1f - num) * 25f;
				}
				if (_speed < -0.2f)
				{
					_speed = -0.2f;
				}
			}
		}
		if (flag)
		{
			_avoiding = true;
		}
		else
		{
			_avoiding = false;
		}
		return flag;
	}

	public void OnDrawGizmos()
	{
		GUIStyle gUIStyle = new GUIStyle();
		Color blue = Color.blue;
		Color color = new Color32(0, byte.MaxValue, 246, byte.MaxValue);
		Color color2 = new Color32(byte.MaxValue, byte.MaxValue, 0, byte.MaxValue);
		gUIStyle.normal.textColor = Color.yellow;
		if (!Application.isPlaying)
		{
			_startPosition = base.transform.position;
		}
		else
		{
			Gizmos.color = color;
			Gizmos.DrawLine(_collider.transform.position, _waypoint);
		}
		if (_controller == null)
		{
			Gizmos.color = blue;
			Gizmos.DrawWireCube(_startPosition, _roamingArea * 2f);
		}
		if (_leader == this)
		{
			Gizmos.color = color2;
			Gizmos.DrawWireCube(_thisTR.position, new Vector3(_leaderArea.x * 2f, 0f, _leaderArea.y * 2f));
			Gizmos.DrawIcon(_collider.transform.position, "leader.png", false);
		}
		else if (_leader != null)
		{
			Gizmos.color = color2;
			Gizmos.DrawLine(_collider.transform.position, _leader._collider.transform.position);
		}
		if (_scared)
		{
			Gizmos.DrawIcon(_collider.transform.position, "scared.png", false);
		}
		if (_dead)
		{
			Gizmos.DrawIcon(_collider.transform.position, "dead.png", false);
		}
	}
}
