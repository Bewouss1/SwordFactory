using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerLevel : MonoBehaviour
{
    [Header("Level Settings")]
    [SerializeField] private int level = 1;
    [SerializeField] private int currentXp = 0;
    [SerializeField] private int xpToNext = 100;
    [SerializeField] private int xpIncrementPerLevel = 20;

    [Header("UI (Optional)")]
    [SerializeField] private Slider xpBar;
    [SerializeField] private TMP_Text levelText;

    void Start()
    {
        UpdateUI();
    }

    public void AddXp(int amount)
    {
        if (amount <= 0) return;

        currentXp += amount;

        while (currentXp >= xpToNext)
        {
            currentXp -= xpToNext;
            level++;
            xpToNext += xpIncrementPerLevel;
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (xpBar != null)
        {
            xpBar.maxValue = xpToNext;
            xpBar.value = currentXp;
        }

        if (levelText != null)
            levelText.text = "Level " + level + " | " + currentXp + "/" + xpToNext + " EXP";
    }
}
