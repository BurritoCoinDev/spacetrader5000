// Space Trader 5000 – Android/Unity Port
// Travel screen: in-flight sequence, click-by-click. Advances until encounter or arrival.

using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static SpaceTrader.GameConstants;

namespace SpaceTrader.UI.Screens
{
    public class TravelUI : MonoBehaviour, IScreenUI
    {
        TextMeshProUGUI _statusText, _clicksText;
        Button _continueBtn;
        bool _waitingForInput;

        public void Initialize(GameObject panel)
        {
            UIFactory.Stretch(panel.GetComponent<RectTransform>());
            UIFactory.Header(panel.transform, "IN FLIGHT", null);

            // Travel progress
            var bg = UIFactory.Panel(panel.transform, "Bg", ColorTheme.PanelBg);
            UIFactory.SetAnchored(bg.GetComponent<RectTransform>(),
                new Vector2(0.05f, 0.35f), new Vector2(0.95f, 0.88f), Vector2.zero, Vector2.zero);

            _statusText = UIFactory.LabelWrap(bg.transform, "Status",
                "Traveling through deep space...",
                ColorTheme.FontBody, ColorTheme.TextPrimary, TextAlignmentOptions.Center);
            UIFactory.Stretch(_statusText.rectTransform, 12, 12, 8, 8);

            _clicksText = UIFactory.Label(panel.transform, "Clicks", "",
                ColorTheme.FontSmall, ColorTheme.TextSecondary, TextAlignmentOptions.Center);
            UIFactory.SetAnchored(_clicksText.rectTransform,
                new Vector2(0.05f, 0.30f), new Vector2(0.95f, 0.35f), Vector2.zero, Vector2.zero);

            _continueBtn = UIFactory.Btn(panel.transform, "Continue", "CONTINUE",
                OnContinue, ColorTheme.ButtonSuccess, ColorTheme.FontButton);
            UIFactory.SetAnchored(_continueBtn.GetComponent<RectTransform>(),
                new Vector2(0.1f, 0.10f), new Vector2(0.9f, 0.22f), Vector2.zero, Vector2.zero);
        }

        public void OnShow()
        {
            _waitingForInput = false;
            _continueBtn.interactable = true;
            AdvanceTravel();
        }

        void AdvanceTravel()
        {
            // Always reset to default first so stale listeners never carry over
            // when the screen is revisited (e.g. returning from an encounter).
            var btnLabel = _continueBtn.GetComponentInChildren<TextMeshProUGUI>();
            btnLabel.text = "CONTINUE";
            _continueBtn.onClick.RemoveAllListeners();
            _continueBtn.onClick.AddListener(OnContinue);
            _waitingForInput = false;

            var G = GameState.Instance;

            if (G.Clicks <= 0)
            {
                TravelerSystem.Arrival();
                _statusText.text = $"Arrived at {GameData.SolarSystemNames[G.CurrentSystem.NameIndex]}!";
                _clicksText.text = "";
                btnLabel.text = "DOCK";
                _continueBtn.onClick.RemoveAllListeners();
                _continueBtn.onClick.AddListener(OnArrival);
                return;
            }

            G.Clicks--;
            int enc = EncounterSystem.DetermineEncounter();
            _clicksText.text = $"Parsecs remaining: {G.Clicks}";

            if (enc < 0)
            {
                _statusText.text = "Traveling through deep space...";
            }
            else
            {
                string encDesc = DescribeEncounter(enc);
                _statusText.text = $"ENCOUNTER!\n{encDesc}";
                _waitingForInput = true;
                btnLabel.text = "ENGAGE";
                _continueBtn.onClick.RemoveAllListeners();
                _continueBtn.onClick.AddListener(OnEncounter);
            }
        }

        void Update()
        {
            if (GameState.Instance == null || !gameObject.activeInHierarchy) return;
            if (!_waitingForInput && GameState.Instance.Clicks > 0)
            {
                // Auto-advance after a short pause (simulate travel time)
                _autoTimer += Time.deltaTime;
                if (_autoTimer >= 0.35f)
                {
                    _autoTimer = 0;
                    AdvanceTravel();
                }
            }
        }

        float _autoTimer = 0;

        void OnContinue() => AdvanceTravel();

        void OnEncounter()
        {
            UIManager.Instance.Navigate(GameScreen.Encounter);
        }

        void OnArrival()
        {
            UIManager.Instance.Navigate(GameScreen.Docked);
        }

        string DescribeEncounter(int enc)
        {
            if (EncounterSystem.IsPolice(enc))
            {
                if (enc == PoliceInspection) return "Police ship — requesting inspection.";
                if (enc == PoliceAttack)     return "Police ship — attacking!";
                return "Police ship.";
            }
            if (EncounterSystem.IsPirate(enc))     return "Pirate ship — attacking!";
            if (EncounterSystem.IsTrader(enc))      return "Trader ship.";
            if (EncounterSystem.IsSpaceMonster(enc)) return "SPACE MONSTER!";
            if (EncounterSystem.IsDragonfly(enc))   return "The Dragonfly — attacking!";
            if (EncounterSystem.IsScarab(enc))      return "The Scarab — attacking!";
            if (EncounterSystem.IsFamousCaptain(enc))
            {
                if (enc == CaptainAhabEncounter)   return "Captain Ahab hails you.";
                if (enc == CaptainConradEncounter) return "Captain Conrad hails you.";
                if (enc == CaptainHuieEncounter)   return "Captain Huie hails you.";
                return "A famous captain hails you.";
            }
            if (enc == MarieCelesteEncounter)   return "The Marie Celeste — drifting!";
            if (enc == BottleOldEncounter)      return "An old bottle drifts past.";
            if (enc == BottleGoodEncounter)     return "A bottle of good stuff drifts past.";
            return "Unknown encounter.";
        }
    }
}
