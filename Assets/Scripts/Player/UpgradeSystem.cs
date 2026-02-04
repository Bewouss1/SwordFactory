using UnityEngine;
using System;

/// <summary>
/// Système d'upgrade pour améliorer les probabilités d'obtenir de meilleures raretés
/// Chaque upgrade applique un multiplicateur sur les chances
/// </summary>
public class UpgradeSystem : MonoBehaviour
{
    [System.Serializable]
    public class UpgradeCategory
    {
        public string name;                  // "Molder", "Quality", "Class", "Rarity"
        public int currentLevel = 0;         // Niveau actuel (0-100)
        public int maxLevel = 100;           // Niveau maximum
        public float baseCost = 100f;        // Coût de base
        public float costMultiplier = 1.15f; // Multiplicateur de coût par niveau
        
        /// <summary>
        /// Calcule le coût pour passer au niveau suivant
        /// </summary>
        public float GetNextLevelCost()
        {
            if (currentLevel >= maxLevel)
                return float.MaxValue;
            
            return baseCost * Mathf.Pow(costMultiplier, currentLevel);
        }
        
        /// <summary>
        /// Améliore le niveau si possible
        /// </summary>
        public bool TryUpgrade(float availableMoney, out float cost)
        {
            cost = GetNextLevelCost();
            
            if (currentLevel >= maxLevel || availableMoney < cost)
                return false;
            
            currentLevel++;
            return true;
        }
    }

    [Header("Upgrade Categories")]
    [SerializeField] private UpgradeCategory molder = new UpgradeCategory { name = "Molder", baseCost = 100f };
    [SerializeField] private UpgradeCategory quality = new UpgradeCategory { name = "Quality", baseCost = 150f };
    [SerializeField] private UpgradeCategory swordClass = new UpgradeCategory { name = "Class", baseCost = 200f };
    [SerializeField] private UpgradeCategory rarity = new UpgradeCategory { name = "Rarity", baseCost = 250f };

    public static UpgradeSystem Instance { get; private set; }

    public UpgradeCategory Molder => molder;
    public UpgradeCategory Quality => quality;
    public UpgradeCategory SwordClass => swordClass;
    public UpgradeCategory Rarity => rarity;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Calcule le multiplicateur de chance basé sur le niveau d'upgrade et la rareté de base
    /// LOGIQUE INVERSÉE : On multiplie les weights des rares pour les favoriser
    /// 
    /// Progressions observées (appliquées à l'INVERSE - on augmente les rares):
    /// - Très rare (weight faible) : multiplicateur agressif (1.036 par niveau)
    /// - Moyenne : multiplicateur modéré (1.026 par niveau)
    /// - Commune (weight élevé) : multiplicateur faible (1.024 par niveau)
    /// </summary>
    public float CalculateUpgradedWeight(float baseWeight, int upgradeLevel, int totalOptions)
    {
        if (upgradeLevel <= 0 || totalOptions <= 1)
            return baseWeight;

        // Déterminer le multiplicateur basé sur la rareté relative de l'option
        // Plus le weight est FAIBLE (rare), plus on le MULTIPLIE fortement
        // Plus le weight est ÉLEVÉ (commun), moins on le multiplie (ou on le diminue)
        
        float multiplier;
        if (baseWeight < 0.001f)
        {
            // Très rare : forte augmentation (inverse de 0.965)
            multiplier = Mathf.Pow(1.036f, upgradeLevel); // 1/0.965 ≈ 1.036
        }
        else if (baseWeight < 0.1f)
        {
            // Rare à moyenne : augmentation modérée (inverse de 0.975)
            multiplier = Mathf.Pow(1.026f, upgradeLevel); // 1/0.975 ≈ 1.026
        }
        else
        {
            // Commun : légère augmentation ou diminution (inverse de 0.977)
            multiplier = Mathf.Pow(0.977f, upgradeLevel); // On DIMINUE les communs
        }

        return baseWeight * multiplier;
    }

    /// <summary>
    /// Applique le bonus de luck aux poids d'un tableau d'options
    /// Chaque option reçoit un bonus basé sur sa rareté de base
    /// Les options rares bénéficient d'un bonus plus important
    /// </summary>
    public void ApplyLuckBonus(SwordAttributesConfig.AttributeOption[] options, int upgradeLevel)
    {
        if (options == null || options.Length == 0 || upgradeLevel <= 0)
            return;

        Debug.Log($"[UpgradeSystem] Applying luck bonus (Level {upgradeLevel}) to {options.Length} options");

        // Calculer les nouveaux poids avec le système d'upgrade
        for (int i = 0; i < options.Length; i++)
        {
            float oldWeight = options[i].weight;
            options[i].weight = CalculateUpgradedWeight(options[i].weight, upgradeLevel, options.Length);
            
            // Log pour les 3 plus rares
            if (i >= options.Length - 3)
            {
                float improvement = ((options[i].weight / oldWeight) - 1f) * 100f;
                Debug.Log($"[UpgradeSystem]   {options[i].name}: {oldWeight:F6} → {options[i].weight:F6} (+{improvement:F2}%)");
            }
        }
    }

    /// <summary>
    /// Tente d'améliorer une catégorie donnée
    /// </summary>
    public bool TryUpgradeCategory(UpgradeCategory category, PlayerMoney playerMoney)
    {
        if (category == null || playerMoney == null)
            return false;

        if (category.TryUpgrade(playerMoney.CurrentMoney, out float cost))
        {
            playerMoney.AddMoney(-cost);
            Debug.Log($"[UpgradeSystem] Upgraded {category.name} to level {category.currentLevel} for ${cost:F2}");
            return true;
        }

        return false;
    }
}
