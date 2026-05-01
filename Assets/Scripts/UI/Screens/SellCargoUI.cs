// Space Trader 5000 – Android/Unity Port
// Sell Cargo screen: sell goods from hold, dump or jettison.

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static SpaceTrader.GameConstants;

namespace SpaceTrader.UI.Screens
{
    public class SellCargoUI : MonoBehaviour, IScreenUI
    {
        TextMeshProUGUI _creditsText, _baysText;
        Transform _listContent;
        readonly List<RowData> _rows = new();

        struct RowData
        {
            public TextMeshProUGUI Name, SellPrice, Held, Profit;
            public UnityEngine.UI.Button SellBtn;
            public int Index;
        }

        public void Initialize(GameObject panel)
        {
            UIFactory.Stretch(panel.GetComponent<RectTransform>());
            UIFactory.Header(panel.transform, "SELL CARGO",
                () => UIManager.Instance.NavigateBack());

            var strip = UIFactory.Panel(panel.transform, "Strip", ColorTheme.RowBg);
            UIFactory.SetAnchored(strip.GetComponent<RectTransform>(),
                new Vector2(0, 0.88f), new Vector2(1, 0.935f), Vector2.zero, Vector2.zero);

            _creditsText = UIFactory.Label(strip.transform, "Credits", "",
                ColorTheme.FontBody, ColorTheme.TextPositive, TextAlignmentOptions.Left);
            UIFactory.Stretch(_creditsText.rectTransform, 12, 12, 4, 4);

            _baysText = UIFactory.Label(strip.transform, "Bays", "",
                ColorTheme.FontBody, ColorTheme.TextSecondary, TextAlignmentOptions.Right);
            UIFactory.Stretch(_baysText.rectTransform, 12, 12, 4, 4);

            // Column headers
            var colHdr = UIFactory.Panel(panel.transform, "ColHdr", ColorTheme.HeaderBg);
            UIFactory.SetAnchored(colHdr.GetComponent<RectTransform>(),
                new Vector2(0, 0.84f), new Vector2(1, 0.88f), Vector2.zero, Vector2.zero);
            string[] hdrs = { "Item", "Sell", "Held", "Profit", "" };
            float[] widths = { 0.28f, 0.20f, 0.12f, 0.20f, 0.20f };
            float x = 0;
            foreach (var (h, w) in System.Linq.Enumerable.Zip(hdrs, widths, (a, b) => (a, b)))
            {
                var lbl = UIFactory.Label(colHdr.transform, h, h,
                    ColorTheme.FontSmall, ColorTheme.TextSecondary, TextAlignmentOptions.Left);
                UIFactory.SetAnchored(lbl.rectTransform,
                    new Vector2(x, 0), new Vector2(x + w, 1), new Vector2(4, 2), new Vector2(-4, -2));
                x += w;
            }

            var (scroll, content) = UIFactory.ScrollView(panel.transform, "SellList");
            UIFactory.SetAnchored(scroll.GetComponent<RectTransform>(),
                new Vector2(0, 0.02f), new Vector2(1, 0.84f), Vector2.zero, Vector2.zero);
            _listContent = content;

            BuildRows();
        }

        void BuildRows()
        {
            foreach (Transform child in _listContent) Destroy(child.gameObject);
            _rows.Clear();

            for (int i = 0; i < MaxTradeItem; i++)
            {
                int idx = i;
                var row = UIFactory.RowPanel(_listContent, $"Row{i}",
                    i % 2 == 0 ? ColorTheme.RowBg : ColorTheme.RowAlt, 80);

                var d = new RowData { Index = i };

                d.Name = UIFactory.Label(row.transform, "Name", GameData.Tradeitems[i].Name,
                    ColorTheme.FontSmall, ColorTheme.TextPrimary);
                UIFactory.SetAnchored(d.Name.rectTransform,
                    new Vector2(0, 0), new Vector2(0.28f, 1), new Vector2(8, 4), new Vector2(-4, -4));

                d.SellPrice = UIFactory.Label(row.transform, "Price", "---",
                    ColorTheme.FontSmall, ColorTheme.TextPositive, TextAlignmentOptions.Right);
                UIFactory.SetAnchored(d.SellPrice.rectTransform,
                    new Vector2(0.28f, 0), new Vector2(0.48f, 1), new Vector2(4, 4), new Vector2(-4, -4));

                d.Held = UIFactory.Label(row.transform, "Held", "0",
                    ColorTheme.FontSmall, ColorTheme.TextSecondary, TextAlignmentOptions.Center);
                UIFactory.SetAnchored(d.Held.rectTransform,
                    new Vector2(0.48f, 0), new Vector2(0.60f, 1), new Vector2(4, 4), new Vector2(-4, -4));

                d.Profit = UIFactory.Label(row.transform, "Profit", "",
                    ColorTheme.FontSmall, ColorTheme.TextPositive, TextAlignmentOptions.Right);
                UIFactory.SetAnchored(d.Profit.rectTransform,
                    new Vector2(0.60f, 0), new Vector2(0.80f, 1), new Vector2(4, 4), new Vector2(-4, -4));

                d.SellBtn = UIFactory.SmallBtn(row.transform, "SellBtn", "SELL",
                    () => OnSell(idx), ColorTheme.ButtonSuccess);
                UIFactory.SetAnchored(d.SellBtn.GetComponent<UnityEngine.RectTransform>(),
                    new Vector2(0.80f, 0.1f), new Vector2(1f, 0.9f), new Vector2(4, 0), new Vector2(-8, 0));

                _rows.Add(d);
            }
        }

        public void OnShow() => Refresh();

        void Refresh()
        {
            var G = GameState.Instance;
            _creditsText.text = UIFactory.Cr(G.Credits);
            _baysText.text    = $"Bays: {CargoSystem.FreeCargoBays()}/{CargoSystem.TotalCargoBays()}";

            for (int i = 0; i < MaxTradeItem; i++)
            {
                var d     = _rows[i];
                int held  = G.Ship.Cargo[i];
                long sp   = G.SellPrice[i];
                bool hasCargo = held > 0;

                d.Name.color = ColorTheme.TextPrimary;
                d.Held.text  = held.ToString();

                if (hasCargo && sp > 0)
                {
                    d.SellPrice.text  = UIFactory.Cr(sp);
                    long profitPer    = sp - G.BuyingPrice[i];
                    d.Profit.text     = (profitPer >= 0 ? "+" : "") + UIFactory.Cr(profitPer);
                    d.Profit.color    = profitPer >= 0 ? ColorTheme.TextPositive : ColorTheme.TextNegative;
                    d.SellBtn.interactable = true;
                }
                else
                {
                    d.SellPrice.text  = sp > 0 ? UIFactory.Cr(sp) : "---";
                    d.Profit.text     = "";
                    d.SellBtn.interactable = hasCargo;
                }
            }
        }

        void OnSell(int idx)
        {
            int held = GameState.Instance.Ship.Cargo[idx];
            if (held <= 0) return;
            CargoSystem.SellCargo(idx, held, SellCargo);
            Refresh();
        }
    }
}
