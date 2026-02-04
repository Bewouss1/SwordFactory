using UnityEngine;
using TMPro;

/// <summary>
/// Gère l'argent du joueur et son affichage dans l'UI.
/// </summary>
public class PlayerMoney : MonoBehaviour
{
    [SerializeField] private TMP_Text moneyText;
    [SerializeField] private float currentMoney = 0f;

    public float CurrentMoney => currentMoney;

    void OnEnable()
    {
        RefreshUI();
    }

    public void AddMoney(float amount)
    {
        currentMoney += amount;
        currentMoney = Mathf.Max(0f, currentMoney); // Ne pas descendre en dessous de 0
        RefreshUI();
    }

    public void SetMoney(float amount)
    {
        currentMoney = Mathf.Max(0f, amount);
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (moneyText == null)
            return;

        moneyText.text = SwordStats.FormatMoneyValue(currentMoney);
    }
}
