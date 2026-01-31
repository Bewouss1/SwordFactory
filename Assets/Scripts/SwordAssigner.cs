using UnityEngine;

/// <summary>
/// Gère l'assignement de tous les attributs d'une épée (moule, qualité, classe, rareté, etc.)
/// Responsabilité unique : configurateur d'épée
/// </summary>
public class SwordAssigner : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private SwordAttributesConfig attributesConfig;

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
    /// Assigne un enchantement aléatoire
    /// </summary>
    public void AssignEnchant(SwordStats swordStats)
    {
        string enchantName = PickRandomEnchant();
        swordStats.SetEnchant(enchantName);
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

    private string PickRandomEnchant()
    {
        return PickRandomAttribute(attributesConfig?.enchantOptions) ?? string.Empty;
    }

    /// <summary>
    /// Méthode générique pour choisir un attribut aléatoire pondéré
    /// </summary>
    private string PickRandomAttribute(SwordAttributesConfig.AttributeOption[] options)
    {
        if (options == null || options.Length == 0)
            return null;

        var option = PickRandomFromWeightedArray(options, opt => opt.weight);
        return option.name;
    }

    /// <summary>
    /// Fonction générique pour choisir un élément aléatoire pondéré
    /// </summary>
    private T PickRandomFromWeightedArray<T>(T[] options, System.Func<T, float> getWeight)
    {
        if (options == null || options.Length == 0)
        {
            Debug.LogError("SwordAssigner: Options array is empty!");
            return default;
        }

        float totalWeight = 0f;
        foreach (var option in options)
            totalWeight += Mathf.Max(0f, getWeight(option));

        if (totalWeight <= 0f)
            return options[0];

        float roll = Random.value * totalWeight;
        foreach (var option in options)
        {
            float w = Mathf.Max(0f, getWeight(option));
            if (roll <= w)
                return option;

            roll -= w;
        }

        return options[options.Length - 1];
    }
}
