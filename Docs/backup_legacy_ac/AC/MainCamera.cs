using System;
using System.Collections;
using UnityEngine;
using UnityEngine.XR;

namespace AC
{
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_main_camera.html")]
	[ExecuteInEditMode]
	public class MainCamera : MonoBehaviour
	{
		protected enum MainCameraMode
		{
			NormalSnap = 0,
			NormalTransition = 1
		}

		protected class GameCameraData
		{
			public Vector3 position { get; private set; }

			public Quaternion rotation { get; private set; }

			public bool isOrthographic { get; private set; }

			public float fieldOfView { get; private set; }

			public float orthographicSize { get; private set; }

			public float focalDistance { get; private set; }

			public bool is2D { get; private set; }

			public Vector2 perspectiveOffset { get; private set; }

			public GameCameraData()
			{
			}

			public GameCameraData(MainCamera mainCamera)
			{
				position = mainCamera.transform.position;
				rotation = mainCamera.transform.rotation;
				fieldOfView = mainCamera.Camera.fieldOfView;
				isOrthographic = mainCamera.Camera.orthographic;
				orthographicSize = mainCamera.Camera.orthographicSize;
				focalDistance = mainCamera.GetFocalDistance();
				is2D = false;
				perspectiveOffset = Vector2.zero;
			}

			public GameCameraData(_Camera _camera)
			{
				position = _camera.CameraTransform.position;
				rotation = _camera.CameraTransform.rotation;
				is2D = _camera.Is2D();
				Vector2 vector = _camera.CreateRotationOffset();
				if (is2D)
				{
					if (_camera.Camera.orthographic)
					{
						position += (Vector3)vector;
					}
				}
				else if (_camera.Camera.orthographic)
				{
					position += _camera.transform.right * vector.x + _camera.transform.up * vector.y;
				}
				else
				{
					rotation *= Quaternion.Euler(5f * new Vector3(0f - vector.y, vector.x, 0f));
				}
				fieldOfView = _camera.Camera.fieldOfView;
				isOrthographic = _camera.Camera.orthographic;
				orthographicSize = _camera.Camera.orthographicSize;
				focalDistance = _camera.focalDistance;
				perspectiveOffset = ((!is2D) ? Vector2.zero : _camera.GetPerspectiveOffset());
				if (is2D && !_camera.Camera.orthographic)
				{
					perspectiveOffset += vector;
				}
			}

			public GameCameraData CreateMix(GameCameraData otherData, float otherDataWeight, bool slerpRotation = false)
			{
				if (otherDataWeight <= 0f)
				{
					return this;
				}
				if (otherDataWeight >= 1f)
				{
					return otherData;
				}
				GameCameraData gameCameraData = new GameCameraData();
				gameCameraData.is2D = otherData.is2D;
				if (gameCameraData.is2D)
				{
					float x = AdvGame.Lerp(perspectiveOffset.x, otherData.perspectiveOffset.x, otherDataWeight);
					float y = AdvGame.Lerp(perspectiveOffset.y, otherData.perspectiveOffset.y, otherDataWeight);
					gameCameraData.perspectiveOffset = new Vector2(x, y);
				}
				gameCameraData.position = Vector3.Lerp(position, otherData.position, otherDataWeight);
				gameCameraData.rotation = ((!slerpRotation) ? Quaternion.Slerp(rotation, otherData.rotation, otherDataWeight) : Quaternion.Lerp(rotation, otherData.rotation, otherDataWeight));
				gameCameraData.isOrthographic = otherData.isOrthographic;
				gameCameraData.fieldOfView = Mathf.Lerp(fieldOfView, otherData.fieldOfView, otherDataWeight);
				gameCameraData.orthographicSize = Mathf.Lerp(orthographicSize, otherData.orthographicSize, otherDataWeight);
				gameCameraData.focalDistance = Mathf.Lerp(focalDistance, otherData.focalDistance, otherDataWeight);
				return gameCameraData;
			}
		}

		[SerializeField]
		protected Texture2D fadeTexture;

		protected Texture2D tempFadeTexture;

		[NonSerialized]
		public _Camera attachedCamera;

		[NonSerialized]
		public _Camera lastNavCamera;

		protected _Camera lastNavCamera2;

		[SerializeField]
		protected bool renderFading = true;

		protected _Camera transitionFromCamera;

		protected MainCameraMode mainCameraMode;

		protected bool isCrossfading;

		protected Texture2D crossfadeTexture;

		protected Vector2 perspectiveOffset = new Vector2(0f, 0f);

		protected float fadeDuration;

		protected float fadeTimer;

		protected int drawDepth = -1000;

		protected float alpha;

		protected FadeType fadeType;

		protected bool hideSceneWhileLoading;

		protected GameCameraData currentFrameCameraData;

		protected MoveMethod moveMethod;

		protected float transitionDuration;

		protected float transitionTimer;

		protected AnimationCurve timeCurve;

		protected _Camera previousAttachedCamera;

		protected GameCameraData oldCameraData;

		protected bool retainPreviousSpeed;

		protected Texture2D actualFadeTexture;

		protected float shakeStartTime;

		protected float shakeDuration;

		protected float shakeStartIntensity;

		protected CameraShakeEffect shakeEffect;

		protected float shakeIntensity;

		protected Vector3 shakePosition;

		protected Vector3 shakeRotation;

		protected Camera borderCam;

		protected float borderWidth;

		protected MenuOrientation borderOrientation;

		protected Rect borderRect1 = new Rect(0f, 0f, 0f, 0f);

		protected Rect borderRect2 = new Rect(0f, 0f, 0f, 0f);

		protected Rect midBorderRect = new Rect(0f, 0f, 0f, 0f);

		protected Vector2 aspectRatioScaleCorrection = Vector2.zero;

		protected Vector2 aspectRatioOffsetCorrection = Vector2.zero;

		protected bool isSplitScreen;

		protected bool isTopLeftSplit;

		protected CameraSplitOrientation splitOrientation;

		protected _Camera splitCamera;

		protected float splitAmountMain = 0.49f;

		protected float splitAmountOther = 0.49f;

		private Rect overlayRect;

		private float overlayDepthBackup;

		protected float focalDistance = 10f;

		protected Camera ownCamera;

		protected AudioListener _audioListener;

		protected bool timelineOverride;

		protected bool timelineFadeOverride;

		protected Texture2D timelineFadeTexture;

		protected float timelineFadeWeight;

		protected Rect safeScreenRectInverted;

		protected Rect playableScreenRect;

		protected Rect playableScreenRectRelative;

		protected Rect playableScreenRectInverted;

		protected Rect playableScreenRectRelativeInverted;

		private Rect lastSafeRect;

		private float lastAspectRatio;

		public bool restoreTransformOnLoadVR;

		public MainCameraForwardDirection forwardDirection;

		public Camera Camera
		{
			get
			{
				if (ownCamera == null)
				{
					ownCamera = GetComponent<Camera>();
					if (ownCamera == null)
					{
						ownCamera = GetComponentInChildren<Camera>();
						if (ownCamera == null)
						{
							ACDebug.LogError("The MainCamera script requires a Camera component.", base.gameObject);
						}
					}
				}
				return ownCamera;
			}
		}

		protected AudioListener AudioListener
		{
			get
			{
				if (_audioListener == null)
				{
					_audioListener = GetComponent<AudioListener>();
					if (_audioListener == null && Camera != null)
					{
						_audioListener = Camera.GetComponent<AudioListener>();
					}
					if (_audioListener == null)
					{
						ACDebug.LogWarning("No AudioListener found on the MainCamera!", base.gameObject);
					}
				}
				return _audioListener;
			}
		}

		public void OnAwake(bool hideWhileLoading = true)
		{
			base.gameObject.tag = "MainCamera";
			if (hideWhileLoading)
			{
				hideSceneWhileLoading = true;
			}
			if ((bool)base.transform.parent && base.transform.parent.name != "_Cameras")
			{
				ACDebug.LogWarning("Note: The MainCamera is parented to an unknown object. Be careful when moving the parent, as it may cause mis-alignment when the MainCamera is attached to a GameCamera.", this);
			}
			if (KickStarter.settingsManager != null && KickStarter.settingsManager.forceAspectRatio)
			{
				KickStarter.settingsManager.landscapeModeOnly = false;
			}
			RecalculateRects();
		}

		public void OnStart()
		{
			AssignFadeTexture();
			if (KickStarter.sceneChanger != null)
			{
				SetFadeTexture(KickStarter.sceneChanger.GetAndResetTransitionTexture());
			}
			if (KickStarter.playerMenus.ArePauseMenusOn())
			{
				hideSceneWhileLoading = false;
			}
			else
			{
				StartCoroutine("ShowScene");
			}
		}

		public void _LateUpdate()
		{
			if ((bool)KickStarter.settingsManager && KickStarter.settingsManager.IsInLoadingScene())
			{
				return;
			}
			if (lastSafeRect != ACScreen.safeArea || lastAspectRatio != KickStarter.settingsManager.AspectRatio)
			{
				if (lastSafeRect.width > 0f)
				{
					KickStarter.playerMenus.RecalculateAll();
				}
				lastSafeRect = ACScreen.safeArea;
				lastAspectRatio = KickStarter.settingsManager.AspectRatio;
			}
			UpdateCameraFade();
			UpdateLastGameplayCamera();
			if ((bool)attachedCamera && !(attachedCamera is GameCamera25D))
			{
				if (mainCameraMode == MainCameraMode.NormalSnap)
				{
					currentFrameCameraData = new GameCameraData(attachedCamera);
				}
				else if (mainCameraMode == MainCameraMode.NormalTransition)
				{
					UpdateCameraTransition();
				}
				if (!timelineOverride)
				{
					ApplyCameraData(currentFrameCameraData);
				}
			}
			else if ((bool)attachedCamera && attachedCamera is GameCamera25D)
			{
				base.transform.position = attachedCamera.CameraTransform.position;
				base.transform.rotation = attachedCamera.CameraTransform.rotation;
				perspectiveOffset = attachedCamera.GetPerspectiveOffset();
				if (AllowProjectionShifting(Camera))
				{
					Camera.projectionMatrix = AdvGame.SetVanishingPoint(Camera, perspectiveOffset);
				}
				currentFrameCameraData = new GameCameraData(this);
			}
			if (KickStarter.stateHandler.gameState == GameState.Paused)
			{
				return;
			}
			if (shakeIntensity > 0f)
			{
				if (shakeEffect != CameraShakeEffect.Rotate)
				{
					shakePosition = UnityEngine.Random.insideUnitSphere * shakeIntensity * 0.5f;
				}
				if (shakeEffect != CameraShakeEffect.Translate)
				{
					shakeRotation = new Vector3(UnityEngine.Random.Range(0f - shakeIntensity, shakeIntensity) * 0.2f, UnityEngine.Random.Range(0f - shakeIntensity, shakeIntensity) * 0.2f, UnityEngine.Random.Range(0f - shakeIntensity, shakeIntensity) * 0.2f);
				}
				shakeIntensity = Mathf.Lerp(shakeStartIntensity, 0f, AdvGame.Interpolate(shakeStartTime, shakeDuration, MoveMethod.Linear));
				base.transform.position += shakePosition;
				base.transform.localEulerAngles += shakeRotation;
			}
			else if (shakeIntensity < 0f)
			{
				StopShaking();
			}
		}

		protected virtual void OnDestroy()
		{
			crossfadeTexture = null;
		}

		public void SetDefaultFadeTexture(Texture2D _fadeTexture)
		{
			if (_fadeTexture != null)
			{
				fadeTexture = _fadeTexture;
			}
		}

		public void RecalculateRects()
		{
			if (SetAspectRatio())
			{
				CreateBorderCamera();
			}
			CalculatePlayableScreenArea();
			SetCameraRect();
			CalculateUnityUIAspectRatioCorrection();
			if (isSplitScreen && (bool)splitCamera)
			{
				splitCamera.SetSplitScreen();
			}
			if (Application.isPlaying)
			{
				KickStarter.eventManager.Call_OnUpdatePlayableScreenArea();
			}
		}

		public void PauseGame(bool canWait = false)
		{
			if (hideSceneWhileLoading)
			{
				if (canWait)
				{
					StartCoroutine("PauseWhenLoaded");
				}
			}
			else
			{
				KickStarter.stateHandler.gameState = GameState.Paused;
				KickStarter.sceneSettings.PauseGame();
			}
		}

		public void CancelPauseGame()
		{
			StopCoroutine("PauseWhenLoaded");
		}

		public void HideScene()
		{
			hideSceneWhileLoading = true;
			StartCoroutine("ShowScene");
		}

		public void Shake(float _shakeIntensity, float _duration, CameraShakeEffect _shakeEffect)
		{
			shakePosition = Vector3.zero;
			shakeRotation = Vector3.zero;
			shakeEffect = _shakeEffect;
			shakeDuration = _duration;
			shakeStartTime = Time.time;
			shakeIntensity = _shakeIntensity;
			shakeStartIntensity = shakeIntensity;
			KickStarter.eventManager.Call_OnShakeCamera(shakeIntensity, shakeDuration);
		}

		public bool IsShaking()
		{
			if (shakeIntensity > 0f)
			{
				return true;
			}
			return false;
		}

		public void StopShaking()
		{
			shakeIntensity = 0f;
			shakePosition = Vector3.zero;
			shakeRotation = Vector3.zero;
			KickStarter.eventManager.Call_OnShakeCamera(0f, 0f);
		}

		public virtual void PrepareForBackground()
		{
			Camera.clearFlags = CameraClearFlags.Depth;
			if (LayerMask.NameToLayer(KickStarter.settingsManager.backgroundImageLayer) != -1)
			{
				Camera.cullingMask &= ~(1 << LayerMask.NameToLayer(KickStarter.settingsManager.backgroundImageLayer));
			}
		}

		public void SetFirstPerson()
		{
			if (KickStarter.player == null)
			{
				ACDebug.LogWarning("Cannot set first-person camera because no Player can be found!");
				return;
			}
			FirstPersonCamera componentInChildren = KickStarter.player.GetComponentInChildren<FirstPersonCamera>();
			if ((bool)componentInChildren)
			{
				SetGameCamera(componentInChildren);
			}
			if ((bool)attachedCamera)
			{
				if (lastNavCamera != attachedCamera)
				{
					lastNavCamera2 = lastNavCamera;
				}
				lastNavCamera = attachedCamera;
			}
		}

		public void DrawCameraFade()
		{
			if (timelineFadeOverride)
			{
				Color color = GUI.color;
				Color color2 = GUI.color;
				color2.a = timelineFadeWeight;
				GUI.color = color2;
				GUI.depth = drawDepth;
				GUI.DrawTexture(new Rect(0f, 0f, ACScreen.width, ACScreen.height), timelineFadeTexture);
				GUI.color = color;
			}
			else if (hideSceneWhileLoading && actualFadeTexture != null)
			{
				if (renderFading)
				{
					GUI.DrawTexture(new Rect(0f, 0f, ACScreen.width, ACScreen.height), actualFadeTexture);
				}
			}
			else if (alpha > 0f)
			{
				Color color3 = GUI.color;
				color3.a = alpha;
				GUI.color = color3;
				GUI.depth = drawDepth;
				if (isCrossfading)
				{
					if ((bool)crossfadeTexture)
					{
						GUI.DrawTexture(new Rect(0f, 0f, ACScreen.width, ACScreen.height), crossfadeTexture);
					}
					else
					{
						ACDebug.LogWarning("Cannot crossfade as the crossfade texture was not succesfully generated.");
					}
				}
				else if (renderFading)
				{
					if ((bool)actualFadeTexture)
					{
						GUI.DrawTexture(new Rect(0f, 0f, ACScreen.width, ACScreen.height), actualFadeTexture);
					}
					else
					{
						ACDebug.LogWarning("Cannot fade camera as no fade texture has been assigned.");
					}
				}
			}
			else if (actualFadeTexture != fadeTexture && !isFading())
			{
				ReleaseFadeTexture();
			}
		}

		public float GetFadeAlpha()
		{
			return alpha;
		}

		public Texture2D GetFadeTexture()
		{
			AssignFadeTexture();
			return actualFadeTexture;
		}

		public void ResetProjection()
		{
			if (Camera != null)
			{
				perspectiveOffset = Vector2.zero;
				Camera.projectionMatrix = AdvGame.SetVanishingPoint(Camera, perspectiveOffset);
				Camera.ResetProjectionMatrix();
			}
		}

		public void ResetMoving()
		{
			mainCameraMode = MainCameraMode.NormalSnap;
			transitionTimer = 0f;
			transitionDuration = 0f;
		}

		public void SnapToAttached()
		{
			if ((bool)attachedCamera && (bool)attachedCamera.Camera)
			{
				ResetMoving();
				transitionFromCamera = null;
				bool flag = previousAttachedCamera != null && previousAttachedCamera.transform.rotation != attachedCamera.transform.rotation;
				currentFrameCameraData = new GameCameraData(attachedCamera);
				ApplyCameraData(currentFrameCameraData);
				if (flag && !SceneSettings.IsUnity2D() && KickStarter.stateHandler.IsInGameplay() && KickStarter.settingsManager.movementMethod == MovementMethod.Direct && KickStarter.settingsManager.directMovementType == DirectMovementType.RelativeToCamera && KickStarter.playerInput != null && KickStarter.player != null && (KickStarter.player.GetPath() == null || !KickStarter.player.IsLockedToPath()))
				{
					KickStarter.playerInput.cameraLockSnap = true;
				}
			}
		}

		public void Crossfade(float _transitionDuration, _Camera _linkedCamera)
		{
			object[] value = new object[2] { _transitionDuration, _linkedCamera };
			StartCoroutine("StartCrossfade", value);
		}

		public void StopCrossfade()
		{
			StopCoroutine("StartCrossfade");
			if (isCrossfading)
			{
				isCrossfading = false;
				alpha = 0f;
			}
			UnityEngine.Object.Destroy(crossfadeTexture);
			crossfadeTexture = null;
		}

		public void _ExitSceneWithOverlay()
		{
			StartCoroutine("ExitSceneWithOverlay");
		}

		public _Camera GetTransitionFromCamera()
		{
			if (mainCameraMode == MainCameraMode.NormalTransition)
			{
				return transitionFromCamera;
			}
			return null;
		}

		public void SetGameCamera(_Camera newCamera, float transitionTime = 0f, MoveMethod _moveMethod = MoveMethod.Linear, AnimationCurve _animationCurve = null, bool _retainPreviousSpeed = false, bool snapCamera = true)
		{
			if (newCamera == null)
			{
				return;
			}
			if (KickStarter.eventManager != null)
			{
				KickStarter.eventManager.Call_OnSwitchCamera(attachedCamera, newCamera, transitionTime);
			}
			if (attachedCamera != null && attachedCamera is GameCamera25D)
			{
				transitionTime = 0f;
				if (!(newCamera is GameCamera25D))
				{
					RemoveBackground();
				}
			}
			previousAttachedCamera = attachedCamera;
			oldCameraData = currentFrameCameraData;
			if (oldCameraData == null)
			{
				oldCameraData = new GameCameraData(this);
			}
			retainPreviousSpeed = mainCameraMode == MainCameraMode.NormalSnap && _retainPreviousSpeed;
			Camera.ResetProjectionMatrix();
			if (newCamera != attachedCamera && transitionTime > 0f)
			{
				transitionFromCamera = attachedCamera;
			}
			else
			{
				transitionFromCamera = null;
			}
			attachedCamera = newCamera;
			UpdateLastGameplayCamera();
			if ((bool)attachedCamera && (bool)attachedCamera.Camera)
			{
				Camera.farClipPlane = attachedCamera.Camera.farClipPlane;
				Camera.nearClipPlane = attachedCamera.Camera.nearClipPlane;
			}
			if (attachedCamera is GameCamera25D)
			{
				GameCamera25D gameCamera25D = (GameCamera25D)attachedCamera;
				gameCamera25D.SetActiveBackground();
			}
			if (attachedCamera is GameCamera2D)
			{
				Camera.transparencySortMode = TransparencySortMode.Orthographic;
			}
			else if ((bool)attachedCamera)
			{
				if (attachedCamera.Camera.orthographic)
				{
					Camera.transparencySortMode = TransparencySortMode.Orthographic;
				}
				else
				{
					Camera.transparencySortMode = TransparencySortMode.Perspective;
				}
			}
			KickStarter.stateHandler.LimitHotspotsToCamera(attachedCamera);
			if (transitionTime > 0f)
			{
				SmoothChange(transitionTime, _moveMethod, _animationCurve);
			}
			else if (attachedCamera != null)
			{
				if (snapCamera)
				{
					attachedCamera.MoveCameraInstant();
				}
				SnapToAttached();
			}
		}

		public void SetFadeTexture(Texture2D tex)
		{
			if (tex != null)
			{
				tempFadeTexture = tex;
				AssignFadeTexture();
			}
			else
			{
				ReleaseFadeTexture();
			}
		}

		public void FadeOut(float _fadeDuration, Texture2D tempTex, bool forceCompleteTransition = true)
		{
			if (tempTex != null)
			{
				SetFadeTexture(tempTex);
			}
			FadeOut(_fadeDuration, forceCompleteTransition);
		}

		public virtual void FadeIn(float _fadeDuration, bool forceCompleteTransition = true)
		{
			AssignFadeTexture();
			if ((forceCompleteTransition || alpha > 0f) && _fadeDuration > 0f)
			{
				fadeDuration = _fadeDuration;
				if (forceCompleteTransition)
				{
					alpha = 1f;
					fadeTimer = _fadeDuration;
				}
				else
				{
					fadeTimer = _fadeDuration * alpha;
				}
				fadeType = FadeType.fadeIn;
			}
			else
			{
				alpha = 0f;
				fadeTimer = (fadeDuration = 0f);
				ReleaseFadeTexture();
			}
		}

		public virtual void FadeOut(float _fadeDuration, bool forceCompleteTransition = true)
		{
			AssignFadeTexture();
			if (alpha <= 0f)
			{
				alpha = 0.01f;
			}
			if ((forceCompleteTransition || alpha < 1f) && _fadeDuration > 0f)
			{
				if (forceCompleteTransition)
				{
					alpha = 0.01f;
					fadeTimer = _fadeDuration;
				}
				else
				{
					alpha = Mathf.Clamp01(alpha);
					fadeTimer = _fadeDuration * (1f - alpha);
				}
				fadeDuration = _fadeDuration;
				fadeType = FadeType.fadeOut;
			}
			else
			{
				alpha = 1f;
				fadeTimer = (fadeDuration = 0f);
			}
		}

		public virtual bool isFading()
		{
			if (fadeType == FadeType.fadeOut && alpha < 1f)
			{
				return true;
			}
			if (fadeType == FadeType.fadeIn && alpha > 0f)
			{
				return true;
			}
			return false;
		}

		public Vector3 PositionRelativeToCamera(Vector3 _position)
		{
			return _position.x * ForwardVector() + _position.z * RightVector();
		}

		public Vector3 RightVector()
		{
			return (forwardDirection != MainCameraForwardDirection.CameraComponent) ? base.transform.right : ownCamera.transform.right;
		}

		public Vector3 ForwardVector()
		{
			Vector3 result = ((forwardDirection != MainCameraForwardDirection.CameraComponent) ? base.transform.forward : ownCamera.transform.forward);
			result.y = 0f;
			return result;
		}

		public void SetCameraRect()
		{
			if (!Application.isPlaying)
			{
				return;
			}
			if (SetAspectRatio() && Application.isPlaying)
			{
				CreateBorderCamera();
			}
			if (isSplitScreen)
			{
				Camera.rect = GetSplitScreenRect(true);
			}
			else
			{
				Rect rect = default(Rect);
				Rect rect2 = new Rect(ACScreen.safeArea.x / (float)ACScreen.width, ACScreen.safeArea.y / (float)ACScreen.height, ACScreen.safeArea.width / (float)ACScreen.width, ACScreen.safeArea.height / (float)ACScreen.height);
				if (borderOrientation == MenuOrientation.Vertical)
				{
					float num = borderWidth * rect2.width;
					rect = new Rect(num + rect2.x, rect2.y, rect2.width - 2f * num, rect2.height);
				}
				else if (borderOrientation == MenuOrientation.Horizontal)
				{
					float num2 = borderWidth * rect2.height;
					rect = new Rect(rect2.x, rect2.y + num2, rect2.width, rect2.height - 2f * num2);
				}
				Camera.rect = rect;
			}
			if (!KickStarter.stateHandler)
			{
				return;
			}
			foreach (BackgroundCamera backgroundCamera in KickStarter.stateHandler.BackgroundCameras)
			{
				backgroundCamera.UpdateRect();
			}
		}

		public Vector2 CorrectScreenPositionForUnityUI(Vector2 screenPosition)
		{
			return new Vector2(screenPosition.x * aspectRatioScaleCorrection.x + aspectRatioOffsetCorrection.x, screenPosition.y * aspectRatioScaleCorrection.y + aspectRatioOffsetCorrection.y);
		}

		public Vector2 GetWindowViewportDifference()
		{
			return aspectRatioOffsetCorrection;
		}

		public void DrawBorders()
		{
			if (!Application.isPlaying)
			{
				if (AdvGame.GetReferences() == null || AdvGame.GetReferences().settingsManager == null || !AdvGame.GetReferences().settingsManager.forceAspectRatio)
				{
					return;
				}
				SetAspectRatio();
			}
			if (borderWidth > 0f)
			{
				if (fadeTexture == null)
				{
					ACDebug.LogWarning("Cannot draw camera borders because no Fade texture is assigned in the MainCamera!");
					return;
				}
				GUI.depth = 10;
				GUI.DrawTexture(borderRect1, fadeTexture);
				GUI.DrawTexture(borderRect2, fadeTexture);
			}
			else if (isSplitScreen && splitOrientation != CameraSplitOrientation.Overlay)
			{
				if (fadeTexture == null)
				{
					ACDebug.LogWarning("Cannot draw camera borders because no Fade texture is assigned in the MainCamera!", base.gameObject);
					return;
				}
				GUI.depth = 10;
				GUI.DrawTexture(midBorderRect, fadeTexture);
			}
			if (Application.isPlaying)
			{
				if (ACScreen.safeArea.x > 0f)
				{
					GUI.DrawTexture(new Rect(0f, 0f, ACScreen.safeArea.x, ACScreen.height), fadeTexture);
				}
				if (ACScreen.safeArea.y > 0f)
				{
					GUI.DrawTexture(new Rect(0f, (float)ACScreen.height - ACScreen.safeArea.y, ACScreen.width, ACScreen.safeArea.y), fadeTexture);
				}
				if (ACScreen.safeArea.width < (float)ACScreen.width - ACScreen.safeArea.x)
				{
					GUI.DrawTexture(new Rect(ACScreen.safeArea.x + ACScreen.safeArea.width, 0f, (float)ACScreen.width - ACScreen.safeArea.width, ACScreen.height), fadeTexture);
				}
				if (ACScreen.safeArea.height < (float)ACScreen.height - ACScreen.safeArea.y)
				{
					GUI.DrawTexture(new Rect(0f, 0f, ACScreen.width, (float)ACScreen.height - ACScreen.safeArea.height - ACScreen.safeArea.y), fadeTexture);
				}
			}
		}

		public bool IsOrthographic()
		{
			if (Camera == null)
			{
				return false;
			}
			return Camera.orthographic;
		}

		public Vector2 LimitToAspect(Vector2 position)
		{
			if (!KickStarter.cursorManager.keepCursorWithinScreen && KickStarter.playerCursor.LimitCursorToMenu == null)
			{
				return position;
			}
			if (KickStarter.settingsManager.forceAspectRatio)
			{
				switch (borderOrientation)
				{
				case MenuOrientation.Horizontal:
					return LimitVector(position, 0f, borderWidth);
				case MenuOrientation.Vertical:
					return LimitVector(position, borderWidth, 0f);
				}
			}
			return LimitVector(position, 0f, 0f);
		}

		public bool IsPointInCamera(Vector2 point)
		{
			if (isSplitScreen)
			{
				point = new Vector2(point.x / (float)ACScreen.width, point.y / (float)ACScreen.height);
				return Camera.rect.Contains(point);
			}
			return true;
		}

		public Vector2 GetMainGameViewOffset()
		{
			Vector2 result = new Vector2(ACScreen.safeArea.x, (float)ACScreen.height - ACScreen.safeArea.height - ACScreen.safeArea.y);
			if (borderWidth > 0f)
			{
				if (borderOrientation == MenuOrientation.Horizontal)
				{
					result.y += ACScreen.safeArea.height * borderWidth;
				}
				else
				{
					result.x += ACScreen.safeArea.width * borderWidth;
				}
			}
			return result;
		}

		public Rect LimitMenuToAspect(Rect rect)
		{
			if (KickStarter.settingsManager == null || !KickStarter.settingsManager.forceAspectRatio)
			{
				return rect;
			}
			rect.position += ACScreen.safeArea.position;
			if (borderOrientation == MenuOrientation.Horizontal)
			{
				int num = (int)((float)ACScreen.height * borderWidth);
				if (rect.y < (float)num)
				{
					rect.y = num;
					if (rect.height > (float)(ACScreen.height - num - num))
					{
						rect.height = ACScreen.height - num - num;
					}
				}
				else if (rect.y + rect.height > (float)(ACScreen.height - num))
				{
					rect.y = (float)(ACScreen.height - num) - rect.height;
				}
			}
			else
			{
				int num2 = (int)((float)ACScreen.width * borderWidth);
				if (rect.x < (float)num2)
				{
					rect.x = num2;
					if (rect.width > (float)(ACScreen.width - num2 - num2))
					{
						rect.width = ACScreen.width - num2 - num2;
					}
				}
				else if (rect.x + rect.width > (float)(ACScreen.width - num2))
				{
					rect.x = (float)(ACScreen.width - num2) - rect.width;
				}
			}
			return rect;
		}

		public void SetSplitScreen(_Camera _camera1, _Camera _camera2, CameraSplitOrientation _splitOrientation, bool _isTopLeft, float _splitAmountMain, float _splitAmountOther)
		{
			splitCamera = _camera2;
			isSplitScreen = true;
			splitOrientation = _splitOrientation;
			isTopLeftSplit = _isTopLeft;
			SetGameCamera(_camera1);
			StartSplitScreen(_splitAmountMain, _splitAmountOther);
		}

		public void StartSplitScreen(float _splitAmountMain, float _splitAmountOther)
		{
			splitAmountMain = _splitAmountMain;
			splitAmountOther = _splitAmountOther;
			splitCamera.SetSplitScreen();
			SetCameraRect();
			SetMidBorder();
		}

		public void SetBoxOverlay(_Camera underlayCamera, _Camera overlayCamera, Rect _overlayRect, bool useRectCentre = true)
		{
			if (underlayCamera == null)
			{
				ACDebug.LogWarning("Cannot set box overlay because no underlay camera was set.");
				return;
			}
			if (overlayCamera == null)
			{
				ACDebug.LogWarning("Cannot set box overlay because no overlay camera was set.");
				return;
			}
			splitCamera = underlayCamera;
			isSplitScreen = true;
			splitOrientation = CameraSplitOrientation.Overlay;
			midBorderRect = new Rect(0f, 0f, 0f, 0f);
			SetGameCamera(overlayCamera);
			overlayDepthBackup = Camera.depth;
			Camera.depth = underlayCamera.Camera.depth + 1f;
			overlayRect = _overlayRect;
			if (useRectCentre)
			{
				overlayRect.center = new Vector2(_overlayRect.x, _overlayRect.y);
			}
			splitCamera.SetSplitScreen();
			SetCameraRect();
		}

		public void RemoveSplitScreen()
		{
			if (isSplitScreen && splitOrientation == CameraSplitOrientation.Overlay)
			{
				Camera.depth = overlayDepthBackup;
			}
			isSplitScreen = false;
			SetCameraRect();
			if ((bool)splitCamera)
			{
				splitCamera.RemoveSplitScreen();
				if (splitOrientation == CameraSplitOrientation.Overlay)
				{
					SetGameCamera(splitCamera);
				}
				splitCamera = null;
			}
		}

		public Rect GetSplitScreenRect(bool isMainCamera)
		{
			Rect playableScreenArea = GetPlayableScreenArea(true);
			if (splitOrientation == CameraSplitOrientation.Overlay)
			{
				if (isMainCamera)
				{
					return new Rect(playableScreenArea.x + playableScreenArea.width * overlayRect.x, playableScreenArea.y + playableScreenArea.height * overlayRect.y, playableScreenArea.width * overlayRect.width, playableScreenArea.height * overlayRect.height);
				}
				return playableScreenArea;
			}
			bool flag = ((!isMainCamera) ? (!isTopLeftSplit) : isTopLeftSplit);
			float num = ((!isMainCamera) ? splitAmountOther : splitAmountMain);
			Vector2 position = playableScreenArea.position;
			if (splitOrientation == CameraSplitOrientation.Horizontal && flag)
			{
				position.y += (1f - num) * playableScreenArea.height;
			}
			else if (splitOrientation == CameraSplitOrientation.Vertical && !flag)
			{
				position.x += (1f - num) * playableScreenArea.width;
			}
			Vector2 size = ((splitOrientation != CameraSplitOrientation.Horizontal) ? new Vector2(num * playableScreenArea.width, playableScreenArea.height) : new Vector2(playableScreenArea.width, num * playableScreenArea.height));
			return new Rect(position, size);
		}

		public float GetFocalDistance()
		{
			return focalDistance;
		}

		public void Disable()
		{
			if ((bool)Camera)
			{
				Camera.enabled = false;
			}
			if ((bool)AudioListener)
			{
				AudioListener.enabled = false;
			}
		}

		public void Enable()
		{
			if ((bool)Camera)
			{
				Camera.enabled = true;
			}
			if ((bool)AudioListener)
			{
				AudioListener.enabled = true;
			}
		}

		public bool IsEnabled()
		{
			if ((bool)Camera)
			{
				return Camera.enabled;
			}
			return false;
		}

		public void SetCameraTag(string _tag)
		{
			if (Camera != null)
			{
				Camera.gameObject.tag = _tag;
			}
		}

		public void SetAudioState(bool state)
		{
			if ((bool)AudioListener)
			{
				AudioListener.enabled = state;
			}
		}

		public _Camera GetLastGameplayCamera()
		{
			if (lastNavCamera != null)
			{
				if (lastNavCamera2 != null && attachedCamera == lastNavCamera)
				{
					return lastNavCamera2;
				}
				return lastNavCamera;
			}
			ACDebug.LogWarning("Could not get the last gameplay camera - was it previously set?");
			return null;
		}

		public Vector2 GetPerspectiveOffset()
		{
			return perspectiveOffset;
		}

		public PlayerData SaveData(PlayerData playerData)
		{
			SnapToAttached();
			if ((bool)attachedCamera)
			{
				playerData.gameCamera = Serializer.GetConstantID(attachedCamera.gameObject);
				if (KickStarter.sceneChanger.GetSubSceneIndexOfGameObject(attachedCamera.gameObject) > 0)
				{
					ACDebug.LogWarning("Cannot save the active camera '" + attachedCamera.gameObject.name + "' as it is not in the active scene.", attachedCamera.gameObject);
				}
			}
			if ((bool)lastNavCamera)
			{
				playerData.lastNavCamera = Serializer.GetConstantID(lastNavCamera.gameObject);
			}
			if ((bool)lastNavCamera2)
			{
				playerData.lastNavCamera2 = Serializer.GetConstantID(lastNavCamera2.gameObject);
			}
			if (shakeIntensity > 0f)
			{
				playerData.shakeIntensity = shakeIntensity;
				playerData.shakeDuration = shakeDuration;
				playerData.shakeEffect = (int)shakeEffect;
			}
			else
			{
				playerData.shakeIntensity = 0f;
				playerData.shakeDuration = 0f;
				playerData.shakeEffect = 0;
				StopShaking();
			}
			playerData.mainCameraLocX = base.transform.position.x;
			playerData.mainCameraLocY = base.transform.position.y;
			playerData.mainCameraLocZ = base.transform.position.z;
			playerData.mainCameraRotX = base.transform.eulerAngles.x;
			playerData.mainCameraRotY = base.transform.eulerAngles.y;
			playerData.mainCameraRotZ = base.transform.eulerAngles.z;
			playerData.isSplitScreen = isSplitScreen;
			if (isSplitScreen)
			{
				switch (splitOrientation)
				{
				case CameraSplitOrientation.Horizontal:
					playerData.splitIsVertical = false;
					playerData.overlayRectX = 0f;
					playerData.overlayRectY = 0f;
					playerData.overlayRectWidth = 0f;
					playerData.overlayRectHeight = 0f;
					playerData.isTopLeftSplit = isTopLeftSplit;
					playerData.splitAmountMain = splitAmountMain;
					playerData.splitAmountOther = splitAmountOther;
					break;
				case CameraSplitOrientation.Vertical:
					playerData.splitIsVertical = true;
					playerData.overlayRectX = 0f;
					playerData.overlayRectY = 0f;
					playerData.overlayRectWidth = 0f;
					playerData.overlayRectHeight = 0f;
					playerData.isTopLeftSplit = isTopLeftSplit;
					playerData.splitAmountMain = splitAmountMain;
					playerData.splitAmountOther = splitAmountOther;
					break;
				case CameraSplitOrientation.Overlay:
					playerData.splitIsVertical = false;
					playerData.overlayRectX = overlayRect.x;
					playerData.overlayRectY = overlayRect.y;
					playerData.overlayRectWidth = overlayRect.width;
					playerData.overlayRectHeight = overlayRect.height;
					playerData.isTopLeftSplit = false;
					playerData.splitAmountMain = overlayDepthBackup;
					playerData.splitAmountOther = 0f;
					break;
				}
				if ((bool)splitCamera && (bool)splitCamera.GetComponent<ConstantID>())
				{
					playerData.splitCameraID = splitCamera.GetComponent<ConstantID>().constantID;
				}
				else
				{
					playerData.splitCameraID = 0;
				}
			}
			return playerData;
		}

		public void LoadData(PlayerData playerData, bool snapCamera = true)
		{
			if (isSplitScreen)
			{
				RemoveSplitScreen();
			}
			StopShaking();
			if (playerData.shakeIntensity > 0f && playerData.shakeDuration > 0f)
			{
				Shake(playerData.shakeIntensity, playerData.shakeDuration, (CameraShakeEffect)playerData.shakeEffect);
			}
			_Camera camera = Serializer.returnComponent<_Camera>(playerData.gameCamera);
			if (camera != null)
			{
				if (attachedCamera != camera)
				{
					snapCamera = true;
				}
				if (snapCamera)
				{
					camera.MoveCameraInstant();
					SetGameCamera(camera);
				}
				else
				{
					SetGameCamera(camera, 0f, MoveMethod.Linear, null, false, false);
				}
			}
			else if (KickStarter.settingsManager.movementMethod == MovementMethod.FirstPerson && KickStarter.settingsManager.IsInFirstPerson())
			{
				SetFirstPerson();
			}
			lastNavCamera = Serializer.returnComponent<_Camera>(playerData.lastNavCamera);
			lastNavCamera2 = Serializer.returnComponent<_Camera>(playerData.lastNavCamera2);
			ResetMoving();
			if (!XRSettings.enabled || restoreTransformOnLoadVR)
			{
				base.transform.position = new Vector3(playerData.mainCameraLocX, playerData.mainCameraLocY, playerData.mainCameraLocZ);
				base.transform.eulerAngles = new Vector3(playerData.mainCameraRotX, playerData.mainCameraRotY, playerData.mainCameraRotZ);
				ResetProjection();
			}
			SnapToAttached();
			isSplitScreen = playerData.isSplitScreen;
			if (!isSplitScreen)
			{
				return;
			}
			if (playerData.splitCameraID != 0)
			{
				_Camera camera2 = Serializer.returnComponent<_Camera>(playerData.splitCameraID);
				if ((bool)camera2)
				{
					splitCamera = camera2;
				}
			}
			if (!Mathf.Approximately(playerData.overlayRectX, 0f) || !Mathf.Approximately(playerData.overlayRectY, 0f) || !Mathf.Approximately(playerData.overlayRectWidth, 0f) || !Mathf.Approximately(playerData.overlayRectHeight, 0f))
			{
				overlayRect = new Rect(playerData.overlayRectX, playerData.overlayRectY, playerData.overlayRectWidth, playerData.overlayRectHeight);
				SetBoxOverlay(splitCamera, attachedCamera, overlayRect, false);
				return;
			}
			isTopLeftSplit = playerData.isTopLeftSplit;
			if (playerData.splitIsVertical)
			{
				splitOrientation = CameraSplitOrientation.Vertical;
			}
			else
			{
				splitOrientation = CameraSplitOrientation.Horizontal;
			}
			StartSplitScreen(playerData.splitAmountMain, playerData.splitAmountOther);
		}

		public void DrawStatus()
		{
			if (IsEnabled())
			{
				if (timelineOverride)
				{
					GUILayout.Label("Current camera: Set by Timeline");
				}
				else if (attachedCamera != null && !GUILayout.Button("Current camera: " + attachedCamera.gameObject.name))
				{
				}
			}
			else
			{
				GUILayout.Label("MainCamera: Disabled");
			}
		}

		public void SetTimelineOverride(_Camera cam1, _Camera cam2, float cam2Weight, float _shakeIntensity = 0f)
		{
			if (currentFrameCameraData == null)
			{
				currentFrameCameraData = new GameCameraData(this);
			}
			timelineOverride = true;
			if (_shakeIntensity > 0f)
			{
				shakeEffect = CameraShakeEffect.TranslateAndRotate;
				shakeIntensity = _shakeIntensity * 0.2f;
			}
			else
			{
				shakeIntensity = 0f;
			}
			if (cam1 == null)
			{
				if (cam2 == null)
				{
					ReleaseTimelineOverride();
					return;
				}
				GameCameraData cameraData = new GameCameraData(cam2);
				ApplyCameraData(currentFrameCameraData, cameraData, cam2Weight);
			}
			else
			{
				ApplyCameraData(cam1, cam2, cam2Weight);
			}
		}

		public void ReleaseTimelineOverride()
		{
			timelineOverride = false;
			shakeIntensity = 0f;
		}

		public void SetTimelineFadeOverride(Texture2D _timelineFadeTexture, float _timelineFadeWeight)
		{
			if (_timelineFadeTexture == null)
			{
				ReleaseTimelineFadeOverride();
				return;
			}
			timelineFadeOverride = true;
			timelineFadeTexture = _timelineFadeTexture;
			timelineFadeWeight = _timelineFadeWeight;
		}

		public void ReleaseTimelineFadeOverride()
		{
			timelineFadeOverride = false;
		}

		public Rect GetPlayableScreenArea(bool relativeToScreenSize, bool invertY = false)
		{
			if (!Application.isPlaying)
			{
				RecalculateRects();
			}
			if (relativeToScreenSize)
			{
				return (!invertY) ? playableScreenRectRelative : playableScreenRectRelativeInverted;
			}
			return (!invertY) ? playableScreenRect : playableScreenRectInverted;
		}

		public Vector2 ConvertToMenuSpace(Vector2 point)
		{
			Vector2 vector = point;
			vector -= safeScreenRectInverted.position;
			vector.x /= ACScreen.safeArea.width / (float)ACScreen.width;
			vector.y /= ACScreen.safeArea.height / (float)ACScreen.height;
			return new Vector2(vector.x / (float)ACScreen.width, vector.y / (float)ACScreen.height);
		}

		public Vector2 ConvertRelativeScreenSpaceToUI(Vector2 point)
		{
			Vector2 vector = new Vector2(point.x, 1f - point.y);
			vector.x *= GetPlayableScreenArea(false).width;
			vector.y *= GetPlayableScreenArea(false).height;
			return vector + GetPlayableScreenArea(false).position;
		}

		protected IEnumerator PauseWhenLoaded()
		{
			while (hideSceneWhileLoading)
			{
				yield return new WaitForEndOfFrame();
			}
			KickStarter.stateHandler.gameState = GameState.Paused;
		}

		protected virtual void RemoveBackground()
		{
			Camera.clearFlags = CameraClearFlags.Skybox;
			if (LayerMask.NameToLayer(KickStarter.settingsManager.backgroundImageLayer) != -1)
			{
				Camera.cullingMask &= ~(1 << LayerMask.NameToLayer(KickStarter.settingsManager.backgroundImageLayer));
			}
		}

		protected void UpdateCameraFade()
		{
			if (!(fadeTimer > 0f))
			{
				return;
			}
			fadeTimer -= Time.deltaTime;
			alpha = 1f - fadeTimer / fadeDuration;
			if (fadeType == FadeType.fadeIn)
			{
				alpha = 1f - alpha;
			}
			alpha = Mathf.Clamp01(alpha);
			if (fadeTimer <= 0f)
			{
				if (fadeType == FadeType.fadeIn)
				{
					alpha = 0f;
				}
				else
				{
					alpha = 1f;
				}
				fadeDuration = (fadeTimer = 0f);
				StopCrossfade();
			}
		}

		protected void UpdateLastGameplayCamera()
		{
			if (KickStarter.stateHandler.IsInGameplay() && (bool)attachedCamera)
			{
				if (lastNavCamera != attachedCamera)
				{
					lastNavCamera2 = lastNavCamera;
				}
				lastNavCamera = attachedCamera;
			}
		}

		protected IEnumerator ShowScene()
		{
			yield return new WaitForSeconds(0.1f);
			hideSceneWhileLoading = false;
		}

		protected void UpdateCameraTransition()
		{
			if (!(transitionTimer > 0f))
			{
				return;
			}
			transitionTimer -= Time.deltaTime;
			if (transitionTimer <= 0f)
			{
				ResetMoving();
				return;
			}
			float weight = 1f - transitionTimer / transitionDuration;
			if (retainPreviousSpeed && previousAttachedCamera != null)
			{
				oldCameraData = new GameCameraData(previousAttachedCamera);
			}
			GameCameraData otherData = new GameCameraData(attachedCamera);
			float otherDataWeight = AdvGame.Interpolate(weight, moveMethod, timeCurve);
			currentFrameCameraData = oldCameraData.CreateMix(otherData, otherDataWeight, moveMethod == MoveMethod.Curved);
		}

		protected IEnumerator StartCrossfade(object[] parms)
		{
			float _transitionDuration = (float)parms[0];
			_Camera _linkedCamera = (_Camera)parms[1];
			yield return new WaitForEndOfFrame();
			crossfadeTexture = new Texture2D(ACScreen.width, ACScreen.height);
			crossfadeTexture.ReadPixels(new Rect(0f, 0f, ACScreen.width, ACScreen.height), 0, 0, false);
			crossfadeTexture.Apply();
			ResetMoving();
			isCrossfading = true;
			SetGameCamera(_linkedCamera);
			FadeOut(0f);
			FadeIn(_transitionDuration);
		}

		protected IEnumerator ExitSceneWithOverlay()
		{
			yield return new WaitForEndOfFrame();
			Texture2D screenTex = new Texture2D(ACScreen.width, ACScreen.height);
			screenTex.ReadPixels(new Rect(0f, 0f, ACScreen.width, ACScreen.height), 0, 0, false);
			screenTex.Apply();
			screenTex.Apply();
			SetFadeTexture(screenTex);
			KickStarter.sceneChanger.SetTransitionTexture(screenTex);
			FadeOut(0f);
		}

		protected void SmoothChange(float _transitionDuration, MoveMethod method, AnimationCurve _timeCurve = null)
		{
			moveMethod = method;
			mainCameraMode = MainCameraMode.NormalTransition;
			StopCrossfade();
			transitionTimer = (transitionDuration = _transitionDuration);
			if (method == MoveMethod.CustomCurve)
			{
				timeCurve = _timeCurve;
			}
			else
			{
				timeCurve = null;
			}
		}

		protected void ReleaseFadeTexture()
		{
			tempFadeTexture = null;
			AssignFadeTexture();
		}

		protected void AssignFadeTexture()
		{
			if (tempFadeTexture != null)
			{
				actualFadeTexture = tempFadeTexture;
			}
			else
			{
				actualFadeTexture = fadeTexture;
			}
		}

		protected void CalculatePlayableScreenArea()
		{
			Rect source = new Rect(ACScreen.safeArea);
			safeScreenRectInverted = new Rect(new Vector2(ACScreen.safeArea.x, (float)ACScreen.height - ACScreen.safeArea.y - ACScreen.safeArea.height), ACScreen.safeArea.size);
			if (borderWidth > 0f)
			{
				if (borderOrientation == MenuOrientation.Horizontal)
				{
					float num = borderWidth * ACScreen.safeArea.height;
					source.y += num;
					source.height -= 2f * num;
				}
				else if (borderOrientation == MenuOrientation.Vertical)
				{
					float num2 = borderWidth * ACScreen.safeArea.width;
					source.x += num2;
					source.width -= 2f * num2;
				}
			}
			playableScreenRect = new Rect(source);
			playableScreenRectInverted = new Rect(new Vector2(source.x, (float)ACScreen.height - source.y - source.height), source.size);
			playableScreenRectRelative = new Rect(playableScreenRect.x / (float)ACScreen.width, playableScreenRect.y / (float)ACScreen.height, playableScreenRect.width / (float)ACScreen.width, playableScreenRect.height / (float)ACScreen.height);
			playableScreenRectRelativeInverted = new Rect(playableScreenRectInverted.x / (float)ACScreen.width, playableScreenRectInverted.y / (float)ACScreen.height, playableScreenRectInverted.width / (float)ACScreen.width, playableScreenRectInverted.height / (float)ACScreen.height);
		}

		protected bool SetAspectRatio()
		{
			float num = 0f;
			Vector2 vector = new Vector2(ACScreen.width, ACScreen.height);
			Vector2 size = ACScreen.safeArea.size;
			Vector2 vector2 = new Vector2(ACScreen.safeArea.x, vector.y - ACScreen.safeArea.height - ACScreen.safeArea.y);
			num = ((Screen.orientation == ScreenOrientation.LandscapeRight || Screen.orientation == ScreenOrientation.LandscapeLeft) ? (size.x / size.y) : ((!(size.y > size.x) || !KickStarter.settingsManager.landscapeModeOnly) ? (size.x / size.y) : (size.y / size.x)));
			if (!KickStarter.settingsManager.forceAspectRatio || Mathf.Approximately(num, KickStarter.settingsManager.wantedAspectRatio))
			{
				borderWidth = 0f;
				borderOrientation = MenuOrientation.Horizontal;
				if ((bool)borderCam)
				{
					UnityEngine.Object.Destroy(borderCam.gameObject);
				}
				return false;
			}
			if (num > KickStarter.settingsManager.wantedAspectRatio)
			{
				borderWidth = 1f - KickStarter.settingsManager.wantedAspectRatio / num;
				borderWidth /= 2f;
				borderOrientation = MenuOrientation.Vertical;
				borderRect1 = new Rect(0f, 0f, borderWidth * ACScreen.safeArea.width, ACScreen.safeArea.height);
				borderRect2 = new Rect(ACScreen.safeArea.width * (1f - borderWidth), 0f, borderWidth * ACScreen.safeArea.width, ACScreen.safeArea.height);
			}
			else
			{
				borderWidth = 1f - num / KickStarter.settingsManager.wantedAspectRatio;
				borderWidth /= 2f;
				borderOrientation = MenuOrientation.Horizontal;
				borderRect1 = new Rect(0f, 0f, ACScreen.safeArea.width, borderWidth * ACScreen.safeArea.height);
				borderRect2 = new Rect(0f, ACScreen.safeArea.height * (1f - borderWidth), ACScreen.safeArea.width, borderWidth * ACScreen.safeArea.height);
			}
			borderRect1.position += vector2;
			borderRect2.position += vector2;
			return true;
		}

		protected void CalculateUnityUIAspectRatioCorrection()
		{
			if (Application.isPlaying)
			{
				Vector2 size = GetPlayableScreenArea(false).size;
				Vector2 size2 = ACScreen.safeArea.size;
				aspectRatioScaleCorrection = new Vector2(size.x / size2.x, size.y / size2.y);
				aspectRatioOffsetCorrection = new Vector2((size2.x - size.x) / 2f, (size2.y - size.y) / 2f);
			}
		}

		protected void CreateBorderCamera()
		{
			if (!borderCam && Application.isPlaying)
			{
				borderCam = new GameObject("BorderCamera", typeof(Camera)).GetComponent<Camera>();
				borderCam.transform.parent = base.transform;
				borderCam.depth = -2.1474836E+09f;
				borderCam.clearFlags = CameraClearFlags.Color;
				borderCam.backgroundColor = Color.black;
				borderCam.cullingMask = 0;
			}
		}

		protected Vector2 LimitVector(Vector2 point, float xBorder, float yBorder)
		{
			if (KickStarter.playerCursor.LimitCursorToMenu != null)
			{
				if (KickStarter.playerCursor.LimitCursorToMenu.IsUnityUI())
				{
					if (KickStarter.playerCursor.LimitCursorToMenu.RuntimeCanvas != null)
					{
						if (KickStarter.playerCursor.LimitCursorToMenu.RuntimeCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
						{
							RectTransform rectTransform = KickStarter.playerCursor.LimitCursorToMenu.rectTransform;
							Vector2 vector = Vector2.Scale(rectTransform.rect.size, rectTransform.lossyScale);
							Rect rect = new Rect(rectTransform.position.x, (float)ACScreen.height - rectTransform.position.y, vector.x, vector.y);
							rect.x -= rectTransform.pivot.x * vector.x;
							rect.y -= (1f - rectTransform.pivot.y) * vector.y;
							point.x = Mathf.Clamp(point.x, rect.x, rect.x + rect.width);
							point.y = Mathf.Clamp(point.y, (float)ACScreen.height - rect.y - rect.height, (float)ACScreen.height - rect.y);
						}
						else
						{
							ACDebug.LogWarning(string.Concat("Cannot limit the cursor's position within the boundary of Menu ", KickStarter.playerCursor.LimitCursorToMenu.RuntimeCanvas, " as it is not set to the 'Screen Space Overlay' render mode!"), KickStarter.playerCursor.LimitCursorToMenu.RuntimeCanvas);
						}
					}
				}
				else
				{
					Rect rect2 = KickStarter.playerCursor.LimitCursorToMenu.GetRect();
					point.x = Mathf.Clamp(point.x, rect2.x, rect2.x + rect2.width);
					point.y = Mathf.Clamp(point.y, (float)ACScreen.height - rect2.y - rect2.height, (float)ACScreen.height - rect2.y);
				}
			}
			int num = (int)(ACScreen.safeArea.width * xBorder);
			if (point.x <= (float)num + ACScreen.safeArea.x)
			{
				point.x = (float)(num + 1) + ACScreen.safeArea.x;
			}
			else if (point.x >= ACScreen.safeArea.width - (float)num + ACScreen.safeArea.x)
			{
				point.x = ACScreen.safeArea.width - (float)num - 1f + ACScreen.safeArea.x;
			}
			int num2 = (int)(ACScreen.safeArea.height * yBorder);
			if (point.y <= (float)num2 + ACScreen.safeArea.y)
			{
				point.y = (float)(num2 + 1) + ACScreen.safeArea.y;
			}
			else if (point.y >= ACScreen.safeArea.height - (float)num2 + ACScreen.safeArea.y)
			{
				point.y = ACScreen.safeArea.height - (float)num2 - 1f + ACScreen.safeArea.y;
			}
			return point;
		}

		protected void SetMidBorder()
		{
			if (borderWidth <= 0f && splitAmountMain + splitAmountOther < 1f)
			{
				Vector2 size = ACScreen.safeArea.size;
				if (splitOrientation == CameraSplitOrientation.Horizontal)
				{
					if (isTopLeftSplit)
					{
						midBorderRect = new Rect(0f, size.y * splitAmountMain, size.x, size.y * (1f - splitAmountOther - splitAmountMain));
					}
					else
					{
						midBorderRect = new Rect(0f, size.y * splitAmountOther, size.x, size.y * (1f - splitAmountOther - splitAmountMain));
					}
				}
				else if (isTopLeftSplit)
				{
					midBorderRect = new Rect(size.x * splitAmountMain, 0f, size.x * (1f - splitAmountOther - splitAmountMain), size.y);
				}
				else
				{
					midBorderRect = new Rect(size.x * splitAmountOther, 0f, size.x * (1f - splitAmountOther - splitAmountMain), size.y);
				}
			}
			else
			{
				midBorderRect = new Rect(0f, 0f, 0f, 0f);
			}
		}

		protected void ApplyCameraData(_Camera _camera1, _Camera _camera2, float camera2Weight, MoveMethod _moveMethod = MoveMethod.Linear, AnimationCurve _timeCurve = null)
		{
			if (_camera1 == null)
			{
				ApplyCameraData(new GameCameraData(_camera2));
				return;
			}
			if (_camera2 == null)
			{
				ApplyCameraData(new GameCameraData(_camera1));
				return;
			}
			GameCameraData cameraData = new GameCameraData(_camera1);
			GameCameraData cameraData2 = new GameCameraData(_camera2);
			ApplyCameraData(cameraData, cameraData2, camera2Weight, _moveMethod, _timeCurve);
		}

		protected void ApplyCameraData(GameCameraData cameraData1, GameCameraData cameraData2, float camera2Weight, MoveMethod _moveMethod = MoveMethod.Linear, AnimationCurve _timeCurve = null)
		{
			float otherDataWeight = AdvGame.Interpolate(camera2Weight, _moveMethod, _timeCurve);
			GameCameraData cameraData3 = cameraData1.CreateMix(cameraData2, otherDataWeight, _moveMethod == MoveMethod.Curved);
			ApplyCameraData(cameraData3);
		}

		protected void ApplyCameraData(GameCameraData cameraData)
		{
			if (cameraData.is2D)
			{
				perspectiveOffset = cameraData.perspectiveOffset;
				Camera.ResetProjectionMatrix();
			}
			base.transform.position = cameraData.position;
			base.transform.rotation = cameraData.rotation;
			Camera.orthographic = cameraData.isOrthographic;
			Camera.fieldOfView = cameraData.fieldOfView;
			Camera.orthographicSize = cameraData.orthographicSize;
			focalDistance = cameraData.focalDistance;
			if (cameraData.is2D && !cameraData.isOrthographic)
			{
				Camera.projectionMatrix = AdvGame.SetVanishingPoint(Camera, perspectiveOffset);
			}
		}

		public static bool AllowProjectionShifting(Camera _camera)
		{
			return !_camera.orthographic;
		}
	}
}
