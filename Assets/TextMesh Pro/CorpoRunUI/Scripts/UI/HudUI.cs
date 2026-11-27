using CorpoRun.Shop;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CorpoRun.UI
{
    // HUD w trakcie biegu: wynik, monety, teczki, power-upy, odległość ochrony.
    public class HudUI : MonoBehaviour
    {
        public Button pauseButton;
        public TMP_Text scoreText;
        public TMP_Text scoreShadow;
        public TMP_Text bestText;
        public TMP_Text coinsText;
        public TMP_Text filesText;
        public ShopCatalog catalog;
        public Transform powerUpList;
        public PowerUpTimerUI powerUpTemplate;
        public GameObject securityMeter;
        public RectTransform securityFill;

        public int Score { get; private set; }
        public int Coins { get; private set; }
        public int Files { get; private set; }

        public void ResetRun()
        {
            SetScore(0);
            SetCoins(0);
            SetFiles(0);
            SetSecurity(0f);
            ClearPowerUps();
            bestText.text = "BEST " + ShopFormat.Number(RunStats.Best);
        }

        public void SetScore(int score)
        {
            Score = Mathf.Max(0, score);
            string text = ShopFormat.Number(Score);
            scoreText.text = text;
            if (scoreShadow != null) scoreShadow.text = text;
        }

        public void AddScore(int amount) => SetScore(Score + amount);

        public void SetCoins(int coins)
        {
            Coins = Mathf.Max(0, coins);
            coinsText.text = ShopFormat.Number(Coins);
        }

        public void AddCoins(int amount = 1) => SetCoins(Coins + amount);

        public void SetFiles(int files)
        {
            Files = Mathf.Max(0, files);
            filesText.text = Files.ToString();
        }

        public void AddFiles(int amount = 1) => SetFiles(Files + amount);

        // 0 = ochrona daleko, 1 = zaraz cię złapie.
        public void SetSecurity(float danger)
        {
            securityFill.anchorMax = new Vector2(Mathf.Clamp01(danger), 1f);
        }

        public void ShowSecurityMeter(bool show) => securityMeter.SetActive(show);

        // Pokazuje (albo odnawia) licznik power-upa, np. ShowPowerUp("magnet", 12f).
        public void ShowPowerUp(string powerUpId, float seconds)
        {
            PowerUpDef def = catalog != null ? catalog.GetPowerUp(powerUpId) : null;
            PowerUpTimerUI timer = FindTimer(powerUpId, true) ?? FindTimer(null, false) ?? Instantiate(powerUpTemplate, powerUpList);
            timer.Begin(powerUpId, def != null ? def.icon : null, def != null ? def.displayName : powerUpId, seconds);
        }

        public void HidePowerUp(string powerUpId)
        {
            PowerUpTimerUI timer = FindTimer(powerUpId, true);
            if (timer != null) timer.Stop();
        }

        public void ClearPowerUps()
        {
            foreach (Transform child in powerUpList) child.gameObject.SetActive(false);
        }

        // active = true: szuka działającego licznika o danym id; false: pierwszego wolnego.
        PowerUpTimerUI FindTimer(string id, bool active)
        {
            foreach (Transform child in powerUpList)
            {
                PowerUpTimerUI timer = child.GetComponent<PowerUpTimerUI>();
                if (timer == null || child.gameObject.activeSelf != active) continue;
                if (!active || timer.PowerUpId == id) return timer;
            }
            return null;
        }
    }
}
