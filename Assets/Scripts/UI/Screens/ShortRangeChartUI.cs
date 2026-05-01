// Space Trader 5000 – Android/Unity Port
// Short Range Chart: zoomed star map centered on current system.

using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SpaceTrader;
using static SpaceTrader.GameConstants;

namespace SpaceTrader.UI.Screens
{
    public class ShortRangeChartUI : MonoBehaviour, IScreenUI
    {
        const float ViewRadius  = 50f;  // parsecs shown each side
        const int   CirclePts   = 36;   // dots used to draw fuel-range circle
        const float DotSize     = 14f;
        const float CurDotSize  = 18f;
        const float HitSize     = 60f;  // transparent touch target around each dot

        RectTransform _mapRect;
        TextMeshProUGUI _fuelText;

        // One entry per solar system
        RectTransform[]     _dotRt;
        Image[]             _dotImg;
        TextMeshProUGUI[]   _dotLabel;

        // Range circle
        RectTransform[] _circleRt;

        public void Initialize(GameObject panel)
        {
            UIFactory.Stretch(panel.GetComponent<RectTransform>());
            UIFactory.Header(panel.transform, "SHORT RANGE CHART",
                () => UIManager.Instance.NavigateBack());

            // Fuel strip below header
            var strip = UIFactory.Panel(panel.transform, "Strip", ColorTheme.RowBg);
            UIFactory.SetAnchored(strip.GetComponent<RectTransform>(),
                new Vector2(0, 0.88f), new Vector2(1, 0.935f), Vector2.zero, Vector2.zero);
            _fuelText = UIFactory.Label(strip.transform, "Fuel", "",
                ColorTheme.FontSmall, ColorTheme.TextSecondary, TextAlignmentOptions.Left);
            UIFactory.Stretch(_fuelText.rectTransform, 12, 12, 4, 4);

            // Map area
            var mapGo = UIFactory.Panel(panel.transform, "Map", new Color(0.04f, 0.04f, 0.10f));
            UIFactory.SetAnchored(mapGo.GetComponent<RectTransform>(),
                new Vector2(0, 0.02f), new Vector2(1, 0.88f), Vector2.zero, Vector2.zero);
            _mapRect = mapGo.GetComponent<RectTransform>();

            // Pre-create system dots + labels
            _dotRt    = new RectTransform[MaxSolarSystem];
            _dotImg   = new Image[MaxSolarSystem];
            _dotLabel = new TextMeshProUGUI[MaxSolarSystem];

            for (int i = 0; i < MaxSolarSystem; i++)
            {
                int idx = i;

                // Transparent hit area — large enough for a finger tap
                var hit    = UIFactory.Panel(mapGo.transform, $"Hit{i}", Color.clear);
                var rt     = hit.GetComponent<RectTransform>();
                rt.sizeDelta  = new Vector2(HitSize, HitSize);
                rt.anchorMin  = rt.anchorMax = new Vector2(0.5f, 0.5f);
                _dotRt[i]     = rt;

                var dotBtn = hit.AddComponent<Button>();
                dotBtn.onClick.AddListener(() => SelectSystem(idx));

                // Visual dot centered inside the hit area
                var dot    = UIFactory.Panel(hit.transform, $"Dot{i}", Color.grey);
                var drt    = dot.GetComponent<RectTransform>();
                drt.sizeDelta    = new Vector2(DotSize, DotSize);
                drt.anchorMin    = drt.anchorMax = new Vector2(0.5f, 0.5f);
                drt.anchoredPosition = Vector2.zero;
                _dotImg[i] = dot.GetComponent<Image>();

                // Name label (small, to the right of the dot)
                var lbl = UIFactory.Label(mapGo.transform, $"Lbl{i}", "",
                    16, ColorTheme.TextSecondary,
                    TextAlignmentOptions.Left);
                var lrt          = lbl.rectTransform;
                lrt.sizeDelta    = new Vector2(120, 20);
                lrt.anchorMin    = lrt.anchorMax = new Vector2(0.5f, 0.5f);
                _dotLabel[i]     = lbl;
            }

            // Range circle dots (drawn over everything, so add last)
            _circleRt = new RectTransform[CirclePts];
            for (int k = 0; k < CirclePts; k++)
            {
                var cd  = UIFactory.Panel(mapGo.transform, $"Circle{k}", ColorTheme.TextAccent);
                var crt = cd.GetComponent<RectTransform>();
                crt.sizeDelta  = new Vector2(4, 4);
                crt.anchorMin  = crt.anchorMax = new Vector2(0.5f, 0.5f);
                _circleRt[k]   = crt;
            }
        }

        public void OnShow()
        {
            var G    = GameState.Instance;
            int fuel = G.Ship.Fuel;
            int tanks = FuelSystem.GetFuelTanks();
            _fuelText.text = $"Fuel: {fuel}  Range: {tanks}";

            int cur = G.Commander.CurSystem;
            var curSys = G.SolarSystem[cur];

            // Place system dots
            for (int i = 0; i < MaxSolarSystem; i++)
            {
                var sys = G.SolarSystem[i];
                float dx = sys.X - curSys.X;
                float dy = sys.Y - curSys.Y;

                // Normalized position: 0.5 = center of map
                float nx = 0.5f + dx / (2f * ViewRadius);
                float ny = 0.5f + dy / (2f * ViewRadius);

                bool inView = nx >= -0.05f && nx <= 1.05f && ny >= -0.05f && ny <= 1.05f;
                _dotRt[i].gameObject.SetActive(inView);
                _dotLabel[i].gameObject.SetActive(inView);

                if (!inView) continue;

                _dotRt[i].anchorMin = _dotRt[i].anchorMax = new Vector2(nx, ny);
                _dotRt[i].anchoredPosition = Vector2.zero;

                // Label offset slightly right of dot
                _dotLabel[i].rectTransform.anchorMin =
                _dotLabel[i].rectTransform.anchorMax = new Vector2(nx, ny);
                _dotLabel[i].rectTransform.anchoredPosition = new Vector2(10, 4);

                bool isCurrent = i == cur;
                bool visited   = sys.Visited || isCurrent;
                long dist      = GameMath.RealDistance(curSys, sys);
                bool inRange   = dist <= fuel && !isCurrent;
                bool wormhole  = !isCurrent && TravelerSystem.WormholeExists(cur, i);

                // Visual dot size (hit area stays at HitSize)
                var visRt = _dotRt[i].GetChild(0).GetComponent<RectTransform>();
                visRt.sizeDelta = isCurrent ? new Vector2(CurDotSize, CurDotSize)
                                            : new Vector2(DotSize, DotSize);

                if (isCurrent)
                    _dotImg[i].color = ColorTheme.TextAccent;
                else if (inRange || wormhole)
                    _dotImg[i].color = ColorTheme.TextPositive;
                else if (visited)
                    _dotImg[i].color = ColorTheme.TextSecondary;
                else
                    _dotImg[i].color = ColorTheme.TextDisabled;

                string name = GameData.SolarSystemNames[sys.NameIndex];
                _dotLabel[i].text  = (visited || isCurrent) ? name : "";
                _dotLabel[i].color = isCurrent ? ColorTheme.TextAccent
                                   : inRange || wormhole ? ColorTheme.TextPositive
                                   : ColorTheme.TextSecondary;
            }

            // Draw range circle at fuel parsecs radius
            PlaceRangeCircle(fuel);
        }

        void PlaceRangeCircle(int fuel)
        {
            float r = (float)fuel / (2f * ViewRadius); // normalised radius
            for (int k = 0; k < CirclePts; k++)
            {
                float angle = 2f * Mathf.PI * k / CirclePts;
                float nx = 0.5f + r * Mathf.Cos(angle);
                float ny = 0.5f + r * Mathf.Sin(angle);
                _circleRt[k].anchorMin = _circleRt[k].anchorMax = new Vector2(nx, ny);
                _circleRt[k].anchoredPosition = Vector2.zero;
            }
        }

        void SelectSystem(int idx)
        {
            GameState.Instance.WarpSystem = idx;
            UIManager.Instance.Navigate(GameScreen.TargetSystem);
        }
    }
}
