using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Gère l'assignement de tous les attributs d'une épée (moule, qualité, classe, rareté, etc.)
/// Responsabilité unique : configurateur d'épée
/// </summary>
public class SwordAssigner : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private SwordAttributesConfig attributesConfig;
    [SerializeField] private EnchantmentConfig enchantmentConfig;

    void OnEnable()
    {
        ValidateOptions();
    }

    private void ValidateOptions()
    {
        if (attributesConfig == null)
        {
            Debug.LogWarning("SwordAssigner: Attributes config is missing!", this);
            return;
        }

        if (attributesConfig.moldOptions == null || attributesConfig.moldOptions.Length == 0)
            Debug.LogWarning("SwordAssigner: Mold options are not configured!", this);

        if (attributesConfig.qualityOptions == null || attributesConfig.qualityOptions.Length == 0)
            Debug.LogWarning("SwordAssigner: Quality options are not configured!", this);

        if (attributesConfig.classOptions == null || attributesConfig.classOptions.Length == 0)
            Debug.LogWarning("SwordAssigner: Class options are not configured!", this);

        if (attributesConfig.rarityOptions == null || attributesConfig.rarityOptions.Length == 0)
            Debug.LogWarning("SwordAssigner: Rarity options are not configured!", this);
    }

    /// <summary>
    /// Assigne tous les attributs à une épée
    /// </summary>
    public void AssignAllAttributes(Transform swordTransform)
    {
        if (swordTransform == null)
        {
            Debug.LogError("SwordAssigner: Cannot assign to null sword transform!", this);
            return;
        }

        SwordStats swordStats = swordTransform.GetComponent<SwordStats>();
        if (swordStats == null)
        {
            Debug.LogWarning("SwordAssigner: Sword must have SwordStats component!", swordTransform);
            return;
        }

        AssignMold(swordStats);
        AssignQuality(swordStats);
        AssignClass(swordStats);
        AssignRarity(swordStats);
        AssignEnchantments(swordStats);
    }

    void Start()
    {
        // Debug : afficher la progression des niveaux 1-30 au démarrage
        if (UpgradeSystem.Instance != null && attributesConfig != null && attributesConfig.moldOptions != null)
        {
            UpgradeSystem.Instance.LogProgressionLevels(attributesConfig.moldOptions, UpgradeSystem.Instance.Molder, 30);
        }
    }

    /// <summary>
    /// Assigne uniquement le moule
    /// </summary>
    public void AssignMold(SwordStats swordStats)
    {
        string moldName = PickRandomMold();
        swordStats.SetMold(moldName);
    }

    /// <summary>
    /// Assigne uniquement la qualité
    /// </summary>
    public void AssignQuality(SwordStats swordStats)
    {
        string qualityName = PickRandomQuality();
        swordStats.SetQuality(qualityName);
    }

    /// <summary>
    /// Assigne uniquement la classe
    /// </summary>
    public void AssignClass(SwordStats swordStats)
    {
        string className = PickRandomClass();
        swordStats.SetSwordClass(className);
    }

    /// <summary>
    /// Assigne uniquement la rareté
    /// </summary>
    public void AssignRarity(SwordStats swordStats)
    {
        string rarityName = PickRandomRarity();
        swordStats.SetRarity(rarityName);
    }

    /// <summary>
    /// Assigne le niveau de l'épée en fonction du niveau du joueur
    /// </summary>
    public void AssignLevel(SwordStats swordStats, PlayerLevel playerLevel)
    {
        int swordLevel = playerLevel != null ? playerLevel.GetCurrentLevel() : 1;
        swordStats.SetLevel(swordLevel);
    }

    /// <summary>
    /// Assigne 1-3 enchantements aléatoires
    /// </summary>
    public void AssignEnchantments(SwordStats swordStats)
    {
        if (enchantmentConfig == null)
        {
            Debug.LogWarning("SwordAssigner: EnchantmentConfig is missing!", this);
            swordStats.SetEnchantments(new List<Enchantment>());
            return;
        }

        int enchantCount = enchantmentConfig.PickEnchantCount();
        List<Enchantment> enchantments = new List<Enchantment>();
        List<string> usedTypes = new List<string>();

        for (int i = 0; i < enchantCount; i++)
        {
            // Choisir un type d'enchant pas encore utilisé
            string enchantType = PickRandomEnchantType(usedTypes);
            if (string.IsNullOrEmpty(enchantType))
                break;

            usedTypes.Add(enchantType);

            // Choisir un niveau aléatoire pour cet enchant
            int level = enchantmentConfig.PickRandomLevel();

            enchantments.Add(new Enchantment
            {
                type = enchantType,
                level = level
            });
        }

        swordStats.SetEnchantments(enchantments);
    }

    private string PickRandomEnchantType(List<string> excludedTypes)
    {
        if (enchantmentConfig == null || enchantmentConfig.enchantmentTypes == null)
            return null;

        List<string> availableTypes = new List<string>();
        foreach (var type in enchantmentConfig.enchantmentTypes)
        {
            if (!excludedTypes.Contains(type))
                availableTypes.Add(type);
        }

        if (availableTypes.Count == 0)
            return null;

        return availableTypes[Random.Range(0, availableTypes.Count)];
    }

    private string PickRandomMold()
    {
        return PickRandomAttributeWithUpgrade(attributesConfig?.moldOptions, UpgradeSystem.Instance?.Molder) ?? string.Empty;
    }

    private string PickRandomQuality()
    {
        return PickRandomAttributeWithUpgrade(attributesConfig?.qualityOptions, UpgradeSystem.Instance?.Quality) ?? string.Empty;
    }

    private string PickRandomClass()
    {
        return PickRandomAttributeWithUpgrade(attributesConfig?.classOptions, UpgradeSystem.Instance?.SwordClass) ?? string.Empty;
    }

    private string PickRandomRarity()
    {
        return PickRandomAttributeWithUpgrade(attributesConfig?.rarityOptions, UpgradeSystem.Instance?.Rarity) ?? string.Empty;
    }

    /// <summary>
    /// Méthode générique pour choisir un attribut aléatoire pondéré avec bonus d'upgrade
    /// Utilise le système de fallback continu : test en cascade du plus rare au plus commun
    /// </summary>
    private string PickRandomAttributeWithUpgrade(SwordAttributesConfig.AttributeOption[] options, UpgradeSystem.UpgradeCategory category)
    {
        if (options == null || options.Length == 0)
            return string.Empty;

        int upgradeLevel = category != null ? category.currentLevel : 0;

        // Créer une copie des options pour ne pas modifier les originales
        SwordAttributesConfig.AttributeOption[] modifiedOptions = new SwordAttributesConfig.AttributeOption[options.Length];
        System.Array.Copy(options, modifiedOptions, options.Length);

        // Appliquer le bonus de luck avec la nouvelle logique
        if (upgradeLevel > 0 && UpgradeSystem.Instance != null && category != null)
        {
            UpgradeSystem.Instance.ApplyLuckBonus(modifiedOptions, upgradeLevel, category);
        }

        // TEST EN CASCADE : partir du plus rare au plus commun
        var option = PickRandomAttributeCascade(modifiedOptions, category);
        
        return option.name;
    }

    /// <summary>
    /// Test en cascade : du plus rare au plus commun, le dernier non-retiré est le fallback
    /// Utilise directement les odds (1/X) stockées dans weight
    /// </summary>
    private SwordAttributesConfig.AttributeOption PickRandomAttributeCascade(SwordAttributesConfig.AttributeOption[] options, UpgradeSystem.UpgradeCategory category)
    {
        if (options == null || options.Length == 0)
            return options[0];

        // Créer une liste triée par odds croissantes (du plus rare au plus commun)
        var sortedByRarity = new System.Collections.Generic.List<SwordAttributesConfig.AttributeOption>();
        foreach (var opt in options)
        {
            if (category == null || !category.removedOptions.Contains(opt.name))
            {
                sortedByRarity.Add(opt);
            }
        }

        if (sortedByRarity.Count == 0)
            return options[0];

        // Trier : odds élevées (rare) d'abord
        sortedByRarity.Sort((a, b) => b.weight.CompareTo(a.weight));

        // Tester en cascade du plus rare au plus commun
        foreach (var option in sortedByRarity)
        {
            // weight contient maintenant les odds (1/X)
            float odds = option.weight;
            float probability = 1f / odds;
            
            // Test : roll un nombre entre 0 et 1
            if (Random.value < probability)
            {
                return option;
            }
        }

        // Fallback : le plus commun (dernier de la liste triée)
        return sortedByRarity[sortedByRarity.Count - 1];
    }

    private void LogWeights(SwordAttributesConfig.AttributeOption[] options, string label)
    {
        float total = 0f;
        foreach (var opt in options)
            total += opt.weight;

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine($"--- {label} WEIGHTS (Total: {total:F6}) ---");
        
        // Afficher les 5 plus rares
        int startIndex = Mathf.Max(0, options.Length - 5);
        for (int i = startIndex; i < options.Length; i++)
        {
            float percent = (options[i].weight / total) * 100f;
            float odds = 1f / (options[i].weight / total);
            sb.AppendLine($"  {options[i].name}: {percent:F4}% (1/{odds:F1})");
        }
        
        Debug.Log(sb.ToString());
    }
}
