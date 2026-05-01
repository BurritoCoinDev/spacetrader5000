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
            long worth = ShipPriceSystem.CurrentShipPriceWithoutCargo(true);
            long max   = GameMath.Max(0L, worth / 10 - G.Debt);
            return GameMath.Min(max, DebtTooLarge - G.Debt);
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
            => !G.Insurance && G.NoClaim == 0 && G.EscapePod;

        public static void BuyInsurance()
        {
            if (!CanGetInsurance()) return;
            G.Insurance = true;
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
