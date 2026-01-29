using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float rotationSpeed = 10f;

    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Transform cameraYawSource;
    [SerializeField] private Transform spawnPoint;  // Point de départ du joueur

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        
        // Téléporter le joueur au spawn point s'il existe
        if (spawnPoint != null)
        {
            transform.position = spawnPoint.position;
            transform.rotation = spawnPoint.rotation;
        }
    }

    void Update()
    {
        GroundCheck();
        Move();
        ApplyGravity();
    }

    void GroundCheck()
    {
        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0f)
            velocity.y = -2f;
    }

    void Move()
    {
        // Input
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 input = new Vector3(horizontal, 0f, vertical);
        input = Vector3.ClampMagnitude(input, 1f); // Limite la diagonale

        if (input.magnitude > 0f)
        {
            if (cameraTransform == null)
            {
                Debug.LogError("PlayerMovement: Camera transform reference is missing!", this);
                return;
            }

            // Déplacement relatif au yaw de la caméra (ignore le pitch pour éviter l'inversion)
            Transform yawSource = cameraYawSource != null
                ? cameraYawSource
                : (cameraTransform != null ? cameraTransform.parent : null);

            if (yawSource == null)
                yawSource = cameraTransform;

            float yaw = yawSource.eulerAngles.y;
            Quaternion yawRotation = Quaternion.Euler(0f, yaw, 0f);
            Vector3 moveDir = (yawRotation * input).normalized;

            // Déplacement horizontal
            controller.Move(moveDir * moveSpeed * Time.deltaTime);

            // Rotation fluide vers la direction
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Saut
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    void ApplyGravity()
    {
        // Appliquer la gravité
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
