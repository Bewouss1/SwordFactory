using UnityEngine;

/// <summary>
/// Gère le spawn d'épées lorsque le joueur appuie sur E à proximité.
/// L'épée est ensuite déplacée par le ConveyorController.
/// </summary>
public class SpawnSwordOnInteract : MonoBehaviour
{
    [SerializeField] private Transform player;                          // Référence au joueur
    [SerializeField] private GameObject swordPrefab;                    // Prefab d'épée (doit avoir SwordStats)
    [SerializeField] private Transform swordsSpawnedContainer;          // Parent des épées dans la hiérarchie
    [SerializeField] private ConveyorController conveyorController;     // Contrôleur du convoyeur

    private ProximityButton proximityButton;

    void Awake()
    {
        proximityButton = GetComponent<ProximityButton>();
        ValidateReferences();
    }

    void ValidateReferences()
    {
        if (player == null)
            Debug.LogError("SpawnSwordOnInteract: Player reference is missing!", this);

        if (swordPrefab == null)
            Debug.LogError("SpawnSwordOnInteract: Sword prefab is missing!", this);

        if (conveyorController == null)
            Debug.LogError("SpawnSwordOnInteract: Conveyor controller is missing!", this);

        if (swordsSpawnedContainer == null)
            Debug.LogWarning("SpawnSwordOnInteract: SwordsSpawnedContainer is null (swords will be at root)", this);
    }

    void Update()
    {
        if (player == null || proximityButton == null)
            return;

        float distanceSqr = (player.position - transform.position).sqrMagnitude;

        // Utiliser proximityButton pour la distance (pas de duplication)
        if (distanceSqr <= proximityButton.ShowDistance * proximityButton.ShowDistance && Input.GetKeyDown(KeyCode.E))
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

        // Instancier l'épée
        GameObject swordInstance = Instantiate(
            swordPrefab,
            transform.position,
            Quaternion.identity,
            swordsSpawnedContainer
        );

        // Vérifier que l'épée a un composant SwordStats
        if (swordInstance.GetComponent<SwordStats>() == null)
        {
            Debug.LogWarning("SpawnSwordOnInteract: Sword prefab should have a SwordStats component!", swordInstance);
        }

        // Démarrer le mouvement via le convoyeur
        conveyorController.StartMovingSword(swordInstance.transform);
    }
}
