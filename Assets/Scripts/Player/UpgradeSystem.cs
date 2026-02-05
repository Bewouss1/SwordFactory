using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Système d'upgrade pour améliorer les probabilités d'obtenir de meilleures raretés
/// Utilise un système de fallback continu : le mold le plus commun (1/X minimal) stagne,
/// tous les autres s'améliorent progressivement au coefficient 0.989
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
        
        // Système de fallback : track des molds plafonnés à 1/1 (deviennent le nouveau minimal)
        [System.NonSerialized]
        public List<string> cappedAt1Options = new List<string>();
        
        // Track des molds complètement retirés de l'affichage (quand un autre les remplace)
        [System.NonSerialized]
        public List<string> removedOptions = new List<string>();
        
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
    /// Trouve l'index du mold minimal (celui avec les odds minimales = plus commun actuellement)
    /// </summary>
    private int FindMinimalOptionIndex(SwordAttributesConfig.AttributeOption[] options, UpgradeCategory category)
    {
        int minimalIndex = -1;
        float minOdds = float.MaxValue;

        for (int i = 0; i < options.Length; i++)
        {
            // Vérifier si ce mold est dans la liste des retirés
            if (category.removedOptions.Contains(options[i].name))
                continue;

            // Calculer les odds (1/X) pour ce mold
            // Plus X est petit, plus c'est commun
            if (options[i].weight > 0f)
            {
                float odds = 1f / options[i].weight;
                if (odds < minOdds)
                {
                    minOdds = odds;
                    minimalIndex = i;
                }
            }
        }

        return minimalIndex;
    }

    /// <summary>
    /// Applique le bonus de luck avec le système de fallback continu basé sur les ODDS
    /// Système : le mold minimal stagne, tous les autres voient leurs odds s'améliorer
    /// Quand un mold atteint les odds du minimal, il devient le nouveau minimal et l'ancien est retiré
    /// </summary>
    public void ApplyLuckBonus(SwordAttributesConfig.AttributeOption[] options, int upgradeLevel, UpgradeCategory category = null)
    {
        if (options == null || options.Length == 0 || upgradeLevel <= 0)
            return;

        // Coefficient d'amélioration : les odds diminuent (chances augmentent)
        // 0.91 = environ 9% d'amélioration par upgrade
        const float oddsImprovementFactor = 0.91f;

        // Créer des listes temporaires pour la simulation
        var tempCappedOptions = new List<string>();
        var tempRemovedOptions = new List<string>();
        if (category != null)
        {
            tempCappedOptions.AddRange(category.cappedAt1Options);
            tempRemovedOptions.AddRange(category.removedOptions);
        }

        // Initialiser les odds : utiliser 1 pour les cappés, baseOdds pour les autres
        float[] currentOdds = new float[options.Length];
        for (int i = 0; i < options.Length; i++)
        {
            if (tempCappedOptions.Contains(options[i].name))
            {
                currentOdds[i] = 1f; // Les cappés restent à 1/1
            }
            else
            {
                currentOdds[i] = options[i].baseOdds;
            }
        }

        // À chaque upgrade, améliorer les odds des non-minimaux
        for (int level = 1; level <= upgradeLevel; level++)
        {
            // Trouver l'index du minimal (celui avec les odds les plus basses parmi les actifs)
            int minimalIndex = -1;
            float minOdds = float.MaxValue;

            for (int i = 0; i < options.Length; i++)
            {
                if (tempRemovedOptions.Contains(options[i].name))
                    continue;

                if (currentOdds[i] < minOdds)
                {
                    minOdds = currentOdds[i];
                    minimalIndex = i;
                }
            }

            if (minimalIndex < 0)
                break;

            // Améliorer les odds des non-minimaux
            for (int i = 0; i < options.Length; i++)
            {
                if (tempRemovedOptions.Contains(options[i].name))
                    continue;

                if (i != minimalIndex)
                {
                    // Skip si déjà cappé à 1/1
                    if (tempCappedOptions.Contains(options[i].name))
                        continue;

                    // Améliorer les odds
                    float newOdds = currentOdds[i] * oddsImprovementFactor;
                    
                    // Si on atteint ou dépasse le minimal
                    if (newOdds <= currentOdds[minimalIndex])
                    {
                        // Plafonner exactement au niveau du minimal
                        currentOdds[i] = currentOdds[minimalIndex];
                        
                        // Si le minimal est à 1/1, marquer ce mold comme cappé aussi
                        if (currentOdds[minimalIndex] <= 1f)
                        {
                            if (!tempCappedOptions.Contains(options[i].name))
                            {
                                tempCappedOptions.Add(options[i].name);
                            }
                            
                            // Retirer l'ancien minimal de l'affichage
                            if (!tempRemovedOptions.Contains(options[minimalIndex].name))
                            {
                                tempRemovedOptions.Add(options[minimalIndex].name);
                            }
                        }
                    }
                    else
                    {
                        currentOdds[i] = newOdds;
                    }
                }
            }
        }
        
        // Mettre à jour les listes de la catégorie
        if (category != null)
        {
            category.cappedAt1Options.Clear();
            category.cappedAt1Options.AddRange(tempCappedOptions);
            category.removedOptions.Clear();
            category.removedOptions.AddRange(tempRemovedOptions);
        }

        // Stocker les odds finales dans les weights pour le système de tirage
        // (le système de tirage en cascade les utilisera directement)
        for (int i = 0; i < options.Length; i++)
        {
            if (tempRemovedOptions.Contains(options[i].name))
            {
                options[i].weight = 0f;
            }
            else
            {
                // Stocker les odds directement dans weight (sera utilisé par le test cascade)
                options[i].weight = currentOdds[i];
            }
        }
    }

    /// <summary>
    /// Applique le bonus de luck avec le système de fallback continu
    /// Overload pour compatibilité avec l'ancien code (sans category)
    /// </summary>
    public void ApplyLuckBonus(SwordAttributesConfig.AttributeOption[] options, int upgradeLevel)
    {
        // Créer une catégorie temporaire pour tracker les retirés
        var tempCategory = new UpgradeCategory { name = "Temp" };
        ApplyLuckBonus(options, upgradeLevel, tempCategory);
    }

    /// <summary>
    /// Debug : affiche la progression des probas du niveau 1 à maxLevel
    /// </summary>
    public void LogProgressionLevels(SwordAttributesConfig.AttributeOption[] baseOptions, UpgradeCategory category, int maxLevel = 50)
    {
        if (baseOptions == null || baseOptions.Length == 0)
            return;

        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"\n========== PROGRESSION {category.name} (Level 0 to {maxLevel}) ==========");

        // Logger les odds de base
        sb.Append("BASE ODDS: ");
        foreach (var opt in baseOptions)
        {
            sb.Append($"{opt.name}=1/{opt.baseOdds:F1} ");
        }
        sb.AppendLine();
        
        // Logger le level 0
        sb.Append("Level 0: ");
        foreach (var opt in baseOptions)
        {
            sb.Append($"{opt.name}=1/{opt.baseOdds:F1} ");
        }
        sb.AppendLine();

        // Simuler chaque niveau
        for (int level = 1; level <= maxLevel; level++)
        {
            // Créer une copie pour ce niveau
            var levelOptions = new SwordAttributesConfig.AttributeOption[baseOptions.Length];
            System.Array.Copy(baseOptions, levelOptions, baseOptions.Length);
            
            var tempCategory = new UpgradeCategory { name = category.name };
            ApplyLuckBonus(levelOptions, level, tempCategory);

            // Calculer le total (sans les retirés)
            float totalWeight = 0f;
            foreach (var opt in levelOptions)
            {
                if (!tempCategory.removedOptions.Contains(opt.name))
                    totalWeight += opt.weight;
            }

            // Logger ce niveau
            sb.Append($"Level {level}: ");
            foreach (var opt in levelOptions)
            {
                if (tempCategory.removedOptions.Contains(opt.name))
                {
                    sb.Append($"{opt.name}=REMOVED ");
                }
                else
                {
                    // weight contient maintenant directement les odds
                    sb.Append($"{opt.name}=1/{opt.weight:F1} ");
                }
            }
            sb.AppendLine();
        }

        sb.AppendLine("========== END PROGRESSION ==========\n");
        Debug.Log(sb.ToString());
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
