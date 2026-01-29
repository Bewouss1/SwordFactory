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
        MovementBehavior behavior = swordTransform.gameObject.AddComponent<MovementBehavior>();
        behavior.Initialize(this, swordTransform, pausePoints, speed, waitTime);
    }

    /// <summary>
    /// Assigne les attributs en fonction du point de pause atteint
    /// Point 0 (pausePoint 1): Mold
    /// Point 1 (pausePoint 2): Quality
    /// Point 2 (pausePoint 3): Class
    /// Point 3 (pausePoint 4): Rarity
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
                swordAssigner.AssignRarity(swordStats);
                Debug.Log($"[Conveyor] Point 3: Assigned Rarity - {swordStats.Rarity}");
                break;

            default:
                // Points supplémentaires (pas d'assignement)
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
        }
        else
        {
            Debug.LogWarning("ConveyorController: All slots are occupied. Destroying sword.", this);
            Destroy(swordTransform.gameObject);
        }
    }

    /// <summary>
    /// Comportement de mouvement attaché dynamiquement à chaque épée
    /// </summary>
    private class MovementBehavior : MonoBehaviour
    {
        private ConveyorController controller;
        private Transform swordTransform;
        private Transform[] pausePoints;
        private float speed;
        private float waitTime;
        private int currentIndex = 0;

        public void Initialize(ConveyorController ctrl, Transform sword, Transform[] points, float spd, float wait)
        {
            controller = ctrl;
            swordTransform = sword;
            pausePoints = points;
            speed = spd;
            waitTime = wait;

            // Positionner au premier point
            swordTransform.position = pausePoints[0].position;

            // Démarrer le mouvement
            StartCoroutine(MoveRoutine());
        }

        private IEnumerator MoveRoutine()
        {
            while (currentIndex < pausePoints.Length - 1)
            {
                if (swordTransform == null)
                {
                    Debug.LogWarning("ConveyorController: Sword was destroyed during movement!");
                    Destroy(this);
                    yield break;
                }

                if (pausePoints[currentIndex + 1] == null)
                {
                    Debug.LogError($"ConveyorController: PausePoint at index {currentIndex + 1} is null!");
                    yield break;
                }

                Vector3 target = pausePoints[currentIndex + 1].position;

                // Déplacement fluide vers le point suivant
                while ((swordTransform.position - target).sqrMagnitude > 0.0001f)
                {
                    if (swordTransform == null)
                    {
                        Destroy(this);
                        yield break;
                    }

                    swordTransform.position = Vector3.MoveTowards(
                        swordTransform.position,
                        target,
                        speed * Time.deltaTime
                    );
                    yield return null;
                }

                // Assigner les attributs progressivement à chaque point
                controller.AssignAttributeAtPoint(swordTransform, currentIndex);

                yield return new WaitForSeconds(waitTime);
                currentIndex++;
            }

            // Épée termine le parcours : la placer dans la SellZone
            controller.PlaceSwordInSellZone(swordTransform);

            // Détruire ce comportement une fois le parcours terminé
            Destroy(this);
        }
    }
}
