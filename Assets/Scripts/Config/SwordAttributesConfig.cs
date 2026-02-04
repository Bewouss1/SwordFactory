using UnityEngine;

[CreateAssetMenu(menuName = "SwordFactory/Sword Attributes Config", fileName = "SwordAttributesConfig")]
public class SwordAttributesConfig : ScriptableObject
{
    [System.Serializable]
    public struct AttributeOption
    {
        [ReadOnly]
        public string name;      // wood, stone, epic, etc.
        
        [ReadOnly]
        public float weight;     // pour le tirage aléatoire (auto-calculé)
        
        [ReadOnly]
        public float multiplier; // pour la valeur (auto-calculé)
        
        [ReadOnly]
        public Color color;      // couleur associée à cet attribut
    }

    [Header("Mold Options")]
    public AttributeOption[] moldOptions;

    [Header("Quality Options")]
    public AttributeOption[] qualityOptions;

    [Header("Class Options")]
    public AttributeOption[] classOptions;

    [Header("Rarity Options")]
    public AttributeOption[] rarityOptions;

    [Header("Enchant Options")]
    public AttributeOption[] enchantOptions;

    [Header("Fixed Colors")]
    public Color enchantTextColor = new Color(1f, 0.84f, 0f); // Couleur dorée par défaut
}