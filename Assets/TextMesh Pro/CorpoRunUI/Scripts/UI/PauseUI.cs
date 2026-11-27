using CorpoRun.Shop;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CorpoRun.UI
{
    // Okno pauzy z podsumowaniem biegu.
    public class PauseUI : MonoBehaviour
    {
        public TMP_Text scoreText;
        public TMP_Text coinsText;
        public TMP_Text filesText;
        public Button resumeButton;
        public Button restartButton;
        public Button optionsButton;
        public Button quitButton;

        public void Show(int score, int coins, int files)
        {
            scoreText.text = ShopFormat.Number(score);
            coinsText.text = ShopFormat.Number(coins);
            filesText.text = files.ToString();
            gameObject.SetActive(true);
        }

        public void Hide() => gameObject.SetActive(false);
    }
}
