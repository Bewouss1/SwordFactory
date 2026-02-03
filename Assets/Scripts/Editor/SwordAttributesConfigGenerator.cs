using UnityEditor;
using UnityEngine;

using System.Globalization;

public class SwordAttributesConfigGenerator : EditorWindow
{
    private SwordAttributesConfig config;
    private Vector2 scrollPosition;

    [MenuItem("Window/Sword Factory/Attributes Config Generator")]
    public static void ShowWindow()
    {
        GetWindow<SwordAttributesConfigGenerator>("Config Generator");
    }

    /// <summary>
    /// Parse une chance au format texte (ex: "1", "1.33", "200M", "1.6B", "6.553T", "1.85Qd")
    /// et retourne le dénominateur numérique
    /// </summary>
    private static double ParseChance(string chanceStr)
    {
        // Remplace les virgules par des points (locale FR)
        chanceStr = chanceStr.Replace(",", ".");
        chanceStr = chanceStr.Trim();

        // Vérifie les suffixes multiplicateurs
        double multiplier = 1.0;
        if (chanceStr.EndsWith("Qd", System.StringComparison.OrdinalIgnoreCase))
        {
            multiplier = 1e15; // Quadrillion
            chanceStr = chanceStr.Substring(0, chanceStr.Length - 2).Trim();
        }
        else if (chanceStr.EndsWith("T", System.StringComparison.OrdinalIgnoreCase))
        {
            multiplier = 1e12; // Trillion
            chanceStr = chanceStr.Substring(0, chanceStr.Length - 1).Trim();
        }
        else if (chanceStr.EndsWith("B", System.StringComparison.OrdinalIgnoreCase))
        {
            multiplier = 1e9; // Billion
            chanceStr = chanceStr.Substring(0, chanceStr.Length - 1).Trim();
        }
        else if (chanceStr.EndsWith("M", System.StringComparison.OrdinalIgnoreCase))
        {
            multiplier = 1e6; // Million
            chanceStr = chanceStr.Substring(0, chanceStr.Length - 1).Trim();
        }
        else if (chanceStr.EndsWith("K", System.StringComparison.OrdinalIgnoreCase))
        {
            multiplier = 1e3; // Thousand
            chanceStr = chanceStr.Substring(0, chanceStr.Length - 1).Trim();
        }

        if (double.TryParse(chanceStr, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
        {
            return value * multiplier;
        }

        Debug.LogError($"Could not parse chance: '{chanceStr}'");
        return 1.0;
    }

    private static Color ParseHexColor(string hex)
    {
        if (string.IsNullOrWhiteSpace(hex))
            return Color.white;

        if (!hex.StartsWith("#"))
            hex = "#" + hex;

        if (ColorUtility.TryParseHtmlString(hex, out var color))
            return color;

        return Color.white;
    }

    private static string GetRarityBaseName(string rarityName)
    {
        if (string.IsNullOrWhiteSpace(rarityName))
            return string.Empty;

        string name = rarityName.Trim();
        int end = name.Length - 1;

        while (end >= 0)
        {
            char c = name[end];
            if (c == '+' || c == '-' || char.IsDigit(c))
                end--;
            else
                break;
        }

        return end >= 0 ? name.Substring(0, end + 1) : string.Empty;
    }

    private static Color GetRarityColor(string rarityName)
    {
        string baseName = GetRarityBaseName(rarityName).ToLowerInvariant();

        switch (baseName)
        {
            case "basic": return ParseHexColor("555555");
            case "common": return ParseHexColor("999999");
            case "uncommon": return ParseHexColor("008000");
            case "rare": return ParseHexColor("0066FF");
            case "epic": return ParseHexColor("550099");
            case "legendary": return ParseHexColor("FFA500");
            case "mythical": return ParseHexColor("FF0000");
            case "divine": return ParseHexColor("CF00CF");
            case "super": return ParseHexColor("13FF00");
            case "mega": return ParseHexColor("FF0000");
            case "ultra": return ParseHexColor("FF4ADB");
            case "omega": return ParseHexColor("0000FF");
            case "extreme": return ParseHexColor("66FF66");
            case "ultimate": return ParseHexColor("FF99FF");
            case "insane": return ParseHexColor("292977");
            case "hyper": return ParseHexColor("000000");
            default: return Color.white;
        }
    }

    private static Color GetClassColor(string className)
    {
        string name = className.Trim().ToLowerInvariant();

        switch (name)
        {
            case "regular": return ParseHexColor("E6E6E6");
            case "nice": return ParseHexColor("808080");
            case "cool": return ParseHexColor("0000FF");
            case "decent": return ParseHexColor("0645AD");
            case "strong": return ParseHexColor("00FFFF");
            case "tough": return ParseHexColor("78FF78");
            case "solid": return ParseHexColor("00FF00");
            case "powerful": return ParseHexColor("FFFF00");
            case "crazy": return ParseHexColor("FFA500");
            case "unstoppable": return ParseHexColor("FF0000");
            case "impossible": return ParseHexColor("FF00FF");
            case "ethereal": return ParseHexColor("800080");
            case "unimaginable": return ParseHexColor("78FF78");
            case "outrageous": return ParseHexColor("99FF00");
            case "limitless": return ParseHexColor("FFA500");
            default: return Color.white;
        }
    }

    private void OnGUI()
    {
        GUILayout.Label("Sword Attributes Config Generator", EditorStyles.boldLabel);

        config = EditorGUILayout.ObjectField("Config Asset", config, typeof(SwordAttributesConfig), false) as SwordAttributesConfig;

        EditorGUILayout.Space();

        if (GUILayout.Button("Populate Molds from Preset", GUILayout.Height(30)))
        {
            if (config != null)
                PopulateMolds();
            else
                EditorUtility.DisplayDialog("Error", "Please assign a SwordAttributesConfig asset", "OK");
        }

        if (GUILayout.Button("Populate Qualities from Preset", GUILayout.Height(30)))
        {
            if (config != null)
                PopulateQualities();
            else
                EditorUtility.DisplayDialog("Error", "Please assign a SwordAttributesConfig asset", "OK");
        }

        if (GUILayout.Button("Populate Classes from Preset", GUILayout.Height(30)))
        {
            if (config != null)
                PopulateClasses();
            else
                EditorUtility.DisplayDialog("Error", "Please assign a SwordAttributesConfig asset", "OK");
        }

        if (GUILayout.Button("Populate Rarities from Preset", GUILayout.Height(30)))
        {
            if (config != null)
                PopulateRarities();
            else
                EditorUtility.DisplayDialog("Error", "Please assign a SwordAttributesConfig asset", "OK");
        }

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "This will populate the molds, qualities, classes, and rarities with predefined options.",
            MessageType.Info);
    }

    private void PopulateMolds()
    {
        // Données: Name, Chance (dénominateur), Multiplicateur
        var moldData = new (string name, string chance, float multiplier, Color color)[]
        {
            ("Normal", "1", 1f, ParseHexColor("00FF00")),                      // Gris
            ("Bronze", "10", 2.5f, ParseHexColor("cd7f32")),                   // Marron bronze
            ("Silver", "100", 6f, ParseHexColor("c0c0c0")),                    // Gris clair
            ("Gold", "1000", 15f, ParseHexColor("ffff00")),                    // Jaune or
            ("Sapphire", "10000", 35f, ParseHexColor("0000ff")),              // Bleu saphir
            ("Emerald", "100000", 60f, ParseHexColor("78ff78")),              // Vert émeraude
            ("Ruby", "1000000", 100f, ParseHexColor("ff0000")),               // Rouge rubis
            ("Amethyst", "10000000", 145f, ParseHexColor("800080")),          // Violet améthyste
            ("Diamond", "100000000", 190f, ParseHexColor("00ffff")),          // Blanc diamant
            ("Opal", "1000000000", 250f, ParseHexColor("000000")),            // Rose opal
            ("Uranium", "5000000000", 300f, ParseHexColor("66cc33")),         // Vert uranium
            ("Crystal", "10000000000", 350f, ParseHexColor("00b5e2")),        // Cyan cristal
            ("Moonstone", "50000000000", 420f, ParseHexColor("e6e6ff")),      // Blanc lune
            ("Topaz", "100000000000", 490f, ParseHexColor("9f512a")),         // Orange topaze
            ("Painite", "1000000000000", 700f, ParseHexColor("cc4d4d")),      // Rose-rouge
            ("Anhydrite", "1500000000000", 850f, ParseHexColor("e6cc33")),    // Jaune pâle
            ("Azure", "10000000000000", 950f, ParseHexColor("3399ff")),       // Bleu azur
            ("Volcanic", "10000000000000", 1175f, ParseHexColor("4d1a0d")),   // Noir volcanique
            ("Jade", "10500000000000", 1200f, ParseHexColor("00ff00")),       // Vert jade
            ("Shale", "30000000000000", 1250f, ParseHexColor("666666")),      // Gris shale
            ("Platinum", "60000000000000", 1300f, ParseHexColor("ffffff")),   // Gris platine
            ("Quartz", "100000000000000", 1400f, ParseHexColor("e6e6e6")),    // Blanc quartz
            ("Asgarite", "250000000000000", 1500f, ParseHexColor("ccb3ff")),  // Violet asgarite
            ("Stardust", "400000000000000", 1750f, ParseHexColor("ffcc33")),  // Or stardust
            ("Zeolite", "500000000000000", 1900f, ParseHexColor("b3ccff")),   // Bleu clair zeolite
            ("Ammolite", "1000000000000000", 2000f, ParseHexColor("e69aff")), // Rose ammolite
        };

        var moldOptions = new SwordAttributesConfig.AttributeOption[moldData.Length];

        for (int i = 0; i < moldData.Length; i++)
        {
            var (name, chance, multiplier, color) = moldData[i];
            // Parse la chance et calcule le weight
            double denom = ParseChance(chance);
            float weight = (float)(1.0 / denom);

            moldOptions[i] = new SwordAttributesConfig.AttributeOption
            {
                name = name,
                weight = weight,
                multiplier = multiplier,
                color = color
            };
        }

        // Assigne les moldOptions au config
        SerializedObject serializedConfig = new SerializedObject(config);
        SerializedProperty moldProperty = serializedConfig.FindProperty("moldOptions");
        
        moldProperty.arraySize = moldOptions.Length;
        for (int i = 0; i < moldOptions.Length; i++)
        {
            SerializedProperty element = moldProperty.GetArrayElementAtIndex(i);
            
            element.FindPropertyRelative("name").stringValue = moldOptions[i].name;
            element.FindPropertyRelative("weight").floatValue = moldOptions[i].weight;
            element.FindPropertyRelative("multiplier").floatValue = moldOptions[i].multiplier;
            element.FindPropertyRelative("color").colorValue = moldOptions[i].color;
        }

        serializedConfig.ApplyModifiedProperties();
        AssetDatabase.SaveAssets();
        
        EditorUtility.DisplayDialog("Success", 
            $"Populated {moldOptions.Length} molds successfully!", "OK");
    }

    private void PopulateQualities()
    {
        // Données: Name, Chance (en format texte), Multiplicateur
        var qualityData = new (string name, string chance, float multiplier)[]
        {
            ("Broken", "1", 1f),
            ("Rough", "3", 1.25f),
            ("Bad", "9", 1.5f),
            ("Okay", "27", 2f),
            ("Fine", "81", 3f),
            ("Good", "243", 4.5f),
            ("Great", "729", 6f),
            ("Awesome", "2187", 9f),
            ("Excellent", "6561", 12.5f),
            ("Perfect", "19683", 18f),
            ("Best", "59049", 27.5f),
            ("Incredible", "177147", 40f),
            ("Tremendous", "531441", 55f),
            ("Fantastic", "1594000", 70f),     // 1.594M
            ("Absurd", "4782000", 90f),        // 4.782M
            ("Exquisite", "14340000", 125f),   // 14.34M
            ("Remarkable", "43040000", 145f),  // 43.04M
            ("Extravagant", "129100000", 175f), // 129.1M
            ("Phenomenal", "387400000", 210f), // 387.4M
            ("Brilliant", "1162000000", 250f), // 1.162B
            ("Astounding", "3480000000", 300f), // 3.48B
            ("Marvelous", "10460000000", 360f), // 10.46B
            ("Spectacular", "31380000000", 430f), // 31.38B
            ("Extraordinary", "94140000000", 510f), // 94.14B
            ("Fabulous", "282400000000", 620f), // 282.4B
            ("Splendid", "847200000000", 745f), // 847.2B
            ("Glorious", "2540000000000", 900f), // 2.54T
            ("Magnificent", "7620000000000", 1075f), // 7.62T
            ("Terrific", "22870000000000", 1290f), // 22.87T
            ("Wondrous", "68630000000000", 1550f), // 68.63T
            ("Astonishing", "205800000000000", 1850f), // 205.8T
            ("Miraculous", "617600000000000", 2200f), // 617.6T
            ("Unlimited", "1850000000000000", 2500f), // 1.85Qd
        };

        var qualityOptions = new SwordAttributesConfig.AttributeOption[qualityData.Length];

        for (int i = 0; i < qualityData.Length; i++)
        {
            var (name, chance, multiplier) = qualityData[i];
            // Parse la chance et calcule le weight
            double denom = ParseChance(chance);
            float weight = (float)(1.0 / denom);

            qualityOptions[i] = new SwordAttributesConfig.AttributeOption
            {
                name = name,
                weight = weight,
                multiplier = multiplier,
                color = Color.white // Couleur neutre, sera remplacée par celle de la rareté
            };
        }

        // Assigne les qualityOptions au config
        SerializedObject serializedConfig = new SerializedObject(config);
        SerializedProperty qualityProperty = serializedConfig.FindProperty("qualityOptions");
        
        qualityProperty.arraySize = qualityOptions.Length;
        for (int i = 0; i < qualityOptions.Length; i++)
        {
            SerializedProperty element = qualityProperty.GetArrayElementAtIndex(i);
            
            element.FindPropertyRelative("name").stringValue = qualityOptions[i].name;
            element.FindPropertyRelative("weight").floatValue = qualityOptions[i].weight;
            element.FindPropertyRelative("multiplier").floatValue = qualityOptions[i].multiplier;
            element.FindPropertyRelative("color").colorValue = qualityOptions[i].color;
        }

        serializedConfig.ApplyModifiedProperties();
        AssetDatabase.SaveAssets();
        
        EditorUtility.DisplayDialog("Success", 
            $"Populated {qualityOptions.Length} qualities successfully!", "OK");
    }

    private void PopulateClasses()
    {
        // Données: Name, Chance (en format texte), Power Multiplicateur
        var classData = new (string name, string chance, float powerMultiplier)[]
        {
            ("Regular", "1", 1f),
            ("Nice", "4", 1.15f),
            ("Cool", "16", 1.3f),
            ("Decent", "64", 1.5f),
            ("Strong", "256", 2f),
            ("Tough", "1024", 2.75f),
            ("Solid", "4096", 4f),
            ("Powerful", "16384", 5.25f),
            ("Crazy", "65536", 7.5f),
            ("Unstoppable", "262144", 11f),
            ("Impossible", "1048000", 20f),       // 1.048M
            ("Ethereal", "4194000", 32.5f),       // 4.194M
            ("Unimaginable", "16770000", 45f),    // 16.77M
            ("Outrageous", "67100000", 65f),      // 67.10M
            ("Limitless", "268400000", 90f),      // 268.4M
            ("Invincible", "1073000000", 125f),   // 1.073B
            ("Chaotic", "4290000000", 150f),      // 4.29B
            ("Unbeatable", "17170000000", 180f),  // 17.17B
            ("Colossal", "68710000000", 215f),    // 68.71B
            ("Unbreakable", "274800000000", 260f), // 274.8B
            ("Indomitable", "1090000000000", 310f), // 1.09T
            ("Omnipotent", "4390000000000", 375f), // 4.39T
            ("Vigorous", "17590000000000", 450f), // 17.59T
            ("Ruthless", "70360000000000", 540f), // 70.36T
            ("Mighty", "281400000000000", 645f),  // 281.4T
            ("Immortal", "1120000000000000", 750f), // 1.12Qd
        };

        var classOptions = new SwordAttributesConfig.AttributeOption[classData.Length];

        for (int i = 0; i < classData.Length; i++)
        {
            var (name, chance, powerMultiplier) = classData[i];
            // Parse la chance et calcule le weight
            double denom = ParseChance(chance);
            float weight = (float)(1.0 / denom);

            classOptions[i] = new SwordAttributesConfig.AttributeOption
            {
                name = name,
                weight = weight,
                multiplier = powerMultiplier, // Stocke le multiplicateur de puissance ici
                color = GetClassColor(name) // Couleur selon la classe
            };
        }

        // Assigne les classOptions au config
        SerializedObject serializedConfig = new SerializedObject(config);
        SerializedProperty classProperty = serializedConfig.FindProperty("classOptions");
        
        classProperty.arraySize = classOptions.Length;
        for (int i = 0; i < classOptions.Length; i++)
        {
            SerializedProperty element = classProperty.GetArrayElementAtIndex(i);
            
            element.FindPropertyRelative("name").stringValue = classOptions[i].name;
            element.FindPropertyRelative("weight").floatValue = classOptions[i].weight;
            element.FindPropertyRelative("multiplier").floatValue = classOptions[i].multiplier;
            element.FindPropertyRelative("color").colorValue = classOptions[i].color;
        }

        serializedConfig.ApplyModifiedProperties();
        AssetDatabase.SaveAssets();
        
        EditorUtility.DisplayDialog("Success", 
            $"Populated {classOptions.Length} classes successfully!", "OK");
    }

    private void PopulateRarities()
    {
        // Données: Name, Chance (dénominateur), Multiplicateur Value, Multiplicateur Power
        var rarityData = new (string name, string chance, float valueMultiplier, float powerMultiplier)[]
        {
            ("Basic-", "1", 1f, 1f),
            ("Basic", "1.33", 1.1f, 1.2f),
            ("Basic+", "1.77", 1.21f, 1.44f),
            ("Basic++", "2.37", 1.33f, 1.72f),
            ("Common-", "3.16", 1.46f, 2.07f),
            ("Common", "4.21", 1.61f, 2.48f),
            ("Common+", "5.62", 1.77f, 2.98f),
            ("Common++", "7.49", 1.94f, 3.58f),
            ("Uncommon-", "10", 2.14f, 4.29f),
            ("Uncommon", "13.33", 2.35f, 5.15f),
            ("Uncommon+", "17.78", 2.59f, 6.19f),
            ("Uncommon++", "23.71", 2.85f, 7.43f),
            ("Rare-", "31.62", 3.13f, 8.91f),
            ("Rare", "42.16", 3.45f, 10.69f),
            ("Rare+", "56.23", 3.79f, 12.83f),
            ("Rare++", "74.98", 4.17f, 15.4f),
            ("Epic-", "100", 4.59f, 18.48f),
            ("Epic", "133.35", 5.05f, 22.18f),
            ("Epic+", "177.82", 5.55f, 26.62f),
            ("Epic++", "237.13", 6.11f, 31.94f),
            ("Legendary-", "316.22", 6.72f, 38.33f),
            ("Legendary", "421.69", 7.4f, 46f),
            ("Legendary+", "562.34", 8.14f, 55.2f),
            ("Legendary++", "749.89", 8.95f, 66.24f),
            ("Mythical-", "1000", 9.84f, 79.49f),
            ("Mythical", "1334", 10.83f, 95.39f),
            ("Mythical+", "1778", 11.91f, 114.47f),
            ("Mythical++", "2371", 13.1f, 137.37f),
            ("Divine-", "3162", 14.42f, 164.84f),
            ("Divine", "4217", 15.86f, 197.81f),
            ("Divine+", "5623", 17.44f, 237.37f),
            ("Divine++", "7499", 19.19f, 284.85f),
            ("Super-", "10000", 21.11f, 341.82f),
            ("Super", "13335", 23.22f, 410.18f),
            ("Super+", "17783", 25.54f, 492.22f),
            ("Super++", "23714", 28.1f, 590.66f),
            ("Mega-", "31623", 30.91f, 708.8f),
            ("Mega", "42170", 34f, 850.56f),
            ("Mega+", "56234", 37.4f, 1021f),
            ("Mega++", "74989", 39.27f, 1123f),
            ("Ultra-", "100000", 41.23f, 1235f),
            ("Ultra", "133352", 43.29f, 1359f),
            ("Ultra+", "177828", 45.46f, 1495f),
            ("Ultra++", "237137", 47.73f, 1644f),
            ("Omega-", "316228", 50.12f, 1809f),
            ("Omega", "421697", 52.62f, 1990f),
            ("Omega+", "562341", 55.25f, 2189f),
            ("Omega++", "749894", 58.02f, 2407f),
            ("Extreme-", "1000000", 60.92f, 2648f),
            ("Extreme", "1333000", 63.96f, 2913f),
            ("Extreme+", "1778000", 67.16f, 3204f),
            ("Extreme++", "2371000", 70.52f, 3525f),
            ("Ultimate-", "3162000", 74.05f, 3877f),
            ("Ultimate", "4216000", 77.75f, 4265f),
            ("Ultimate+", "5623000", 81.64f, 4691f),
            ("Ultimate++", "7498000", 85.72f, 5161f),
            ("Insane-", "10000000", 90f, 5677f),
            ("Insane", "13330000", 94.5f, 6244f),
            ("Insane+", "17780000", 99.23f, 6869f),
            ("Insane++", "23710000", 104.19f, 7556f),
            ("Hyper-", "31620000", 109.4f, 8311f),
            ("Hyper", "42160000", 114.87f, 9142f),
            ("Hyper+", "56230000", 120.61f, 10057f),
            ("Hyper++", "74980000", 126.65f, 11062f),
            ("Godly-", "100000000", 139.31f, 12722f),
            ("Godly", "200000000", 153.24f, 14.63f),
            ("Godly+", "400000000", 168.57f, 16824f),
            ("Godly++", "800000000", 185.42f, 19348f),
            ("Unique-", "1600000000", 203.97f, 22250f),
            ("Unique", "3200000000", 224.36f, 25588f),
            ("Unique+", "6400000000", 246.8f, 29426f),
            ("Unique++", "12800000000", 271.48f, 33840f),
            ("Exotic-", "25600000000", 298.63f, 38916f),
            ("Exotic", "51200000000", 328.49f, 44753f),
            ("Exotic+", "102400000000", 361.34f, 51466f),
            ("Exotic++", "204800000000", 397.48f, 59186f),
            ("Supreme-", "409600000000", 437.23f, 68064f),
            ("Supreme", "819200000000", 480.95f, 78273f),
            ("Supreme+", "1638000000000", 529.04f, 90014f),
            ("Supreme++", "3276000000000", 581.95f, 103516f),
            ("Celestial-", "6553000000000", 640.14f, 119044f),
            ("Celestial", "13100000000000", 704.16f, 136900f),
            ("Celestial+", "26210000000000", 774.58f, 157435f),
            ("Celestial++", "52420000000000", 852.03f, 181050f),
            ("Eternal-", "104800000000000", 937.24f, 208208f),
            ("Eternal", "209700000000000", 1031f, 239439f),
            ("Eternal+", "419400000000000", 1134f, 317658f),
            ("Eternal++", "838800000000000", 1247f, 316658f),
            ("Cosmic-", "1670000000000000", 1372f, 364157f),
            ("Cosmic", "3350000000000000", 1509f, 418781f),
            ("Cosmic+", "6710000000000000", 1660f, 481598f),
            ("Cosmic++", "13420000000000000", 1826f, 553838f),
            ("Cosmic+3", "26840000000000000", 2009f, 636913f),
        };

        var rarityOptions = new SwordAttributesConfig.AttributeOption[rarityData.Length];

        for (int i = 0; i < rarityData.Length; i++)
        {
            var (name, chance, valueMultiplier, powerMultiplier) = rarityData[i];
            // Parse la chance et calcule le weight
            double denom = ParseChance(chance);
            float weight = (float)(1.0 / denom);

            rarityOptions[i] = new SwordAttributesConfig.AttributeOption
            {
                name = name,
                weight = weight,
                multiplier = valueMultiplier, // Multiplicateur de VALEUR (money)
                color = GetRarityColor(name) // Couleur selon la rareté
                // Note: Le multiplicateur de POWER (powerMultiplier) peut être implémenté ultérieurement
            };
        }

        // Assigne les rarityOptions au config
        SerializedObject serializedConfig = new SerializedObject(config);
        SerializedProperty rarityProperty = serializedConfig.FindProperty("rarityOptions");
        
        rarityProperty.arraySize = rarityOptions.Length;
        for (int i = 0; i < rarityOptions.Length; i++)
        {
            SerializedProperty element = rarityProperty.GetArrayElementAtIndex(i);
            
            element.FindPropertyRelative("name").stringValue = rarityOptions[i].name;
            element.FindPropertyRelative("weight").floatValue = rarityOptions[i].weight;
            element.FindPropertyRelative("multiplier").floatValue = rarityOptions[i].multiplier;
            element.FindPropertyRelative("color").colorValue = rarityOptions[i].color;
        }

        serializedConfig.ApplyModifiedProperties();
        AssetDatabase.SaveAssets();
        
        EditorUtility.DisplayDialog("Success", 
            $"Populated {rarityOptions.Length} rarities successfully!", "OK");
    }
}
