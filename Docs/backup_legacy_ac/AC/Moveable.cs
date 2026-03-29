using UnityEngine;

namespace AC
{
	[AddComponentMenu("Adventure Creator/Misc/Moveable")]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_moveable.html")]
	public class Moveable : MonoBehaviour
	{
		protected float positionChangeTime;

		protected float positionStartTime;

		protected AnimationCurve positionTimeCurve;

		protected MoveMethod positionMethod;

		protected Vector3 startPosition;

		protected Vector3 endPosition;

		protected bool inWorldSpace;

		protected float rotateChangeTime;

		protected float rotateStartTime;

		protected AnimationCurve rotateTimeCurve;

		protected MoveMethod rotateMethod;

		protected bool doEulerRotation;

		protected Vector3 startEulerRotation;

		protected Vector3 endEulerRotation;

		protected Quaternion startRotation;

		protected Quaternion endRotation;

		protected float scaleChangeTime;

		protected float scaleStartTime;

		protected AnimationCurve scaleTimeCurve;

		protected MoveMethod scaleMethod;

		protected Vector3 startScale;

		protected Vector3 endScale;

		protected Char character;

		protected Rigidbody _rigidbody;

		public Rigidbody Rigidbody
		{
			get
			{
				return _rigidbody;
			}
		}

		protected virtual void Awake()
		{
			_rigidbody = GetComponent<Rigidbody>();
		}

		protected void Update()
		{
			if (positionChangeTime > 0f)
			{
				if (Time.time < positionStartTime + positionChangeTime)
				{
					if (inWorldSpace)
					{
						base.transform.position = ((positionMethod != MoveMethod.Curved) ? AdvGame.Lerp(startPosition, endPosition, AdvGame.Interpolate(positionStartTime, positionChangeTime, positionMethod, positionTimeCurve)) : Vector3.Slerp(startPosition, endPosition, AdvGame.Interpolate(positionStartTime, positionChangeTime, positionMethod, positionTimeCurve)));
					}
					else
					{
						base.transform.localPosition = ((positionMethod != MoveMethod.Curved) ? AdvGame.Lerp(startPosition, endPosition, AdvGame.Interpolate(positionStartTime, positionChangeTime, positionMethod, positionTimeCurve)) : Vector3.Slerp(startPosition, endPosition, AdvGame.Interpolate(positionStartTime, positionChangeTime, positionMethod, positionTimeCurve)));
					}
				}
				else
				{
					if (inWorldSpace)
					{
						base.transform.position = endPosition;
					}
					else
					{
						base.transform.localPosition = endPosition;
					}
					positionChangeTime = 0f;
				}
			}
			if (rotateChangeTime > 0f)
			{
				if (Time.time < rotateStartTime + rotateChangeTime)
				{
					if (doEulerRotation)
					{
						if (inWorldSpace)
						{
							base.transform.eulerAngles = ((rotateMethod != MoveMethod.Curved) ? AdvGame.Lerp(startEulerRotation, endEulerRotation, AdvGame.Interpolate(rotateStartTime, rotateChangeTime, rotateMethod, rotateTimeCurve)) : Vector3.Slerp(startEulerRotation, endEulerRotation, AdvGame.Interpolate(rotateStartTime, rotateChangeTime, rotateMethod, rotateTimeCurve)));
						}
						else
						{
							base.transform.localEulerAngles = ((rotateMethod != MoveMethod.Curved) ? AdvGame.Lerp(startEulerRotation, endEulerRotation, AdvGame.Interpolate(rotateStartTime, rotateChangeTime, rotateMethod, rotateTimeCurve)) : Vector3.Slerp(startEulerRotation, endEulerRotation, AdvGame.Interpolate(rotateStartTime, rotateChangeTime, rotateMethod, rotateTimeCurve)));
						}
					}
					else if (inWorldSpace)
					{
						base.transform.rotation = ((rotateMethod != MoveMethod.Curved) ? AdvGame.Lerp(startRotation, endRotation, AdvGame.Interpolate(rotateStartTime, rotateChangeTime, rotateMethod, rotateTimeCurve)) : Quaternion.Slerp(startRotation, endRotation, AdvGame.Interpolate(rotateStartTime, rotateChangeTime, rotateMethod, rotateTimeCurve)));
					}
					else
					{
						base.transform.localRotation = ((rotateMethod != MoveMethod.Curved) ? AdvGame.Lerp(startRotation, endRotation, AdvGame.Interpolate(rotateStartTime, rotateChangeTime, rotateMethod, rotateTimeCurve)) : Quaternion.Slerp(startRotation, endRotation, AdvGame.Interpolate(rotateStartTime, rotateChangeTime, rotateMethod, rotateTimeCurve)));
					}
				}
				else
				{
					if (doEulerRotation)
					{
						if (inWorldSpace)
						{
							base.transform.eulerAngles = endEulerRotation;
						}
						else
						{
							base.transform.localEulerAngles = endEulerRotation;
						}
					}
					else if (inWorldSpace)
					{
						base.transform.rotation = endRotation;
					}
					else
					{
						base.transform.localRotation = endRotation;
					}
					if (character == null)
					{
						character = GetComponent<Char>();
					}
					if (character != null)
					{
						character.SetLookDirection(character.TransformRotation * Vector3.forward, true);
						character.StopTurning();
					}
					rotateChangeTime = 0f;
				}
			}
			if (!(scaleChangeTime > 0f))
			{
				return;
			}
			if (Time.time < scaleStartTime + scaleChangeTime)
			{
				if (scaleMethod == MoveMethod.Curved)
				{
					base.transform.localScale = Vector3.Slerp(startScale, endScale, AdvGame.Interpolate(scaleStartTime, scaleChangeTime, scaleMethod, scaleTimeCurve));
				}
				else
				{
					base.transform.localScale = AdvGame.Lerp(startScale, endScale, AdvGame.Interpolate(scaleStartTime, scaleChangeTime, scaleMethod, scaleTimeCurve));
				}
			}
			else
			{
				base.transform.localScale = endScale;
				scaleChangeTime = 0f;
			}
		}

		public void StopMoving()
		{
			positionChangeTime = (rotateChangeTime = (scaleChangeTime = 0f));
		}

		public bool IsMoving(TransformType transformType)
		{
			switch (transformType)
			{
			case TransformType.Translate:
				return positionChangeTime > 0f;
			case TransformType.Rotate:
				return rotateChangeTime > 0f;
			case TransformType.Scale:
				return scaleChangeTime > 0f;
			case TransformType.CopyMarker:
				return positionChangeTime > 0f;
			default:
				return false;
			}
		}

		public void EndMovement()
		{
			if (positionChangeTime > 0f)
			{
				base.transform.localPosition = endPosition;
			}
			if (rotateChangeTime > 0f)
			{
				if (doEulerRotation)
				{
					base.transform.localEulerAngles = endEulerRotation;
				}
				else
				{
					base.transform.localRotation = endRotation;
				}
			}
			if (scaleChangeTime > 0f)
			{
				base.transform.localScale = endScale;
			}
			StopMoving();
		}

		public void Move(Vector3 _newVector, MoveMethod _moveMethod, bool _inWorldSpace, float _transitionTime, TransformType _transformType, bool _doEulerRotation, AnimationCurve _timeCurve, bool clearExisting)
		{
			if (_rigidbody != null && !_rigidbody.isKinematic)
			{
				Rigidbody rigidbody = _rigidbody;
				Vector3 zero = Vector3.zero;
				_rigidbody.angularVelocity = zero;
				rigidbody.linearVelocity = zero;
			}
			inWorldSpace = _inWorldSpace;
			if (_transitionTime <= 0f)
			{
				if (clearExisting)
				{
					positionChangeTime = (rotateChangeTime = (scaleChangeTime = 0f));
				}
				switch (_transformType)
				{
				case TransformType.Translate:
					if (inWorldSpace)
					{
						base.transform.position = _newVector;
					}
					else
					{
						base.transform.localPosition = _newVector;
					}
					positionChangeTime = 0f;
					break;
				case TransformType.Rotate:
					if (inWorldSpace)
					{
						base.transform.eulerAngles = _newVector;
					}
					else
					{
						base.transform.localEulerAngles = _newVector;
					}
					rotateChangeTime = 0f;
					break;
				case TransformType.Scale:
					if (inWorldSpace)
					{
						Transform parent = base.transform.parent;
						base.transform.SetParent(null, true);
						base.transform.localScale = _newVector;
						if ((bool)parent)
						{
							base.transform.SetParent(parent, true);
						}
					}
					else
					{
						base.transform.localScale = _newVector;
					}
					scaleChangeTime = 0f;
					break;
				}
				return;
			}
			switch (_transformType)
			{
			case TransformType.Translate:
				startPosition = (endPosition = ((!inWorldSpace) ? base.transform.localPosition : base.transform.position));
				endPosition = _newVector;
				positionMethod = _moveMethod;
				positionChangeTime = _transitionTime;
				positionStartTime = Time.time;
				positionMethod = _moveMethod;
				if (positionMethod == MoveMethod.CustomCurve)
				{
					positionTimeCurve = _timeCurve;
				}
				else
				{
					positionTimeCurve = null;
				}
				if (startPosition == endPosition)
				{
					Move(_newVector, _moveMethod, _inWorldSpace, 0f, _transformType, _doEulerRotation, _timeCurve, clearExisting);
				}
				else if (clearExisting)
				{
					rotateChangeTime = (scaleChangeTime = 0f);
				}
				break;
			case TransformType.Rotate:
				startEulerRotation = (endEulerRotation = ((!inWorldSpace) ? base.transform.localEulerAngles : base.transform.eulerAngles));
				startRotation = (endRotation = ((!inWorldSpace) ? base.transform.localRotation : base.transform.rotation));
				endRotation = Quaternion.Euler(_newVector);
				endEulerRotation = _newVector;
				doEulerRotation = _doEulerRotation;
				rotateMethod = _moveMethod;
				rotateChangeTime = _transitionTime;
				rotateStartTime = Time.time;
				rotateMethod = _moveMethod;
				if (rotateMethod == MoveMethod.CustomCurve)
				{
					rotateTimeCurve = _timeCurve;
				}
				else
				{
					rotateTimeCurve = null;
				}
				if ((doEulerRotation && startEulerRotation == endEulerRotation) || (!doEulerRotation && startRotation == endRotation))
				{
					Move(_newVector, _moveMethod, _inWorldSpace, 0f, _transformType, _doEulerRotation, _timeCurve, clearExisting);
				}
				else if (clearExisting)
				{
					positionChangeTime = (scaleChangeTime = 0f);
				}
				break;
			case TransformType.Scale:
				if (inWorldSpace)
				{
					ACDebug.LogWarning("Cannot change the world-space scale value of " + base.gameObject.name + " over time.", base.gameObject);
				}
				startScale = (endScale = base.transform.localScale);
				endScale = _newVector;
				scaleMethod = _moveMethod;
				scaleChangeTime = _transitionTime;
				scaleStartTime = Time.time;
				scaleMethod = _moveMethod;
				if (scaleMethod == MoveMethod.CustomCurve)
				{
					scaleTimeCurve = _timeCurve;
				}
				else
				{
					scaleTimeCurve = null;
				}
				if (startScale == endScale)
				{
					Move(_newVector, _moveMethod, _inWorldSpace, 0f, _transformType, _doEulerRotation, _timeCurve, clearExisting);
				}
				else if (clearExisting)
				{
					positionChangeTime = (rotateChangeTime = 0f);
				}
				break;
			}
		}

		public void Move(Marker _marker, MoveMethod _moveMethod, bool _inWorldSpace, float _transitionTime, AnimationCurve _timeCurve)
		{
			if (_rigidbody != null && !_rigidbody.isKinematic)
			{
				Rigidbody rigidbody = _rigidbody;
				Vector3 zero = Vector3.zero;
				_rigidbody.angularVelocity = zero;
				rigidbody.linearVelocity = zero;
			}
			inWorldSpace = _inWorldSpace;
			if (_transitionTime <= 0f)
			{
				positionChangeTime = (rotateChangeTime = (scaleChangeTime = 0f));
				if (inWorldSpace)
				{
					Transform parent = base.transform.parent;
					base.transform.SetParent(null, true);
					base.transform.localScale = _marker.transform.lossyScale;
					base.transform.position = _marker.transform.position;
					base.transform.rotation = _marker.transform.rotation;
					if ((bool)parent)
					{
						base.transform.SetParent(parent, true);
					}
				}
				else
				{
					base.transform.localPosition = _marker.transform.localPosition;
					base.transform.localEulerAngles = _marker.transform.localEulerAngles;
					base.transform.localScale = _marker.transform.localScale;
				}
				return;
			}
			doEulerRotation = false;
			positionMethod = (rotateMethod = (scaleMethod = _moveMethod));
			if (inWorldSpace)
			{
				startPosition = base.transform.position;
				startRotation = base.transform.rotation;
				startScale = base.transform.localScale;
				endPosition = _marker.transform.position;
				endRotation = _marker.transform.rotation;
				endScale = _marker.transform.localScale;
			}
			else
			{
				startPosition = base.transform.localPosition;
				startRotation = base.transform.localRotation;
				startScale = base.transform.localScale;
				endPosition = _marker.transform.localPosition;
				endRotation = _marker.transform.localRotation;
				endScale = _marker.transform.localScale;
			}
			if (startPosition == endPosition && startRotation == endRotation && startScale == endScale)
			{
				Move(_marker, _moveMethod, _inWorldSpace, 0f, _timeCurve);
				return;
			}
			positionChangeTime = (rotateChangeTime = (scaleChangeTime = _transitionTime));
			positionStartTime = (rotateStartTime = (scaleStartTime = Time.time));
			if (_moveMethod == MoveMethod.CustomCurve)
			{
				positionTimeCurve = _timeCurve;
				rotateTimeCurve = _timeCurve;
				scaleTimeCurve = _timeCurve;
			}
			else
			{
				positionTimeCurve = (rotateTimeCurve = (scaleTimeCurve = null));
			}
		}

		public MoveableData SaveData(MoveableData saveData)
		{
			if (positionChangeTime > 0f)
			{
				saveData.LocX = endPosition.x;
				saveData.LocY = endPosition.y;
				saveData.LocZ = endPosition.z;
			}
			if (rotateChangeTime > 0f)
			{
				saveData.doEulerRotation = doEulerRotation;
				if (doEulerRotation)
				{
					saveData.LocX = endEulerRotation.x;
					saveData.LocY = endEulerRotation.y;
					saveData.LocZ = endEulerRotation.z;
				}
				else
				{
					saveData.RotW = endRotation.w;
					saveData.RotX = endRotation.x;
					saveData.RotY = endRotation.y;
					saveData.RotZ = endRotation.z;
				}
			}
			else
			{
				saveData.doEulerRotation = true;
			}
			if (scaleChangeTime > 0f)
			{
				saveData.ScaleX = endScale.x;
				saveData.ScaleY = endScale.y;
				saveData.ScaleZ = endScale.z;
			}
			saveData.inWorldSpace = inWorldSpace;
			return saveData;
		}

		public void LoadData(MoveableData saveData)
		{
			inWorldSpace = saveData.inWorldSpace;
			if (!saveData.doEulerRotation)
			{
				if (inWorldSpace)
				{
					base.transform.rotation = new Quaternion(saveData.RotW, saveData.RotX, saveData.RotY, saveData.RotZ);
				}
				else
				{
					base.transform.localRotation = new Quaternion(saveData.RotW, saveData.RotX, saveData.RotY, saveData.RotZ);
				}
			}
			StopMoving();
		}

		protected void Kill()
		{
			StopMoving();
		}
	}
}
