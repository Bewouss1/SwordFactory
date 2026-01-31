using UnityEngine;
using System.Collections;

/// <summary>
/// Contrôle un convoyeur qui déplace des épées entre plusieurs points.
/// Ce script doit être attaché au GameObject du convoyeur dans la scène.
/// Responsabilité : Gestion du mouvement uniquement. L'assignement des attributs
/// est délégué à SwordAssigner.
/// </summary>
public class ConveyorController : MonoBehaviour
{
    [Header("Conveyor Settings")]
    [SerializeField] private Transform[] pausePoints;   // Points du parcours
    [SerializeField] private float speed = 2f;          // Vitesse de déplacement
    [SerializeField] private float waitTime = 1f;       // Temps d'attente à chaque point
    
    [Header("Dependencies")]
    [SerializeField] private SwordAssigner swordAssigner;  // Pour assigner les attributs
    [SerializeField] private SellZone sellZone;            // Zone de vente pour placer les épées finies
    [SerializeField] private PlayerLevel playerLevel;      // Référence pour le niveau du joueur

    public Transform[] PausePoints => pausePoints;

    /// <summary>
    /// Démarre le mouvement d'une épée sur le convoyeur
    /// </summary>
    public void StartMovingSword(Transform swordTransform)
    {
        if (swordTransform == null)
        {
            Debug.LogError("ConveyorController: Cannot move null sword transform!", this);
            return;
        }

        if (pausePoints == null || pausePoints.Length < 2)
        {
            Debug.LogError("ConveyorController: PausePoints array is invalid (needs at least 2 points)!", this);
            return;
        }

        // Attacher le comportement de mouvement à l'épée
        SwordMovementBehavior behavior = swordTransform.gameObject.AddComponent<SwordMovementBehavior>();
        behavior.Initialize(this, swordTransform, pausePoints, speed, waitTime);
    }

    /// <summary>
    /// Assigne les attributs en fonction du point de pause atteint
    /// Point 0: Mold
    /// Point 1: Quality
    /// Point 2: Class
    /// Point 3: Level
    /// Point 4: Enchant
    /// Point 5: Rarity
    /// </summary>
    public void AssignAttributeAtPoint(Transform swordTransform, int pointIndex)
    {
        if (swordAssigner == null)
        {
            Debug.LogWarning("ConveyorController: SwordAssigner reference is missing!", this);
            return;
        }

        SwordStats swordStats = swordTransform.GetComponent<SwordStats>();
        if (swordStats == null)
        {
            Debug.LogWarning("ConveyorController: Sword has no SwordStats component!", swordTransform);
            return;
        }

        switch (pointIndex)
        {
            case 0:
                swordAssigner.AssignMold(swordStats);
                Debug.Log($"[Conveyor] Point 0: Assigned Mold - {swordStats.Mold}");
                break;

            case 1:
                swordAssigner.AssignQuality(swordStats);
                Debug.Log($"[Conveyor] Point 1: Assigned Quality - {swordStats.Quality}");
                break;

            case 2:
                swordAssigner.AssignClass(swordStats);
                Debug.Log($"[Conveyor] Point 2: Assigned Class - {swordStats.SwordClass}");
                break;

            case 3:
                swordAssigner.AssignLevel(swordStats, playerLevel);
                Debug.Log($"[Conveyor] Point 3: Assigned Level - {swordStats.Level}");
                break;

            case 4:
                swordAssigner.AssignEnchant(swordStats);
                Debug.Log($"[Conveyor] Point 4: Assigned Enchant - {swordStats.Enchant}");
                break;

            case 5:
                swordAssigner.AssignRarity(swordStats);
                Debug.Log($"[Conveyor] Point 5: Assigned Rarity - {swordStats.Rarity}");
                break;

            default:
                // Points supplémentaires: pas d'assignement
                break;
        }
    }

    /// <summary>
    /// Place l'épée terminée dans la SellZone
    /// </summary>
    public void PlaceSwordInSellZone(Transform swordTransform)
    {
        if (sellZone == null)
        {
            Debug.LogWarning("ConveyorController: SellZone reference is missing! Destroying sword instead.", this);
            Destroy(swordTransform.gameObject);
            return;
        }

        Transform slot = sellZone.GetNextSlot();
        if (slot != null) 
        {
            swordTransform.position = slot.position;
            swordTransform.rotation = slot.rotation;
            swordTransform.SetParent(slot); // Optionnel : rendre l'épée enfant du slot

            SwordStats stats = swordTransform.GetComponent<SwordStats>();
            if (stats != null)
            {
                Debug.Log($"[Conveyor] Sword placed in SellZone: {stats.GetSummary()}");
            }

            sellZone.StartSellCountdown(swordTransform, slot);
        }
        else
        {
            Debug.LogWarning("ConveyorController: All slots are occupied. Destroying sword.", this);
            Destroy(swordTransform.gameObject);
        }
    }
}
