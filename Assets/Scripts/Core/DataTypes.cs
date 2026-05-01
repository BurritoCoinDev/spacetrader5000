// Space Trader 5000 - Android/Unity Port
// Ported from Space Trader 1.2.2 by Pieter Spronck (GPL)
// Original: DataTypes.h

using System;

namespace SpaceTrader
{
    [Serializable]
    public class Ship
    {
        public int   Type;
        public int[] Cargo        = new int[GameConstants.MaxTradeItem];
        public int[] Weapon       = new int[GameConstants.MaxWeapon];
        public int[] Shield       = new int[GameConstants.MaxShield];
        public long[]ShieldStrength = new long[GameConstants.MaxShield];
        public int[] Gadget       = new int[GameConstants.MaxGadget];
        public int[] Crew         = new int[GameConstants.MaxCrew];
        public int   Fuel;
        public long  Hull;
        public long  Tribbles;

        public Ship Clone()
        {
            var s = new Ship
            {
                Type     = Type,
                Fuel     = Fuel,
                Hull     = Hull,
                Tribbles = Tribbles
            };
            Array.Copy(Cargo, s.Cargo, Cargo.Length);
            Array.Copy(Weapon, s.Weapon, Weapon.Length);
            Array.Copy(Shield, s.Shield, Shield.Length);
            Array.Copy(ShieldStrength, s.ShieldStrength, ShieldStrength.Length);
            Array.Copy(Gadget, s.Gadget, Gadget.Length);
            Array.Copy(Crew, s.Crew, Crew.Length);
            return s;
        }
    }

    [Serializable]
    public class Gadget
    {
        public string Name;
        public long   Price;
        public int    TechLevel;
        public int    Chance;
    }

    [Serializable]
    public class Weapon
    {
        public string Name;
        public long   Power;
        public long   Price;
        public int    TechLevel;
        public int    Chance;
    }

    [Serializable]
    public class Shield
    {
        public string Name;
        public long   Power;
        public long   Price;
        public int    TechLevel;
        public int    Chance;
    }

    [Serializable]
    public class CrewMember
    {
        public int NameIndex;
        public int Pilot;
        public int Fighter;
        public int Trader;
        public int Engineer;
        public int CurSystem;
    }

    [Serializable]
    public class ShipType
    {
        public string Name;
        public int    CargoBays;
        public int    WeaponSlots;
        public int    ShieldSlots;
        public int    GadgetSlots;
        public int    CrewQuarters;
        public int    FuelTanks;
        public int    MinTechLevel;
        public int    CostOfFuel;
        public long   Price;
        public int    Bounty;
        public int    Occurrence;
        public long   HullStrength;
        public int    Police;
        public int    Pirates;
        public int    Traders;
        public int    RepairCosts;
        public int    Size;
    }

    [Serializable]
    public class SolarSystem
    {
        public int  NameIndex;
        public int  TechLevel;
        public int  Politics;
        public int  Status;
        public int  X;
        public int  Y;
        public int  SpecialResources;
        public int  Size;
        public int[] Qty      = new int[GameConstants.MaxTradeItem];
        public int  CountDown;
        public bool Visited;
        public int  Special;
    }

    [Serializable]
    public class TradeItem
    {
        public string Name;
        public int    TechProduction;
        public int    TechUsage;
        public int    TechTopProduction;
        public int    PriceLowTech;
        public int    PriceInc;
        public int    Variance;
        public int    DoublePriceStatus;
        public int    CheapResource;
        public int    ExpensiveResource;
        public int    MinTradePrice;
        public int    MaxTradePrice;
        public int    RoundOff;
    }

    [Serializable]
    public class Politics
    {
        public string Name;
        public int    ReactionIllegal;
        public int    StrengthPolice;
        public int    StrengthPirates;
        public int    StrengthTraders;
        public int    MinTechLevel;
        public int    MaxTechLevel;
        public int    BribeLevel;
        public bool   DrugsOK;
        public bool   FirearmsOK;
        public int    Wanted;
    }

    [Serializable]
    public class SpecialEvent
    {
        public string Title;
        public int    QuestStringID;
        public long   Price;
        public int    Occurrence;
        public bool   JustAMessage;
    }

    [Serializable]
    public class PoliceRecord
    {
        public string Name;
        public int    MinScore;
    }

    [Serializable]
    public class Reputation
    {
        public string Name;
        public int    MinScore;
    }

    [Serializable]
    public class HighScore
    {
        public string Name;
        public int    Status;
        public int    Days;
        public long   Worth;
        public int    Difficulty;
    }
}
