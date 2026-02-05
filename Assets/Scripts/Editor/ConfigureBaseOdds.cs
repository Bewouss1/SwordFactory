using UnityEngine;
using UnityEditor;

/// <summary>
/// Script Editor pour configurer automatiquement les baseOdds dans SwordAttributesConfig
/// Menu : Tools > Configure Base Odds
/// </summary>
public class ConfigureBaseOdds : EditorWindow
{
    [MenuItem("Tools/Configure Base Odds")]
    public static void ConfigureAllBaseOdds()
    {
        // Trouver le SwordAttributesConfig dans le projet
        string[] guids = AssetDatabase.FindAssets("t:SwordAttributesConfig");
        
        if (guids.Length == 0)
        {
            Debug.LogError("SwordAttributesConfig not found!");
            return;
        }

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        SwordAttributesConfig config = AssetDatabase.LoadAssetAtPath<SwordAttributesConfig>(path);

        if (config == null)
        {
            Debug.LogError("Could not load SwordAttributesConfig!");
            return;
        }

        // Configurer les baseOdds pour les molds
        if (config.moldOptions != null && config.moldOptions.Length > 0)
        {
            // Progression basée sur le tableau des molds
            float[] baseOddsProgression = { 
                1f,                    // Normal: 1/1
                10f,                   // Bronze: 1/10
                100f,                  // Silver: 1/100
                1000f,                 // Gold: 1/1000
                10000f,                // Sapphire: 1/10k
                100000f,               // Emerald: 1/100k
                1000000f,              // Ruby: 1/1M
                10000000f,             // Amethyst: 1/10M
                100000000f,            // Diamond: 1/100M
                1000000000f,           // Opal: 1/1B
                5000000000f,           // Uranium: 1/5B
                10000000000f,          // Crystal: 1/10B
                50000000000f,          // Moonstone: 1/50B
                100000000000f,         // Topaz: 1/100B
                1000000000000f,        // Painite: 1/1T
                1500000000000f,        // Anhydrite: 1/1.5T
                10000000000000f,       // Azure: 1/10T
                10000000000000f,       // Volcanic: 1/10T
                10500000000000f,       // Jade: 1/10.5T
                30000000000000f,       // Shale: 1/30T
                60000000000000f,       // Platinum: 1/60T
                100000000000000f,      // Quartz: 1/100T
                250000000000000f,      // Asgarite: 1/250T
                400000000000000f,      // Stardust: 1/400T
                500000000000000f,      // Zeolite: 1/500T
                1000000000000000f      // Ammolite: 1/1Qd
            };

            for (int i = 0; i < config.moldOptions.Length; i++)
            {
                if (i < baseOddsProgression.Length)
                {
                    config.moldOptions[i].baseOdds = baseOddsProgression[i];
                }
                else
                {
                    // Pour les éléments au-delà, multiplier par 10
                    config.moldOptions[i].baseOdds = baseOddsProgression[baseOddsProgression.Length - 1] * Mathf.Pow(10, i - baseOddsProgression.Length + 1);
                }
            }
        }

        // Marquer comme modifié et sauvegarder
        EditorUtility.SetDirty(config);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"✅ Base Odds configured successfully for {config.moldOptions.Length} molds!");
        
        // Afficher les valeurs
        Debug.Log("=== CONFIGURED BASE ODDS ===");
        foreach (var mold in config.moldOptions)
        {
            Debug.Log($"{mold.name}: 1/{mold.baseOdds}");
        }
    }
}
