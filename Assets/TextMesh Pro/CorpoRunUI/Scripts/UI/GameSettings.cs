using System;
using UnityEngine;

namespace CorpoRun.UI
{
    // Ustawienia gracza w PlayerPrefs: dźwięk, wibracje, czułość swipe, grafika, język.
    public static class GameSettings
    {
        const string MusicKey = "set_music";
        const string SfxKey = "set_sfx";
        const string VibrationKey = "set_vibration";
        const string SwipeKey = "set_swipe";
        const string GraphicsKey = "set_graphics";
        const string LanguageKey = "set_language";

        // Wywoływane po każdej zmianie ustawień.
        public static event Action Changed;

        public static int Music
        {
            get => PlayerPrefs.GetInt(MusicKey, 70);
            set => SetInt(MusicKey, Mathf.Clamp(value, 0, 100));
        }

        public static int Sfx
        {
            get => PlayerPrefs.GetInt(SfxKey, 85);
            set => SetInt(SfxKey, Mathf.Clamp(value, 0, 100));
        }

        public static bool Vibration
        {
            get => PlayerPrefs.GetInt(VibrationKey, 1) == 1;
            set => SetInt(VibrationKey, value ? 1 : 0);
        }

        // 0 = Low, 1 = Medium, 2 = High
        public static int SwipeSensitivity
        {
            get => PlayerPrefs.GetInt(SwipeKey, 1);
            set => SetInt(SwipeKey, Mathf.Clamp(value, 0, 2));
        }

        // 0 = Low, 1 = Medium, 2 = High
        public static int Graphics
        {
            get => PlayerPrefs.GetInt(GraphicsKey, 1);
            set
            {
                SetInt(GraphicsKey, Mathf.Clamp(value, 0, 2));
                ApplyGraphics();
            }
        }

        // 0 = English, 1 = Polski (na razie tylko zapis wyboru)
        public static int Language
        {
            get => PlayerPrefs.GetInt(LanguageKey, 0);
            set => SetInt(LanguageKey, Mathf.Clamp(value, 0, 1));
        }

        public static float MusicVolume => Music / 100f;
        public static float SfxVolume => Sfx / 100f;

        // Mnożnik progu swipe: Low = trzeba przesunąć palec dalej, High = wystarczy krócej.
        public static float SwipeThresholdMultiplier => SwipeSensitivity == 0 ? 1.4f : SwipeSensitivity == 2 ? 0.7f : 1f;

        public static void ApplyGraphics()
        {
            int count = QualitySettings.names.Length;
            if (count == 0) return;
            int level = Graphics == 0 ? 0 : Graphics == 1 ? (count - 1) / 2 : count - 1;
            QualitySettings.SetQualityLevel(level, true);
        }

        // Zmienia jakość tylko, jeśli gracz już ją kiedyś wybrał (inaczej zostaje ta z projektu).
        public static void ApplySavedGraphics()
        {
            if (PlayerPrefs.HasKey(GraphicsKey)) ApplyGraphics();
        }

        // Wibracja tylko, gdy gracz ma ją włączoną.
        public static void Vibrate()
        {
#if UNITY_ANDROID || UNITY_IOS
            if (Vibration) Handheld.Vibrate();
#endif
        }

        static void SetInt(string key, int value)
        {
            PlayerPrefs.SetInt(key, value);
            PlayerPrefs.Save();
            Changed?.Invoke();
        }
    }
}
