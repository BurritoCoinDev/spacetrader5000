// Space Trader 5000 – Android/Unity Port
// Options screen: gameplay toggles and settings, matching the original Palm OS options.

using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceTrader.UI.Screens
{
    public class OptionsUI : MonoBehaviour, IScreenUI
    {
        // One row per option — either a bool toggle or the LeaveEmpty numeric field.
        abstract class OptionRow
        {
            public abstract void Refresh(GameState G);
        }

        class ToggleRow : OptionRow
        {
            public Button Btn;
            public TextMeshProUGUI BtnLabel;
            public Func<GameState, bool> Get;
            public Action<GameState, bool> Set;

            public override void Refresh(GameState G)
            {
                bool on = Get(G);
                Btn.GetComponent<Image>().color = on ? ColorTheme.ButtonSuccess : ColorTheme.ButtonDanger;
                BtnLabel.text = on ? "ON" : "OFF";
            }
        }

        class NumericRow : OptionRow
        {
            public TextMeshProUGUI ValueLabel;
            public Func<GameState, int> Get;
            public Action<GameState, int> Set;
            public int Min, Max;

            public override void Refresh(GameState G)
            {
                ValueLabel.text = Get(G).ToString();
            }
        }

        readonly List<OptionRow> _rows = new();

        public void Initialize(GameObject panel)
        {
            UIFactory.Stretch(panel.GetComponent<RectTransform>());
            UIFactory.Header(panel.transform, "OPTIONS",
                () => UIManager.Instance.NavigateBack());

            var (scroll, content) = UIFactory.ScrollView(panel.transform, "OptionsList");
            UIFactory.SetAnchored(scroll.GetComponent<RectTransform>(),
                new Vector2(0, 0.02f), new Vector2(1, 0.88f), Vector2.zero, Vector2.zero);

            BuildToggle(content, "Auto-fuel when docked",
                G => G.AutoFuel,         (G, v) => G.AutoFuel = v);
            BuildToggle(content, "Auto-repair when docked",
                G => G.AutoRepair,       (G, v) => G.AutoRepair = v);
            BuildToggle(content, "Always ignore traders",
                G => G.AlwaysIgnoreTraders,  (G, v) => G.AlwaysIgnoreTraders = v);
            BuildToggle(content, "Always ignore police",
                G => G.AlwaysIgnorePolice,   (G, v) => G.AlwaysIgnorePolice = v);
            BuildToggle(content, "Always ignore pirates",
                G => G.AlwaysIgnorePirates,  (G, v) => G.AlwaysIgnorePirates = v);
            BuildToggle(content, "Ignore in-orbit traders",
                G => G.AlwaysIgnoreTradeInOrbit, (G, v) => G.AlwaysIgnoreTradeInOrbit = v);
            BuildToggle(content, "Reserve money for auto-buy",
                G => G.ReserveMoney,     (G, v) => G.ReserveMoney = v);
            BuildToggle(content, "Auto-pay for news",
                G => G.NewsAutoPay,      (G, v) => G.NewsAutoPay = v);
            BuildToggle(content, "Auto-clear tracking after warp",
                G => G.TrackAutoOff,     (G, v) => G.TrackAutoOff = v);
            BuildNumeric(content, "Cargo bays to leave empty",
                G => G.LeaveEmpty, (G, v) => G.LeaveEmpty = v, 0, 50);
        }

        void BuildToggle(Transform content, string label,
            Func<GameState, bool> get, Action<GameState, bool> set)
        {
            int rowIdx = _rows.Count;
            var row = UIFactory.RowPanel(content, $"Opt{rowIdx}",
                rowIdx % 2 == 0 ? ColorTheme.RowBg : ColorTheme.RowAlt, 80);

            var nameLbl = UIFactory.Label(row.transform, "Label", label,
                ColorTheme.FontSmall, ColorTheme.TextPrimary, TextAlignmentOptions.Left);
            UIFactory.SetAnchored(nameLbl.rectTransform,
                new Vector2(0, 0), new Vector2(0.72f, 1), new Vector2(12, 6), new Vector2(-4, -6));
            nameLbl.textWrappingMode = TMPro.TextWrappingModes.Normal;
            nameLbl.overflowMode     = TMPro.TextOverflowModes.Overflow;

            var tr = new ToggleRow { Get = get, Set = set };
            tr.Btn = UIFactory.Btn(row.transform, "Toggle", "ON", null, ColorTheme.ButtonSuccess,
                ColorTheme.FontButtonSm);
            UIFactory.SetAnchored(tr.Btn.GetComponent<RectTransform>(),
                new Vector2(0.74f, 0.12f), new Vector2(1f, 0.88f),
                new Vector2(0, 0), new Vector2(-12, 0));
            tr.BtnLabel = tr.Btn.GetComponentInChildren<TextMeshProUGUI>();

            tr.Btn.onClick.AddListener(() =>
            {
                var G = GameState.Instance;
                tr.Set(G, !tr.Get(G));
                tr.Refresh(G);
            });

            _rows.Add(tr);
        }

        void BuildNumeric(Transform content, string label,
            Func<GameState, int> get, Action<GameState, int> set, int min, int max)
        {
            int rowIdx = _rows.Count;
            var row = UIFactory.RowPanel(content, $"Opt{rowIdx}",
                rowIdx % 2 == 0 ? ColorTheme.RowBg : ColorTheme.RowAlt, 80);

            var nameLbl = UIFactory.Label(row.transform, "Label", label,
                ColorTheme.FontSmall, ColorTheme.TextPrimary, TextAlignmentOptions.Left);
            UIFactory.SetAnchored(nameLbl.rectTransform,
                new Vector2(0, 0), new Vector2(0.58f, 1), new Vector2(12, 6), new Vector2(-4, -6));
            nameLbl.textWrappingMode = TMPro.TextWrappingModes.Normal;
            nameLbl.overflowMode     = TMPro.TextOverflowModes.Overflow;

            var nr = new NumericRow { Get = get, Set = set, Min = min, Max = max };

            var decBtn = UIFactory.SmallBtn(row.transform, "Dec", "-", null);
            UIFactory.SetAnchored(decBtn.GetComponent<RectTransform>(),
                new Vector2(0.60f, 0.12f), new Vector2(0.73f, 0.88f), Vector2.zero, Vector2.zero);

            nr.ValueLabel = UIFactory.Label(row.transform, "Value", "0",
                ColorTheme.FontBody, ColorTheme.TextAccent, TextAlignmentOptions.Center);
            UIFactory.SetAnchored(nr.ValueLabel.rectTransform,
                new Vector2(0.73f, 0), new Vector2(0.87f, 1), new Vector2(2, 4), new Vector2(-2, -4));

            var incBtn = UIFactory.SmallBtn(row.transform, "Inc", "+", null);
            UIFactory.SetAnchored(incBtn.GetComponent<RectTransform>(),
                new Vector2(0.87f, 0.12f), new Vector2(1f, 0.88f),
                new Vector2(0, 0), new Vector2(-12, 0));

            decBtn.onClick.AddListener(() =>
            {
                var G = GameState.Instance;
                int v = Mathf.Max(nr.Min, nr.Get(G) - 1);
                nr.Set(G, v);
                nr.Refresh(G);
            });
            incBtn.onClick.AddListener(() =>
            {
                var G = GameState.Instance;
                int v = Mathf.Min(nr.Max, nr.Get(G) + 1);
                nr.Set(G, v);
                nr.Refresh(G);
            });

            _rows.Add(nr);
        }

        public void OnShow()
        {
            var G = GameState.Instance;
            foreach (var row in _rows)
                row.Refresh(G);
        }
    }
}
