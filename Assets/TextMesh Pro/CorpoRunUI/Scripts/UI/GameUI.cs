using CorpoRun.Shop;
using UnityEngine;
using UnityEngine.Events;

namespace CorpoRun.UI
{
    // Steruje ekranami: menu → bieg (HUD) → pauza / koniec → menu.
    // Gra woła np. GameUI.Instance.Hud.AddCoins(1) i GameUI.Instance.GameOver().
    public class GameUI : MonoBehaviour
    {
        public static GameUI Instance { get; private set; }

        [Header("Ekrany")]
        public MainMenuUI menu;
        public HudUI hud;
        public PauseUI pause;
        public GameOverUI gameOver;
        public OptionsUI options;
        public ShopUI shop;

        [Header("Zdarzenia dla gry (podepnij w Inspectorze)")]
        public UnityEvent onPlay = new UnityEvent();
        public UnityEvent onRestart = new UnityEvent();
        public UnityEvent onContinue = new UnityEvent();
        public UnityEvent onQuitToMenu = new UnityEvent();
        public UnityEvent onPause = new UnityEvent();
        public UnityEvent onResume = new UnityEvent();

        [Tooltip("Pauza zatrzymuje czas gry (Time.timeScale = 0).")]
        public bool freezeTimeOnPause = true;

        int bankedCoins;

        public HudUI Hud => hud;
        public bool IsPlaying { get; private set; }
        public bool IsPaused => pause.gameObject.activeSelf;

        void Awake()
        {
            Instance = this;
            menu.playButton.onClick.AddListener(Play);
            menu.shopButton.onClick.AddListener(OpenShop);
            menu.optionsButton.onClick.AddListener(OpenOptions);
            hud.pauseButton.onClick.AddListener(Pause);
            pause.resumeButton.onClick.AddListener(Resume);
            pause.restartButton.onClick.AddListener(Restart);
            pause.optionsButton.onClick.AddListener(OpenOptions);
            pause.quitButton.onClick.AddListener(QuitToMenu);
            gameOver.continueButton.onClick.AddListener(Continue);
            gameOver.restartButton.onClick.AddListener(Restart);
            gameOver.menuButton.onClick.AddListener(QuitToMenu);
        }

        void Start()
        {
            GameSettings.ApplySavedGraphics();
            pause.Hide();
            gameOver.Hide();
            options.gameObject.SetActive(false);
            ShowScreens(true, false);
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        // Telefon zminimalizował grę w trakcie biegu → automatyczna pauza.
        void OnApplicationPause(bool paused)
        {
            if (paused && IsPlaying && !IsPaused) Pause();
        }

        public void Play()
        {
            StartRun();
            onPlay.Invoke();
        }

        public void Pause()
        {
            if (!IsPlaying || IsPaused) return;
            pause.Show(hud.Score, hud.Coins, hud.Files);
            if (freezeTimeOnPause) Time.timeScale = 0f;
            onPause.Invoke();
        }

        public void Resume()
        {
            pause.Hide();
            Time.timeScale = 1f;
            onResume.Invoke();
        }

        public void Restart()
        {
            BankCoins();
            StartRun();
            onRestart.Invoke();
        }

        // Wołaj z gry, gdy ochrona złapie gracza. canContinue = pokaż przycisk CONTINUE (reklama).
        public void GameOver(bool canContinue = false)
        {
            if (!IsPlaying) return;
            IsPlaying = false;
            pause.Hide();
            BankCoins();
            bool newBest = RunStats.Submit(hud.Score, out int previousBest);
            gameOver.Show(hud.Score, previousBest, newBest, hud.Coins, hud.Files, canContinue);
        }

        // Po obejrzeniu reklamy: bieg toczy się dalej z tym samym wynikiem.
        public void Continue()
        {
            gameOver.Hide();
            IsPlaying = true;
            onContinue.Invoke();
        }

        public void QuitToMenu()
        {
            if (IsPlaying) BankCoins();
            IsPlaying = false;
            Time.timeScale = 1f;
            pause.Hide();
            gameOver.Hide();
            options.gameObject.SetActive(false);
            ShowScreens(true, false);
            menu.Refresh();
            onQuitToMenu.Invoke();
        }

        public void OpenOptions() => options.Open();

        public void OpenShop()
        {
            if (shop != null) shop.Open();
        }

        void StartRun()
        {
            Time.timeScale = 1f;
            pause.Hide();
            gameOver.Hide();
            options.gameObject.SetActive(false);
            ShowScreens(false, true);
            hud.ResetRun();
            bankedCoins = 0;
            IsPlaying = true;
        }

        // Przenosi monety z biegu do portfela (tylko te jeszcze nieprzeniesione).
        void BankCoins()
        {
            int fresh = hud.Coins - bankedCoins;
            if (fresh > 0) ShopSave.AddCoins(fresh);
            bankedCoins = hud.Coins;
        }

        void ShowScreens(bool showMenu, bool showHud)
        {
            menu.gameObject.SetActive(showMenu);
            hud.gameObject.SetActive(showHud);
        }
    }
}
