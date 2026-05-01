// Space Trader 5000 – Android/Unity Port
// Buy Equipment screen: weapons, shields, gadgets.

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static SpaceTrader.GameConstants;

namespace SpaceTrader.UI.Screens
{
    public class BuyEquipmentUI : MonoBehaviour, IScreenUI
    {
        TextMeshProUGUI _creditsText, _shipText;
        readonly List<(Button btn, TextMeshProUGUI lbl, int equip, int category)> _items = new();

        // category: 0=weapon 1=shield 2=gadget
        public void Initialize(GameObject panel)
        {
            UIFactory.Stretch(panel.GetComponent<RectTransform>());
            UIFactory.Header(panel.transform, "BUY EQUIPMENT",
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

            // Column headers
            var colHdr = UIFactory.Panel(panel.transform, "ColHdr", ColorTheme.HeaderBg);
            UIFactory.SetAnchored(colHdr.GetComponent<RectTransform>(),
                new Vector2(0, 0.84f), new Vector2(1, 0.88f), Vector2.zero, Vector2.zero);
            BuildColumnHeaders(colHdr.transform);

            var (scroll, content) = UIFactory.ScrollView(panel.transform, "EquipList");
            UIFactory.SetAnchored(scroll.GetComponent<RectTransform>(),
                new Vector2(0, 0.02f), new Vector2(1, 0.84f), Vector2.zero, Vector2.zero);

            BuildEquipRows(content, "WEAPONS", 0);
            BuildEquipRows(content, "SHIELDS", 1);
            BuildEquipRows(content, "GADGETS", 2);
        }

        void BuildColumnHeaders(Transform parent)
        {
            string[] hdrs   = { "Item",  "Price", "TL", "" };
            float[]  widths = { 0.44f,   0.24f,   0.12f, 0.20f };
            float x = 0;
            foreach (var (h, w) in System.Linq.Enumerable.Zip(hdrs, widths, (a, b) => (a, b)))
            {
                var lbl = UIFactory.Label(parent, h, h,
                    ColorTheme.FontSmall, ColorTheme.TextSecondary, TextAlignmentOptions.Left);
                UIFactory.SetAnchored(lbl.rectTransform,
                    new Vector2(x, 0), new Vector2(x + w, 1), new Vector2(4, 2), new Vector2(-4, -2));
                x += w;
            }
        }

        void BuildEquipRows(Transform content, string header, int category)
        {
            // Section divider header
            var divRow = UIFactory.RowPanel(content, $"{header}Hdr", ColorTheme.HeaderBg, 36);
            var divLbl = UIFactory.Label(divRow.transform, "Lbl", header,
                ColorTheme.FontTiny, ColorTheme.TextAccent);
            UIFactory.Stretch(divLbl.rectTransform, 8, 8, 2, 2);

            int count = category == 0 ? MaxWeaponType + ExtraWeapons
                      : category == 1 ? MaxShieldType + ExtraShields
                      :                  MaxGadgetType + ExtraGadgets;

            for (int i = 0; i < count; i++)
            {
                int idx = i;
                int cat = category;
                string name  = category == 0 ? GameData.Weapontypes[i].Name
                             : category == 1 ? GameData.Shieldtypes[i].Name
                             :                 GameData.Gadgettypes[i].Name;
                long   price = category == 0 ? GameData.Weapontypes[i].Price
                             : category == 1 ? GameData.Shieldtypes[i].Price
                             :                 GameData.Gadgettypes[i].Price;
                int    tl    = category == 0 ? GameData.Weapontypes[i].TechLevel
                             : category == 1 ? GameData.Shieldtypes[i].TechLevel
                             :                 GameData.Gadgettypes[i].TechLevel;

                int rowIdx = _items.Count;
                var row = UIFactory.RowPanel(content, $"{header}Row{i}",
                    rowIdx % 2 == 0 ? ColorTheme.RowBg : ColorTheme.RowAlt, 72);

                var nameL = UIFactory.Label(row.transform, "Name", name,
                    ColorTheme.FontSmall, ColorTheme.TextPrimary);
                UIFactory.SetAnchored(nameL.rectTransform,
                    new Vector2(0, 0), new Vector2(0.44f, 1), new Vector2(8, 4), new Vector2(-4, -4));

                var priceL = UIFactory.Label(row.transform, "Price", UIFactory.Cr(price),
                    ColorTheme.FontSmall, ColorTheme.TextPositive, TextAlignmentOptions.Right);
                UIFactory.SetAnchored(priceL.rectTransform,
                    new Vector2(0.44f, 0), new Vector2(0.68f, 1), new Vector2(4, 4), new Vector2(-4, -4));

                var tlL = UIFactory.Label(row.transform, "TL", tl.ToString(),
                    ColorTheme.FontSmall, ColorTheme.TextSecondary, TextAlignmentOptions.Center);
                UIFactory.SetAnchored(tlL.rectTransform,
                    new Vector2(0.68f, 0), new Vector2(0.80f, 1), new Vector2(4, 4), new Vector2(-4, -4));

                var btn = UIFactory.SmallBtn(row.transform, "Buy", "BUY",
                    () => OnBuy(idx, cat), ColorTheme.ButtonSuccess);
                UIFactory.SetAnchored(btn.GetComponent<RectTransform>(),
                    new Vector2(0.80f, 0.1f), new Vector2(1f, 0.9f), new Vector2(4, 0), new Vector2(-8, 0));

                _items.Add((btn, nameL, i, category));
            }
        }

        public void OnShow() => Refresh();

        void Refresh()
        {
            var G  = GameState.Instance;
            var st = GameData.Shiptypes[G.Ship.Type];
            _creditsText.text = UIFactory.Cr(G.Credits);
            _shipText.text    = $"Ship: {st.Name}";

            int usedW = 0, usedS = 0, usedG = 0;
            for (int i = 0; i < MaxWeapon; i++) if (G.Ship.Weapon[i] >= 0) usedW++;
            for (int i = 0; i < MaxShield; i++) if (G.Ship.Shield[i] >= 0) usedS++;
            for (int i = 0; i < MaxGadget; i++) if (G.Ship.Gadget[i] >= 0) usedG++;

            bool hasWeaponSlot = usedW < st.WeaponSlots;
            bool hasShieldSlot = usedS < st.ShieldSlots;
            bool hasGadgetSlot = usedG < st.GadgetSlots;

            foreach (var (btn, lbl, idx, cat) in _items)
            {
                long price = cat == 0 ? GameData.Weapontypes[idx].Price
                           : cat == 1 ? GameData.Shieldtypes[idx].Price
                           :             GameData.Gadgettypes[idx].Price;
                int tl     = cat == 0 ? GameData.Weapontypes[idx].TechLevel
                           : cat == 1 ? GameData.Shieldtypes[idx].TechLevel
                           :             GameData.Gadgettypes[idx].TechLevel;

                bool hasSlot    = cat == 0 ? hasWeaponSlot : cat == 1 ? hasShieldSlot : hasGadgetSlot;
                bool techOK     = G.CurrentSystem.TechLevel >= tl;
                bool canAfford  = G.Credits >= price;
                btn.interactable = hasSlot && techOK && canAfford;
                lbl.color = (techOK && hasSlot) ? ColorTheme.TextPrimary : ColorTheme.TextDisabled;
            }
        }

        void OnBuy(int idx, int cat)
        {
            bool ok = cat == 0 ? ShipyardSystem.InstallWeapon(idx)
                    : cat == 1 ? ShipyardSystem.InstallShield(idx)
                    :             ShipyardSystem.InstallGadget(idx);
            if (ok) Refresh();
        }
    }
}
