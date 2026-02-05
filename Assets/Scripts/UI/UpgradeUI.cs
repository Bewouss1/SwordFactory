using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Interface utilisateur pour le système d'upgrade
/// Affiche les niveaux actuels et permet d'améliorer chaque catégorie
/// </summary>
public class UpgradeUI : MonoBehaviour
{
    [System.Serializable]
    public class CategoryUI
    {
        public string categoryName;
        public TMP_Text levelText;           // Ex: "Level 1 Molder"
        public TMP_Text upgradeCostText;     // Ex: "Upgrade x1 ($50)"
        public Button upgradeButton;         // Bouton associé au texte du coût
    }

    [Header("References")]
    [SerializeField] private PlayerMoney playerMoney;
    [SerializeField] private UpgradeSystem upgradeSystem;

    [Header("UI Elements")]
    [SerializeField] private CategoryUI molderUI;
    [SerializeField] private CategoryUI qualityUI;
    [SerializeField] private CategoryUI classUI;
    [SerializeField] private CategoryUI rarityUI;

    [Header("Panel")]
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private KeyCode toggleKey = KeyCode.U;

    void Start()
    {
        // Trouver les références si non assignées
        if (upgradeSystem == null)
            upgradeSystem = UpgradeSystem.Instance;

        if (playerMoney == null)
            playerMoney = FindFirstObjectByType<PlayerMoney>();

        // Setup des boutons
        if (molderUI.upgradeButton != null)
            molderUI.upgradeButton.onClick.AddListener(() => UpgradeMolder());

        if (qualityUI.upgradeButton != null)
            qualityUI.upgradeButton.onClick.AddListener(() => UpgradeQuality());

        if (classUI.upgradeButton != null)
            classUI.upgradeButton.onClick.AddListener(() => UpgradeClass());

        if (rarityUI.upgradeButton != null)
            rarityUI.upgradeButton.onClick.AddListener(() => UpgradeRarity());

        // Cacher le panel au démarrage
        if (upgradePanel != null)
            upgradePanel.SetActive(false);

        UpdateUI();
    }

    void Update()
    {
        // Toggle du panel avec la touche U
        if (Input.GetKeyDown(toggleKey))
        {
            if (upgradePanel != null)
            {
                bool newState = !upgradePanel.activeSelf;
                upgradePanel.SetActive(newState);
                
                // Gérer le curseur
                if (newState)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
                else
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
            }
        }

        // Mettre à jour l'UI si le panel est visible
        if (upgradePanel != null && upgradePanel.activeSelf)
        {
            UpdateUI();
        }
    }

    private void UpgradeMolder()
    {
        if (upgradeSystem != null && playerMoney != null)
        {
            upgradeSystem.TryUpgradeCategory(upgradeSystem.Molder, playerMoney);
            UpdateUI();
        }
    }

    private void UpgradeQuality()
    {
        if (upgradeSystem != null && playerMoney != null)
        {
            upgradeSystem.TryUpgradeCategory(upgradeSystem.Quality, playerMoney);
            UpdateUI();
        }
    }

    private void UpgradeClass()
    {
        if (upgradeSystem != null && playerMoney != null)
        {
            upgradeSystem.TryUpgradeCategory(upgradeSystem.SwordClass, playerMoney);
            UpdateUI();
        }
    }

    private void UpgradeRarity()
    {
        if (upgradeSystem != null && playerMoney != null)
        {
            upgradeSystem.TryUpgradeCategory(upgradeSystem.Rarity, playerMoney);
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        if (upgradeSystem == null || playerMoney == null)
            return;

        UpdateCategoryUI(molderUI, upgradeSystem.Molder);
        UpdateCategoryUI(qualityUI, upgradeSystem.Quality);
        UpdateCategoryUI(classUI, upgradeSystem.SwordClass);
        UpdateCategoryUI(rarityUI, upgradeSystem.Rarity);
    }

    private void UpdateCategoryUI(CategoryUI ui, UpgradeSystem.UpgradeCategory category)
    {
        if (category == null)
            return;

        // Texte du niveau : "Level X Molder"
        if (ui.levelText != null)
        {
            ui.levelText.text = $"Level {category.currentLevel} {category.name}";
        }

        // Coût et multiplicateur
        float nextCost = category.GetNextLevelCost();
        bool canAfford = playerMoney.CurrentMoney >= nextCost;
        bool maxed = category.currentLevel >= category.maxLevel;

        if (ui.upgradeCostText != null)
        {
            if (maxed)
            {
                ui.upgradeCostText.text = "MAX LEVEL";
                ui.upgradeCostText.color = Color.green;
            }
            else
            {
                ui.upgradeCostText.text = $"Upgrade x1 ({SwordStats.FormatMoneyValue(nextCost)})";
                ui.upgradeCostText.color = canAfford ? Color.white : Color.red;
            }
        }

        // Bouton
        if (ui.upgradeButton != null)
        {
            ui.upgradeButton.interactable = !maxed && canAfford;
        }
    }
}
