using UnityEngine;
using System.Collections;

/// <summary>
/// Gère le mouvement d'une épée sur le convoyeur.
/// Responsabilité unique : déplacer l'épée entre les pause points et appeler les callbacks.
/// </summary>
public class SwordMovementBehavior : MonoBehaviour
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
        if (pausePoints != null && pausePoints.Length > 0)
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
                Debug.LogWarning("SwordMovementBehavior: Sword was destroyed during movement!");
                Destroy(this);
                yield break;
            }

            if (pausePoints[currentIndex + 1] == null)
            {
                Debug.LogError($"SwordMovementBehavior: PausePoint at index {currentIndex + 1} is null!");
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
