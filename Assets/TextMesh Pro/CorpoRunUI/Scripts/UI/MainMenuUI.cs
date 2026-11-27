using CorpoRun.Shop;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CorpoRun.UI
{
    // Menu główne: monety, rekord, przyciski i (opcjonalnie) misja dnia.
    public class MainMenuUI : MonoBehaviour
    {
        public TMP_Text coinsText;
        public TMP_Text bestText;
        public Button playButton;
        public Button shopButton;
        public Button optionsButton;
        public TMP_Text versionText;

        [Header("Misja dnia (opcjonalnie)")]
        public GameObject missionCard;
        public TMP_Text missionTitle;
        public TMP_Text missionTimer;
        public RectTransform missionFill;
        public TMP_Text missionProgress;
        public TMP_Text missionReward;

        void OnEnable()
        {
            ShopSave.Changed += Refresh;
            Refresh();
        }

        void OnDisable() => ShopSave.Changed -= Refresh;

        public void Refresh()
        {
            coinsText.text = ShopFormat.Number(ShopSave.Coins);
            bestText.text = ShopFormat.Number(RunStats.Best);
            if (versionText != null) versionText.text = "BeetleBit Studio · v" + Application.version;
        }

        public void SetDailyMission(string title, int current, int target, int reward, string endsIn)
        {
            missionCard.SetActive(true);
            missionTitle.text = title;
            missionTimer.text = "ends in " + endsIn;
            float progress = target > 0 ? Mathf.Clamp01((float)current / target) : 0f;
            missionFill.anchorMax = new Vector2(progress, 1f);
            missionProgress.text = current + " / " + target;
            missionReward.text = "Reward: " + ShopFormat.Number(reward) + " coins";
        }

        public void HideDailyMission() => missionCard.SetActive(false);
    }
}
