// Space Trader 5000 - Android/Unity Port
// Ported from Space Trader 1.2.2 by Pieter Spronck (GPL)
// Original: ShipPrice.c

using static SpaceTrader.GameConstants;

namespace SpaceTrader
{
    public static class ShipPriceSystem
    {
        static GameState G => GameState.Instance;

        public static long BasePrice(int techLevel, long price)
            => (price + (techLevel - GameData.Shiptypes[G.Ship.Type].MinTechLevel)) * 90 / 100;

        public static long BaseSellPrice(int itemIndex, long price)
        {
            long sell = price * 3 / 4;
            if (G.PoliceRecordScore < DubiousScore) sell = sell * 90 / 100;
            return sell;
        }

        public static long EnemyShipPrice(Ship sh)
        {
            long price = GameData.Shiptypes[sh.Type].Price;
            for (int i = 0; i < MaxWeapon; i++)
                if (sh.Weapon[i] >= 0) price += GameData.Weapontypes[sh.Weapon[i]].Price;
            for (int i = 0; i < MaxShield; i++)
                if (sh.Shield[i] >= 0) price += GameData.Shieldtypes[sh.Shield[i]].Price;
            for (int i = 0; i < MaxGadget; i++)
                if (sh.Gadget[i] >= 0) price += GameData.Gadgettypes[sh.Gadget[i]].Price;
            return price * 3 / 4;
        }

        public static long CurrentShipPriceWithoutCargo(bool forInsurance)
        {
            var ship = G.Ship;
            long price = GameData.Shiptypes[ship.Type].Price *
                         (100 - SkillSystem.TraderSkill(ship)) / 100;
            if (forInsurance) price = price * 3 / 4;

            for (int i = 0; i < MaxWeapon; i++)
                if (ship.Weapon[i] >= 0)
                    price += forInsurance
                        ? GameData.Weapontypes[ship.Weapon[i]].Price * 3 / 4
                        : GameData.Weapontypes[ship.Weapon[i]].Price;
            for (int i = 0; i < MaxShield; i++)
                if (ship.Shield[i] >= 0)
                    price += forInsurance
                        ? GameData.Shieldtypes[ship.Shield[i]].Price * 3 / 4
                        : GameData.Shieldtypes[ship.Shield[i]].Price;
            for (int i = 0; i < MaxGadget; i++)
                if (ship.Gadget[i] >= 0)
                    price += forInsurance
                        ? GameData.Gadgettypes[ship.Gadget[i]].Price * 3 / 4
                        : GameData.Gadgettypes[ship.Gadget[i]].Price;
            return price;
        }

        public static long CurrentShipPrice(bool forInsurance)
        {
            long price = CurrentShipPriceWithoutCargo(forInsurance);
            for (int i = 0; i < MaxTradeItem; i++)
                price += G.Ship.Cargo[i] * G.BuyingPrice[i];
            return price;
        }
    }
}
