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
        return PickRandomAttribute(attributesConfig?.moldOptions) ?? string.Empty;
    }

    private string PickRandomQuality()
    {
        return PickRandomAttribute(attributesConfig?.qualityOptions) ?? string.Empty;
    }

    private string PickRandomClass()
    {
        return PickRandomAttribute(attributesConfig?.classOptions) ?? string.Empty;
    }

    private string PickRandomRarity()
    {
        return PickRandomAttribute(attributesConfig?.rarityOptions) ?? string.Empty;
    }

    /// <summary>
    /// Méthode générique pour choisir un attribut aléatoire pondéré
    /// Utilise ProbabilityHelper pour une logique centralisée
    /// </summary>
    private string PickRandomAttribute(SwordAttributesConfig.AttributeOption[] options)
    {
        if (options == null || options.Length == 0)
            return null;

        var option = ProbabilityHelper.PickRandomWeighted(options, opt => opt.weight);
        
        // DEBUG: Décommenter pour diagnostiquer les probabilités
        // Debug.Log(ProbabilityHelper.GenerateWeightReport(options, opt => opt.weight, opt => opt.name));
        
        return option.name;
    }
}
