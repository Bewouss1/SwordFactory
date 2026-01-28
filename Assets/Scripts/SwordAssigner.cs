using UnityEngine;

/// <summary>
/// Gère l'assignement de tous les attributs d'une épée (moule, qualité, classe, rareté, etc.)
/// Responsabilité unique : configurateur d'épée
/// </summary>
public class SwordAssigner : MonoBehaviour
{
    [System.Serializable]
    public struct MoldOption
    {
        public string name;         // wood, stone, iron, etc.
        public float weight;
    }

    [System.Serializable]
    public struct QualityOption
    {
        public string name;         // broken, good, excellent, etc.
        public float weight;
    }

    [System.Serializable]
    public struct ClassOption
    {
        public string name;         // regular, strong, powerful, etc.
        public float weight;
    }

    [System.Serializable]
    public struct RarityOption
    {
        public string name;         // common, rare, epic, etc.
        public float weight;
    }

    [Header("Mold Settings")]
    [SerializeField] private MoldOption[] moldOptions;

    [Header("Quality Settings")]
    [SerializeField] private QualityOption[] qualityOptions;

    [Header("Class Settings")]
    [SerializeField] private ClassOption[] classOptions;

    [Header("Rarity Settings")]
    [SerializeField] private RarityOption[] rarityOptions;

    void OnEnable()
    {
        ValidateOptions();
    }

    private void ValidateOptions()
    {
        if (moldOptions == null || moldOptions.Length == 0)
            Debug.LogWarning("SwordAssigner: Mold options are not configured!", this);

        if (qualityOptions == null || qualityOptions.Length == 0)
            Debug.LogWarning("SwordAssigner: Quality options are not configured!", this);

        if (classOptions == null || classOptions.Length == 0)
            Debug.LogWarning("SwordAssigner: Class options are not configured!", this);

        if (rarityOptions == null || rarityOptions.Length == 0)
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

    private string PickRandomMold()
    {
        var option = PickRandomFromWeightedArray(moldOptions, opt => opt.weight);
        return option.name;
    }

    private string PickRandomQuality()
    {
        var option = PickRandomFromWeightedArray(qualityOptions, opt => opt.weight);
        return option.name;
    }

    private string PickRandomClass()
    {
        var option = PickRandomFromWeightedArray(classOptions, opt => opt.weight);
        return option.name;
    }

    private string PickRandomRarity()
    {
        var option = PickRandomFromWeightedArray(rarityOptions, opt => opt.weight);
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
