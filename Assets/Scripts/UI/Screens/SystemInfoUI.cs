// Space Trader 5000 – Android/Unity Port
// System Info screen: system details matching original Palm OS layout.

using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static SpaceTrader.GameConstants;

namespace SpaceTrader.UI.Screens
{
    public class SystemInfoUI : MonoBehaviour, IScreenUI
    {
        // Info label references (value side only)
        TextMeshProUGUI _valName, _valSize, _valTech, _valGov, _valRes, _valPolice, _valPirates;
        TextMeshProUGUI _statusText;

        public void Initialize(GameObject panel)
        {
            UIFactory.Stretch(panel.GetComponent<RectTransform>());
            UIFactory.Header(panel.transform, "SYSTEM INFO",
                () => UIManager.Instance.NavigateBack());

            // ── Info block ───────────────────────────────────────────────────────
            var infoPanel = UIFactory.Panel(panel.transform, "InfoBlock", ColorTheme.PanelBg);
            UIFactory.SetAnchored(infoPanel.GetComponent<RectTransform>(),
                new Vector2(0, 0.42f), new Vector2(1, 0.88f),
                new Vector2(8, 4), new Vector2(-8, -4));

            // Seven labeled rows, evenly spaced in the top 80% of infoPanel
            float rowH = 0.80f / 7f;
            _valName   = AddInfoRow(infoPanel.transform, "Name",       "Name:",       0, rowH);
            _valSize   = AddInfoRow(infoPanel.transform, "Size",       "Size:",       1, rowH);
            _valTech   = AddInfoRow(infoPanel.transform, "Tech level:", "TechLevel",  2, rowH);
            _valGov    = AddInfoRow(infoPanel.transform, "Government:","Gov",         3, rowH);
            _valRes    = AddInfoRow(infoPanel.transform, "Resources:", "Res",         4, rowH);
            _valPolice = AddInfoRow(infoPanel.transform, "Police:",    "Police",      5, rowH);
            _valPirates= AddInfoRow(infoPanel.transform, "Pirates:",   "Pirates",     6, rowH);

            // Status sentence in the bottom 20% of infoPanel
            _statusText = UIFactory.LabelWrap(infoPanel.transform, "Status", "",
                ColorTheme.FontSmall, ColorTheme.TextSecondary, TextAlignmentOptions.Left);
            UIFactory.SetAnchored(_statusText.rectTransform,
                new Vector2(0, 0), new Vector2(1, 0.20f),
                new Vector2(8, 2), new Vector2(-8, -2));

            // ── Price table header ───────────────────────────────────────────────
            var colHdr = UIFactory.Panel(panel.transform, "ColHdr", ColorTheme.HeaderBg);
            UIFactory.SetAnchored(colHdr.GetComponent<RectTransform>(),
                new Vector2(0, 0.37f), new Vector2(1, 0.42f), Vector2.zero, Vector2.zero);

            string[] hdrs   = { "Item", "Buy", "Sell", "Qty" };
            float[]  widths = { 0.40f,  0.20f, 0.20f,  0.20f };
            float x = 0;
            foreach (var (h, w) in System.Linq.Enumerable.Zip(hdrs, widths, (a, b) => (a, b)))
            {
                var lbl = UIFactory.Label(colHdr.transform, h, h,
                    ColorTheme.FontSmall, ColorTheme.TextSecondary, TextAlignmentOptions.Left);
                UIFactory.SetAnchored(lbl.rectTransform,
                    new Vector2(x, 0), new Vector2(x + w, 1), new Vector2(4, 2), new Vector2(-4, -2));
                x += w;
            }

            // ── Price list ───────────────────────────────────────────────────────
            var (scroll, content) = UIFactory.ScrollView(panel.transform, "PriceList");
            UIFactory.SetAnchored(scroll.GetComponent<RectTransform>(),
                new Vector2(0, 0.02f), new Vector2(1, 0.37f), Vector2.zero, Vector2.zero);

            for (int i = 0; i < MaxTradeItem; i++)
            {
                var row = UIFactory.RowPanel(content, $"Row{i}",
                    i % 2 == 0 ? ColorTheme.RowBg : ColorTheme.RowAlt, 72);

                var nameL = UIFactory.Label(row.transform, "Name", GameData.Tradeitems[i].Name,
                    ColorTheme.FontSmall, ColorTheme.TextPrimary);
                UIFactory.SetAnchored(nameL.rectTransform,
                    new Vector2(0, 0), new Vector2(0.40f, 1), new Vector2(8, 4), new Vector2(-4, -4));

                var buyL = UIFactory.Label(row.transform, "Buy", "---",
                    ColorTheme.FontSmall, ColorTheme.TextPositive, TextAlignmentOptions.Left);
                buyL.name = $"Buy{i}";
                UIFactory.SetAnchored(buyL.rectTransform,
                    new Vector2(0.40f, 0), new Vector2(0.60f, 1), new Vector2(4, 4), new Vector2(-4, -4));

                var sellL = UIFactory.Label(row.transform, "Sell", "---",
                    ColorTheme.FontSmall, ColorTheme.TextSecondary, TextAlignmentOptions.Left);
                sellL.name = $"Sell{i}";
                UIFactory.SetAnchored(sellL.rectTransform,
                    new Vector2(0.60f, 0), new Vector2(0.80f, 1), new Vector2(4, 4), new Vector2(-4, -4));

                var qtyL = UIFactory.Label(row.transform, "Qty", "---",
                    ColorTheme.FontSmall, ColorTheme.TextSecondary, TextAlignmentOptions.Center);
                qtyL.name = $"Qty{i}";
                UIFactory.SetAnchored(qtyL.rectTransform,
                    new Vector2(0.80f, 0), new Vector2(1f, 1), new Vector2(4, 4), new Vector2(-4, -4));
            }
        }

        // Creates a bold label on the left and a value label on the right within the info panel.
        TextMeshProUGUI AddInfoRow(Transform parent, string labelText, string valueName,
            int rowIndex, float rowH)
        {
            float yTop = 1f - rowIndex * rowH;
            float yBot = yTop - rowH;

            var lbl = UIFactory.Label(parent, $"Lbl{valueName}", labelText,
                ColorTheme.FontSmall, ColorTheme.TextAccent, TextAlignmentOptions.Left);
            UIFactory.SetAnchored(lbl.rectTransform,
                new Vector2(0, yBot), new Vector2(0.42f, yTop),
                new Vector2(8, 2), new Vector2(-4, -2));

            var val = UIFactory.Label(parent, $"Val{valueName}", "",
                ColorTheme.FontSmall, ColorTheme.TextPrimary, TextAlignmentOptions.Left);
            UIFactory.SetAnchored(val.rectTransform,
                new Vector2(0.42f, yBot), new Vector2(1f, yTop),
                new Vector2(4, 2), new Vector2(-8, -2));

            return val;
        }

        public void OnShow()
        {
            var G   = GameState.Instance;
            var sys = G.CurrentSystem;
            var pol = GameData.PoliticsTypes[sys.Politics];

            _valName.text    = GameData.SolarSystemNames[sys.NameIndex];
            _valSize.text    = GameData.SystemSize[sys.Size];
            _valTech.text    = GameData.TechLevelNames[sys.TechLevel];
            _valGov.text     = pol.Name;
            _valRes.text     = GameData.SpecialResources[sys.SpecialResources];
            _valPolice.text  = GameData.ActivityDescriptions[pol.StrengthPolice];
            _valPirates.text = GameData.ActivityDescriptions[pol.StrengthPirates];

            _statusText.text = sys.Status == Uneventful
                ? "The system is currently under no particular pressure."
                : $"The system is currently {GameData.StatusDescriptions[sys.Status]}.";

            var content = transform.Find("PriceList/Viewport/Content");
            if (content == null) return;

            for (int i = 0; i < MaxTradeItem; i++)
            {
                long bp  = G.BuyPrice[i];
                long sp  = G.SellPrice[i];
                int  qty = sys.Qty[i];

                var buyL  = content.Find($"Row{i}/Buy{i}")?.GetComponent<TextMeshProUGUI>();
                var sellL = content.Find($"Row{i}/Sell{i}")?.GetComponent<TextMeshProUGUI>();
                var qtyL  = content.Find($"Row{i}/Qty{i}")?.GetComponent<TextMeshProUGUI>();

                if (buyL  != null) buyL.text  = bp  > 0 ? UIFactory.Cr(bp) : "---";
                if (sellL != null) sellL.text  = sp  > 0 ? UIFactory.Cr(sp) : "---";
                if (qtyL  != null) qtyL.text   = qty > 0 ? qty.ToString()   : "---";
            }
        }
    }
}
