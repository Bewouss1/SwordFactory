using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float mouseSensitivity = 3f;
    [SerializeField] private float cameraDistance = 4f;
    [SerializeField] private float minY = -40f;
    [SerializeField] private float maxY = 60f;
    [SerializeField] private LayerMask collisionMask = ~0;

    private Transform pivot;
    private float rotationX;
    private float rotationY;

    void Start()
    {
        pivot = transform.parent;
        transform.localPosition = new Vector3(0f, 0f, -cameraDistance);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (target == null || pivot == null) return;

        rotationX += Input.GetAxis("Mouse X") * mouseSensitivity;
        rotationY -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        rotationY = Mathf.Clamp(rotationY, minY, maxY);

        pivot.position = target.position;
        pivot.rotation = Quaternion.Euler(rotationY, rotationX, 0f);

        // Détection de collision
        Vector3 desiredPosition = pivot.position + pivot.rotation * new Vector3(0f, 0f, -cameraDistance);
        Vector3 direction = desiredPosition - pivot.position;
        float distance = direction.magnitude;

        if (Physics.SphereCast(pivot.position, 0.1f, direction.normalized, out RaycastHit hit, distance, collisionMask))
        {
            transform.position = pivot.position + direction.normalized * Mathf.Max(hit.distance - 0.2f, 0.5f);
        }
        else
        {
            transform.position = desiredPosition;
        }
    }
}
