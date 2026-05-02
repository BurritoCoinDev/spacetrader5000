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
            // FuelCompactor REPLACES the ship's tank capacity with a flat 18,
            // it does not add to it. Without this, large ships (Wasp etc.)
            // reach unintended ranges far beyond what the original allows.
            if (SkillSystem.HasGadget(ship, FuelCompactor)) return 18;
            return GameData.Shiptypes[ship.Type].FuelTanks;
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
