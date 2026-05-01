// Space Trader 5000 – Android/Unity Port
// Warp confirmation screen: shows cost, distance, destination info, then warps.

using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static SpaceTrader.GameConstants;

namespace SpaceTrader.UI.Screens
{
    public class WarpUI : MonoBehaviour, IScreenUI
    {
        TextMeshProUGUI _infoText, _costText, _errorText;
        Button _warpBtn;

        public void Initialize(GameObject panel)
        {
            UIFactory.Stretch(panel.GetComponent<RectTransform>());
            UIFactory.Header(panel.transform, "WARP",
                () => UIManager.Instance.NavigateBack());

            var body = UIFactory.Panel(panel.transform, "Body", ColorTheme.PanelBg);
            UIFactory.SetAnchored(body.GetComponent<RectTransform>(),
                new Vector2(0.04f, 0.40f), new Vector2(0.96f, 0.87f),
                Vector2.zero, Vector2.zero);

            _infoText = UIFactory.LabelWrap(body.transform, "Info", "",
                ColorTheme.FontBody, ColorTheme.TextPrimary, TextAlignmentOptions.Left);
            UIFactory.Stretch(_infoText.rectTransform, 12, 12, 8, 8);

            _costText = UIFactory.Label(panel.transform, "Cost", "",
                ColorTheme.FontBody, ColorTheme.TextWarning, TextAlignmentOptions.Center);
            UIFactory.SetAnchored(_costText.rectTransform,
                new Vector2(0.04f, 0.30f), new Vector2(0.96f, 0.40f), Vector2.zero, Vector2.zero);

            _errorText = UIFactory.LabelWrap(panel.transform, "Error", "",
                ColorTheme.FontSmall, ColorTheme.TextNegative, TextAlignmentOptions.Center);
            UIFactory.SetAnchored(_errorText.rectTransform,
                new Vector2(0.04f, 0.18f), new Vector2(0.96f, 0.29f), Vector2.zero, Vector2.zero);

            _warpBtn = UIFactory.Btn(panel.transform, "WarpBtn", "ENGAGE WARP",
                OnWarp, ColorTheme.ButtonSuccess, ColorTheme.FontButton);
            UIFactory.SetAnchored(_warpBtn.GetComponent<RectTransform>(),
                new Vector2(0.1f, 0.05f), new Vector2(0.9f, 0.16f), Vector2.zero, Vector2.zero);
        }

        public void OnShow()
        {
            var G     = GameState.Instance;
            int cur   = G.Commander.CurSystem;
            int dest  = G.WarpSystem;
            var destSys = G.SolarSystem[dest];
            var pol   = GameData.PoliticsTypes[destSys.Politics];

            float dist = (float)GameMath.RealDistance(G.SolarSystem[cur], destSys);
            long merMon = MoneySystem.MercenaryMoney();
            long insMon = G.Insurance ? MoneySystem.InsuranceMoney() : 0;
            long wormTax = TravelerSystem.WormholeTax(cur, dest);
            long totalCost = merMon + insMon + wormTax;

            bool isWormhole = TravelerSystem.WormholeExists(cur, dest);
            bool visited    = destSys.Visited;

            _infoText.text =
                $"Destination: {GameData.SolarSystemNames[destSys.NameIndex]}\n" +
                $"Distance: {dist:F0} parsecs" + (isWormhole ? " (via wormhole)" : "") + "\n" +
                (visited
                    ? $"Government: {pol.Name}\nTech level: {GameData.TechLevelNames[destSys.TechLevel]}"
                    : "Unvisited system");

            string costLine = $"Fuel: {(isWormhole ? 0 : (int)dist)} units";
            if (merMon > 0)  costLine += $"  Crew: {UIFactory.Cr(merMon)}";
            if (insMon > 0)  costLine += $"  Insurance: {UIFactory.Cr(insMon)}";
            if (wormTax > 0) costLine += $"  Toll: {UIFactory.Cr(wormTax)}";
            _costText.text = costLine;

            _errorText.text = "";
            _warpBtn.interactable = true;

            // Pre-flight checks
            if (G.Debt > DebtTooLarge)
            {
                _errorText.text = "Cannot warp — debt exceeds limit!";
                _warpBtn.interactable = false;
            }
            else if (G.Credits < totalCost)
            {
                _errorText.text = "Cannot afford crew/insurance payments!";
                _warpBtn.interactable = false;
            }
            else if (!isWormhole && G.Ship.Fuel < (int)dist)
            {
                _errorText.text = "Not enough fuel!";
                _warpBtn.interactable = false;
            }
        }

        void OnWarp()
        {
            var result = TravelerSystem.DoWarp(false);
            switch (result)
            {
                case TravelerSystem.WarpResult.Success:
                    UIManager.Instance.Navigate(GameScreen.Travel);
                    break;
                case TravelerSystem.WarpResult.DebtTooLarge:
                    _errorText.text = "Cannot warp — debt exceeds limit!";
                    break;
                case TravelerSystem.WarpResult.CantPayMercenaries:
                    _errorText.text = "Cannot pay crew!";
                    break;
                case TravelerSystem.WarpResult.CantPayInsurance:
                    _errorText.text = "Cannot pay insurance!";
                    break;
                case TravelerSystem.WarpResult.CantPayWormholeTax:
                    _errorText.text = "Cannot pay wormhole tax!";
                    break;
                case TravelerSystem.WarpResult.WildNeedsWeapon:
                    _errorText.text = "Jonathan Wild requires a beam laser!";
                    break;
            }
        }
    }
}
