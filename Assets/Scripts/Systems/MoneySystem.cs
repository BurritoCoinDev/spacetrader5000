// Space Trader 5000 - Android/Unity Port
// Ported from Space Trader 1.2.2 by Pieter Spronck (GPL)
// Original: Money.c

using static SpaceTrader.GameConstants;

namespace SpaceTrader
{
    public static class MoneySystem
    {
        static GameState G => GameState.Instance;

        public static long InsuranceMoney()
        {
            if (!G.Insurance) return 0;
            // NoClaim discount caps at 90% of the base premium, applied per
            // day insured. Without it premiums never get cheaper over time.
            long basePrice = ShipPriceSystem.CurrentShipPriceWithoutCargo(true);
            long discount  = 100 - GameMath.Min(G.NoClaim, 90);
            long premium   = (basePrice * 5 / 2000) * discount / 100;
            return GameMath.Max(1L, premium);
        }

        public static long MercenaryMoney()
        {
            long total = 0;
            var ship = G.Ship;
            for (int i = 1; i < MaxCrew; i++)
            {
                int idx = ship.Crew[i];
                if (idx < 0) continue;
                var m = G.Mercenary[idx];
                total += (m.Pilot + m.Fighter + m.Trader + m.Engineer) * 3;
            }
            return total;
        }

        public static long ToSpend()
        {
            long reserve = G.ReserveMoney ? InsuranceMoney() + MercenaryMoney() : 0;
            return GameMath.Max(0L, G.Credits - reserve);
        }

        // Net worth: credits + ship value (including cargo at purchase price) - debt
        // CurrentShipPrice already includes cargo, so do not add it again.
        public static long CurrentWorth()
        {
            return G.Credits - G.Debt + ShipPriceSystem.CurrentShipPrice(false)
                 + (G.MoonBought ? CostMoon : 0);
        }

        public static void PayInterest()
        {
            if (G.Debt <= 0) return;
            long interest = GameMath.Max(1L, G.Debt / 10);
            // Pay from Credits first; only the shortfall grows Debt.
            if (G.Credits > interest)
            {
                G.Credits -= interest;
            }
            else
            {
                G.Debt   += interest - G.Credits;
                G.Credits = 0;
            }
        }
    }
}
