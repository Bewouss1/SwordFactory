using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Système d'enchantements : chaque épée peut avoir 1-3 enchants (Sharpness, Power, Resistance)
/// avec des niveaux I à XXXVI
/// </summary>
/// 
[System.Serializable]
public struct EnchantmentLevel
{
    public int level;                  // 1 = I, 2 = II, etc.
    public float weight;               // Probabilité (1/chance)
    public float valueMultiplier;      // Multiplicateur de valeur
    public float damageMultiplier;     // Multiplicateur de dégâts
    public float healthMultiplier;     // Multiplicateur de vie
}

[System.Serializable]
public struct Enchantment
{
    public string type;                // "Sharpness", "Power", "Resistance"
    public int level;                  // Niveau (1-36)
    
    public string GetDisplayName()
    {
        return $"{type} {ToRomanNumeral(level)}";
    }
    
    public static string ToRomanNumeral(int number)
    {
        if (number <= 0 || number > 3999)
            return number.ToString();
            
        string[] thousands = { "", "M", "MM", "MMM" };
        string[] hundreds = { "", "C", "CC", "CCC", "CD", "D", "DC", "DCC", "DCCC", "CM" };
        string[] tens = { "", "X", "XX", "XXX", "XL", "L", "LX", "LXX", "LXXX", "XC" };
        string[] ones = { "", "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX" };
        
        return thousands[number / 1000] +
               hundreds[(number % 1000) / 100] +
               tens[(number % 100) / 10] +
               ones[number % 10];
    }
}

[CreateAssetMenu(menuName = "SwordFactory/Enchantment Config", fileName = "EnchantmentConfig")]
public class EnchantmentConfig : ScriptableObject
{
    [Header("Enchantment Types")]
    public string[] enchantmentTypes = { "Sharpness", "Power", "Resistance" };
    
    [Header("Enchantment Levels (I to XXXVI)")]
    public EnchantmentLevel[] levels;
    
    [Header("Number of Enchants per Sword")]
    [Tooltip("Probabilité d'avoir exactement N enchants sur une épée")]
    public float weight1Enchant = 1f;    // 1 enchant
    public float weight2Enchants = 0.5f; // 2 enchants
    public float weight3Enchants = 0.25f; // 3 enchants
    
    /// <summary>
    /// Génère un niveau d'enchantement aléatoire pondéré
    /// </summary>
    public int PickRandomLevel()
    {
        if (levels == null || levels.Length == 0)
            return 1;
            
        float totalWeight = 0f;
        foreach (var level in levels)
            totalWeight += Mathf.Max(0f, level.weight);
            
        if (totalWeight <= 0f)
            return 1;
            
        float roll = Random.value * totalWeight;
        foreach (var level in levels)
        {
            float w = Mathf.Max(0f, level.weight);
            if (roll <= w)
                return level.level;
            roll -= w;
        }
        
        return levels[levels.Length - 1].level;
    }
    
    /// <summary>
    /// Détermine combien d'enchants cette épée devrait avoir (1-3)
    /// </summary>
    public int PickEnchantCount()
    {
        float total = weight1Enchant + weight2Enchants + weight3Enchants;
        float roll = Random.value * total;
        
        if (roll <= weight1Enchant)
            return 1;
        else if (roll <= weight1Enchant + weight2Enchants)
            return 2;
        else
            return 3;
    }
    
    /// <summary>
    /// Récupère les multiplicateurs pour un niveau donné
    /// </summary>
    public EnchantmentLevel GetLevelData(int level)
    {
        if (levels == null || levels.Length == 0)
            return new EnchantmentLevel { level = 1, valueMultiplier = 1f, damageMultiplier = 1f, healthMultiplier = 1f };
            
        foreach (var lvl in levels)
        {
            if (lvl.level == level)
                return lvl;
        }
        
        // Fallback
        return new EnchantmentLevel { level = level, valueMultiplier = 1f, damageMultiplier = 1f, healthMultiplier = 1f };
    }
}
