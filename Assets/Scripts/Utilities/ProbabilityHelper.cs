using UnityEngine;
using System;

/// <summary>
/// Classe utilitaire pour les calculs de probabilités et tirages aléatoires pondérés
/// Centralise la logique de sélection aléatoire utilisée dans le projet
/// </summary>
public static class ProbabilityHelper
{
    /// <summary>
    /// Sélectionne un élément aléatoire d'un tableau en fonction des poids
    /// </summary>
    /// <typeparam name="T">Type des éléments du tableau</typeparam>
    /// <param name="options">Tableau d'options</param>
    /// <param name="getWeight">Fonction pour extraire le poids de chaque élément</param>
    /// <returns>L'élément sélectionné, ou default(T) si le tableau est vide</returns>
    public static T PickRandomWeighted<T>(T[] options, Func<T, float> getWeight)
    {
        if (options == null || options.Length == 0)
        {
            Debug.LogWarning("ProbabilityHelper: Cannot pick from null or empty array!");
            return default;
        }

        // Calcul du poids total
        float totalWeight = 0f;
        foreach (var option in options)
        {
            float weight = getWeight(option);
            if (weight < 0f)
            {
                Debug.LogWarning($"ProbabilityHelper: Negative weight detected ({weight}), treating as 0");
                weight = 0f;
            }
            totalWeight += weight;
        }

        // Si aucun poids valide, retourner le premier élément
        if (totalWeight <= 0f)
        {
            Debug.LogWarning("ProbabilityHelper: Total weight is 0 or negative, returning first element");
            return options[0];
        }

        // Tirage aléatoire
        float roll = UnityEngine.Random.value * totalWeight;
        float cumulative = 0f;

        for (int i = 0; i < options.Length; i++)
        {
            float weight = Mathf.Max(0f, getWeight(options[i]));
            cumulative += weight;

            if (roll <= cumulative)
                return options[i];
        }

        // Fallback : retourner le dernier élément
        return options[options.Length - 1];
    }

    /// <summary>
    /// Calcule le pourcentage de chance pour un élément dans un tableau pondéré
    /// </summary>
    public static float CalculatePercentage<T>(T[] options, Func<T, float> getWeight, T target, Func<T, T, bool> comparer)
    {
        if (options == null || options.Length == 0)
            return 0f;

        float totalWeight = 0f;
        float targetWeight = 0f;

        foreach (var option in options)
        {
            float weight = Mathf.Max(0f, getWeight(option));
            totalWeight += weight;

            if (comparer(option, target))
                targetWeight = weight;
        }

        return totalWeight > 0f ? (targetWeight / totalWeight) * 100f : 0f;
    }

    /// <summary>
    /// Affiche un rapport détaillé des probabilités d'un tableau pondéré
    /// Utile pour le debug
    /// </summary>
    public static string GenerateWeightReport<T>(T[] options, Func<T, float> getWeight, Func<T, string> getName)
    {
        if (options == null || options.Length == 0)
            return "Empty array";

        float totalWeight = 0f;
        foreach (var option in options)
            totalWeight += Mathf.Max(0f, getWeight(option));

        System.Text.StringBuilder report = new System.Text.StringBuilder();
        report.AppendLine($"Total Weight: {totalWeight:F6}");
        report.AppendLine("Probabilities:");

        foreach (var option in options)
        {
            float weight = Mathf.Max(0f, getWeight(option));
            float percentage = totalWeight > 0f ? (weight / totalWeight) * 100f : 0f;
            float odds = weight > 0f ? 1f / weight : 0f;
            
            report.AppendLine($"  {getName(option)}: {percentage:F4}% (1 in {odds:F2}) | weight={weight:F10}");
        }

        return report.ToString();
    }
}
