using UnityEngine;
using UnityEditor;
using System.Text;

public class DebugOdds : EditorWindow
{
    [MenuItem("Tools/Debug - Show All Odds")]
    public static void ShowAllOdds()
    {
        // Trouver les configs
        string[] swordConfigGuids = AssetDatabase.FindAssets("t:SwordAttributesConfig");
        string[] enchantConfigGuids = AssetDatabase.FindAssets("t:EnchantmentConfig");

        if (swordConfigGuids.Length == 0 || enchantConfigGuids.Length == 0)
        {
            Debug.LogError("Could not find SwordAttributesConfig or EnchantmentConfig!");
            return;
        }

        SwordAttributesConfig swordConfig = AssetDatabase.LoadAssetAtPath<SwordAttributesConfig>(
            AssetDatabase.GUIDToAssetPath(swordConfigGuids[0]));
        EnchantmentConfig enchantConfig = AssetDatabase.LoadAssetAtPath<EnchantmentConfig>(
            AssetDatabase.GUIDToAssetPath(enchantConfigGuids[0]));

        if (swordConfig == null || enchantConfig == null)
        {
            Debug.LogError("Could not load configs!");
            return;
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("\n========== ALL ODDS DEBUG ==========\n");

        // Molds
        sb.AppendLine("=== MOLDS ===");
        PrintOdds(swordConfig.moldOptions, sb);

        // Qualities
        sb.AppendLine("\n=== QUALITIES ===");
        PrintOdds(swordConfig.qualityOptions, sb);

        // Classes
        sb.AppendLine("\n=== CLASSES ===");
        PrintOdds(swordConfig.classOptions, sb);

        // Rarities
        sb.AppendLine("\n=== RARITIES ===");
        PrintOdds(swordConfig.rarityOptions, sb);

        // Enchantments
        sb.AppendLine("\n=== ENCHANTMENTS ===");
        PrintEnchantmentOdds(enchantConfig.levels, sb);

        sb.AppendLine("========== END DEBUG ==========\n");
        Debug.Log(sb.ToString());
    }

    private static void PrintOdds(SwordAttributesConfig.AttributeOption[] options, StringBuilder sb)
    {
        if (options == null || options.Length == 0)
        {
            sb.AppendLine("  (Empty)");
            return;
        }

        // Calculer le total des odds
        float totalOdds = 0f;
        foreach (var opt in options)
        {
            totalOdds += 1f / opt.baseOdds;
        }

        foreach (var opt in options)
        {
            float probability = (1f / opt.baseOdds) / totalOdds * 100f;
            sb.AppendLine($"  {opt.name,-15} baseOdds: 1/{opt.baseOdds,-15:F2} | Probability: {probability:F4}%");
        }
    }

    private static void PrintEnchantmentOdds(EnchantmentLevel[] levels, StringBuilder sb)
    {
        if (levels == null || levels.Length == 0)
        {
            sb.AppendLine("  (Empty)");
            return;
        }

        // Calculer le total des odds
        float totalOdds = 0f;
        foreach (var lvl in levels)
        {
            totalOdds += 1f / lvl.weight;
        }

        foreach (var lvl in levels)
        {
            float probability = (1f / lvl.weight) / totalOdds * 100f;
            string displayName = $"Enchant {Enchantment.ToRomanNumeral(lvl.level)}";
            sb.AppendLine($"  {displayName,-15} weight: 1/{lvl.weight,-15:F2} | Probability: {probability:F4}%");
        }
    }
}
