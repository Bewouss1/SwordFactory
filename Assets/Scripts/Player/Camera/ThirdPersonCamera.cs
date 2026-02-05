using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float mouseSensitivity = GameConstants.DEFAULT_MOUSE_SENSITIVITY;
    [SerializeField] private float cameraDistance = GameConstants.DEFAULT_CAMERA_DISTANCE;
    [SerializeField] private float minY = GameConstants.DEFAULT_CAMERA_MIN_Y;
    [SerializeField] private float maxY = GameConstants.DEFAULT_CAMERA_MAX_Y;
    [SerializeField] private LayerMask collisionMask = ~0;
    [SerializeField] private KeyCode toggleCursorKey = KeyCode.F;

    private Transform pivot;
    private float rotationX;
    private float rotationY;
    private bool cursorUnlocked;
    
    // Cache pour éviter allocations à chaque frame
    private Vector3 cachedDesiredPosition;
    private Vector3 cachedDirection;

    void Start()
    {
        pivot = transform.parent;
        transform.localPosition = new Vector3(0f, 0f, -cameraDistance);
        SetCursorState(false);
    }

    void LateUpdate()
    {
        if (target == null || pivot == null) return;

        // Vérifier si le panel d'upgrade est visible - ne pas bouger la caméra avec la souris dans ce cas
        UpgradeUI upgradeUI = FindFirstObjectByType<UpgradeUI>();
        bool upgradeUIActive = upgradeUI != null && upgradeUI.IsUpgradePanelActive();

        if (upgradeUIActive && !cursorUnlocked)
        {
            SetCursorState(true);
        }
        else if (!upgradeUIActive && Input.GetKeyDown(toggleCursorKey))
        {
            SetCursorState(!cursorUnlocked);
        }

        if (!upgradeUIActive && !cursorUnlocked)
        {
            rotationX += Input.GetAxis("Mouse X") * mouseSensitivity;
            rotationY -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            rotationY = Mathf.Clamp(rotationY, minY, maxY);
        }

        pivot.position = target.position;
        pivot.rotation = Quaternion.Euler(rotationY, rotationX, 0f);

        // Détection de collision - réutiliser les vecteurs en cache
        cachedDesiredPosition = pivot.position + pivot.rotation * new Vector3(0f, 0f, -cameraDistance);
        cachedDirection = cachedDesiredPosition - pivot.position;
        float distance = cachedDirection.magnitude;

        if (Physics.SphereCast(pivot.position, 0.1f, cachedDirection.normalized, out RaycastHit hit, distance, collisionMask))
        {
            transform.position = pivot.position + cachedDirection.normalized * Mathf.Max(hit.distance - 0.2f, 0.5f);
        }
        else
        {
            transform.position = cachedDesiredPosition;
        }
    }

    private void SetCursorState(bool unlocked)
    {
        cursorUnlocked = unlocked;
        Cursor.lockState = unlocked ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = unlocked;
    }
}
