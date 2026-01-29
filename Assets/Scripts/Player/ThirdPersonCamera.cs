using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Transform pivotOverride;
    [SerializeField] private float mouseSensitivity = 3f;
    [SerializeField] private float minY = -40f;
    [SerializeField] private float maxY = 60f;

    [Header("Camera Follow")]
    [SerializeField] private Vector3 cameraOffset = new Vector3(0f, 2f, -4f);
    [SerializeField] private float collisionRadius = 0.2f;
    [SerializeField] private float collisionBuffer = 0.1f;
    [SerializeField] private float minDistance = 0.5f;
    [SerializeField] private LayerMask collisionMask = ~0;

    private float rotationX;
    private float rotationY;

    void Awake()
    {
        if (target == null)
        {
            Debug.LogError("ThirdPersonCamera: Target transform is missing!", this);
            enabled = false;
            return;
        }
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (target == null)
            return;

        Transform pivot = pivotOverride != null ? pivotOverride : (transform.parent != null ? transform.parent : target);

        rotationX += Input.GetAxis("Mouse X") * mouseSensitivity;
        rotationY -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        rotationY = Mathf.Clamp(rotationY, minY, maxY);

        // Le pivot gère la rotation caméra (yaw + pitch)
        pivot.position = target.position;
        pivot.rotation = Quaternion.Euler(rotationY, rotationX, 0f);

        Vector3 pivotPos = pivot.position;
        Vector3 desiredPosition = pivot.TransformPoint(cameraOffset);

        Vector3 direction = desiredPosition - pivotPos;
        float distance = direction.magnitude;
        Vector3 finalPosition = desiredPosition;

        if (distance > 0.001f)
        {
            if (Physics.SphereCast(
                pivotPos,
                collisionRadius,
                direction.normalized,
                out RaycastHit hit,
                distance,
                collisionMask,
                QueryTriggerInteraction.Ignore))
            {
                float adjustedDistance = Mathf.Max(hit.distance - collisionBuffer, minDistance);
                finalPosition = pivotPos + direction.normalized * adjustedDistance;
            }
        }

        transform.position = finalPosition;
        transform.LookAt(pivotPos);
    }
}
