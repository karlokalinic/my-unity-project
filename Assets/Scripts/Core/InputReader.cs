using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public static class InputReader
{
    public enum InputContext
    {
        Gameplay,
        UI,
        Dialogue
    }

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
    private static InputAction uiNavigateAction;
    private static InputAction uiSubmitAction;
    private static InputAction uiCancelAction;
    private static InputAction dialogueNavigateAction;
    private static InputAction dialogueSubmitAction;
    private static InputAction dialogueCancelAction;
    private static InputActionAsset boundAsset;
    private static InputActionMap playerActionMap;
    private static InputActionMap uiActionMap;
    private static InputActionMap dialogueActionMap;
    private static InputContext currentContext = InputContext.Gameplay;
    private static readonly Stack<InputContext> contextStack = new Stack<InputContext>();

    private static bool dialogueStickUpLatch;
    private static bool dialogueStickDownLatch;
    private static bool dialogueStickLeftLatch;
    private static bool dialogueStickRightLatch;
    public static InputContext CurrentContext => currentContext;

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

        boundAsset = asset;
        playerActionMap = asset.FindActionMap("Player", false);
        uiActionMap = asset.FindActionMap("UI", false);
        dialogueActionMap = asset.FindActionMap("Dialogue", false) ?? uiActionMap;

        moveAction = playerActionMap?.FindAction("Move");
        lookAction = playerActionMap?.FindAction("Look");
        attackAction = playerActionMap?.FindAction("Attack");
        interactAction = playerActionMap?.FindAction("Interact");
        sprintAction = playerActionMap?.FindAction("Sprint");
        previousAction = playerActionMap?.FindAction("Previous");
        nextAction = playerActionMap?.FindAction("Next");

        uiNavigateAction = uiActionMap?.FindAction("Navigate");
        uiSubmitAction = uiActionMap?.FindAction("Submit");
        uiCancelAction = uiActionMap?.FindAction("Cancel");
        dialogueNavigateAction = dialogueActionMap?.FindAction("Navigate");
        dialogueSubmitAction = dialogueActionMap?.FindAction("Submit");
        dialogueCancelAction = dialogueActionMap?.FindAction("Cancel");

        EnableIfPresent(moveAction);
        EnableIfPresent(lookAction);
        EnableIfPresent(attackAction);
        EnableIfPresent(interactAction);
        EnableIfPresent(sprintAction);
        EnableIfPresent(previousAction);
        EnableIfPresent(nextAction);
        EnableIfPresent(uiNavigateAction);
        EnableIfPresent(uiSubmitAction);
        EnableIfPresent(uiCancelAction);
        EnableIfPresent(dialogueNavigateAction);
        EnableIfPresent(dialogueSubmitAction);
        EnableIfPresent(dialogueCancelAction);
        contextStack.Clear();
        SetContext(InputContext.Gameplay);
    }

    public static void PushContext(InputContext context)
    {
        contextStack.Push(currentContext);
        SetContext(context);
    }

    public static void PopContext(InputContext fallback = InputContext.Gameplay)
    {
        SetContext(contextStack.Count > 0 ? contextStack.Pop() : fallback);
    }

    public static void ResetContextStack(InputContext context = InputContext.Gameplay)
    {
        contextStack.Clear();
        SetContext(context);
    }

    public static void SetContext(InputContext context)
    {
        if (currentContext == context && boundAsset != null)
        {
            return;
        }

        if (currentContext == InputContext.Dialogue && context != InputContext.Dialogue)
        {
            ResetDialogueInputLatches();
        }

        currentContext = context;

        if (boundAsset == null)
        {
            return;
        }

        if (context == InputContext.Gameplay)
        {
            playerActionMap?.Enable();
            uiActionMap?.Disable();
            if (dialogueActionMap != uiActionMap)
            {
                dialogueActionMap?.Disable();
            }
            return;
        }

        playerActionMap?.Disable();
        if (context == InputContext.UI)
        {
            uiActionMap?.Enable();
            if (dialogueActionMap != uiActionMap)
            {
                dialogueActionMap?.Disable();
            }
            return;
        }

        dialogueActionMap?.Enable();
        if (uiActionMap != dialogueActionMap)
        {
            uiActionMap?.Disable();
        }
    }

    public static Vector2 GetMoveVector()
    {
        if (boundAsset != null)
        {
            return moveAction != null ? Vector2.ClampMagnitude(moveAction.ReadValue<Vector2>(), 1f) : Vector2.zero;
        }

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
        if (boundAsset != null)
        {
            return lookAction != null ? lookAction.ReadValue<Vector2>() : Vector2.zero;
        }

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
        if (boundAsset != null)
        {
            return sprintAction != null && sprintAction.IsPressed();
        }

        if (sprintAction != null)
        {
            return sprintAction.IsPressed();
        }

        return (Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed) ||
               (Gamepad.current != null && Gamepad.current.leftStickButton.isPressed);
    }

    public static bool RotateLeftPressed()
    {
        if (boundAsset != null)
        {
            return previousAction != null && previousAction.WasPressedThisFrame();
        }

        if (previousAction != null && previousAction.WasPressedThisFrame())
        {
            return true;
        }

        return (Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame) ||
               (Gamepad.current != null && Gamepad.current.leftShoulder.wasPressedThisFrame);
    }

    public static bool RotateRightPressed()
    {
        if (boundAsset != null)
        {
            return nextAction != null && nextAction.WasPressedThisFrame();
        }

        if (nextAction != null && nextAction.WasPressedThisFrame())
        {
            return true;
        }

        return (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame) ||
               (Gamepad.current != null && Gamepad.current.rightShoulder.wasPressedThisFrame);
    }

    public static bool AimHeld()
    {
        if (boundAsset != null && currentContext != InputContext.Gameplay)
        {
            return false;
        }

        return (Mouse.current != null && Mouse.current.rightButton.isPressed) ||
               (Gamepad.current != null && Gamepad.current.leftTrigger.ReadValue() > 0.35f);
    }

    public static bool AimTogglePressed()
    {
        if (boundAsset != null && currentContext != InputContext.Gameplay)
        {
            return false;
        }

        return Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame;
    }

    public static bool FireHeld()
    {
        if (boundAsset != null)
        {
            return attackAction != null && attackAction.IsPressed();
        }

        if (attackAction != null)
        {
            return attackAction.IsPressed();
        }

        return (Mouse.current != null && Mouse.current.leftButton.isPressed) ||
               (Gamepad.current != null && Gamepad.current.rightTrigger.ReadValue() > 0.35f);
    }

    public static bool FirePressed()
    {
        if (boundAsset != null)
        {
            return attackAction != null && attackAction.WasPressedThisFrame();
        }

        if (attackAction != null && attackAction.WasPressedThisFrame())
        {
            return true;
        }

        return (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) ||
               (Gamepad.current != null && Gamepad.current.rightTrigger.wasPressedThisFrame);
    }

    public static bool InspectRotateHeld()
    {
        if (boundAsset != null && currentContext != InputContext.Gameplay)
        {
            return false;
        }

        return Mouse.current != null && Mouse.current.leftButton.isPressed;
    }

    public static bool ReloadPressed()
    {
        if (boundAsset != null && currentContext != InputContext.Gameplay) return false;

        return (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame) ||
               (Gamepad.current != null && Gamepad.current.buttonWest.wasPressedThisFrame);
    }

    public static bool InteractPressed()
    {
        if (boundAsset != null)
        {
            return interactAction != null && interactAction.WasPressedThisFrame();
        }

        if (interactAction != null && interactAction.WasPressedThisFrame())
        {
            return true;
        }

        return (Keyboard.current != null && (Keyboard.current.eKey.wasPressedThisFrame || Keyboard.current.fKey.wasPressedThisFrame)) ||
               (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame);
    }

    public static bool CancelPressed()
    {
        if (boundAsset != null)
        {
            InputAction action = ResolveCancelAction();
            return action != null && action.WasPressedThisFrame();
        }

        InputAction resolved = ResolveCancelAction();
        if (resolved != null && resolved.WasPressedThisFrame())
        {
            return true;
        }

        return (Keyboard.current != null && (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.tabKey.wasPressedThisFrame)) ||
               (Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame);
    }

    public static bool DialogueSubmitPressed()
    {
        if (boundAsset != null)
        {
            InputAction action = ResolveSubmitAction();
            return action != null && action.WasPressedThisFrame();
        }

        InputAction resolved = ResolveSubmitAction();
        if (resolved != null && resolved.WasPressedThisFrame())
        {
            return true;
        }

        return (Keyboard.current != null && (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame || Keyboard.current.eKey.wasPressedThisFrame)) ||
               (Gamepad.current != null && (Gamepad.current.buttonSouth.wasPressedThisFrame || Gamepad.current.buttonNorth.wasPressedThisFrame));
    }

    public static Vector2 GetDialogueNavigateVector()
    {
        if (boundAsset != null)
        {
            InputAction action = ResolveNavigateAction();
            return action != null ? action.ReadValue<Vector2>() : Vector2.zero;
        }

        InputAction resolved = ResolveNavigateAction();
        if (resolved != null)
        {
            Vector2 nav = resolved.ReadValue<Vector2>();
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
    public static string GetDialogueSubmitLabel() => GetBindingLabel(ResolveSubmitAction() ?? interactAction, DialogueSubmitKeyLabel);
    public static string GetDialogueCancelLabel() => GetBindingLabel(ResolveCancelAction(), DialogueLeaveKeyLabel);

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

    private static InputAction ResolveNavigateAction()
    {
        return currentContext == InputContext.Dialogue
            ? dialogueNavigateAction ?? uiNavigateAction
            : uiNavigateAction;
    }

    private static InputAction ResolveSubmitAction()
    {
        return currentContext == InputContext.Dialogue
            ? dialogueSubmitAction ?? uiSubmitAction
            : uiSubmitAction;
    }

    private static InputAction ResolveCancelAction()
    {
        return currentContext == InputContext.Dialogue
            ? dialogueCancelAction ?? uiCancelAction
            : uiCancelAction;
    }

    private static void ResetDialogueInputLatches()
    {
        dialogueStickUpLatch = false;
        dialogueStickDownLatch = false;
        dialogueStickLeftLatch = false;
        dialogueStickRightLatch = false;
    }
}
