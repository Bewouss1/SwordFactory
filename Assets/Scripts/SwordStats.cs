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

    [Header("Text Display")]
    [SerializeField] private TMP_Text moldText;           // Pour afficher le moule
    [SerializeField] private TMP_Text classText;          // Pour afficher la classe
    [SerializeField] private TMP_Text rarityQualityText;  // "Rarity / Quality"

    public string Mold => mold;
    public string Quality => quality;
    public string SwordClass => swordClass;
    public string Rarity => rarity;

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

    void OnEnable()
    {
        UpdateDisplay();
    }

    /// <summary>
    /// Affiche un résumé des stats de l'épée
    /// </summary>
    public string GetSummary()
    {
        return $"[{rarity.ToUpper()}] {mold} | Quality: {quality} | Class: {swordClass}";
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
    }
}