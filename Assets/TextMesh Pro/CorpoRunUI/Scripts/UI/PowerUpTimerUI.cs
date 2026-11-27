using CorpoRun.Shop;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CorpoRun.UI
{
    // Licznik aktywnego power-upa w HUD (kółko, które się opróżnia).
    public class PowerUpTimerUI : MonoBehaviour
    {
        public Image ring;
        public Image icon;
        public TMP_Text nameText;
        public TMP_Text timeText;

        float duration = 1f;
        float left;

        public string PowerUpId { get; private set; }

        public void Begin(string id, Sprite sprite, string displayName, float seconds)
        {
            PowerUpId = id;
            duration = Mathf.Max(0.01f, seconds);
            left = seconds;
            icon.sprite = sprite;
            icon.enabled = sprite != null;
            nameText.text = displayName.ToUpperInvariant();
            ring.color = ColorFor(id);
            gameObject.SetActive(true);
            UpdateView();
        }

        public void Stop() => gameObject.SetActive(false);

        void Update()
        {
            left -= Time.deltaTime;
            if (left <= 0f)
            {
                Stop();
                return;
            }
            UpdateView();
        }

        void UpdateView()
        {
            ring.fillAmount = Mathf.Clamp01(left / duration);
            timeText.text = Mathf.CeilToInt(Mathf.Max(left, 0f)) + " s";
        }

        public static Color ColorFor(string id)
        {
            switch (id)
            {
                case "magnet": return ShopColors.Hex("5CC8FF");
                case "coins2": return ShopColors.Hex("FFD15C");
                case "shield": return ShopColors.Hex("5BE38A");
                case "headstart": return ShopColors.Hex("C08BFF");
                default: return Color.white;
            }
        }
    }
}
