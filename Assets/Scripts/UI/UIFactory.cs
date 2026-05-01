// Space Trader 5000 – Android/Unity Port
// Programmatic UI element factory. All screens use these helpers so the
// visual style stays consistent without requiring prefabs.

using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SpaceTrader.UI
{
    public static class UIFactory
    {
        // ── Layout helpers ────────────────────────────────────────────────────

        public static void Stretch(RectTransform rt, float l = 0, float r = 0, float t = 0, float b = 0)
        {
            rt.anchorMin  = Vector2.zero;
            rt.anchorMax  = Vector2.one;
            rt.offsetMin  = new Vector2(l, b);
            rt.offsetMax  = new Vector2(-r, -t);
        }

        public static void SetAnchored(RectTransform rt,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin; rt.offsetMax = offsetMax;
        }

        public static void Pin(RectTransform rt, TextAnchor anchor, float w, float h, float x = 0, float y = 0)
        {
            var (ax, ay) = anchor switch
            {
                TextAnchor.UpperLeft    => (0f, 1f), TextAnchor.UpperCenter => (0.5f, 1f),
                TextAnchor.UpperRight   => (1f, 1f), TextAnchor.MiddleLeft  => (0f, 0.5f),
                TextAnchor.MiddleCenter => (0.5f, 0.5f), TextAnchor.MiddleRight => (1f, 0.5f),
                TextAnchor.LowerLeft    => (0f, 0f), TextAnchor.LowerCenter => (0.5f, 0f),
                _                       => (1f, 0f),
            };
            rt.anchorMin = rt.anchorMax = new Vector2(ax, ay);
            rt.pivot     = new Vector2(ax, ay);
            rt.anchoredPosition = new Vector2(x, y);
            rt.sizeDelta = new Vector2(w, h);
        }

        // ── Containers ────────────────────────────────────────────────────────

        public static GameObject Panel(Transform parent, string name, Color color = default)
        {
            var go  = new GameObject(name);
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = color == default ? ColorTheme.PanelBg : color;
            return go;
        }

        public static GameObject TransparentPanel(Transform parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            return go;
        }

        // ── Text ─────────────────────────────────────────────────────────────

        public static TextMeshProUGUI Label(Transform parent, string name,
            string text = "", int size = ColorTheme.FontBody, Color color = default,
            TextAlignmentOptions align = TextAlignmentOptions.Left)
        {
            var go  = new GameObject(name);
            go.transform.SetParent(parent, false);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text             = text;
            tmp.fontSize         = size;
            tmp.color            = color == default ? ColorTheme.TextPrimary : color;
            tmp.alignment        = align;
            tmp.overflowMode     = TextOverflowModes.Ellipsis;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            return tmp;
        }

        public static TextMeshProUGUI LabelWrap(Transform parent, string name,
            string text = "", int size = ColorTheme.FontBody, Color color = default,
            TextAlignmentOptions align = TextAlignmentOptions.Left)
        {
            var tmp = Label(parent, name, text, size, color, align);
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.overflowMode     = TextOverflowModes.Overflow;
            return tmp;
        }

        // ── Buttons ───────────────────────────────────────────────────────────

        public static Button Btn(Transform parent, string name, string label,
            UnityAction onClick, Color bgColor = default, int fontSize = ColorTheme.FontButton)
        {
            var go  = Panel(parent, name, bgColor == default ? ColorTheme.ButtonNormal : bgColor);
            var btn = go.AddComponent<Button>();

            var colors = btn.colors;
            colors.normalColor      = Color.white;
            colors.highlightedColor = new Color(1.3f, 1.3f, 1.3f);
            colors.pressedColor     = new Color(0.7f, 0.7f, 0.7f);
            colors.disabledColor    = new Color(0.5f, 0.5f, 0.5f);
            btn.colors              = colors;

            var txt = Label(go.transform, "Label", label, fontSize,
                            ColorTheme.TextPrimary, TextAlignmentOptions.Center);
            Stretch(txt.GetComponent<RectTransform>(), 4, 4, 4, 4);

            if (onClick != null) btn.onClick.AddListener(onClick);
            return btn;
        }

        public static Button SmallBtn(Transform parent, string name, string label,
            UnityAction onClick, Color bgColor = default)
            => Btn(parent, name, label, onClick, bgColor, ColorTheme.FontButtonSm);

        // ── Input Field ───────────────────────────────────────────────────────

        public static TMP_InputField InputField(Transform parent, string name, string placeholder)
        {
            var go  = Panel(parent, name, ColorTheme.RowBg);
            var inp = go.AddComponent<TMP_InputField>();

            var phGo  = new GameObject("Placeholder"); phGo.transform.SetParent(go.transform, false);
            var phTmp = phGo.AddComponent<TextMeshProUGUI>();
            phTmp.text      = placeholder;
            phTmp.fontSize  = ColorTheme.FontBody;
            phTmp.color     = ColorTheme.TextDisabled;
            phTmp.fontStyle = FontStyles.Italic;
            Stretch(phTmp.rectTransform, 8, 8, 4, 4);

            var txtGo  = new GameObject("Text"); txtGo.transform.SetParent(go.transform, false);
            var txtTmp = txtGo.AddComponent<TextMeshProUGUI>();
            txtTmp.fontSize = ColorTheme.FontBody;
            txtTmp.color    = ColorTheme.TextPrimary;
            Stretch(txtTmp.rectTransform, 8, 8, 4, 4);

            inp.textComponent  = txtTmp;
            inp.placeholder    = phTmp;
            inp.characterLimit = GameConstants.NameLen;
            return inp;
        }

        // ── Scroll view ───────────────────────────────────────────────────────

        public static (ScrollRect scroll, Transform content) ScrollView(Transform parent, string name)
        {
            var go = Panel(parent, name, Color.clear);
            var sr = go.AddComponent<ScrollRect>();
            sr.horizontal = false;

            var viewport = new GameObject("Viewport"); viewport.transform.SetParent(go.transform, false);
            var vmask    = viewport.AddComponent<Mask>();
            vmask.showMaskGraphic = false;
            var vimg = viewport.AddComponent<Image>(); vimg.color = Color.clear;
            var vrt  = viewport.GetComponent<RectTransform>();
            Stretch(vrt);

            var content = new GameObject("Content"); content.transform.SetParent(viewport.transform, false);
            var vlg = content.AddComponent<VerticalLayoutGroup>();
            vlg.childControlWidth     = true;  vlg.childControlHeight     = false;
            vlg.childForceExpandWidth = true;  vlg.childForceExpandHeight = false;
            vlg.spacing = 2;
            var csf = content.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var crt = content.GetComponent<RectTransform>();
            crt.anchorMin = new Vector2(0, 1); crt.anchorMax = new Vector2(1, 1);
            crt.pivot     = new Vector2(0.5f, 1);
            crt.offsetMin = Vector2.zero; crt.offsetMax = Vector2.zero;

            sr.viewport          = vrt;
            sr.content           = crt;
            sr.scrollSensitivity = 40;

            return (sr, content.transform);
        }

        // ── Divider ───────────────────────────────────────────────────────────

        public static Image Divider(Transform parent, string name = "Divider")
        {
            var go  = new GameObject(name); go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>(); img.color = ColorTheme.Divider;
            var rt  = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, 2);
            rt.anchorMin = new Vector2(0, 0.5f); rt.anchorMax = new Vector2(1, 0.5f);
            return img;
        }

        // ── Progress bar ─────────────────────────────────────────────────────

        public static Slider ProgressBar(Transform parent, string name, Color fillColor)
        {
            var go  = Panel(parent, name, ColorTheme.BarBackground);
            var sld = go.AddComponent<Slider>();
            sld.minValue     = 0; sld.maxValue = 1; sld.value = 1;
            sld.direction    = Slider.Direction.LeftToRight;
            sld.interactable = false;

            var fill = Panel(go.transform, "Fill", fillColor);
            var frt  = fill.GetComponent<RectTransform>();
            frt.anchorMin = Vector2.zero; frt.anchorMax = new Vector2(1, 1);

            var fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(go.transform, false);
            fillArea.AddComponent<RectTransform>();
            Stretch(fillArea.GetComponent<RectTransform>());
            fill.transform.SetParent(fillArea.transform, false);
            Stretch(frt);

            sld.fillRect = frt;
            return sld;
        }

        // ── Header bar ────────────────────────────────────────────────────────

        // Creates a horizontal header with a back button on the left and a title label.
        // Returns the title TextMeshProUGUI for later updates.
        public static TextMeshProUGUI Header(Transform parent, string title,
            UnityAction onBack, float height = 90)
        {
            var bar = Panel(parent, "Header", ColorTheme.HeaderBg);
            var brt = bar.GetComponent<RectTransform>();
            brt.anchorMin = new Vector2(0, 1); brt.anchorMax = Vector2.one;
            brt.offsetMin = new Vector2(0, -height); brt.offsetMax = Vector2.zero;

            if (onBack != null)
            {
                var back = Btn(bar.transform, "BackBtn", "<", onBack,
                               ColorTheme.ButtonNormal, ColorTheme.FontBody);
                Pin(back.GetComponent<RectTransform>(), TextAnchor.MiddleLeft, height, height, 0, 0);
            }

            var lbl = Label(bar.transform, "TitleLabel", title,
                            ColorTheme.FontHeader, ColorTheme.TextAccent, TextAlignmentOptions.Center);
            Stretch(lbl.rectTransform, onBack != null ? height : 0, 0, 4, 4);
            return lbl;
        }

        // ── Formatted credit string ───────────────────────────────────────────

        public static string Cr(long amount) => $"{amount:N0} cr.";
    }
}
