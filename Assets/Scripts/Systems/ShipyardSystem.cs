// Space Trader 5000 - Android/Unity Port
// Ported from Space Trader 1.2.2 by Pieter Spronck (GPL)
// Original: Shipyard.c

using static SpaceTrader.GameConstants;

namespace SpaceTrader
{
    public static class ShipyardSystem
    {
        static GameState G => GameState.Instance;

        public static long GetHullStrength()
        {
            long base_ = GameData.Shiptypes[G.Ship.Type].HullStrength;
            return G.HullUpgraded ? base_ + UpgradedHull : base_;
        }

        public static long RepairCostFor(int amount)
        {
            int repair = GameMath.Min(amount, (int)(GetHullStrength() - G.Ship.Hull));
            return repair * GameData.Shiptypes[G.Ship.Type].RepairCosts;
        }

        public static void BuyRepairs(int amount)
        {
            int max    = (int)(GetHullStrength() - G.Ship.Hull);
            int repair = GameMath.Min(amount, max);
            long cost  = repair * GameData.Shiptypes[G.Ship.Type].RepairCosts;
            cost       = GameMath.Min(cost, G.Credits);
            repair     = (int)(cost / GameData.Shiptypes[G.Ship.Type].RepairCosts);
            G.Ship.Hull  += repair;
            G.Credits    -= cost;
        }

        public static int GetFirstEmptySlot(int slotCount, int[] slots)
        {
            for (int i = 0; i < slotCount; i++)
                if (slots[i] < 0) return i;
            return -1;
        }

        public static bool InstallWeapon(int weaponType)
        {
            var ship  = G.Ship;
            var stype = GameData.Shiptypes[ship.Type];
            int slot  = GetFirstEmptySlot(stype.WeaponSlots, ship.Weapon);
            if (slot < 0) return false;
            long cost = GameData.Weapontypes[weaponType].Price;
            if (G.Credits < cost) return false;
            ship.Weapon[slot] = weaponType;
            G.Credits -= cost;
            return true;
        }

        public static bool RemoveWeapon(int slotIndex)
        {
            var ship = G.Ship;
            if (ship.Weapon[slotIndex] < 0) return false;
            long sell     = GameData.Weapontypes[ship.Weapon[slotIndex]].Price * 3 / 4;
            G.Credits    += sell;
            ship.Weapon[slotIndex] = -1;
            return true;
        }

        public static bool InstallShield(int shieldType)
        {
            var ship  = G.Ship;
            var stype = GameData.Shiptypes[ship.Type];
            int slot  = GetFirstEmptySlot(stype.ShieldSlots, ship.Shield);
            if (slot < 0) return false;
            long cost = GameData.Shieldtypes[shieldType].Price;
            if (G.Credits < cost) return false;
            ship.Shield[slot]         = shieldType;
            ship.ShieldStrength[slot] = GameData.Shieldtypes[shieldType].Power;
            G.Credits -= cost;
            return true;
        }

        public static bool RemoveShield(int slotIndex)
        {
            var ship = G.Ship;
            if (ship.Shield[slotIndex] < 0) return false;
            long sell     = GameData.Shieldtypes[ship.Shield[slotIndex]].Price * 3 / 4;
            G.Credits    += sell;
            ship.Shield[slotIndex]         = -1;
            ship.ShieldStrength[slotIndex] = 0;
            return true;
        }

        public static bool InstallGadget(int gadgetType)
        {
            var ship  = G.Ship;
            var stype = GameData.Shiptypes[ship.Type];
            // Reject duplicates — original Shipyard.c blocks two of the same
            // gadget type. Otherwise FuelCompactor / AutoRepair stack and
            // multiply their effects.
            for (int i = 0; i < MaxGadget; i++)
                if (ship.Gadget[i] == gadgetType) return false;
            int slot  = GetFirstEmptySlot(stype.GadgetSlots, ship.Gadget);
            if (slot < 0) return false;
            long cost = GameData.Gadgettypes[gadgetType].Price;
            if (G.Credits < cost) return false;
            ship.Gadget[slot] = gadgetType;
            G.Credits -= cost;
            return true;
        }

        public static bool RemoveGadget(int slotIndex)
        {
            var ship = G.Ship;
            if (ship.Gadget[slotIndex] < 0) return false;
            long sell     = GameData.Gadgettypes[ship.Gadget[slotIndex]].Price * 3 / 4;
            G.Credits    += sell;
            ship.Gadget[slotIndex] = -1;
            return true;
        }

        // Transfer all cargo and equipment to a new ship.
        public static void CreateShip(int shipType)
        {
            var newShip = new Ship
            {
                Type = shipType,
                Fuel = GameData.Shiptypes[shipType].FuelTanks,
                Hull = GameData.Shiptypes[shipType].HullStrength,
            };
            var stype = GameData.Shiptypes[shipType];

            // Transfer weapons (up to new ship's slots)
            for (int i = 0; i < MaxWeapon; i++) newShip.Weapon[i]  = -1;
            for (int i = 0; i < MaxShield; i++) { newShip.Shield[i] = -1; newShip.ShieldStrength[i] = 0; }
            for (int i = 0; i < MaxGadget; i++) newShip.Gadget[i]  = -1;
            for (int i = 0; i < MaxCrew; i++)   newShip.Crew[i]    = -1;
            newShip.Crew[0] = G.Ship.Crew[0]; // commander stays
            int cSlot = 1;
            for (int i = 1; i < MaxCrew && cSlot < stype.CrewQuarters; i++)
                if (G.Ship.Crew[i] >= 0) newShip.Crew[cSlot++] = G.Ship.Crew[i];

            int wSlot = 0;
            for (int i = 0; i < MaxWeapon && wSlot < stype.WeaponSlots; i++)
                if (G.Ship.Weapon[i] >= 0) newShip.Weapon[wSlot++] = G.Ship.Weapon[i];

            int sSlot = 0;
            for (int i = 0; i < MaxShield && sSlot < stype.ShieldSlots; i++)
                if (G.Ship.Shield[i] >= 0)
                {
                    newShip.Shield[sSlot]         = G.Ship.Shield[i];
                    newShip.ShieldStrength[sSlot] = G.Ship.ShieldStrength[i];
                    sSlot++;
                }

            int gSlot = 0;
            for (int i = 0; i < MaxGadget && gSlot < stype.GadgetSlots; i++)
                if (G.Ship.Gadget[i] >= 0) newShip.Gadget[gSlot++] = G.Ship.Gadget[i];

            // Cargo and BuyingPrice are wiped on ship purchase — matches
            // original BuyShipEvent.c. The shipyard UI should warn / require
            // the player to sell cargo before the trade-in.
            for (int i = 0; i < MaxTradeItem; i++)
            {
                newShip.Cargo[i]   = 0;
                G.BuyingPrice[i]   = 0;
            }

            // Tribbles travel with the commander
            newShip.Tribbles = G.Ship.Tribbles;

            G.Ship = newShip;
            G.HullUpgraded = false;
            // Original also resets ScarabStatus from 3 → 0 on a buy
            if (G.ScarabStatus == 3) G.ScarabStatus = 0;
        }
    }
}
