using UnityEngine;
using TMPro;

/// <summary>
/// Affiche les statistiques des upgrades en temps réel (optionnel, pour debug)
/// </summary>
public class UpgradeStatsDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text statsText;
    [SerializeField] private SwordAttributesConfig attributesConfig;
    [SerializeField] private KeyCode toggleKey = KeyCode.I;
    
    private bool isVisible = false;

    void Start()
    {
        if (statsText != null)
            statsText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            isVisible = !isVisible;
            if (statsText != null)
                statsText.gameObject.SetActive(isVisible);
            
            if (isVisible)
                UpdateDisplay();
        }
        
        if (isVisible && Time.frameCount % 30 == 0) // Mise à jour toutes les 0.5s
        {
            UpdateDisplay();
        }
    }

    private void UpdateDisplay()
    {
        if (statsText == null || UpgradeSystem.Instance == null || attributesConfig == null)
            return;

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        
        sb.AppendLine("=== UPGRADE STATS ===");
        sb.AppendLine($"Molder: Lvl {UpgradeSystem.Instance.Molder.currentLevel}");
        sb.AppendLine($"Quality: Lvl {UpgradeSystem.Instance.Quality.currentLevel}");
        sb.AppendLine($"Class: Lvl {UpgradeSystem.Instance.SwordClass.currentLevel}");
        sb.AppendLine($"Rarity: Lvl {UpgradeSystem.Instance.Rarity.currentLevel}");
        
        sb.AppendLine("\n=== MOLDS TOP 3 (upgraded) ===");
        DisplayTopOptions(sb, attributesConfig.moldOptions, UpgradeSystem.Instance.Molder.currentLevel);
        
        statsText.text = sb.ToString();
    }

    private void DisplayTopOptions(System.Text.StringBuilder sb, SwordAttributesConfig.AttributeOption[] options, int level)
    {
        if (options == null || options.Length == 0)
            return;

        // Créer une copie et appliquer l'upgrade
        var modifiedOptions = new SwordAttributesConfig.AttributeOption[options.Length];
        System.Array.Copy(options, modifiedOptions, options.Length);
        
        if (level > 0)
            UpgradeSystem.Instance.ApplyLuckBonus(modifiedOptions, level);

        // Calculer le total
        float total = 0f;
        foreach (var opt in modifiedOptions)
            total += opt.weight;

        // Afficher les 3 plus rares
        int count = Mathf.Min(3, modifiedOptions.Length);
        for (int i = modifiedOptions.Length - 1; i >= modifiedOptions.Length - count; i--)
        {
            float percent = (modifiedOptions[i].weight / total) * 100f;
            float odds = 1f / (modifiedOptions[i].weight / total);
            sb.AppendLine($"{modifiedOptions[i].name}: {percent:F3}% (1/{odds:F1})");
        }
    }
}
