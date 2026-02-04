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

        Debug.Log($"[SwordAssigner] New sword created: {swordStats.GetSummary()}");
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
        return PickRandomAttributeWithUpgrade(attributesConfig?.moldOptions, UpgradeSystem.Instance?.Molder.currentLevel ?? 0) ?? string.Empty;
    }

    private string PickRandomQuality()
    {
        return PickRandomAttributeWithUpgrade(attributesConfig?.qualityOptions, UpgradeSystem.Instance?.Quality.currentLevel ?? 0) ?? string.Empty;
    }

    private string PickRandomClass()
    {
        return PickRandomAttributeWithUpgrade(attributesConfig?.classOptions, UpgradeSystem.Instance?.SwordClass.currentLevel ?? 0) ?? string.Empty;
    }

    private string PickRandomRarity()
    {
        return PickRandomAttributeWithUpgrade(attributesConfig?.rarityOptions, UpgradeSystem.Instance?.Rarity.currentLevel ?? 0) ?? string.Empty;
    }

    /// <summary>
    /// Méthode générique pour choisir un attribut aléatoire pondéré avec bonus d'upgrade
    /// </summary>
    private string PickRandomAttributeWithUpgrade(SwordAttributesConfig.AttributeOption[] options, int upgradeLevel)
    {
        if (options == null || options.Length == 0)
            return null;

        // Créer une copie des options pour ne pas modifier les originales
        SwordAttributesConfig.AttributeOption[] modifiedOptions = new SwordAttributesConfig.AttributeOption[options.Length];
        System.Array.Copy(options, modifiedOptions, options.Length);

        // DEBUG: Afficher les poids AVANT upgrade
        Debug.Log($"[SwordAssigner] === AVANT UPGRADE (Level {upgradeLevel}) ===");
        LogWeights(options, "ORIGINAL");

        // Appliquer le bonus de luck si un upgrade existe
        if (upgradeLevel > 0 && UpgradeSystem.Instance != null)
        {
            UpgradeSystem.Instance.ApplyLuckBonus(modifiedOptions, upgradeLevel);
            Debug.Log($"[SwordAssigner] === APRÈS UPGRADE (Level {upgradeLevel}) ===");
            LogWeights(modifiedOptions, "UPGRADED");
        }

        var option = ProbabilityHelper.PickRandomWeighted(modifiedOptions, opt => opt.weight);
        
        Debug.Log($"[SwordAssigner] ✅ SELECTED: {option.name} (upgrade level: {upgradeLevel})");
        
        return option.name;
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
