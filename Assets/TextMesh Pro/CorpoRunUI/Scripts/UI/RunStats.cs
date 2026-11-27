using UnityEngine;

namespace CorpoRun.UI
{
    // Rekord gracza (najlepszy wynik).
    public static class RunStats
    {
        const string BestKey = "best_score";

        public static int Best => PlayerPrefs.GetInt(BestKey, 0);

        // Zapisuje wynik. Zwraca true, gdy to nowy rekord.
        public static bool Submit(int score, out int previousBest)
        {
            previousBest = Best;
            if (score <= previousBest) return false;
            PlayerPrefs.SetInt(BestKey, score);
            PlayerPrefs.Save();
            return true;
        }
    }
}
