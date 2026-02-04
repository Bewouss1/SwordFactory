using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Système d'événements centralisé pour découpler les composants du jeu
/// Utilise le pattern Observer pour une meilleure architecture
/// </summary>
public class GameEvents : MonoBehaviour
{
    public static GameEvents Instance { get; private set; }

    [System.Serializable]
    public class SwordEvent : UnityEvent<Transform> { }

    [System.Serializable]
    public class MoneyEvent : UnityEvent<float> { }

    [System.Serializable]
    public class LevelEvent : UnityEvent<int> { }

    // Événements liés aux épées
    public SwordEvent OnSwordCreated = new SwordEvent();
    public SwordEvent OnSwordSold = new SwordEvent();

    // Événements liés au joueur
    public MoneyEvent OnMoneyChanged = new MoneyEvent();
    public LevelEvent OnLevelUp = new LevelEvent();

    void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Méthodes helper pour déclencher les événements de manière centralisée
    public static void TriggerSwordCreated(Transform sword)
    {
        Instance?.OnSwordCreated?.Invoke(sword);
    }

    public static void TriggerSwordSold(Transform sword)
    {
        Instance?.OnSwordSold?.Invoke(sword);
    }

    public static void TriggerMoneyChanged(float newAmount)
    {
        Instance?.OnMoneyChanged?.Invoke(newAmount);
    }

    public static void TriggerLevelUp(int newLevel)
    {
        Instance?.OnLevelUp?.Invoke(newLevel);
    }
}
