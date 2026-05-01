// Space Trader 5000 - Android/Unity Port
// Ported from Space Trader 1.2.2 by Pieter Spronck (GPL)
// Original: Cargo.c

using static SpaceTrader.GameConstants;

namespace SpaceTrader
{
    public static class CargoSystem
    {
        static GameState G => GameState.Instance;

        public static int TotalCargoBays()
        {
            var ship = G.Ship;
            int bays = GameData.Shiptypes[ship.Type].CargoBays;
            if (SkillSystem.HasGadget(ship, ExtraBays)) bays += 5;
            if (G.ReactorStatus > 0 && G.ReactorStatus <= 20) bays -= 5;
            return bays;
        }

        public static int FilledCargoBays()
        {
            int total = 0;
            for (int i = 0; i < MaxTradeItem; i++) total += G.Ship.Cargo[i];
            return total;
        }

        public static int FreeCargoBays() => TotalCargoBays() - FilledCargoBays();

        public static int GetAmountToBuy(int index)
        {
            if (G.BuyPrice[index] <= 0) return 0;
            int byMoney = G.Credits >= G.BuyPrice[index]
                ? (int)(G.Credits / G.BuyPrice[index]) : 0;
            int bySpace  = GameMath.Max(0, FreeCargoBays() - G.LeaveEmpty);
            int byStock  = G.SolarSystem[G.Commander.CurSystem].Qty[index];
            return GameMath.Min(GameMath.Min(byMoney, bySpace), byStock);
        }

        public static int GetAmountToSell(int index) => G.Ship.Cargo[index];

        public static void BuyCargo(int index, int amount)
        {
            amount = GameMath.Min(amount, GetAmountToBuy(index));
            if (amount <= 0) return;

            long totalCost = G.BuyPrice[index] * amount;
            G.Ship.Cargo[index] += amount;
            G.SolarSystem[G.Commander.CurSystem].Qty[index] -= amount;
            G.Credits    -= totalCost;
            G.BuyingPrice[index] = (G.BuyingPrice[index] * (G.Ship.Cargo[index] - amount) +
                                     G.BuyPrice[index] * amount) /
                                    G.Ship.Cargo[index];
        }

        // The SellCargo method shadows the GameConstants.SellCargo constant in
        // this scope, so the operation constants must be fully qualified.
        public static void SellCargo(int index, int amount, int operation)
        {
            if (amount <= 0 || amount > G.Ship.Cargo[index]) return;

            if (operation == GameConstants.SellCargo || operation == GameConstants.DumpCargo)
            {
                long revenue = operation == GameConstants.SellCargo ? G.SellPrice[index] * amount : 0;
                G.Credits += revenue;
                if (operation == GameConstants.SellCargo)
                    G.SolarSystem[G.Commander.CurSystem].Qty[index] += amount;
            }
            G.Ship.Cargo[index] -= amount;
            if (G.Ship.Cargo[index] == 0) G.BuyingPrice[index] = 0;
        }

        public static void PlunderCargo(int index, int amount, Ship opponent)
        {
            amount = GameMath.Min(amount, GameMath.Min(opponent.Cargo[index], FreeCargoBays()));
            if (amount <= 0) return;
            opponent.Cargo[index] -= amount;
            G.Ship.Cargo[index]   += amount;
        }

        public static long BasePrice(int techLevel, long price)
            => (price + (techLevel - GameData.Shiptypes[G.Ship.Type].MinTechLevel)) * 90 / 100;

        public static bool HasTradeableItems(Ship sh, int systemIdx, int operation)
        {
            var sys = G.SolarSystem[systemIdx];
            for (int i = 0; i < MaxTradeItem; i++)
            {
                if (operation == GameConstants.SellCargo && sh.Cargo[i] > 0 && G.SellPrice[i] > 0) return true;
                if (operation == GameConstants.DumpCargo && sh.Cargo[i] > 0) return true;
                if (operation == GameConstants.JettisonCargo && sh.Cargo[i] > 0) return true;
            }
            return false;
        }
    }
}
