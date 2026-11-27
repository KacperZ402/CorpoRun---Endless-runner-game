using System;
using UnityEngine;

namespace CorpoRun.Shop
{
    // Zapis sklepu w PlayerPrefs: monety, poziomy ulepszeń, kupione i założony skin.
    public static class ShopSave
    {
        // Jeśli Twoja gra już zapisuje monety pod innym kluczem, wpisz go tutaj.
        public const string CoinsKey = "coins";

        const string LevelPrefix = "upgrade_";
        const string OwnedPrefix = "skin_owned_";
        const string EquippedKey = "skin_equipped";

        // Wywoływane po każdej zmianie (monety, poziom, skin) - sklep i HUD mogą się odświeżyć.
        public static event Action Changed;

        public static int Coins => PlayerPrefs.GetInt(CoinsKey, 0);

        public static void AddCoins(int amount)
        {
            if (amount <= 0) return;
            PlayerPrefs.SetInt(CoinsKey, Coins + amount);
            Save();
        }

        public static bool TrySpend(int amount)
        {
            if (amount < 0 || Coins < amount) return false;
            PlayerPrefs.SetInt(CoinsKey, Coins - amount);
            Save();
            return true;
        }

        public static int GetLevel(string powerUpId) => Mathf.Max(1, PlayerPrefs.GetInt(LevelPrefix + powerUpId, 1));

        public static void SetLevel(string powerUpId, int level)
        {
            PlayerPrefs.SetInt(LevelPrefix + powerUpId, Mathf.Max(1, level));
            Save();
        }

        public static bool IsOwned(string skinId) => PlayerPrefs.GetInt(OwnedPrefix + skinId, 0) == 1;

        public static void SetOwned(string skinId)
        {
            PlayerPrefs.SetInt(OwnedPrefix + skinId, 1);
            Save();
        }

        public static string EquippedSkin => PlayerPrefs.GetString(EquippedKey, "classic");

        public static void Equip(string skinId)
        {
            PlayerPrefs.SetString(EquippedKey, skinId);
            Save();
        }

        // Tylko do testów: zeruje monety, poziomy i kupione skiny z katalogu.
        public static void ResetForTesting(ShopCatalog catalog)
        {
            PlayerPrefs.DeleteKey(CoinsKey);
            PlayerPrefs.DeleteKey(EquippedKey);
            if (catalog != null)
            {
                foreach (PowerUpDef p in catalog.powerUps) PlayerPrefs.DeleteKey(LevelPrefix + p.id);
                foreach (SkinDef s in catalog.skins) PlayerPrefs.DeleteKey(OwnedPrefix + s.id);
            }
            Save();
        }

        static void Save()
        {
            PlayerPrefs.Save();
            Changed?.Invoke();
        }
    }
}
