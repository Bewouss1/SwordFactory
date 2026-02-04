using UnityEngine;
using System.Globalization;

#if UNITY_EDITOR
/// <summary>
/// Méthodes utilitaires partagées pour les générateurs de configuration
/// </summary>
public static class EditorHelpers
{
    /// <summary>
    /// Parse une chance au format texte (ex: "1", "1.33", "200M", "1.6B", "6.553T", "1.85Qd")
    /// et retourne le dénominateur numérique
    /// </summary>
    public static double ParseChance(string chanceStr)
    {
        // Remplace les virgules par des points (locale FR)
        chanceStr = chanceStr.Replace(",", ".");
        chanceStr = chanceStr.Trim();

        // Vérifie les suffixes multiplicateurs
        double multiplier = 1.0;
        if (chanceStr.EndsWith("Qd", System.StringComparison.OrdinalIgnoreCase))
        {
            multiplier = 1e15; // Quadrillion
            chanceStr = chanceStr.Substring(0, chanceStr.Length - 2).Trim();
        }
        else if (chanceStr.EndsWith("T", System.StringComparison.OrdinalIgnoreCase))
        {
            multiplier = 1e12; // Trillion
            chanceStr = chanceStr.Substring(0, chanceStr.Length - 1).Trim();
        }
        else if (chanceStr.EndsWith("B", System.StringComparison.OrdinalIgnoreCase))
        {
            multiplier = 1e9; // Billion
            chanceStr = chanceStr.Substring(0, chanceStr.Length - 1).Trim();
        }
        else if (chanceStr.EndsWith("M", System.StringComparison.OrdinalIgnoreCase))
        {
            multiplier = 1e6; // Million
            chanceStr = chanceStr.Substring(0, chanceStr.Length - 1).Trim();
        }
        else if (chanceStr.EndsWith("K", System.StringComparison.OrdinalIgnoreCase))
        {
            multiplier = 1e3; // Thousand
            chanceStr = chanceStr.Substring(0, chanceStr.Length - 1).Trim();
        }

        if (double.TryParse(chanceStr, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
        {
            return value * multiplier;
        }

        Debug.LogError($"EditorHelpers: Could not parse chance '{chanceStr}'");
        return 1.0;
    }

    /// <summary>
    /// Parse une couleur hexadécimale
    /// </summary>
    public static Color ParseHexColor(string hex)
    {
        if (string.IsNullOrWhiteSpace(hex))
            return Color.white;

        if (!hex.StartsWith("#"))
            hex = "#" + hex;

        if (ColorUtility.TryParseHtmlString(hex, out var color))
            return color;

        return Color.white;
    }
}
#endif
