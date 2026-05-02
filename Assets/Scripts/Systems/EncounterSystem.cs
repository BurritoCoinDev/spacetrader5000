// Space Trader 5000 - Android/Unity Port
// Ported from Space Trader 1.2.2 by Pieter Spronck (GPL)
// Original: Encounter.c (pure game-logic portions)
// UI event handlers are excluded; they belong to future UI scripts.

using static SpaceTrader.GameConstants;
using static SpaceTrader.GameMath;

namespace SpaceTrader
{
    public static class EncounterSystem
    {
        static GameState G => GameState.Instance;

        public static long TotalWeapons(Ship sh, int minWeapon, int maxWeapon)
        {
            long total = 0;
            for (int i = 0; i < MaxWeapon; i++)
            {
                if (sh.Weapon[i] < 0) continue;
                if (sh.Weapon[i] >= minWeapon && sh.Weapon[i] <= maxWeapon)
                    total += GameData.Weapontypes[sh.Weapon[i]].Power;
            }
            return total;
        }

        public static long TotalShields(Ship sh)
        {
            long total = 0;
            for (int i = 0; i < MaxShield; i++)
                if (sh.Shield[i] >= 0) total += GameData.Shieldtypes[sh.Shield[i]].Power;
            return total;
        }

        public static long TotalShieldStrength(Ship sh)
        {
            long total = 0;
            for (int i = 0; i < MaxShield; i++)
                if (sh.Shield[i] >= 0) total += sh.ShieldStrength[i];
            return total;
        }

        public static long GetBounty(Ship sh)
        {
            long bounty = ShipPriceSystem.EnemyShipPrice(sh) / 200 / 25 * 25;
            if (bounty <= 0)  bounty = 25;
            if (bounty > 2500) bounty = 2500;
            return bounty;
        }

        public static int PoliceStrength(int systemIndex)
        {
            int base_ = GameData.PoliticsTypes[G.SolarSystem[systemIndex].Politics].StrengthPolice;
            if (G.PoliceRecordScore < PsychopathScore) return 3 * base_;
            if (G.PoliceRecordScore < VillainScore)    return 2 * base_;
            return base_;
        }

        public static bool IsPolice(int enc)         => enc >= Police       && enc <= MaxPolice;
        public static bool IsPirate(int enc)         => enc >= Pirate       && enc <= MaxPirate;
        public static bool IsTrader(int enc)         => enc >= Trader       && enc <= MaxTrader;
        public static bool IsSpaceMonster(int enc)   => enc >= SpaceMonsterAttack && enc <= MaxSpaceMonster;
        public static bool IsDragonfly(int enc)      => enc >= DragonflyAttack    && enc <= MaxDragonfly;
        public static bool IsScarab(int enc)         => enc >= ScarabAttack       && enc <= MaxScarab;
        public static bool IsFamousCaptain(int enc)  => enc >= FamousCaptain      && enc <= MaxFamousCaptain;

        public static void GenerateOpponent(int oppType)
        {
            var opp = G.Opponent;
            for (int i = 0; i < MaxWeapon; i++)  opp.Weapon[i]  = -1;
            for (int i = 0; i < MaxShield; i++)  { opp.Shield[i] = -1; opp.ShieldStrength[i] = 0; }
            for (int i = 0; i < MaxGadget; i++)  opp.Gadget[i]  = -1;
            for (int i = 0; i < MaxCrew; i++)    opp.Crew[i]    = -1;

            if (oppType == FamousCaptain)
            {
                opp.Type = MaxShipType - 1;
                for (int i = 0; i < MaxShield; i++) { opp.Shield[i] = ReflectiveShield; opp.ShieldStrength[i] = RShieldPower; }
                for (int i = 0; i < MaxWeapon; i++)   opp.Weapon[i] = MilitaryLaserWeapon;
                opp.Gadget[0] = TargetingSystem;
                opp.Gadget[1] = NavigatingSystem;
                opp.Hull      = GameData.Shiptypes[opp.Type].HullStrength;
                opp.Crew[0]   = MaxCrewMember;
                G.Mercenary[MaxCrewMember].Pilot    = MaxSkill;
                G.Mercenary[MaxCrewMember].Fighter  = MaxSkill;
                G.Mercenary[MaxCrewMember].Trader   = MaxSkill;
                G.Mercenary[MaxCrewMember].Engineer = MaxSkill;
                return;
            }

            int tries  = 1;
            if (oppType == Mantis)     tries = 1 + G.Difficulty;
            if (oppType == Police)
            {
                tries = G.PoliceRecordScore < VillainScore && G.WildStatus != 1 ? 3 :
                        G.PoliceRecordScore < PsychopathScore || G.WildStatus == 1 ? 5 : 1;
                tries = Max(1, tries + G.Difficulty - Normal);
            }
            if (oppType == Pirate)
            {
                tries = 1 + (int)(MoneySystem.CurrentWorth() / 100000L);
                tries = Max(1, tries + G.Difficulty - Normal);
            }

            int shipType = oppType == Trader ? 0 : 1;
            int k        = G.Difficulty >= Normal ? G.Difficulty - Normal : 0;
            var pol      = GameData.PoliticsTypes[G.SolarSystem[G.WarpSystem].Politics];

            for (int j = 0; j < tries; j++)
            {
                int chosen;
                do
                {
                    long d   = GetRandom(100);
                    chosen   = 0;
                    long sum = GameData.Shiptypes[0].Occurrence;
                    while (sum < d && chosen < MaxShipType - 1) { chosen++; sum += GameData.Shiptypes[chosen].Occurrence; }

                    if (oppType == Police  && (GameData.Shiptypes[chosen].Police   < 0 || pol.StrengthPolice   + k < GameData.Shiptypes[chosen].Police))   continue;
                    if (oppType == Pirate  && (GameData.Shiptypes[chosen].Pirates  < 0 || pol.StrengthPirates  + k < GameData.Shiptypes[chosen].Pirates))  continue;
                    if (oppType == Trader  && (GameData.Shiptypes[chosen].Traders  < 0 || pol.StrengthTraders  + k < GameData.Shiptypes[chosen].Traders))  continue;
                    break;
                } while (true);

                if (chosen > shipType) shipType = chosen;
            }

            if (oppType == Mantis)      opp.Type = MantisType;
            else                        opp.Type = shipType;

            tries = Max(1, (int)(MoneySystem.CurrentWorth() / 150000L) + G.Difficulty - Normal);

            var stype = GameData.Shiptypes[opp.Type];
            int gCount = stype.GadgetSlots <= 0 ? 0 :
                         G.Difficulty <= Hard ? GetRandom(stype.GadgetSlots + 1) : stype.GadgetSlots;
            if (gCount < stype.GadgetSlots && tries > 4)        gCount++;
            else if (gCount < stype.GadgetSlots && tries > 2)   gCount += GetRandom(2);

            for (int i = 0; i < gCount; i++)
            {
                int best = 0;
                for (int e = 0; e < tries; e++)
                {
                    long rnd = GetRandom(100); int g = 0; long s = GameData.Gadgettypes[0].Chance;
                    while (rnd >= s && g < MaxGadgetType - 1) { g++; s += GameData.Gadgettypes[g].Chance; }
                    if (!SkillSystem.HasGadget(opp, g) && g > best) best = g;
                }
                opp.Gadget[i] = best;
            }

            int wCount = stype.WeaponSlots <= 0 ? 0 :
                         G.Difficulty <= Hard ? 1 + GetRandom(stype.WeaponSlots) : stype.WeaponSlots;
            for (int i = 0; i < wCount; i++)
            {
                int best = 0;
                for (int e = 0; e < tries; e++)
                {
                    long rnd = GetRandom(100); int w = 0; long s = GameData.Weapontypes[0].Chance;
                    while (rnd >= s && w < MaxWeaponType - 1) { w++; s += GameData.Weapontypes[w].Chance; }
                    if (!SkillSystem.HasWeapon(opp, w, true) && w > best) best = w;
                }
                opp.Weapon[i] = best;
            }

            int sCount = stype.ShieldSlots <= 0 ? 0 :
                         G.Difficulty <= Hard ? GetRandom(stype.ShieldSlots + 1) : stype.ShieldSlots;
            for (int i = 0; i < sCount; i++)
            {
                int best = 0;
                for (int e = 0; e < tries; e++)
                {
                    long rnd = GetRandom(100); int sh = 0; long s = GameData.Shieldtypes[0].Chance;
                    while (rnd >= s && sh < MaxShieldType - 1) { sh++; s += GameData.Shieldtypes[sh].Chance; }
                    if (!SkillSystem.HasShield(opp, sh) && sh > best) best = sh;
                }
                opp.Shield[i]         = best;
                opp.ShieldStrength[i] = GameData.Shieldtypes[best].Power;
            }

            int bays = stype.CargoBays;
            for (int i = 0; i < MaxGadget; i++) if (opp.Gadget[i] == ExtraBays) bays += 5;
            for (int i = 0; i < MaxTradeItem; i++) opp.Cargo[i] = 0;
            for (int i = 0; i < bays / 5; i++)
            {
                int item = GetRandom(MaxTradeItem);
                if (opp.Cargo[item] < bays) opp.Cargo[item] += GetRandom(5);
            }

            opp.Hull = GameData.Shiptypes[opp.Type].HullStrength;
            opp.Fuel = stype.FuelTanks;

            // Always use the reserved famous-captain slot (MaxCrewMember) for
            // opponent crew so we don't scramble a hireable merc's stats.
            // Original Traveler.c uses the same slot for every opponent.
            opp.Crew[0] = MaxCrewMember;
            G.Mercenary[MaxCrewMember].Pilot    = SkillSystem.RandomSkill();
            G.Mercenary[MaxCrewMember].Fighter  = SkillSystem.RandomSkill();
            G.Mercenary[MaxCrewMember].Trader   = SkillSystem.RandomSkill();
            G.Mercenary[MaxCrewMember].Engineer = SkillSystem.RandomSkill();
        }

        public static bool ExecuteAttack(Ship attacker, Ship defender, bool defenderFlees, bool commanderUnderAttack)
        {
            // Beginner: commander takes zero damage while fleeing (free escape guarantee)
            if (G.Difficulty == Beginner && commanderUnderAttack && defenderFlees)
                return false;

            // Scarab hull is only vulnerable to Pulse (0) and Morgan (3) lasers
            long damage;
            if (defender.Type == ScarabType)
                damage = TotalWeapons(attacker, PulseLaserWeapon, PulseLaserWeapon)
                       + TotalWeapons(attacker, MorganLaserWeapon, MorganLaserWeapon);
            else
                damage = TotalWeapons(attacker, 0, MaxWeaponType + ExtraWeapons - 1);

            if (damage <= 0) return false;

            int fSkill = SkillSystem.FighterSkill(attacker);
            damage     = damage * (fSkill + GetRandom(MaxSkill)) / (2 * MaxSkill);

            if (defenderFlees) damage = damage * 2 / 3;
            damage += GetRandom((int)(damage / 2 + 1)) - GetRandom((int)(damage / 2 + 1));
            damage = Max(0L, damage);

            // Active reactor amplifies incoming damage to the commander
            if (commanderUnderAttack && G.ReactorStatus > 0 && G.ReactorStatus < 21)
                damage += damage * (G.Difficulty + 1) / 4;

            long shieldsLeft = TotalShieldStrength(defender);
            if (shieldsLeft > 0)
            {
                long shieldDamage = Min(damage, shieldsLeft);
                damage -= shieldDamage;
                for (int i = 0; i < MaxShield && shieldDamage > 0; i++)
                {
                    if (defender.Shield[i] < 0) continue;
                    long take = Min(defender.ShieldStrength[i], shieldDamage);
                    defender.ShieldStrength[i] -= take;
                    shieldDamage -= take;
                }
            }

            if (damage > 0)
            {
                // Cap per-hit hull damage to prevent one-shot kills
                long maxHullHit = commanderUnderAttack
                    ? Max(1L, defender.Hull / Max(1, Impossible - G.Difficulty))
                    : Max(1L, defender.Hull / 2);
                damage = Min(damage, maxHullHit);

                defender.Hull -= damage;
                if (commanderUnderAttack && SkillSystem.HasGadget(defender, AutoRepairSystem))
                    defender.Hull += Max(0L, GetRandom(SkillSystem.EngineerSkill(defender)));
                if (defender.Hull <= 0) return true;
            }

            return false;
        }

        public static void EscapeWithPod()
        {
            if (!G.EscapePod) return;

            // Order mirrors original Encounter.c EscapeWithPod():
            // arrive at destination first, then process quest/state cleanup.
            if (G.ScarabStatus == 3) G.ScarabStatus = 0;

            G.Commander.CurSystem = G.WarpSystem;
            TravelerSystem.Arrival();

            // Reactor melts down if it was on the pod
            if (G.ReactorStatus > 0 && G.ReactorStatus < 21) G.ReactorStatus = 0;
            // Antidote is destroyed
            if (G.JaporiDiseaseStatus == 1) G.JaporiDiseaseStatus = 0;
            // Artifact is lost
            if (G.ArtifactOnBoard) G.ArtifactOnBoard = false;
            // Jarek is taken home
            if (G.JarekStatus == 1) G.JarekStatus = 0;
            // Wild is arrested — police record penalty
            if (G.WildStatus == 1)
            {
                G.PoliceRecordScore += CaughtWithWildScore;
                G.WildStatus = 0;
            }
            // Tribbles don't survive the pod
            if (G.Ship.Tribbles > 0) G.Ship.Tribbles = 0;

            // Insurance pays out the previous ship value before we replace it
            long insurancePayout = 0;
            if (G.Insurance)
            {
                insurancePayout = ShipPriceSystem.CurrentShipPriceWithoutCargo(true);
                G.Insurance = false;
            }

            // Replace with a fresh Flea — wipes cargo, weapons, shields, gadgets,
            // mercenaries, and any per-cargo running-average buying price.
            var newShip = new Ship
            {
                Type = 0,
                Fuel = GameData.Shiptypes[0].FuelTanks,
                Hull = GameData.Shiptypes[0].HullStrength,
            };
            for (int i = 0; i < MaxWeapon; i++)  newShip.Weapon[i]  = -1;
            for (int i = 0; i < MaxShield; i++)  { newShip.Shield[i] = -1; newShip.ShieldStrength[i] = 0; }
            for (int i = 0; i < MaxGadget; i++)  newShip.Gadget[i]  = -1;
            for (int i = 0; i < MaxCrew; i++)    newShip.Crew[i]    = -1;
            newShip.Crew[0] = 0;
            for (int i = 0; i < MaxTradeItem; i++) G.BuyingPrice[i] = 0;

            G.Ship      = newShip;
            G.EscapePod = false;
            G.NoClaim   = 0;
            G.Credits  += insurancePayout;

            // Cost of the new Flea: 500 credits, or whatever's left + debt
            if (G.Credits > 500)
            {
                G.Credits -= 500;
            }
            else
            {
                G.Debt   += 500 - G.Credits;
                G.Credits = 0;
            }

            // Building the new ship takes 3 days
            TravelerSystem.IncDays(3);
        }

        public static int DetermineEncounter()
        {
            int sys = G.WarpSystem;
            var pol = GameData.PoliticsTypes[G.SolarSystem[sys].Politics];

            // Day-10 guard: very rare encounters shouldn't fire on the first few warps.
            if (G.Days > 10 && GetRandom(1000) < G.ChanceOfVeryRareEncounter)
            {
                int vr = GetRandom(MaxVeryRareEncounter);
                if (vr == MarieCeleste  && (G.VeryRareEncounter & AlreadyMarie) == 0)
                    return G.EncounterType = MarieCelesteEncounter;
                if (vr == CaptainAhab   && (G.VeryRareEncounter & AlreadyAhab) == 0 && G.PoliceRecordScore > CriminalScore)
                    return G.EncounterType = CaptainAhabEncounter;
                if (vr == CaptainConrad && (G.VeryRareEncounter & AlreadyConrad) == 0 && G.PoliceRecordScore > CriminalScore)
                    return G.EncounterType = CaptainConradEncounter;
                if (vr == CaptainHuie   && (G.VeryRareEncounter & AlreadyHuie) == 0 && G.PoliceRecordScore > CriminalScore)
                    return G.EncounterType = CaptainHuieEncounter;
                if (vr == BottleOld     && (G.VeryRareEncounter & AlreadyBottleOld) == 0)
                    return G.EncounterType = BottleOldEncounter;
                if (vr == BottleGood    && (G.VeryRareEncounter & AlreadyBottleGood) == 0)
                    return G.EncounterType = BottleGoodEncounter;
            }

            // Special quest ships fire at fixed click thresholds, not every click.
            if (G.Clicks == 1 && G.MonsterStatus == 1 && sys == AcamarSystem && GetRandom(100) < 85)
            {
                G.Opponent = G.SpaceMonster.Clone();
                return G.EncounterType = SpaceMonsterAttack;
            }
            if (G.Clicks == 1 && G.DragonflyStatus == 4 && sys == ZalkonSystem && GetRandom(100) < 85)
            {
                G.Opponent = G.Dragonfly_Ship.Clone();
                return G.EncounterType = DragonflyAttack;
            }
            if (G.Clicks == 20 && G.ScarabStatus == 1
                && G.SolarSystem[sys].Special == ScarabDestroyed
                && G.ArrivedViaWormhole && GetRandom(100) < 85)
            {
                G.Opponent = G.Scarab_Ship.Clone();
                return G.EncounterType = ScarabAttack;
            }

            // Single mutually-exclusive encounter roll — mirrors original Traveler.c.
            // Range shrinks with difficulty so encounters become more frequent.
            int roll = GetRandom(44 - 2 * G.Difficulty);

            // Police — strength is multiplied by crime record via PoliceStrength()
            int polStr = (!G.Inspected && pol.StrengthPolice > 0) ? PoliceStrength(sys) : 0;
            if (polStr > 0 && (roll -= polStr) < 0)
            {
                GenerateOpponent(Police);
                if (TravelerSystem.Cloaked(G.Ship, G.Opponent)) return -1;
                if (G.PoliceRecordScore < DubiousScore || G.WildStatus == 1)
                    return G.EncounterType = PoliceAttack;
                G.Inspected = true;
                return G.EncounterType = PoliceInspection;
            }

            // Pirate — Flea is less attractive prey (halved effective strength)
            int pirStr = (!G.Raided && pol.StrengthPirates > 0)
                ? (G.Ship.Type == 0 ? pol.StrengthPirates / 2 + 1 : pol.StrengthPirates)
                : 0;
            if (pirStr > 0 && (roll -= pirStr) < 0)
            {
                GenerateOpponent(Pirate);
                if (TravelerSystem.Cloaked(G.Ship, G.Opponent)) return -1;
                return G.EncounterType = PirateAttack;
            }

            // Trader — occasional trade-in-orbit offer
            if (pol.StrengthTraders > 0 && (roll -= pol.StrengthTraders) < 0)
            {
                GenerateOpponent(Trader);
                if (TravelerSystem.Cloaked(G.Ship, G.Opponent)) return -1;
                if (!G.AlwaysIgnoreTradeInOrbit && GetRandom(1000) < G.ChanceOfTradeInOrbit)
                    return G.EncounterType = TraderSell;
                return G.EncounterType = TraderIgnore;
            }

            // Mantis fires as fallback when no regular encounter occurred.
            if (G.ArtifactOnBoard && GetRandom(20) <= 3)
            {
                GenerateOpponent(Mantis);
                return G.EncounterType = Mantis;
            }

            return -1;
        }
    }
}
