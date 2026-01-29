using UnityEngine;

public class GridSlotSpawner : MonoBehaviour
{
    [Header("Grid Configuration")]
    [SerializeField] private int rows = 6;
    [SerializeField] private int columns = 6;
    [SerializeField] private float slotSize = 1f;
    [SerializeField] private float plateHeight = 0.5f; // Hauteur de la plaque pour positionner les slots au-dessus

    [Header("Prefabs")]
    [SerializeField] private GameObject platePrefab;
    [SerializeField] private GameObject slotPrefab;

    [Header("Generation")]
    [SerializeField] private bool autoGenerate = false;
    [SerializeField] private bool clearExistingSlots = true;

    private SellZone sellZone;

    private void OnValidate()
    {
        if (autoGenerate)
        {
            GenerateGrid();
            autoGenerate = false;
        }
    }

    private void OnEnable()
    {
        sellZone = GetComponent<SellZone>();
    }

    public void GenerateGrid()
    {
        // Nettoyer les anciens slots et la plaque
        if (clearExistingSlots)
        {
            ClearExistingSlots();
        }

        // Calculer les dimensions totales de la grille (sans espacement)
        float gridWidth = columns * slotSize;
        float gridDepth = rows * slotSize;

        // Générer la plaque
        GameObject plate = null;
        if (platePrefab != null)
        {
            plate = Instantiate(platePrefab, Vector3.zero, Quaternion.identity, transform);
            plate.name = "Plate";
            plate.transform.localPosition = Vector3.zero;
            
            // Adapter la taille de la plaque à la grille
            plate.transform.localScale = new Vector3(gridWidth, plateHeight, gridDepth);
        }

        // Calculer le point de départ pour centrer la grille (sans espacement)
        Vector3 gridStart = new Vector3(
            -gridWidth / 2f + slotSize / 2f,
            plateHeight / 2f,  // Les slots commencent juste au-dessus de la plaque
            -gridDepth / 2f + slotSize / 2f
        );

        // Générer la grille de slots
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                // Calculer la position du slot (collés entre eux)
                Vector3 slotPosition = gridStart + new Vector3(
                    col * slotSize,
                    0f,
                    row * slotSize
                );

                CreateSlot(slotPosition, row, col);
            }
        }

        Debug.Log($"Grille {rows}x{columns} ({rows * columns} slots) + plaque {gridWidth}x{gridDepth} générée!");
    }

    private void CreateSlot(Vector3 position, int row, int col)
    {
        GameObject slotObject;

        if (slotPrefab != null)
        {
            // Instancier le préfab du slot
            slotObject = Instantiate(slotPrefab, position, Quaternion.identity, transform);
            slotObject.name = $"Slot_{row}_{col}";
        }
        else
        {
            // Fallback : créer un cube simple
            slotObject = new GameObject($"Slot_{row}_{col}");
            slotObject.transform.parent = transform;
            slotObject.transform.localPosition = position;

            // Ajouter un collider pour la détection
            BoxCollider collider = slotObject.AddComponent<BoxCollider>();
            collider.size = new Vector3(slotSize, slotSize, slotSize);

            // Ajouter un renderer pour visualiser
            MeshFilter meshFilter = slotObject.AddComponent<MeshFilter>();
            meshFilter.mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");

            MeshRenderer meshRenderer = slotObject.AddComponent<MeshRenderer>();
            Material defaultMat = new Material(Shader.Find("Standard"));
            defaultMat.color = new Color(0.5f, 0.8f, 1f, 0.3f);
            meshRenderer.material = defaultMat;
        }

        // Ajouter/mettre à jour le script SlotPosition
        SlotPosition slotPos = slotObject.GetComponent<SlotPosition>();
        if (slotPos == null)
        {
            slotPos = slotObject.AddComponent<SlotPosition>();
        }
        slotPos.SetGridPosition(row, col);
    }

    private void ClearExistingSlots()
    {
        // Supprimer tous les enfants sauf les objets spécifiés (la plate par exemple)
        foreach (Transform child in transform)
        {
            if (child.name.StartsWith("Slot_"))
            {
                DestroyImmediate(child.gameObject);
            }
        }
    }

#if UNITY_EDITOR
    // Bouton personnalisé dans l'inspecteur
    [ContextMenu("Generate Grid")]
    public void GenerateGridContextMenu()
    {
        GenerateGrid();
    }

    [ContextMenu("Clear Slots")]
    public void ClearSlotsContextMenu()
    {
        ClearExistingSlots();
    }
#endif
}
