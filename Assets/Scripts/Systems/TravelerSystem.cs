// Space Trader 5000 - Android/Unity Port
// Ported from Space Trader 1.2.2 by Pieter Spronck (GPL)
// Original: Traveler.c (pure game-logic portions)
// UI event handlers are excluded here; they belong to future UI scripts.

using static SpaceTrader.GameConstants;
using static SpaceTrader.GameMath;

namespace SpaceTrader
{
    public static class TravelerSystem
    {
        static GameState G => GameState.Instance;

        public static long GetScore(int endStatus, int days, long worth, int level)
        {
            long w = worth < 1000000 ? worth : 1000000 + (worth - 1000000) / 10;
            if (endStatus == Killed)   return (level + 1L) * (w * 90  / 50000);
            if (endStatus == Retired)  return (level + 1L) * (w * 95  / 50000);
            long d = Max(0L, (level + 1L) * 100 - days);
            return (level + 1L) * ((w + d * 1000) / 500);
        }

        public static void TryRecordHighScore(int endStatus)
        {
            long score = GetScore(endStatus, G.Days, MoneySystem.CurrentWorth(), G.Difficulty);
            for (int i = 0; i < MaxHighScore; i++)
            {
                long existing = GetScore(G.Hscores[i].Status, G.Hscores[i].Days,
                                         G.Hscores[i].Worth, G.Hscores[i].Difficulty);
                if (score > existing || G.Hscores[i].Name == null || G.Hscores[i].Name.Length == 0)
                {
                    for (int j = MaxHighScore - 1; j > i; j--)
                        G.Hscores[j] = G.Hscores[j - 1];
                    G.Hscores[i] = new HighScore
                    {
                        Name       = G.NameCommander,
                        Status     = endStatus,
                        Days       = G.Days,
                        Worth      = MoneySystem.CurrentWorth(),
                        Difficulty = G.Difficulty,
                    };
                    break;
                }
            }
        }

        public static long StandardPrice(int good, int size, int tech, int government, int resources)
        {
            var pol  = GameData.PoliticsTypes[government];
            var item = GameData.Tradeitems[good];

            if ((good == Narcotics && !pol.DrugsOK) ||
                (good == Firearms  && !pol.FirearmsOK)) return 0L;

            long price = item.PriceLowTech + tech * item.PriceInc;
            if (pol.Wanted == good)                        price = price * 4 / 3;
            price = price * (100 - 2 * pol.StrengthTraders) / 100;
            price = price * (100 - size) / 100;

            if (resources > 0)
            {
                if (item.CheapResource >= 0 && resources == item.CheapResource)
                    price = price * 3 / 4;
                if (item.ExpensiveResource >= 0 && resources == item.ExpensiveResource)
                    price = price * 4 / 3;
            }

            if (tech < item.TechUsage || price < 0) return 0L;
            return price;
        }

        public static void DeterminePrices(int systemID)
        {
            var sys = G.SolarSystem[systemID];
            for (int i = 0; i < MaxTradeItem; i++)
            {
                long base_ = StandardPrice(i, sys.Size, sys.TechLevel, sys.Politics, sys.SpecialResources);
                if (base_ <= 0) { G.BuyPrice[i] = G.SellPrice[i] = 0; continue; }

                var item = GameData.Tradeitems[i];
                if (item.DoublePriceStatus >= 0 && sys.Status == item.DoublePriceStatus)
                    base_ = base_ * 3 / 2;

                base_ += GetRandom(item.Variance) - GetRandom(item.Variance);
                if (base_ <= 0) { G.BuyPrice[i] = G.SellPrice[i] = 0; continue; }

                G.SellPrice[i] = base_;
                if (G.PoliceRecordScore < DubiousScore)
                    G.SellPrice[i] = base_ * 90 / 100;
            }
            SkillSystem.RecalculateBuyPrices(systemID);
        }

        public static void InitializeTradeitems(int index)
        {
            var sys = G.SolarSystem[index];
            for (int i = 0; i < MaxTradeItem; i++)
            {
                var item = GameData.Tradeitems[i];
                var pol  = GameData.PoliticsTypes[sys.Politics];

                if ((i == Narcotics && !pol.DrugsOK) ||
                    (i == Firearms  && !pol.FirearmsOK) ||
                    sys.TechLevel < item.TechProduction)
                {
                    sys.Qty[i] = 0;
                    continue;
                }

                int qty = (9 + GetRandom(5) - Abs(item.TechTopProduction - sys.TechLevel))
                          * (1 + sys.Size);

                if (i == Robots || i == Narcotics)
                    qty = qty * (5 - G.Difficulty) / (6 - G.Difficulty) + 1;

                if (item.CheapResource >= 0 && sys.SpecialResources == item.CheapResource)
                    qty = qty * 4 / 3;
                if (item.ExpensiveResource >= 0 && sys.SpecialResources == item.ExpensiveResource)
                    qty = qty * 3 / 4;
                if (item.DoublePriceStatus >= 0 && sys.Status == item.DoublePriceStatus)
                    qty = qty / 5;

                qty += GetRandom(10) - GetRandom(10);
                sys.Qty[i] = Max(0, qty);
            }
        }

        public static void ShuffleStatus()
        {
            for (int i = 0; i < MaxSolarSystem; i++)
            {
                if (G.SolarSystem[i].Status > 0)
                { if (GetRandom(100) < 15) G.SolarSystem[i].Status = Uneventful; }
                else if (GetRandom(100) < 15)
                    G.SolarSystem[i].Status = 1 + GetRandom(MaxStatus - 1);
            }
        }

        public static void ChangeQuantities()
        {
            for (int i = 0; i < MaxSolarSystem; i++)
            {
                if (--G.SolarSystem[i].CountDown <= 0)
                {
                    G.SolarSystem[i].CountDown = 3 + G.Difficulty;
                    InitializeTradeitems(i);
                }
                else
                {
                    for (int j = 0; j < MaxTradeItem; j++)
                    {
                        G.SolarSystem[i].Qty[j] += GetRandom(5) - GetRandom(5);
                        if (G.SolarSystem[i].Qty[j] < 0)
                            G.SolarSystem[i].Qty[j] = 0;
                    }
                }
            }
        }

        public static bool WormholeExists(int from, int to)
        {
            for (int i = 0; i < MaxWormhole; i++)
                if (G.Wormhole[i] == from)
                    return (i + 1 < MaxWormhole && G.Wormhole[i + 1] == to) ||
                           (i == MaxWormhole - 1 && G.Wormhole[0] == to);
            return false;
        }

        public static long WormholeTax(int from, int to)
            => WormholeExists(from, to) ? GameData.Shiptypes[G.Ship.Type].CostOfFuel * 25L : 0L;

        public static bool Cloaked(Ship sh, Ship opp)
        {
            if (!SkillSystem.HasGadget(sh, CloakingDevice)) return false;
            return SkillSystem.PilotSkill(sh) + GetRandom(MaxSkill) >
                   SkillSystem.EngineerSkill(opp) + GetRandom(MaxSkill);
        }

        public static int NextSystemWithinRange(int current, bool back)
        {
            int i = back ? current - 1 : current + 1;
            while (true)
            {
                if (i < 0) i = MaxSolarSystem - 1;
                else if (i >= MaxSolarSystem) i = 0;
                if (i == current) break;

                if (WormholeExists(G.Commander.CurSystem, i)) return i;
                long dist = RealDistance(G.SolarSystem[G.Commander.CurSystem], G.SolarSystem[i]);
                if (dist <= FuelSystem.GetFuelTanks() && dist > 0) return i;

                if (back) i--; else i++;
            }
            return -1;
        }

        public static void IncDays(int amount)
        {
            G.Days += amount;

            if (G.ReactorStatus > 0 && G.ReactorStatus < 21)
            {
                G.ReactorStatus += amount;
                if (G.ReactorStatus > 20)
                {
                    EncounterSystem.EscapeWithPod();
                    return;
                }
            }

            if (G.InvasionStatus > 0 && G.InvasionStatus < 8)
                G.InvasionStatus += amount;

            if (G.ExperimentStatus > 0 && G.ExperimentStatus < 12)
            {
                G.ExperimentStatus += amount;
                if (G.ExperimentStatus >= 12)
                {
                    G.FabricRipProbability = FabricRipInitialProbability;
                    G.ExperimentStatus = 12;
                }
            }
            else if (G.ExperimentStatus == 12 && G.FabricRipProbability > 0)
                G.FabricRipProbability--;

            ChangeQuantities();
        }

        public static void Arrival()
        {
            var cmdr = G.Commander;
            cmdr.CurSystem = G.WarpSystem;
            G.SolarSystem[G.WarpSystem].Visited = true;

            if (G.TrackAutoOff && G.TrackedSystem == G.WarpSystem)
                G.TrackedSystem = -1;

            ShuffleStatus();
            DeterminePrices(G.WarpSystem);

            if (G.AutoFuel)
                FuelSystem.BuyFuel(FuelSystem.GetFuelTanks() - FuelSystem.GetFuel());
            if (G.AutoRepair)
                ShipyardSystem.BuyRepairs((int)(ShipyardSystem.GetHullStrength() - G.Ship.Hull));

            if (G.Ship.Tribbles > 0 && G.ReactorStatus == 0)
            {
                G.Ship.Tribbles += 1 + GetRandom((int)(G.Ship.Tribbles > 100 ? G.Ship.Tribbles / 100 : 1));
                if (G.Ship.Tribbles > MaxTribbles) G.Ship.Tribbles = MaxTribbles;
            }

            if (G.ReactorStatus > 0)
            {
                for (int i = 0; i < 3; i++)
                    if (G.Ship.Cargo[Narcotics] > 0) G.Ship.Cargo[Narcotics]--;
                    else if (G.Ship.Cargo[Food] > 0) G.Ship.Cargo[Food]--;
            }

            G.Raided     = false;
            G.Inspected  = false;
            G.AlreadyPaidForNewspaper = false;
        }

        public enum WarpResult
        {
            Success,
            WildNeedsWeapon,
            DebtTooLarge,
            CantPayMercenaries,
            CantPayInsurance,
            CantPayWormholeTax,
        }

        public static WarpResult DoWarp(bool viaSingularity)
        {
            int curSys = G.Commander.CurSystem;

            if (G.WildStatus == 1 && !SkillSystem.HasWeapon(G.Ship, BeamLaserWeapon, false))
                return WarpResult.WildNeedsWeapon;

            if (G.Debt > DebtTooLarge)                               return WarpResult.DebtTooLarge;
            if (MoneySystem.MercenaryMoney() > G.Credits)            return WarpResult.CantPayMercenaries;

            long wormTax = WormholeTax(curSys, G.WarpSystem);
            long merMon  = MoneySystem.MercenaryMoney();
            long insMon  = MoneySystem.InsuranceMoney();

            if (G.Insurance && insMon + merMon > G.Credits)          return WarpResult.CantPayInsurance;
            if (insMon + merMon + wormTax > G.Credits)               return WarpResult.CantPayWormholeTax;

            if (!viaSingularity)
            {
                G.Credits -= wormTax + merMon + insMon;
            }

            for (int i = 0; i < MaxShield; i++)
            {
                if (G.Ship.Shield[i] < 0) continue;
                G.Ship.ShieldStrength[i] = GameData.Shieldtypes[G.Ship.Shield[i]].Power;
            }

            G.SolarSystem[curSys].CountDown = 3 + G.Difficulty;

            if (WormholeExists(curSys, G.WarpSystem) || viaSingularity)
            {
                G.ArrivedViaWormhole = true;
            }
            else
            {
                int dist = (int)RealDistance(G.SolarSystem[curSys], G.SolarSystem[G.WarpSystem]);
                G.Ship.Fuel -= Min(dist, FuelSystem.GetFuel());
                G.ArrivedViaWormhole = false;
            }

            if (!viaSingularity)
            {
                MoneySystem.PayInterest();
                IncDays(1);
                if (G.Insurance) G.NoClaim++;
            }

            G.Clicks         = (int)RealDistance(G.SolarSystem[G.Commander.CurSystem], G.SolarSystem[G.WarpSystem]);
            G.Raided         = false;
            G.Inspected      = false;
            G.LitterWarning  = false;
            G.MonsterHull    = Min(G.MonsterHull * 105 / 100,
                                   GameData.Shiptypes[G.SpaceMonster.Type].HullStrength);

            if (G.Days % 3 == 0 && G.PoliceRecordScore > CleanScore)
                G.PoliceRecordScore--;
            if (G.PoliceRecordScore < DubiousScore)
            {
                if (G.Difficulty <= Normal) G.PoliceRecordScore++;
                else if (G.Days % G.Difficulty == 0) G.PoliceRecordScore++;
            }

            G.PossibleToGoThroughRip = true;
            DeterminePrices(G.WarpSystem);
            Travel();
            return WarpResult.Success;
        }

        public static bool Travel()
        {
            return true;
        }

        public static bool StartNewGame()
        {
            G.Reset();

            var cmdr = G.Commander;
            cmdr.CurSystem = SolSystem;
            cmdr.Pilot    = 0;
            cmdr.Fighter  = 0;
            cmdr.Trader   = 0;
            cmdr.Engineer = 0;

            PlaceWormholes();
            GenerateGalaxy();
            PlaceSpecialEvents();

            G.WarpSystem = SolSystem;
            DeterminePrices(SolSystem);
            G.Ship.Fuel = FuelSystem.GetFuelTanks();
            G.GameLoaded = false;
            return true;
        }

        static void GenerateGalaxy()
        {
            for (int i = 0; i < MaxSolarSystem; i++)
            {
                var sys = G.SolarSystem[i];
                sys.NameIndex        = i;
                sys.TechLevel        = GetRandom(MaxTechLevel);
                sys.Politics         = GetRandom(MaxPolitics);
                sys.Status           = Uneventful;
                sys.SpecialResources = GetRandom(MaxResources);
                sys.Size             = GetRandom(MaxSize);
                sys.CountDown        = 3 + G.Difficulty;
                sys.Visited          = false;
                sys.Special          = -1;

                bool placed = false;
                int tries   = 0;
                while (!placed && tries < 100)
                {
                    tries++;
                    sys.X = GalaxyWidth  / 7 + GetRandom(GalaxyWidth  * 5 / 7);
                    sys.Y = GalaxyHeight / 7 + GetRandom(GalaxyHeight * 5 / 7);
                    placed = true;
                    for (int j = 0; j < i; j++)
                    {
                        if (RealDistance(sys, G.SolarSystem[j]) < MinDistance)
                        { placed = false; break; }
                    }
                }

                InitializeTradeitems(i);
            }

            // Sol is always Hi-tech Democratic to match the original Palm OS game.
            // Without this, Sol's TechLevel is random and most goods may be unavailable
            // at game start, making the BuyCargo screen look empty.
            var sol = G.SolarSystem[SolSystem];
            sol.TechLevel        = MaxTechLevel - 1; // Hi-tech
            sol.Politics         = 6;                // Democracy
            sol.Size             = 3;                // Large
            sol.SpecialResources = NoSpecialResources;
            InitializeTradeitems(SolSystem);
        }

        static void PlaceWormholes()
        {
            G.Wormhole = new int[MaxWormhole];
            for (int i = 0; i < MaxWormhole; i++) G.Wormhole[i] = -1;

            bool[] used = new bool[MaxSolarSystem];
            for (int i = 0; i < MaxWormhole && i < MaxSolarSystem; i++)
            {
                int s;
                do { s = GetRandom(MaxSolarSystem); } while (used[s]);
                G.Wormhole[i] = s;
                used[s] = true;
            }
        }

        static void PlaceSpecialEvents()
        {
            G.SolarSystem[ZalkonSystem].Special    = DragonflyDestroyed; // Dragonfly was heading to Zalkon
            G.SolarSystem[BaratasSystem].Special   = FlyBaratas;
            G.SolarSystem[MelinaSystem].Special    = FlyMelina;
            G.SolarSystem[RegulasSystem].Special   = FlyRegulas;
            G.SolarSystem[AcamarSystem].Special    = MonsterKilled;      // Monster originated at Acamar
            G.SolarSystem[JaporiSystem].Special    = MedicineDelivery;
            G.SolarSystem[UtopiaSystem].Special    = MoonBoughtEvent;

            for (int i = EndFixed; i < MaxSpecialEvent; i++)
            {
                int tries = 0;
                while (tries < 100)
                {
                    tries++;
                    int s = GetRandom(MaxSolarSystem);
                    if (G.SolarSystem[s].Special < 0)
                    { G.SolarSystem[s].Special = i; break; }
                }
            }
        }

        public static bool AnyEmptySlots(Ship ship)
        {
            var st = GameData.Shiptypes[ship.Type];
            for (int i = 0; i < st.WeaponSlots; i++) if (ship.Weapon[i] < 0) return true;
            for (int i = 0; i < st.ShieldSlots; i++) if (ship.Shield[i] < 0) return true;
            for (int i = 0; i < st.GadgetSlots; i++) if (ship.Gadget[i] < 0) return true;
            return false;
        }
    }
}
