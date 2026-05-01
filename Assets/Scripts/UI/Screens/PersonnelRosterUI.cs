// Space Trader 5000 – Android/Unity Port
// Personnel Roster screen: hire/fire mercenaries.

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static SpaceTrader.GameConstants;

namespace SpaceTrader.UI.Screens
{
    public class PersonnelRosterUI : MonoBehaviour, IScreenUI
    {
        TextMeshProUGUI _creditsText;
        Transform _listContent;

        public void Initialize(GameObject panel)
        {
            UIFactory.Stretch(panel.GetComponent<RectTransform>());
            UIFactory.Header(panel.transform, "PERSONNEL",
                () => UIManager.Instance.NavigateBack());

            var strip = UIFactory.Panel(panel.transform, "Strip", ColorTheme.RowBg);
            UIFactory.SetAnchored(strip.GetComponent<RectTransform>(),
                new Vector2(0, 0.88f), new Vector2(1, 0.935f), Vector2.zero, Vector2.zero);

            _creditsText = UIFactory.Label(strip.transform, "Credits", "",
                ColorTheme.FontBody, ColorTheme.TextPositive, TextAlignmentOptions.Left);
            UIFactory.Stretch(_creditsText.rectTransform, 12, 12, 4, 4);

            var colHdr = UIFactory.Panel(panel.transform, "ColHdr", ColorTheme.HeaderBg);
            UIFactory.SetAnchored(colHdr.GetComponent<RectTransform>(),
                new Vector2(0, 0.84f), new Vector2(1, 0.88f), Vector2.zero, Vector2.zero);

            string[] hdrs   = { "Name", "P", "F", "T", "E", "Daily", "" };
            float[]  widths = { 0.28f, 0.08f, 0.08f, 0.08f, 0.08f, 0.18f, 0.22f };
            float x = 0;
            foreach (var (h, w) in System.Linq.Enumerable.Zip(hdrs, widths, (a, b) => (a, b)))
            {
                var lbl = UIFactory.Label(colHdr.transform, h, h,
                    ColorTheme.FontSmall, ColorTheme.TextSecondary, TextAlignmentOptions.Center);
                UIFactory.SetAnchored(lbl.rectTransform,
                    new Vector2(x, 0), new Vector2(x + w, 1), new Vector2(2, 2), new Vector2(-2, -2));
                x += w;
            }

            var (scroll, content) = UIFactory.ScrollView(panel.transform, "PersonnelList");
            UIFactory.SetAnchored(scroll.GetComponent<RectTransform>(),
                new Vector2(0, 0.02f), new Vector2(1, 0.84f), Vector2.zero, Vector2.zero);
            _listContent = content;
        }

        public void OnShow() => BuildAndRefresh();

        int _rowCount;

        void BuildAndRefresh()
        {
            foreach (Transform child in _listContent) Destroy(child.gameObject);
            _rowCount = 0;

            var G = GameState.Instance;
            _creditsText.text = UIFactory.Cr(G.Credits);

            var st = GameData.Shiptypes[G.Ship.Type];

            int crewUsed = 0;
            for (int i = 1; i < MaxCrew; i++)
                if (G.Ship.Crew[i] >= 0) crewUsed++;

            bool crewFull = crewUsed >= st.CrewQuarters - 1;

            for (int i = 1; i < MaxCrew; i++)
            {
                if (G.Ship.Crew[i] < 0) continue;
                int slotIdx = i;
                int mercIdx = G.Ship.Crew[i];
                var merc    = G.Mercenary[mercIdx];
                long daily  = (merc.Pilot + merc.Fighter + merc.Trader + merc.Engineer) * 3;
                AddMercRow(merc, mercIdx, daily, true, () => {
                    G.Ship.Crew[slotIdx] = -1;
                    BuildAndRefresh();
                });
            }

            // Available crew at current system
            for (int m = 1; m < MaxCrewMember && m < GameData.MercenaryNames.Length; m++)
            {
                bool onCrew = false;
                for (int s = 0; s < MaxCrew; s++)
                    if (G.Ship.Crew[s] == m) { onCrew = true; break; }
                if (onCrew) continue;

                if (G.Mercenary[m].CurSystem != G.Commander.CurSystem) continue;

                int mercIdx = m;
                var merc    = G.Mercenary[m];
                long daily  = (merc.Pilot + merc.Fighter + merc.Trader + merc.Engineer) * 3;
                AddMercRow(merc, mercIdx, daily, false, () => {
                    if (crewFull) return;
                    for (int s = 1; s < MaxCrew; s++)
                    {
                        if (G.Ship.Crew[s] < 0) { G.Ship.Crew[s] = mercIdx; break; }
                    }
                    BuildAndRefresh();
                }, !crewFull);
            }
        }

        void AddMercRow(CrewMember merc, int mercIdx, long daily, bool hired,
            UnityEngine.Events.UnityAction onClick, bool canHire = true)
        {
            int rowIdx = _rowCount++;
            var row = UIFactory.RowPanel(_listContent, $"Merc{mercIdx}",
                rowIdx % 2 == 0 ? ColorTheme.RowBg : ColorTheme.RowAlt, 72);

            float[] xPos = { 0, 0.28f, 0.36f, 0.44f, 0.52f, 0.60f, 0.78f };
            float[] wArr = { 0.28f, 0.08f, 0.08f, 0.08f, 0.08f, 0.18f, 0.22f };

            string[] vals = {
                GameData.MercenaryNames[mercIdx],
                merc.Pilot.ToString(), merc.Fighter.ToString(),
                merc.Trader.ToString(), merc.Engineer.ToString(),
                UIFactory.Cr(daily), hired ? "FIRE" : "HIRE"
            };

            for (int i = 0; i < 6; i++)
            {
                var lbl = UIFactory.Label(row.transform, $"V{i}", vals[i],
                    ColorTheme.FontSmall,
                    i == 0 ? ColorTheme.TextPrimary : i == 5 ? ColorTheme.TextAccent : ColorTheme.TextSecondary,
                    i < 2 ? TextAlignmentOptions.Left : TextAlignmentOptions.Center);
                UIFactory.SetAnchored(lbl.rectTransform,
                    new Vector2(xPos[i], 0), new Vector2(xPos[i] + wArr[i], 1),
                    new Vector2(4, 4), new Vector2(-4, -4));
            }

            var btn = UIFactory.SmallBtn(row.transform, "Action", vals[6],
                onClick, hired ? ColorTheme.ButtonDanger : ColorTheme.ButtonSuccess);
            UIFactory.SetAnchored(btn.GetComponent<RectTransform>(),
                new Vector2(0.78f, 0.1f), new Vector2(1f, 0.9f), new Vector2(4, 0), new Vector2(-8, 0));
            btn.interactable = hired || canHire;
        }
    }
}
