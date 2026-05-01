// Space Trader 5000 – Android/Unity Port
// System Info screen: current system details + trade prices table.

using TMPro;
using UnityEngine;
using static SpaceTrader.GameConstants;

namespace SpaceTrader.UI.Screens
{
    public class SystemInfoUI : MonoBehaviour, IScreenUI
    {
        public void Initialize(GameObject panel)
        {
            UIFactory.Stretch(panel.GetComponent<RectTransform>());
            UIFactory.Header(panel.transform, "SYSTEM INFO",
                () => UIManager.Instance.NavigateBack());

            var infoPanel = UIFactory.Panel(panel.transform, "InfoBlock", ColorTheme.PanelBg);
            UIFactory.SetAnchored(infoPanel.GetComponent<RectTransform>(),
                new Vector2(0, 0.72f), new Vector2(1, 0.88f), new Vector2(8, 4), new Vector2(-8, -4));
            var infoText = UIFactory.LabelWrap(infoPanel.transform, "Info", "",
                ColorTheme.FontSmall, ColorTheme.TextPrimary, TextAlignmentOptions.Left);
            infoText.name = "InfoText";
            UIFactory.Stretch(infoText.rectTransform, 8, 8, 4, 4);

            var colHdr = UIFactory.Panel(panel.transform, "ColHdr", ColorTheme.HeaderBg);
            UIFactory.SetAnchored(colHdr.GetComponent<RectTransform>(),
                new Vector2(0, 0.68f), new Vector2(1, 0.72f), Vector2.zero, Vector2.zero);

            string[] hdrs   = { "Item", "Buy", "Sell", "Qty" };
            float[]  widths = { 0.40f,  0.20f,  0.20f, 0.20f };
            float x = 0;
            foreach (var (h, w) in System.Linq.Enumerable.Zip(hdrs, widths, (a, b) => (a, b)))
            {
                var lbl = UIFactory.Label(colHdr.transform, h, h,
                    ColorTheme.FontSmall, ColorTheme.TextSecondary, TextAlignmentOptions.Left);
                UIFactory.SetAnchored(lbl.rectTransform,
                    new Vector2(x, 0), new Vector2(x + w, 1), new Vector2(4, 2), new Vector2(-4, -2));
                x += w;
            }

            var (scroll, content) = UIFactory.ScrollView(panel.transform, "PriceList");
            UIFactory.SetAnchored(scroll.GetComponent<RectTransform>(),
                new Vector2(0, 0.02f), new Vector2(1, 0.68f), Vector2.zero, Vector2.zero);

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

        public void OnShow()
        {
            var G   = GameState.Instance;
            var sys = G.CurrentSystem;
            var pol = GameData.PoliticsTypes[sys.Politics];

            var panel    = transform;
            var infoText = panel.Find("InfoBlock/InfoText")?.GetComponent<TextMeshProUGUI>();
            if (infoText != null)
            {
                string status = sys.Status > Uneventful
                    ? $"  Status: {GameData.StatusDescriptions[sys.Status]}"
                    : "";
                string resources = sys.SpecialResources > 0
                    ? $"\n  Resources: {GameData.SpecialResources[sys.SpecialResources]}"
                    : "";
                infoText.text =
                    $"System: {GameData.SolarSystemNames[sys.NameIndex]}  " +
                    $"Tech: {GameData.TechLevelNames[sys.TechLevel]}\n" +
                    $"Government: {pol.Name}  Size: {GameData.SystemSize[sys.Size]}" +
                    status + resources;
            }

            var content = panel.Find("PriceList/Viewport/Content");
            if (content == null) return;

            for (int i = 0; i < MaxTradeItem; i++)
            {
                long bp  = G.BuyPrice[i];
                long sp  = G.SellPrice[i];
                int  qty = sys.Qty[i];

                var buyL  = content.Find($"Row{i}/Buy{i}")?.GetComponent<TextMeshProUGUI>();
                var sellL = content.Find($"Row{i}/Sell{i}")?.GetComponent<TextMeshProUGUI>();
                var qtyL  = content.Find($"Row{i}/Qty{i}")?.GetComponent<TextMeshProUGUI>();

                if (buyL  != null) buyL.text  = bp > 0 ? UIFactory.Cr(bp) : "---";
                if (sellL != null) sellL.text  = sp > 0 ? UIFactory.Cr(sp) : "---";
                if (qtyL  != null) qtyL.text   = qty > 0 ? qty.ToString() : "---";
            }
        }
    }
}
