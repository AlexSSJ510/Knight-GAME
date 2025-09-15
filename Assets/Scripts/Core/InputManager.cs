using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    private PlayerInputActions _actions;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(this);

        _actions = new PlayerInputActions();
        _actions.Enable();
    }

    public float GetHorizontalAxis() => _actions.Player.Move.ReadValue<Vector2>().x;
    public bool IsJumpPressed() => _actions.Player.Jump.WasPressedThisFrame();
    public bool IsAttackPressed() => _actions.Player.Attack.WasPressedThisFrame();
    public bool IsShootPressed() => _actions.Player.Shoot.IsPressed();
    public bool IsDashPressed() => _actions.Player.Dash.WasPressedThisFrame();
}