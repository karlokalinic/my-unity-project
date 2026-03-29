using UnityEngine;

namespace AC
{
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_player_q_t_e.html")]
	public class PlayerQTE : MonoBehaviour
	{
		protected QTEState qteState;

		protected QTEType qteType;

		protected string inputName;

		protected Animator animator;

		protected bool wrongKeyFails;

		protected float holdDuration;

		protected float cooldownTime;

		protected int targetPresses;

		protected bool doCooldown;

		protected float progress;

		protected int numPresses;

		protected float startTime;

		protected float endTime;

		protected float lastPressTime;

		protected bool canMash;

		protected float axisThreshold;

		protected const string touchScreenTap = "TOUCHSCREENTAP";

		protected string verticalInputName;

		protected bool rotationIsClockwise;

		protected float targetRotations;

		protected float currentRotations;

		protected float maxRotation;

		protected Vector2 lastFrameRotationInput;

		public void OnAwake()
		{
			SkipQTE();
		}

		public QTEState GetState()
		{
			return qteState;
		}

		public void SkipQTE()
		{
			endTime = 0f;
			qteState = QTEState.Win;
		}

		public void StartSinglePressQTE(string _inputName, float _duration, Animator _animator = null, bool _wrongKeyFails = false)
		{
			if (string.IsNullOrEmpty(_inputName) && KickStarter.settingsManager.inputMethod == InputMethod.TouchScreen)
			{
				_inputName = "TOUCHSCREENTAP";
			}
			if (!string.IsNullOrEmpty(_inputName) && !(_duration <= 0f))
			{
				Setup(QTEType.SingleKeypress, _inputName, _duration, _animator, _wrongKeyFails, 0f);
			}
		}

		public void StartSingleAxisQTE(string _inputName, float _duration, float _axisThreshold, Animator _animator = null, bool _wrongKeyFails = false)
		{
			if (!string.IsNullOrEmpty(_inputName) && !(_duration <= 0f))
			{
				Setup(QTEType.SingleAxis, _inputName, _duration, _animator, _wrongKeyFails, _axisThreshold);
			}
		}

		public void StartHoldKeyQTE(string _inputName, float _duration, float _holdDuration, Animator _animator = null, bool _wrongKeyFails = false)
		{
			if (string.IsNullOrEmpty(_inputName) && KickStarter.settingsManager.inputMethod == InputMethod.TouchScreen)
			{
				_inputName = "TOUCHSCREENTAP";
			}
			if (!string.IsNullOrEmpty(_inputName) && !(_duration <= 0f))
			{
				if (_holdDuration > _duration)
				{
					_holdDuration = _duration;
				}
				holdDuration = _holdDuration;
				Setup(QTEType.HoldKey, _inputName, _duration, _animator, _wrongKeyFails, 0f);
			}
		}

		public void StartButtonMashQTE(string _inputName, float _duration, int _targetPresses, bool _doCooldown, float _cooldownTime, Animator _animator = null, bool _wrongKeyFails = false)
		{
			if (string.IsNullOrEmpty(_inputName) && KickStarter.settingsManager.inputMethod == InputMethod.TouchScreen)
			{
				_inputName = "TOUCHSCREENTAP";
			}
			if (!string.IsNullOrEmpty(_inputName) && !(_duration <= 0f))
			{
				targetPresses = _targetPresses;
				doCooldown = _doCooldown;
				cooldownTime = _cooldownTime;
				Setup(QTEType.ButtonMash, _inputName, _duration, _animator, _wrongKeyFails, 0f);
			}
		}

		public void StartThumbstickRotationQTE(string _horizontalInputName, string _verticalInputName, float _duration, float _targetRotations, bool _rotationIsClockwise, Animator _animator = null, bool _wrongDirectionFails = false)
		{
			if (!string.IsNullOrEmpty(_horizontalInputName) && !string.IsNullOrEmpty(_verticalInputName) && !(_duration <= 0f) && !(_targetRotations <= 0f))
			{
				verticalInputName = _verticalInputName;
				rotationIsClockwise = _rotationIsClockwise;
				targetRotations = ((!rotationIsClockwise) ? (0f - _targetRotations) : _targetRotations);
				maxRotation = 0f;
				currentRotations = 0f;
				lastFrameRotationInput = Vector2.zero;
				Setup(QTEType.ThumbstickRotation, _horizontalInputName, _duration, _animator, _wrongDirectionFails, 0.2f);
			}
		}

		public float GetRemainingTimeFactor()
		{
			if (endTime <= 0f || Time.time <= startTime)
			{
				return 1f;
			}
			if (Time.time >= endTime)
			{
				return 0f;
			}
			return 1f - (Time.time - startTime) / (endTime - startTime);
		}

		public float GetProgress()
		{
			if (qteState == QTEState.Win)
			{
				progress = 1f;
			}
			else if (qteState == QTEState.Lose)
			{
				progress = 0f;
			}
			else if (endTime > 0f)
			{
				switch (qteType)
				{
				case QTEType.HoldKey:
					progress = ((!(lastPressTime > 0f)) ? 0f : ((Time.time - lastPressTime) / holdDuration));
					break;
				case QTEType.ButtonMash:
					progress = (float)numPresses / (float)targetPresses;
					break;
				case QTEType.ThumbstickRotation:
					progress = Mathf.Clamp01(currentRotations / targetRotations);
					break;
				}
			}
			return progress;
		}

		public bool QTEIsActive()
		{
			if (endTime > 0f)
			{
				return true;
			}
			return false;
		}

		public void UpdateQTE()
		{
			if (endTime <= 0f)
			{
				return;
			}
			if (Time.time > endTime)
			{
				Lose();
				return;
			}
			switch (qteType)
			{
			case QTEType.SingleKeypress:
				if (inputName == "TOUCHSCREENTAP")
				{
					if ((float)Input.touchCount > 0f)
					{
						Win();
					}
				}
				else if (KickStarter.playerInput.InputGetButtonDown(inputName))
				{
					Win();
				}
				else if (wrongKeyFails && KickStarter.playerInput.InputAnyKey() && KickStarter.playerInput.GetMouseState() == MouseState.Normal)
				{
					Lose();
				}
				break;
			case QTEType.SingleAxis:
			{
				float num2 = KickStarter.playerInput.InputGetAxis(inputName);
				if (axisThreshold > 0f && num2 > axisThreshold)
				{
					Win();
				}
				else if (axisThreshold < 0f && num2 < axisThreshold)
				{
					Win();
				}
				else if (wrongKeyFails)
				{
					if (axisThreshold > 0f && num2 < 0f - axisThreshold)
					{
						Lose();
					}
					else if (axisThreshold < 0f && num2 > 0f - axisThreshold)
					{
						Lose();
					}
				}
				break;
			}
			case QTEType.HoldKey:
				if (inputName == "TOUCHSCREENTAP")
				{
					if ((float)Input.touchCount > 0f)
					{
						if (lastPressTime <= 0f)
						{
							lastPressTime = Time.time;
						}
						else if (Time.time > lastPressTime + holdDuration)
						{
							Win();
							break;
						}
					}
					else
					{
						lastPressTime = 0f;
					}
				}
				else if (KickStarter.playerInput.InputGetButton(inputName))
				{
					if (lastPressTime <= 0f)
					{
						lastPressTime = Time.time;
					}
					else if (Time.time > lastPressTime + holdDuration)
					{
						Win();
						break;
					}
				}
				else
				{
					if (wrongKeyFails && Input.anyKey)
					{
						Lose();
						break;
					}
					lastPressTime = 0f;
				}
				if (animator != null)
				{
					if (lastPressTime <= 0f)
					{
						animator.SetBool("Held", false);
					}
					else
					{
						animator.SetBool("Held", true);
					}
				}
				break;
			case QTEType.ButtonMash:
				if (inputName == "TOUCHSCREENTAP")
				{
					if (Input.touchCount > 1)
					{
						if (canMash)
						{
							numPresses++;
							lastPressTime = Time.time;
							if ((bool)animator)
							{
								animator.Play("Hit", 0, 0f);
							}
							canMash = false;
						}
					}
					else
					{
						canMash = true;
						if (doCooldown && lastPressTime > 0f && Time.time > lastPressTime + cooldownTime)
						{
							numPresses--;
							lastPressTime = Time.time;
						}
					}
				}
				else
				{
					if (KickStarter.playerInput.InputGetButtonDown(inputName))
					{
						if (canMash)
						{
							numPresses++;
							lastPressTime = Time.time;
							if ((bool)animator)
							{
								animator.Play("Hit", 0, 0f);
							}
							canMash = false;
						}
					}
					else
					{
						canMash = true;
						if (doCooldown && lastPressTime > 0f && Time.time > lastPressTime + cooldownTime)
						{
							numPresses--;
							lastPressTime = Time.time;
						}
					}
					if (!KickStarter.playerInput.InputGetButtonDown(inputName) && wrongKeyFails && Input.anyKeyDown)
					{
						Lose();
						break;
					}
				}
				if (numPresses < 0)
				{
					numPresses = 0;
				}
				if (numPresses >= targetPresses)
				{
					Win();
				}
				break;
			case QTEType.ThumbstickRotation:
			{
				Vector2 vector = new Vector2(KickStarter.playerInput.InputGetAxis(inputName), KickStarter.playerInput.InputGetAxis(verticalInputName));
				if (vector.sqrMagnitude > axisThreshold)
				{
					if (lastFrameRotationInput != Vector2.zero)
					{
						float num = AdvGame.SignedAngle(vector, lastFrameRotationInput);
						if (num > 180f)
						{
							num -= 360f;
						}
						else if (num < -180f)
						{
							num += 360f;
						}
						currentRotations += num / 360f;
						if (rotationIsClockwise)
						{
							maxRotation = Mathf.Max(maxRotation, currentRotations);
							if (currentRotations > targetRotations)
							{
								Win();
								break;
							}
						}
						else
						{
							maxRotation = Mathf.Min(maxRotation, currentRotations);
							if (currentRotations < targetRotations)
							{
								Win();
								break;
							}
						}
						if (wrongKeyFails)
						{
							if (rotationIsClockwise)
							{
								if (maxRotation - currentRotations > 0.15f)
								{
									Lose();
									break;
								}
							}
							else if (maxRotation - currentRotations < -0.15f)
							{
								Lose();
								break;
							}
						}
					}
					lastFrameRotationInput = vector;
				}
				else
				{
					currentRotations = 0f;
					lastFrameRotationInput = Vector2.zero;
				}
				break;
			}
			}
		}

		protected void Setup(QTEType _qteType, string _inputName, float _duration, Animator _animator, bool _wrongKeyFails, float _axisThreshold)
		{
			qteType = _qteType;
			qteState = QTEState.None;
			progress = 0f;
			inputName = _inputName;
			animator = _animator;
			wrongKeyFails = _wrongKeyFails;
			numPresses = 0;
			startTime = Time.time;
			lastPressTime = 0f;
			endTime = Time.time + _duration;
			axisThreshold = _axisThreshold;
		}

		protected virtual void Win()
		{
			if (animator != null)
			{
				animator.Play("Win");
			}
			qteState = QTEState.Win;
			endTime = 0f;
		}

		protected virtual void Lose()
		{
			qteState = QTEState.Lose;
			endTime = 0f;
			if ((bool)animator)
			{
				animator.Play("Lose");
			}
		}
	}
}
