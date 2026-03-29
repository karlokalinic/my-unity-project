using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public static class InputReader
{
    public enum KeyboardBindingAction
    {
        MoveForward,
        MoveBackward,
        MoveLeft,
        MoveRight,
        Sprint,
        RotateLeft,
        RotateRight,
        Interact,
        Reload,
        Jump,
        Flashlight,
        Inventory,
        Pause,
        DialogueSubmit
    }

    public enum InputContext
    {
        Gameplay,
        UI,
        Dialogue
    }

    public const string InteractKeyLabel = "F";
    public const string AlternateInteractKeyLabel = "E";
    public const string RotateLeftKeyLabel = "Q";
    public const string RotateRightKeyLabel = "E";
    public const string DialogueSubmitKeyLabel = "Enter";
    public const string DialogueLeaveKeyLabel = "Esc";
    private const string BindingPrefPrefix = "input.keyboard.";

    private static InputAction moveAction;
    private static InputAction lookAction;
    private static InputAction attackAction;
    private static InputAction interactAction;
    private static InputAction sprintAction;
    private static InputAction jumpAction;
    private static InputAction flashlightAction;
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
    private static readonly Dictionary<KeyboardBindingAction, Key> keyboardBindings = new Dictionary<KeyboardBindingAction, Key>();
    private static readonly KeyboardBindingAction[] keyboardBindingOrder =
    {
        KeyboardBindingAction.MoveForward,
        KeyboardBindingAction.MoveBackward,
        KeyboardBindingAction.MoveLeft,
        KeyboardBindingAction.MoveRight,
        KeyboardBindingAction.Sprint,
        KeyboardBindingAction.RotateLeft,
        KeyboardBindingAction.RotateRight,
        KeyboardBindingAction.Interact,
        KeyboardBindingAction.Reload,
        KeyboardBindingAction.Jump,
        KeyboardBindingAction.Flashlight,
        KeyboardBindingAction.Inventory,
        KeyboardBindingAction.Pause,
        KeyboardBindingAction.DialogueSubmit
    };
    private static bool keyboardBindingsLoaded;

    private static bool dialogueStickUpLatch;
    private static bool dialogueStickDownLatch;
    private static bool dialogueStickLeftLatch;
    private static bool dialogueStickRightLatch;
    private static bool movementStickLatch;
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
        jumpAction = playerActionMap?.FindAction("Jump");
        flashlightAction = playerActionMap?.FindAction("Flashlight") ?? playerActionMap?.FindAction("ToggleFlashlight");
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
        EnableIfPresent(jumpAction);
        EnableIfPresent(flashlightAction);
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

    public static IReadOnlyList<KeyboardBindingAction> GetKeyboardBindingOrder()
    {
        EnsureKeyboardBindingsLoaded();
        return keyboardBindingOrder;
    }

    public static string GetBindingActionDisplayName(KeyboardBindingAction action)
    {
        return action switch
        {
            KeyboardBindingAction.MoveForward => "Move Forward",
            KeyboardBindingAction.MoveBackward => "Move Backward",
            KeyboardBindingAction.MoveLeft => "Move Left",
            KeyboardBindingAction.MoveRight => "Move Right",
            KeyboardBindingAction.Sprint => "Sprint",
            KeyboardBindingAction.RotateLeft => "Rotate Camera Left",
            KeyboardBindingAction.RotateRight => "Rotate Camera Right",
            KeyboardBindingAction.Interact => "Interact",
            KeyboardBindingAction.Reload => "Reload",
            KeyboardBindingAction.Jump => "Jump",
            KeyboardBindingAction.Flashlight => "Flashlight",
            KeyboardBindingAction.Inventory => "Inventory",
            KeyboardBindingAction.Pause => "Pause / Menu",
            KeyboardBindingAction.DialogueSubmit => "Dialogue Confirm",
            _ => action.ToString()
        };
    }

    public static Key GetKeyboardBinding(KeyboardBindingAction action)
    {
        EnsureKeyboardBindingsLoaded();
        return keyboardBindings[action];
    }

    public static void SetKeyboardBinding(KeyboardBindingAction action, Key key)
    {
        EnsureKeyboardBindingsLoaded();
        keyboardBindings[action] = key;
        PlayerPrefs.SetString(BindingPrefPrefix + action, key.ToString());
        PlayerPrefs.Save();
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
        if (Keyboard.current != null)
        {
            if (IsKeyboardBindingHeld(KeyboardBindingAction.MoveLeft)) move.x -= 1f;
            if (IsKeyboardBindingHeld(KeyboardBindingAction.MoveRight)) move.x += 1f;
            if (IsKeyboardBindingHeld(KeyboardBindingAction.MoveBackward)) move.y -= 1f;
            if (IsKeyboardBindingHeld(KeyboardBindingAction.MoveForward)) move.y += 1f;
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
        if (sprintAction != null && sprintAction.IsPressed())
        {
            return true;
        }

        return (Keyboard.current != null && IsKeyboardBindingHeld(KeyboardBindingAction.Sprint)) ||
               (Gamepad.current != null && Gamepad.current.leftStickButton.isPressed);
    }

    public static bool RotateLeftPressed()
    {
        if (previousAction != null && previousAction.WasPressedThisFrame())
        {
            return true;
        }

        return (Keyboard.current != null && IsKeyboardBindingPressedThisFrame(KeyboardBindingAction.RotateLeft)) ||
               (Gamepad.current != null && Gamepad.current.leftShoulder.wasPressedThisFrame);
    }

    public static bool RotateRightPressed()
    {
        if (nextAction != null && nextAction.WasPressedThisFrame())
        {
            return true;
        }

        return (Keyboard.current != null && IsKeyboardBindingPressedThisFrame(KeyboardBindingAction.RotateRight)) ||
               (Gamepad.current != null && Gamepad.current.rightShoulder.wasPressedThisFrame);
    }

    public static bool AimHeld()
    {
        if (boundAsset != null && currentContext != InputContext.Gameplay)
        {
            return false;
        }

        return (Mouse.current != null && Mouse.current.rightButton.isPressed) ||
               (Gamepad.current != null && Gamepad.current.leftTrigger.ReadValue() > 0.82f);
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
        if (attackAction != null && attackAction.IsPressed())
        {
            return true;
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
        if (boundAsset != null && currentContext != InputContext.Gameplay)
        {
            return false;
        }

        return Mouse.current != null && Mouse.current.leftButton.isPressed;
    }

    public static bool ReloadPressed()
    {
        if (boundAsset != null && currentContext != InputContext.Gameplay) return false;

        return (Keyboard.current != null && IsKeyboardBindingPressedThisFrame(KeyboardBindingAction.Reload)) ||
               (Gamepad.current != null && Gamepad.current.buttonWest.wasPressedThisFrame);
    }

    public static bool JumpPressed()
    {
        if (boundAsset != null && currentContext != InputContext.Gameplay)
        {
            return false;
        }

        if (jumpAction != null && jumpAction.WasPressedThisFrame())
        {
            return true;
        }

        return (Keyboard.current != null && IsKeyboardBindingPressedThisFrame(KeyboardBindingAction.Jump)) ||
               (Gamepad.current != null && Gamepad.current.buttonNorth.wasPressedThisFrame);
    }

    public static bool FlashlightTogglePressed()
    {
        if (boundAsset != null && currentContext != InputContext.Gameplay)
        {
            return false;
        }

        if (flashlightAction != null && flashlightAction.WasPressedThisFrame())
        {
            return true;
        }

        return (Keyboard.current != null && IsKeyboardBindingPressedThisFrame(KeyboardBindingAction.Flashlight)) ||
               (Gamepad.current != null && GamepadFlashlightPressed(Gamepad.current));
    }

    private static bool GamepadFlashlightPressed(Gamepad gamepad)
    {
        return gamepad != null &&
               (gamepad.dpad.up.wasPressedThisFrame || gamepad.rightStickButton.wasPressedThisFrame);
    }

    public static bool InteractPressed()
    {
        if (interactAction != null && interactAction.WasPressedThisFrame())
        {
            return true;
        }

        return (Keyboard.current != null && IsKeyboardBindingPressedThisFrame(KeyboardBindingAction.Interact)) ||
               (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame);
    }

    public static bool MovementIntentPressed(float stickThreshold = 0.15f)
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null &&
            (IsKeyboardBindingHeld(KeyboardBindingAction.MoveForward) ||
             IsKeyboardBindingHeld(KeyboardBindingAction.MoveLeft) ||
             IsKeyboardBindingHeld(KeyboardBindingAction.MoveBackward) ||
             IsKeyboardBindingHeld(KeyboardBindingAction.MoveRight)))
        {
            return true;
        }

        Gamepad gamepad = Gamepad.current;
        if (gamepad != null && gamepad.leftStick.ReadValue().sqrMagnitude > stickThreshold * stickThreshold)
        {
            return true;
        }

        return false;
    }

    public static bool MovementIntentStarted(float stickThreshold = 0.15f)
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null &&
            (IsKeyboardBindingPressedThisFrame(KeyboardBindingAction.MoveForward) ||
             IsKeyboardBindingPressedThisFrame(KeyboardBindingAction.MoveLeft) ||
             IsKeyboardBindingPressedThisFrame(KeyboardBindingAction.MoveBackward) ||
             IsKeyboardBindingPressedThisFrame(KeyboardBindingAction.MoveRight)))
        {
            return true;
        }

        Gamepad gamepad = Gamepad.current;
        if (gamepad == null)
        {
            movementStickLatch = false;
            return false;
        }

        float thresholdSquared = stickThreshold * stickThreshold;
        bool active = gamepad.leftStick.ReadValue().sqrMagnitude > thresholdSquared;
        bool triggered = active && !movementStickLatch;
        movementStickLatch = active;
        return triggered;
    }

    public static bool CancelPressed()
    {
        InputAction resolved = ResolveCancelAction();
        if (resolved != null && resolved.WasPressedThisFrame())
        {
            return true;
        }

        return (Keyboard.current != null && IsKeyboardBindingPressedThisFrame(KeyboardBindingAction.Pause)) ||
               (Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame);
    }

    public static bool InventoryTogglePressed()
    {
        if (Keyboard.current != null && IsKeyboardBindingPressedThisFrame(KeyboardBindingAction.Inventory))
        {
            return true;
        }

        Gamepad gamepad = Gamepad.current;
        if (gamepad != null && (gamepad.startButton.wasPressedThisFrame || gamepad.selectButton.wasPressedThisFrame))
        {
            return true;
        }

        return false;
    }

    public static bool DialogueSubmitPressed()
    {
        InputAction resolved = ResolveSubmitAction();
        if (resolved != null && resolved.WasPressedThisFrame())
        {
            return true;
        }

        return (Keyboard.current != null && (IsKeyboardBindingPressedThisFrame(KeyboardBindingAction.DialogueSubmit) || Keyboard.current.numpadEnterKey.wasPressedThisFrame)) ||
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
            if (IsKeyboardBindingPressedThisFrame(KeyboardBindingAction.MoveForward) || keyboard.upArrowKey.wasPressedThisFrame)
            {
                navigate.y += 1f;
            }

            if (IsKeyboardBindingPressedThisFrame(KeyboardBindingAction.MoveBackward) || keyboard.downArrowKey.wasPressedThisFrame)
            {
                navigate.y -= 1f;
            }

            if (IsKeyboardBindingPressedThisFrame(KeyboardBindingAction.MoveLeft) || keyboard.leftArrowKey.wasPressedThisFrame)
            {
                navigate.x -= 1f;
            }

            if (IsKeyboardBindingPressedThisFrame(KeyboardBindingAction.MoveRight) || keyboard.rightArrowKey.wasPressedThisFrame)
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

    public static string GetInteractLabel() => GetBindingLabel(interactAction, GetKeyboardBindingLabel(KeyboardBindingAction.Interact));
    public static string GetDialogueSubmitLabel() => GetBindingLabel(ResolveSubmitAction() ?? interactAction, GetKeyboardBindingLabel(KeyboardBindingAction.DialogueSubmit));
    public static string GetDialogueCancelLabel() => GetBindingLabel(ResolveCancelAction(), GetKeyboardBindingLabel(KeyboardBindingAction.Pause));

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

    private static string GetKeyboardBindingLabel(KeyboardBindingAction action)
    {
        Key key = GetKeyboardBinding(action);
        return key.ToString().ToUpperInvariant();
    }

    private static bool IsKeyboardBindingHeld(KeyboardBindingAction action)
    {
        if (Keyboard.current == null)
        {
            return false;
        }

        Key key = GetKeyboardBinding(action);
        KeyControl control = Keyboard.current[key];
        return control != null && control.isPressed;
    }

    private static bool IsKeyboardBindingPressedThisFrame(KeyboardBindingAction action)
    {
        if (Keyboard.current == null)
        {
            return false;
        }

        Key key = GetKeyboardBinding(action);
        KeyControl control = Keyboard.current[key];
        return control != null && control.wasPressedThisFrame;
    }

    private static void EnsureKeyboardBindingsLoaded()
    {
        if (keyboardBindingsLoaded)
        {
            return;
        }

        keyboardBindings.Clear();
        for (int i = 0; i < keyboardBindingOrder.Length; i++)
        {
            KeyboardBindingAction action = keyboardBindingOrder[i];
            Key key = GetDefaultKeyboardBinding(action);
            string saved = PlayerPrefs.GetString(BindingPrefPrefix + action, string.Empty);
            if (!string.IsNullOrWhiteSpace(saved) && Enum.TryParse(saved, ignoreCase: true, out Key parsed))
            {
                key = parsed;
            }

            keyboardBindings[action] = key;
        }

        keyboardBindingsLoaded = true;
    }

    private static Key GetDefaultKeyboardBinding(KeyboardBindingAction action)
    {
        return action switch
        {
            KeyboardBindingAction.MoveForward => Key.W,
            KeyboardBindingAction.MoveBackward => Key.S,
            KeyboardBindingAction.MoveLeft => Key.A,
            KeyboardBindingAction.MoveRight => Key.D,
            KeyboardBindingAction.Sprint => Key.LeftShift,
            KeyboardBindingAction.RotateLeft => Key.Q,
            KeyboardBindingAction.RotateRight => Key.E,
            KeyboardBindingAction.Interact => Key.F,
            KeyboardBindingAction.Reload => Key.R,
            KeyboardBindingAction.Jump => Key.Space,
            KeyboardBindingAction.Flashlight => Key.L,
            KeyboardBindingAction.Inventory => Key.Tab,
            KeyboardBindingAction.Pause => Key.Escape,
            KeyboardBindingAction.DialogueSubmit => Key.Enter,
            _ => Key.None
        };
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
