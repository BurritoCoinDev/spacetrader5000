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
            int tanks = GetFuelTanks();
            int add   = GameMath.Min(amount, tanks - G.Ship.Fuel);
            int cost  = add * GameData.Shiptypes[G.Ship.Type].CostOfFuel;
            G.Ship.Fuel += add;
            G.Credits   -= cost;
        }

        public static int FuelCostFor(int amount)
        {
            int add = GameMath.Min(amount, GetFuelTanks() - G.Ship.Fuel);
            return add * GameData.Shiptypes[G.Ship.Type].CostOfFuel;
        }
    }
}
