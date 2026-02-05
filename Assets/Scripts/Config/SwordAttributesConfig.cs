using UnityEngine;

[CreateAssetMenu(menuName = "SwordFactory/Sword Attributes Config", fileName = "SwordAttributesConfig")]
public class SwordAttributesConfig : ScriptableObject
{
    [System.Serializable]
    public struct AttributeOption
    {
        public string name;      // wood, stone, epic, etc.
        [Tooltip("Base odds (1/X) - Lower number = more common. Example: 1 = always, 5 = 1 in 5, 1000 = 1 in 1000")]
        public float baseOdds;   // Les odds de base (1/X) - plus facile à configurer
        public float multiplier; // pour la valeur
        public Color color;      // couleur associée à cet attribut
        
        [System.NonSerialized]
        public float weight;     // Calculé automatiquement au runtime
    }

    [Header("Mold Options")]
    public AttributeOption[] moldOptions;

    [Header("Quality Options")]
    public AttributeOption[] qualityOptions;

    [Header("Class Options")]
    public AttributeOption[] classOptions;

    [Header("Rarity Options")]
    public AttributeOption[] rarityOptions;

    [Header("Fixed Colors")]
    public Color enchantTextColor = new Color(1f, 0.84f, 0f); // Couleur dorée pour les enchantements
}