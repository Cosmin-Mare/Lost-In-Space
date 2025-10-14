using System;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    private PlayerInputActions playerInputActions;

    private static InputManager _instance;

    public static InputManager Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this);
        }
        else
        {
            _instance = this;
        }
        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
    }

    public Vector2 GetPlayerMovement()
    {
        return playerInputActions.Player.Move.ReadValue<Vector2>();
    }

    public Vector2 GetMouseDelta()
    {
        return playerInputActions.Player.Look.ReadValue<Vector2>();
    }

    public bool GetJump()
    {
        return playerInputActions.Player.Jump.triggered;
    }
}
