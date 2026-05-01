// Space Trader 5000 - Android/Unity Port
// Ported from Space Trader 1.2.2 by Pieter Spronck (GPL)
// Original: Fuel.c

using static SpaceTrader.GameConstants;

namespace SpaceTrader
{
    public static class FuelSystem
    {
        static GameState G => GameState.Instance;

        public static int GetFuelTanks()
        {
            var ship = G.Ship;
            int tanks = GameData.Shiptypes[ship.Type].FuelTanks;
            if (SkillSystem.HasGadget(ship, FuelCompactor)) tanks += MaxRange;
            return tanks;
        }

        public static int GetFuel() => G.Ship.Fuel;

        public static void BuyFuel(int amount)
        {
            int tanks   = GetFuelTanks();
            int costFor = GameData.Shiptypes[G.Ship.Type].CostOfFuel;
            int add     = GameMath.Min(amount, tanks - G.Ship.Fuel);
            if (costFor > 0)
                add = GameMath.Min(add, (int)(G.Credits / costFor));
            if (add <= 0) return;
            G.Ship.Fuel += add;
            G.Credits   -= add * costFor;
        }

        public static int FuelCostFor(int amount)
        {
            int add = GameMath.Min(amount, GetFuelTanks() - G.Ship.Fuel);
            return add * GameData.Shiptypes[G.Ship.Type].CostOfFuel;
        }
    }
}
