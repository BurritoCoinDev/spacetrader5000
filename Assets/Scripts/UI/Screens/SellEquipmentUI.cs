// Space Trader 5000 – Android/Unity Port
// Sell Equipment screen: remove installed weapons, shields, gadgets for 75% refund.

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static SpaceTrader.GameConstants;

namespace SpaceTrader.UI.Screens
{
    public class SellEquipmentUI : MonoBehaviour, IScreenUI
    {
        TextMeshProUGUI _creditsText;
        Transform _listContent;

        public void Initialize(GameObject panel)
        {
            UIFactory.Stretch(panel.GetComponent<RectTransform>());
            UIFactory.Header(panel.transform, "SELL EQUIPMENT",
                () => UIManager.Instance.NavigateBack());

            var strip = UIFactory.Panel(panel.transform, "Strip", ColorTheme.RowBg);
            UIFactory.SetAnchored(strip.GetComponent<RectTransform>(),
                new Vector2(0, 0.88f), new Vector2(1, 0.935f), Vector2.zero, Vector2.zero);

            _creditsText = UIFactory.Label(strip.transform, "Credits", "",
                ColorTheme.FontBody, ColorTheme.TextPositive, TextAlignmentOptions.Left);
            UIFactory.Stretch(_creditsText.rectTransform, 12, 12, 4, 4);

            var note = UIFactory.Label(strip.transform, "Note", "75% refund",
                ColorTheme.FontSmall, ColorTheme.TextSecondary, TextAlignmentOptions.Right);
            UIFactory.Stretch(note.rectTransform, 12, 12, 4, 4);

            var (scroll, content) = UIFactory.ScrollView(panel.transform, "EquipList");
            UIFactory.SetAnchored(scroll.GetComponent<RectTransform>(),
                new Vector2(0, 0.02f), new Vector2(1, 0.88f), Vector2.zero, Vector2.zero);
            _listContent = content;
        }

        public void OnShow() => BuildAndRefresh();

        int _rowCount;

        void BuildAndRefresh()
        {
            foreach (Transform child in _listContent) Destroy(child.gameObject);
            _rowCount = 0;

            var G  = GameState.Instance;
            _creditsText.text = UIFactory.Cr(G.Credits);

            bool any = false;

            // Quest-reward equipment (Morgan's Laser, Lightning Shield, Fuel
            // Compactor) lives at indices >= Max*Type and isn't sellable —
            // original blocks the sale to prevent laundering high-value items.
            for (int i = 0; i < MaxWeapon; i++)
            {
                int wIdx = G.Ship.Weapon[i];
                if (wIdx < 0 || wIdx >= MaxWeaponType) continue;
                any = true;
                int slot = i;
                var wt   = GameData.Weapontypes[wIdx];
                AddRow($"W{i}", wt.Name, wt.Price * 3 / 4, () => {
                    ShipyardSystem.RemoveWeapon(slot);
                    BuildAndRefresh();
                });
            }

            for (int i = 0; i < MaxShield; i++)
            {
                int sIdx = G.Ship.Shield[i];
                if (sIdx < 0 || sIdx >= MaxShieldType) continue;
                any = true;
                int slot = i;
                var sh   = GameData.Shieldtypes[sIdx];
                AddRow($"S{i}", sh.Name, sh.Price * 3 / 4, () => {
                    ShipyardSystem.RemoveShield(slot);
                    BuildAndRefresh();
                });
            }

            for (int i = 0; i < MaxGadget; i++)
            {
                int gIdx = G.Ship.Gadget[i];
                if (gIdx < 0 || gIdx >= MaxGadgetType) continue;
                any = true;
                int slot = i;
                var gd   = GameData.Gadgettypes[gIdx];
                AddRow($"G{i}", gd.Name, gd.Price * 3 / 4, () => {
                    ShipyardSystem.RemoveGadget(slot);
                    BuildAndRefresh();
                });
            }

            if (!any)
            {
                var none = UIFactory.Label(_listContent, "None",
                    "No equipment installed.",
                    ColorTheme.FontBody, ColorTheme.TextDisabled, TextAlignmentOptions.Center);
                none.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 80);
                none.gameObject.AddComponent<LayoutElement>().preferredHeight = 80;
            }
        }

        void AddRow(string name, string itemName, long refund, UnityEngine.Events.UnityAction onClick)
        {
            int rowIdx = _rowCount++;
            var row = UIFactory.RowPanel(_listContent, name,
                rowIdx % 2 == 0 ? ColorTheme.RowBg : ColorTheme.RowAlt, 72);

            var nameL = UIFactory.Label(row.transform, "Name", itemName,
                ColorTheme.FontSmall, ColorTheme.TextPrimary);
            UIFactory.SetAnchored(nameL.rectTransform,
                new Vector2(0, 0), new Vector2(0.55f, 1), new Vector2(8, 4), new Vector2(-4, -4));

            var refundL = UIFactory.Label(row.transform, "Refund",
                $"Refund: {UIFactory.Cr(refund)}",
                ColorTheme.FontSmall, ColorTheme.TextPositive, TextAlignmentOptions.Right);
            UIFactory.SetAnchored(refundL.rectTransform,
                new Vector2(0.55f, 0), new Vector2(0.80f, 1), new Vector2(4, 4), new Vector2(-4, -4));

            var sellBtn = UIFactory.SmallBtn(row.transform, "Sell", "SELL",
                onClick, ColorTheme.ButtonDanger);
            UIFactory.SetAnchored(sellBtn.GetComponent<RectTransform>(),
                new Vector2(0.80f, 0.1f), new Vector2(1f, 0.9f), new Vector2(4, 0), new Vector2(-8, 0));
        }
    }
}
