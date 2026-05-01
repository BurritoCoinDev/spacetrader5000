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

        // ── Ship combat statistics ────────────────────────────────────────────

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

        // ── Bounty calculation ────────────────────────────────────────────────

        public static long GetBounty(Ship sh)
        {
            long bounty = ShipPriceSystem.EnemyShipPrice(sh) / 200 / 25 * 25;
            if (bounty <= 0)  bounty = 25;
            if (bounty > 2500) bounty = 2500;
            return bounty;
        }

        // ── Police strength modifier ──────────────────────────────────────────

        public static int PoliceStrength(int systemIndex)
        {
            int base_ = GameData.PoliticsTypes[G.SolarSystem[systemIndex].Politics].StrengthPolice;
            if (G.PoliceRecordScore < PsychopathScore) return 3 * base_;
            if (G.PoliceRecordScore < VillainScore)    return 2 * base_;
            return base_;
        }

        // ── Encounter type categorisation ─────────────────────────────────────

        public static bool IsPolice(int enc)         => enc >= Police       && enc <= MaxPolice;
        public static bool IsPirate(int enc)         => enc >= Pirate       && enc <= MaxPirate;
        public static bool IsTrader(int enc)         => enc >= Trader       && enc <= MaxTrader;
        public static bool IsSpaceMonster(int enc)   => enc >= SpaceMonsterAttack && enc <= MaxSpaceMonster;
        public static bool IsDragonfly(int enc)      => enc >= DragonflyAttack    && enc <= MaxDragonfly;
        public static bool IsScarab(int enc)         => enc >= ScarabAttack       && enc <= MaxScarab;
        public static bool IsFamousCaptain(int enc)  => enc >= FamousCaptain      && enc <= MaxFamousCaptain;

        // ── Opponent generation ───────────────────────────────────────────────

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

            // Assign gadgets
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
                    while (rnd < s && g < MaxGadgetType - 1) { g++; s += GameData.Gadgettypes[g].Chance; }
                    if (!SkillSystem.HasGadget(opp, g) && g > best) best = g;
                }
                opp.Gadget[i] = best;
            }

            // Assign weapons
            int wCount = stype.WeaponSlots <= 0 ? 0 :
                         G.Difficulty <= Hard ? 1 + GetRandom(stype.WeaponSlots) : stype.WeaponSlots;
            for (int i = 0; i < wCount; i++)
            {
                int best = 0;
                for (int e = 0; e < tries; e++)
                {
                    long rnd = GetRandom(100); int w = 0; long s = GameData.Weapontypes[0].Chance;
                    while (rnd < s && w < MaxWeaponType - 1) { w++; s += GameData.Weapontypes[w].Chance; }
                    if (!SkillSystem.HasWeapon(opp, w, true) && w > best) best = w;
                }
                opp.Weapon[i] = best;
            }

            // Assign shields
            int sCount = stype.ShieldSlots <= 0 ? 0 :
                         G.Difficulty <= Hard ? GetRandom(stype.ShieldSlots + 1) : stype.ShieldSlots;
            for (int i = 0; i < sCount; i++)
            {
                int best = 0;
                for (int e = 0; e < tries; e++)
                {
                    long rnd = GetRandom(100); int sh = 0; long s = GameData.Shieldtypes[0].Chance;
                    while (rnd < s && sh < MaxShieldType - 1) { sh++; s += GameData.Shieldtypes[sh].Chance; }
                    if (!SkillSystem.HasShield(opp, sh) && sh > best) best = sh;
                }
                opp.Shield[i]         = best;
                opp.ShieldStrength[i] = GameData.Shieldtypes[best].Power;
            }

            // Fill opponent cargo
            int bays = stype.CargoBays;
            for (int i = 0; i < MaxGadget; i++) if (opp.Gadget[i] == ExtraBays) bays += 5;
            for (int i = 0; i < MaxTradeItem; i++) opp.Cargo[i] = 0;
            for (int i = 0; i < bays / 5; i++)
            {
                int item = GetRandom(MaxTradeItem);
                if (opp.Cargo[item] < bays) opp.Cargo[item] += GetRandom(5);
            }

            // Hull strength
            opp.Hull = GameData.Shiptypes[opp.Type].HullStrength;

            // Assign a random mercenary as crew
            opp.Crew[0] = 1 + GetRandom(MaxCrewMember - 1);
            G.Mercenary[opp.Crew[0]].Pilot    = SkillSystem.RandomSkill();
            G.Mercenary[opp.Crew[0]].Fighter  = SkillSystem.RandomSkill();
            G.Mercenary[opp.Crew[0]].Trader   = SkillSystem.RandomSkill();
            G.Mercenary[opp.Crew[0]].Engineer = SkillSystem.RandomSkill();
        }

        // ── One round of combat ───────────────────────────────────────────────

        // Returns true if the defender is destroyed.
        public static bool ExecuteAttack(Ship attacker, Ship defender, bool defenderFlees, bool commanderUnderAttack)
        {
            long damage = TotalWeapons(attacker, 0, MaxWeaponType + ExtraWeapons - 1);
            if (damage <= 0) return false;

            // Attacker's skill improves aim
            int fSkill = SkillSystem.FighterSkill(attacker);
            damage     = damage * (fSkill + GetRandom(MaxSkill)) / (2 * MaxSkill);

            // Fleeing target is easier to hit; big targets easier to hit
            if (defenderFlees)          damage = damage * 2 / 3;
            damage += GetRandom((int)(damage / 2 + 1)) - GetRandom((int)(damage / 2 + 1));
            damage = Max(0L, damage);

            // Apply to shields first
            long shieldsLeft = TotalShieldStrength(defender);
            if (shieldsLeft > 0)
            {
                long shieldDamage = Min(damage, shieldsLeft);
                damage -= shieldDamage;
                // Distribute shield damage proportionally
                for (int i = 0; i < MaxShield && shieldDamage > 0; i++)
                {
                    if (defender.Shield[i] < 0) continue;
                    long take = Min(defender.ShieldStrength[i], shieldDamage);
                    defender.ShieldStrength[i] -= take;
                    shieldDamage -= take;
                }
            }

            // Remaining damage to hull
            if (damage > 0)
            {
                defender.Hull -= damage;
                if (commanderUnderAttack && SkillSystem.HasGadget(defender, AutoRepairSystem))
                    defender.Hull += Max(0L, GetRandom(SkillSystem.EngineerSkill(defender)));
                if (defender.Hull <= 0) return true; // destroyed
            }

            return false;
        }

        // ── Escape pod ────────────────────────────────────────────────────────

        public static void EscapeWithPod()
        {
            if (!G.EscapePod) return;

            // Commander survives; all cargo and equipment lost
            var newShip = new Ship
            {
                Type = 0, // Flea
                Fuel = 0,
                Hull = GameData.Shiptypes[0].HullStrength,
            };
            for (int i = 0; i < MaxWeapon; i++)  newShip.Weapon[i]  = -1;
            for (int i = 0; i < MaxShield; i++)  { newShip.Shield[i] = -1; newShip.ShieldStrength[i] = 0; }
            for (int i = 0; i < MaxGadget; i++)  newShip.Gadget[i]  = -1;
            for (int i = 0; i < MaxCrew; i++)    newShip.Crew[i]    = -1;
            newShip.Crew[0] = 0;

            G.Ship      = newShip;
            G.EscapePod = false;
            G.NoClaim   = 0;

            if (G.Insurance)
            {
                G.Credits   += ShipPriceSystem.CurrentShipPriceWithoutCargo(true);
                G.Insurance  = false;
            }
            else
            {
                G.Insurance = false;
            }

            G.Commander.CurSystem = G.WarpSystem;
            TravelerSystem.Arrival();
        }

        // ── Determine encounter type for the next click ───────────────────────
        // Returns -1 if no encounter, otherwise sets G.EncounterType.

        public static int DetermineEncounter()
        {
            int sys = G.WarpSystem;
            var pol = GameData.PoliticsTypes[G.SolarSystem[sys].Politics];

            // Very rare encounters
            if (GetRandom(1000) < G.ChanceOfVeryRareEncounter)
            {
                int vr = GetRandom(MaxVeryRareEncounter);
                if (vr == MarieCeleste  && (G.VeryRareEncounter & AlreadyMarie) == 0)
                    return G.EncounterType = MarieCelesteEncounter;
                if (vr == CaptainAhab   && (G.VeryRareEncounter & AlreadyAhab) == 0 && G.PoliceRecordScore >= CleanScore)
                    return G.EncounterType = CaptainAhabEncounter;
                if (vr == CaptainConrad && (G.VeryRareEncounter & AlreadyConrad) == 0 && G.PoliceRecordScore >= CleanScore)
                    return G.EncounterType = CaptainConradEncounter;
                if (vr == CaptainHuie   && (G.VeryRareEncounter & AlreadyHuie) == 0 && G.PoliceRecordScore >= CleanScore)
                    return G.EncounterType = CaptainHuieEncounter;
                if (vr == BottleOld     && (G.VeryRareEncounter & AlreadyBottleOld) == 0)
                    return G.EncounterType = BottleOldEncounter;
                if (vr == BottleGood    && (G.VeryRareEncounter & AlreadyBottleGood) == 0)
                    return G.EncounterType = BottleGoodEncounter;
            }

            // Space monster at Acamar
            if (G.MonsterStatus == 1 && sys == AcamarSystem && GetRandom(100) < 85)
            {
                G.Opponent = G.SpaceMonster.Clone();
                return G.EncounterType = SpaceMonsterAttack;
            }

            // Dragonfly
            if (G.DragonflyStatus >= 1 && G.DragonflyStatus <= 4 && GetRandom(100) < 85)
            {
                G.Opponent = G.Dragonfly_Ship.Clone();
                return G.EncounterType = DragonflyAttack;
            }

            // Police — skip if already inspected and cleared this voyage
            if (pol.StrengthPolice > 0 && !G.Inspected && GetRandom(MaxPolice) < PoliceStrength(sys))
            {
                GenerateOpponent(Police);
                if (G.PoliceRecordScore < DubiousScore || G.WildStatus == 1)
                    return G.EncounterType = PoliceAttack;
                return G.EncounterType = PoliceInspection;
            }

            // Pirates
            if (pol.StrengthPirates > 0 && GetRandom(MaxPirate) < pol.StrengthPirates)
            {
                GenerateOpponent(Pirate);
                return G.EncounterType = PirateAttack;
            }

            // Traders
            if (pol.StrengthTraders > 0 && GetRandom(MaxTrader) < pol.StrengthTraders)
            {
                GenerateOpponent(Trader);
                return G.EncounterType = TraderIgnore;
            }

            return -1;
        }
    }
}
