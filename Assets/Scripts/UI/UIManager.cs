// Space Trader 5000 – Android/Unity Port
// Central screen manager. Builds the Canvas, instantiates every screen
// panel, and handles navigation and the Android back button.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace SpaceTrader.UI
{
    public enum GameScreen
    {
        Title, NewCommander, Docked,
        BuyCargo, SellCargo, Shipyard,
        BuyEquipment, SellEquipment, PersonnelRoster, Bank,
        SystemInfo, CommanderStatus,
        GalacticChart, Warp, Travel,
        Encounter, SpecialEvent, HighScores,
    }

    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        Canvas                                      _canvas;
        readonly Dictionary<GameScreen, GameObject> _panels   = new();
        readonly Dictionary<GameScreen, IScreenUI>  _screens  = new();
        readonly Stack<GameScreen>                  _navStack = new();

        // ── Lifecycle ──────────────────────────────────────────────

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            BuildCanvas();
            RegisterScreens();
        }

        void Start()
        {
            Navigate(GameScreen.Title);
        }

        void Update()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) HandleBack();
        }

        // ── Public navigation API ────────────────────────────────────

        public void Navigate(GameScreen screen)
        {
            _navStack.Push(screen);
            ShowOnly(screen);
        }

        public void NavigateBack()
        {
            if (_navStack.Count > 1)
            {
                _navStack.Pop();
                ShowOnly(_navStack.Peek());
            }
        }

        public GameScreen Current =>
            _navStack.Count > 0 ? _navStack.Peek() : GameScreen.Title;

        public T GetScreen<T>(GameScreen key) where T : class, IScreenUI
            => _screens.TryGetValue(key, out var s) ? s as T : null;

        // ── Canvas construction ──────────────────────────────────────────

        void BuildCanvas()
        {
            var cGo = new GameObject("Canvas");
            cGo.transform.SetParent(transform, false);
            _canvas = cGo.AddComponent<Canvas>();
            _canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 0;

            var cs = cGo.AddComponent<CanvasScaler>();
            cs.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            cs.referenceResolution = new Vector2(1080, 1920);
            cs.screenMatchMode     = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            cs.matchWidthOrHeight  = 0.5f;

            cGo.AddComponent<GraphicRaycaster>();

            var bg    = new GameObject("Background");
            bg.transform.SetParent(cGo.transform, false);
            var bgImg = bg.AddComponent<Image>();
            bgImg.color = ColorTheme.Background;
            UIFactory.Stretch(bgImg.GetComponent<RectTransform>());
        }

        void RegisterScreens()
        {
            AddScreen<Screens.TitleScreenUI>     (GameScreen.Title);
            AddScreen<Screens.NewCommanderUI>    (GameScreen.NewCommander);
            AddScreen<Screens.DockedUI>          (GameScreen.Docked);
            AddScreen<Screens.BuyCargoUI>        (GameScreen.BuyCargo);
            AddScreen<Screens.SellCargoUI>       (GameScreen.SellCargo);
            AddScreen<Screens.ShipyardUI>        (GameScreen.Shipyard);
            AddScreen<Screens.BuyEquipmentUI>    (GameScreen.BuyEquipment);
            AddScreen<Screens.SellEquipmentUI>   (GameScreen.SellEquipment);
            AddScreen<Screens.PersonnelRosterUI> (GameScreen.PersonnelRoster);
            AddScreen<Screens.BankUI>            (GameScreen.Bank);
            AddScreen<Screens.SystemInfoUI>      (GameScreen.SystemInfo);
            AddScreen<Screens.CommanderStatusUI> (GameScreen.CommanderStatus);
            AddScreen<Screens.GalacticChartUI>   (GameScreen.GalacticChart);
            AddScreen<Screens.WarpUI>            (GameScreen.Warp);
            AddScreen<Screens.TravelUI>          (GameScreen.Travel);
            AddScreen<Screens.EncounterUI>       (GameScreen.Encounter);
            AddScreen<Screens.SpecialEventUI>    (GameScreen.SpecialEvent);
            AddScreen<Screens.HighScoresUI>      (GameScreen.HighScores);

            foreach (var p in _panels.Values) p.SetActive(false);
        }

        void AddScreen<T>(GameScreen key) where T : MonoBehaviour, IScreenUI
        {
            var panel = new GameObject(key.ToString());
            panel.transform.SetParent(_canvas.transform, false);
            var rt = panel.AddComponent<RectTransform>();
            UIFactory.Stretch(rt);
            var screen = panel.AddComponent<T>();
            screen.Initialize(panel);
            _panels[key]  = panel;
            _screens[key] = screen;
        }

        void ShowOnly(GameScreen screen)
        {
            foreach (var kv in _panels)
            {
                bool show = kv.Key == screen;
                if (kv.Value.activeSelf != show) kv.Value.SetActive(show);
            }

            if (_screens.TryGetValue(screen, out var s)) s.OnShow();

            // ContentSizeFitter and TMP don't recompute when a panel is
            // re-activated after being hidden. Force a full layout rebuild.
            if (_panels.TryGetValue(screen, out var activePanel))
            {
                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate(
                    activePanel.GetComponent<RectTransform>());
            }
        }

        void HandleBack()
        {
            switch (Current)
            {
                case GameScreen.Title:
                    break;
                case GameScreen.Docked:
                    break;
                default:
                    NavigateBack();
                    break;
            }
        }
    }

    public interface IScreenUI
    {
        void Initialize(GameObject panel);
        void OnShow();
    }
}
