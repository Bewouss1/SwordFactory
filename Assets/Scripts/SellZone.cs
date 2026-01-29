using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Gère une zone de vente avec des slots pour les épées
/// Les épées y sont placées après leur passage sur le convoyeur
/// </summary>
public class SellZone : MonoBehaviour
{
    [SerializeField] private Transform slotsContainer;
    [SerializeField] private List<Transform> slots = new List<Transform>();
    private int currentSlotIndex = 0;

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
}
