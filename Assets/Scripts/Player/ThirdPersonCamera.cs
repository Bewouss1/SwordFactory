using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform target;
    public float mouseSensitivity = 3f;
    public float minY = -40f;
    public float maxY = 70f;

    private float rotationX;
    private float rotationY;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (target == null)
        {
            Debug.LogError("ThirdPersonCamera: Target transform is missing!", this);
            enabled = false;
            return;
        }

        rotationX += Input.GetAxis("Mouse X") * mouseSensitivity;
        rotationY -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        rotationY = Mathf.Clamp(rotationY, minY, maxY);

        target.rotation = Quaternion.Euler(rotationY, rotationX, 0);
    }
}
