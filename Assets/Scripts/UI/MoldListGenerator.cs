using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

/// <summary>
/// Génère dynamiquement la liste des molds dans le ScrollView
/// Duplique un prefab pour chaque mold avec ses stats, couleurs et position Y
/// </summary>
public class MoldListGenerator : MonoBehaviour
{
    public enum ListType
    {
        Mold,
        Rarity,
        Class,
        Quality,
        EnchantLevel
    }

    [Header("References")]
    [SerializeField] private Transform contentContainer;  // Le Content du ScrollView
    [SerializeField] private GameObject moldItemPrefab;   // Prefab à dupliquer
    [SerializeField] private SwordAttributesConfig attributesConfig;
    [SerializeField] private EnchantmentConfig enchantmentConfig;
    
    private UpgradeSystem upgradeSystem;
    private int lastMolderLevel = -1;
    private int lastQualityLevel = -1;
    private int lastClassLevel = -1;
    private int lastRarityLevel = -1;
    
    [Header("Settings")]
    [SerializeField] private ListType listType = ListType.Mold;
    [SerializeField] private float startYPosition = 0f;        // Position Y du premier élément
    [SerializeField] private float yPositionOffset = -61.5f;  // Écart en Y entre chaque élément
    [SerializeField] private bool generateOnStart = true;

    private void Start()
    {
        if (upgradeSystem == null)
            upgradeSystem = UpgradeSystem.Instance;

        if (generateOnStart)
        {
            GenerateMoldList();
        }
        
        UpdateUpgradeLevels();
    }

    private void Update()
    {
        // Vérifier si un upgrade a changé
        if (upgradeSystem == null)
            return;

        bool levelChanged = false;
        
        if (listType == ListType.Mold && upgradeSystem.Molder.currentLevel != lastMolderLevel)
            levelChanged = true;
        else if (listType == ListType.Quality && upgradeSystem.Quality.currentLevel != lastQualityLevel)
            levelChanged = true;
        else if (listType == ListType.Class && upgradeSystem.SwordClass.currentLevel != lastClassLevel)
            levelChanged = true;
        else if (listType == ListType.Rarity && upgradeSystem.Rarity.currentLevel != lastRarityLevel)
            levelChanged = true;
        
        if (levelChanged)
        {
            GenerateMoldList();
            UpdateUpgradeLevels();
        }
    }

    private void UpdateUpgradeLevels()
    {
        if (upgradeSystem == null)
            return;
            
        lastMolderLevel = upgradeSystem.Molder.currentLevel;
        lastQualityLevel = upgradeSystem.Quality.currentLevel;
        lastClassLevel = upgradeSystem.SwordClass.currentLevel;
        lastRarityLevel = upgradeSystem.Rarity.currentLevel;
    }

    /// <summary>
    /// Génère la liste complète des molds dans l'ordre de rareté
    /// </summary>
    public void GenerateMoldList()
    {
        if (contentContainer == null)
        {
            Debug.LogError("MoldListGenerator: Content Container not assigned!");
            return;
        }

        if (moldItemPrefab == null)
        {
            Debug.LogError("MoldListGenerator: Mold Item Prefab not assigned!");
            return;
        }

        if (attributesConfig == null)
        {
            if (listType != ListType.EnchantLevel)
            {
                Debug.LogError("MoldListGenerator: Sword Attributes Config not assigned!");
                return;
            }
        }

        if (listType == ListType.EnchantLevel && enchantmentConfig == null)
        {
            Debug.LogError("MoldListGenerator: Enchantment Config not assigned!");
            return;
        }

        // Nettoyer les éléments existants (sauf le prefab s'il est dans le content)
        ClearExistingMolds();

        if (listType == ListType.EnchantLevel)
        {
            CreateEnchantmentList();
        }
        else
        {
            CreateAttributeList();
        }

        // Les éléments se positionneront automatiquement via le layout du ScrollView
    }

    /// <summary>
    /// Crée la liste des attributs avec les upgrades appliqués
    /// </summary>
    private void CreateAttributeList()
    {
        var options = GetAttributeOptionsForType();
        if (options == null || options.Length == 0)
            return;

        // Créer une copie et appliquer les upgrades
        var modifiedOptions = new SwordAttributesConfig.AttributeOption[options.Length];
        System.Array.Copy(options, modifiedOptions, options.Length);
        
        var category = GetUpgradeCategoryForListType();
        int upgradeLevel = category != null ? category.currentLevel : 0;
        
        // Initialiser les weights
        if (upgradeLevel > 0 && upgradeSystem != null && category != null)
        {
            UpgradeSystem.Instance.ApplyLuckBonus(modifiedOptions, upgradeLevel, category);
        }
        else
        {
            // Si pas d'upgrade, initialiser weight à baseOdds
            for (int i = 0; i < modifiedOptions.Length; i++)
            {
                modifiedOptions[i].weight = modifiedOptions[i].baseOdds;
            }
        }

        // Trier par baseOdds (pas les odds upgradées)
        var sorted = new SwordAttributesConfig.AttributeOption[modifiedOptions.Length];
        System.Array.Copy(modifiedOptions, sorted, modifiedOptions.Length);
        Array.Sort(sorted, (a, b) => a.baseOdds.CompareTo(b.baseOdds));

        for (int i = 0; i < sorted.Length; i++)
        {
            float displayOdds = sorted[i].weight;
            
            CreateListItem(
                sorted[i].name,
                true,
                sorted[i].color,
                sorted[i].multiplier,
                displayOdds,
                i,
                category != null && category.removedOptions.Contains(sorted[i].name)
            );
        }
    }

    private void CreateEnchantmentList()
    {
        if (enchantmentConfig == null || enchantmentConfig.levels == null || enchantmentConfig.levels.Length == 0)
            return;

        var sorted = new EnchantmentLevel[enchantmentConfig.levels.Length];
        Array.Copy(enchantmentConfig.levels, sorted, enchantmentConfig.levels.Length);
        
        Array.Sort(sorted, (a, b) => GetOddsFromWeight(a.weight).CompareTo(GetOddsFromWeight(b.weight)));

        for (int i = 0; i < sorted.Length; i++)
        {
            string displayName = $"Enchant {Enchantment.ToRomanNumeral(sorted[i].level)}";
            float odds = GetOddsFromWeight(sorted[i].weight);

            CreateListItem(
                displayName,
                true,  // Activer la couleur
                new Color(0.8f, 0f, 1f),  // Violet
                sorted[i].valueMultiplier,
                odds,
                i
            );
        }
    }

    /// <summary>
    /// Crée un élément de liste à la position spécifiée
    /// </summary>
    private void CreateListItem(string displayName, bool hasNameColor, Color nameColor, float valueMultiplier, float odds, int index, bool isRemoved = false)
    {
        // Dupliquer le prefab
        GameObject moldItem = Instantiate(moldItemPrefab, contentContainer);
        
        // Définir la position Y
        RectTransform rectTransform = moldItem.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            Vector3 newPosition = rectTransform.localPosition;
            newPosition.y = startYPosition + (yPositionOffset * index);
            rectTransform.localPosition = newPosition;
        }

        // Mettre à jour les texts
        var textComponents = moldItem.GetComponentsInChildren<TMP_Text>();
        TMP_Text oddsText = null;

        foreach (var text in textComponents)
        {
            switch (text.gameObject.name)
            {
                case "NameText":
                case "Name Text":
                case "MoldText":
                case "Mold Text":
                    text.text = displayName;
                    if (hasNameColor)
                        text.color = nameColor;
                    break;

                case "ValueText":
                case "Value Text":
                    // Multiplicateur de valeur - sans zéros inutiles et sans virg/point inutile
                    string formattedValue = valueMultiplier.ToString("F2", System.Globalization.CultureInfo.InvariantCulture).TrimEnd('0').TrimEnd('.');
                    text.text = $"{formattedValue} x Value Multiplier";
                    break;

                case "ProducedText":
                case "Produced Text":
                    // Nombre de molds produits (à implémenter)
                    text.text = "0 Produced Globally";
                    break;

                case "OddsText":
                case "Odds Text":
                    oddsText = text;
                    // Afficher les odds
                    if (isRemoved || odds <= 0f)
                    {
                        text.text = "1/0";
                    }
                    else
                    {
                        text.text = $"1/{SwordStats.FormatCompactNumber(odds)}";
                    }
                    break;
            }
        }

        // Activer l'élément
        if (!moldItem.activeSelf)
            moldItem.SetActive(true);
    }

    /// <summary>
    /// Nettoie les éléments de mold existants
    /// </summary>
    private void ClearExistingMolds()
    {
        if (contentContainer == null)
            return;

        // Détruire tous les enfants sauf le prefab lui-même
        int childCount = contentContainer.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            Transform child = contentContainer.GetChild(i);
            if (child.gameObject != moldItemPrefab)
            {
                Destroy(child.gameObject);
            }
        }
    }

    /// <summary>
    /// Régénère la liste (utile pour refresh après un upgrade)
    /// </summary>
    public void RefreshMoldList()
    {
        GenerateMoldList();
    }

    private SwordAttributesConfig.AttributeOption[] GetAttributeOptionsForType()
    {
        switch (listType)
        {
            case ListType.Mold:
                return attributesConfig.moldOptions;
            case ListType.Rarity:
                return attributesConfig.rarityOptions;
            case ListType.Class:
                return attributesConfig.classOptions;
            case ListType.Quality:
                return attributesConfig.qualityOptions;
            default:
                return attributesConfig.moldOptions;
        }
    }

    private UpgradeSystem.UpgradeCategory GetUpgradeCategoryForListType()
    {
        if (upgradeSystem == null)
            return null;
            
        switch (listType)
        {
            case ListType.Mold:
                return upgradeSystem.Molder;
            case ListType.Quality:
                return upgradeSystem.Quality;
            case ListType.Class:
                return upgradeSystem.SwordClass;
            case ListType.Rarity:
                return upgradeSystem.Rarity;
            default:
                return null;
        }
    }

    private float GetOddsFromWeight(float weight)
    {
        // weight = odds maintenant (après harmonisation du système)
        if (weight <= 0f)
            return 0f;

        return weight;
    }
}
