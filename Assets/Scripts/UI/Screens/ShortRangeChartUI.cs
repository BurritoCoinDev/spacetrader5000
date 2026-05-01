// Space Trader 5000 – Android/Unity Port
// Short Range Chart: zoomed star map with pinch-to-zoom and pan.

using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SpaceTrader;
using static SpaceTrader.GameConstants;

namespace SpaceTrader.UI.Screens
{
    public class ShortRangeChartUI : MonoBehaviour, IScreenUI
    {
        const float DefaultZoom = 50f;   // parsecs from center to edge on reset
        const float MinZoom     = 8f;
        const float MaxZoom     = 150f;
        const int   CirclePts   = 48;    // dots used to draw fuel-range circle
        const float DotSize     = 14f;
        const float CurDotSize  = 18f;
        const float HitSize     = 60f;   // transparent touch target per system

        // ── UI refs ───────────────────────────────────────────────────────────
        RectTransform   _mapRect;
        TextMeshProUGUI _fuelText;

        RectTransform[]   _hitRt;       // outer hit area (positioned by pan/zoom)
        Image[]           _dotImg;      // inner visual dot
        TextMeshProUGUI[] _dotLabel;
        RectTransform[]   _circleRt;

        // ── View state ────────────────────────────────────────────────────────
        float   _zoom  = DefaultZoom;   // parsecs visible from center to edge
        Vector2 _pan   = Vector2.zero;  // offset in parsecs from cur system center
        float   _scale;                 // pixels per parsec (equal in x & y)
        int     _curSystem;
        int     _fuel;

        // ── Touch tracking ────────────────────────────────────────────────────
        float _prevPinchDist;
        bool  _isPinching;

        // ── Lifecycle ─────────────────────────────────────────────────────────

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

            // Map area — RectMask2D clips dots that pan outside the viewport
            var mapGo = UIFactory.Panel(panel.transform, "Map", new Color(0.04f, 0.04f, 0.10f));
            UIFactory.SetAnchored(mapGo.GetComponent<RectTransform>(),
                new Vector2(0, 0.02f), new Vector2(1, 0.88f), Vector2.zero, Vector2.zero);
            mapGo.AddComponent<RectMask2D>();
            _mapRect = mapGo.GetComponent<RectTransform>();

            // Pre-create system entries
            _hitRt    = new RectTransform[MaxSolarSystem];
            _dotImg   = new Image[MaxSolarSystem];
            _dotLabel = new TextMeshProUGUI[MaxSolarSystem];

            for (int i = 0; i < MaxSolarSystem; i++)
            {
                int idx = i;

                // Transparent hit area carries the Button and acts as the
                // moveable anchor point; visual dot + label are children so
                // they translate automatically when the hit area is repositioned.
                var hit = UIFactory.Panel(mapGo.transform, $"Hit{i}", Color.clear);
                var hrt = hit.GetComponent<RectTransform>();
                hrt.sizeDelta        = new Vector2(HitSize, HitSize);
                hrt.anchorMin        = hrt.anchorMax = new Vector2(0.5f, 0.5f);
                hrt.anchoredPosition = Vector2.zero;
                _hitRt[i] = hrt;

                var btn = hit.AddComponent<Button>();
                btn.onClick.AddListener(() => SelectSystem(idx));

                // Visual dot — centered inside hit area
                var dot = UIFactory.Panel(hit.transform, "Dot", Color.grey);
                var drt = dot.GetComponent<RectTransform>();
                drt.sizeDelta        = new Vector2(DotSize, DotSize);
                drt.anchorMin        = drt.anchorMax = new Vector2(0.5f, 0.5f);
                drt.anchoredPosition = Vector2.zero;
                _dotImg[i] = dot.GetComponent<Image>();

                // Name label — offset right of dot, inside same hit-area transform
                var lbl = UIFactory.Label(hit.transform, "Lbl", "",
                    16, ColorTheme.TextSecondary, TextAlignmentOptions.Left);
                var lrt = lbl.rectTransform;
                lrt.sizeDelta        = new Vector2(130, 22);
                lrt.anchorMin        = lrt.anchorMax = new Vector2(0.5f, 0.5f);
                lrt.anchoredPosition = new Vector2(HitSize * 0.5f + 4, 4);
                _dotLabel[i] = lbl;
            }

            // Range circle dots (siblings of hit areas, added last so they render on top)
            _circleRt = new RectTransform[CirclePts];
            for (int k = 0; k < CirclePts; k++)
            {
                var cd  = UIFactory.Panel(mapGo.transform, $"Circle{k}", ColorTheme.TextAccent);
                var crt = cd.GetComponent<RectTransform>();
                crt.sizeDelta        = new Vector2(4, 4);
                crt.anchorMin        = crt.anchorMax = new Vector2(0.5f, 0.5f);
                crt.anchoredPosition = Vector2.zero;
                _circleRt[k] = crt;
            }
        }

        public void OnShow()
        {
            // Reset view to default each time the screen is opened
            _zoom = DefaultZoom;
            _pan  = Vector2.zero;

            var G = GameState.Instance;
            _curSystem = G.Commander.CurSystem;
            _fuel      = G.Ship.Fuel;

            int tanks = FuelSystem.GetFuelTanks();
            _fuelText.text = $"Fuel: {_fuel}  Range: {tanks}";

            Canvas.ForceUpdateCanvases();
            RecalcScale();
            RefreshDots();
        }

        void Update()
        {
            if (!gameObject.activeSelf) return;
            HandleTouch();
        }

        // ── Touch input ───────────────────────────────────────────────────────

        void HandleTouch()
        {
            int count = Input.touchCount;

            if (count >= 2)
            {
                Touch t0 = Input.GetTouch(0);
                Touch t1 = Input.GetTouch(1);
                float dist = Vector2.Distance(t0.position, t1.position);

                if (t0.phase == TouchPhase.Began || t1.phase == TouchPhase.Began)
                {
                    _prevPinchDist = dist;
                    _isPinching    = true;
                }
                else if (_isPinching && dist > 1f && _prevPinchDist > 1f)
                {
                    _zoom = Mathf.Clamp(_zoom * (_prevPinchDist / dist), MinZoom, MaxZoom);
                    RecalcScale();
                    RefreshDots();
                    _prevPinchDist = dist;
                }
            }
            else if (count == 1)
            {
                _isPinching = false;
                Touch t = Input.GetTouch(0);
                if (t.phase == TouchPhase.Moved)
                {
                    // Convert screen-pixel delta → parsec delta (y-axis: screen up = parsec up)
                    _pan.x -= t.deltaPosition.x / _scale;
                    _pan.y -= t.deltaPosition.y / _scale;
                    RefreshDots();
                }
            }
            else
            {
                _isPinching = false;
            }
        }

        // ── Rendering ─────────────────────────────────────────────────────────

        void RecalcScale()
        {
            Rect r = _mapRect.rect;
            if (r.width <= 0 || r.height <= 0) { _scale = 5f; return; }
            // Use the shorter side so parsec distance is equal in both axes
            _scale = Mathf.Min(r.width, r.height) * 0.5f / _zoom;
        }

        void RefreshDots()
        {
            var G      = GameState.Instance;
            var curSys = G.SolarSystem[_curSystem];
            Rect r     = _mapRect.rect;
            float hw   = r.width  * 0.5f + HitSize;
            float hh   = r.height * 0.5f + HitSize;

            for (int i = 0; i < MaxSolarSystem; i++)
            {
                var sys = G.SolarSystem[i];
                // Pixel offset from map center, accounting for pan
                float px = (sys.X - curSys.X - _pan.x) * _scale;
                float py = (sys.Y - curSys.Y - _pan.y) * _scale;

                bool inView = Mathf.Abs(px) <= hw && Mathf.Abs(py) <= hh;
                _hitRt[i].gameObject.SetActive(inView);
                if (!inView) continue;

                _hitRt[i].anchoredPosition = new Vector2(px, py);

                bool isCurrent = i == _curSystem;
                bool visited   = sys.Visited || isCurrent;
                long dist      = GameMath.RealDistance(curSys, sys);
                bool inRange   = dist <= _fuel && !isCurrent;
                bool wormhole  = !isCurrent && TravelerSystem.WormholeExists(_curSystem, i);

                // Visual dot size
                var drt = _hitRt[i].GetChild(0).GetComponent<RectTransform>();
                drt.sizeDelta = isCurrent
                    ? new Vector2(CurDotSize, CurDotSize)
                    : new Vector2(DotSize,    DotSize);

                if (isCurrent)          _dotImg[i].color = ColorTheme.TextAccent;
                else if (inRange || wormhole) _dotImg[i].color = ColorTheme.TextPositive;
                else if (visited)       _dotImg[i].color = ColorTheme.TextSecondary;
                else                    _dotImg[i].color = ColorTheme.TextDisabled;

                _dotLabel[i].text  = (visited || isCurrent) ? GameData.SolarSystemNames[sys.NameIndex] : "";
                _dotLabel[i].color = isCurrent          ? ColorTheme.TextAccent
                                   : inRange || wormhole ? ColorTheme.TextPositive
                                   : ColorTheme.TextSecondary;
            }

            PlaceRangeCircle();
        }

        void PlaceRangeCircle()
        {
            float r = _fuel * _scale;  // pixel radius of fuel range
            // Offset by pan so circle stays centered on current system
            float panPx = -_pan.x * _scale;
            float panPy = -_pan.y * _scale;

            for (int k = 0; k < CirclePts; k++)
            {
                float angle = 2f * Mathf.PI * k / CirclePts;
                _circleRt[k].anchoredPosition = new Vector2(
                    panPx + r * Mathf.Cos(angle),
                    panPy + r * Mathf.Sin(angle));
            }
        }

        void SelectSystem(int idx)
        {
            GameState.Instance.WarpSystem = idx;
            UIManager.Instance.Navigate(GameScreen.TargetSystem);
        }
    }
}
