using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    
    
    private InputSystem_Actions _actions;
    
    public event Action<Vector2> OnMove;
    public event Action<Vector2> OnLook;

    private bool _subscribed;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _actions = new InputSystem_Actions();
        _actions.Player.Enable();
        
        Subscribe();
    }

    private void OnDestroy()
    {
        UnSubscribe();
    }

    private void Subscribe()
    {
        if (_subscribed) return;
        
        _actions.Player.Move.performed += OnMovePerformed;
        _actions.Player.Move.canceled += OnMoveCancelled;
        _actions.Player.Look.performed += OnLookPerformed;
        _actions.Player.Look.canceled += OnLookCancelled;
        _subscribed = true;
    }
    
    private void UnSubscribe()
    {
        if (!_subscribed) return;
        
        _actions.Player.Move.performed -= OnMovePerformed;
        _actions.Player.Move.canceled -= OnMoveCancelled;
        _actions.Player.Look.performed -= OnLookPerformed;
        _actions.Player.Look.canceled -= OnLookCancelled;
        _subscribed = false;
    }
    
    public void OnMovePerformed(InputAction.CallbackContext context)
    {
        OnMove?.Invoke(context.ReadValue<Vector2>());
    }

    public void OnMoveCancelled(InputAction.CallbackContext context)
    {
        OnMove?.Invoke(Vector2.zero);
    }

    public void OnLookPerformed(InputAction.CallbackContext context)
    {
        OnLook?.Invoke(context.ReadValue<Vector2>());
    }
    
    public void OnLookCancelled(InputAction.CallbackContext context)
    {
        OnLook?.Invoke(Vector2.zero);
    }
}
