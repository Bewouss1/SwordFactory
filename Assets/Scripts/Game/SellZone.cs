using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// Gère une zone de vente avec des slots pour les épées
/// Les épées y sont placées après leur passage sur le convoyeur
/// </summary>
public class SellZone : MonoBehaviour
{
    [SerializeField] private Transform slotsContainer;
    [SerializeField] private List<Transform> slots = new List<Transform>();
    private int currentSlotIndex = 0;

    [Header("Sell Settings")]
    [SerializeField] private float sellCountdownSeconds = 30f;
    [SerializeField] private bool hideTimeTextWhenIdle = true;

    [Header("Money")]
    [SerializeField] private PlayerMoney playerMoney;

    [Header("Level / XP")]
    [SerializeField] private PlayerLevel playerLevel;

    private readonly Dictionary<Transform, Coroutine> activeCountdowns = new Dictionary<Transform, Coroutine>();

    void OnEnable()
    {
        // Récupérer automatiquement tous les enfants comme slots
        slots.Clear();
        Transform root = slotsContainer != null ? slotsContainer : transform;

        foreach (Transform child in root)
        {
            slots.Add(child);
        }

        if (slots.Count == 0)
        {
            Debug.LogWarning("SellZone: No slots found! Add slots as children of the container.", this);
        }

    }

    /// <summary>
    /// Obtient le prochain slot disponible
    /// </summary>
    public Transform GetNextSlot()
    {
        if (slots.Count == 0)
        {
            Debug.LogError("SellZone: No slots available!", this);
            return null;
        }
        return GetNextAvailableSlot();
    }

    /// <summary>
    /// Cherche un slot libre. Retourne null si tous les slots sont occupés.
    /// </summary>
    private Transform GetNextAvailableSlot()
    {
        if (slots.Count == 0)
            return null;

        int checkedCount = 0;
        int index = currentSlotIndex;

        while (checkedCount < slots.Count)
        {
            Transform slot = slots[index];
            if (slot != null && !IsSlotOccupied(slot))
            {
                currentSlotIndex = (index + 1) % slots.Count;
                return slot;
            }

            index = (index + 1) % slots.Count;
            checkedCount++;
        }

        return null; // tous les slots sont occupés
    }

    private bool IsSlotOccupied(Transform slot)
    {
        if (slot == null)
            return true;

        // Un slot est occupé s'il contient une épée
        return slot.GetComponentInChildren<SwordStats>() != null;
    }

    /// <summary>
    /// Réinitialise l'index des slots (utile pour restart)
    /// </summary>
    public void ResetSlots()
    {
        currentSlotIndex = 0;
    }

    public void StartSellCountdown(Transform swordTransform, Transform slot)
    {
        if (swordTransform == null)
            return;

        TMP_Text timeText = ResolveTimeText(slot, swordTransform);

        if (activeCountdowns.TryGetValue(swordTransform, out Coroutine existing))
        {
            StopCoroutine(existing);
            activeCountdowns.Remove(swordTransform);
        }

        Coroutine routine = StartCoroutine(SellCountdownRoutine(swordTransform, timeText));
        activeCountdowns[swordTransform] = routine;
    }

    private TMP_Text ResolveTimeText(Transform slot, Transform swordTransform)
    {
        TMP_Text timeText = null;

        if (swordTransform != null)
        {
            SwordStats stats = swordTransform.GetComponent<SwordStats>();
            if (stats != null)
                timeText = stats.TimeText;
        }

        if (timeText == null && slot != null)
            timeText = slot.GetComponentInChildren<TMP_Text>(true);

        if (timeText != null)
        {
            if (hideTimeTextWhenIdle)
                timeText.gameObject.SetActive(true);
        }

        return timeText;
    }

    private IEnumerator SellCountdownRoutine(Transform swordTransform, TMP_Text timeText)
    {
        Transform swordKey = swordTransform;
        float remaining = Mathf.Max(0f, sellCountdownSeconds);
        int lastDisplayed = -1;

        while (remaining > 0f)
        {
            if (swordTransform == null)
                break;

            int display = Mathf.CeilToInt(remaining);
            if (display != lastDisplayed)
            {
                lastDisplayed = display;
                if (timeText != null)
                    timeText.text = display.ToString();
            }

            yield return new WaitForSeconds(1f);
            remaining -= 1f;
        }

        if (timeText != null)
            timeText.text = "0";

        if (swordTransform != null)
        {
            SwordStats stats = swordTransform.GetComponent<SwordStats>();
            float value = stats != null ? stats.GetValue() : 0f;

            if (playerMoney != null)
                playerMoney.AddMoney(value);
            else if (value > 0f)
                Debug.LogWarning("SellZone: PlayerMoney reference is missing, cannot add money.", this);

            if (playerLevel != null)
                playerLevel.AddXp(10);

            Destroy(swordTransform.gameObject);
        }

        if (timeText != null && hideTimeTextWhenIdle)
            timeText.gameObject.SetActive(false);

        activeCountdowns.Remove(swordKey);
    }
}
