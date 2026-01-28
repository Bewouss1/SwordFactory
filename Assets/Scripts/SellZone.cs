using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Gère une zone de vente avec des slots pour les épées
/// Les épées y sont placées après leur passage sur le convoyeur
/// </summary>
public class SellZone : MonoBehaviour
{
    [SerializeField] private List<Transform> slots = new List<Transform>();
    private int currentSlotIndex = 0;

    void OnEnable()
    {
        // Récupérer automatiquement tous les enfants comme slots
        foreach (Transform child in transform)
        {
            slots.Add(child);
        }

        if (slots.Count == 0)
        {
            Debug.LogWarning("SellZone: No slots found! Create empty GameObjects as children.", this);
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

        Transform slot = slots[currentSlotIndex];
        currentSlotIndex = (currentSlotIndex + 1) % slots.Count; // Boucle à travers les slots

        return slot;
    }

    /// <summary>
    /// Réinitialise l'index des slots (utile pour restart)
    /// </summary>
    public void ResetSlots()
    {
        currentSlotIndex = 0;
    }
}
