// Space Trader 5000 - Android/Unity Port
// Ported from Space Trader 1.2.2 by Pieter Spronck (GPL)
// Original: Bank.c

using static SpaceTrader.GameConstants;

namespace SpaceTrader
{
    public static class BankSystem
    {
        static GameState G => GameState.Instance;

        public static long MaxLoan()
        {
            // Criminals are capped at 500 cr; clean records get net worth /
            // 10 rounded down to the nearest 500, floored at 1000 and capped
            // at 25000 — matches original Bank.c.
            if (G.PoliceRecordScore < CleanScore) return 500L;
            long raw = MoneySystem.CurrentWorth() / 10L;
            raw = (raw / 500L) * 500L;
            return GameMath.Min(25000L, GameMath.Max(1000L, raw));
        }

        public static void GetLoan(long amount)
        {
            long loan = GameMath.Min(amount, MaxLoan());
            G.Credits += loan;
            G.Debt    += loan;
        }

        public static void PayBack(long amount)
        {
            long pay  = GameMath.Min(amount, GameMath.Min(G.Credits, G.Debt));
            G.Credits -= pay;
            G.Debt    -= pay;
        }

        public static bool CanGetInsurance()
            => !G.Insurance && G.EscapePod;

        public static void BuyInsurance()
        {
            if (!CanGetInsurance()) return;
            G.Insurance = true;
            G.NoClaim   = 0;       // restart the no-claim counter on (re)purchase
        }

        public static void CancelInsurance()
        {
            G.Insurance = false;
            G.NoClaim   = 0;
        }

        public static bool CanBuyEscapePod()
            => !G.EscapePod && G.Credits >= 2000;

        public static void BuyEscapePod()
        {
            if (!CanBuyEscapePod()) return;
            G.EscapePod  = true;
            G.Credits   -= 2000;
        }
    }
}
