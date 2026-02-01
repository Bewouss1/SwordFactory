using UnityEditor;
using UnityEngine;
using System.Globalization;

public class EnchantmentConfigGenerator : EditorWindow
{
    private EnchantmentConfig config;

    [MenuItem("Window/Sword Factory/Enchantment Config Generator")]
    public static void ShowWindow()
    {
        GetWindow<EnchantmentConfigGenerator>("Enchantment Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Enchantment Config Generator", EditorStyles.boldLabel);

        config = EditorGUILayout.ObjectField("Enchantment Config", config, typeof(EnchantmentConfig), false) as EnchantmentConfig;

        EditorGUILayout.Space();

        if (GUILayout.Button("Populate Enchantment Levels (I-XXXVI)", GUILayout.Height(30)))
        {
            if (config != null)
                PopulateEnchantmentLevels();
            else
                EditorUtility.DisplayDialog("Error", "Please assign an EnchantmentConfig asset", "OK");
        }

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "This will populate 36 enchantment levels with their chances and multipliers.",
            MessageType.Info);
    }

    private static double ParseChance(string chanceStr)
    {
        chanceStr = chanceStr.Replace(",", ".");
        chanceStr = chanceStr.Trim();

        double multiplier = 1.0;
        if (chanceStr.EndsWith("Qd", System.StringComparison.OrdinalIgnoreCase))
        {
            multiplier = 1e15;
            chanceStr = chanceStr.Substring(0, chanceStr.Length - 2).Trim();
        }
        else if (chanceStr.EndsWith("T", System.StringComparison.OrdinalIgnoreCase))
        {
            multiplier = 1e12;
            chanceStr = chanceStr.Substring(0, chanceStr.Length - 1).Trim();
        }
        else if (chanceStr.EndsWith("B", System.StringComparison.OrdinalIgnoreCase))
        {
            multiplier = 1e9;
            chanceStr = chanceStr.Substring(0, chanceStr.Length - 1).Trim();
        }
        else if (chanceStr.EndsWith("M", System.StringComparison.OrdinalIgnoreCase))
        {
            multiplier = 1e6;
            chanceStr = chanceStr.Substring(0, chanceStr.Length - 1).Trim();
        }
        else if (chanceStr.EndsWith("K", System.StringComparison.OrdinalIgnoreCase))
        {
            multiplier = 1e3;
            chanceStr = chanceStr.Substring(0, chanceStr.Length - 1).Trim();
        }

        if (double.TryParse(chanceStr, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
        {
            return value * multiplier;
        }

        Debug.LogError($"Could not parse chance: '{chanceStr}'");
        return 1.0;
    }

    private void PopulateEnchantmentLevels()
    {
        // Données: Level, Chance, Value Multi, Damage Multi, Health Multi
        var levelData = new (int level, string chance, float valueMult, float damageMult, float healthMult)[]
        {
            (1, "1", 1.01f, 1.44f, 1.1f),
            (2, "2.71", 1.02f, 1.96f, 1.2f),
            (3, "7.38", 1.03f, 2.56f, 1.3f),
            (4, "20.08", 1.04f, 3.24f, 1.4f),
            (5, "54.59", 1.05f, 4f, 1.5f),
            (6, "148.41", 1.06f, 4.84f, 1.6f),
            (7, "403.42", 1.07f, 5.76f, 1.7f),
            (8, "1097", 1.08f, 6.76f, 1.8f),
            (9, "2981", 1.09f, 7.84f, 1.9f),
            (10, "8103", 1.1f, 9f, 2f),
            (11, "22026", 1.11f, 10.24f, 2.1f),
            (12, "59874", 1.12f, 11.56f, 2.2f),
            (13, "162755", 1.13f, 12.96f, 2.3f),
            (14, "442413", 1.14f, 14.44f, 2.4f),
            (15, "1202604", 1.15f, 16f, 2.5f),
            (16, "3269017", 1.16f, 17.64f, 2.6f),
            (17, "8886111", 1.17f, 19.36f, 2.7f),
            (18, "24154953", 1.18f, 21.16f, 2.8f),
            (19, "65659969", 1.19f, 23.04f, 2.9f),
            (20, "178482301", 1.2f, 25f, 3f),
            (21, "485165195", 1.21f, 27.04f, 3.1f),
            (22, "1310000000", 1.22f, 29.16f, 3.2f),
            (23, "3580000000", 1.23f, 31.36f, 3.3f),
            (24, "9740000000", 1.24f, 33.64f, 3.4f),
            (25, "26480000000", 1.25f, 36f, 3.5f),
            (26, "72000000000", 1.26f, 38.44f, 3.6f),
            (27, "195700000000", 1.27f, 40.96f, 3.7f),
            (28, "532000000000", 1.28f, 43.56f, 3.8f),
            (29, "1440000000000", 1.29f, 46.24f, 3.9f),
            (30, "3930000000000", 1.3f, 49f, 4f),
            (31, "10680000000000", 1.31f, 51.84f, 4.1f),
            (32, "29040000000000", 1.32f, 54.76f, 4.2f),
            (33, "78960000000000", 1.33f, 57.76f, 4.3f),
            (34, "214600000000000", 1.34f, 60.84f, 4.4f),
            (35, "583400000000000", 1.35f, 64f, 4.5f),
            (36, "1580000000000000", 1.36f, 67.24f, 4.6f),
        };

        EnchantmentLevel[] levels = new EnchantmentLevel[levelData.Length];

        for (int i = 0; i < levelData.Length; i++)
        {
            var (level, chance, valueMult, damageMult, healthMult) = levelData[i];
            
            double denom = ParseChance(chance);
            float weight = (float)(1.0 / denom);

            levels[i] = new EnchantmentLevel
            {
                level = level,
                weight = weight,
                valueMultiplier = valueMult,
                damageMultiplier = damageMult,
                healthMultiplier = healthMult
            };
        }

        // Assigne les levels au config
        SerializedObject serializedConfig = new SerializedObject(config);
        SerializedProperty levelsProperty = serializedConfig.FindProperty("levels");
        
        levelsProperty.arraySize = levels.Length;
        for (int i = 0; i < levels.Length; i++)
        {
            SerializedProperty element = levelsProperty.GetArrayElementAtIndex(i);
            
            element.FindPropertyRelative("level").intValue = levels[i].level;
            element.FindPropertyRelative("weight").floatValue = levels[i].weight;
            element.FindPropertyRelative("valueMultiplier").floatValue = levels[i].valueMultiplier;
            element.FindPropertyRelative("damageMultiplier").floatValue = levels[i].damageMultiplier;
            element.FindPropertyRelative("healthMultiplier").floatValue = levels[i].healthMultiplier;
        }

        serializedConfig.ApplyModifiedProperties();
        AssetDatabase.SaveAssets();
        
        EditorUtility.DisplayDialog("Success", 
            $"Populated {levels.Length} enchantment levels successfully!", "OK");
    }
}
