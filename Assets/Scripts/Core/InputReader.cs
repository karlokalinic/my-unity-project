using UnityEngine;
using UnityEngine.InputSystem;

public static class InputReader
{
    public const string InteractKeyLabel = "E";
    public const string AlternateInteractKeyLabel = "F";
    public const string RotateLeftKeyLabel = "Q";
    public const string RotateRightKeyLabel = "R";
    public const string DialogueSubmitKeyLabel = "Enter";
    public const string DialogueLeaveKeyLabel = "Esc";

    private static InputAction moveAction;
    private static InputAction lookAction;
    private static InputAction attackAction;
    private static InputAction interactAction;
    private static InputAction sprintAction;
    private static InputAction previousAction;
    private static InputAction nextAction;
    private static InputAction navigateAction;
    private static InputAction submitAction;
    private static InputAction cancelAction;

    private static bool dialogueStickUpLatch;
    private static bool dialogueStickDownLatch;
    private static bool dialogueStickLeftLatch;
    private static bool dialogueStickRightLatch;

    /// <summary>
    /// Bind Input Action references from a provided asset (Gameplay/UI/Dialogue maps).
    /// Safe to call multiple times; enables the actions lazily.
    /// </summary>
    public static void BindActions(InputActionAsset asset)
    {
        if (asset == null)
        {
            return;
        }

        InputActionMap playerMap = asset.FindActionMap("Player", false);
        InputActionMap uiMap = asset.FindActionMap("UI", false);

        moveAction = playerMap?.FindAction("Move");
        lookAction = playerMap?.FindAction("Look");
        attackAction = playerMap?.FindAction("Attack");
        interactAction = playerMap?.FindAction("Interact");
        sprintAction = playerMap?.FindAction("Sprint");
        previousAction = playerMap?.FindAction("Previous");
        nextAction = playerMap?.FindAction("Next");

        navigateAction = uiMap?.FindAction("Navigate");
        submitAction = uiMap?.FindAction("Submit");
        cancelAction = uiMap?.FindAction("Cancel");

        EnableIfPresent(moveAction);
        EnableIfPresent(lookAction);
        EnableIfPresent(attackAction);
        EnableIfPresent(interactAction);
        EnableIfPresent(sprintAction);
        EnableIfPresent(previousAction);
        EnableIfPresent(nextAction);
        EnableIfPresent(navigateAction);
        EnableIfPresent(submitAction);
        EnableIfPresent(cancelAction);
    }

    public static Vector2 GetMoveVector()
    {
        if (moveAction != null)
        {
            Vector2 actionMove = moveAction.ReadValue<Vector2>();
            if (actionMove.sqrMagnitude > 0.0001f)
            {
                return Vector2.ClampMagnitude(actionMove, 1f);
            }
        }

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
    }

    public static Vector2 GetLookDelta()
    {
        if (lookAction != null)
        {
            Vector2 look = lookAction.ReadValue<Vector2>();
            if (look.sqrMagnitude > 0.0001f)
            {
                return look;
            }
        }

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
    }

    public static bool SprintHeld()
    {
        if (sprintAction != null)
        {
            return sprintAction.IsPressed();
        }

        return (Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed) ||
               (Gamepad.current != null && Gamepad.current.leftStickButton.isPressed);
    }

    public static bool RotateLeftPressed()
    {
        if (previousAction != null && previousAction.WasPressedThisFrame())
        {
            return true;
        }

        return (Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame) ||
               (Gamepad.current != null && Gamepad.current.leftShoulder.wasPressedThisFrame);
    }

    public static bool RotateRightPressed()
    {
        if (nextAction != null && nextAction.WasPressedThisFrame())
        {
            return true;
        }

        return (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame) ||
               (Gamepad.current != null && Gamepad.current.rightShoulder.wasPressedThisFrame);
    }

    public static bool AimHeld()
    {
        return (Mouse.current != null && Mouse.current.rightButton.isPressed) ||
               (Gamepad.current != null && Gamepad.current.leftTrigger.ReadValue() > 0.35f);
    }

    public static bool AimTogglePressed()
    {
        return Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame;
    }

    public static bool FireHeld()
    {
        if (attackAction != null)
        {
            return attackAction.IsPressed();
        }

        return (Mouse.current != null && Mouse.current.leftButton.isPressed) ||
               (Gamepad.current != null && Gamepad.current.rightTrigger.ReadValue() > 0.35f);
    }

    public static bool FirePressed()
    {
        if (attackAction != null && attackAction.WasPressedThisFrame())
        {
            return true;
        }

        return (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) ||
               (Gamepad.current != null && Gamepad.current.rightTrigger.wasPressedThisFrame);
    }

    public static bool InspectRotateHeld()
    {
        return Mouse.current != null && Mouse.current.leftButton.isPressed;
    }

    public static bool ReloadPressed()
    {
        return (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame) ||
               (Gamepad.current != null && Gamepad.current.buttonWest.wasPressedThisFrame);
    }

    public static bool InteractPressed()
    {
        if (interactAction != null && interactAction.WasPressedThisFrame())
        {
            return true;
        }

        return (Keyboard.current != null && (Keyboard.current.eKey.wasPressedThisFrame || Keyboard.current.fKey.wasPressedThisFrame)) ||
               (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame);
    }

    public static bool CancelPressed()
    {
        if (cancelAction != null && cancelAction.WasPressedThisFrame())
        {
            return true;
        }

        return (Keyboard.current != null && (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.tabKey.wasPressedThisFrame)) ||
               (Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame);
    }

    public static bool DialogueSubmitPressed()
    {
        if (submitAction != null && submitAction.WasPressedThisFrame())
        {
            return true;
        }

        return (Keyboard.current != null && (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame || Keyboard.current.eKey.wasPressedThisFrame)) ||
               (Gamepad.current != null && (Gamepad.current.buttonSouth.wasPressedThisFrame || Gamepad.current.buttonNorth.wasPressedThisFrame));
    }

    public static Vector2 GetDialogueNavigateVector()
    {
        if (navigateAction != null)
        {
            Vector2 nav = navigateAction.ReadValue<Vector2>();
            if (nav.sqrMagnitude > 0.0001f)
            {
                return nav;
            }
        }

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
    }

    private static bool ReadStickEdge(ref bool latch, float value, float threshold)
    {
        bool nowActive = value > threshold;
        bool triggered = nowActive && !latch;
        latch = nowActive;
        return triggered;
    }

    public static string GetInteractLabel() => GetBindingLabel(interactAction, InteractKeyLabel);
    public static string GetDialogueSubmitLabel() => GetBindingLabel(submitAction ?? interactAction, DialogueSubmitKeyLabel);
    public static string GetDialogueCancelLabel() => GetBindingLabel(cancelAction, DialogueLeaveKeyLabel);

    private static string GetBindingLabel(InputAction action, string fallback)
    {
        if (action == null)
        {
            return fallback;
        }

        try
        {
            return action.GetBindingDisplayString();
        }
        catch
        {
            return fallback;
        }
    }

    private static void EnableIfPresent(InputAction action)
    {
        if (action == null) return;
        if (!action.enabled)
        {
            action.Enable();
        }
    }
}
