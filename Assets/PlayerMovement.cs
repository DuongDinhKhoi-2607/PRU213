using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float _movementSpeed = 8f;
    [SerializeField] private float _runMultiplier = 2f;
    [SerializeField] private float _gravity = -15f;
    [SerializeField] private float _jumpHeight = 2f;
}
