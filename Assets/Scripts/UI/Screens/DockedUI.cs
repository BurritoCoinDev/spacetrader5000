// Space Trader 5000 – Android/Unity Port
// Main docked hub: system overview, status bars, and navigation grid.

using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SpaceTrader.Persistence;
using static SpaceTrader.GameConstants;

namespace SpaceTrader.UI.Screens
{
    public class DockedUI : MonoBehaviour, IScreenUI
    {
        TextMeshProUGUI _systemName, _govText, _techText, _statusText;
        TextMeshProUGUI _creditsText, _daysText, _debtText;
        Slider _fuelBar, _hullBar;
        TextMeshProUGUI _fuelLabel, _hullLabel;
        TextMeshProUGUI _specialText;

        public void Initialize(GameObject panel)
        {
            UIFactory.Stretch(panel.GetComponent<RectTransform>());

            // ── Header area (top 12%) ─────────────────────────────────────────
            var header = UIFactory.Panel(panel.transform, "Header", ColorTheme.HeaderBg);
            UIFactory.SetAnchored(header.GetComponent<RectTransform>(),
                new Vector2(0, 0.88f), Vector2.one, Vector2.zero, Vector2.zero);

            _systemName = UIFactory.Label(header.transform, "SysName", "Sol",
                ColorTheme.FontHeader, ColorTheme.TextAccent, TextAlignmentOptions.Center);
            UIFactory.SetAnchored(_systemName.rectTransform,
                new Vector2(0, 0.5f), Vector2.one, new Vector2(8, 0), new Vector2(-8, 0));

            _govText = UIFactory.Label(header.transform, "GovText", "",
                ColorTheme.FontSmall, ColorTheme.TextSecondary, TextAlignmentOptions.Left);
            UIFactory.SetAnchored(_govText.rectTransform,
                Vector2.zero, new Vector2(0.5f, 0.5f),
                new Vector2(8, 4), new Vector2(-4, 0));

            _techText = UIFactory.Label(header.transform, "TechText", "",
                ColorTheme.FontSmall, ColorTheme.TextSecondary, TextAlignmentOptions.Right);
            UIFactory.SetAnchored(_techText.rectTransform,
                new Vector2(0.5f, 0), new Vector2(1, 0.5f),
                new Vector2(4, 4), new Vector2(-8, 0));

            // ── Credits/Days strip (88–82%) ────────────────────────────────────
            var strip = UIFactory.Panel(panel.transform, "Strip", ColorTheme.RowBg);
            UIFactory.SetAnchored(strip.GetComponent<RectTransform>(),
                new Vector2(0, 0.82f), new Vector2(1, 0.88f), Vector2.zero, Vector2.zero);

            _creditsText = UIFactory.Label(strip.transform, "Credits", "",
                ColorTheme.FontBody, ColorTheme.TextPositive, TextAlignmentOptions.Left);
            UIFactory.SetAnchored(_creditsText.rectTransform,
                Vector2.zero, new Vector2(0.5f, 1), new Vector2(12, 4), new Vector2(-4, -4));

            _daysText = UIFactory.Label(strip.transform, "Days", "",
                ColorTheme.FontBody, ColorTheme.TextSecondary, TextAlignmentOptions.Right);
            UIFactory.SetAnchored(_daysText.rectTransform,
                new Vector2(0.5f, 0), Vector2.one, new Vector2(4, 4), new Vector2(-12, -4));

            _debtText = UIFactory.Label(panel.transform, "Debt", "",
                ColorTheme.FontSmall, ColorTheme.TextNegative, TextAlignmentOptions.Center);
            UIFactory.SetAnchored(_debtText.rectTransform,
                new Vector2(0.05f, 0.79f), new Vector2(0.95f, 0.82f), Vector2.zero, Vector2.zero);

            // ── Status bar (78–76%) ───────────────────────────────────────────
            _statusText = UIFactory.LabelWrap(panel.transform, "Status", "",
                ColorTheme.FontSmall, ColorTheme.TextWarning, TextAlignmentOptions.Center);
            UIFactory.SetAnchored(_statusText.rectTransform,
                new Vector2(0.05f, 0.75f), new Vector2(0.95f, 0.79f), Vector2.zero, Vector2.zero);

            // ── Special event notice (74–72%) ─────────────────────────────────
            _specialText = UIFactory.LabelWrap(panel.transform, "Special", "",
                ColorTheme.FontSmall, ColorTheme.TextAccent, TextAlignmentOptions.Center);
            UIFactory.SetAnchored(_specialText.rectTransform,
                new Vector2(0.05f, 0.71f), new Vector2(0.95f, 0.75f), Vector2.zero, Vector2.zero);

            // ── Navigation grid (70–20%) ──────────────────────────────────────
            BuildNavGrid(panel);

            // ── Fuel / hull bars (18–4%) ──────────────────────────────────────
            BuildStatusBars(panel);
        }

        void BuildNavGrid(GameObject panel)
        {
            var grid = UIFactory.TransparentPanel(panel.transform, "NavGrid");
            UIFactory.SetAnchored(grid.GetComponent<RectTransform>(),
                new Vector2(0.02f, 0.20f), new Vector2(0.98f, 0.70f), Vector2.zero, Vector2.zero);

            var glg = grid.AddComponent<GridLayoutGroup>();
            glg.cellSize    = new Vector2(330, 130);
            glg.spacing     = new Vector2(12, 12);
            glg.padding     = new RectOffset(6, 6, 6, 6);
            glg.constraint  = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = 3;
            glg.childAlignment  = TextAnchor.UpperCenter;

            (string label, GameScreen screen)[] buttons =
            {
                ("BUY CARGO",    GameScreen.BuyCargo),
                ("SELL CARGO",   GameScreen.SellCargo),
                ("SHIPYARD",     GameScreen.Shipyard),
                ("BUY EQUIP",    GameScreen.BuyEquipment),
                ("SELL EQUIP",   GameScreen.SellEquipment),
                ("PERSONNEL",    GameScreen.PersonnelRoster),
                ("BANK",         GameScreen.Bank),
                ("SYS INFO",     GameScreen.SystemInfo),
                ("CMDR STATUS",  GameScreen.CommanderStatus),
                ("GAL CHART",    GameScreen.GalacticChart),
                ("SHORT RANGE",  GameScreen.ShortRangeChart),
                ("WARP",         GameScreen.Warp),
                ("HIGH SCORES",  GameScreen.HighScores),
                ("OPTIONS",      GameScreen.Options),
            };

            foreach (var (label, screen) in buttons)
            {
                var s = screen; // capture
                var btn = UIFactory.Btn(grid.transform, label, label,
                    () => UIManager.Instance.Navigate(s));
                btn.GetComponent<RectTransform>().sizeDelta = glg.cellSize;
            }
        }

        void BuildStatusBars(GameObject panel)
        {
            // Fuel
            var fuelRow = UIFactory.TransparentPanel(panel.transform, "FuelRow");
            UIFactory.SetAnchored(fuelRow.GetComponent<RectTransform>(),
                new Vector2(0.02f, 0.11f), new Vector2(0.98f, 0.19f), Vector2.zero, Vector2.zero);

            _fuelLabel = UIFactory.Label(fuelRow.transform, "FuelLbl", "FUEL",
                ColorTheme.FontSmall, ColorTheme.FuelFill);
            UIFactory.Pin(_fuelLabel.rectTransform, TextAnchor.MiddleLeft, 100, 40);

            _fuelBar = UIFactory.ProgressBar(fuelRow.transform, "FuelBar", ColorTheme.FuelFill);
            var fbrt = _fuelBar.GetComponent<RectTransform>();
            fbrt.anchorMin = new Vector2(0, 0); fbrt.anchorMax = new Vector2(1, 1);
            fbrt.offsetMin = new Vector2(110, 6); fbrt.offsetMax = new Vector2(-8, -6);

            // Hull
            var hullRow = UIFactory.TransparentPanel(panel.transform, "HullRow");
            UIFactory.SetAnchored(hullRow.GetComponent<RectTransform>(),
                new Vector2(0.02f, 0.04f), new Vector2(0.98f, 0.11f), Vector2.zero, Vector2.zero);

            _hullLabel = UIFactory.Label(hullRow.transform, "HullLbl", "HULL",
                ColorTheme.FontSmall, ColorTheme.HullFill);
            UIFactory.Pin(_hullLabel.rectTransform, TextAnchor.MiddleLeft, 100, 40);

            _hullBar = UIFactory.ProgressBar(hullRow.transform, "HullBar", ColorTheme.HullFill);
            var hbrt = _hullBar.GetComponent<RectTransform>();
            hbrt.anchorMin = new Vector2(0, 0); hbrt.anchorMax = new Vector2(1, 1);
            hbrt.offsetMin = new Vector2(110, 6); hbrt.offsetMax = new Vector2(-8, -6);
        }

        public void OnShow()
        {
            var G   = GameState.Instance;
            var sys = G.CurrentSystem;
            var pol = GameData.PoliticsTypes[sys.Politics];

            _systemName.text = GameData.SolarSystemNames[sys.NameIndex];
            _govText.text    = pol.Name;
            _techText.text   = GameData.TechLevelNames[sys.TechLevel];
            _statusText.text = sys.Status > Uneventful
                ? $"System is {GameData.StatusDescriptions[sys.Status]}"
                : "";

            _creditsText.text = UIFactory.Cr(G.Credits);
            _daysText.text    = $"Day {G.Days}";
            _debtText.text    = G.Debt > 0 ? $"Debt: {UIFactory.Cr(G.Debt)}" : "";

            // Special event notice
            _specialText.text = sys.Special >= 0
                ? $"Special event available!"
                : "";

            // Fuel bar
            int maxFuel = FuelSystem.GetFuelTanks();
            _fuelBar.value    = maxFuel > 0 ? (float)G.Ship.Fuel / maxFuel : 0;
            _fuelLabel.text   = $"FUEL {G.Ship.Fuel}/{maxFuel}";

            // Hull bar
            long maxHull = ShipyardSystem.GetHullStrength();
            float hullPct = maxHull > 0 ? (float)G.Ship.Hull / maxHull : 0;
            _hullBar.value    = hullPct;
            _hullBar.fillRect.GetComponent<Image>().color =
                hullPct < 0.25f ? ColorTheme.HullLow : ColorTheme.HullFill;
            _hullLabel.text   = $"HULL {G.Ship.Hull}/{maxHull}";

            SaveSystem.Save();
        }
    }
}
