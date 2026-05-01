// Space Trader 5000 – Android/Unity Port
// Commander Status screen: skills, record, reputation, net worth, ship stats.

using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static SpaceTrader.GameConstants;

namespace SpaceTrader.UI.Screens
{
    public class CommanderStatusUI : MonoBehaviour, IScreenUI
    {
        TextMeshProUGUI _bodyText;

        public void Initialize(GameObject panel)
        {
            UIFactory.Stretch(panel.GetComponent<RectTransform>());
            UIFactory.Header(panel.transform, "CMDR STATUS",
                () => UIManager.Instance.NavigateBack());

            var (scroll, content) = UIFactory.ScrollView(panel.transform, "StatusScroll");
            UIFactory.SetAnchored(scroll.GetComponent<RectTransform>(),
                new Vector2(0, 0.02f), new Vector2(1, 0.88f), Vector2.zero, Vector2.zero);

            _bodyText = UIFactory.LabelWrap(content, "Body", "",
                ColorTheme.FontSmall, ColorTheme.TextPrimary, TextAlignmentOptions.Left);
            _bodyText.margin = new Vector4(12, 8, 12, 8);

            // Top-anchored width-stretch — NOT full stretch. Full stretch (anchorMax.y=1)
            // creates a circular dependency with the VLG ContentSizeFitter: content height
            // is 0 so child height is 0 so TMP preferred height is 0 so content stays 0.
            // ContentSizeFitter on this element breaks the cycle: TMP reports its preferred
            // height independently, VLG reads it, content grows to fit.
            var rt = _bodyText.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot     = new Vector2(0.5f, 1);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            var csf = _bodyText.gameObject.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        public void OnShow()
        {
            var G    = GameState.Instance;
            var cmdr = G.Commander;
            var ship = G.Ship;
            var st   = GameData.Shiptypes[ship.Type];

            // Police record name
            string polRec = "Clean";
            foreach (var pr in GameData.PoliceRecords)
                if (G.PoliceRecordScore >= pr.MinScore) polRec = pr.Name;

            // Reputation name
            string repName = "Harmless";
            foreach (var r in GameData.Reputations)
                if (G.ReputationScore >= r.MinScore) repName = r.Name;

            // Effective skills
            int pilot    = SkillSystem.PilotSkill(ship);
            int fighter  = SkillSystem.FighterSkill(ship);
            int trader   = SkillSystem.TraderSkill(ship);
            int engineer = SkillSystem.EngineerSkill(ship);

            // Weapons
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine($"Commander:  {G.NameCommander}");
            sb.AppendLine($"Difficulty: {GameData.DifficultyLevel[G.Difficulty]}");
            sb.AppendLine($"Days:       {G.Days}");
            sb.AppendLine($"Net worth:  {UIFactory.Cr(MoneySystem.CurrentWorth())}");
            sb.AppendLine($"Credits:    {UIFactory.Cr(G.Credits)}");
            if (G.Debt > 0) sb.AppendLine($"Debt:       {UIFactory.Cr(G.Debt)}");
            sb.AppendLine();
            sb.AppendLine($"Police record: {polRec} ({G.PoliceRecordScore})");
            sb.AppendLine($"Reputation:    {repName}");
            sb.AppendLine($"Kills:         {G.ReputationScore}");
            sb.AppendLine();
            sb.AppendLine($"Skills (base / effective):");
            sb.AppendLine($"  Pilot:    {cmdr.Pilot} / {pilot}");
            sb.AppendLine($"  Fighter:  {cmdr.Fighter} / {fighter}");
            sb.AppendLine($"  Trader:   {cmdr.Trader} / {trader}");
            sb.AppendLine($"  Engineer: {cmdr.Engineer} / {engineer}");
            sb.AppendLine();
            sb.AppendLine($"Ship: {st.Name}");
            sb.AppendLine($"  Hull:    {ship.Hull}/{st.HullStrength}");
            sb.AppendLine($"  Fuel:    {ship.Fuel}/{FuelSystem.GetFuelTanks()}");
            sb.AppendLine($"  Cargo:   {CargoSystem.FilledCargoBays()}/{CargoSystem.TotalCargoBays()}");

            // Equipment
            for (int i = 0; i < MaxWeapon; i++)
                if (ship.Weapon[i] >= 0) sb.AppendLine($"  Weapon:  {GameData.Weapontypes[ship.Weapon[i]].Name}");
            for (int i = 0; i < MaxShield; i++)
                if (ship.Shield[i] >= 0) sb.AppendLine($"  Shield:  {GameData.Shieldtypes[ship.Shield[i]].Name} ({ship.ShieldStrength[i]}/{GameData.Shieldtypes[ship.Shield[i]].Power})");
            for (int i = 0; i < MaxGadget; i++)
                if (ship.Gadget[i] >= 0) sb.AppendLine($"  Gadget:  {GameData.Gadgettypes[ship.Gadget[i]].Name}");

            if (G.EscapePod)  sb.AppendLine($"  Escape pod: installed");
            if (G.Insurance)  sb.AppendLine($"  Insurance: active (claim {G.NoClaim} days)");
            if (ship.Tribbles > 0) sb.AppendLine($"  Tribbles: {ship.Tribbles}");

            _bodyText.text = sb.ToString();
        }
    }
}
