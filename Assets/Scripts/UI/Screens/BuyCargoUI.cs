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

        readonly int[]  _origCargo        = new int[MaxTradeItem];
        readonly long[] _origBuyingPrice  = new long[MaxTradeItem];

        struct RowWidgets
        {
            public TextMeshProUGUI Name, Price, Avail, Held;
            public Button DecBtn, IncBtn, MaxBtn;
            public TextMeshProUGUI QtyLabel;
            public int[] PendingQty;
        }

        // Column x-positions shared between headers and rows
        // Name | Price | Avail | Hold | - | Qty | + | MAX
        static readonly float[] ColX = { 0f, 0.23f, 0.40f, 0.51f, 0.62f, 0.70f, 0.78f, 0.86f, 1.0f };

        public void Initialize(GameObject panel)
        {
            UIFactory.Stretch(panel.GetComponent<RectTransform>());
            UIFactory.Header(panel.transform, "BUY CARGO",
                () => UIManager.Instance.NavigateBack());

            var strip = UIFactory.Panel(panel.transform, "Strip", ColorTheme.RowBg);
            UIFactory.SetAnchored(strip.GetComponent<RectTransform>(),
                new Vector2(0, 0.88f), new Vector2(1, 0.935f), Vector2.zero, Vector2.zero);

            _creditsText = UIFactory.Label(strip.transform, "Credits", "",
                ColorTheme.FontHeader, ColorTheme.TextPositive, TextAlignmentOptions.Left);
            UIFactory.Stretch(_creditsText.rectTransform, 12, 12, 4, 4);

            _baysText = UIFactory.Label(strip.transform, "Bays", "",
                ColorTheme.FontHeader, ColorTheme.TextSecondary, TextAlignmentOptions.Right);
            UIFactory.Stretch(_baysText.rectTransform, 12, 12, 4, 4);

            var colHdr = UIFactory.Panel(panel.transform, "ColHdr", ColorTheme.HeaderBg);
            UIFactory.SetAnchored(colHdr.GetComponent<RectTransform>(),
                new Vector2(0, 0.84f), new Vector2(1, 0.88f), Vector2.zero, Vector2.zero);
            BuildColumnHeaders(colHdr.transform);

            var (scroll, content) = UIFactory.ScrollView(panel.transform, "CargoList");
            UIFactory.SetAnchored(scroll.GetComponent<RectTransform>(),
                new Vector2(0, 0.10f), new Vector2(1, 0.84f), Vector2.zero, Vector2.zero);

            // Stretch content to fill viewport so rows share space equally
            var contentRt = content.GetComponent<RectTransform>();
            contentRt.anchorMin = Vector2.zero;
            contentRt.anchorMax = Vector2.one;
            contentRt.offsetMin = Vector2.zero;
            contentRt.offsetMax = Vector2.zero;
            content.GetComponent<ContentSizeFitter>().verticalFit =
                ContentSizeFitter.FitMode.Unconstrained;
            content.GetComponent<VerticalLayoutGroup>().childForceExpandHeight = true;

            _listContent = content;

            var btnRow = UIFactory.TransparentPanel(panel.transform, "BtnRow");
            UIFactory.SetAnchored(btnRow.GetComponent<RectTransform>(),
                new Vector2(0.02f, 0.02f), new Vector2(0.98f, 0.10f), Vector2.zero, Vector2.zero);

            var buyAllBtn = UIFactory.Btn(btnRow.transform, "BuyAll", "BUY ALL",
                OnBuyAll, ColorTheme.ButtonSuccess);
            UIFactory.SetAnchored(buyAllBtn.GetComponent<RectTransform>(),
                Vector2.zero, new Vector2(0.48f, 1), Vector2.zero, Vector2.zero);

            var clearBtn = UIFactory.Btn(btnRow.transform, "Undo", "UNDO",
                OnClear, ColorTheme.ButtonDanger);
            UIFactory.SetAnchored(clearBtn.GetComponent<RectTransform>(),
                new Vector2(0.52f, 0), Vector2.one, Vector2.zero, Vector2.zero);

            BuildRows();
        }

        void BuildColumnHeaders(Transform parent)
        {
            // Headers aligned to ColX: Item, Price, Avail, Hold, then a combined "Qty / Max" section
            // Headers must match the value alignment of their column:
            // Price values are right-aligned, so the header is too.
            (string text, int col, TextAlignmentOptions align, int rightPad)[] hdrs =
            {
                ("Item",  0, TextAlignmentOptions.Left,   -4),
                ("Price", 1, TextAlignmentOptions.Right,  -12),
                ("Avail", 2, TextAlignmentOptions.Center, -4),
                ("Hold",  3, TextAlignmentOptions.Center, -4),
                ("Bought",4, TextAlignmentOptions.Center, -4),   // spans cols 4-6 (the - qty + group)
                ("Max",   7, TextAlignmentOptions.Center, -4),
            };

            // col 4 header spans across the three button columns (4,5,6) to label the qty controls
            float[] colSpanEnd = { ColX[1], ColX[2], ColX[3], ColX[4], ColX[7], ColX[8] };

            for (int i = 0; i < hdrs.Length; i++)
            {
                float xMin = ColX[hdrs[i].col];
                float xMax = colSpanEnd[i];
                var lbl = UIFactory.Label(parent, hdrs[i].text, hdrs[i].text,
                    ColorTheme.FontBody, ColorTheme.TextSecondary, hdrs[i].align);
                UIFactory.SetAnchored(lbl.rectTransform,
                    new Vector2(xMin, 0), new Vector2(xMax, 1),
                    new Vector2(4, 2), new Vector2(hdrs[i].rightPad, -2));
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

                // Override fixed height so rows fill available space equally
                var le = row.GetComponent<LayoutElement>();
                le.minHeight       = 0;
                le.preferredHeight = 0;
                le.flexibleHeight  = 1;

                var w = new RowWidgets { PendingQty = new[] { 0 } };

                w.Name = UIFactory.Label(row.transform, "Name", GameData.Tradeitems[i].Name,
                    ColorTheme.FontBody, ColorTheme.TextPrimary);
                UIFactory.SetAnchored(w.Name.rectTransform,
                    new Vector2(ColX[0], 0), new Vector2(ColX[1], 1),
                    new Vector2(8, 4), new Vector2(-4, -4));

                w.Price = UIFactory.Label(row.transform, "Price", "---",
                    ColorTheme.FontBody, ColorTheme.TextPositive, TextAlignmentOptions.Right);
                UIFactory.SetAnchored(w.Price.rectTransform,
                    new Vector2(ColX[1], 0), new Vector2(ColX[2], 1),
                    new Vector2(4, 4), new Vector2(-12, -4));

                w.Avail = UIFactory.Label(row.transform, "Avail", "---",
                    ColorTheme.FontBody, ColorTheme.TextSecondary, TextAlignmentOptions.Center);
                UIFactory.SetAnchored(w.Avail.rectTransform,
                    new Vector2(ColX[2], 0), new Vector2(ColX[3], 1),
                    new Vector2(4, 4), new Vector2(-4, -4));

                w.Held = UIFactory.Label(row.transform, "Held", "0",
                    ColorTheme.FontBody, ColorTheme.TextSecondary, TextAlignmentOptions.Center);
                UIFactory.SetAnchored(w.Held.rectTransform,
                    new Vector2(ColX[3], 0), new Vector2(ColX[4], 1),
                    new Vector2(4, 4), new Vector2(-4, -4));

                w.DecBtn = UIFactory.Btn(row.transform, "Dec", "-",
                    () => AdjustQty(idx, -1), ColorTheme.ButtonNormal);
                UIFactory.SetAnchored(w.DecBtn.GetComponent<RectTransform>(),
                    new Vector2(ColX[4], 0.1f), new Vector2(ColX[5], 0.9f),
                    new Vector2(2, 0), new Vector2(-2, 0));

                w.QtyLabel = UIFactory.Label(row.transform, "Qty", "0",
                    ColorTheme.FontHeader, ColorTheme.TextPositive, TextAlignmentOptions.Center);
                UIFactory.SetAnchored(w.QtyLabel.rectTransform,
                    new Vector2(ColX[5], 0), new Vector2(ColX[6], 1),
                    new Vector2(2, 4), new Vector2(-2, -4));

                w.IncBtn = UIFactory.Btn(row.transform, "Inc", "+",
                    () => AdjustQty(idx, 1), ColorTheme.ButtonNormal);
                UIFactory.SetAnchored(w.IncBtn.GetComponent<RectTransform>(),
                    new Vector2(ColX[6], 0.1f), new Vector2(ColX[7], 0.9f),
                    new Vector2(2, 0), new Vector2(-2, 0));

                w.MaxBtn = UIFactory.Btn(row.transform, "Max", "MAX",
                    () => BuyMax(idx), ColorTheme.ButtonSuccess);
                UIFactory.SetAnchored(w.MaxBtn.GetComponent<RectTransform>(),
                    new Vector2(ColX[7], 0.1f), new Vector2(ColX[8], 0.9f),
                    new Vector2(2, 0), new Vector2(-4, 0));

                _rows.Add(w);
            }
        }

        public void OnShow()
        {
            var G = GameState.Instance;
            for (int i = 0; i < MaxTradeItem; i++)
            {
                _origCargo[i]       = G.Ship.Cargo[i];
                _origBuyingPrice[i] = G.BuyingPrice[i];
            }
            Refresh();
        }

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
                w.MaxBtn.interactable = canBuy;
                w.Name.color = ColorTheme.TextPrimary;
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
            if (diff > 0)
            {
                CargoSystem.BuyCargo(idx, diff);
            }
            else if (diff < 0)
            {
                int amount = -diff;
                G.Credits                += G.BuyPrice[idx] * amount;
                G.Ship.Cargo[idx]        -= amount;
                G.CurrentSystem.Qty[idx] += amount;
                RecomputeBuyingPrice(idx);
            }

            _creditsText.text = UIFactory.Cr(G.Credits);
            _baysText.text    = $"Bays: {CargoSystem.FreeCargoBays()}/{CargoSystem.TotalCargoBays()}";
            w.Held.text       = G.Ship.Cargo[idx].ToString();
            w.Avail.text      = G.CurrentSystem.Qty[idx].ToString();
        }

        void BuyMax(int idx)
        {
            var G = GameState.Instance;
            var w = _rows[idx];
            // Buy as many as possible from the current state
            int additional = CargoSystem.GetAmountToBuy(idx);
            if (additional <= 0) return;
            CargoSystem.BuyCargo(idx, additional);
            w.PendingQty[0] += additional;
            w.QtyLabel.text  = w.PendingQty[0].ToString();
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
                if (max > 0)
                {
                    CargoSystem.BuyCargo(i, max);
                    // Track per-row pending so UNDO/CLEAR can reverse it
                    _rows[i].PendingQty[0] += max;
                }
            }
            // Re-render readouts without resetting PendingQty (Refresh() does)
            var G = GameState.Instance;
            _creditsText.text = UIFactory.Cr(G.Credits);
            _baysText.text    = $"Bays: {CargoSystem.FreeCargoBays()}/{CargoSystem.TotalCargoBays()}";
            for (int i = 0; i < MaxTradeItem; i++)
            {
                _rows[i].QtyLabel.text = _rows[i].PendingQty[0].ToString();
                _rows[i].Held.text     = G.Ship.Cargo[i].ToString();
                _rows[i].Avail.text    = G.CurrentSystem.Qty[i].ToString();
            }
        }

        void OnClear()
        {
            var G = GameState.Instance;
            for (int i = 0; i < MaxTradeItem; i++)
            {
                int qty = _rows[i].PendingQty[0];
                if (qty > 0)
                {
                    G.Credits              += G.BuyPrice[i] * qty;
                    G.Ship.Cargo[i]        -= qty;
                    G.CurrentSystem.Qty[i] += qty;
                    _rows[i].PendingQty[0]  = 0;
                    RecomputeBuyingPrice(i);
                }
            }
            Refresh();
        }

        void RecomputeBuyingPrice(int idx)
        {
            var G = GameState.Instance;
            int remaining = G.Ship.Cargo[idx];
            int boughtThisSession = remaining - _origCargo[idx];

            if (remaining <= 0)
            {
                G.BuyingPrice[idx] = 0;
            }
            else if (boughtThisSession <= 0)
            {
                G.BuyingPrice[idx] = _origBuyingPrice[idx];
            }
            else
            {
                G.BuyingPrice[idx] =
                    (_origBuyingPrice[idx] * _origCargo[idx] +
                     G.BuyPrice[idx] * boughtThisSession) / remaining;
            }
        }
    }
}
