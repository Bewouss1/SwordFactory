using UnityEngine;

[CreateAssetMenu(menuName = "SwordFactory/Sword Attributes Config", fileName = "SwordAttributesConfig")]
public class SwordAttributesConfig : ScriptableObject
{
    [System.Serializable]
    public struct AttributeOption
    {
        public string name;      // wood, stone, epic, etc.
        public float weight;     // pour le tirage aléatoire
        public float multiplier; // pour la valeur
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
}