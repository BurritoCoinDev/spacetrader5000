// Space Trader 5000 – Android/Unity Port
// Target System screen: details for a selected warp destination.

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SpaceTrader;
using static SpaceTrader.GameConstants;

namespace SpaceTrader.UI.Screens
{
    public class TargetSystemUI : MonoBehaviour, IScreenUI
    {
        TextMeshProUGUI _valName, _valSize, _valTech, _valGov,
                        _valDist, _valPolice, _valPirates, _valCosts;
        Button _warpBtn;

        public void Initialize(GameObject panel)
        {
            UIFactory.Stretch(panel.GetComponent<RectTransform>());
            UIFactory.Header(panel.transform, "TARGET SYSTEM",
                () => UIManager.Instance.NavigateBack());

            // ── Info block ────────────────────────────────────────────────────
            var info = UIFactory.Panel(panel.transform, "InfoBlock", ColorTheme.PanelBg);
            UIFactory.SetAnchored(info.GetComponent<RectTransform>(),
                new Vector2(0, 0.48f), new Vector2(1, 0.88f),
                new Vector2(8, 4), new Vector2(-8, -4));

            float rowH = 0.80f / 8f;
            _valName    = AddRow(info.transform, "Name:",         "Name",    0, rowH);
            _valSize    = AddRow(info.transform, "Size:",         "Size",    1, rowH);
            _valTech    = AddRow(info.transform, "Tech level:",   "Tech",    2, rowH);
            _valGov     = AddRow(info.transform, "Government:",   "Gov",     3, rowH);
            _valDist    = AddRow(info.transform, "Distance:",     "Dist",    4, rowH);
            _valPolice  = AddRow(info.transform, "Police:",       "Police",  5, rowH);
            _valPirates = AddRow(info.transform, "Pirates:",      "Pirates", 6, rowH);
            _valCosts   = AddRow(info.transform, "Current costs:","Costs",   7, rowH);

            // ── Nav arrows: cycle through reachable systems ───────────────────
            var prevBtn = UIFactory.Btn(panel.transform, "Prev", "<",
                OnPrev, ColorTheme.ButtonNormal, ColorTheme.FontButton);
            UIFactory.SetAnchored(prevBtn.GetComponent<RectTransform>(),
                new Vector2(0.00f, 0.88f), new Vector2(0.08f, 0.935f),
                Vector2.zero, Vector2.zero);

            var nextBtn = UIFactory.Btn(panel.transform, "Next", ">",
                OnNext, ColorTheme.ButtonNormal, ColorTheme.FontButton);
            UIFactory.SetAnchored(nextBtn.GetComponent<RectTransform>(),
                new Vector2(0.92f, 0.88f), new Vector2(1.00f, 0.935f),
                Vector2.zero, Vector2.zero);

            // ── Action buttons ────────────────────────────────────────────────
            var avgBtn = UIFactory.Btn(panel.transform, "AvgPrice", "AVERAGE PRICE LIST",
                () => UIManager.Instance.Navigate(GameScreen.AveragePriceList),
                ColorTheme.ButtonNormal, ColorTheme.FontButton);
            UIFactory.SetAnchored(avgBtn.GetComponent<RectTransform>(),
                new Vector2(0.03f, 0.33f), new Vector2(0.60f, 0.45f), Vector2.zero, Vector2.zero);

            var srcBtn = UIFactory.Btn(panel.transform, "ShortChart", "SHORT RANGE CHART",
                () => UIManager.Instance.Navigate(GameScreen.ShortRangeChart),
                ColorTheme.ButtonNormal, ColorTheme.FontButton);
            UIFactory.SetAnchored(srcBtn.GetComponent<RectTransform>(),
                new Vector2(0.03f, 0.20f), new Vector2(0.60f, 0.32f), Vector2.zero, Vector2.zero);

            _warpBtn = UIFactory.Btn(panel.transform, "Warp", "WARP",
                OnWarp, ColorTheme.ButtonSuccess, ColorTheme.FontButton);
            UIFactory.SetAnchored(_warpBtn.GetComponent<RectTransform>(),
                new Vector2(0.64f, 0.20f), new Vector2(0.97f, 0.45f), Vector2.zero, Vector2.zero);
        }

        public void OnShow()
        {
            var G      = GameState.Instance;
            int cur    = G.Commander.CurSystem;
            int target = G.WarpSystem;

            if (target < 0 || target >= MaxSolarSystem) target = cur;

            var sys  = G.SolarSystem[target];
            var pol  = GameData.PoliticsTypes[sys.Politics];
            bool visited = sys.Visited || target == cur;
            long dist = GameMath.RealDistance(G.SolarSystem[cur], sys);
            bool inRange = dist <= G.Ship.Fuel || TravelerSystem.WormholeExists(cur, target);

            if (visited)
            {
                _valName.text    = GameData.SolarSystemNames[sys.NameIndex];
                _valSize.text    = GameData.SystemSize[sys.Size];
                _valTech.text    = GameData.TechLevelNames[sys.TechLevel];
                _valGov.text     = pol.Name;
                _valPolice.text  = GameData.ActivityDescriptions[pol.StrengthPolice];
                _valPirates.text = GameData.ActivityDescriptions[pol.StrengthPirates];
            }
            else
            {
                _valName.text    = GameData.SolarSystemNames[sys.NameIndex];
                _valSize.text    = "Unknown";
                _valTech.text    = "Unknown";
                _valGov.text     = "Unknown";
                _valPolice.text  = "Unknown";
                _valPirates.text = "Unknown";
            }

            _valDist.text = $"{dist} parsecs";

            long costs = MoneySystem.MercenaryMoney() + MoneySystem.InsuranceMoney()
                       + TravelerSystem.WormholeTax(cur, target);
            _valCosts.text = UIFactory.Cr(costs);

            bool isHere = target == cur;
            _warpBtn.interactable = !isHere && inRange;
        }

        TextMeshProUGUI AddRow(Transform parent, string label, string id, int idx, float rowH)
        {
            float yTop = 1f - idx * rowH;
            float yBot = yTop - rowH;

            var lbl = UIFactory.Label(parent, $"Lbl{id}", label,
                ColorTheme.FontSmall, ColorTheme.TextAccent, TextAlignmentOptions.Left);
            UIFactory.SetAnchored(lbl.rectTransform,
                new Vector2(0, yBot), new Vector2(0.42f, yTop),
                new Vector2(8, 2), new Vector2(-4, -2));

            var val = UIFactory.Label(parent, $"Val{id}", "",
                ColorTheme.FontSmall, ColorTheme.TextPrimary, TextAlignmentOptions.Left);
            UIFactory.SetAnchored(val.rectTransform,
                new Vector2(0.42f, yBot), new Vector2(1f, yTop),
                new Vector2(4, 2), new Vector2(-8, -2));

            return val;
        }

        void OnPrev() => CycleTarget(-1);
        void OnNext() => CycleTarget(+1);

        void CycleTarget(int dir)
        {
            var G   = GameState.Instance;
            int cur = G.Commander.CurSystem;
            int t   = G.WarpSystem;
            int fuel = G.Ship.Fuel;

            // Collect reachable (in-range or wormhole) system indices
            var reachable = new System.Collections.Generic.List<int>();
            for (int i = 0; i < MaxSolarSystem; i++)
            {
                if (i == cur) continue;
                long d = GameMath.RealDistance(G.SolarSystem[cur], G.SolarSystem[i]);
                if (d <= fuel || TravelerSystem.WormholeExists(cur, i))
                    reachable.Add(i);
            }

            if (reachable.Count == 0) return;

            int pos = reachable.IndexOf(t);
            if (pos < 0) pos = 0;
            else pos = (pos + dir + reachable.Count) % reachable.Count;

            G.WarpSystem = reachable[pos];
            OnShow();
        }

        void OnWarp()
        {
            UIManager.Instance.Navigate(GameScreen.Warp);
        }
    }
}
