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
        if (player == null)
            return;

        float distance = Vector3.Distance(player.position, transform.position);

        if (button != null && distance <= button.showDistance && Input.GetKeyDown(KeyCode.E))
        {
            SpawnSword();
        }
    }

    void SpawnSword()
    {
        if (swordPrefab == null)
        {
            Debug.LogError("SpawnSwordOnInteract: Sword prefab is missing!", this);
            return;
        }

        if (conveyorController == null)
        {
            Debug.LogError("SpawnSwordOnInteract: Conveyor controller reference is missing!", this);
            return;
        }

        if (conveyorController.pausePoints == null || conveyorController.pausePoints.Length < 2)
        {
            Debug.LogError("SpawnSwordOnInteract: Conveyor pausePoints array is invalid (needs at least 2 points)!", this);
            return;
        }

        GameObject swordInstance = Instantiate(
            swordPrefab,
            transform.position,
            Quaternion.identity,
            SwordsSpawnedContainer
        );

        MoveSword moveSword = swordInstance.GetComponent<MoveSword>();
        
        if (moveSword == null)
        {
            Debug.LogError("SpawnSwordOnInteract: Spawned sword prefab missing MoveSword component!", swordInstance);
            Destroy(swordInstance);
            return;
        }

        moveSword.pausePoints = conveyorController.pausePoints;
        moveSword.SetSword(swordInstance.transform);
    }
}
