using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Vector2 _movementVector;
    private Vector2 _lookVector;
    private CharacterController _characterController;
    public Camera lookCamera;
    
    [Header("Movement")]
    public float speed = 5f;
    public float gravity = -9.81f;
    
    [Header("Mouse Look")]
    public float mouseSensitivity = 15f;
    private float _xRotation;
    
    private float _yVelocity;
    private bool _subscribed;

    private void Start()
    {
        _characterController = GetComponent<CharacterController>();
        if (_characterController == null)
        {
            Debug.LogError("CharacterController is null");
            return;
        }

        if (InputManager.Instance != null) Subscribe();
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnDestroy()
    {
        if (_subscribed) UnSubscribe();
    }

    private void Subscribe()
    {
        if (_subscribed || InputManager.Instance == null) return;
        InputManager.Instance.OnMove += Move;
        InputManager.Instance.OnLook += Look;
        _subscribed = true;
    }

    private void UnSubscribe()
    {
        if (!_subscribed || InputManager.Instance == null) return;
        InputManager.Instance.OnMove -= Move;
        InputManager.Instance.OnLook -= Look;
        _subscribed = false;
    }

    private void Move(Vector2 movementVector)
    {
        _movementVector = movementVector;
    }

    private void Look(Vector2 lookVector)
    {
        _lookVector = lookVector;
    }

    void Update()
    {
        Vector3 forward = lookCamera.transform.forward;
        forward.y = 0;
        forward.Normalize();
    
        Vector3 right = lookCamera.transform.right;
        right.y = 0;
        right.Normalize();
    
        Vector3 movement = (right * _movementVector.x + forward * _movementVector.y) * speed;
    
        if (_characterController.isGrounded && _yVelocity < 0)
            _yVelocity = -2f;
        _yVelocity += gravity * Time.deltaTime;
        movement.y = _yVelocity;
    
        _characterController.Move(movement * Time.deltaTime);
    
        float mouseX = _lookVector.x * mouseSensitivity * Time.deltaTime;
        float mouseY = _lookVector.y * mouseSensitivity * Time.deltaTime;
    
        transform.Rotate(Vector3.up * mouseX);
        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);
        lookCamera.transform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
    }

}
