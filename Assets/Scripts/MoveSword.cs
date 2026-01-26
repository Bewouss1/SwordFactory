using UnityEngine;
using System.Collections;

public class MoveSword : MonoBehaviour
{
    [System.Serializable]
    public struct MoldOption
    {
        public string name;         // Label for the mold
        public float weight;        // Relative chance to pick this mold
    }

    public Transform[] pausePoints;   // Points dans la scène (conveyor)
    public float speed = 2f;         // Vitesse de déplacement
    public float waitTime = 1f;      // Temps d'attente à chaque point
    public MoldOption[] moldOptions;  // Options de mold avec pourcentages

    private Transform sword;          // L'épée à déplacer
    private int currentIndex = 0;     // Index du point actuel
    private Coroutine moveCoroutine; // Référence à la coroutine de déplacement
    private bool moldAssigned;       // Evite les ré-attributions

    // Appelé depuis le script de spawn
    public void SetSword(Transform newSword)
    {
        if (newSword == null)
        {
            Debug.LogError("MoveSword: Cannot set null sword transform!", this);
            return;
        }

        if (pausePoints == null || pausePoints.Length < 2)
        {
            Debug.LogError("MoveSword: PausePoints array is invalid (needs at least 2 points)!", this);
            return;
        }

        sword = newSword;
        currentIndex = 0;
        moldAssigned = false;

        sword.position = pausePoints[0].position;

        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);

        moveCoroutine = StartCoroutine(MoveRoutine());
    }

    IEnumerator MoveRoutine()
    {
        while (currentIndex < pausePoints.Length - 1)
        {
            if (sword == null)
            {
                Debug.LogWarning("MoveSword: Sword was destroyed during movement!", this);
                yield break;
            }

            if (pausePoints[currentIndex + 1] == null)
            {
                Debug.LogError($"MoveSword: PausePoint at index {currentIndex + 1} is null!", this);
                yield break;
            }

            Vector3 target = pausePoints[currentIndex + 1].position;

            while (Vector3.Distance(sword.position, target) > 0.01f)
            {
                if (sword == null)
                    yield break;

                sword.position = Vector3.MoveTowards(
                    sword.position,
                    target,
                    speed * Time.deltaTime
                );
                yield return null;
            }

            if (currentIndex == 0)
                AssignMoldOnce();

            yield return new WaitForSeconds(waitTime);
            currentIndex++;
        }
    }

    void AssignMoldOnce()
    {
        if (moldAssigned || sword == null || moldOptions == null || moldOptions.Length == 0)
            return;

        SwordMold swordMold = sword.GetComponent<SwordMold>();
        if (swordMold == null)
            return;

        string chosen = PickRandomMold();
        if (!string.IsNullOrEmpty(chosen))
        {
            swordMold.SetMold(chosen);
            moldAssigned = true;
        }
    }

    string PickRandomMold()
    {
        float totalWeight = 0f;
        for (int i = 0; i < moldOptions.Length; i++)
            totalWeight += Mathf.Max(0f, moldOptions[i].weight);

        if (totalWeight <= 0f)
            return string.Empty;

        float roll = Random.value * totalWeight;
        for (int i = 0; i < moldOptions.Length; i++)
        {
            float w = Mathf.Max(0f, moldOptions[i].weight);
            if (roll <= w)
                return moldOptions[i].name;

            roll -= w;
        }

        return moldOptions[moldOptions.Length - 1].name;
    }
}
