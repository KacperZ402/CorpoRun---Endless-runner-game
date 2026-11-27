using CorpoRun.Shop;
using CorpoRun.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using K = CorpoRun.EditorTools.UIKit;

namespace CorpoRun.EditorTools
{
    // Górne menu Unity: CorpoRun → Zbuduj interfejs gry.
    // Tworzy GameCanvas (menu, HUD, pauza, koniec gry, opcje) i ShopCanvas (sklep), wszystko podpięte.
    public static class GameUIBuilder
    {
        static readonly Color Soft = new Color(0.788f, 0.824f, 0.855f, 1f); // #C9D2DA

        [MenuItem("CorpoRun/Zbuduj interfejs gry")]
        public static void Build()
        {
            if (!K.Init()) return;

            ShopUI shop = K.FindInScene<ShopUI>();
            if (shop == null) shop = ShopUIBuilder.Create();
            shop.gameObject.SetActive(false);

            K.EnsureEventSystem();
            Transform canvas = K.CreateCanvas("GameCanvas", 40);
            RectTransform rootRt = K.Node("GameUI", canvas);
            K.Stretch(rootRt);
            GameUI ui = rootRt.gameObject.AddComponent<GameUI>();
            ui.shop = shop;
            ui.hud = BuildHud(rootRt, shop.catalog);
            ui.menu = BuildMenu(rootRt);
            ui.pause = BuildPause(rootRt);
            ui.gameOver = BuildGameOver(rootRt);
            ui.options = BuildOptions(rootRt);

            ui.hud.gameObject.SetActive(false);
            ui.pause.gameObject.SetActive(false);
            ui.gameOver.gameObject.SetActive(false);
            ui.options.gameObject.SetActive(false);
            ui.menu.Refresh();

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Selection.activeGameObject = ui.gameObject;
            Debug.Log("CorpoRun: interfejs gotowy (GameCanvas → GameUI, sklep w ShopCanvas).");
        }

        // ------------------------------------------------------------------ menu główne

        static MainMenuUI BuildMenu(RectTransform parent)
        {
            RectTransform rt = K.Node("MainMenu", parent);
            K.Stretch(rt);
            Image shade = rt.gameObject.AddComponent<Image>();
            shade.sprite = K.GradientH;
            shade.color = new Color(0.039f, 0.055f, 0.094f, 1f);
            MainMenuUI menu = rt.gameObject.AddComponent<MainMenuUI>();

            // prawy górny róg: monety i rekord
            RectTransform top = K.Place(K.Node("TopRight", rt), new Vector2(1f, 1f), new Vector2(-32f, -28f), new Vector2(320f, 48f));
            K.Row(top, 12f, TextAnchor.MiddleRight);
            K.FitWidth(top);
            RectTransform coins = Pill(top, "Coins", 48f, 8, 18);
            K.Fixed(K.IconImage(coins, "Coin", K.Coin, Color.white), 32f, 32f);
            menu.coinsText = K.Txt(K.Node("Amount", coins), "0", K.Bold, 20f, Color.white, TextAlignmentOptions.Left);
            RectTransform best = Pill(top, "Best", 48f, 18, 18);
            K.Fixed(K.IconImage(best, "Trophy", K.Trophy, ShopColors.Gold), 22f, 22f);
            K.Txt(K.Node("Label", best), "BEST", K.Semi, 14f, Soft, TextAlignmentOptions.Left);
            menu.bestText = K.Txt(K.Node("Amount", best), "0", K.Bold, 20f, Color.white, TextAlignmentOptions.Left);

            // logo i hasło
            float logoHeight = 92f;
            float logoWidth = K.Logo != null ? logoHeight * K.Logo.rect.width / K.Logo.rect.height : 520f;
            Image logo = K.IconImage(rt, "Logo", K.Logo, Color.white);
            K.Place(logo.rectTransform, new Vector2(0f, 1f), new Vector2(88f, -120f), new Vector2(logoWidth, logoHeight));
            K.Txt(K.Place(K.Node("Tagline", rt), new Vector2(0f, 1f), new Vector2(88f, -226f), new Vector2(640f, 26f)),
                "Steal the files. Outrun security. Don't look back.", K.Semi, 18f, Soft, TextAlignmentOptions.Left, 2.8f);

            // przyciski
            RectTransform play = K.Place(K.Node("PlayButton", rt), new Vector2(0f, 1f), new Vector2(88f, -298f), new Vector2(380f, 88f));
            menu.playButton = K.GoldButton(play, "PLAY", K.Play, new Vector2(30f, 34f), 38f, 20f);
            RectTransform row = K.Place(K.Node("MoreButtons", rt), new Vector2(0f, 1f), new Vector2(88f, -400f), new Vector2(380f, 64f));
            K.Row(row, 14f, TextAnchor.MiddleCenter).childForceExpandWidth = true;
            menu.shopButton = MenuButton(row, "SHOP", K.Cart);
            menu.optionsButton = MenuButton(row, "OPTIONS", K.Gear);

            // misja dnia (wzór - podłącz przez SetDailyMission albo wyłącz obiekt)
            RectTransform card = K.Place(K.Node("DailyMission", rt), new Vector2(1f, 0f), new Vector2(-32f, 32f), new Vector2(320f, 130f));
            K.Box(card, new Color(0.078f, 0.094f, 0.141f, 0.82f), new Color(1f, 1f, 1f, 0.15f), 18f, K.Border2);
            K.Column(card, 10f, TextAnchor.UpperLeft, 20, 20, 18, 18).childForceExpandWidth = true;
            K.FitHeight(card);
            RectTransform head = K.Node("Header", card);
            K.Row(head, 0f, TextAnchor.MiddleLeft);
            K.Txt(K.Node("Label", head), "DAILY MISSION", K.Bold, 12f, ShopColors.Gold, TextAlignmentOptions.Left, 25f);
            K.Spacer(head);
            menu.missionTimer = K.Txt(K.Node("Timer", head), "ends in 06:12:40", K.Semi, 12f, ShopColors.Grey, TextAlignmentOptions.Right);
            menu.missionTitle = K.Txt(K.Node("Title", card), "Steal 5 secret files in one run", K.Bold, 17f, Color.white, TextAlignmentOptions.Left);
            RectTransform bar = K.Node("Progress", card);
            K.Height(bar, 10f);
            K.Rounded(bar, K.Panel, new Color(1f, 1f, 1f, 0.14f), 5f);
            menu.missionFill = Fill(bar, 0.6f, ShopColors.Gold, 5f);
            RectTransform foot = K.Node("Footer", card);
            K.Row(foot, 0f, TextAnchor.MiddleLeft);
            menu.missionProgress = K.Txt(K.Node("Count", foot), "3 / 5", K.Semi, 13f, Soft, TextAlignmentOptions.Left);
            K.Spacer(foot);
            menu.missionReward = K.Txt(K.Node("Reward", foot), "Reward: 200 coins", K.Semi, 13f, Soft, TextAlignmentOptions.Right);
            menu.missionCard = card.gameObject;

            menu.versionText = K.Txt(K.Place(K.Node("Version", rt), new Vector2(0f, 0f), new Vector2(88f, 28f), new Vector2(400f, 18f)),
                "BeetleBit Studio · v0.1", K.Semi, 12f, ShopColors.Dim, TextAlignmentOptions.Left, 8f);
            return menu;
        }

        static RectTransform Pill(RectTransform parent, string name, float height, int padLeft, int padRight)
        {
            RectTransform rt = K.Node(name, parent);
            K.Box(rt, new Color(0.078f, 0.094f, 0.141f, 0.8f), new Color(1f, 1f, 1f, 0.15f), height / 2f, K.Border2);
            K.Row(rt, 10f, TextAnchor.MiddleLeft, padLeft, padRight);
            K.Height(rt, height);
            return rt;
        }

        static Button MenuButton(RectTransform parent, string label, Sprite icon)
        {
            RectTransform rt = K.Node(label + "Button", parent);
            K.Flexible(K.Height(rt, 64f));
            Image background = K.Box(rt, new Color(0.078f, 0.094f, 0.141f, 0.75f), new Color(1f, 1f, 1f, 0.22f), 16f, K.Border2);
            Button button = K.MakeButton(background);
            K.Row(rt, 10f, TextAnchor.MiddleCenter);
            K.Fixed(K.IconImage(rt, "Icon", icon, Color.white), 22f, 22f);
            K.Txt(K.Node("Label", rt), label, K.Bold, 20f, Color.white, TextAlignmentOptions.Center);
            return button;
        }

        // Pasek wypełnienia rozciągany kotwicami (szerokość = anchorMax.x).
        static RectTransform Fill(RectTransform track, float amount, Color color, float radius)
        {
            RectTransform fill = K.Node("Fill", track);
            fill.anchorMin = Vector2.zero;
            fill.anchorMax = new Vector2(amount, 1f);
            fill.offsetMin = Vector2.zero;
            fill.offsetMax = Vector2.zero;
            K.Rounded(fill, K.Panel, color, radius);
            return fill;
        }

        // ------------------------------------------------------------------ HUD

        static HudUI BuildHud(RectTransform parent, ShopCatalog catalog)
        {
            RectTransform rt = K.Node("HUD", parent);
            K.Stretch(rt);
            HudUI hud = rt.gameObject.AddComponent<HudUI>();
            hud.catalog = catalog;

            // pauza
            RectTransform pauseRt = K.Place(K.Node("PauseButton", rt), new Vector2(0f, 1f), new Vector2(28f, -24f), new Vector2(64f, 64f));
            hud.pauseButton = K.MakeButton(K.Box(pauseRt, K.HudFill, new Color(1f, 1f, 1f, 0.22f), 20f, K.Border2));
            RectTransform bars = K.Place(K.Node("Bars", pauseRt), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(24f, 26f));
            K.Row(bars, 8f, TextAnchor.MiddleCenter);
            for (int i = 0; i < 2; i++) K.Fixed(K.Rounded(K.Node("Bar", bars), K.Panel, Color.white, 3f), 8f, 26f);

            // wynik na środku
            RectTransform score = K.Place(K.Node("Score", rt), new Vector2(0.5f, 1f), new Vector2(0f, -14f), new Vector2(500f, 72f));
            RectTransform shadow = K.Node("Shadow", score);
            K.Stretch(shadow);
            shadow.offsetMin = new Vector2(0f, -4f);
            shadow.offsetMax = new Vector2(0f, -4f);
            hud.scoreShadow = K.Txt(shadow, "0", K.Display, 64f, new Color(0.039f, 0.055f, 0.094f, 0.6f), TextAlignmentOptions.Center);
            RectTransform value = K.Node("Value", score);
            K.Stretch(value);
            hud.scoreText = K.Txt(value, "0", K.Display, 64f, Color.white, TextAlignmentOptions.Center);
            hud.bestText = K.Txt(K.Place(K.Node("Best", rt), new Vector2(0.5f, 1f), new Vector2(0f, -86f), new Vector2(500f, 20f)),
                "BEST 0", K.Bold, 13f, ShopColors.Light, TextAlignmentOptions.Center, 30f);

            // prawy górny róg: monety i teczki
            RectTransform right = K.Place(K.Node("TopRight", rt), new Vector2(1f, 1f), new Vector2(-28f, -24f), new Vector2(200f, 106f));
            K.Column(right, 10f, TextAnchor.UpperRight);
            K.Fit(right);
            RectTransform coinPill = K.Node("Coins", right);
            K.Box(coinPill, K.HudFill, new Color(1f, 1f, 1f, 0.18f), 26f, K.Border2);
            K.Row(coinPill, 10f, TextAnchor.MiddleLeft, 8, 20);
            K.Height(coinPill, 52f);
            K.Fixed(K.IconImage(coinPill, "Coin", K.Coin, Color.white), 36f, 36f);
            hud.coinsText = K.Txt(K.Node("Amount", coinPill), "0", K.Display, 24f, Color.white, TextAlignmentOptions.Left);
            RectTransform filesPill = K.Node("Files", right);
            K.Box(filesPill, K.HudFill, new Color(1f, 1f, 1f, 0.18f), 22f, K.Border2);
            K.Row(filesPill, 10f, TextAnchor.MiddleLeft, 12, 16);
            K.Height(filesPill, 44f);
            K.Fixed(K.IconImage(filesPill, "Icon", K.Folder, Color.white), 24f, 22f);
            hud.filesText = K.Txt(K.Node("Amount", filesPill), "0", K.Bold, 18f, Color.white, TextAlignmentOptions.Left);
            K.Txt(K.Node("Label", filesPill), "FILES", K.Semi, 12f, Soft, TextAlignmentOptions.Left);

            // lewy dolny róg: aktywne power-upy
            RectTransform list = K.Place(K.Node("PowerUps", rt), new Vector2(0f, 0f), new Vector2(28f, 28f), new Vector2(760f, 72f));
            K.Row(list, 20f, TextAnchor.LowerLeft);
            hud.powerUpList = list;
            hud.powerUpTemplate = PowerUpTimer(list);
            hud.powerUpTemplate.gameObject.SetActive(false);

            // dół środek: jak blisko jest ochrona
            RectTransform security = K.Place(K.Node("SecurityMeter", rt), new Vector2(0.5f, 0f), new Vector2(0f, 26f), new Vector2(400f, 52f));
            K.Box(security, K.HudFill, new Color(0.878f, 0.196f, 0.169f, 0.6f), 16f, K.Border2);
            K.Row(security, 12f, TextAnchor.MiddleLeft, 16, 16, 10, 10);
            K.Fixed(K.IconImage(security, "Icon", K.Guard, ShopColors.Hex("FF6B5E")), 28f, 28f);
            K.Txt(K.Node("Label", security), "SECURITY", K.Bold, 12f, ShopColors.Hex("FF8A80"), TextAlignmentOptions.Left, 16f);
            RectTransform track = K.Node("Track", security);
            K.Flexible(K.Height(track, 10f));
            K.Rounded(track, K.Panel, new Color(1f, 1f, 1f, 0.14f), 5f);
            hud.securityFill = Fill(track, 0f, ShopColors.Hex("E0322B"), 5f);
            hud.securityMeter = security.gameObject;
            return hud;
        }

        static PowerUpTimerUI PowerUpTimer(RectTransform parent)
        {
            RectTransform rt = K.Node("PowerUpTimer", parent);
            K.Row(rt, 12f, TextAnchor.MiddleLeft);
            PowerUpTimerUI timer = rt.gameObject.AddComponent<PowerUpTimerUI>();

            RectTransform dial = K.Node("Dial", rt);
            K.Fixed(dial, 72f, 72f);
            K.Centered(dial, "Back", K.Disc, new Color(0.078f, 0.094f, 0.141f, 0.8f), 66f);
            Image track = K.IconImage(dial, "Track", K.Ring, new Color(1f, 1f, 1f, 0.15f));
            K.Stretch(track.rectTransform);
            Image ring = K.IconImage(dial, "Ring", K.Ring, PowerUpTimerUI.ColorFor("magnet"));
            K.Stretch(ring.rectTransform);
            ring.type = Image.Type.Filled;
            ring.fillMethod = Image.FillMethod.Radial360;
            ring.fillOrigin = (int)Image.Origin360.Top;
            ring.fillClockwise = false;
            ring.fillAmount = 0.65f;
            timer.ring = ring;
            timer.icon = K.Centered(dial, "Icon", null, Color.white, 50f);

            RectTransform texts = K.Node("Texts", rt);
            K.Column(texts, 2f, TextAnchor.MiddleLeft);
            timer.nameText = K.Txt(K.Node("Name", texts), "MAGNET", K.Bold, 14f, Color.white, TextAlignmentOptions.Left);
            timer.timeText = K.Txt(K.Node("Time", texts), "6 s", K.Semi, 12f, Soft, TextAlignmentOptions.Left);
            return timer;
        }

        // ------------------------------------------------------------------ pauza

        static PauseUI BuildPause(RectTransform parent)
        {
            RectTransform rt = K.Node("Pause", parent);
            K.Stretch(rt);
            Image dim = rt.gameObject.AddComponent<Image>();
            dim.color = new Color(0.039f, 0.055f, 0.094f, 0.62f);
            PauseUI pause = rt.gameObject.AddComponent<PauseUI>();

            RectTransform panel = K.Place(K.Node("Panel", rt), new Vector2(0.5f, 1f), new Vector2(0f, -90f), new Vector2(440f, 540f));
            K.Box(panel, ShopColors.Panel, new Color(1f, 1f, 1f, 0.15f), 28f, K.Border2);
            K.Column(panel, 14f, TextAnchor.UpperCenter, 40, 40, 36, 32).childForceExpandWidth = true;
            K.FitHeight(panel);
            K.Txt(K.Node("Title", panel), "PAUSED", K.Display, 46f, Color.white, TextAlignmentOptions.Center, 4f);

            RectTransform stats = K.Node("Stats", panel);
            K.Row(stats, 28f, TextAnchor.MiddleCenter, 0, 0, 4, 10);
            pause.scoreText = Stat(stats, "SCORE", Color.white);
            Divider(stats, 48f);
            pause.coinsText = Stat(stats, "COINS", ShopColors.Gold);
            Divider(stats, 48f);
            pause.filesText = Stat(stats, "FILES", Color.white);

            pause.resumeButton = K.GoldButton(K.Height(K.Node("ResumeButton", panel), 76f), "RESUME", K.Play, new Vector2(24f, 28f), 30f);
            pause.restartButton = K.SecondaryButton(K.Height(K.Node("RestartButton", panel), 60f), "RESTART", K.Restart);
            pause.optionsButton = K.SecondaryButton(K.Height(K.Node("OptionsButton", panel), 60f), "OPTIONS", K.Gear);
            pause.quitButton = K.TextButton(K.Height(K.Node("QuitButton", panel), 48f), "Quit to main menu", K.Bold, 16f, Soft, 6f);
            return pause;
        }

        static TextMeshProUGUI Stat(RectTransform parent, string label, Color valueColor)
        {
            RectTransform column = K.Node(label, parent);
            K.Column(column, 2f, TextAnchor.MiddleCenter);
            K.Txt(K.Node("Label", column), label, K.Bold, 12f, ShopColors.Grey, TextAlignmentOptions.Center, 16f);
            return K.Txt(K.Node("Value", column), "0", K.Bold, 26f, valueColor, TextAlignmentOptions.Center);
        }

        static void Divider(RectTransform parent, float height)
        {
            K.Fixed(K.Plain(K.Node("Divider", parent), new Color(1f, 1f, 1f, 0.12f)), 2f, height);
        }

        // ------------------------------------------------------------------ złapany (koniec gry)

        static GameOverUI BuildGameOver(RectTransform parent)
        {
            RectTransform rt = K.Node("GameOver", parent);
            K.Stretch(rt);
            Image vignette = rt.gameObject.AddComponent<Image>();
            vignette.sprite = K.Vignette;
            GameOverUI gameOver = rt.gameObject.AddComponent<GameOverUI>();

            Image stamp = K.IconImage(rt, "Stamp", K.Stamp, Color.white);
            K.Place(stamp.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -125f), new Vector2(460f, 230f));
            stamp.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            gameOver.stamp = stamp.rectTransform;

            K.Txt(K.Place(K.Node("Subtitle", rt), new Vector2(0.5f, 1f), new Vector2(0f, -238f), new Vector2(800f, 26f)),
                "Security got you. The files are back in the vault.", K.Semi, 18f, ShopColors.Hex("E6D2D4"), TextAlignmentOptions.Center);

            // wynik i łup
            RectTransform card = K.Place(K.Node("Results", rt), new Vector2(0.5f, 1f), new Vector2(0f, -276f), new Vector2(560f, 146f));
            K.Box(card, ShopColors.Panel, ShopColors.PanelBorder, 24f, K.Border2);
            K.Row(card, 24f, TextAnchor.MiddleLeft, 28, 28, 22, 22);

            RectTransform left = K.Node("Score", card);
            K.Flexible(left);
            K.Column(left, 2f, TextAnchor.MiddleLeft);
            K.Txt(K.Node("Label", left), "SCORE", K.Bold, 12f, ShopColors.Grey, TextAlignmentOptions.Left, 25f);
            gameOver.scoreText = K.Txt(K.Node("Value", left), "0", K.Display, 52f, Color.white, TextAlignmentOptions.Left);
            RectTransform bestRow = K.Node("BestRow", left);
            K.Row(bestRow, 8f, TextAnchor.MiddleLeft, 0, 0, 6, 0);
            RectTransform badge = K.Node("NewBest", bestRow);
            K.Rounded(badge, K.Panel, ShopColors.Gold, 10f);
            K.Row(badge, 0f, TextAnchor.MiddleCenter, 10, 10, 4, 4);
            K.Txt(K.Node("Label", badge), "NEW BEST!", K.Bold, 12f, ShopColors.Ink, TextAlignmentOptions.Center, 16f);
            gameOver.newBestBadge = badge.gameObject;
            gameOver.previousText = K.Txt(K.Node("Previous", bestRow), "previous 0", K.Semi, 13f, ShopColors.Grey, TextAlignmentOptions.Left);

            Divider(card, 100f);

            RectTransform loot = K.Node("Loot", card);
            K.Column(loot, 12f, TextAnchor.MiddleLeft);
            RectTransform coinRow = K.Node("Coins", loot);
            K.Row(coinRow, 10f, TextAnchor.MiddleLeft);
            K.Fixed(K.IconImage(coinRow, "Icon", K.Coin, Color.white), 30f, 30f);
            gameOver.coinsText = K.Txt(K.Node("Value", coinRow), "+0", K.Bold, 22f, ShopColors.Gold, TextAlignmentOptions.Left);
            RectTransform fileRow = K.Node("Files", loot);
            K.Row(fileRow, 10f, TextAnchor.MiddleLeft);
            K.Fixed(K.IconImage(fileRow, "Icon", K.Folder, Color.white), 30f, 26f);
            gameOver.filesText = K.Txt(K.Node("Value", fileRow), "0 files", K.Bold, 22f, Color.white, TextAlignmentOptions.Left);

            // przyciski
            RectTransform buttons = K.Place(K.Node("Buttons", rt), new Vector2(0.5f, 1f), new Vector2(0f, -438f), new Vector2(560f, 78f));
            K.Row(buttons, 14f, TextAnchor.MiddleCenter);

            RectTransform cont = K.Node("ContinueButton", buttons);
            LayoutElement contLayout = K.Element(K.Height(cont, 78f));
            contLayout.flexibleWidth = 1.6f;
            contLayout.preferredWidth = 0f;
            Image contBg = K.Rounded(cont, K.Panel, ShopColors.Gold, 18f);
            K.Drop(contBg, ShopColors.GoldShadow, 6f);
            gameOver.continueButton = K.MakeButton(contBg);
            K.Column(cont, 4f, TextAnchor.MiddleCenter);
            RectTransform contTop = K.Node("Top", cont);
            K.Row(contTop, 10f, TextAnchor.MiddleCenter);
            K.Fixed(K.IconImage(contTop, "Icon", K.Ad, ShopColors.Ink), 26f, 20f);
            K.Txt(K.Node("Label", contTop), "CONTINUE", K.Display, 26f, ShopColors.Ink, TextAlignmentOptions.Center);
            K.Txt(K.Node("Hint", cont), "watch a short ad", K.Bold, 12f, ShopColors.Ink, TextAlignmentOptions.Center, 8f);

            RectTransform restart = K.Node("RestartButton", buttons);
            LayoutElement restartLayout = K.Element(K.Height(restart, 78f));
            restartLayout.flexibleWidth = 1f;
            restartLayout.preferredWidth = 0f;
            gameOver.restartButton = K.SecondaryButton(restart, "RESTART", K.Restart);

            gameOver.menuButton = K.TextButton(K.Place(K.Node("MenuButton", rt), new Vector2(0.5f, 1f), new Vector2(0f, -532f), new Vector2(300f, 44f)),
                "Main menu", K.Bold, 16f, Soft, 6f);
            return gameOver;
        }

        // ------------------------------------------------------------------ opcje

        static OptionsUI BuildOptions(RectTransform parent)
        {
            RectTransform rt = K.Node("Options", parent);
            K.Stretch(rt);
            Image background = rt.gameObject.AddComponent<Image>();
            background.color = K.ScreenFill;
            OptionsUI options = rt.gameObject.AddComponent<OptionsUI>();

            options.backButton = K.BackButton(rt);
            K.Txt(K.Place(K.Node("Title", rt), new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(600f, 44f)),
                "OPTIONS", K.Display, 40f, Color.white, TextAlignmentOptions.Center, 5f);

            RectTransform panel = K.Place(K.Node("Panel", rt), new Vector2(0.5f, 1f), new Vector2(0f, -108f), new Vector2(760f, 534f));
            K.Box(panel, ShopColors.Panel, ShopColors.PanelBorder, 26f, K.Border2);
            K.Column(panel, 0f, TextAnchor.UpperCenter, 32, 32, 10, 10).childForceExpandWidth = true;

            RectTransform music = SettingRow(panel, "MusicRow", 74f, true);
            K.Width(RowLabel(music, "Music"), 220f);
            options.musicSlider = BuildSlider(music);
            options.musicValue = ValueText(music);

            RectTransform sfx = SettingRow(panel, "SfxRow", 74f, true);
            K.Width(RowLabel(sfx, "Sound effects"), 220f);
            options.sfxSlider = BuildSlider(sfx);
            options.sfxValue = ValueText(sfx);

            RectTransform vibration = SettingRow(panel, "VibrationRow", 74f, true);
            K.Flexible(RowLabel(vibration, "Vibration"));
            options.vibration = BuildSwitch(vibration);

            RectTransform swipe = SettingRow(panel, "SwipeRow", 74f, true);
            K.Flexible(RowLabel(swipe, "Swipe sensitivity"));
            options.swipe = BuildSegmented(swipe, "Low", "Medium", "High");

            RectTransform graphics = SettingRow(panel, "GraphicsRow", 74f, true);
            K.Flexible(RowLabel(graphics, "Graphics"));
            options.graphics = BuildSegmented(graphics, "Low", "Medium", "High");

            RectTransform language = SettingRow(panel, "LanguageRow", 74f, true);
            K.Flexible(RowLabel(language, "Language"));
            options.language = BuildSegmented(language, "English", "Polski");

            RectTransform footer = SettingRow(panel, "Footer", 70f, false);
            RectTransform links = K.Node("Links", footer);
            K.Row(links, 24f, TextAnchor.MiddleLeft);
            options.restoreButton = LinkButton(links, "Restore purchases");
            options.privacyButton = LinkButton(links, "Privacy policy");
            options.creditsButton = LinkButton(links, "Credits");
            K.Spacer(footer);
            options.versionText = K.Txt(K.Node("Version", footer), "BeetleBit Studio · v0.1", K.Semi, 12f, ShopColors.Dim, TextAlignmentOptions.Right);
            return options;
        }

        static RectTransform SettingRow(RectTransform parent, string name, float height, bool divider)
        {
            RectTransform rt = K.Node(name, parent);
            K.Height(rt, height);
            K.Row(rt, 20f, TextAnchor.MiddleLeft);
            if (divider)
            {
                RectTransform line = K.Node("Divider", rt);
                line.anchorMin = Vector2.zero;
                line.anchorMax = new Vector2(1f, 0f);
                line.pivot = new Vector2(0.5f, 0f);
                line.offsetMin = Vector2.zero;
                line.offsetMax = new Vector2(0f, 1f);
                K.Plain(line, new Color(1f, 1f, 1f, 0.1f));
                K.Element(line).ignoreLayout = true;
            }
            return rt;
        }

        static TextMeshProUGUI RowLabel(RectTransform row, string text)
        {
            return K.Txt(K.Node("Label", row), text, K.Bold, 18f, Color.white, TextAlignmentOptions.Left);
        }

        static TextMeshProUGUI ValueText(RectTransform row)
        {
            return K.Width(K.Txt(K.Node("Value", row), "0", K.Bold, 16f, ShopColors.Gold, TextAlignmentOptions.Right), 48f);
        }

        static Slider BuildSlider(RectTransform row)
        {
            RectTransform rt = K.Node("Slider", row);
            K.Flexible(K.Height(rt, 28f));
            K.Plain(rt, new Color(1f, 1f, 1f, 0f)).raycastTarget = true;

            RectTransform track = K.Node("Background", rt);
            track.anchorMin = new Vector2(0f, 0.5f);
            track.anchorMax = new Vector2(1f, 0.5f);
            track.pivot = new Vector2(0.5f, 0.5f);
            track.sizeDelta = new Vector2(0f, 8f);
            track.anchoredPosition = Vector2.zero;
            K.Rounded(track, K.Panel, new Color(1f, 1f, 1f, 0.18f), 4f);

            RectTransform fillArea = K.Node("Fill Area", rt);
            fillArea.anchorMin = new Vector2(0f, 0.5f);
            fillArea.anchorMax = new Vector2(1f, 0.5f);
            fillArea.pivot = new Vector2(0.5f, 0.5f);
            fillArea.offsetMin = new Vector2(0f, -4f);
            fillArea.offsetMax = new Vector2(-28f, 4f);
            RectTransform fill = K.Node("Fill", fillArea);
            fill.anchorMin = Vector2.zero;
            fill.anchorMax = new Vector2(0.7f, 1f);
            fill.offsetMin = Vector2.zero;
            fill.offsetMax = new Vector2(14f, 0f);
            K.Rounded(fill, K.Panel, ShopColors.Gold, 4f);

            RectTransform handleArea = K.Node("Handle Slide Area", rt);
            K.Stretch(handleArea);
            handleArea.offsetMin = new Vector2(14f, 0f);
            handleArea.offsetMax = new Vector2(-14f, 0f);
            RectTransform handle = K.Node("Handle", handleArea);
            handle.anchorMin = new Vector2(0.7f, 0f);
            handle.anchorMax = new Vector2(0.7f, 1f);
            handle.pivot = new Vector2(0.5f, 0.5f);
            handle.sizeDelta = new Vector2(28f, 0f);
            handle.anchoredPosition = Vector2.zero;
            Image knob = handle.gameObject.AddComponent<Image>();
            knob.sprite = K.Disc;
            knob.color = Color.white;

            Slider slider = rt.gameObject.AddComponent<Slider>();
            slider.fillRect = fill;
            slider.handleRect = handle;
            slider.targetGraphic = knob;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = 100f;
            slider.wholeNumbers = true;
            slider.value = 70f;
            ColorBlock colors = slider.colors;
            colors.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            slider.colors = colors;
            Navigation navigation = slider.navigation;
            navigation.mode = Navigation.Mode.None;
            slider.navigation = navigation;
            return slider;
        }

        static SwitchUI BuildSwitch(RectTransform row)
        {
            RectTransform rt = K.Node("Switch", row);
            K.Fixed(rt, 76f, 44f);
            Image background = K.Rounded(rt, K.Panel, ShopColors.Gold, 22f);
            SwitchUI toggle = rt.gameObject.AddComponent<SwitchUI>();
            toggle.button = K.MakeButton(background);
            toggle.background = background;
            RectTransform knob = K.Node("Knob", rt);
            knob.anchorMin = new Vector2(0f, 0.5f);
            knob.anchorMax = new Vector2(0f, 0.5f);
            knob.pivot = new Vector2(0f, 0.5f);
            knob.sizeDelta = new Vector2(36f, 36f);
            knob.anchoredPosition = new Vector2(toggle.onX, 0f);
            Image knobImage = knob.gameObject.AddComponent<Image>();
            knobImage.sprite = K.Disc;
            knobImage.raycastTarget = false;
            toggle.knob = knob;
            return toggle;
        }

        static SegmentedUI BuildSegmented(RectTransform row, params string[] labels)
        {
            RectTransform rt = K.Node("Choice", row);
            K.Rounded(rt, K.Panel, new Color(1f, 1f, 1f, 0.08f), 14f);
            K.Row(rt, 4f, TextAnchor.MiddleCenter, 4, 4, 4, 4);
            SegmentedUI choice = rt.gameObject.AddComponent<SegmentedUI>();
            choice.options = new Button[labels.Length];
            for (int i = 0; i < labels.Length; i++)
            {
                RectTransform option = K.Node(labels[i], rt);
                LayoutElement layout = K.Element(K.Height(option, 44f));
                layout.minWidth = 92f;
                choice.options[i] = K.MakeButton(K.Rounded(option, K.Panel, new Color(1f, 1f, 1f, 0f), 11f));
                K.Row(option, 0f, TextAnchor.MiddleCenter, 14, 14);
                K.Txt(K.Node("Label", option), labels[i], K.Bold, 15f, Soft, TextAlignmentOptions.Center);
            }
            choice.Select(0, false);
            return choice;
        }

        static Button LinkButton(RectTransform parent, string label)
        {
            RectTransform rt = K.Node(label, parent);
            Button button = K.MakeButton(K.Plain(rt, new Color(1f, 1f, 1f, 0f)));
            K.Row(rt, 0f, TextAnchor.MiddleCenter, 0, 0, 8, 8);
            button.targetGraphic = K.Txt(K.Node("Label", rt), label, K.Bold, 14f, Soft, TextAlignmentOptions.Center);
            return button;
        }
    }
}
