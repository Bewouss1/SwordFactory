using UnityEngine;
using System.Collections;

public class MoveSword : MonoBehaviour
{
    public Transform[] pausePoints;   // Points dans la scène (conveyor)
    public float speed = 2f;         // Vitesse de déplacement
    public float waitTime = 1f;      // Temps d'attente à chaque point

    private Transform sword;          // L'épée à déplacer
    private int currentIndex = 0;     // Index du point actuel
    private Coroutine moveCoroutine; // Référence à la coroutine de déplacement

    // Appelé depuis le script de spawn
    public void SetSword(Transform newSword)
    {
        sword = newSword;
        currentIndex = 0;

        sword.position = pausePoints[0].position;

        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);

        moveCoroutine = StartCoroutine(MoveRoutine());
    }

    IEnumerator MoveRoutine()
    {
        while (currentIndex < pausePoints.Length - 1)
        {
            Vector3 target = pausePoints[currentIndex + 1].position;

            while (Vector3.Distance(sword.position, target) > 0.01f)
            {
                sword.position = Vector3.MoveTowards(
                    sword.position,
                    target,
                    speed * Time.deltaTime
                );
                yield return null;
            }

            yield return new WaitForSeconds(waitTime);
            currentIndex++;
        }
    }
}
