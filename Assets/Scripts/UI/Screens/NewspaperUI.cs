// Space Trader 5000 – Android/Unity Port
// Newspaper screen: buy and read the local news.

using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static SpaceTrader.GameConstants;

namespace SpaceTrader.UI.Screens
{
    public class NewspaperUI : MonoBehaviour, IScreenUI
    {
        TextMeshProUGUI _creditsText;
        TextMeshProUGUI _mastheadText;
        Transform _listContent;
        Button _buyBtn;
        TextMeshProUGUI _priceLabel;

        public void Initialize(GameObject panel)
        {
            UIFactory.Stretch(panel.GetComponent<RectTransform>());
            UIFactory.Header(panel.transform, "NEWSPAPER",
                () => UIManager.Instance.NavigateBack());

            // Credits strip
            var strip = UIFactory.Panel(panel.transform, "Strip", ColorTheme.RowBg);
            UIFactory.SetAnchored(strip.GetComponent<RectTransform>(),
                new Vector2(0, 0.88f), new Vector2(1, 0.935f), Vector2.zero, Vector2.zero);

            _creditsText = UIFactory.Label(strip.transform, "Credits", "",
                ColorTheme.FontBody, ColorTheme.TextPositive, TextAlignmentOptions.Left);
            UIFactory.Stretch(_creditsText.rectTransform, 12, 12, 4, 4);

            // Masthead (newspaper title)
            _mastheadText = UIFactory.Label(panel.transform, "Masthead", "",
                ColorTheme.FontHeader, ColorTheme.TextAccent, TextAlignmentOptions.Center);
            UIFactory.SetAnchored(_mastheadText.rectTransform,
                new Vector2(0.02f, 0.80f), new Vector2(0.98f, 0.87f), Vector2.zero, Vector2.zero);

            // Buy button + price
            var buyBar = UIFactory.Panel(panel.transform, "BuyBar", ColorTheme.RowBg);
            UIFactory.SetAnchored(buyBar.GetComponent<RectTransform>(),
                new Vector2(0, 0.74f), new Vector2(1, 0.79f), Vector2.zero, Vector2.zero);

            _priceLabel = UIFactory.Label(buyBar.transform, "Price", "",
                ColorTheme.FontSmall, ColorTheme.TextSecondary, TextAlignmentOptions.Left);
            UIFactory.SetAnchored(_priceLabel.rectTransform,
                new Vector2(0, 0), new Vector2(0.60f, 1), new Vector2(12, 4), new Vector2(-4, -4));

            _buyBtn = UIFactory.SmallBtn(buyBar.transform, "Buy", "BUY PAPER",
                OnBuy, ColorTheme.ButtonSuccess);
            UIFactory.SetAnchored(_buyBtn.GetComponent<RectTransform>(),
                new Vector2(0.60f, 0.05f), new Vector2(1f, 0.95f), new Vector2(4, 0), new Vector2(-8, 0));

            // Scroll area for headlines
            var (scroll, content) = UIFactory.ScrollView(panel.transform, "NewsList");
            UIFactory.SetAnchored(scroll.GetComponent<RectTransform>(),
                new Vector2(0, 0.02f), new Vector2(1, 0.73f), Vector2.zero, Vector2.zero);
            _listContent = content;
        }

        public void OnShow() => Refresh();

        void Refresh()
        {
            var G    = GameState.Instance;
            long price = NewsSystem.NewspaperPrice();

            _creditsText.text = UIFactory.Cr(G.Credits);
            _mastheadText.text = MastheadFor(G.CurrentSystem.Politics);

            _priceLabel.text = G.AlreadyPaidForNewspaper
                ? "Today's edition — already purchased."
                : $"Cost: {UIFactory.Cr(price)}";
            _buyBtn.interactable = !G.AlreadyPaidForNewspaper && G.Credits >= price;

            // Rebuild headlines list
            foreach (Transform child in _listContent) Destroy(child.gameObject);

            if (G.AlreadyPaidForNewspaper)
                ShowHeadlines();
            else
            {
                var row = UIFactory.RowPanel(_listContent, "Prompt", ColorTheme.RowBg, 120);
                var msg = UIFactory.LabelWrap(row.transform, "PromptText",
                    "Purchase this system's newspaper to read today's headlines.",
                    ColorTheme.FontSmall, ColorTheme.TextSecondary, TextAlignmentOptions.Center);
                UIFactory.Stretch(msg.rectTransform, 12, 12, 8, 8);
            }
        }

        void OnBuy()
        {
            var G     = GameState.Instance;
            long price = NewsSystem.NewspaperPrice();
            if (G.Credits < price) return;

            G.Credits -= price;
            G.AlreadyPaidForNewspaper = true;
            Refresh();
        }

        void ShowHeadlines()
        {
            var headlines = NewsSystem.GetHeadlines();
            for (int i = 0; i < headlines.Count; i++)
            {
                // Use fixed height per headline row — 110px fits 2-3 lines at FontSmall
                var row = UIFactory.RowPanel(_listContent, $"Story{i}",
                    i % 2 == 0 ? ColorTheme.RowBg : ColorTheme.RowAlt, 110);

                var lbl = UIFactory.LabelWrap(row.transform, "Text", headlines[i],
                    ColorTheme.FontSmall, i == 0 ? ColorTheme.TextPrimary : ColorTheme.TextSecondary,
                    TextAlignmentOptions.Left);
                UIFactory.Stretch(lbl.rectTransform, 12, 12, 8, 8);
            }
        }

        // Returns a newspaper title appropriate to the current government type.
        static string MastheadFor(int politics)
        {
            string[] mastheads =
            {
                "The Anarchist's Broadsheet",   // 0 Anarchy
                "The Feudal Herald",             // 1 Feudal
                "The Tech Consortium Bulletin",  // 2 Multi-Government
                "The Corporate Dispatch",        // 3 Dictatorship
                "The People's Tribune",          // 4 Communist
                "The Syndicalist Review",        // 5 Confederacy
                "The Free Trader's Journal",     // 6 Democracy
                "The Governor's Gazette",        // 7 Monarchy
                "The Colonial Observer",         // 8 Fascism
                "The Pacifist's Post",           // 9 Pacifism
                "The Utopian Chronicle",         // 10 Socialism
                "The Black Market Beat",         // 11 State of War
                "The Star Theocracy Tribune",    // 12 Theocracy
                "The Rebel Transmitter",         // 13 Neo-feudal
                "The Trade League Courier",      // 14 Moneyed
                "The Capitalist's Weekly",       // 15 Corporate State
                "The Border Post",               // 16 Independence
            };
            return (politics >= 0 && politics < mastheads.Length)
                ? mastheads[politics]
                : "The Interstellar Courier";
        }
    }
}
