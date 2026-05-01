// Space Trader 5000 – Android/Unity Port
// Galactic Chart screen: 2D star map with touch-to-select warp target.

using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SpaceTrader;
using static SpaceTrader.GameConstants;

namespace SpaceTrader.UI.Screens
{
    public class GalacticChartUI : MonoBehaviour, IScreenUI
    {
        RectTransform _mapRect;
        TextMeshProUGUI _selectedText, _fuelText;
        Button _warpBtn;
        GameObject[] _dots;
        int _selectedSystem = -1;

        public void Initialize(GameObject panel)
        {
            UIFactory.Stretch(panel.GetComponent<RectTransform>());
            UIFactory.Header(panel.transform, "GALACTIC CHART",
                () => UIManager.Instance.NavigateBack());

            // Info strip
            var strip = UIFactory.Panel(panel.transform, "Strip", ColorTheme.RowBg);
            UIFactory.SetAnchored(strip.GetComponent<RectTransform>(),
                new Vector2(0, 0.88f), new Vector2(1, 0.935f), Vector2.zero, Vector2.zero);

            _fuelText = UIFactory.Label(strip.transform, "Fuel", "",
                ColorTheme.FontSmall, ColorTheme.TextSecondary, TextAlignmentOptions.Left);
            UIFactory.Stretch(_fuelText.rectTransform, 12, 12, 4, 4);

            // Selected system info
            _selectedText = UIFactory.Label(panel.transform, "SelInfo", "Tap a system to select",
                ColorTheme.FontSmall, ColorTheme.TextAccent, TextAlignmentOptions.Center);
            UIFactory.SetAnchored(_selectedText.rectTransform,
                new Vector2(0, 0.84f), new Vector2(1, 0.88f), Vector2.zero, Vector2.zero);

            // Map area
            var mapPanel = UIFactory.Panel(panel.transform, "Map", ColorTheme.Background);
            UIFactory.SetAnchored(mapPanel.GetComponent<RectTransform>(),
                new Vector2(0, 0.10f), new Vector2(1, 0.84f), Vector2.zero, Vector2.zero);
            _mapRect = mapPanel.GetComponent<RectTransform>();

            // Pre-create dots
            _dots = new GameObject[MaxSolarSystem];
            for (int i = 0; i < MaxSolarSystem; i++)
            {
                int idx = i;
                var dot = UIFactory.Panel(mapPanel.transform, $"Dot{i}", ColorTheme.TextSecondary);
                var rt  = dot.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(14, 14);
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                var dotBtn = dot.AddComponent<Button>();
                dotBtn.onClick.AddListener(() => SelectSystem(idx));
                _dots[i] = dot;
            }

            // WARP button
            _warpBtn = UIFactory.Btn(panel.transform, "WarpBtn", "SELECT FOR WARP",
                OnWarp, ColorTheme.ButtonSuccess);
            UIFactory.SetAnchored(_warpBtn.GetComponent<RectTransform>(),
                new Vector2(0.1f, 0.02f), new Vector2(0.9f, 0.09f), Vector2.zero, Vector2.zero);
        }

        public void OnShow()
        {
            var G = GameState.Instance;
            int fuel = G.Ship.Fuel;
            int tanks = FuelSystem.GetFuelTanks();
            _fuelText.text = $"Fuel: {fuel}  Range: {tanks}";
            _selectedSystem = G.WarpSystem >= 0 ? G.WarpSystem : G.Commander.CurSystem;
            PlaceDots();
            UpdateSelection();
        }

        void PlaceDots()
        {
            var G   = GameState.Instance;
            int cur = G.Commander.CurSystem;
            int fuel = G.Ship.Fuel;

            for (int i = 0; i < MaxSolarSystem; i++)
            {
                var sys = G.SolarSystem[i];
                float nx = (float)sys.X / GalaxyWidth;
                float ny = (float)sys.Y / GalaxyHeight;

                var rt = _dots[i].GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(nx, ny);
                rt.anchorMax = new Vector2(nx, ny);

                bool isCurrent = i == cur;
                bool visited   = sys.Visited || isCurrent;
                long dist      = GameMath.RealDistance(G.SolarSystem[cur], sys);
                bool inRange   = dist <= fuel && !isCurrent;

                var img = _dots[i].GetComponent<Image>();
                if (isCurrent)
                    img.color = ColorTheme.TextAccent;        // gold — current system
                else if (inRange && visited)
                    img.color = ColorTheme.TextPositive;      // green — reachable & known
                else if (inRange)
                    img.color = new Color(0.20f, 0.60f, 0.30f, 1f); // dim green — reachable, unvisited
                else if (visited)
                    img.color = ColorTheme.TextSecondary;     // grey — visited but out of range
                else
                    img.color = ColorTheme.TextDisabled;      // dark — unknown & out of range

                rt.sizeDelta = isCurrent ? new Vector2(18, 18) : new Vector2(12, 12);
            }
        }

        void SelectSystem(int idx)
        {
            _selectedSystem = idx;
            UpdateSelection();
        }

        void UpdateSelection()
        {
            if (_selectedSystem < 0 || _selectedSystem >= MaxSolarSystem) return;
            var G    = GameState.Instance;
            int cur  = G.Commander.CurSystem;
            int fuel = G.Ship.Fuel;
            var sys  = G.SolarSystem[_selectedSystem];
            bool visited = sys.Visited || _selectedSystem == cur;

            string name = GameData.SolarSystemNames[sys.NameIndex];
            long dist   = GameMath.RealDistance(G.SolarSystem[cur], sys);
            bool inRange = dist <= fuel || TravelerSystem.WormholeExists(cur, _selectedSystem);
            bool isHere  = _selectedSystem == cur;

            string info = visited
                ? $"{name}  [{GameData.TechLevelNames[sys.TechLevel]}]  Dist: {dist}"
                : $"Unknown system  Dist: {dist}";
            _selectedText.text  = isHere ? $"{name} (current)" : info;
            _selectedText.color = isHere ? ColorTheme.TextAccent
                                : inRange ? ColorTheme.TextPositive
                                : ColorTheme.TextNegative;

            _warpBtn.interactable = !isHere && inRange;
            if (_warpBtn.interactable)
                GameState.Instance.WarpSystem = _selectedSystem;

            // Refresh dot highlights
            PlaceDots();

            // Override selected dot colour
            if (!isHere)
            {
                var img = _dots[_selectedSystem].GetComponent<Image>();
                img.color = inRange ? Color.white : ColorTheme.TextNegative;
                _dots[_selectedSystem].GetComponent<RectTransform>().sizeDelta = new Vector2(18, 18);
            }
        }

        void OnWarp()
        {
            UIManager.Instance.Navigate(GameScreen.Warp);
        }
    }
}
