using UnityEngine;

public class SpawnSwordOnInteract : MonoBehaviour
{
    public Transform player;                        // Joueur
    public GameObject uiButton;                     // Bouton UI à distance
    public GameObject swordPrefab;                  // Épée à instancier
    public Transform SwordsSpawnedContainer;       // Parent des épées instanciées
    public MoveSword conveyorController;           // Pour récupérer les pausePoints

    private ProximityButton button;       // Référence au script du même GameObject

    void Awake()
    {
        button = GetComponent<ProximityButton>();
    }

    void Update()
    {
        float distance = Vector3.Distance(player.position, transform.position);

        // Utilise la showDistance du ProximityButton
        if (button != null && distance <= button.showDistance && Input.GetKeyDown(KeyCode.E))
        {
            SpawnSword();
        }
    }

    void SpawnSword()
    {
        GameObject swordInstance = Instantiate(
            swordPrefab,
            transform.position,
            Quaternion.identity,
            SwordsSpawnedContainer
        );

        MoveSword moveSword = swordInstance.GetComponent<MoveSword>();

        moveSword.pausePoints = conveyorController.pausePoints;
        moveSword.SetSword(swordInstance.transform);
    }
}
