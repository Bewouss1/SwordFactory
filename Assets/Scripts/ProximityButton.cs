using UnityEngine;

public class ProximityButton : MonoBehaviour
{
    [SerializeField] private Transform player;        // Référence au joueur pour la distance
    [SerializeField] private GameObject uiButton;     // Le bouton UI à afficher
    [SerializeField] private float showDistance = 3f; // Distance à partir de laquelle le bouton apparaît
    [SerializeField] private Vector3 offset = new Vector3(0, 1, 0); // Décalage du bouton au-dessus du cube

    private Camera mainCamera;
    private float showDistanceSqr; // Distance au carré pour optimisation

    public float ShowDistance => showDistance;

    void Start()
    {
        mainCamera = Camera.main;
        showDistanceSqr = showDistance * showDistance;
        
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

        float distanceSqr = (player.position - transform.position).sqrMagnitude;

        if (distanceSqr <= showDistanceSqr)
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
