# 🎮 Système d'Upgrade - Guide d'utilisation

## 📋 Vue d'ensemble

Le système d'upgrade permet d'améliorer les probabilités d'obtenir de meilleures raretés pour :
- **Molder** (Normal, Bronze, Silver, Gold, etc.)
- **Quality** (Broken, Rough, Bad, Okay, etc.)
- **Class** (Regular, Nice, Cool, Strong, etc.)
- **Rarity** (Basic, Common, Uncommon, Rare, etc.)

## 🔧 Installation

### 1. Dans la scène Unity

1. Créez un GameObject vide nommé `"UpgradeSystem"`
2. Ajoutez le component `UpgradeSystem`
3. Le système deviendra automatiquement singleton (DontDestroyOnLoad)

### 2. Configuration de l'UI

1. Créez un Canvas avec un Panel pour l'interface d'upgrade
2. Pour chaque catégorie (Molder, Quality, Class, Rarity), créez :
   - Un `TextMeshPro` pour afficher le niveau actuel
   - Un `TextMeshPro` pour afficher le coût
   - Un `Button` pour upgrade
3. Créez un GameObject avec le component `UpgradeUI`
4. Assignez les références dans l'inspecteur

## 🎯 Fonctionnement

### Formule de progression

Basé sur la progression : **1/1310 → 1/1264 → 1/1220 → 1/1180...**

- Niveau 0 : Aucun bonus
- Niveau 1 : ~3.6% de chance en plus pour les raretés élevées
- Niveau 10 : ~37% de chance en plus
- Niveau 50 : ~5x plus de chances
- Niveau 100 : ~10x plus de chances

### Coût par niveau

```csharp
Coût = BaseCost × (1.15 ^ Niveau)
```

**Coûts de base :**
- Molder : $100
- Quality : $150
- Class : $200
- Rarity : $250

**Exemples :**
- Molder niveau 1 → 10 : ~$100 → $2,759
- Molder niveau 50 : $108,366
- Molder niveau 100 : $1,174,313

### Redistribution des probabilités

Le bonus s'applique **proportionnellement** aux raretés :
- Les raretés **communes** (début de liste) : peu/pas de bonus
- Les raretés **moyennes** : bonus modéré
- Les raretés **légendaires** (fin de liste) : bonus maximum

**Exemple avec Molder niveau 10 :**
```
Sans upgrade:
- Normal: 100% → 95% (-5%)
- Bronze: 10%  → 10.5% (+5%)
- Silver: 1%   → 1.5% (+50%)
- Gold: 0.1%   → 0.2% (+100%)

Avec upgrade niveau 10:
- Normal: 95%
- Bronze: 10.5%
- Silver: 1.5%
- Gold: 0.2%
```

## 🎮 Utilisation In-Game

### Ouvrir le menu d'upgrade
- Appuyez sur **U** (configurable dans UpgradeUI)
- Le curseur se déverrouille automatiquement

### Améliorer une catégorie
1. Vérifiez que vous avez assez d'argent
2. Cliquez sur le bouton "Upgrade"
3. Le niveau augmente immédiatement
4. Les nouvelles épées générées bénéficient du bonus

## 🔨 Personnalisation

### Modifier les coûts de base

Dans `UpgradeSystem.cs` :
```csharp
[SerializeField] private UpgradeCategory molder = new UpgradeCategory 
{ 
    name = "Molder", 
    baseCost = 100f,           // ← Changez ici
    costMultiplier = 1.15f     // ← Ou ici pour la progression
};
```

### Modifier le niveau maximum

```csharp
public int maxLevel = 100;  // ← Changez ici (dans UpgradeCategory)
```

### Modifier la formule de luck

Dans `UpgradeSystem.GetLuckMultiplier()` :
```csharp
float reductionFactor = 0.965f + (i * 0.0001f);  // ← Ajustez cette formule
```

## 📊 Debug & Tests

### Afficher les probabilités modifiées

Dans `SwordAssigner.cs`, décommentez :
```csharp
Debug.Log(ProbabilityHelper.GenerateWeightReport(modifiedOptions, opt => opt.weight, opt => opt.name));
```

### Tester rapidement

Ajoutez dans `UpgradeUI.Start()` :
```csharp
// TEST: Commencer avec 1M d'argent
playerMoney.SetMoney(1000000f);

// TEST: Molder déjà niveau 10
upgradeSystem.Molder.currentLevel = 10;
```

## ⚠️ Notes importantes

1. **Les upgrades sont permanents** - Sauvegardez-les si vous voulez la persistance
2. **Les bonus ne s'appliquent qu'aux nouvelles épées** - Pas rétroactif
3. **Le système est découplé** - Fonctionne sans modifier les configs existantes
4. **Thread-safe** - Le système utilise des copies des arrays d'options

## 🚀 Améliorations futures possibles

- Sauvegarder les niveaux (PlayerPrefs ou fichier JSON)
- Animations lors des upgrades
- Particules visuelles pour montrer le bonus actif
- Stats pour voir le % d'amélioration en temps réel
- Upgrades par paliers (débloque de nouvelles raretés)
