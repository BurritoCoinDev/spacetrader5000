// Space Trader 5000 – Android/Unity Port
// Shipyard screen: buy/sell ship, buy fuel, buy repairs.

using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static SpaceTrader.GameConstants;

namespace SpaceTrader.UI.Screens
{
    public class ShipyardUI : MonoBehaviour, IScreenUI
    {
        TextMeshProUGUI _creditsText, _shipText;
        TextMeshProUGUI _fuelCostText, _repairCostText;
        Button _buyFuelBtn, _buyRepairBtn;

        public void Initialize(GameObject panel)
        {
            UIFactory.Stretch(panel.GetComponent<RectTransform>());
            UIFactory.Header(panel.transform, "SHIPYARD",
                () => UIManager.Instance.NavigateBack());

            var strip = UIFactory.Panel(panel.transform, "Strip", ColorTheme.RowBg);
            UIFactory.SetAnchored(strip.GetComponent<RectTransform>(),
                new Vector2(0, 0.88f), new Vector2(1, 0.935f), Vector2.zero, Vector2.zero);

            _creditsText = UIFactory.Label(strip.transform, "Credits", "",
                ColorTheme.FontBody, ColorTheme.TextPositive, TextAlignmentOptions.Left);
            UIFactory.Stretch(_creditsText.rectTransform, 12, 12, 4, 4);

            _shipText = UIFactory.Label(strip.transform, "Ship", "",
                ColorTheme.FontBody, ColorTheme.TextSecondary, TextAlignmentOptions.Right);
            UIFactory.Stretch(_shipText.rectTransform, 12, 12, 4, 4);

            BuildSection(panel.transform, "FUEL",
                new Vector2(0, 0.72f), new Vector2(1, 0.87f),
                BuildFuelContent);

            BuildSection(panel.transform, "REPAIRS",
                new Vector2(0, 0.54f), new Vector2(1, 0.71f),
                BuildRepairContent);

            BuildSection(panel.transform, "BUY NEW SHIP",
                new Vector2(0, 0.02f), new Vector2(1, 0.53f),
                BuildShipContent);
        }

        void BuildSection(Transform parent, string title,
            Vector2 aMin, Vector2 aMax,
            System.Action<Transform> builder)
        {
            var sec = UIFactory.Panel(parent, title, ColorTheme.PanelBg);
            UIFactory.SetAnchored(sec.GetComponent<RectTransform>(),
                aMin, aMax, new Vector2(8, 4), new Vector2(-8, -4));

            var hdr = UIFactory.Label(sec.transform, "Hdr", title,
                ColorTheme.FontSmall, ColorTheme.TextAccent, TextAlignmentOptions.Left);
            UIFactory.SetAnchored(hdr.rectTransform,
                new Vector2(0, 0.80f), Vector2.one, new Vector2(8, 0), new Vector2(-8, 0));

            UIFactory.Divider(sec.transform, "Div");

            builder(sec.transform);
        }

        void BuildFuelContent(Transform parent)
        {
            _fuelCostText = UIFactory.Label(parent, "FuelInfo", "",
                ColorTheme.FontSmall, ColorTheme.TextPrimary, TextAlignmentOptions.Left);
            UIFactory.SetAnchored(_fuelCostText.rectTransform,
                new Vector2(0, 0.10f), new Vector2(0.65f, 0.75f), new Vector2(8, 0), new Vector2(-4, 0));

            _buyFuelBtn = UIFactory.Btn(parent, "BuyFuel", "FILL TANK",
                OnBuyFuel, ColorTheme.ButtonSuccess);
            UIFactory.SetAnchored(_buyFuelBtn.GetComponent<RectTransform>(),
                new Vector2(0.65f, 0.10f), new Vector2(1f, 0.80f), new Vector2(4, 4), new Vector2(-8, -4));
        }

        void BuildRepairContent(Transform parent)
        {
            _repairCostText = UIFactory.Label(parent, "RepairInfo", "",
                ColorTheme.FontSmall, ColorTheme.TextPrimary, TextAlignmentOptions.Left);
            UIFactory.SetAnchored(_repairCostText.rectTransform,
                new Vector2(0, 0.10f), new Vector2(0.65f, 0.75f), new Vector2(8, 0), new Vector2(-4, 0));

            _buyRepairBtn = UIFactory.Btn(parent, "BuyRepair", "FULL REPAIR",
                OnBuyRepair, ColorTheme.ButtonSuccess);
            UIFactory.SetAnchored(_buyRepairBtn.GetComponent<RectTransform>(),
                new Vector2(0.65f, 0.10f), new Vector2(1f, 0.80f), new Vector2(4, 4), new Vector2(-8, -4));
        }

        void BuildShipContent(Transform parent)
        {
            var (scroll, content) = UIFactory.ScrollView(parent, "ShipList");
            UIFactory.SetAnchored(scroll.GetComponent<RectTransform>(),
                new Vector2(0, 0), new Vector2(1, 0.80f), new Vector2(4, 4), new Vector2(-4, -4));

            for (int i = 0; i < MaxShipType; i++)
            {
                int idx = i;
                var st = GameData.Shiptypes[i];
                var row = UIFactory.RowPanel(content, $"Ship{i}",
                    i % 2 == 0 ? ColorTheme.RowBg : ColorTheme.RowAlt, 100);

                var nameL = UIFactory.Label(row.transform, "Name", st.Name,
                    ColorTheme.FontSmall, ColorTheme.TextPrimary);
                UIFactory.SetAnchored(nameL.rectTransform,
                    new Vector2(0, 0.5f), new Vector2(0.30f, 1), new Vector2(8, 4), new Vector2(-4, -4));

                var statsL = UIFactory.Label(row.transform, "Stats",
                    $"W:{st.WeaponSlots} S:{st.ShieldSlots} G:{st.GadgetSlots} Bay:{st.CargoBays}",
                    ColorTheme.FontTiny, ColorTheme.TextSecondary);
                UIFactory.SetAnchored(statsL.rectTransform,
                    new Vector2(0, 0), new Vector2(0.30f, 0.5f), new Vector2(8, 4), new Vector2(-4, -4));

                var priceL = UIFactory.Label(row.transform, "Price", UIFactory.Cr(st.Price),
                    ColorTheme.FontSmall, ColorTheme.TextPositive, TextAlignmentOptions.Right);
                UIFactory.SetAnchored(priceL.rectTransform,
                    new Vector2(0.30f, 0.1f), new Vector2(0.65f, 0.9f), new Vector2(4, 4), new Vector2(-4, -4));

                var buyBtn = UIFactory.SmallBtn(row.transform, "Buy", "BUY",
                    () => OnBuyShip(idx), ColorTheme.ButtonSuccess);
                UIFactory.SetAnchored(buyBtn.GetComponent<RectTransform>(),
                    new Vector2(0.65f, 0.15f), new Vector2(1f, 0.85f), new Vector2(4, 0), new Vector2(-8, 0));
            }
        }

        public void OnShow() => Refresh();

        void Refresh()
        {
            var G = GameState.Instance;
            var st = GameData.Shiptypes[G.Ship.Type];
            _creditsText.text = UIFactory.Cr(G.Credits);
            _shipText.text    = $"Ship: {st.Name}";

            int maxFuel = FuelSystem.GetFuelTanks();
            int missing  = maxFuel - G.Ship.Fuel;
            long fuelCost = FuelSystem.FuelCostFor(missing);
            _fuelCostText.text = $"Fuel: {G.Ship.Fuel}/{maxFuel}\nCost to fill: {UIFactory.Cr(fuelCost)}";
            _buyFuelBtn.interactable = missing > 0 && G.Credits >= fuelCost;

            long maxHull   = ShipyardSystem.GetHullStrength();
            long missingHull = maxHull - G.Ship.Hull;
            long repairCost  = ShipyardSystem.RepairCostFor((int)missingHull);
            _repairCostText.text = $"Hull: {G.Ship.Hull}/{maxHull}\nCost to repair: {UIFactory.Cr(repairCost)}";
            _buyRepairBtn.interactable = missingHull > 0 && G.Credits >= repairCost;
        }

        void OnBuyFuel()
        {
            var G = GameState.Instance;
            int missing = FuelSystem.GetFuelTanks() - G.Ship.Fuel;
            FuelSystem.BuyFuel(missing);
            Refresh();
        }

        void OnBuyRepair()
        {
            long maxHull    = ShipyardSystem.GetHullStrength();
            long missingHull = maxHull - GameState.Instance.Ship.Hull;
            ShipyardSystem.BuyRepairs((int)missingHull);
            Refresh();
        }

        void OnBuyShip(int shipIdx)
        {
            var G  = GameState.Instance;
            var st = GameData.Shiptypes[shipIdx];
            long tradein = ShipPriceSystem.CurrentShipPriceWithoutCargo(true);
            long net     = st.Price - tradein;

            if (G.Credits < net) return;
            G.Credits -= net;
            ShipyardSystem.CreateShip(shipIdx);
            Refresh();
        }
    }
}
