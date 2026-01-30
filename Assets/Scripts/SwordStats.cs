using UnityEngine;
using TMPro;

/// <summary>
/// Stocke tous les attributs/statistiques d'une épée et gère leur affichage
/// Fusionne l'ancien SwordMold + nouvelles stats
/// </summary>
public class SwordStats : MonoBehaviour
{
    [SerializeField] private string mold = "";         // wood, stone, iron, etc.
    [SerializeField] private string quality = "";      // broken, good, excellent, etc.
    [SerializeField] private string swordClass = "";   // regular, strong, powerful, etc.
    [SerializeField] private string rarity = "";       // common, rare, epic, etc.
    [SerializeField] private int level = 1;            // niveau de l'épée
    [SerializeField] private string enchant = "";      // enchantement

    [Header("Text Display")]
    [SerializeField] private TMP_Text moldText;           // Pour afficher le moule
    [SerializeField] private TMP_Text classText;          // Pour afficher la classe
    [SerializeField] private TMP_Text rarityQualityText;  // "Rarity / Quality"
    [SerializeField] private TMP_Text levelText;          // Pour afficher le niveau
    [SerializeField] private TMP_Text enchantText;        // Pour afficher l'enchantement
    [SerializeField] private TMP_Text moneyText;          // Valeur de l'épée
    [SerializeField] private TMP_Text timeText;           // Compte à rebours de vente

    [Header("Value Settings")]
    [SerializeField] private float baseValue = 10f;
    [SerializeField] private SwordAttributesConfig attributesConfig;

    public string Mold => mold;
    public string Quality => quality;
    public string SwordClass => swordClass;
    public string Rarity => rarity;
    public int Level => level;
    public string Enchant => enchant;
    public TMP_Text TimeText => timeText;

    public float GetValue()
    {
        return CalculateValue();
    }

    public string GetFormattedValue()
    {
        return FormatMoneyValue(CalculateValue());
    }

    public void SetMold(string value)
    {
        mold = value;
        UpdateDisplay();
    }

    public void SetQuality(string value)
    {
        quality = value;
        UpdateDisplay();
    }

    public void SetSwordClass(string value)
    {
        swordClass = value;
        UpdateDisplay();
    }

    public void SetRarity(string value)
    {
        rarity = value;
        UpdateDisplay();
    }

    public void SetLevel(int value)
    {
        level = value;
        UpdateDisplay();
    }

    public void SetEnchant(string value)
    {
        enchant = value;
        UpdateDisplay();
    }

    void OnEnable()
    {
        UpdateDisplay();
    }

    /// <summary>
    /// Affiche un résumé des stats de l'épée
    /// </summary>
    public string GetSummary()
    {
        return $"[{rarity.ToUpper()}] {mold} Lvl{level} {enchant} | Quality: {quality} | Class: {swordClass}";
    }

    /// <summary>
    /// Met à jour tous les TextMeshPro si configurés
    /// </summary>
    private void UpdateDisplay()
    {
        if (moldText != null)
            moldText.text = mold;

        if (classText != null)
            classText.text = swordClass;

        // Mettre à jour le texte combiné Rarity / Quality
        if (rarityQualityText != null)
        {
            string combined = $"{rarity} / {quality}";
            rarityQualityText.text = combined;
        }

        if (levelText != null)
            levelText.text = "Level " + level;

        if (enchantText != null)
            enchantText.text = enchant;

        UpdateMoneyDisplay();
    }

    private void UpdateMoneyDisplay()
    {
        if (moneyText == null)
            return;

        float value = CalculateValue();
        moneyText.text = FormatMoneyValue(value);
    }

    private float CalculateValue()
    {
        if (attributesConfig == null)
            return baseValue;

        float moldMult = GetMultiplier(mold, attributesConfig.moldOptions);
        float qualityMult = GetMultiplier(quality, attributesConfig.qualityOptions);
        float classMult = GetMultiplier(swordClass, attributesConfig.classOptions);
        float rarityMult = GetMultiplier(rarity, attributesConfig.rarityOptions);
        float enchantMult = GetMultiplier(enchant, attributesConfig.enchantOptions);
        float levelMult = 1f + (level * 0.01f);

        return Mathf.Max(0f, baseValue * moldMult * qualityMult * classMult * rarityMult * enchantMult * levelMult);
    }

    private float GetMultiplier(string key, SwordAttributesConfig.AttributeOption[] options)
    {
        if (options == null || options.Length == 0)
            return 1f;

        for (int i = 0; i < options.Length; i++)
        {
            if (string.Equals(options[i].name, key, System.StringComparison.OrdinalIgnoreCase))
            {
                return Mathf.Max(0f, options[i].multiplier);
            }
        }

        return 1f;
    }

    public static string FormatMoneyValue(float value)
    {
        string[] suffixes = { "", "k", "M", "B", "T", "Qd", "Qn" };
        int suffixIndex = 0;
        float displayValue = value;

        while (displayValue >= 1000f && suffixIndex < suffixes.Length - 1)
        {
            displayValue /= 1000f;
            suffixIndex++;
        }

        string formatted = displayValue.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
        return $"${formatted}{suffixes[suffixIndex]}";
    }
}