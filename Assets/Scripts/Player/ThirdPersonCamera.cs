using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float mouseSensitivity = 3f;
    [SerializeField] private float minY = -40f;
    [SerializeField] private float maxY = 70f;

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

        rotationX += Input.GetAxis("Mouse X") * mouseSensitivity;
        rotationY -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        rotationY = Mathf.Clamp(rotationY, minY, maxY);

        target.rotation = Quaternion.Euler(rotationY, rotationX, 0);
    }
}
