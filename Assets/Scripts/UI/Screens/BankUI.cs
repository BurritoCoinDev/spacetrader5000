// Space Trader 5000 – Android/Unity Port
// Bank screen: loans, payback, insurance, escape pod.

using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static SpaceTrader.GameConstants;

namespace SpaceTrader.UI.Screens
{
    public class BankUI : MonoBehaviour, IScreenUI
    {
        TextMeshProUGUI _creditsText, _debtText, _maxLoanText;
        TextMeshProUGUI _insuranceText, _podText;
        Button _getLoanBtn, _payBackBtn, _insuranceBtn, _podBtn;

        public void Initialize(GameObject panel)
        {
            UIFactory.Stretch(panel.GetComponent<RectTransform>());
            UIFactory.Header(panel.transform, "BANK",
                () => UIManager.Instance.NavigateBack());

            // Credits / debt strip
            var strip = UIFactory.Panel(panel.transform, "Strip", ColorTheme.RowBg);
            UIFactory.SetAnchored(strip.GetComponent<RectTransform>(),
                new Vector2(0, 0.88f), new Vector2(1, 0.935f), Vector2.zero, Vector2.zero);

            _creditsText = UIFactory.Label(strip.transform, "Credits", "",
                ColorTheme.FontBody, ColorTheme.TextPositive, TextAlignmentOptions.Left);
            UIFactory.Stretch(_creditsText.rectTransform, 12, 12, 4, 4);

            _debtText = UIFactory.Label(strip.transform, "Debt", "",
                ColorTheme.FontBody, ColorTheme.TextNegative, TextAlignmentOptions.Right);
            UIFactory.Stretch(_debtText.rectTransform, 12, 12, 4, 4);

            // Loan section (72–56%)
            BuildLoanSection(panel.transform);

            // Insurance section (54–40%)
            BuildInsuranceSection(panel.transform);

            // Escape pod section (38–24%)
            BuildPodSection(panel.transform);
        }

        void BuildLoanSection(Transform parent)
        {
            var sec = UIFactory.Panel(parent, "LoanSec", ColorTheme.PanelBg);
            UIFactory.SetAnchored(sec.GetComponent<RectTransform>(),
                new Vector2(0.02f, 0.60f), new Vector2(0.98f, 0.87f),
                new Vector2(0, 4), new Vector2(0, -4));

            var hdr = UIFactory.Label(sec.transform, "Hdr", "LOANS",
                ColorTheme.FontBody, ColorTheme.TextAccent, TextAlignmentOptions.Left);
            UIFactory.SetAnchored(hdr.rectTransform,
                new Vector2(0, 0.78f), Vector2.one, new Vector2(8, 0), new Vector2(-8, 0));

            _maxLoanText = UIFactory.Label(sec.transform, "MaxLoan", "",
                ColorTheme.FontSmall, ColorTheme.TextSecondary, TextAlignmentOptions.Left);
            UIFactory.SetAnchored(_maxLoanText.rectTransform,
                new Vector2(0, 0.44f), new Vector2(1, 0.76f), new Vector2(8, 0), new Vector2(-8, 0));

            _getLoanBtn = UIFactory.Btn(sec.transform, "GetLoan", "GET MAX LOAN",
                OnGetLoan, ColorTheme.ButtonSuccess);
            UIFactory.SetAnchored(_getLoanBtn.GetComponent<RectTransform>(),
                new Vector2(0, 0.04f), new Vector2(0.48f, 0.42f), new Vector2(4, 4), new Vector2(-4, -4));

            _payBackBtn = UIFactory.Btn(sec.transform, "PayBack", "PAY BACK ALL",
                OnPayBack, ColorTheme.ButtonNormal);
            UIFactory.SetAnchored(_payBackBtn.GetComponent<RectTransform>(),
                new Vector2(0.52f, 0.04f), new Vector2(1f, 0.42f), new Vector2(4, 4), new Vector2(-4, -4));
        }

        void BuildInsuranceSection(Transform parent)
        {
            var sec = UIFactory.Panel(parent, "InsSec", ColorTheme.PanelBg);
            UIFactory.SetAnchored(sec.GetComponent<RectTransform>(),
                new Vector2(0.02f, 0.36f), new Vector2(0.98f, 0.58f),
                new Vector2(0, 4), new Vector2(0, -4));

            var hdr = UIFactory.Label(sec.transform, "Hdr", "INSURANCE",
                ColorTheme.FontBody, ColorTheme.TextAccent, TextAlignmentOptions.Left);
            UIFactory.SetAnchored(hdr.rectTransform,
                new Vector2(0, 0.70f), Vector2.one, new Vector2(8, 0), new Vector2(-8, 0));

            _insuranceText = UIFactory.Label(sec.transform, "InsInfo", "",
                ColorTheme.FontSmall, ColorTheme.TextSecondary, TextAlignmentOptions.Left);
            UIFactory.SetAnchored(_insuranceText.rectTransform,
                new Vector2(0, 0.36f), new Vector2(0.65f, 0.68f), new Vector2(8, 0), new Vector2(-4, 0));

            _insuranceBtn = UIFactory.Btn(sec.transform, "InsBtn", "",
                OnInsurance, ColorTheme.ButtonSuccess);
            UIFactory.SetAnchored(_insuranceBtn.GetComponent<RectTransform>(),
                new Vector2(0, 0.04f), Vector2.one, new Vector2(4, 4), new Vector2(-4, -4));
        }

        void BuildPodSection(Transform parent)
        {
            var sec = UIFactory.Panel(parent, "PodSec", ColorTheme.PanelBg);
            UIFactory.SetAnchored(sec.GetComponent<RectTransform>(),
                new Vector2(0.02f, 0.12f), new Vector2(0.98f, 0.34f),
                new Vector2(0, 4), new Vector2(0, -4));

            var hdr = UIFactory.Label(sec.transform, "Hdr", "ESCAPE POD",
                ColorTheme.FontBody, ColorTheme.TextAccent, TextAlignmentOptions.Left);
            UIFactory.SetAnchored(hdr.rectTransform,
                new Vector2(0, 0.70f), Vector2.one, new Vector2(8, 0), new Vector2(-8, 0));

            _podText = UIFactory.Label(sec.transform, "PodInfo",
                "Escape pod (2,000 cr): survive ship destruction.",
                ColorTheme.FontSmall, ColorTheme.TextSecondary, TextAlignmentOptions.Left);
            UIFactory.SetAnchored(_podText.rectTransform,
                new Vector2(0, 0.36f), new Vector2(1, 0.68f), new Vector2(8, 0), new Vector2(-8, 0));

            _podBtn = UIFactory.Btn(sec.transform, "PodBtn", "BUY ESCAPE POD",
                OnBuyPod, ColorTheme.ButtonSuccess);
            UIFactory.SetAnchored(_podBtn.GetComponent<RectTransform>(),
                new Vector2(0, 0.04f), Vector2.one, new Vector2(4, 4), new Vector2(-4, -4));
        }

        public void OnShow() => Refresh();

        void Refresh()
        {
            var G = GameState.Instance;
            _creditsText.text = UIFactory.Cr(G.Credits);
            _debtText.text    = G.Debt > 0 ? $"Debt: {UIFactory.Cr(G.Debt)}" : "No debt";

            long maxLoan = BankSystem.MaxLoan();
            _maxLoanText.text = $"Max loan: {UIFactory.Cr(maxLoan)}  (10% daily interest)";
            _getLoanBtn.interactable = maxLoan > 0;
            _payBackBtn.interactable = G.Debt > 0 && G.Credits > 0;

            long insDaily = MoneySystem.InsuranceMoney();
            if (G.Insurance)
            {
                _insuranceText.text = $"Active — {UIFactory.Cr(insDaily)}/day";
                var btnLbl = _insuranceBtn.GetComponentInChildren<TextMeshProUGUI>();
                if (btnLbl != null) btnLbl.text = "CANCEL INSURANCE";
                _insuranceBtn.interactable = true;
                var col = _insuranceBtn.colors;
                col.normalColor = ColorTheme.ButtonDanger;
                _insuranceBtn.colors = col;
            }
            else
            {
                bool can = BankSystem.CanGetInsurance();
                _insuranceText.text = can
                    ? $"Available — {UIFactory.Cr(insDaily)}/day"
                    : "Requires escape pod; only after clean claim.";
                var btnLbl = _insuranceBtn.GetComponentInChildren<TextMeshProUGUI>();
                if (btnLbl != null) btnLbl.text = "BUY INSURANCE";
                _insuranceBtn.interactable = can;
                var col = _insuranceBtn.colors;
                col.normalColor = ColorTheme.ButtonSuccess;
                _insuranceBtn.colors = col;
            }

            if (G.EscapePod)
            {
                _podText.text = "Escape pod: INSTALLED";
                _podBtn.interactable = false;
            }
            else
            {
                _podText.text = "Escape pod (2,000 cr): survive ship destruction.";
                _podBtn.interactable = BankSystem.CanBuyEscapePod();
            }
        }

        void OnGetLoan()
        {
            BankSystem.GetLoan(BankSystem.MaxLoan());
            Refresh();
        }

        void OnPayBack()
        {
            BankSystem.PayBack(GameState.Instance.Debt);
            Refresh();
        }

        void OnInsurance()
        {
            if (GameState.Instance.Insurance)
                BankSystem.CancelInsurance();
            else
                BankSystem.BuyInsurance();
            Refresh();
        }

        void OnBuyPod()
        {
            BankSystem.BuyEscapePod();
            Refresh();
        }
    }
}
