using System;
using System.Collections.Generic;
using UnityEngine;

namespace CorpoRun.Shop
{
    public enum SkinRarity { Common, Rare, Epic, Legendary }

    [Serializable]
    public class PowerUpDef
    {
        public string id = "magnet";
        public string displayName = "Magnet";
        [TextArea(2, 3)] public string description = "";
        public Sprite icon;
        public string statLabel = "Duration";
        public string unit = "s";
        [Tooltip("Wartość na kolejnych poziomach, od poziomu 1.")]
        public float[] values = { 10f, 12f, 15f, 18f, 22f };

        public int MaxLevel => values == null ? 1 : Mathf.Max(1, values.Length);

        public float ValueAt(int level)
        {
            if (values == null || values.Length == 0) return 0f;
            return values[Mathf.Clamp(level, 1, values.Length) - 1];
        }
    }

    [Serializable]
    public class SkinDef
    {
        public string id = "classic";
        public string displayName = "Classic";
        public SkinRarity rarity = SkinRarity.Common;
        [TextArea(2, 3)] public string description = "";
        [Tooltip("0 = skin dostępny od początku.")]
        public int cost;
        public Sprite preview;
    }

    // Wszystkie dane sklepu w jednym pliku: ceny, wartości power-upów, skiny.
    [CreateAssetMenu(fileName = "ShopCatalog", menuName = "CorpoRun/Shop Catalog")]
    public class ShopCatalog : ScriptableObject
    {
        [Tooltip("Koszt ulepszenia: poziom 1→2, 2→3, 3→4, 4→5.")]
        public int[] upgradeCosts = { 250, 500, 1000, 2000 };
        public List<PowerUpDef> powerUps = new List<PowerUpDef>();
        public List<SkinDef> skins = new List<SkinDef>();

        public PowerUpDef GetPowerUp(string id) => powerUps.Find(p => p.id == id);

        public SkinDef GetSkin(string id) => skins.Find(s => s.id == id);

        // Aktualna wartość power-upa wg zapisanego poziomu, np. czas magnesu w sekundach.
        public float CurrentValue(string powerUpId)
        {
            PowerUpDef p = GetPowerUp(powerUpId);
            return p == null ? 0f : p.ValueAt(ShopSave.GetLevel(powerUpId));
        }

        // Koszt ulepszenia z podanego poziomu albo -1, gdy power-up ma już maksymalny poziom.
        public int UpgradeCost(PowerUpDef p, int level)
        {
            if (p == null || level >= p.MaxLevel) return -1;
            int i = level - 1;
            return i >= 0 && i < upgradeCosts.Length ? upgradeCosts[i] : -1;
        }

        public bool IsOwned(SkinDef s) => s != null && (s.cost <= 0 || ShopSave.IsOwned(s.id));
    }
}
