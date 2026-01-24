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
        mainCamera = Camera.main; // Récupère la caméra principale
        uiButton.SetActive(false); // Cache le bouton au départ
        uiButton.transform.position = transform.position + offset;
    }

    void Update()
    {
        float distance = Vector3.Distance(player.position, transform.position);

        if (distance <= showDistance)
        {
            uiButton.SetActive(true);
            // Faire face à la caméra
            uiButton.transform.LookAt(mainCamera.transform);
            uiButton.transform.Rotate(0, 180f, 0); // Pour corriger l'orientation si le bouton est à l'envers
        }
        else
        {
            uiButton.SetActive(false);
        }
    }
}
