// Space Trader 5000 – Android/Unity Port
// Average Price List: estimated trade prices at a target system, with
// absolute / price-difference toggle and per-item buy dialog.

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SpaceTrader;
using static SpaceTrader.GameConstants;

namespace SpaceTrader.UI.Screens
{
    public class AveragePriceListUI : MonoBehaviour, IScreenUI
    {
        // Two columns × five rows of trade item cells
        // (Water Furs Food Ore Games | Firearms Medicine Machines Narcotics Robots)
        struct ItemCell
        {
            public TextMeshProUGUI NameLbl, PriceLbl;
            public Button          Btn;
        }

        TextMeshProUGUI _sysName, _resourceText, _bayText;
        Button _toggleBtn;
        bool _showDifference;

        readonly ItemCell[] _cells = new ItemCell[MaxTradeItem];

        // ── Buy dialog overlay ────────────────────────────────────────────────
        GameObject              _buyDialog;
        TextMeshProUGUI         _buyMsg;
        TMP_InputField          _buyInput;
        int                     _buyItemIdx;
        long                    _buyUnitPrice;

        public void Initialize(GameObject panel)
        {
            UIFactory.Stretch(panel.GetComponent<RectTransform>());
            UIFactory.Header(panel.transform, "AVERAGE PRICE LIST",
                () => UIManager.Instance.NavigateBack());

            // ── System name strip ─────────────────────────────────────────────
            var nameStrip = UIFactory.Panel(panel.transform, "NameStrip", ColorTheme.RowBg);
            UIFactory.SetAnchored(nameStrip.GetComponent<RectTransform>(),
                new Vector2(0, 0.86f), new Vector2(1, 0.915f), Vector2.zero, Vector2.zero);

            _sysName = UIFactory.Label(nameStrip.transform, "SysName", "",
                ColorTheme.FontBody, ColorTheme.TextAccent, TextAlignmentOptions.Center);
            UIFactory.Stretch(_sysName.rectTransform, 8, 8, 4, 4);

            // ── Resource text ─────────────────────────────────────────────────
            _resourceText = UIFactory.LabelWrap(panel.transform, "ResText", "",
                ColorTheme.FontSmall, ColorTheme.TextSecondary, TextAlignmentOptions.Center);
            UIFactory.SetAnchored(_resourceText.rectTransform,
                new Vector2(0.02f, 0.82f), new Vector2(0.98f, 0.86f), Vector2.zero, Vector2.zero);

            // ── 2-column trade grid (10 items = 5 rows × 2 cols) ─────────────
            BuildItemGrid(panel);

            // ── Bottom strip ──────────────────────────────────────────────────
            // Cargo bays info
            _bayText = UIFactory.Label(panel.transform, "Bays", "",
                ColorTheme.FontSmall, ColorTheme.TextSecondary, TextAlignmentOptions.Right);
            UIFactory.SetAnchored(_bayText.rectTransform,
                new Vector2(0.55f, 0.31f), new Vector2(0.97f, 0.37f), Vector2.zero, Vector2.zero);

            // Toggle button (Absolute Prices ↔ Price Differences)
            _toggleBtn = UIFactory.Btn(panel.transform, "Toggle", "PRICE DIFFERENCES",
                OnToggle, ColorTheme.ButtonNormal, ColorTheme.FontButton);
            UIFactory.SetAnchored(_toggleBtn.GetComponent<RectTransform>(),
                new Vector2(0.03f, 0.23f), new Vector2(0.60f, 0.34f), Vector2.zero, Vector2.zero);

            var sysInfoBtn = UIFactory.Btn(panel.transform, "SysInfo", "SYSTEM INFORMATION",
                () => UIManager.Instance.Navigate(GameScreen.SystemInfo),
                ColorTheme.ButtonNormal, ColorTheme.FontButton);
            UIFactory.SetAnchored(sysInfoBtn.GetComponent<RectTransform>(),
                new Vector2(0.03f, 0.12f), new Vector2(0.60f, 0.22f), Vector2.zero, Vector2.zero);

            var srcBtn = UIFactory.Btn(panel.transform, "ShortChart", "SHORT RANGE CHART",
                () => UIManager.Instance.Navigate(GameScreen.ShortRangeChart),
                ColorTheme.ButtonNormal, ColorTheme.FontButton);
            UIFactory.SetAnchored(srcBtn.GetComponent<RectTransform>(),
                new Vector2(0.03f, 0.02f), new Vector2(0.60f, 0.11f), Vector2.zero, Vector2.zero);

            var warpBtn = UIFactory.Btn(panel.transform, "Warp", "WARP",
                () => UIManager.Instance.Navigate(GameScreen.Warp),
                ColorTheme.ButtonSuccess, ColorTheme.FontButton);
            UIFactory.SetAnchored(warpBtn.GetComponent<RectTransform>(),
                new Vector2(0.64f, 0.02f), new Vector2(0.97f, 0.34f), Vector2.zero, Vector2.zero);

            // ── Buy dialog (starts hidden) ────────────────────────────────────
            BuildBuyDialog(panel);
        }

        void BuildItemGrid(GameObject panel)
        {
            // Two half-width columns, 5 rows each
            // Left column: Water Furs Food Ore Games  (indices 0-4)
            // Right column: Firearms Medicine Machines Narcotics Robots (indices 5-9)
            float gridTop  = 0.80f;
            float gridBot  = 0.38f;
            float rowH     = (gridTop - gridBot) / 5f;

            for (int i = 0; i < MaxTradeItem; i++)
            {
                int col = i / 5;    // 0=left, 1=right
                int row = i % 5;

                float xMin = col == 0 ? 0.02f : 0.52f;
                float xMax = col == 0 ? 0.50f : 0.98f;
                float yTop = gridTop - row * rowH;
                float yBot = yTop - rowH;

                var rowGo = UIFactory.Panel(panel.transform, $"ItemRow{i}",
                    row % 2 == 0 ? ColorTheme.RowBg : ColorTheme.RowAlt);
                UIFactory.SetAnchored(rowGo.GetComponent<RectTransform>(),
                    new Vector2(xMin, yBot), new Vector2(xMax, yTop),
                    new Vector2(2, 2), new Vector2(-2, -2));

                var btn = rowGo.AddComponent<Button>();

                var nameLbl = UIFactory.Label(rowGo.transform, "Name",
                    GameData.Tradeitems[i].Name,
                    ColorTheme.FontSmall, ColorTheme.TextPrimary, TextAlignmentOptions.Left);
                UIFactory.SetAnchored(nameLbl.rectTransform,
                    new Vector2(0, 0), new Vector2(0.55f, 1),
                    new Vector2(4, 2), new Vector2(-2, -2));

                var priceLbl = UIFactory.Label(rowGo.transform, "Price", "---",
                    ColorTheme.FontSmall, ColorTheme.TextPositive, TextAlignmentOptions.Right);
                UIFactory.SetAnchored(priceLbl.rectTransform,
                    new Vector2(0.55f, 0), new Vector2(1, 1),
                    new Vector2(2, 2), new Vector2(-4, -2));

                _cells[i] = new ItemCell { NameLbl = nameLbl, PriceLbl = priceLbl, Btn = btn };
            }
        }

        void BuildBuyDialog(GameObject panel)
        {
            _buyDialog = UIFactory.Panel(panel.transform, "BuyDialog", new Color(0.1f, 0.1f, 0.3f, 0.97f));
            UIFactory.SetAnchored(_buyDialog.GetComponent<RectTransform>(),
                new Vector2(0.04f, 0.28f), new Vector2(0.96f, 0.72f), Vector2.zero, Vector2.zero);

            _buyMsg = UIFactory.LabelWrap(_buyDialog.transform, "Msg", "",
                ColorTheme.FontSmall, ColorTheme.TextPrimary, TextAlignmentOptions.Left);
            UIFactory.SetAnchored(_buyMsg.rectTransform,
                new Vector2(0, 0.55f), Vector2.one, new Vector2(10, 4), new Vector2(-10, -4));

            _buyInput = UIFactory.InputField(_buyDialog.transform, "QtyInput", "0");
            UIFactory.SetAnchored(_buyInput.GetComponent<RectTransform>(),
                new Vector2(0.02f, 0.34f), new Vector2(0.98f, 0.55f), Vector2.zero, Vector2.zero);

            var okBtn = UIFactory.Btn(_buyDialog.transform, "OK", "OK",
                OnBuyOK, ColorTheme.ButtonSuccess, ColorTheme.FontButton);
            UIFactory.SetAnchored(okBtn.GetComponent<RectTransform>(),
                new Vector2(0.02f, 0.05f), new Vector2(0.32f, 0.32f), Vector2.zero, Vector2.zero);

            var allBtn = UIFactory.Btn(_buyDialog.transform, "All", "ALL",
                OnBuyAll, ColorTheme.ButtonNormal, ColorTheme.FontButton);
            UIFactory.SetAnchored(allBtn.GetComponent<RectTransform>(),
                new Vector2(0.36f, 0.05f), new Vector2(0.64f, 0.32f), Vector2.zero, Vector2.zero);

            var noneBtn = UIFactory.Btn(_buyDialog.transform, "None", "NONE",
                () => _buyDialog.SetActive(false), ColorTheme.ButtonNormal, ColorTheme.FontButton);
            UIFactory.SetAnchored(noneBtn.GetComponent<RectTransform>(),
                new Vector2(0.68f, 0.05f), new Vector2(0.98f, 0.32f), Vector2.zero, Vector2.zero);

            _buyDialog.SetActive(false);
        }

        public void OnShow()
        {
            _buyDialog.SetActive(false);
            RefreshPrices();
        }

        void RefreshPrices()
        {
            var G      = GameState.Instance;
            int target = G.WarpSystem >= 0 ? G.WarpSystem : G.Commander.CurSystem;
            int cur    = G.Commander.CurSystem;

            var sys = G.SolarSystem[target];
            _sysName.text = GameData.SolarSystemNames[sys.NameIndex];
            _resourceText.text = sys.SpecialResources > 0
                ? GameData.SpecialResources[sys.SpecialResources]
                : "Special resources unknown";

            _bayText.text = $"Bays: {CargoSystem.FilledCargoBays()}/{CargoSystem.TotalCargoBays()}";

            string toggleLabel = _showDifference ? "ABSOLUTE PRICES" : "PRICE DIFFERENCES";
            _toggleBtn.GetComponentInChildren<TextMeshProUGUI>().text = toggleLabel;

            // Buying is only meaningful when the displayed system is the
            // current system — otherwise StandardPrice is just an estimate
            // and we don't have system stock to deduct from.
            bool atTarget = (target == cur);

            for (int i = 0; i < MaxTradeItem; i++)
            {
                // Average estimated price at target system (no random variance)
                long targetPrice = TravelerSystem.StandardPrice(
                    i, sys.Size, sys.TechLevel, sys.Politics, sys.SpecialResources);

                int idx = i;
                _cells[i].Btn.onClick.RemoveAllListeners();

                if (targetPrice <= 0)
                {
                    _cells[i].PriceLbl.text  = "---";
                    _cells[i].PriceLbl.color = ColorTheme.TextSecondary;
                    _cells[i].NameLbl.fontStyle = FontStyles.Normal;
                    _cells[i].Btn.interactable  = false;
                    continue;
                }

                // Only allow buying at the current system, where G.BuyPrice
                // is real and CargoSystem.BuyCargo can deduct stock.
                _cells[i].Btn.interactable = atTarget && G.BuyPrice[i] > 0;
                if (_cells[i].Btn.interactable)
                    _cells[i].Btn.onClick.AddListener(() => OpenBuyDialog(idx));

                if (_showDifference)
                {
                    // Compare against current system's buy price (or estimated if not known)
                    long curPrice = G.BuyPrice[i] > 0 ? G.BuyPrice[i]
                        : TravelerSystem.StandardPrice(i,
                            G.SolarSystem[cur].Size,
                            G.SolarSystem[cur].TechLevel,
                            G.SolarSystem[cur].Politics,
                            G.SolarSystem[cur].SpecialResources);

                    long diff = targetPrice - curPrice;
                    _cells[i].PriceLbl.text  = diff >= 0 ? $"+{diff} cr." : $"{diff} cr.";
                    _cells[i].PriceLbl.color = diff > 0 ? ColorTheme.TextPositive
                                             : diff < 0 ? ColorTheme.TextNegative
                                             : ColorTheme.TextSecondary;
                }
                else
                {
                    _cells[i].PriceLbl.text  = UIFactory.Cr(targetPrice);
                    _cells[i].PriceLbl.color = ColorTheme.TextPositive;
                }

                // Bold for illegal goods
                bool illegal = (i == Narcotics || i == Firearms);
                _cells[i].NameLbl.fontStyle = illegal ? FontStyles.Bold : FontStyles.Normal;
            }
        }

        void OnToggle()
        {
            _showDifference = !_showDifference;
            RefreshPrices();
        }

        void OpenBuyDialog(int itemIdx)
        {
            var G = GameState.Instance;
            _buyItemIdx   = itemIdx;
            _buyUnitPrice = G.BuyPrice[itemIdx]; // skill / record adjusted

            int maxBuy = CargoSystem.GetAmountToBuy(itemIdx);

            _buyMsg.text = $"At {UIFactory.Cr(_buyUnitPrice)} each, you can afford {maxBuy}.\n" +
                           $"How many do you want to buy?";
            _buyInput.text = maxBuy > 0 ? "1" : "0";
            _buyDialog.SetActive(true);
        }

        void OnBuyOK()
        {
            if (!int.TryParse(_buyInput.text, out int qty) || qty <= 0)
            {
                _buyDialog.SetActive(false);
                return;
            }
            ExecuteBuy(qty);
        }

        void OnBuyAll()
        {
            ExecuteBuy(CargoSystem.GetAmountToBuy(_buyItemIdx));
        }

        void ExecuteBuy(int qty)
        {
            // CargoSystem.BuyCargo handles affordability, free-bay clamping,
            // running-average BuyingPrice, and system-stock deduction.
            CargoSystem.BuyCargo(_buyItemIdx, qty);
            _buyDialog.SetActive(false);
            RefreshPrices();
        }
    }
}
