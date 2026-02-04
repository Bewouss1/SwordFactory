using UnityEngine;
using TMPro;
using System.Collections.Generic;

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
    [SerializeField] private List<Enchantment> enchantments = new List<Enchantment>();  // Liste d'enchantements

    [Header("Text Display")]
    [SerializeField] private TMP_Text moldText;           // Pour afficher le moule
    [SerializeField] private TMP_Text classText;          // Pour afficher la classe
    [SerializeField] private TMP_Text rarityQualityText;  // "Rarity / Quality"
    [SerializeField] private TMP_Text levelText;          // Pour afficher le niveau
    [SerializeField] private TMP_Text enchantText;        // Pour afficher l'enchantement
    [SerializeField] private TMP_Text moneyText;          // Valeur de l'épée
    [SerializeField] private TMP_Text timeText;           // Compte à rebours de vente

    [Header("Container")]
    [SerializeField] private MeshRenderer containerRenderer;  // Container (cube transparent) pour changer sa couleur

    [Header("Value Settings")]
    [SerializeField] private float baseValue = 10f;
    [SerializeField] private SwordAttributesConfig attributesConfig;
    [SerializeField] private EnchantmentConfig enchantmentConfig;

    [Header("Default Colors")]
    [SerializeField] private Color defaultColor = Color.white; // Couleur par défaut quand pas de correspondance

    public string Mold => mold;
    public string Quality => quality;
    public string SwordClass => swordClass;
    public string Rarity => rarity;
    public int Level => level;
    public List<Enchantment> Enchantments => enchantments;
    public TMP_Text TimeText => timeText;

    public float GetValue() => CalculateValue();
    public string GetFormattedValue() => FormatMoneyValue(CalculateValue());

    public void SetMold(string value) => UpdateAttribute(ref mold, value);
    public void SetQuality(string value) => UpdateAttribute(ref quality, value);
    public void SetSwordClass(string value) => UpdateAttribute(ref swordClass, value);
    public void SetRarity(string value) => UpdateAttribute(ref rarity, value);
    public void SetEnchantments(List<Enchantment> value)
    {
        enchantments = value ?? new List<Enchantment>();
        UpdateDisplay();
    }
    public void SetLevel(int value)
    {
        level = value;
        UpdateDisplay();
    }

    private void UpdateAttribute(ref string attribute, string value)
    {
        attribute = value;
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
        string enchantsStr = enchantments != null && enchantments.Count > 0 
            ? string.Join(", ", enchantments.ConvertAll(e => e.GetDisplayName()))
            : "None";
        return $"[{rarity.ToUpper()}] {mold} Lvl{level} [{enchantsStr}] | Quality: {quality} | Class: {swordClass}";
    }

    /// <summary>
    /// Met à jour tous les TextMeshPro si configurés
    /// </summary>
    private void UpdateDisplay()
    {
        // Couleurs pour les textes - toujours blanc ou couleur de l'attribut assigné
        if (moldText != null)
        {
            moldText.text = mold;
            moldText.color = IsAttributeEmpty(mold) ? Color.white : GetColorForAttribute(mold, attributesConfig?.moldOptions);
        }

        if (classText != null)
        {
            classText.text = swordClass;
            classText.color = IsAttributeEmpty(swordClass) ? Color.white : GetColorForAttribute(swordClass, attributesConfig?.classOptions);
        }

        // Calculer la couleur de rareté (utilisée pour le texte ET le container)
        Color rarityColor = IsAttributeEmpty(rarity) ? Color.white : GetColorForAttribute(rarity, attributesConfig?.rarityOptions);

        // Mettre à jour le texte combiné Rarity / Quality
        if (rarityQualityText != null)
        {
            string combined = $"{rarity} / {quality}";
            rarityQualityText.text = combined;
            rarityQualityText.color = rarityColor;
        }

        if (levelText != null)
            levelText.text = "Level " + level;

        if (enchantText != null)
        {
            if (enchantments != null && enchantments.Count > 0)
            {
                string enchantDisplay = string.Join("\n", enchantments.ConvertAll(e => e.GetDisplayName()));
                enchantText.text = enchantDisplay;
            }
            else
            {
                enchantText.text = "";
            }
            
            if (attributesConfig != null)
                enchantText.color = attributesConfig.enchantTextColor;
        }

        // moneyText: blanc si mold vide, sinon couleur du mold
        if (moneyText != null)
            moneyText.color = IsAttributeEmpty(mold) ? Color.white : GetColorForAttribute(mold, attributesConfig?.moldOptions);

        // Container: defaultColor tant que rareté non assignée, puis couleur de la rareté
        if (containerRenderer != null)
        {
            Material mat = containerRenderer.material;
            if (mat != null)
            {
                Color containerColor = IsAttributeEmpty(rarity) ? defaultColor : rarityColor;
                // Debug: Décommenter pour diagnostiquer les couleurs
                // Debug.Log($"[SwordStats] Rarity: '{rarity}' | ContainerColor: {containerColor}");
                containerColor.a = mat.color.a; // Conserver la transparence
                mat.color = containerColor;
            }
        }

        UpdateMoneyDisplay();
    }

    /// <summary>
    /// Vérifie si un attribut est considéré comme vide (null, vide, ou "Unknown")
    /// </summary>
    private bool IsAttributeEmpty(string attribute)
    {
        return string.IsNullOrEmpty(attribute) || attribute.Equals("Unknown", System.StringComparison.OrdinalIgnoreCase);
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
        
        // Calcul du multiplicateur d'enchantement (produit de tous les enchants)
        float enchantMult = 1f;
        if (enchantments != null && enchantmentConfig != null)
        {
            foreach (var ench in enchantments)
            {
                var levelData = enchantmentConfig.GetLevelData(ench.level);
                enchantMult *= levelData.valueMultiplier;
            }
        }
        
        float levelMult = 1f + (Mathf.Max(0, level - 1) * GameConstants.LEVEL_MULTIPLIER_PER_LEVEL);

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

    /// <summary>
    /// Formate une valeur monétaire avec suffixes (k, M, B, T, Qd, Qn)
    /// Méthode statique publique réutilisable dans tout le projet
    /// </summary>
    public static string FormatMoneyValue(float value)
    {
        if (value < 0f)
            return "$0.00";

        int suffixIndex = 0;
        float displayValue = value;

        while (displayValue >= 1000f && suffixIndex < GameConstants.MONEY_SUFFIXES.Length - 1)
        {
            displayValue /= 1000f;
            suffixIndex++;
        }

        string formatted = displayValue.ToString(GameConstants.MONEY_FORMAT, System.Globalization.CultureInfo.InvariantCulture);
        return $"${formatted}{GameConstants.MONEY_SUFFIXES[suffixIndex]}";
    }

    /// <summary>
    /// Récupère la couleur associée à un attribut
    /// ATTENTION: Cette méthode retourne Color.white si aucune correspondance,
    /// car elle est utilisée pour les TEXTES uniquement.
    /// Pour le container, utiliser defaultColor directement dans UpdateDisplay().
    /// </summary>
    private Color GetColorForAttribute(string attributeName, SwordAttributesConfig.AttributeOption[] options)
    {
        if (options == null || options.Length == 0 || string.IsNullOrEmpty(attributeName))
            return Color.white;

        for (int i = 0; i < options.Length; i++)
        {
            if (string.Equals(options[i].name, attributeName, System.StringComparison.OrdinalIgnoreCase))
            {
                Color color = options[i].color;
                color.a = 1f; // Force l'opacité à 1 pour que le texte soit visible
                return color;
            }
        }

        return Color.white;
    }
}