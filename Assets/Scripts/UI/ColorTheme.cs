// Space Trader 5000 – Android/Unity Port
// Shared color palette and font sizes for all screens.

using UnityEngine;

namespace SpaceTrader.UI
{
    public static class ColorTheme
    {
        // Backgrounds
        public static readonly Color Background      = new Color(0.04f, 0.04f, 0.10f, 1f); // #0A0A1A
        public static readonly Color PanelBg         = new Color(0.06f, 0.08f, 0.15f, 1f); // #0F1426
        public static readonly Color RowBg           = new Color(0.08f, 0.10f, 0.18f, 1f); // #141A2E
        public static readonly Color RowAlt          = new Color(0.06f, 0.08f, 0.14f, 1f);
        public static readonly Color Divider         = new Color(0.15f, 0.20f, 0.35f, 1f);

        // Buttons
        public static readonly Color ButtonNormal    = new Color(0.10f, 0.18f, 0.30f, 1f); // #1A2E4D
        public static readonly Color ButtonHighlight = new Color(0.15f, 0.26f, 0.45f, 1f); // #264273
        public static readonly Color ButtonDanger    = new Color(0.40f, 0.08f, 0.08f, 1f); // #661414
        public static readonly Color ButtonSuccess   = new Color(0.05f, 0.30f, 0.10f, 1f); // #0D4D1A
        public static readonly Color ButtonWarning   = new Color(0.35f, 0.20f, 0.04f, 1f); // #593300
        public static readonly Color ButtonDisabled  = new Color(0.10f, 0.10f, 0.10f, 1f);
        public static readonly Color HeaderBg        = new Color(0.07f, 0.12f, 0.22f, 1f); // #121E38

        // Text
        public static readonly Color TextPrimary     = new Color(0.92f, 0.92f, 0.95f, 1f); // #EAEAF2
        public static readonly Color TextSecondary   = new Color(0.60f, 0.63f, 0.75f, 1f); // #99A0BF
        public static readonly Color TextAccent      = new Color(1.00f, 0.72f, 0.00f, 1f); // #FFB800
        public static readonly Color TextPositive    = new Color(0.20f, 0.85f, 0.40f, 1f); // #33D966
        public static readonly Color TextNegative    = new Color(1.00f, 0.27f, 0.27f, 1f); // #FF4545
        public static readonly Color TextWarning     = new Color(1.00f, 0.55f, 0.00f, 1f); // #FF8C00
        public static readonly Color TextDisabled    = new Color(0.40f, 0.40f, 0.45f, 1f);

        // Progress bars
        public static readonly Color FuelFill        = new Color(0.10f, 0.55f, 0.90f, 1f); // #1A8CE6
        public static readonly Color HullFill        = new Color(0.15f, 0.80f, 0.35f, 1f); // #26CC59
        public static readonly Color HullLow         = new Color(0.90f, 0.25f, 0.10f, 1f); // #E64019
        public static readonly Color BarBackground   = new Color(0.10f, 0.10f, 0.12f, 1f);

        // Font sizes (reference resolution 1080 × 1920)
        public const int FontTitle      = 54;
        public const int FontHeader     = 38;
        public const int FontBody       = 28;
        public const int FontSmall      = 22;
        public const int FontTiny       = 18;
        public const int FontButton     = 30;
        public const int FontButtonSm   = 24;
    }
}
