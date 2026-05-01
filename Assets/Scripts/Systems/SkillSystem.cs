// Space Trader 5000 - Android/Unity Port
// Ported from Space Trader 1.2.2 by Pieter Spronck (GPL)
// Original: Skill.c

using static SpaceTrader.GameConstants;
using static SpaceTrader.GameMath;

namespace SpaceTrader
{
    public static class SkillSystem
    {
        static GameState G => GameState.Instance;

        // ── Per-skill accessors ─────────────────────────────────────────────────────

        public static int TraderSkill(Ship sh)
        {
            int max = G.Mercenary[sh.Crew[0]].Trader;
            for (int i = 1; i < MaxCrew; i++)
            {
                if (sh.Crew[i] < 0) continue;
                if (G.Mercenary[sh.Crew[i]].Trader > max)
                    max = G.Mercenary[sh.Crew[i]].Trader;
            }
            if (G.JarekStatus >= 2) max++;
            return AdaptDifficulty(max);
        }

        public static int FighterSkill(Ship sh)
        {
            int max = G.Mercenary[sh.Crew[0]].Fighter;
            for (int i = 1; i < MaxCrew; i++)
            {
                if (sh.Crew[i] < 0) continue;
                if (G.Mercenary[sh.Crew[i]].Fighter > max)
                    max = G.Mercenary[sh.Crew[i]].Fighter;
            }
            if (HasGadget(sh, TargetingSystem)) max += SkillBonus;
            return AdaptDifficulty(max);
        }

        public static int PilotSkill(Ship sh)
        {
            int max = G.Mercenary[sh.Crew[0]].Pilot;
            for (int i = 1; i < MaxCrew; i++)
            {
                if (sh.Crew[i] < 0) continue;
                if (G.Mercenary[sh.Crew[i]].Pilot > max)
                    max = G.Mercenary[sh.Crew[i]].Pilot;
            }
            if (HasGadget(sh, NavigatingSystem)) max += SkillBonus;
            if (HasGadget(sh, CloakingDevice))   max += CloakBonus;
            return AdaptDifficulty(max);
        }

        public static int EngineerSkill(Ship sh)
        {
            int max = G.Mercenary[sh.Crew[0]].Engineer;
            for (int i = 1; i < MaxCrew; i++)
            {
                if (sh.Crew[i] < 0) continue;
                if (G.Mercenary[sh.Crew[i]].Engineer > max)
                    max = G.Mercenary[sh.Crew[i]].Engineer;
            }
            if (HasGadget(sh, AutoRepairSystem)) max += SkillBonus;
            return AdaptDifficulty(max);
        }

        public static int AdaptDifficulty(int level)
        {
            if (G.Difficulty == Beginner || G.Difficulty == Easy) return level + 1;
            if (G.Difficulty == Impossible) return Max(1, level - 1);
            return level;
        }

        public static int RandomSkill() => 1 + GetRandom(5) + GetRandom(6);

        // ── Equipment queries ─────────────────────────────────────────────────────

        public static bool HasGadget(Ship sh, int gadget)
        {
            for (int i = 0; i < MaxGadget; i++)
                if (sh.Gadget[i] == gadget) return true;
            return false;
        }

        public static bool HasShield(Ship sh, int shieldType)
        {
            for (int i = 0; i < MaxShield; i++)
                if (sh.Shield[i] == shieldType) return true;
            return false;
        }

        public static bool HasWeapon(Ship sh, int weaponType, bool exactMatch)
        {
            for (int i = 0; i < MaxWeapon; i++)
            {
                if (sh.Weapon[i] < 0) continue;
                if (sh.Weapon[i] == weaponType || (!exactMatch && sh.Weapon[i] > weaponType))
                    return true;
            }
            return false;
        }

        // Returns the skill index (0=Pilot,1=Fighter,2=Trader,3=Engineer) of the nth-lowest skill.
        public static int NthLowestSkill(Ship sh, int n)
        {
            var cmdr = G.Mercenary[sh.Crew[0]];
            int lower = 1;
            for (int i = 1; i <= MaxSkill; i++)
            {
                if (cmdr.Pilot    == i) { if (lower++ == n) return 0; }
                if (cmdr.Fighter  == i) { if (lower++ == n) return 1; }
                if (cmdr.Trader   == i) { if (lower++ == n) return 2; }
                if (cmdr.Engineer == i) { if (lower++ == n) return 3; }
            }
            return 0;
        }

        // ── Skill mutation ────────────────────────────────────────────────────────

        public static void IncreaseRandomSkill()
        {
            var cmdr = G.Commander;
            if (cmdr.Pilot >= MaxSkill && cmdr.Trader >= MaxSkill &&
                cmdr.Fighter >= MaxSkill && cmdr.Engineer >= MaxSkill) return;

            int oldTrader = TraderSkill(G.Ship);
            int d;
            do { d = GetRandom(MaxSkillType); }
            while ((d == 0 && cmdr.Pilot >= MaxSkill)    ||
                   (d == 1 && cmdr.Fighter >= MaxSkill)  ||
                   (d == 2 && cmdr.Trader >= MaxSkill)   ||
                   (d == 3 && cmdr.Engineer >= MaxSkill));

            if (d == 0) cmdr.Pilot++;
            else if (d == 1) cmdr.Fighter++;
            else if (d == 2)
            {
                cmdr.Trader++;
                if (oldTrader != TraderSkill(G.Ship))
                    RecalculateBuyPrices(G.Commander.CurSystem);
            }
            else cmdr.Engineer++;
        }

        public static void DecreaseRandomSkill(int amount)
        {
            var cmdr = G.Commander;
            if (cmdr.Pilot <= amount && cmdr.Fighter <= amount &&
                cmdr.Trader <= amount && cmdr.Engineer <= amount) return;

            int d;
            do { d = GetRandom(MaxSkillType); }
            while ((d == 0 && cmdr.Pilot <= amount)    ||
                   (d == 1 && cmdr.Fighter <= amount)  ||
                   (d == 2 && cmdr.Trader <= amount)   ||
                   (d == 3 && cmdr.Engineer <= amount));

            int oldTrader = TraderSkill(G.Ship);
            if (d == 0) cmdr.Pilot -= amount;
            else if (d == 1) cmdr.Fighter -= amount;
            else if (d == 2)
            {
                cmdr.Trader -= amount;
                if (oldTrader != TraderSkill(G.Ship))
                    RecalculateBuyPrices(G.Commander.CurSystem);
            }
            else cmdr.Engineer -= amount;
        }

        public static void TonicTweakRandomSkill()
        {
            var cmdr = G.Commander;
            int oldPilot = cmdr.Pilot, oldFighter = cmdr.Fighter,
                oldTrader = cmdr.Trader, oldEngineer = cmdr.Engineer;

            if (G.Difficulty < Hard)
            {
                while (cmdr.Pilot == oldPilot && cmdr.Fighter == oldFighter &&
                       cmdr.Trader == oldTrader && cmdr.Engineer == oldEngineer)
                {
                    IncreaseRandomSkill();
                    DecreaseRandomSkill(1);
                }
            }
            else
            {
                IncreaseRandomSkill();
                IncreaseRandomSkill();
                DecreaseRandomSkill(3);
            }
        }

        // ── Price recalculation ─────────────────────────────────────────────────

        public static void RecalculateBuyPrices(int systemID)
        {
            var sys = G.SolarSystem[systemID];
            for (int i = 0; i < MaxTradeItem; i++)
            {
                var item = GameData.Tradeitems[i];
                if (sys.TechLevel < item.TechProduction)
                {
                    G.BuyPrice[i] = 0;
                    continue;
                }
                if ((i == Narcotics && !GameData.PoliticsTypes[sys.Politics].DrugsOK) ||
                    (i == Firearms  && !GameData.PoliticsTypes[sys.Politics].FirearmsOK))
                {
                    G.BuyPrice[i] = 0;
                    continue;
                }
                long sell = G.SellPrice[i];
                long buy  = G.PoliceRecordScore < DubiousScore ? (sell * 100) / 90 : sell;
                buy = buy * (103 + (MaxSkill - TraderSkill(G.Ship))) / 100;
                if (buy <= sell) buy = sell + 1;
                G.BuyPrice[i] = buy;
            }
        }

        public static void RecalculateSellPrices()
        {
            for (int i = 0; i < MaxTradeItem; i++)
                G.SellPrice[i] = (G.SellPrice[i] * 100) / 90;
        }

        public static int DifficultyRange(int low, int high)
            => low + G.Difficulty * (high - low) / (MaxDifficulty - 1);
    }
}
