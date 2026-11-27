using System.Collections.Generic;
using UnityEngine;

// Balans ulepszeń power-upów: każde ulepszenie dodaje stały czas, a cena rośnie x1.5 za każdym razem.
// Bez limitu poziomów. Poziomy trzyma zapis (GameSaver).
[CreateAssetMenu(menuName = "Shop/Power-up Upgrades", fileName = "PowerUpUpgrades")]
public class PowerUpUpgrades : ScriptableObject
{
    [System.Serializable]
    public class Entry
    {
        public PowerUpType type;
        public float baseDuration = 10f;      // czas bez ulepszeń
        public float durationPerUpgrade = 1f; // ile sekund dodaje każde ulepszenie
        public int basePrice = 100;           // cena pierwszego ulepszenia
    }


    [Tooltip("O ile rośnie cena każdego kolejnego ulepszenia (2.0 = +100%)")]
    public float priceMultiplier = 2f;

    [Tooltip("Czas dla power-upa, który nie ma wpisu na liście")]
    [SerializeField] private float fallbackDuration = 10f;

    public List<Entry> entries = new List<Entry>();

    public int GetLevel(PowerUpType type)
    {
        return GameSaver.Instance != null ? GameSaver.Instance.GetPowerUpLevel(type) : 0;
    }

    // Aktualny czas trwania (z kupionymi ulepszeniami)
    public float GetDuration(PowerUpType type)
    {
        return GetDuration(type, GetLevel(type));
    }

    // Czas na dowolnym poziomie – np. do pokazania "10 s → 11 s" w UI
    public float GetDuration(PowerUpType type, int level)
    {
        Entry e = Find(type);
        return e != null ? e.baseDuration + level * e.durationPerUpgrade : fallbackDuration;
    }

    // Cena kolejnego ulepszenia
    public int GetNextPrice(PowerUpType type)
    {
        return GetPrice(type, GetLevel(type));
    }

    // Cena wejścia z poziomu "level" na następny: basePrice * multiplier^level
    public int GetPrice(PowerUpType type, int level)
    {
        Entry e = Find(type);
        if (e == null) return int.MaxValue;

        double price = e.basePrice * System.Math.Pow(priceMultiplier, level);
        if (price >= int.MaxValue) return int.MaxValue; // ochrona przed przepełnieniem przy bardzo wysokich poziomach

        return (int)System.Math.Round(price, System.MidpointRounding.AwayFromZero);
    }

    public bool CanAfford(PowerUpType type)
    {
        return GameSaver.Instance != null && GameSaver.Instance.Data.coins >= GetNextPrice(type);
    }

    // Kupno kolejnego ulepszenia. False = za mało monet albo brak wpisu na liście.
    public bool TryBuy(PowerUpType type)
    {
        if (GameSaver.Instance == null || Find(type) == null) return false;
        return GameSaver.Instance.TryBuyPowerUpUpgrade(type, GetNextPrice(type));
    }

    private Entry Find(PowerUpType type)
    {
        foreach (Entry e in entries)
            if (e != null && e.type == type) return e;

        return null;
    }
}