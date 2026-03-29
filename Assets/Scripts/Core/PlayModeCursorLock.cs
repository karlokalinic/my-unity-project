using UnityEngine;

[DisallowMultipleComponent]
public class PlayModeCursorLock : MonoBehaviour
{
    [SerializeField] private bool lockCursorInPlayMode = true;
    [SerializeField] private bool hideCursorWhenLocked = true;
    [SerializeField] private bool allowTemporaryUnlockOnEscape;

    private bool manuallyUnlocked;

    private void OnEnable()
    {
        if (Application.isPlaying && lockCursorInPlayMode)
        {
            ApplyCursorState(true);
        }
    }

    private void Update()
    {
        if (!Application.isPlaying || !lockCursorInPlayMode)
        {
            return;
        }

        if (allowTemporaryUnlockOnEscape && InputReader.CancelPressed())
        {
            manuallyUnlocked = !manuallyUnlocked;
        }

        bool shouldLock = Application.isFocused && !manuallyUnlocked && !GameplayPauseFacade.IsPaused;
        ApplyCursorState(shouldLock);
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!Application.isPlaying || !lockCursorInPlayMode)
        {
            return;
        }

        bool shouldLock = hasFocus && !manuallyUnlocked && !GameplayPauseFacade.IsPaused;
        ApplyCursorState(shouldLock);
    }

    private void OnDisable()
    {
        if (Application.isPlaying)
        {
            ReleaseCursor();
        }
    }

    private void ApplyCursorState(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = hideCursorWhenLocked ? !locked : true;
    }

    private static void ReleaseCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
