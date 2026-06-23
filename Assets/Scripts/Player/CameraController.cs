using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Target")]
    public Transform playerTransform;
    public Vector3 offset = new Vector3(0f, 4f, -6f);

    [Header("Follow Settings")]
    public float followSpeed = 8f;
    public float rotationSpeed = 5f;

    [Header("Orbit Settings")]
    public float orbitSpeed = 3f;
    public float minVerticalAngle = -30f;
    public float maxVerticalAngle = 60f;
    private float currentYaw = 0f;
    private float currentPitch = 20f;

    [Header("Collision")]
    public LayerMask collisionMask = ~0;
    public float collisionRadius = 0.3f;
    public float clipOffset = 0.3f;

    [Header("Look At")]
    public Vector3 lookAtOffset = new Vector3(0f, 1.5f, 0f);

    private Vector3 currentVelocity;
    private Vector3 currentPosition;

    void Start()
    {
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerTransform = player.transform;
        }

        if (playerTransform != null)
        {
            currentPosition = playerTransform.position + offset;
            transform.position = currentPosition;
            transform.LookAt(playerTransform.position + lookAtOffset);
        }
    }

    void LateUpdate()
    {
        if (playerTransform == null) return;

        // Điểm đích camera cần di chuyển tới (luôn cố định ở phía sau nhân vật theo offset)
        Vector3 targetPosition = playerTransform.position + offset;

        // Di chuyển camera mượt mà tới vị trí đích
        currentPosition = Vector3.SmoothDamp(currentPosition, targetPosition, ref currentVelocity, 1f / followSpeed);
        transform.position = currentPosition;

        // Camera luôn hướng về phía nhân vật
        Vector3 lookTarget = playerTransform.position + lookAtOffset;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookTarget - currentPosition), rotationSpeed * Time.deltaTime);
    }
}
