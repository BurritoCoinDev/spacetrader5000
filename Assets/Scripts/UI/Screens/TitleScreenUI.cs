// Space Trader 5000 – Android/Unity Port
// Title screen: New Game / Continue / High Scores.

using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SpaceTrader.Persistence;

namespace SpaceTrader.UI.Screens
{
    public class TitleScreenUI : MonoBehaviour, IScreenUI
    {
        Button _continueBtn;
        TextMeshProUGUI _versionLabel;

        public void Initialize(GameObject panel)
        {
            var rt = panel.GetComponent<RectTransform>();
            UIFactory.Stretch(rt);

            // Title text
            var title = UIFactory.LabelWrap(panel.transform, "Title",
                "SPACE\nTRADER\n5000",
                ColorTheme.FontTitle, ColorTheme.TextAccent,
                TextAlignmentOptions.Center);
            UIFactory.SetAnchored(title.rectTransform,
                new Vector2(0.1f, 0.55f), new Vector2(0.9f, 0.95f),
                Vector2.zero, Vector2.zero);

            // Subtitle
            var sub = UIFactory.Label(panel.transform, "Subtitle",
                "Originally by Pieter Spronck",
                ColorTheme.FontSmall, ColorTheme.TextSecondary,
                TextAlignmentOptions.Center);
            UIFactory.SetAnchored(sub.rectTransform,
                new Vector2(0.05f, 0.51f), new Vector2(0.95f, 0.56f),
                Vector2.zero, Vector2.zero);

            // New Game button
            var newBtn = UIFactory.Btn(panel.transform, "NewGameBtn", "NEW GAME",
                OnNewGame, ColorTheme.ButtonSuccess, ColorTheme.FontButton);
            UIFactory.SetAnchored(newBtn.GetComponent<RectTransform>(),
                new Vector2(0.1f, 0.38f), new Vector2(0.9f, 0.49f),
                Vector2.zero, Vector2.zero);

            // Continue button
            _continueBtn = UIFactory.Btn(panel.transform, "ContinueBtn", "CONTINUE",
                OnContinue, ColorTheme.ButtonNormal, ColorTheme.FontButton);
            UIFactory.SetAnchored(_continueBtn.GetComponent<RectTransform>(),
                new Vector2(0.1f, 0.25f), new Vector2(0.9f, 0.36f),
                Vector2.zero, Vector2.zero);

            // High Scores button
            var hsBtn = UIFactory.Btn(panel.transform, "HighScoresBtn", "HIGH SCORES",
                OnHighScores, ColorTheme.ButtonNormal, ColorTheme.FontButton);
            UIFactory.SetAnchored(hsBtn.GetComponent<RectTransform>(),
                new Vector2(0.1f, 0.12f), new Vector2(0.9f, 0.23f),
                Vector2.zero, Vector2.zero);

            // Version label
            _versionLabel = UIFactory.Label(panel.transform, "Version",
                "v1.2.2 Unity Port",
                ColorTheme.FontTiny, ColorTheme.TextDisabled,
                TextAlignmentOptions.Center);
            UIFactory.SetAnchored(_versionLabel.rectTransform,
                new Vector2(0.05f, 0.02f), new Vector2(0.95f, 0.08f),
                Vector2.zero, Vector2.zero);
        }

        public void OnShow()
        {
            _continueBtn.interactable = SaveSystem.SaveExists();
            var colors = _continueBtn.colors;
            colors.normalColor = SaveSystem.SaveExists()
                ? Color.white : ColorTheme.TextDisabled;
            _continueBtn.colors = colors;
        }

        void OnNewGame()
        {
            SaveSystem.DeleteSave();
            UIManager.Instance.Navigate(GameScreen.NewCommander);
        }

        void OnContinue()
        {
            if (!SaveSystem.SaveExists()) return;
            SaveSystem.Load();
            UIManager.Instance.Navigate(GameScreen.Docked);
        }

        void OnHighScores()
        {
            UIManager.Instance.Navigate(GameScreen.HighScores);
        }
    }
}
