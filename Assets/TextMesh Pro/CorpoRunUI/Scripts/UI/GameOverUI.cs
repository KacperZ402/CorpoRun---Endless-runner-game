using System.Collections;
using CorpoRun.Shop;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CorpoRun.UI
{
    // Ekran "złapany": pieczątka GAME OVER, wynik, łup i przyciski.
    public class GameOverUI : MonoBehaviour
    {
        public RectTransform stamp;
        public TMP_Text scoreText;
        public GameObject newBestBadge;
        public TMP_Text previousText;
        public TMP_Text coinsText;
        public TMP_Text filesText;
        public Button continueButton;
        public Button restartButton;
        public Button menuButton;

        public void Show(int score, int previousBest, bool newBest, int coins, int files, bool canContinue)
        {
            scoreText.text = ShopFormat.Number(score);
            newBestBadge.SetActive(newBest);
            previousText.text = newBest
                ? "previous " + ShopFormat.Number(previousBest)
                : "best " + ShopFormat.Number(Mathf.Max(previousBest, score));
            coinsText.text = "+" + ShopFormat.Number(coins);
            filesText.text = files == 1 ? "1 file" : files + " files";
            continueButton.gameObject.SetActive(canContinue);
            gameObject.SetActive(true);
            StartCoroutine(StampIn());
        }

        public void Hide() => gameObject.SetActive(false);

        // Pieczątka "uderza" w ekran.
        IEnumerator StampIn()
        {
            if (stamp == null) yield break;
            for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / 0.22f)
            {
                float k = 1f - (1f - t) * (1f - t);
                stamp.localScale = Vector3.one * Mathf.Lerp(1.8f, 1f, k);
                yield return null;
            }
            stamp.localScale = Vector3.one;
        }
    }
}
