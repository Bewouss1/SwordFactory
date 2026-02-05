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

        // Configurer les baseOdds pour les qualities (33 niveaux, progression ×3)
        if (config.qualityOptions != null && config.qualityOptions.Length > 0)
        {
            float[] qualityOddsProgression = {
                1f, 3f, 9f, 27f, 81f, 243f, 729f, 2187f, 6561f, 19683f,
                59049f, 177147f, 531441f, 1594323f, 4782969f, 14348907f, 43046721f, 129140163f, 387420489f, 1162261467f,
                3486784401f, 10460353203f, 31381059609f, 94143178827f, 282429536481f, 847288609443f, 2541865828329f, 7625597484987f, 22876792454961f, 68630377364883f,
                205891132094649f, 617673396283947f, 1853020188851841f
            };

            for (int i = 0; i < config.qualityOptions.Length && i < qualityOddsProgression.Length; i++)
            {
                config.qualityOptions[i].baseOdds = qualityOddsProgression[i];
            }
        }

        // Configurer les baseOdds pour les classes (26 niveaux, progression ×4)
        if (config.classOptions != null && config.classOptions.Length > 0)
        {
            float[] classOddsProgression = {
                1f, 4f, 16f, 64f, 256f, 1024f, 4096f, 16384f, 65536f, 262144f,
                1048576f, 4194304f, 16777216f, 67108864f, 268435456f, 1073741824f, 4294967296f, 17179869184f, 68719476736f, 274877906944f,
                1099511627776f, 4398046511104f, 17592186044416f, 70368744177664f, 281474976710656f, 1125899906842624f
            };

            for (int i = 0; i < config.classOptions.Length && i < classOddsProgression.Length; i++)
            {
                config.classOptions[i].baseOdds = classOddsProgression[i];
            }
        }

        // Configurer les baseOdds pour les rarities (84 niveaux)
        if (config.rarityOptions != null && config.rarityOptions.Length > 0)
        {
            float[] rarityOddsProgression = {
                1f, 1.33f, 1.77f, 2.37f, 3.16f, 4.21f, 5.62f, 7.49f, 10f, 13.33f,
                17.78f, 23.71f, 31.62f, 42.16f, 56.23f, 74.98f, 100f, 133.35f, 177.82f, 237.13f,
                316.22f, 421.69f, 562.34f, 749.89f, 1000f, 1334f, 1778f, 2371f, 3162f, 4217f,
                5623f, 7499f, 10000f, 13335f, 17783f, 23714f, 31623f, 42170f, 56234f, 74989f,
                100000f, 133352f, 177828f, 237137f, 316228f, 421697f, 562341f, 749894f, 1000000f, 1333000f,
                1778000f, 2371000f, 3162000f, 4216000f, 5623000f, 7498000f, 10000000f, 13330000f, 17780000f, 23710000f,
                31620000f, 42160000f, 56230000f, 74980000f, 100000000f, 200000000f, 400000000f, 800000000f, 1600000000f, 3200000000f,
                6400000000f, 12800000000f, 25600000000f, 51200000000f, 102400000000f, 204800000000f, 409600000000f, 819200000000f, 
                1638000000000f, 3276000000000f, 6553000000000f, 13100000000000f, 26210000000000f, 52420000000000f, 104800000000000f, 
                209700000000000f, 419400000000000f, 838800000000000f, 1670000000000000f, 3350000000000000f, 6710000000000000f, 
                13420000000000000f, 26840000000000000f
            };

            for (int i = 0; i < config.rarityOptions.Length && i < rarityOddsProgression.Length; i++)
            {
                config.rarityOptions[i].baseOdds = rarityOddsProgression[i];
            }
        }

        // Marquer comme modifié et sauvegarder
        EditorUtility.SetDirty(config);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"✅ Base Odds configured successfully!");
        Debug.Log($"   Molds: {config.moldOptions.Length}");
        Debug.Log($"   Qualities: {config.qualityOptions.Length}");
        Debug.Log($"   Classes: {config.classOptions.Length}");
        Debug.Log($"   Rarities: {config.rarityOptions.Length}");
    }
}
