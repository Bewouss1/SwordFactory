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
    /// Calcule le multiplicateur de chance basé sur le niveau d'upgrade
    /// REDISTRIBUTION : les rares augmentent, les communs diminuent
    /// </summary>
    public float CalculateUpgradedWeight(float baseWeight, int upgradeLevel, int totalOptions)
    {
        if (upgradeLevel <= 0)
            return baseWeight;

        // Stratégie : favoriser les rares (weight faible) et défavoriser les communs (weight élevé)
        float multiplier;
        
        if (baseWeight >= 0.1f) // Communs (Normal, Bronze...)
        {
            // DIMINUER : 0.96^level
            multiplier = Mathf.Pow(0.96f, upgradeLevel);
        }
        else if (baseWeight >= 0.001f) // Rares moyens (Silver, Gold, Sapphire...)
        {
            // AUGMENTER modérément : 1.02^level
            multiplier = Mathf.Pow(1.02f, upgradeLevel);
        }
        else // Ultra rares (Ruby, Diamond...)
        {
            // AUGMENTER fortement : 1.04^level
            multiplier = Mathf.Pow(1.04f, upgradeLevel);
        }

        return baseWeight * multiplier;
    }

    /// <summary>
    /// Applique le bonus de luck aux poids d'un tableau d'options
    /// FORMULE DYNAMIQUE : redistribue progressivement vers 1/1 puis au-delà
    /// </summary>
    public void ApplyLuckBonus(SwordAttributesConfig.AttributeOption[] options, int upgradeLevel)
    {
        if (options == null || options.Length == 0 || upgradeLevel <= 0)
            return;

        Debug.Log($"[UpgradeSystem] Applying luck bonus (Level {upgradeLevel}) to {options.Length} options");

        // Appliquer upgrade progressif niveau par niveau
        for (int level = 1; level <= upgradeLevel; level++)
        {
            // Calculer le total actuel
            float totalWeight = 0f;
            foreach (var opt in options)
                totalWeight += opt.weight;

            // Appliquer la transformation à chaque option
            for (int i = 0; i < options.Length; i++)
            {
                float currentOdds = totalWeight / options[i].weight; // 1/X actuel
                
                // Si odds > 1 (rare) → diminuer odds (augmenter weight)
                // Si odds < 1 (ultra commun post-1/1) → augmenter odds (diminuer weight)
                // Point pivot à 1/1
                
                float multiplier;
                if (currentOdds > 1.0f)
                {
                    // Rare : rapprocher de 1/1 en augmentant le weight
                    // Plus c'est rare, plus on augmente fort
                    float rarityFactor = Mathf.Log10(currentOdds + 1f); // Log pour progression douce
                    multiplier = 1f + (rarityFactor * 0.08f); // 8% par niveau (était 4%)
                }
                else
                {
                    // Déjà au-delà de 1/1 : continuer à diminuer le weight
                    multiplier = 0.92f; // -8% par niveau (était -4%)
                }
                
                options[i].weight *= multiplier;
            }
        }

        // Log final
        float finalTotal = 0f;
        foreach (var opt in options)
            finalTotal += opt.weight;
        
        for (int i = 0; i < Mathf.Min(5, options.Length); i++)
        {
            float odds = finalTotal / options[i].weight;
            Debug.Log($"[UpgradeSystem]   {options[i].name}: 1/{odds:F2}");
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
