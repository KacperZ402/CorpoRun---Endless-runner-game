using System.IO;
using CorpoRun.Shop;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CorpoRun.EditorTools
{
    // Wspólne klocki do budowania UI w edytorze: grafiki, fonty, panele, przyciski, układy.
    public static class UIKit
    {
        const string KitPath = "/Scripts/Editor/UIKit.cs";
        const float SpriteRadius = 24f; // promień narożnika w slice_panel.png

        public static string Root { get; private set; }

        public static Sprite Panel, Border2, Border3, Glow, Disc, Ring, GradientH, Vignette;
        public static Sprite Coin, Back, Cart, TabUpgrades, TabSkins, ToastIcon;
        public static Sprite Play, Restart, Gear, Trophy, Folder, Guard, Ad, Stamp, Logo;
        public static TMP_FontAsset Display, Bold, Semi;

        public static readonly Color HudFill = new Color(0.078f, 0.094f, 0.141f, 0.75f);
        public static readonly Color ButtonFill = new Color(0.078f, 0.094f, 0.141f, 0.85f);
        public static readonly Color ScreenFill = new Color(0.078f, 0.102f, 0.157f, 0.97f);

        public static bool Init()
        {
            Root = FindRoot();
            if (Root == null)
            {
                Debug.LogError("CorpoRun: nie znalazłem folderu z interfejsem (szukam pliku *" + KitPath + ").");
                return false;
            }

            ConfigureSprites();
            Panel = LoadSprite("UI/slice_panel.png");
            Border2 = LoadSprite("UI/slice_border2.png");
            Border3 = LoadSprite("UI/slice_border3.png");
            Glow = LoadSprite("UI/glow.png");
            Disc = LoadSprite("UI/disc.png");
            Ring = LoadSprite("UI/ring.png");
            GradientH = LoadSprite("UI/gradient_h.png");
            Vignette = LoadSprite("UI/vignette.png");
            Coin = LoadSprite("Icons/coin.png");
            Back = LoadSprite("Icons/back.png");
            Cart = LoadSprite("Icons/cart.png");
            TabUpgrades = LoadSprite("Icons/tab_upgrades.png");
            TabSkins = LoadSprite("Icons/tab_skins.png");
            ToastIcon = LoadSprite("Icons/toast.png");
            Play = LoadSprite("Icons/play.png");
            Restart = LoadSprite("Icons/restart.png");
            Gear = LoadSprite("Icons/gear.png");
            Trophy = LoadSprite("Icons/trophy.png");
            Folder = LoadSprite("Icons/folder.png");
            Guard = LoadSprite("Icons/guard.png");
            Ad = LoadSprite("Icons/ad.png");
            Stamp = LoadSprite("Screens/game_over.png");
            Logo = LoadSprite("Logo/logo_1_klasyczny_3d.png");

            Display = LoadFont("ArchivoBlack-Regular");
            Bold = LoadFont("Inter-ExtraBold");
            Semi = LoadFont("Inter-SemiBold");
            return true;
        }

        // ------------------------------------------------------------------ zasoby

        static string FindRoot()
        {
            foreach (string guid in AssetDatabase.FindAssets("UIKit t:MonoScript"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.EndsWith(KitPath)) return path.Substring(0, path.Length - KitPath.Length);
            }
            return null;
        }

        static void ConfigureSprites()
        {
            foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { Root + "/Sprites" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) continue;
                bool slice = Path.GetFileName(path).StartsWith("slice_");
                Vector4 border = slice ? new Vector4(32f, 32f, 32f, 32f) : Vector4.zero;
                if (importer.textureType == TextureImporterType.Sprite && importer.spriteBorder == border
                    && importer.mipmapEnabled == !slice && importer.alphaIsTransparency) continue;
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = !slice;
                importer.spriteBorder = border;
                importer.SaveAndReimport();
            }
        }

        public static Sprite LoadSprite(string relative)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Root + "/Sprites/" + relative);
            if (sprite == null) Debug.LogWarning("CorpoRun: brak grafiki " + relative);
            return sprite;
        }

        static TMP_FontAsset LoadFont(string file)
        {
            string assetPath = Root + "/Fonts/" + file + " SDF.asset";
            TMP_FontAsset existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);
            if (existing != null) return existing;

            UnityEngine.Font source = AssetDatabase.LoadAssetAtPath<UnityEngine.Font>(Root + "/Fonts/" + file + ".ttf");
            if (source != null)
            {
                try
                {
                    TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(source);
                    if (fontAsset != null)
                    {
                        fontAsset.name = file + " SDF";
                        AssetDatabase.CreateAsset(fontAsset, assetPath);
                        if (fontAsset.atlasTextures != null)
                        {
                            foreach (Texture2D atlas in fontAsset.atlasTextures)
                            {
                                if (atlas == null) continue;
                                atlas.name = fontAsset.name + " Atlas";
                                AssetDatabase.AddObjectToAsset(atlas, fontAsset);
                            }
                        }
                        if (fontAsset.material != null)
                        {
                            fontAsset.material.name = fontAsset.name + " Material";
                            AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
                        }
                        EditorUtility.SetDirty(fontAsset);
                        AssetDatabase.SaveAssets();
                        return fontAsset;
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning("CorpoRun: nie udało się zrobić fontu TMP z " + file + ": " + e.Message);
                }
            }
            Debug.LogWarning("CorpoRun: używam domyślnego fontu TMP zamiast " + file + ".");
            try
            {
                return TMP_Settings.defaultFontAsset;
            }
            catch (System.Exception)
            {
                return null; // brak TMP Essentials - TMP sam zaproponuje import
            }
        }

        public static T FindInScene<T>() where T : Object
        {
#if UNITY_2023_1_OR_NEWER
            return Object.FindFirstObjectByType<T>(FindObjectsInactive.Include);
#else
            return Object.FindObjectOfType<T>(true);
#endif
        }

        public static void EnsureEventSystem()
        {
            if (FindInScene<EventSystem>() != null) return;
            GameObject go = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
            go.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
            go.AddComponent<StandaloneInputModule>();
#endif
            Undo.RegisterCreatedObjectUndo(go, "CorpoRun UI");
        }

        // Canvas dopasowany do ekranu 1280x720 (na innych proporcjach się poszerza).
        public static Transform CreateCanvas(string name, int sortingOrder)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.layer = LayerMask.NameToLayer("UI");
            Canvas canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;
            CanvasScaler scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            Undo.RegisterCreatedObjectUndo(go, "CorpoRun UI");
            return go.transform;
        }

        // ------------------------------------------------------------------ klocki

        public static RectTransform Node(string name, Transform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.layer = LayerMask.NameToLayer("UI");
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        // Kotwica i pivot w tym samym punkcie rodzica, pozycja i rozmiar w px (ekran 1280x720).
        public static RectTransform Place(RectTransform rt, Vector2 anchor, Vector2 position, Vector2 size)
        {
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = anchor;
            rt.anchoredPosition = position;
            rt.sizeDelta = size;
            return rt;
        }

        public static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        public static Image Rounded(RectTransform rt, Sprite sprite, Color color, float radius)
        {
            Image image = rt.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.type = Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = SpriteRadius / radius;
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        // Zaokrąglony panel z cienką ramką (ramka to osobne dziecko "Border").
        public static Image Box(RectTransform rt, Color fill, Color line, float radius, Sprite ring)
        {
            Image image = Rounded(rt, Panel, fill, radius);
            RectTransform frame = Node("Border", rt);
            Stretch(frame);
            Rounded(frame, ring, line, radius);
            frame.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            return image;
        }

        public static Image Plain(RectTransform rt, Color color)
        {
            Image image = rt.gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        public static Image IconImage(RectTransform parent, string name, Sprite sprite, Color color)
        {
            RectTransform rt = Node(name, parent);
            Image image = rt.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.preserveAspect = true;
            image.raycastTarget = false;
            return image;
        }

        public static Image Centered(RectTransform parent, string name, Sprite sprite, Color color, float size)
        {
            Image image = IconImage(parent, name, sprite, color);
            Place(image.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(size, size));
            return image;
        }

        public static T Fixed<T>(T component, float width, float height) where T : Component
        {
            LayoutElement element = Element(component);
            element.minWidth = width;
            element.preferredWidth = width;
            element.minHeight = height;
            element.preferredHeight = height;
            return component;
        }

        public static T Width<T>(T component, float width) where T : Component
        {
            LayoutElement element = Element(component);
            element.minWidth = width;
            element.preferredWidth = width;
            return component;
        }

        public static T Height<T>(T component, float height) where T : Component
        {
            LayoutElement element = Element(component);
            element.minHeight = height;
            element.preferredHeight = height;
            return component;
        }

        public static T Flexible<T>(T component, float weight = 1f) where T : Component
        {
            Element(component).flexibleWidth = weight;
            return component;
        }

        public static LayoutElement Element(Component component)
        {
            LayoutElement element = component.GetComponent<LayoutElement>();
            return element != null ? element : component.gameObject.AddComponent<LayoutElement>();
        }

        public static RectTransform Spacer(RectTransform parent)
        {
            RectTransform rt = Node("Spacer", parent);
            Flexible(rt);
            return rt;
        }

        public static TextMeshProUGUI Txt(RectTransform rt, string text, TMP_FontAsset font, float size, Color color, TextAlignmentOptions alignment, float spacing = 0f)
        {
            TextMeshProUGUI label = rt.gameObject.AddComponent<TextMeshProUGUI>();
            if (font != null) label.font = font;
            label.text = text;
            label.fontSize = size;
            label.color = color;
            label.alignment = alignment;
            label.characterSpacing = spacing;
            label.raycastTarget = false;
            return label;
        }

        public static HorizontalLayoutGroup Row(RectTransform rt, float spacing, TextAnchor alignment, int left = 0, int right = 0, int top = 0, int bottom = 0)
        {
            HorizontalLayoutGroup group = rt.gameObject.AddComponent<HorizontalLayoutGroup>();
            group.spacing = spacing;
            group.childAlignment = alignment;
            group.padding = new RectOffset(left, right, top, bottom);
            group.childControlWidth = true;
            group.childControlHeight = true;
            group.childForceExpandWidth = false;
            group.childForceExpandHeight = false;
            return group;
        }

        public static VerticalLayoutGroup Column(RectTransform rt, float spacing, TextAnchor alignment, int left = 0, int right = 0, int top = 0, int bottom = 0)
        {
            VerticalLayoutGroup group = rt.gameObject.AddComponent<VerticalLayoutGroup>();
            group.spacing = spacing;
            group.childAlignment = alignment;
            group.padding = new RectOffset(left, right, top, bottom);
            group.childControlWidth = true;
            group.childControlHeight = true;
            group.childForceExpandWidth = false;
            group.childForceExpandHeight = false;
            return group;
        }

        public static void Fit(RectTransform rt)
        {
            ContentSizeFitter fitter = rt.gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        public static void FitWidth(RectTransform rt)
        {
            rt.gameObject.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        public static void FitHeight(RectTransform rt)
        {
            rt.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        public static Button MakeButton(Image graphic)
        {
            graphic.raycastTarget = true;
            Button button = graphic.gameObject.AddComponent<Button>();
            button.targetGraphic = graphic;
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = Color.white;
            colors.selectedColor = Color.white;
            colors.disabledColor = Color.white;
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            colors.fadeDuration = 0.06f;
            button.colors = colors;
            Navigation navigation = button.navigation;
            navigation.mode = Navigation.Mode.None;
            button.navigation = navigation;
            return button;
        }

        // "Podłoga" pod przyciskiem jak w projekcie (kopia kształtu przesunięta w dół).
        public static Shadow Drop(Image graphic, Color color, float distance)
        {
            Shadow shadow = graphic.gameObject.AddComponent<Shadow>();
            shadow.effectDistance = new Vector2(0f, -distance);
            shadow.effectColor = color;
            shadow.useGraphicAlpha = false;
            return shadow;
        }

        // Przycisk: przezroczysty obszar kliknięcia + tekst (np. "Main menu").
        public static Button TextButton(RectTransform rt, string label, TMP_FontAsset font, float size, Color color, float spacing = 0f)
        {
            Button button = MakeButton(Plain(rt, new Color(1f, 1f, 1f, 0f)));
            RectTransform textRt = Node("Label", rt);
            Stretch(textRt);
            button.targetGraphic = Txt(textRt, label, font, size, color, TextAlignmentOptions.Center, spacing);
            return button;
        }

        public static Button BackButton(RectTransform parent)
        {
            RectTransform rt = Place(Node("BackButton", parent), new Vector2(0f, 1f), new Vector2(28f, -24f), new Vector2(64f, 64f));
            Button button = MakeButton(Box(rt, ButtonFill, new Color(1f, 1f, 1f, 0.22f), 20f, Border2));
            Centered(rt, "Icon", Back, Color.white, 26f);
            return button;
        }

        public static Button GoldButton(RectTransform rt, string label, Sprite icon, Vector2 iconSize, float fontSize, float radius = 18f)
        {
            Image background = Rounded(rt, Panel, ShopColors.Gold, radius);
            Drop(background, ShopColors.GoldShadow, 6f);
            Button button = MakeButton(background);
            Row(rt, 14f, TextAnchor.MiddleCenter);
            if (icon != null) Fixed(IconImage(rt, "Icon", icon, ShopColors.Ink), iconSize.x, iconSize.y);
            Txt(Node("Label", rt), label, Display, fontSize, ShopColors.Ink, TextAlignmentOptions.Center);
            return button;
        }

        public static Button SecondaryButton(RectTransform rt, string label, Sprite icon, float fontSize = 19f)
        {
            Button button = MakeButton(Box(rt, new Color(1f, 1f, 1f, 0.08f), new Color(1f, 1f, 1f, 0.22f), 16f, Border2));
            Row(rt, 10f, TextAnchor.MiddleCenter);
            if (icon != null) Fixed(IconImage(rt, "Icon", icon, Color.white), 22f, 22f);
            Txt(Node("Label", rt), label, Bold, fontSize, Color.white, TextAlignmentOptions.Center);
            return button;
        }
    }
}
