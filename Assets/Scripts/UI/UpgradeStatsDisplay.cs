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
        
        sb.AppendLine("\n=== ALL MOLDS (upgraded) ===");
        DisplayAllOptions(sb, attributesConfig.moldOptions, UpgradeSystem.Instance.Molder.currentLevel, UpgradeSystem.Instance.Molder);
        
        statsText.text = sb.ToString();
    }

    private void DisplayAllOptions(System.Text.StringBuilder sb, SwordAttributesConfig.AttributeOption[] options, int level, UpgradeSystem.UpgradeCategory category)
    {
        if (options == null || options.Length == 0)
            return;

        // Créer une copie et appliquer l'upgrade
        var modifiedOptions = new SwordAttributesConfig.AttributeOption[options.Length];
        System.Array.Copy(options, modifiedOptions, options.Length);
        
        if (level > 0 && category != null)
            UpgradeSystem.Instance.ApplyLuckBonus(modifiedOptions, level, category);

        // Afficher TOUS les molds (avec indication si retiré)
        for (int i = 0; i < modifiedOptions.Length; i++)
        {
            if (category != null && category.removedOptions.Contains(modifiedOptions[i].name))
            {
                sb.AppendLine($"{modifiedOptions[i].name}: REMOVED");
            }
            else
            {
                // weight contient maintenant directement les odds
                float odds = modifiedOptions[i].weight;
                sb.AppendLine($"{modifiedOptions[i].name}: 1/{odds:F1}");
            }
        }
    }
}
