using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CorpoRun.UI
{
    // Opcje: głośność, wibracje, czułość swipe, grafika, język. Zapis w GameSettings.
    public class OptionsUI : MonoBehaviour
    {
        public Button backButton;
        public Slider musicSlider;
        public TMP_Text musicValue;
        public Slider sfxSlider;
        public TMP_Text sfxValue;
        public SwitchUI vibration;
        public SegmentedUI swipe;
        public SegmentedUI graphics;
        public SegmentedUI language;
        public Button restoreButton;
        public Button privacyButton;
        public Button creditsButton;
        public TMP_Text versionText;

        [Tooltip("Adres polityki prywatności (otwiera się w przeglądarce).")]
        public string privacyPolicyUrl = "";

        public UnityEvent onRestorePurchases = new UnityEvent();
        public UnityEvent onCredits = new UnityEvent();
        public UnityEvent onClosed = new UnityEvent();

        void Awake()
        {
            backButton.onClick.AddListener(Close);
            musicSlider.onValueChanged.AddListener(value =>
            {
                GameSettings.Music = Mathf.RoundToInt(value);
                musicValue.text = GameSettings.Music.ToString();
            });
            sfxSlider.onValueChanged.AddListener(value =>
            {
                GameSettings.Sfx = Mathf.RoundToInt(value);
                sfxValue.text = GameSettings.Sfx.ToString();
            });
            vibration.onValueChanged.AddListener(on =>
            {
                GameSettings.Vibration = on;
                if (on) GameSettings.Vibrate();
            });
            swipe.onValueChanged.AddListener(index => GameSettings.SwipeSensitivity = index);
            graphics.onValueChanged.AddListener(index => GameSettings.Graphics = index);
            language.onValueChanged.AddListener(index => GameSettings.Language = index);
            restoreButton.onClick.AddListener(() => onRestorePurchases.Invoke());
            privacyButton.onClick.AddListener(OpenPrivacyPolicy);
            creditsButton.onClick.AddListener(() => onCredits.Invoke());
        }

        void OnEnable() => Load();

        public void Open() => gameObject.SetActive(true);

        public void Close()
        {
            gameObject.SetActive(false);
            onClosed.Invoke();
        }

        void Load()
        {
            musicSlider.SetValueWithoutNotify(GameSettings.Music);
            musicValue.text = GameSettings.Music.ToString();
            sfxSlider.SetValueWithoutNotify(GameSettings.Sfx);
            sfxValue.text = GameSettings.Sfx.ToString();
            vibration.Set(GameSettings.Vibration, false);
            swipe.Select(GameSettings.SwipeSensitivity, false);
            graphics.Select(GameSettings.Graphics, false);
            language.Select(GameSettings.Language, false);
            if (versionText != null) versionText.text = "BeetleBit Studio · v" + Application.version;
        }

        void OpenPrivacyPolicy()
        {
            if (!string.IsNullOrEmpty(privacyPolicyUrl)) Application.OpenURL(privacyPolicyUrl);
        }
    }
}
