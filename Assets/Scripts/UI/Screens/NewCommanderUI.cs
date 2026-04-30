// Space Trader 5000 – Android/Unity Port
// New Commander setup screen: name, difficulty, and skill allocation.

using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SpaceTrader.Persistence;
using static SpaceTrader.GameConstants;

namespace SpaceTrader.UI.Screens
{
    public class NewCommanderUI : MonoBehaviour, IScreenUI
    {
        TMP_InputField _nameInput;
        TextMeshProUGUI _diffLabel, _remainLabel;
        TextMeshProUGUI[] _skillLabels = new TextMeshProUGUI[4];
        int[] _skills = new int[4]; // pilot, fighter, trader, engineer
        int _difficulty = Normal;
        const int TotalPoints = 16; // 2×MaxSkill - 4 base

        static readonly string[] SkillNames = { "Pilot", "Fighter", "Trader", "Engineer" };

        public void Initialize(GameObject panel)
        {
            UIFactory.Stretch(panel.GetComponent<RectTransform>());
            UIFactory.Header(panel.transform, "NEW COMMANDER", null);

            // Name row
            var nameLabel = UIFactory.Label(panel.transform, "NameLbl",
                "Commander Name:", ColorTheme.FontBody, ColorTheme.TextSecondary);
            UIFactory.SetAnchored(nameLabel.rectTransform,
                new Vector2(0.05f, 0.83f), new Vector2(0.50f, 0.88f), Vector2.zero, Vector2.zero);

            _nameInput = UIFactory.InputField(panel.transform, "NameInput", "Jameson");
            UIFactory.SetAnchored(_nameInput.GetComponent<RectTransform>(),
                new Vector2(0.05f, 0.76f), new Vector2(0.95f, 0.83f), Vector2.zero, Vector2.zero);

            // Difficulty row
            var diffRow = UIFactory.TransparentPanel(panel.transform, "DiffRow");
            UIFactory.SetAnchored(diffRow.GetComponent<RectTransform>(),
                new Vector2(0.05f, 0.68f), new Vector2(0.95f, 0.75f), Vector2.zero, Vector2.zero);

            var dLbl = UIFactory.Label(diffRow.transform, "DiffLbl",
                "Difficulty:", ColorTheme.FontBody, ColorTheme.TextSecondary);
            UIFactory.Pin(dLbl.rectTransform, TextAnchor.MiddleLeft, 200, 50);

            _diffLabel = UIFactory.Label(diffRow.transform, "DiffVal",
                GameData.DifficultyLevel[_difficulty],
                ColorTheme.FontBody, ColorTheme.TextAccent);
            UIFactory.Pin(_diffLabel.rectTransform, TextAnchor.MiddleCenter, 200, 50);

            var decDiff = UIFactory.SmallBtn(diffRow.transform, "DiffDec", "◀",
                () => ChangeDifficulty(-1));
            UIFactory.Pin(decDiff.GetComponent<RectTransform>(), TextAnchor.MiddleRight, 80, 60, -90, 0);

            var incDiff = UIFactory.SmallBtn(diffRow.transform, "DiffInc", "▶",
                () => ChangeDifficulty(1));
            UIFactory.Pin(incDiff.GetComponent<RectTransform>(), TextAnchor.MiddleRight, 80, 60, 0, 0);

            // Skill allocation
            var skillHeader = UIFactory.Label(panel.transform, "SkillHdr",
                "SKILL POINTS", ColorTheme.FontBody, ColorTheme.TextAccent,
                TextAlignmentOptions.Center);
            UIFactory.SetAnchored(skillHeader.rectTransform,
                new Vector2(0.05f, 0.61f), new Vector2(0.95f, 0.67f), Vector2.zero, Vector2.zero);

            _remainLabel = UIFactory.Label(panel.transform, "RemainLbl",
                $"Remaining: {TotalPoints}",
                ColorTheme.FontBody, ColorTheme.TextWarning, TextAlignmentOptions.Center);
            UIFactory.SetAnchored(_remainLabel.rectTransform,
                new Vector2(0.05f, 0.55f), new Vector2(0.95f, 0.61f), Vector2.zero, Vector2.zero);

            for (int i = 0; i < 4; i++)
            {
                int idx = i;
                float y1 = 0.47f - i * 0.09f;
                float y2 = y1 + 0.08f;

                var row = UIFactory.TransparentPanel(panel.transform, $"Skill{i}Row");
                UIFactory.SetAnchored(row.GetComponent<RectTransform>(),
                    new Vector2(0.05f, y1), new Vector2(0.95f, y2), Vector2.zero, Vector2.zero);

                var sLbl = UIFactory.Label(row.transform, "Name", SkillNames[i],
                    ColorTheme.FontBody, ColorTheme.TextPrimary);
                UIFactory.Pin(sLbl.rectTransform, TextAnchor.MiddleLeft, 200, 60);

                var decBtn = UIFactory.SmallBtn(row.transform, "Dec", "−",
                    () => ChangeSkill(idx, -1));
                UIFactory.Pin(decBtn.GetComponent<RectTransform>(), TextAnchor.MiddleRight, 70, 60, -160, 0);

                _skillLabels[i] = UIFactory.Label(row.transform, "Val", "1",
                    ColorTheme.FontHeader, ColorTheme.TextAccent, TextAlignmentOptions.Center);
                UIFactory.Pin(_skillLabels[i].rectTransform, TextAnchor.MiddleRight, 70, 60, -90, 0);

                var incBtn = UIFactory.SmallBtn(row.transform, "Inc", "+",
                    () => ChangeSkill(idx, 1));
                UIFactory.Pin(incBtn.GetComponent<RectTransform>(), TextAnchor.MiddleRight, 70, 60, -20, 0);
            }

            // Start button
            var startBtn = UIFactory.Btn(panel.transform, "StartBtn",
                "BEGIN ADVENTURE",
                OnStart, ColorTheme.ButtonSuccess, ColorTheme.FontButton);
            UIFactory.SetAnchored(startBtn.GetComponent<RectTransform>(),
                new Vector2(0.1f, 0.04f), new Vector2(0.9f, 0.14f), Vector2.zero, Vector2.zero);
        }

        public void OnShow()
        {
            _skills = new[] { 1, 1, 1, 1 };
            RefreshSkills();
        }

        void ChangeDifficulty(int delta)
        {
            _difficulty = Mathf.Clamp(_difficulty + delta, Beginner, Impossible);
            _diffLabel.text = GameData.DifficultyLevel[_difficulty];
        }

        void ChangeSkill(int idx, int delta)
        {
            int remaining = TotalPoints - (_skills[0] + _skills[1] + _skills[2] + _skills[3] - 4);
            if (delta > 0 && remaining <= 0) return;
            if (delta < 0 && _skills[idx] <= 1) return;
            _skills[idx] = Mathf.Clamp(_skills[idx] + delta, 1, MaxSkill);
            RefreshSkills();
        }

        void RefreshSkills()
        {
            int used = _skills[0] + _skills[1] + _skills[2] + _skills[3] - 4;
            int rem  = TotalPoints - used;
            _remainLabel.text  = $"Remaining: {rem}";
            _remainLabel.color = rem == 0 ? ColorTheme.TextPositive : ColorTheme.TextWarning;
            for (int i = 0; i < 4; i++) _skillLabels[i].text = _skills[i].ToString();
        }

        void OnStart()
        {
            string name = _nameInput.text.Trim();
            if (string.IsNullOrEmpty(name)) name = "Jameson";

            TravelerSystem.StartNewGame();
            var G = GameState.Instance;
            G.NameCommander           = name;
            G.Difficulty              = _difficulty;
            G.Commander.Pilot         = _skills[0];
            G.Commander.Fighter       = _skills[1];
            G.Commander.Trader        = _skills[2];
            G.Commander.Engineer      = _skills[3];
            G.Commander.NameIndex     = 0;
            TravelerSystem.DeterminePrices(G.Commander.CurSystem);
            SaveSystem.Save();
            UIManager.Instance.Navigate(GameScreen.Docked);
        }
    }
}
