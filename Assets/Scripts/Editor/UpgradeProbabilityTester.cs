using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
/// <summary>
/// Outil de test pour visualiser l'impact des upgrades sur les probabilités
/// Menu : Window > Sword Factory > Upgrade Probability Tester
/// </summary>
public class UpgradeProbabilityTester : EditorWindow
{
    private SwordAttributesConfig config;
    private int testLevel = 0;
    private Vector2 scrollPosition;
    private string selectedCategory = "moldOptions";

    [MenuItem("Window/Sword Factory/Upgrade Probability Tester")]
    public static void ShowWindow()
    {
        GetWindow<UpgradeProbabilityTester>("Upgrade Tester");
    }

    private void OnGUI()
    {
        GUILayout.Label("Upgrade Probability Tester", EditorStyles.boldLabel);
        
        config = EditorGUILayout.ObjectField("Attributes Config", config, typeof(SwordAttributesConfig), false) as SwordAttributesConfig;
        
        if (config == null)
        {
            EditorGUILayout.HelpBox("Assign a SwordAttributesConfig to test", MessageType.Info);
            return;
        }

        EditorGUILayout.Space();
        
        // Sélection de catégorie
        EditorGUILayout.LabelField("Category:", EditorStyles.boldLabel);
        if (GUILayout.Button("Test Molds")) selectedCategory = "moldOptions";
        if (GUILayout.Button("Test Qualities")) selectedCategory = "qualityOptions";
        if (GUILayout.Button("Test Classes")) selectedCategory = "classOptions";
        if (GUILayout.Button("Test Rarities")) selectedCategory = "rarityOptions";
        
        EditorGUILayout.Space();
        
        // Slider pour le niveau d'upgrade
        testLevel = EditorGUILayout.IntSlider("Upgrade Level", testLevel, 0, 100);
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Calculate Probabilities", GUILayout.Height(30)))
        {
            CalculateAndDisplay();
        }
    }

    private void CalculateAndDisplay()
    {
        var options = GetOptionsArray();
        if (options == null || options.Length == 0)
        {
            Debug.LogError("No options found for category: " + selectedCategory);
            return;
        }

        // Créer des copies pour ne pas modifier les originaux
        var originalOptions = new SwordAttributesConfig.AttributeOption[options.Length];
        var upgradedOptions = new SwordAttributesConfig.AttributeOption[options.Length];
        
        System.Array.Copy(options, originalOptions, options.Length);
        System.Array.Copy(options, upgradedOptions, options.Length);

        // Appliquer l'upgrade
        if (testLevel > 0)
        {
            var upgradeSystem = new GameObject("TempUpgradeSystem").AddComponent<UpgradeSystem>();
            upgradeSystem.ApplyLuckBonus(upgradedOptions, testLevel);
            DestroyImmediate(upgradeSystem.gameObject);
        }

        // Calculer les totaux
        float originalTotal = 0f;
        float upgradedTotal = 0f;
        
        foreach (var opt in originalOptions)
            originalTotal += opt.weight;
        foreach (var opt in upgradedOptions)
            upgradedTotal += opt.weight;

        // Afficher les résultats
        System.Text.StringBuilder report = new System.Text.StringBuilder();
        report.AppendLine($"========================================");
        report.AppendLine($"UPGRADE PROBABILITY TEST - {selectedCategory.ToUpper()}");
        report.AppendLine($"Upgrade Level: {testLevel}");
        report.AppendLine($"========================================\n");

        report.AppendLine("FORMAT: Name | Original% | Upgraded% | Change | Odds");
        report.AppendLine("--------------------------------------------------------");

        for (int i = 0; i < originalOptions.Length; i++)
        {
            float origPercent = (originalOptions[i].weight / originalTotal) * 100f;
            float upgPercent = (upgradedOptions[i].weight / upgradedTotal) * 100f;
            float change = ((upgPercent - origPercent) / origPercent) * 100f;
            
            // Calcul des odds (1/X)
            float origOdds = 1f / (originalOptions[i].weight / originalTotal);
            float upgOdds = 1f / (upgradedOptions[i].weight / upgradedTotal);

            string changeMark = change > 0 ? "↑" : (change < 0 ? "↓" : "=");
            
            report.AppendLine(
                $"{originalOptions[i].name,-20} | " +
                $"{origPercent,6:F3}% | " +
                $"{upgPercent,6:F3}% | " +
                $"{changeMark} {Mathf.Abs(change),6:F1}% | " +
                $"1/{origOdds:F1} → 1/{upgOdds:F1}"
            );
        }

        report.AppendLine("\n========================================");
        report.AppendLine("SUMMARY:");
        report.AppendLine($"Total original weight: {originalTotal:F6}");
        report.AppendLine($"Total upgraded weight: {upgradedTotal:F6}");
        
        // Identifier les gagnants et perdants
        int improved = 0, worsened = 0, unchanged = 0;
        for (int i = 0; i < originalOptions.Length; i++)
        {
            float origPercent = (originalOptions[i].weight / originalTotal) * 100f;
            float upgPercent = (upgradedOptions[i].weight / upgradedTotal) * 100f;
            
            if (upgPercent > origPercent + 0.001f) improved++;
            else if (upgPercent < origPercent - 0.001f) worsened++;
            else unchanged++;
        }
        
        report.AppendLine($"\nImproved: {improved} | Worsened: {worsened} | Unchanged: {unchanged}");
        report.AppendLine("========================================");

        Debug.Log(report.ToString());
    }

    private SwordAttributesConfig.AttributeOption[] GetOptionsArray()
    {
        switch (selectedCategory)
        {
            case "moldOptions": return config.moldOptions;
            case "qualityOptions": return config.qualityOptions;
            case "classOptions": return config.classOptions;
            case "rarityOptions": return config.rarityOptions;
            default: return null;
        }
    }
}
#endif
