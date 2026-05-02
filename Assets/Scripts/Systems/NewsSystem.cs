// Space Trader 5000 – Android/Unity Port
// Ported from Space Trader 1.2.2 by Pieter Spronck (GPL)
// Newspaper: generates context-aware headlines based on current game state.

using System.Collections.Generic;
using static SpaceTrader.GameConstants;

namespace SpaceTrader
{
    public static class NewsSystem
    {
        static GameState G => GameState.Instance;

        // Cost to buy the paper at the current system.
        public static long NewspaperPrice()
            => (G.CurrentSystem.TechLevel + 1) * 2L;

        // Newspaper-style headline for each non-Uneventful system status.
        // {0} is replaced with the system name.
        static readonly string[] StatusHeadlines =
        {
            null, // 0 Uneventful — not reported
            "Military conflict reported in the {0} system. Ore and arms prices are high.",
            "Plague outbreak in the {0} system. Medical supplies desperately needed.",
            "Severe drought gripping the {0} system. Water at a premium.",
            "Evidence suggests terrible boredom in the {0} system. Games and diversions sought.",
            "Bitter cold snap strikes the {0} system. Fur traders rushing to meet demand.",
            "Crop failure devastates the {0} system. Food prices soaring.",
            "Labour shortage cripples the {0} system. Machines and robots needed urgently.",
        };

        // Returns the list of headlines to display. Call after paying.
        public static List<string> GetHeadlines()
        {
            var lines = new List<string>();
            var sys   = G.CurrentSystem;
            var pol   = GameData.PoliticsTypes[sys.Politics];

            // ── Special/quest headlines ───────────────────────────────────────

            if (G.MonsterStatus == 1)
                lines.Add("ACAMAR TERROR: Unidentified monster reported in sector. All ships warned away.");
            else if (G.MonsterStatus == 2)
                lines.Add("ACAMAR SAFE: The creature that threatened Acamar has been destroyed.");

            if (G.DragonflyStatus >= 1 && G.DragonflyStatus <= 4)
                lines.Add("DRAGONFLY SPOTTED: Renegade vessel 'Dragonfly' evades authorities across multiple systems.");
            else if (G.DragonflyStatus >= 5)
                lines.Add("DRAGONFLY DESTROYED: The notorious Dragonfly was neutralised at Zalkon.");

            if (G.ScarabStatus == 1)
                lines.Add("SCARAB THREAT: An armoured rogue vessel has been raiding wormhole corridors.");
            else if (G.ScarabStatus >= 2)
                lines.Add("SCARAB DEFEATED: The hull-plated raider that terrorised wormhole lanes has been stopped.");

            if (G.JarekStatus == 1)
                lines.Add("AMBASSADOR JAREK MISSING: Government official Jarek has not been seen for weeks.");

            if (G.WildStatus == 1)
                lines.Add("FUGITIVE STILL AT LARGE: Jonathan Wild is wanted on multiple counts. Reward offered.");

            if (G.ReactorStatus > 0 && G.ReactorStatus < 21)
                lines.Add($"UNSTABLE REACTOR: An experimental ion reactor is en route to Nix for disposal. " +
                          $"Days remaining: {21 - G.ReactorStatus}.");

            if (G.InvasionStatus > 0 && G.InvasionStatus < 8)
                lines.Add("ALIEN INVASION: Alien forces are advancing on Gemulon. Authorities plead for help.");
            else if (G.InvasionStatus >= 8)
                lines.Add("GEMULON FALLS: Alien invasion succeeded. Gemulon has been conquered.");

            if (G.ExperimentStatus > 0 && G.ExperimentStatus < 12)
                lines.Add($"EXPERIMENT COUNTDOWN: Professor Erkfin's experiment will complete in " +
                          $"{12 - G.ExperimentStatus} days. Scientists warn of spacetime consequences.");
            else if (G.ExperimentStatus >= 12 && G.FabricRipProbability > 0)
                lines.Add("FABRIC RIP DANGER: Spacetime anomalies detected. Travel with extreme caution.");

            if (G.ArtifactOnBoard)
                lines.Add("ALIEN ARTIFACT RECOVERED: A trader recently acquired a strange alien device.");

            if (G.Ship.Tribbles > 100)
                lines.Add($"TRIBBLE INFESTATION: Customs officials on alert for ships carrying tribbles. " +
                          $"Your hold contains approximately {G.Ship.Tribbles:N0}.");

            // ── System-status headlines ──────────────────────────────────────

            if (sys.Status == War)
                lines.Add($"WAR ZONE: {GameData.SolarSystemNames[sys.NameIndex]} is in open conflict. Trade routes disrupted.");
            else if (sys.Status == Plague)
                lines.Add($"OUTBREAK: A virulent plague has struck {GameData.SolarSystemNames[sys.NameIndex]}. Medicine prices surge.");
            else if (sys.Status == Drought)
                lines.Add($"DROUGHT: Water and food are scarce in {GameData.SolarSystemNames[sys.NameIndex]}.");
            else if (sys.Status == Boredom)
                lines.Add($"ENTERTAINMENT WANTED: {GameData.SolarSystemNames[sys.NameIndex]} residents seek diversion. Games prices high.");
            else if (sys.Status == Cold)
                lines.Add($"COLD SNAP: Unusual cold has gripped {GameData.SolarSystemNames[sys.NameIndex]}. Fuel demand up.");
            else if (sys.Status == CropFailure)
                lines.Add($"CROP FAILURE: Harvests have failed on {GameData.SolarSystemNames[sys.NameIndex]}. Food prices soaring.");
            else if (sys.Status == LackOfWorkers)
                lines.Add($"LABOUR SHORTAGE: {GameData.SolarSystemNames[sys.NameIndex]} desperately needs workers and machinery.");

            // ── Other-system status reports (primary trade intelligence) ─────
            // These are the "Evidence suggests..." headlines from the original.
            for (int i = 0; i < MaxSolarSystem; i++)
            {
                if (i == G.Commander.CurSystem) continue;
                var other = G.SolarSystem[i];
                int st = other.Status;
                if (st <= Uneventful || st >= StatusHeadlines.Length) continue;
                string tpl = StatusHeadlines[st];
                if (tpl == null) continue;
                lines.Add(string.Format(tpl, GameData.SolarSystemNames[other.NameIndex]));
            }

            // ── Local price spotlight ────────────────────────────────────────
            // Find the two items with the highest sell price at this system.
            int best1 = -1, best2 = -1;
            long p1 = 0, p2 = 0;
            for (int i = 0; i < MaxTradeItem; i++)
            {
                if (G.SellPrice[i] <= 0) continue;
                if (G.SellPrice[i] > p1) { best2 = best1; p2 = p1; best1 = i; p1 = G.SellPrice[i]; }
                else if (G.SellPrice[i] > p2) { best2 = i; p2 = G.SellPrice[i]; }
            }
            if (best1 >= 0)
            {
                string spotlight = best2 >= 0
                    ? $"MARKET REPORT: {GameData.SolarSystemNames[sys.NameIndex]} buyers paying top rates for " +
                      $"{GameData.Tradeitems[best1].Name} ({p1} cr.) and " +
                      $"{GameData.Tradeitems[best2].Name} ({p2} cr.)."
                    : $"MARKET REPORT: {GameData.SolarSystemNames[sys.NameIndex]} buyers paying top rates for " +
                      $"{GameData.Tradeitems[best1].Name} ({p1} cr.).";
                lines.Add(spotlight);
            }

            // ── Commander's record ────────────────────────────────────────────

            if (G.PoliceRecordScore <= PsychopathScore)
                lines.Add("MOST WANTED: A psychopathic pirate is terrorising the spaceways. Police are mobilising.");
            else if (G.PoliceRecordScore <= VillainScore)
                lines.Add("PIRATE ALERT: A notorious criminal is active in this region.");
            else if (G.PoliceRecordScore >= HeroScore)
                lines.Add("LOCAL HERO: A celebrated trader has been spotted docking at this system.");

            // ── Generic filler if nothing else ───────────────────────────────

            if (lines.Count == 0)
                lines.Add($"TRADE REPORT: Markets on {GameData.SolarSystemNames[sys.NameIndex]} " +
                          $"are {(sys.TechLevel >= 6 ? "booming" : "stable")} today.");

            lines.Add($"PRICE WATCH: {pol.Name} authorities remind traders that " +
                      $"{(pol.DrugsOK ? "all goods" : "narcotics")} {(pol.DrugsOK ? "are" : "are banned and")} " +
                      $"{(pol.FirearmsOK ? "freely traded" : "heavily regulated")} in this system.");

            return lines;
        }
    }
}
