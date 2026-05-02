// Space Trader 5000 – Android/Unity Port
// Sell Cargo screen: incremental per-item sell with -/+/ALL controls,
// SELL ALL and CLEAR (undo) actions, mirroring the Buy Cargo screen.

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static SpaceTrader.GameConstants;

namespace SpaceTrader.UI.Screens
{
    public class SellCargoUI : MonoBehaviour, IScreenUI
    {
        TextMeshProUGUI _creditsText, _baysText;
        Transform _listContent;
        readonly List<RowWidgets> _rows = new();

        // Snapshot of cargo / running-average BuyingPrice when the screen
        // opened, so we can recompute BuyingPrice when the player undoes
        // part of a pending sale via the - button or CLEAR.
        readonly int[]  _origCargo       = new int[MaxTradeItem];
        readonly long[] _origBuyingPrice = new long[MaxTradeItem];

        struct RowWidgets
        {
            public TextMeshProUGUI Name, Price, Held, Profit;
            public Button DecBtn, IncBtn, AllBtn;
            public TextMeshProUGUI QtyLabel;
            public int[] PendingQty;   // amount sold this session
        }

        // Item | Sell | Held | Profit | - | Qty | + | ALL
        static readonly float[] ColX = { 0f, 0.22f, 0.39f, 0.50f, 0.62f, 0.70f, 0.78f, 0.86f, 1.0f };

        public void Initialize(GameObject panel)
        {
            UIFactory.Stretch(panel.GetComponent<RectTransform>());
            UIFactory.Header(panel.transform, "SELL CARGO",
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

            var (scroll, content) = UIFactory.ScrollView(panel.transform, "SellList");
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

            var sellAllBtn = UIFactory.Btn(btnRow.transform, "SellAll", "SELL ALL",
                OnSellAll, ColorTheme.ButtonSuccess);
            UIFactory.SetAnchored(sellAllBtn.GetComponent<RectTransform>(),
                Vector2.zero, new Vector2(0.48f, 1), Vector2.zero, Vector2.zero);

            var clearBtn = UIFactory.Btn(btnRow.transform, "Clear", "CLEAR",
                OnClear, ColorTheme.ButtonDanger);
            UIFactory.SetAnchored(clearBtn.GetComponent<RectTransform>(),
                new Vector2(0.52f, 0), Vector2.one, Vector2.zero, Vector2.zero);

            BuildRows();
        }

        void BuildColumnHeaders(Transform parent)
        {
            // Header alignment must match column value alignment.
            (string text, int col, TextAlignmentOptions align, int rightPad)[] hdrs =
            {
                ("Item",   0, TextAlignmentOptions.Left,   -4),
                ("Sell",   1, TextAlignmentOptions.Right,  -12),
                ("Held",   2, TextAlignmentOptions.Center, -4),
                ("Profit", 3, TextAlignmentOptions.Right,  -12),
                ("Sold",   4, TextAlignmentOptions.Center, -4),  // spans cols 4-6
                ("All",    7, TextAlignmentOptions.Center, -4),
            };
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

                w.Held = UIFactory.Label(row.transform, "Held", "0",
                    ColorTheme.FontBody, ColorTheme.TextSecondary, TextAlignmentOptions.Center);
                UIFactory.SetAnchored(w.Held.rectTransform,
                    new Vector2(ColX[2], 0), new Vector2(ColX[3], 1),
                    new Vector2(4, 4), new Vector2(-4, -4));

                w.Profit = UIFactory.Label(row.transform, "Profit", "",
                    ColorTheme.FontBody, ColorTheme.TextPositive, TextAlignmentOptions.Right);
                UIFactory.SetAnchored(w.Profit.rectTransform,
                    new Vector2(ColX[3], 0), new Vector2(ColX[4], 1),
                    new Vector2(4, 4), new Vector2(-12, -4));

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

                w.AllBtn = UIFactory.Btn(row.transform, "All", "ALL",
                    () => SellAll(idx), ColorTheme.ButtonSuccess);
                UIFactory.SetAnchored(w.AllBtn.GetComponent<RectTransform>(),
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
            var G = GameState.Instance;
            _creditsText.text = UIFactory.Cr(G.Credits);
            _baysText.text    = $"Bays: {CargoSystem.FreeCargoBays()}/{CargoSystem.TotalCargoBays()}";

            for (int i = 0; i < MaxTradeItem; i++)
            {
                var w     = _rows[i];
                int held  = G.Ship.Cargo[i];
                long sp   = G.SellPrice[i];
                bool canSell = held > 0 && sp > 0;

                w.Price.text  = sp > 0 ? UIFactory.Cr(sp) : "---";
                w.Price.color = sp > 0 ? ColorTheme.TextPositive : ColorTheme.TextDisabled;
                w.Held.text   = held.ToString();

                if (held > 0 && sp > 0)
                {
                    long profitPer = sp - _origBuyingPrice[i];
                    w.Profit.text  = (profitPer >= 0 ? "+" : "") + UIFactory.Cr(profitPer);
                    w.Profit.color = profitPer >= 0 ? ColorTheme.TextPositive : ColorTheme.TextNegative;
                }
                else
                {
                    w.Profit.text = "";
                }

                w.PendingQty[0] = 0;
                w.QtyLabel.text = "0";
                w.DecBtn.interactable = canSell;
                w.IncBtn.interactable = canSell;
                w.AllBtn.interactable = canSell;
                w.Name.color = ColorTheme.TextPrimary;
            }
        }

        void AdjustQty(int idx, int delta)
        {
            var G   = GameState.Instance;
            var w   = _rows[idx];
            int cur = w.PendingQty[0];
            // Upper bound: original held quantity at screen open
            int max = _origCargo[idx];

            int newQty = Mathf.Clamp(cur + delta, 0, max);
            w.PendingQty[0] = newQty;
            w.QtyLabel.text = newQty.ToString();

            int diff = newQty - cur;
            if (diff > 0)
            {
                CargoSystem.SellCargo(idx, diff, GameConstants.SellCargo);
            }
            else if (diff < 0)
            {
                // Undo a pending sale: take back the credits, restore cargo and stock.
                int amount = -diff;
                G.Credits                -= G.SellPrice[idx] * amount;
                G.Ship.Cargo[idx]        += amount;
                G.CurrentSystem.Qty[idx] -= amount;
                RestoreBuyingPrice(idx);
            }

            UpdateRowReadout(idx);
        }

        void SellAll(int idx)
        {
            var G = GameState.Instance;
            var w = _rows[idx];
            int additional = G.Ship.Cargo[idx];
            if (additional <= 0) return;
            CargoSystem.SellCargo(idx, additional, GameConstants.SellCargo);
            w.PendingQty[0] += additional;
            w.QtyLabel.text  = w.PendingQty[0].ToString();
            UpdateRowReadout(idx);
        }

        void UpdateRowReadout(int idx)
        {
            var G = GameState.Instance;
            var w = _rows[idx];
            _creditsText.text = UIFactory.Cr(G.Credits);
            _baysText.text    = $"Bays: {CargoSystem.FreeCargoBays()}/{CargoSystem.TotalCargoBays()}";
            w.Held.text       = G.Ship.Cargo[idx].ToString();
        }

        void OnSellAll()
        {
            var G = GameState.Instance;
            for (int i = 0; i < MaxTradeItem; i++)
            {
                int held = G.Ship.Cargo[i];
                if (held > 0 && G.SellPrice[i] > 0)
                {
                    CargoSystem.SellCargo(i, held, GameConstants.SellCargo);
                    _rows[i].PendingQty[0] += held;
                }
            }
            // Re-render everything (prices/profit don't change but Held/Qty do)
            for (int i = 0; i < MaxTradeItem; i++)
            {
                _rows[i].QtyLabel.text = _rows[i].PendingQty[0].ToString();
                _rows[i].Held.text     = G.Ship.Cargo[i].ToString();
            }
            _creditsText.text = UIFactory.Cr(G.Credits);
            _baysText.text    = $"Bays: {CargoSystem.FreeCargoBays()}/{CargoSystem.TotalCargoBays()}";
        }

        void OnClear()
        {
            var G = GameState.Instance;
            for (int i = 0; i < MaxTradeItem; i++)
            {
                int qty = _rows[i].PendingQty[0];
                if (qty > 0)
                {
                    G.Credits                -= G.SellPrice[i] * qty;
                    G.Ship.Cargo[i]          += qty;
                    G.CurrentSystem.Qty[i]   -= qty;
                    _rows[i].PendingQty[0]    = 0;
                    RestoreBuyingPrice(i);
                }
            }
            Refresh();
        }

        // After undoing some or all of a pending sale, restore the
        // running-average BuyingPrice from the snapshot. SellCargo zeros
        // BuyingPrice when cargo hits 0; bringing cargo back must restore it.
        void RestoreBuyingPrice(int idx)
        {
            var G = GameState.Instance;
            if (G.Ship.Cargo[idx] > 0)
                G.BuyingPrice[idx] = _origBuyingPrice[idx];
            else
                G.BuyingPrice[idx] = 0;
        }
    }
}
