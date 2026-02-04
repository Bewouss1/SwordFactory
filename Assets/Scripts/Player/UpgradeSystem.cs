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
    /// PROGRESSION EN CASCADE : Seul le mold de BASE le plus commun diminue jusqu'à exclusion,
    /// puis le suivant, etc. Les autres molds restent INTACTS.
    /// </summary>
    public void ApplyLuckBonus(SwordAttributesConfig.AttributeOption[] options, int upgradeLevel)
    {
        if (options == null || options.Length == 0 || upgradeLevel <= 0)
            return;

        const float levelMultiplier = 0.90f; // progression 2x plus lente que 0.80
        const float fadeMultiplier = 0.85f;  // lissage après seuil
        const float excludeOdds = 100f;      // 1/100
        const float minWeightFactor = 0.001f; // plancher (quasi impossible)

        // Sauvegarder les poids de base (pour les seuils de fade)
        float[] baseWeights = new float[options.Length];

        // Trouver les molds par ordre de rareté de BASE (du plus commun au plus rare)
        float baseTotal = 0f;
        for (int i = 0; i < options.Length; i++)
        {
            baseWeights[i] = options[i].weight;
            baseTotal += options[i].weight;
        }

        // Créer une liste d'indices triés par odds DE BASE croissantes (plus commun d'abord)
        var sortedIndices = BuildSortedIndices(options, baseTotal);

        // Trouver l'index de Gold pour le tracking
        int goldIndex = -1;
        for (int i = 0; i < options.Length; i++)
        {
            if (options[i].name == "Gold")
            {
                goldIndex = i;
                break;
            }
        }

        var goldProgression = new System.Text.StringBuilder();
        goldProgression.AppendLine("\n========== GOLD PROGRESSION TRACKING ==========");

        // Simuler niveau par niveau : PROGRESSION EN CASCADE
        for (int level = 1; level <= upgradeLevel; level++)
        {
            // Trouver le premier mold encore actif (pas encore exclu/quasi-exclu)
            int targetIndex = GetFirstActiveIndex(options, sortedIndices, baseWeights, minWeightFactor);

            if (targetIndex < 0)
                break; // Tous les molds exclus

            // Appliquer le multiplicateur UNIQUEMENT au mold ciblé
            options[targetIndex].weight *= levelMultiplier;

            // Stabiliser le total pour maintenir les odds des autres molds INTACTS
            float currentTotal = CalculateTotal(options);
            float targetOdds = currentTotal / options[targetIndex].weight;

            // Si le mold atteint le seuil 1/100, commencer à le fader
            if (targetOdds >= excludeOdds)
            {
                float minWeight = baseWeights[targetIndex] * minWeightFactor;
                
                // Appliquer fade pour lisser la transition
                if (options[targetIndex].weight > minWeight)
                {
                    options[targetIndex].weight *= fadeMultiplier;
                }
                else
                {
                    options[targetIndex].weight = 0f; // EXCLU définitivement
                }
            }

            // Log Gold progression
            if (goldIndex >= 0 && level <= 100 && options[goldIndex].weight > 0f)
            {
                float goldOdds = CalculateTotal(options) / options[goldIndex].weight;
                goldProgression.AppendLine($"Level {level}: Gold = 1/{goldOdds:F2}");
            }
        }

        goldProgression.AppendLine("========== END GOLD PROGRESSION ==========");
        Debug.Log(goldProgression.ToString());
    }

    private static float CalculateTotal(SwordAttributesConfig.AttributeOption[] options)
    {
        float total = 0f;
        foreach (var opt in options)
            total += opt.weight;
        return total;
    }

    private static int GetFirstActiveIndex(SwordAttributesConfig.AttributeOption[] options, System.Collections.Generic.List<int> sortedIndices, float[] baseWeights, float minWeightFactor)
    {
        foreach (int idx in sortedIndices)
        {
            float minWeight = baseWeights[idx] * minWeightFactor;
            if (options[idx].weight > minWeight)
                return idx;
        }

        return -1;
    }

    private static System.Collections.Generic.List<int> BuildSortedIndices(SwordAttributesConfig.AttributeOption[] options, float baseTotal)
    {
        var sortedIndices = new System.Collections.Generic.List<int>();
        for (int i = 0; i < options.Length; i++)
            sortedIndices.Add(i);

        sortedIndices.Sort((a, b) =>
        {
            float oddsA = baseTotal / options[a].weight;
            float oddsB = baseTotal / options[b].weight;
            return oddsA.CompareTo(oddsB); // Ordre croissant (1/1 avant 1/10)
        });

        return sortedIndices;
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
