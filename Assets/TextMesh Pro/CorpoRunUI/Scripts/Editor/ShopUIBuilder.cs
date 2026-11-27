using CorpoRun.Shop;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using K = CorpoRun.EditorTools.UIKit;

namespace CorpoRun.EditorTools
{
    // Sklep: zakładki Ulepszenia i Skiny. Normalnie buduje go "Zbuduj interfejs gry".
    public static class ShopUIBuilder
    {
        [MenuItem("CorpoRun/Zbuduj sam sklep")]
        public static void Build()
        {
            if (!K.Init()) return;
            ShopUI shop = Create();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Selection.activeGameObject = shop.gameObject;
            Debug.Log("CorpoRun: sklep gotowy (ShopCanvas → Shop).");
        }

        // Tworzy ShopCanvas ze sklepem. Wcześniej musi być wywołane UIKit.Init().
        public static ShopUI Create()
        {
            ShopCatalog catalog = LoadCatalog();
            K.EnsureEventSystem();
            ShopUI shop = BuildShop(K.CreateCanvas("ShopCanvas", 50), catalog);
            shop.Refresh();
            return shop;
        }

        static ShopCatalog LoadCatalog()
        {
            string path = K.Root + "/ShopCatalog.asset";
            ShopCatalog catalog = AssetDatabase.LoadAssetAtPath<ShopCatalog>(path);
            if (catalog != null) return catalog;

            catalog = ScriptableObject.CreateInstance<ShopCatalog>();
            catalog.powerUps.Add(Power("magnet", "Magnet", "Pulls in coins from every lane.", "pu_magnes", "Duration", "s", 10, 12, 15, 18, 22));
            catalog.powerUps.Add(Power("coins2", "Coins x2", "Every coin you grab counts double.", "pu_monety_x2", "Duration", "s", 8, 10, 13, 16, 20));
            catalog.powerUps.Add(Power("shield", "Shield", "Take one hit and keep running.", "pu_tarcza", "Duration", "s", 6, 8, 10, 13, 16));
            catalog.powerUps.Add(Power("headstart", "Headstart", "Dash through the first stretch of the run.", "pu_headstart", "Distance", "m", 300, 500, 750, 1000, 1500));
            catalog.skins.Add(Skin("classic", "Classic", SkinRarity.Common, "The original faceless runner.", 0));
            catalog.skins.Add(Skin("intern", "Intern", SkinRarity.Common, "Unpaid, unseen, unstoppable.", 0));
            catalog.skins.Add(Skin("manager", "Manager", SkinRarity.Rare, "Suit, tie and a stolen quarterly report.", 500));
            catalog.skins.Add(Skin("janitor", "Janitor", SkinRarity.Rare, "Knows every back door in the building.", 800));
            catalog.skins.Add(Skin("undercover", "Undercover", SkinRarity.Epic, "Dressed as security. Nobody suspects a thing.", 1200));
            catalog.skins.Add(Skin("ceo", "CEO", SkinRarity.Legendary, "The boss is leaking his own files.", 2500));
            AssetDatabase.CreateAsset(catalog, path);
            AssetDatabase.SaveAssets();
            return catalog;
        }

        static PowerUpDef Power(string id, string name, string description, string icon, string stat, string unit, params float[] values)
        {
            return new PowerUpDef
            {
                id = id, displayName = name, description = description, icon = K.LoadSprite("Icons/" + icon + ".png"),
                statLabel = stat, unit = unit, values = values
            };
        }

        static SkinDef Skin(string id, string name, SkinRarity rarity, string description, int cost)
        {
            return new SkinDef
            {
                id = id, displayName = name, rarity = rarity, description = description, cost = cost,
                preview = K.LoadSprite("Skins/skin_" + id + ".png")
            };
        }

        static ShopUI BuildShop(Transform canvas, ShopCatalog catalog)
        {
            RectTransform rootRt = K.Node("Shop", canvas);
            K.Stretch(rootRt);
            Image background = rootRt.gameObject.AddComponent<Image>();
            background.color = K.ScreenFill;
            ShopUI shop = rootRt.gameObject.AddComponent<ShopUI>();
            shop.catalog = catalog;

            // nagłówek: wstecz, tytuł, zakładki, monety
            shop.backButton = K.BackButton(rootRt);

            RectTransform title = K.Place(K.Node("Title", rootRt), new Vector2(0f, 1f), new Vector2(112f, -24f), new Vector2(320f, 64f));
            K.Row(title, 12f, TextAnchor.MiddleLeft);
            K.Fixed(K.IconImage(title, "Cart", K.Cart, ShopColors.Gold), 38f, 38f);
            K.Txt(K.Node("Label", title), "SHOP", K.Display, 40f, Color.white, TextAlignmentOptions.Left, 5f);

            RectTransform tabs = K.Place(K.Node("Tabs", rootRt), new Vector2(0.5f, 1f), new Vector2(0f, -26f), new Vector2(420f, 60f));
            K.Box(tabs, new Color(0.078f, 0.094f, 0.141f, 0.88f), new Color(1f, 1f, 1f, 0.15f), 30f, K.Border2);
            shop.upgradesTab = Tab(tabs, "UpgradesTab", K.TabUpgrades, "UPGRADES", 5f);
            shop.skinsTab = Tab(tabs, "SkinsTab", K.TabSkins, "SKINS", 213f);

            RectTransform coins = K.Place(K.Node("Coins", rootRt), new Vector2(1f, 1f), new Vector2(-28f, -28f), new Vector2(150f, 52f));
            K.Box(coins, new Color(0.078f, 0.094f, 0.141f, 0.88f), new Color(1f, 1f, 1f, 0.18f), 26f, K.Border2);
            K.Row(coins, 10f, TextAnchor.MiddleLeft, 8, 20);
            K.FitWidth(coins);
            K.Fixed(K.IconImage(coins, "Coin", K.Coin, Color.white), 36f, 36f);
            shop.coinsText = K.Txt(K.Node("Amount", coins), "0", K.Display, 22f, Color.white, TextAlignmentOptions.Left);

            RectTransform content = K.Place(K.Node("Content", rootRt), new Vector2(0.5f, 1f), new Vector2(0f, -116f), new Vector2(1200f, 570f));

            // zakładka Ulepszenia
            RectTransform upgrades = K.Node("UpgradesPage", content);
            K.Stretch(upgrades);
            K.Column(upgrades, 14f, TextAnchor.UpperCenter).childForceExpandWidth = true;
            shop.upgradesPage = upgrades.gameObject;
            shop.upgradeList = upgrades;
            shop.upgradeRowTemplate = UpgradeRow(upgrades);

            // zakładka Skiny
            RectTransform skins = K.Node("SkinsPage", content);
            K.Stretch(skins);
            shop.skinsPage = skins.gameObject;
            BuildPreview(shop, skins);

            RectTransform grid = K.Place(K.Node("Grid", skins), new Vector2(0f, 1f), new Vector2(430f, 0f), new Vector2(770f, 570f));
            GridLayoutGroup layout = grid.gameObject.AddComponent<GridLayoutGroup>();
            layout.cellSize = new Vector2(244f, 276f);
            layout.spacing = new Vector2(18f, 18f);
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = 3;
            shop.skinGrid = grid;
            shop.skinCardTemplate = SkinCard(grid);

            // komunikat na dole ekranu
            RectTransform toast = K.Place(K.Node("Toast", rootRt), new Vector2(0.5f, 0f), new Vector2(0f, 26f), new Vector2(500f, 56f));
            CanvasGroup group = toast.gameObject.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            group.blocksRaycasts = false;
            group.interactable = false;
            K.Box(toast, new Color(0.055f, 0.071f, 0.11f, 0.96f), ShopColors.Gold, 28f, K.Border2);
            K.Row(toast, 10f, TextAnchor.MiddleCenter);
            K.Fixed(K.IconImage(toast, "Icon", K.ToastIcon, ShopColors.Gold), 20f, 20f);
            shop.toastText = K.Txt(K.Node("Message", toast), "", K.Bold, 17f, Color.white, TextAlignmentOptions.Center);
            shop.toast = group;

            return shop;
        }

        static Button Tab(RectTransform parent, string name, Sprite icon, string label, float x)
        {
            RectTransform rt = K.Place(K.Node(name, parent), new Vector2(0f, 0.5f), new Vector2(x, 0f), new Vector2(202f, 46f));
            Button button = K.MakeButton(K.Rounded(rt, K.Panel, ShopColors.Gold, 23f));
            K.Row(rt, 8f, TextAnchor.MiddleCenter);
            K.Fixed(K.IconImage(rt, "Icon", icon, Color.white), 20f, 20f);
            K.Txt(K.Node("Label", rt), label, K.Bold, 16f, Color.white, TextAlignmentOptions.Center, 6f);
            return button;
        }

        static UpgradeRowUI UpgradeRow(RectTransform parent)
        {
            RectTransform rt = K.Node("UpgradeRow", parent);
            K.Height(rt, 132f);
            K.Box(rt, ShopColors.Panel, ShopColors.PanelBorder, 24f, K.Border2);
            K.Row(rt, 22f, TextAnchor.MiddleLeft, 18, 22);
            UpgradeRowUI row = rt.gameObject.AddComponent<UpgradeRowUI>();

            row.icon = K.Fixed(K.IconImage(rt, "Icon", null, Color.white), 100f, 100f);

            RectTransform texts = K.Node("Texts", rt);
            K.Fixed(texts, 300f, 100f);
            K.Column(texts, 6f, TextAnchor.MiddleLeft).childForceExpandWidth = true;
            row.nameText = K.Txt(K.Node("Name", texts), "Power-up", K.Display, 25f, Color.white, TextAlignmentOptions.Left);
            row.descriptionText = K.Txt(K.Node("Description", texts), "Description", K.Semi, 15f, ShopColors.Grey, TextAlignmentOptions.TopLeft);

            RectTransform level = K.Node("Level", rt);
            K.Flexible(K.Height(level, 100f));
            K.Column(level, 10f, TextAnchor.MiddleLeft);
            row.levelText = K.Txt(K.Node("LevelText", level), "LEVEL 1 / 5", K.Bold, 12f, ShopColors.Grey, TextAlignmentOptions.Left, 16f);
            RectTransform pips = K.Node("Pips", level);
            K.Row(pips, 6f, TextAnchor.MiddleLeft);
            row.pips = new Image[5];
            for (int i = 0; i < row.pips.Length; i++)
                row.pips[i] = K.Fixed(K.Rounded(K.Node("Pip" + (i + 1), pips), K.Panel, ShopColors.PipEmpty, 7f), 56f, 14f);
            row.statText = K.Txt(K.Node("Stat", level), "Duration", K.Bold, 15f, ShopColors.Grey, TextAlignmentOptions.Left);

            RectTransform buttonRt = K.Node("UpgradeButton", rt);
            Image buttonBg = K.Fixed(K.Rounded(buttonRt, K.Panel, ShopColors.Gold, 18f), 230f, 74f);
            row.button = K.MakeButton(buttonBg);
            row.buttonShadow = K.Drop(buttonBg, ShopColors.GoldShadow, 6f);
            K.Column(buttonRt, 5f, TextAnchor.MiddleCenter);
            row.buttonLabel = K.Txt(K.Node("Label", buttonRt), "UPGRADE", K.Bold, 13f, ShopColors.Ink, TextAlignmentOptions.Center, 11.5f);
            RectTransform price = K.Node("Price", buttonRt);
            K.Row(price, 8f, TextAnchor.MiddleCenter);
            row.price = price.gameObject;
            row.priceCoin = K.Fixed(K.IconImage(price, "Coin", K.Coin, Color.white), 24f, 24f);
            row.priceText = K.Txt(K.Node("Amount", price), "250", K.Display, 22f, ShopColors.Ink, TextAlignmentOptions.Left);
            return row;
        }

        static void BuildPreview(ShopUI shop, RectTransform parent)
        {
            RectTransform preview = K.Place(K.Node("Preview", parent), new Vector2(0f, 1f), Vector2.zero, new Vector2(400f, 570f));
            K.Box(preview, ShopColors.Panel, ShopColors.PanelBorder, 26f, K.Border2);

            shop.previewGlow = K.IconImage(preview, "Glow", K.Glow, ShopColors.Alpha(ShopColors.Gold, 0.35f));
            shop.previewGlow.preserveAspect = false;
            K.Place(shop.previewGlow.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -20f), new Vector2(270f, 270f));
            shop.previewFigure = K.IconImage(preview, "Figure", null, Color.white);
            K.Place(shop.previewFigure.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -20f), new Vector2(135f, 270f));

            shop.previewName = K.Txt(K.Place(K.Node("Name", preview), new Vector2(0.5f, 1f), new Vector2(0f, -300f), new Vector2(348f, 36f)),
                "Skin", K.Display, 30f, Color.white, TextAlignmentOptions.Center);

            RectTransform rarity = K.Place(K.Node("Rarity", preview), new Vector2(0.5f, 1f), new Vector2(0f, -346f), new Vector2(100f, 24f));
            shop.previewRarityBackground = K.Rounded(rarity, K.Panel, ShopColors.Alpha(ShopColors.Gold, 0.16f), 10f);
            K.Row(rarity, 0f, TextAnchor.MiddleCenter, 12, 12, 5, 5);
            K.Fit(rarity);
            shop.previewRarity = K.Txt(K.Node("Label", rarity), "COMMON", K.Bold, 12f, ShopColors.Grey, TextAlignmentOptions.Center, 16f);

            shop.previewDescription = K.Txt(K.Place(K.Node("Description", preview), new Vector2(0.5f, 1f), new Vector2(0f, -384f), new Vector2(348f, 44f)),
                "", K.Semi, 14f, ShopColors.Grey, TextAlignmentOptions.Top);

            RectTransform action = K.Place(K.Node("ActionButton", preview), new Vector2(0.5f, 0f), new Vector2(0f, 52f), new Vector2(348f, 68f));
            Image actionBg = K.Rounded(action, K.Panel, ShopColors.Gold, 18f);
            shop.previewButton = K.MakeButton(actionBg);
            shop.previewButtonShadow = K.Drop(actionBg, ShopColors.GoldShadow, 6f);
            K.Row(action, 10f, TextAnchor.MiddleCenter);
            shop.previewButtonLabel = K.Txt(K.Node("Label", action), "BUY", K.Display, 22f, ShopColors.Ink, TextAlignmentOptions.Center);
            RectTransform price = K.Node("Price", action);
            K.Row(price, 8f, TextAnchor.MiddleCenter);
            shop.previewPrice = price.gameObject;
            shop.previewPriceCoin = K.Fixed(K.IconImage(price, "Coin", K.Coin, Color.white), 26f, 26f);
            shop.previewPriceText = K.Txt(K.Node("Amount", price), "0", K.Display, 22f, ShopColors.Ink, TextAlignmentOptions.Left);

            shop.previewHint = K.Txt(K.Place(K.Node("Hint", preview), new Vector2(0.5f, 0f), new Vector2(0f, 16f), new Vector2(348f, 24f)),
                "", K.Semi, 13f, ShopColors.Dim, TextAlignmentOptions.Center);
        }

        static SkinCardUI SkinCard(RectTransform parent)
        {
            RectTransform rt = K.Node("SkinCard", parent);
            Image fill = K.Rounded(rt, K.Panel, ShopColors.Panel, 22f);
            SkinCardUI card = rt.gameObject.AddComponent<SkinCardUI>();
            card.button = K.MakeButton(fill);

            RectTransform frame = K.Node("Border", rt);
            K.Stretch(frame);
            card.border = K.Rounded(frame, K.Border3, ShopColors.PanelBorder, 22f);

            card.figure = K.IconImage(rt, "Figure", null, Color.white);
            K.Place(card.figure.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -24f), new Vector2(70f, 140f));
            card.nameText = K.Txt(K.Place(K.Node("Name", rt), new Vector2(0.5f, 1f), new Vector2(0f, -172f), new Vector2(220f, 24f)),
                "Skin", K.Bold, 18f, Color.white, TextAlignmentOptions.Center);
            card.rarityText = K.Txt(K.Place(K.Node("Rarity", rt), new Vector2(0.5f, 1f), new Vector2(0f, -198f), new Vector2(220f, 16f)),
                "COMMON", K.Bold, 11f, ShopColors.Grey, TextAlignmentOptions.Center, 13.5f);

            RectTransform price = K.Place(K.Node("Price", rt), new Vector2(0.5f, 0f), new Vector2(0f, 16f), new Vector2(200f, 24f));
            K.Row(price, 6f, TextAnchor.MiddleCenter);
            card.priceCoin = K.Fixed(K.IconImage(price, "Coin", K.Coin, Color.white), 20f, 20f);
            card.priceText = K.Txt(K.Node("Amount", price), "0", K.Bold, 16f, ShopColors.Gold, TextAlignmentOptions.Left);

            RectTransform badge = K.Place(K.Node("Badge", rt), new Vector2(1f, 1f), new Vector2(-12f, -12f), new Vector2(70f, 22f));
            card.badgeBackground = K.Rounded(badge, K.Panel, ShopColors.Gold, 8f);
            K.Row(badge, 0f, TextAnchor.MiddleCenter, 8, 8, 4, 4);
            K.Fit(badge);
            card.badgeText = K.Txt(K.Node("Label", badge), "OWNED", K.Bold, 11f, ShopColors.Ink, TextAlignmentOptions.Center, 9f);
            card.badge = badge.gameObject;
            return card;
        }
    }
}
