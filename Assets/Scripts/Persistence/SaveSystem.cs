// Space Trader 5000 – Android/Unity Port
// JSON-based save / load using Application.persistentDataPath.

using System;
using System.IO;
using UnityEngine;
using static SpaceTrader.GameConstants;

namespace SpaceTrader.Persistence
{
    [Serializable]
    class SaveData
    {
        public long   Credits; public long Debt; public int Days;
        public int    WarpSystem; public int Difficulty;
        public long[] BuyPrice  = new long[MaxTradeItem];
        public long[] SellPrice = new long[MaxTradeItem];
        public long[] BuyingPrice = new long[MaxTradeItem];
        public long[] ShipPrice = new long[MaxShipType];
        public int    GalacticChartSystem;
        public long   PoliceKills; public long TraderKills; public long PirateKills;
        public long   PoliceRecordScore; public long ReputationScore;
        public bool   AutoFuel; public bool AutoRepair;
        public int    MonsterStatus; public int DragonflyStatus;
        public int    JaporiDiseaseStatus; public bool MoonBought;
        public long   MonsterHull;
        public string NameCommander;
        public bool   EscapePod; public bool Insurance; public int NoClaim;
        public bool   AlwaysIgnoreTraders; public bool AlwaysIgnorePolice;
        public bool   AlwaysIgnorePirates;
        public int[]  Wormhole = new int[MaxWormhole];
        public bool   ArtifactOnBoard; public bool ReserveMoney;
        public bool   PriceDifferences; public int LeaveEmpty;
        public bool   AlwaysInfo; public bool TextualEncounters;
        public int    JarekStatus; public int InvasionStatus;
        public bool   Continuous; public bool AttackFleeing;
        public int    ExperimentStatus; public int FabricRipProbability;
        public int    VeryRareEncounter; public int ReactorStatus;
        public int    TrackedSystem; public int ScarabStatus; public int WildStatus;
        public int    Shortcut1; public int Shortcut2; public int Shortcut3; public int Shortcut4;

        // Ships
        public ShipSave Ship = new ShipSave();

        // Crew
        public CrewSave[] Mercenary = new CrewSave[MaxCrewMember + 1];

        // Systems
        public SystemSave[] SolarSystem = new SystemSave[MaxSolarSystem];

        // High scores
        public HighScoreSave[] Hscores = new HighScoreSave[MaxHighScore];
    }

    [Serializable] class ShipSave
    {
        public int Type; public int[] Cargo = new int[MaxTradeItem];
        public int[] Weapon = new int[MaxWeapon]; public int[] Shield = new int[MaxShield];
        public long[] ShieldStrength = new long[MaxShield];
        public int[] Gadget = new int[MaxGadget]; public int[] Crew = new int[MaxCrew];
        public int Fuel; public long Hull; public long Tribbles;
    }

    [Serializable] class CrewSave
    {
        public int NameIndex; public int Pilot; public int Fighter;
        public int Trader; public int Engineer; public int CurSystem;
    }

    [Serializable] class SystemSave
    {
        public int NameIndex; public int TechLevel; public int Politics;
        public int Status; public int X; public int Y;
        public int SpecialResources; public int Size;
        public int[] Qty = new int[MaxTradeItem];
        public int CountDown; public bool Visited; public int Special;
    }

    [Serializable] class HighScoreSave
    {
        public string Name; public int Status; public int Days; public long Worth; public int Difficulty;
    }

    public static class SaveSystem
    {
        static string SavePath => Path.Combine(Application.persistentDataPath, "savegame.json");

        public static bool SaveExists() => File.Exists(SavePath);

        public static void Save()
        {
            var G  = GameState.Instance;
            var sd = new SaveData
            {
                Credits = G.Credits, Debt = G.Debt, Days = G.Days,
                WarpSystem = G.WarpSystem, Difficulty = G.Difficulty,
                GalacticChartSystem = G.GalacticChartSystem,
                PoliceKills = G.PoliceKills, TraderKills = G.TraderKills,
                PirateKills = G.PirateKills,
                PoliceRecordScore = G.PoliceRecordScore,
                ReputationScore   = G.ReputationScore,
                AutoFuel = G.AutoFuel, AutoRepair = G.AutoRepair,
                MonsterStatus = G.MonsterStatus, DragonflyStatus = G.DragonflyStatus,
                JaporiDiseaseStatus = G.JaporiDiseaseStatus, MoonBought = G.MoonBought,
                MonsterHull = G.MonsterHull, NameCommander = G.NameCommander,
                EscapePod = G.EscapePod, Insurance = G.Insurance, NoClaim = G.NoClaim,
                AlwaysIgnoreTraders = G.AlwaysIgnoreTraders,
                AlwaysIgnorePolice  = G.AlwaysIgnorePolice,
                AlwaysIgnorePirates = G.AlwaysIgnorePirates,
                ArtifactOnBoard = G.ArtifactOnBoard, ReserveMoney = G.ReserveMoney,
                PriceDifferences = G.PriceDifferences, LeaveEmpty = G.LeaveEmpty,
                AlwaysInfo = G.AlwaysInfo, TextualEncounters = G.TextualEncounters,
                JarekStatus = G.JarekStatus, InvasionStatus = G.InvasionStatus,
                Continuous = G.Continuous, AttackFleeing = G.AttackFleeing,
                ExperimentStatus = G.ExperimentStatus,
                FabricRipProbability = G.FabricRipProbability,
                VeryRareEncounter = G.VeryRareEncounter,
                ReactorStatus = G.ReactorStatus, TrackedSystem = G.TrackedSystem,
                ScarabStatus = G.ScarabStatus, WildStatus = G.WildStatus,
                Shortcut1 = G.Shortcut1, Shortcut2 = G.Shortcut2,
                Shortcut3 = G.Shortcut3, Shortcut4 = G.Shortcut4,
            };

            Array.Copy(G.BuyPrice,   sd.BuyPrice,   MaxTradeItem);
            Array.Copy(G.SellPrice,  sd.SellPrice,  MaxTradeItem);
            Array.Copy(G.BuyingPrice,sd.BuyingPrice, MaxTradeItem);
            Array.Copy(G.ShipPrice,  sd.ShipPrice,  MaxShipType);
            Array.Copy(G.Wormhole,   sd.Wormhole,   MaxWormhole);

            CopyShip(G.Ship, sd.Ship);

            for (int i = 0; i <= MaxCrewMember; i++)
            {
                sd.Mercenary[i] = new CrewSave
                {
                    NameIndex = G.Mercenary[i].NameIndex,
                    Pilot     = G.Mercenary[i].Pilot,
                    Fighter   = G.Mercenary[i].Fighter,
                    Trader    = G.Mercenary[i].Trader,
                    Engineer  = G.Mercenary[i].Engineer,
                    CurSystem = G.Mercenary[i].CurSystem,
                };
            }

            for (int i = 0; i < MaxSolarSystem; i++)
            {
                var s = G.SolarSystem[i];
                var ss = new SystemSave
                {
                    NameIndex = s.NameIndex, TechLevel = s.TechLevel,
                    Politics = s.Politics, Status = s.Status,
                    X = s.X, Y = s.Y, SpecialResources = s.SpecialResources,
                    Size = s.Size, CountDown = s.CountDown,
                    Visited = s.Visited, Special = s.Special,
                };
                Array.Copy(s.Qty, ss.Qty, MaxTradeItem);
                sd.SolarSystem[i] = ss;
            }

            for (int i = 0; i < MaxHighScore; i++)
            {
                sd.Hscores[i] = new HighScoreSave
                {
                    Name = G.Hscores[i].Name, Status = G.Hscores[i].Status,
                    Days = G.Hscores[i].Days, Worth  = G.Hscores[i].Worth,
                    Difficulty = G.Hscores[i].Difficulty,
                };
            }

            File.WriteAllText(SavePath, JsonUtility.ToJson(sd, prettyPrint: false));
        }

        public static bool Load()
        {
            if (!SaveExists()) return false;
            try
            {
                var sd = JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));
                var G  = GameState.Instance;

                G.Credits = sd.Credits; G.Debt = sd.Debt; G.Days = sd.Days;
                G.WarpSystem = sd.WarpSystem; G.Difficulty = sd.Difficulty;
                G.GalacticChartSystem = sd.GalacticChartSystem;
                G.PoliceKills = sd.PoliceKills; G.TraderKills = sd.TraderKills;
                G.PirateKills = sd.PirateKills;
                G.PoliceRecordScore = sd.PoliceRecordScore;
                G.ReputationScore   = sd.ReputationScore;
                G.AutoFuel = sd.AutoFuel; G.AutoRepair = sd.AutoRepair;
                G.MonsterStatus = sd.MonsterStatus; G.DragonflyStatus = sd.DragonflyStatus;
                G.JaporiDiseaseStatus = sd.JaporiDiseaseStatus;
                G.MoonBought = sd.MoonBought; G.MonsterHull = sd.MonsterHull;
                G.NameCommander = sd.NameCommander;
                G.EscapePod = sd.EscapePod; G.Insurance = sd.Insurance; G.NoClaim = sd.NoClaim;
                G.AlwaysIgnoreTraders = sd.AlwaysIgnoreTraders;
                G.AlwaysIgnorePolice  = sd.AlwaysIgnorePolice;
                G.AlwaysIgnorePirates = sd.AlwaysIgnorePirates;
                G.ArtifactOnBoard = sd.ArtifactOnBoard; G.ReserveMoney = sd.ReserveMoney;
                G.PriceDifferences = sd.PriceDifferences; G.LeaveEmpty = sd.LeaveEmpty;
                G.AlwaysInfo = sd.AlwaysInfo; G.TextualEncounters = sd.TextualEncounters;
                G.JarekStatus = sd.JarekStatus; G.InvasionStatus = sd.InvasionStatus;
                G.Continuous = sd.Continuous; G.AttackFleeing = sd.AttackFleeing;
                G.ExperimentStatus = sd.ExperimentStatus;
                G.FabricRipProbability = sd.FabricRipProbability;
                G.VeryRareEncounter = sd.VeryRareEncounter;
                G.ReactorStatus = sd.ReactorStatus; G.TrackedSystem = sd.TrackedSystem;
                G.ScarabStatus = sd.ScarabStatus; G.WildStatus = sd.WildStatus;
                G.Shortcut1 = sd.Shortcut1; G.Shortcut2 = sd.Shortcut2;
                G.Shortcut3 = sd.Shortcut3; G.Shortcut4 = sd.Shortcut4;

                Array.Copy(sd.BuyPrice,    G.BuyPrice,    MaxTradeItem);
                Array.Copy(sd.SellPrice,   G.SellPrice,   MaxTradeItem);
                Array.Copy(sd.BuyingPrice, G.BuyingPrice, MaxTradeItem);
                Array.Copy(sd.ShipPrice,   G.ShipPrice,   MaxShipType);
                Array.Copy(sd.Wormhole,    G.Wormhole,    MaxWormhole);

                CopyShipFromSave(sd.Ship, G.Ship);

                for (int i = 0; i <= MaxCrewMember; i++)
                {
                    G.Mercenary[i].NameIndex = sd.Mercenary[i].NameIndex;
                    G.Mercenary[i].Pilot     = sd.Mercenary[i].Pilot;
                    G.Mercenary[i].Fighter   = sd.Mercenary[i].Fighter;
                    G.Mercenary[i].Trader    = sd.Mercenary[i].Trader;
                    G.Mercenary[i].Engineer  = sd.Mercenary[i].Engineer;
                    G.Mercenary[i].CurSystem = sd.Mercenary[i].CurSystem;
                }

                for (int i = 0; i < MaxSolarSystem; i++)
                {
                    var ss = sd.SolarSystem[i]; var s = G.SolarSystem[i];
                    s.NameIndex = ss.NameIndex; s.TechLevel = ss.TechLevel;
                    s.Politics = ss.Politics; s.Status = ss.Status;
                    s.X = ss.X; s.Y = ss.Y; s.SpecialResources = ss.SpecialResources;
                    s.Size = ss.Size; s.CountDown = ss.CountDown;
                    s.Visited = ss.Visited; s.Special = ss.Special;
                    Array.Copy(ss.Qty, s.Qty, MaxTradeItem);
                }

                for (int i = 0; i < MaxHighScore; i++)
                {
                    G.Hscores[i].Name       = sd.Hscores[i].Name;
                    G.Hscores[i].Status     = sd.Hscores[i].Status;
                    G.Hscores[i].Days       = sd.Hscores[i].Days;
                    G.Hscores[i].Worth      = sd.Hscores[i].Worth;
                    G.Hscores[i].Difficulty = sd.Hscores[i].Difficulty;
                }

                G.GameLoaded = true;
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"SaveSystem.Load failed: {e.Message}");
                return false;
            }
        }

        public static void DeleteSave()
        {
            if (File.Exists(SavePath)) File.Delete(SavePath);
        }

        static void CopyShip(Ship src, ShipSave dst)
        {
            dst.Type = src.Type; dst.Fuel = src.Fuel;
            dst.Hull = src.Hull; dst.Tribbles = src.Tribbles;
            Array.Copy(src.Cargo,         dst.Cargo,         MaxTradeItem);
            Array.Copy(src.Weapon,        dst.Weapon,        MaxWeapon);
            Array.Copy(src.Shield,        dst.Shield,        MaxShield);
            Array.Copy(src.ShieldStrength,dst.ShieldStrength,MaxShield);
            Array.Copy(src.Gadget,        dst.Gadget,        MaxGadget);
            Array.Copy(src.Crew,          dst.Crew,          MaxCrew);
        }

        static void CopyShipFromSave(ShipSave src, Ship dst)
        {
            dst.Type = src.Type; dst.Fuel = src.Fuel;
            dst.Hull = src.Hull; dst.Tribbles = src.Tribbles;
            Array.Copy(src.Cargo,         dst.Cargo,         MaxTradeItem);
            Array.Copy(src.Weapon,        dst.Weapon,        MaxWeapon);
            Array.Copy(src.Shield,        dst.Shield,        MaxShield);
            Array.Copy(src.ShieldStrength,dst.ShieldStrength,MaxShield);
            Array.Copy(src.Gadget,        dst.Gadget,        MaxGadget);
            Array.Copy(src.Crew,          dst.Crew,          MaxCrew);
        }
    }
}
