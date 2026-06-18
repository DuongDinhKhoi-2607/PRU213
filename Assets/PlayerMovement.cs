using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float _movementSpeed = 8f;
    [SerializeField] private float _runMultiplier = 2f;
    [SerializeField] private float _gravity = -15f;
    [SerializeField] private float _jumpHeight = 2f;

    private CharacterController _characterController;
    private Vector3 _velocity;

    private void Start()
    {
        _characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        // Ground check
        if (_characterController.isGrounded && _velocity.y < 0)
        {
            _velocity.y = -2f; // Snaps the character to the ground
        }

        // Get movement inputs
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // Calculate direction relative to player facing direction
        Vector3 move = transform.right * x + transform.forward * z;

        // Apply running speed multiplier if Left Shift is held
        float currentSpeed = _movementSpeed;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            currentSpeed *= _runMultiplier;
        }

        // Move the player (horizontal)
        _characterController.Move(move * currentSpeed * Time.deltaTime);

        // Jump logic
        if (Input.GetButtonDown("Jump") && _characterController.isGrounded)
        {
            _velocity.y = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
        }

        // Apply gravity over time
        _velocity.y += _gravity * Time.deltaTime;

        // Move the player (vertical gravity and jump velocity)
        _characterController.Move(_velocity * Time.deltaTime);
    }
}
