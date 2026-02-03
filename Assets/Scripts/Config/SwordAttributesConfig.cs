using UnityEngine;

[CreateAssetMenu(menuName = "SwordFactory/Sword Attributes Config", fileName = "SwordAttributesConfig")]
public class SwordAttributesConfig : ScriptableObject
{
    [System.Serializable]
    public struct AttributeOption
    {
        [ReadOnly]
        public string name;      // wood, stone, epic, etc.
        
        [ReadOnly]
        public float weight;     // pour le tirage aléatoire (auto-calculé)
        
        [ReadOnly]
        public float multiplier; // pour la valeur (auto-calculé)
        
        [ReadOnly]
        public Color color;      // couleur associée à cet attribut
    }

    [Header("Probabilités Auto-calculées")]
    [Tooltip("Ratio de rareté entre chaque tier (1.35 = chaque tier est 35% plus rare)")]
    [SerializeField] private float rarityRatio = 1.35f;
    
    [Tooltip("Ratio pour les multiplicateurs de rareté (1.21 = chaque tier vaut 21% de plus)")]
    [SerializeField] private float rarityMultiplierRatio = 1.21f;

    [Header("Mold Options")]
    public AttributeOption[] moldOptions;

    [Header("Quality Options")]
    public AttributeOption[] qualityOptions;

    [Header("Class Options")]
    public AttributeOption[] classOptions;

    [Header("Rarity Options")]
    public AttributeOption[] rarityOptions;

    [Header("Enchant Options")]
    public AttributeOption[] enchantOptions;

    [Header("Fixed Colors")]
    public Color enchantTextColor = new Color(1f, 0.84f, 0f); // Couleur dorée par défaut

    /// <summary>
    /// Recalcule automatiquement les poids en fonction du ratio de rareté
    /// Appelé automatiquement dans l'éditeur Unity quand le ScriptableObject est modifié
    /// </summary>
    private void OnValidate()
    {
        RecalculateWeights();
    }

    /// <summary>
    /// Calcule les poids pour chaque catégorie d'attributs selon le ratio de rareté
    /// IMPORTANT: Le ratio 1.35 s'applique UNIQUEMENT à rarityOptions
    /// Les moldOptions gardent leurs poids tels quels (basés sur les chances 1/1, 1/10, etc.)
    /// </summary>
    private void RecalculateWeights()
    {
        if (rarityRatio <= 1f)
            rarityRatio = 1.35f;
        
        if (rarityMultiplierRatio <= 1f)
            rarityMultiplierRatio = 1.21f;

        // Les moldOptions gardent leurs poids d'origine, PAS de ratio appliqué
        // CalculateWeightsForArray(moldOptions); <- SUPPRIMÉ

        // Les autres catégories non-rareté gardent aussi leurs poids d'origine
        // CalculateWeightsForArray(qualityOptions); <- SUPPRIMÉ
        // CalculateWeightsForArray(classOptions); <- SUPPRIMÉ
        // CalculateWeightsForArray(enchantOptions); <- SUPPRIMÉ
        
        // SEULE la rareté utilise le ratio
        CalculateWeightsForArray(rarityOptions);
        
        // Calcule les multiplicateurs pour la rareté uniquement
        CalculateMultipliersForRarity();
    }
    
    /// <summary>
    /// Calcule les multiplicateurs pour les options de rareté
    /// Le premier tier (le plus commun) a un multiplicateur de 1.0,
    /// et chaque tier suivant a un multiplicateur multiplié par le ratio
    /// Exemple avec ratio 1.21 et 4 tiers : [1.0, 1.21, 1.46, 1.77]
    /// </summary>
    private void CalculateMultipliersForRarity()
    {
        if (rarityOptions == null || rarityOptions.Length == 0)
            return;

        for (int i = 0; i < rarityOptions.Length; i++)
        {
            // Le premier élément (index 0, le plus commun) a un multiplicateur de 1.0
            // Chaque élément suivant a un multiplicateur = ratio^i
            // Exemple avec 4 tiers et ratio 1.21:
            // Tier 0 (commun): 1.21^0 = 1.00
            // Tier 1: 1.21^1 = 1.21
            // Tier 2: 1.21^2 = 1.46
            // Tier 3 (rare): 1.21^3 = 1.77
            rarityOptions[i].multiplier = Mathf.Pow(rarityMultiplierRatio, i);
        }
    }

    /// <summary>
    /// Calcule les poids pour un tableau d'options
    /// Les poids sont assignés de manière décroissante : le dernier élément (le plus rare) a un poids de 1,
    /// et chaque élément précédent a un poids multiplié par le ratio
    /// </summary>
    private void CalculateWeightsForArray(AttributeOption[] options)
    {
        if (options == null || options.Length == 0)
            return;

        // Le dernier élément (le plus rare) a un poids de 1
        // Les éléments précédents ont des poids croissants selon le ratio inversé
        for (int i = 0; i < options.Length; i++)
        {
            // Index inversé : 0 = le plus commun, length-1 = le plus rare
            int stepsFromEnd = options.Length - 1 - i;
            
            // Calcul : poids = ratio^stepsFromEnd
            // Exemple avec 4 tiers et ratio 1.35:
            // Tier 0 (commun): 1.35^3 = 2.46
            // Tier 1: 1.35^2 = 1.82
            // Tier 2: 1.35^1 = 1.35
            // Tier 3 (rare): 1.35^0 = 1.0
            options[i].weight = Mathf.Pow(rarityRatio, stepsFromEnd);
        }
    }
}