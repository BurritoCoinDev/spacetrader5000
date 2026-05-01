// Space Trader 5000 – Android/Unity Port
// High Scores screen: top 3 scores.

using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static SpaceTrader.GameConstants;

namespace SpaceTrader.UI.Screens
{
    public class HighScoresUI : MonoBehaviour, IScreenUI
    {
        Transform _listContent;

        public void Initialize(GameObject panel)
        {
            UIFactory.Stretch(panel.GetComponent<RectTransform>());
            UIFactory.Header(panel.transform, "HIGH SCORES",
                () => UIManager.Instance.NavigateBack());

            var colHdr = UIFactory.Panel(panel.transform, "ColHdr", ColorTheme.HeaderBg);
            UIFactory.SetAnchored(colHdr.GetComponent<RectTransform>(),
                new Vector2(0, 0.84f), new Vector2(1, 0.88f), Vector2.zero, Vector2.zero);

            string[] hdrs   = { "#", "Name", "Score", "Worth", "Days", "Diff" };
            float[]  widths = { 0.06f, 0.26f, 0.18f, 0.18f, 0.14f, 0.18f };
            float x = 0;
            foreach (var (h, w) in System.Linq.Enumerable.Zip(hdrs, widths, (a, b) => (a, b)))
            {
                var lbl = UIFactory.Label(colHdr.transform, h, h,
                    ColorTheme.FontSmall, ColorTheme.TextSecondary, TextAlignmentOptions.Left);
                UIFactory.SetAnchored(lbl.rectTransform,
                    new Vector2(x, 0), new Vector2(x + w, 1), new Vector2(4, 2), new Vector2(-4, -2));
                x += w;
            }

            var (scroll, content) = UIFactory.ScrollView(panel.transform, "ScoreList");
            UIFactory.SetAnchored(scroll.GetComponent<RectTransform>(),
                new Vector2(0, 0.02f), new Vector2(1, 0.84f), Vector2.zero, Vector2.zero);
            _listContent = content;
        }

        public void OnShow()
        {
            foreach (Transform child in _listContent) Destroy(child.gameObject);

            var G = GameState.Instance;
            bool any = false;

            for (int i = 0; i < MaxHighScore; i++)
            {
                var hs = G.Hscores[i];
                if (string.IsNullOrEmpty(hs.Name)) continue;
                any = true;

                long score = TravelerSystem.GetScore(hs.Status, hs.Days, hs.Worth, hs.Difficulty);

                var row = UIFactory.RowPanel(_listContent, $"Score{i}",
                    i % 2 == 0 ? ColorTheme.RowBg : ColorTheme.RowAlt, 80);

                string[] vals   = { $"{i + 1}.", hs.Name, score.ToString("N0"), UIFactory.Cr(hs.Worth), hs.Days.ToString(), GameData.DifficultyLevel[hs.Difficulty] };
                float[]  xPos   = { 0,   0.06f, 0.32f, 0.50f, 0.68f, 0.82f };
                float[]  widths = { 0.06f, 0.26f, 0.18f, 0.18f, 0.14f, 0.18f };

                for (int j = 0; j < vals.Length; j++)
                {
                    var lbl = UIFactory.Label(row.transform, $"V{j}", vals[j],
                        ColorTheme.FontSmall,
                        j == 0 ? ColorTheme.TextAccent : j == 2 ? ColorTheme.TextPositive : ColorTheme.TextPrimary);
                    UIFactory.SetAnchored(lbl.rectTransform,
                        new Vector2(xPos[j], 0), new Vector2(xPos[j] + widths[j], 1),
                        new Vector2(4, 4), new Vector2(-4, -4));
                }
            }

            if (!any)
            {
                var none = UIFactory.Label(_listContent, "None",
                    "No scores recorded yet.",
                    ColorTheme.FontBody, ColorTheme.TextDisabled, TextAlignmentOptions.Center);
                none.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 80);
                none.gameObject.AddComponent<LayoutElement>().preferredHeight = 80;
            }
        }
    }
}
