using UnityEngine;

public class ProximityButton : MonoBehaviour
{
    public Transform player;        // Référence au joueur pour la distance
    public GameObject uiButton;     // Le bouton UI à afficher
    public float showDistance = 3f; // Distance à partir de laquelle le bouton apparaît
    public Vector3 offset = new Vector3(0, 1, 0); // Décalage du bouton au-dessus du cube

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        
        if (mainCamera == null)
        {
            Debug.LogError("ProximityButton: Main camera not found!", this);
            enabled = false;
            return;
        }
        
        if (uiButton == null)
        {
            Debug.LogError("ProximityButton: UI Button reference is missing!", this);
            enabled = false;
            return;
        }
        
        uiButton.SetActive(false);
        uiButton.transform.position = transform.position + offset;
    }

    void Update()
    {
        if (player == null || uiButton == null || mainCamera == null)
            return;

        float distance = Vector3.Distance(player.position, transform.position);

        if (distance <= showDistance)
        {
            uiButton.SetActive(true);
            uiButton.transform.position = transform.position + offset;
            uiButton.transform.LookAt(mainCamera.transform);
            uiButton.transform.Rotate(0, 180f, 0);
        }
        else
        {
            uiButton.SetActive(false);
        }
    }
}
