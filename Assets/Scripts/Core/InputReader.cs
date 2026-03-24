using UnityEngine;

#if ENABLE_INPUT_SYSTEM && !UNITY_DISABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public static class InputReader
{
    public const string InteractKeyLabel = "E";
    public const string AlternateInteractKeyLabel = "F";
    public const string RotateLeftKeyLabel = "Q";
    public const string RotateRightKeyLabel = "R";
    public const string DialogueSubmitKeyLabel = "Enter";
    public const string DialogueLeaveKeyLabel = "Esc";

    private static bool dialogueStickUpLatch;
    private static bool dialogueStickDownLatch;
    private static bool dialogueStickLeftLatch;
    private static bool dialogueStickRightLatch;

    public static Vector2 GetMoveVector()
    {
#if ENABLE_INPUT_SYSTEM && !UNITY_DISABLE_INPUT_SYSTEM
        Vector2 move = Vector2.zero;
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.aKey.isPressed) move.x -= 1f;
            if (keyboard.dKey.isPressed) move.x += 1f;
            if (keyboard.sKey.isPressed) move.y -= 1f;
            if (keyboard.wKey.isPressed) move.y += 1f;
        }

        Gamepad gamepad = Gamepad.current;
        if (gamepad != null)
        {
            Vector2 gamepadMove = gamepad.leftStick.ReadValue();
            if (gamepadMove.sqrMagnitude > move.sqrMagnitude)
            {
                move = gamepadMove;
            }
        }

        return Vector2.ClampMagnitude(move, 1f);
#else
        return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
#endif
    }

    public static Vector2 GetLookDelta()
    {
#if ENABLE_INPUT_SYSTEM && !UNITY_DISABLE_INPUT_SYSTEM
        Vector2 delta = Vector2.zero;
        if (Mouse.current != null)
        {
            delta = Mouse.current.delta.ReadValue();
        }
        else if (Gamepad.current != null)
        {
            delta = Gamepad.current.rightStick.ReadValue() * 12f;
        }

        return delta;
#else
        return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
#endif
    }

    public static bool SprintHeld()
    {
#if ENABLE_INPUT_SYSTEM && !UNITY_DISABLE_INPUT_SYSTEM
        return (Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed) ||
               (Gamepad.current != null && Gamepad.current.leftStickButton.isPressed);
#else
        return Input.GetKey(KeyCode.LeftShift);
#endif
    }

    public static bool RotateLeftPressed()
    {
#if ENABLE_INPUT_SYSTEM && !UNITY_DISABLE_INPUT_SYSTEM
        return (Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame) ||
               (Gamepad.current != null && Gamepad.current.leftShoulder.wasPressedThisFrame);
#else
        return Input.GetKeyDown(KeyCode.Q);
#endif
    }

    public static bool RotateRightPressed()
    {
#if ENABLE_INPUT_SYSTEM && !UNITY_DISABLE_INPUT_SYSTEM
        return (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame) ||
               (Gamepad.current != null && Gamepad.current.rightShoulder.wasPressedThisFrame);
#else
        return Input.GetKeyDown(KeyCode.R);
#endif
    }

    public static bool AimHeld()
    {
#if ENABLE_INPUT_SYSTEM && !UNITY_DISABLE_INPUT_SYSTEM
        return (Mouse.current != null && Mouse.current.rightButton.isPressed) ||
               (Gamepad.current != null && Gamepad.current.leftTrigger.ReadValue() > 0.35f);
#else
        return Input.GetMouseButton(1);
#endif
    }

    public static bool AimTogglePressed()
    {
#if ENABLE_INPUT_SYSTEM && !UNITY_DISABLE_INPUT_SYSTEM
        return Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame;
#else
        return Input.GetMouseButtonDown(1);
#endif
    }

    public static bool FireHeld()
    {
#if ENABLE_INPUT_SYSTEM && !UNITY_DISABLE_INPUT_SYSTEM
        return (Mouse.current != null && Mouse.current.leftButton.isPressed) ||
               (Gamepad.current != null && Gamepad.current.rightTrigger.ReadValue() > 0.35f);
#else
        return Input.GetMouseButton(0);
#endif
    }

    public static bool InspectRotateHeld()
    {
#if ENABLE_INPUT_SYSTEM && !UNITY_DISABLE_INPUT_SYSTEM
        return Mouse.current != null && Mouse.current.leftButton.isPressed;
#else
        return Input.GetMouseButton(0);
#endif
    }

    public static bool InteractPressed()
    {
#if ENABLE_INPUT_SYSTEM && !UNITY_DISABLE_INPUT_SYSTEM
        return (Keyboard.current != null && (Keyboard.current.eKey.wasPressedThisFrame || Keyboard.current.fKey.wasPressedThisFrame)) ||
               (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame);
#else
        return Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.F);
#endif
    }

    public static bool CancelPressed()
    {
#if ENABLE_INPUT_SYSTEM && !UNITY_DISABLE_INPUT_SYSTEM
        return (Keyboard.current != null && (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.tabKey.wasPressedThisFrame)) ||
               (Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame);
#else
        return Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Tab);
#endif
    }

    public static bool DialogueSubmitPressed()
    {
#if ENABLE_INPUT_SYSTEM && !UNITY_DISABLE_INPUT_SYSTEM
        return (Keyboard.current != null && (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame || Keyboard.current.eKey.wasPressedThisFrame)) ||
               (Gamepad.current != null && (Gamepad.current.buttonSouth.wasPressedThisFrame || Gamepad.current.buttonNorth.wasPressedThisFrame));
#else
        return Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.E);
#endif
    }

    public static Vector2 GetDialogueNavigateVector()
    {
#if ENABLE_INPUT_SYSTEM && !UNITY_DISABLE_INPUT_SYSTEM
        Vector2 navigate = Vector2.zero;

        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.wKey.wasPressedThisFrame || keyboard.upArrowKey.wasPressedThisFrame)
            {
                navigate.y += 1f;
            }

            if (keyboard.sKey.wasPressedThisFrame || keyboard.downArrowKey.wasPressedThisFrame)
            {
                navigate.y -= 1f;
            }

            if (keyboard.aKey.wasPressedThisFrame || keyboard.leftArrowKey.wasPressedThisFrame)
            {
                navigate.x -= 1f;
            }

            if (keyboard.dKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame)
            {
                navigate.x += 1f;
            }
        }

        Gamepad gamepad = Gamepad.current;
        if (gamepad != null)
        {
            if (gamepad.dpad.up.wasPressedThisFrame || ReadStickEdge(ref dialogueStickUpLatch, gamepad.leftStick.y.ReadValue(), 0.58f))
            {
                navigate.y += 1f;
            }

            if (gamepad.dpad.down.wasPressedThisFrame || ReadStickEdge(ref dialogueStickDownLatch, -gamepad.leftStick.y.ReadValue(), 0.58f))
            {
                navigate.y -= 1f;
            }

            if (gamepad.dpad.left.wasPressedThisFrame || ReadStickEdge(ref dialogueStickLeftLatch, -gamepad.leftStick.x.ReadValue(), 0.62f))
            {
                navigate.x -= 1f;
            }

            if (gamepad.dpad.right.wasPressedThisFrame || ReadStickEdge(ref dialogueStickRightLatch, gamepad.leftStick.x.ReadValue(), 0.62f))
            {
                navigate.x += 1f;
            }
        }

        return navigate;
#else
        Vector2 navigate = Vector2.zero;
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) navigate.y += 1f;
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) navigate.y -= 1f;
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) navigate.x -= 1f;
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) navigate.x += 1f;
        return navigate;
#endif
    }

#if ENABLE_INPUT_SYSTEM && !UNITY_DISABLE_INPUT_SYSTEM
    private static bool ReadStickEdge(ref bool latch, float value, float threshold)
    {
        bool nowActive = value > threshold;
        bool triggered = nowActive && !latch;
        latch = nowActive;
        return triggered;
    }
#endif
}
