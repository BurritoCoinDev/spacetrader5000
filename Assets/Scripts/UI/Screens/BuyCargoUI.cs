// Space Trader 5000 – Android/Unity Port
// Buy Cargo screen: browse available goods, buy by quantity.

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static SpaceTrader.GameConstants;

namespace SpaceTrader.UI.Screens
{
    public class BuyCargoUI : MonoBehaviour, IScreenUI
    {
        TextMeshProUGUI _creditsText, _baysText;
        Transform _listContent;
        readonly List<RowWidgets> _rows = new();

        struct RowWidgets
        {
            public TextMeshProUGUI Name, Price, Avail, Held;
            public Button DecBtn, IncBtn;
            public TextMeshProUGUI QtyLabel;
            public int[] PendingQty; // single-element array for ref capture
        }

        public void Initialize(GameObject panel)
        {
            UIFactory.Stretch(panel.GetComponent<RectTransform>());
            UIFactory.Header(panel.transform, "BUY CARGO",
                () => UIManager.Instance.NavigateBack());

            // Summary strip
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
            BuildColumnHeaders(colHdr.transform);

            // Scroll list
            var (scroll, content) = UIFactory.ScrollView(panel.transform, "CargoList");
            UIFactory.SetAnchored(scroll.GetComponent<RectTransform>(),
                new Vector2(0, 0.10f), new Vector2(1, 0.84f), Vector2.zero, Vector2.zero);
            _listContent = content;

            // Buy All / Clear buttons
            var btnRow = UIFactory.TransparentPanel(panel.transform, "BtnRow");
            UIFactory.SetAnchored(btnRow.GetComponent<RectTransform>(),
                new Vector2(0.02f, 0.02f), new Vector2(0.98f, 0.10f), Vector2.zero, Vector2.zero);

            var buyAllBtn = UIFactory.Btn(btnRow.transform, "BuyAll", "BUY ALL",
                OnBuyAll, ColorTheme.ButtonSuccess);
            UIFactory.SetAnchored(buyAllBtn.GetComponent<RectTransform>(),
                Vector2.zero, new Vector2(0.48f, 1), Vector2.zero, Vector2.zero);

            var clearBtn = UIFactory.Btn(btnRow.transform, "Clear", "CLEAR",
                OnClear, ColorTheme.ButtonDanger);
            UIFactory.SetAnchored(clearBtn.GetComponent<RectTransform>(),
                new Vector2(0.52f, 0), Vector2.one, Vector2.zero, Vector2.zero);

            BuildRows();
        }

        void BuildColumnHeaders(Transform parent)
        {
            string[] headers = { "Item", "Price", "Avail", "Hold", "Qty" };
            float[] widths   = { 0.27f,  0.18f,   0.14f,  0.14f,  0.27f };
            float x = 0;
            for (int i = 0; i < headers.Length; i++)
            {
                var lbl = UIFactory.Label(parent, headers[i], headers[i],
                    ColorTheme.FontSmall, ColorTheme.TextSecondary,
                    i < 2 ? TextAlignmentOptions.Left : TextAlignmentOptions.Center);
                UIFactory.SetAnchored(lbl.rectTransform,
                    new Vector2(x, 0), new Vector2(x + widths[i], 1),
                    new Vector2(4, 2), new Vector2(-4, -2));
                x += widths[i];
            }
        }

        void BuildRows()
        {
            foreach (Transform child in _listContent)
                Destroy(child.gameObject);
            _rows.Clear();

            for (int i = 0; i < MaxTradeItem; i++)
            {
                int idx = i;
                var row = UIFactory.RowPanel(_listContent, $"Row{i}",
                    i % 2 == 0 ? ColorTheme.RowBg : ColorTheme.RowAlt, 80);

                var w = new RowWidgets { PendingQty = new[] { 0 } };

                w.Name = UIFactory.Label(row.transform, "Name", GameData.Tradeitems[i].Name,
                    ColorTheme.FontSmall, ColorTheme.TextPrimary);
                UIFactory.SetAnchored(w.Name.rectTransform,
                    new Vector2(0, 0), new Vector2(0.27f, 1), new Vector2(8, 4), new Vector2(-4, -4));

                w.Price = UIFactory.Label(row.transform, "Price", "---",
                    ColorTheme.FontSmall, ColorTheme.TextPositive, TextAlignmentOptions.Right);
                UIFactory.SetAnchored(w.Price.rectTransform,
                    new Vector2(0.27f, 0), new Vector2(0.45f, 1), new Vector2(4, 4), new Vector2(-4, -4));

                w.Avail = UIFactory.Label(row.transform, "Avail", "---",
                    ColorTheme.FontSmall, ColorTheme.TextSecondary, TextAlignmentOptions.Center);
                UIFactory.SetAnchored(w.Avail.rectTransform,
                    new Vector2(0.45f, 0), new Vector2(0.59f, 1), new Vector2(4, 4), new Vector2(-4, -4));

                w.Held = UIFactory.Label(row.transform, "Held", "0",
                    ColorTheme.FontSmall, ColorTheme.TextSecondary, TextAlignmentOptions.Center);
                UIFactory.SetAnchored(w.Held.rectTransform,
                    new Vector2(0.59f, 0), new Vector2(0.73f, 1), new Vector2(4, 4), new Vector2(-4, -4));

                w.DecBtn = UIFactory.SmallBtn(row.transform, "Dec", "-",
                    () => AdjustQty(idx, -1));
                UIFactory.SetAnchored(w.DecBtn.GetComponent<RectTransform>(),
                    new Vector2(0.73f, 0.1f), new Vector2(0.83f, 0.9f), Vector2.zero, Vector2.zero);

                w.QtyLabel = UIFactory.Label(row.transform, "Qty", "0",
                    ColorTheme.FontBody, ColorTheme.TextAccent, TextAlignmentOptions.Center);
                UIFactory.SetAnchored(w.QtyLabel.rectTransform,
                    new Vector2(0.83f, 0), new Vector2(0.91f, 1), new Vector2(2, 4), new Vector2(-2, -4));

                w.IncBtn = UIFactory.SmallBtn(row.transform, "Inc", "+",
                    () => AdjustQty(idx, 1));
                UIFactory.SetAnchored(w.IncBtn.GetComponent<RectTransform>(),
                    new Vector2(0.91f, 0.1f), new Vector2(1f, 0.9f), new Vector2(2, 0), new Vector2(-4, 0));

                _rows.Add(w);
            }
        }

        public void OnShow() => Refresh();

        void Refresh()
        {
            var G   = GameState.Instance;
            var sys = G.CurrentSystem;
            _creditsText.text = UIFactory.Cr(G.Credits);
            _baysText.text    = $"Bays: {CargoSystem.FreeCargoBays()}/{CargoSystem.TotalCargoBays()}";

            for (int i = 0; i < MaxTradeItem; i++)
            {
                var w     = _rows[i];
                long bp   = G.BuyPrice[i];
                int avail = sys.Qty[i];
                int held  = G.Ship.Cargo[i];
                bool canBuy = bp > 0 && avail > 0;

                w.Price.text  = canBuy ? UIFactory.Cr(bp) : "---";
                w.Price.color = canBuy ? ColorTheme.TextPositive : ColorTheme.TextDisabled;
                w.Avail.text  = canBuy ? avail.ToString() : "---";
                w.Avail.color = canBuy ? ColorTheme.TextSecondary : ColorTheme.TextDisabled;
                w.Held.text   = held > 0 ? held.ToString() : "0";

                w.PendingQty[0] = 0;
                w.QtyLabel.text = "0";
                w.DecBtn.interactable = canBuy;
                w.IncBtn.interactable = canBuy;
                w.Name.color = canBuy ? ColorTheme.TextPrimary : ColorTheme.TextDisabled;
            }
        }

        void AdjustQty(int idx, int delta)
        {
            var G   = GameState.Instance;
            var w   = _rows[idx];
            int cur = w.PendingQty[0];
            int max = CargoSystem.GetAmountToBuy(idx);

            int newQty = Mathf.Clamp(cur + delta, 0, max);
            w.PendingQty[0] = newQty;
            w.QtyLabel.text = newQty.ToString();

            int diff = newQty - cur;
            if (diff > 0)      CargoSystem.BuyCargo(idx, diff);
            else if (diff < 0) CargoSystem.SellCargo(idx, -diff, GameConstants.SellCargo);

            _creditsText.text = UIFactory.Cr(G.Credits);
            _baysText.text    = $"Bays: {CargoSystem.FreeCargoBays()}/{CargoSystem.TotalCargoBays()}";
            w.Held.text       = G.Ship.Cargo[idx].ToString();
            w.Avail.text      = G.CurrentSystem.Qty[idx].ToString();
        }

        void OnBuyAll()
        {
            for (int i = 0; i < MaxTradeItem; i++)
            {
                int max = CargoSystem.GetAmountToBuy(i);
                if (max > 0) CargoSystem.BuyCargo(i, max);
            }
            Refresh();
        }

        void OnClear()
        {
            for (int i = 0; i < MaxTradeItem; i++)
            {
                if (_rows[i].PendingQty[0] > 0)
                {
                    CargoSystem.SellCargo(i, _rows[i].PendingQty[0], GameConstants.SellCargo);
                    _rows[i].PendingQty[0] = 0;
                }
            }
            Refresh();
        }
    }
}
