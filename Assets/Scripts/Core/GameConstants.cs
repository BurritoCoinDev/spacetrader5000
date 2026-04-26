// Space Trader 5000 - Android/Unity Port
// Ported from Space Trader 1.2.2 by Pieter Spronck (GPL)
// Original: spacetrader.h

namespace SpaceTrader
{
    public static class GameConstants
    {
        // Tradeable items
        public const int MaxTradeItem    = 10;
        public const int MaxDigits       = 8;
        public const int MaxPriceDigits  = 5;
        public const int MaxQtyDigits    = 3;

        // Activity / status levels
        public const int MaxActivity     = 8;
        public const int MaxStatus       = 8;

        // System status events
        public const int Uneventful     = 0;
        public const int War            = 1;
        public const int Plague         = 2;
        public const int Drought        = 3;
        public const int Boredom        = 4;
        public const int Cold           = 5;
        public const int CropFailure    = 6;
        public const int LackOfWorkers  = 7;

        // Difficulty levels
        public const int MaxDifficulty  = 5;
        public const int Beginner       = 0;
        public const int Easy           = 1;
        public const int Normal         = 2;
        public const int Hard           = 3;
        public const int Impossible     = 4;

        // Crew / skills
        public const int MaxCrewMember  = 31;
        public const int MaxSkill       = 10;
        public const int NameLen        = 20;

        public const int PilotSkill     = 1;
        public const int FighterSkill   = 2;
        public const int TraderSkill    = 3;
        public const int EngineerSkill  = 4;

        // Trade item indices
        public const int Water      = 0;
        public const int Furs       = 1;
        public const int Food       = 2;
        public const int Ore        = 3;
        public const int Games      = 4;
        public const int Firearms   = 5;
        public const int Medicine   = 6;
        public const int Machinery  = 7;
        public const int Narcotics  = 8;
        public const int Robots     = 9;

        // Ship types
        public const int MaxShipType   = 10;
        public const int MaxRange      = 20;
        public const int ExtraShips    = 5;
        public const int MantisType    = MaxShipType + 2;
        public const int ScarabType    = MaxShipType + 3;
        public const int BottleType    = MaxShipType + 4;

        // Weapons
        public const int MaxWeaponType         = 3;
        public const int ExtraWeapons          = 1;
        public const int PulseLaserWeapon      = 0;
        public const int PulseLaserPower       = 15;
        public const int BeamLaserWeapon       = 1;
        public const int BeamLaserPower        = 25;
        public const int MilitaryLaserWeapon   = 2;
        public const int MilitaryLaserPower    = 35;
        public const int MorganLaserWeapon     = 3;
        public const int MorganLaserPower      = 85;

        // Shields
        public const int MaxShieldType     = 2;
        public const int ExtraShields      = 1;
        public const int EnergyShield      = 0;
        public const int EShieldPower      = 100;
        public const int ReflectiveShield  = 1;
        public const int RShieldPower      = 200;
        public const int LightningShield   = 2;
        public const int LShieldPower      = 350;

        public const int UpgradedHull      = 50;

        // Gadgets
        public const int MaxGadgetType     = 5;
        public const int ExtraGadgets      = 1;
        public const int ExtraBays         = 0;
        public const int AutoRepairSystem  = 1;
        public const int NavigatingSystem  = 2;
        public const int TargetingSystem   = 3;
        public const int CloakingDevice    = 4;
        public const int FuelCompactor     = 5;

        public const int MaxSkillType      = 4;
        public const int SkillBonus        = 3;
        public const int CloakBonus        = 2;

        // Police actions
        public const int Police             = 0;
        public const int PoliceInspection   = 0;
        public const int PoliceIgnore       = 1;
        public const int PoliceAttack       = 2;
        public const int PoliceFlee         = 3;
        public const int MaxPolice          = 9;

        // Pirate actions
        public const int Pirate             = 10;
        public const int PirateAttack       = 10;
        public const int PirateFlee         = 11;
        public const int PirateIgnore       = 12;
        public const int PirateSurrender    = 13;
        public const int MaxPirate          = 19;

        // Trader actions
        public const int Trader             = 20;
        public const int TraderIgnore       = 20;
        public const int TraderFlee         = 21;
        public const int TraderAttack       = 22;
        public const int TraderSurrender    = 23;
        public const int TraderSell         = 24;
        public const int TraderBuy          = 25;
        public const int TraderNoTrade      = 26;
        public const int MaxTrader          = 29;

        // Special encounter types
        public const int SpaceMonsterAttack = 30;
        public const int SpaceMonsterIgnore = 31;
        public const int MaxSpaceMonster    = 39;
        public const int DragonflyAttack    = 40;
        public const int DragonflyIgnore    = 41;
        public const int MaxDragonfly       = 49;
        public const int Mantis             = 50;
        public const int ScarabAttack       = 60;
        public const int ScarabIgnore       = 61;
        public const int MaxScarab          = 69;

        // Famous captain encounters
        public const int FamousCaptain              = 70;
        public const int FamousCaptAttack           = 71;
        public const int CaptainAhabEncounter       = 72;
        public const int CaptainConradEncounter     = 73;
        public const int CaptainHuieEncounter       = 74;
        public const int MaxFamousCaptain           = 79;

        // Other special encounters
        public const int MarieCelesteEncounter      = 80;
        public const int BottleOldEncounter         = 81;
        public const int BottleGoodEncounter        = 82;
        public const int PostMariePoliceEncounter   = 83;

        // Ship limits
        public const int MaxWeapon       = 3;
        public const int MaxShield       = 3;
        public const int MaxGadget       = 3;
        public const int MaxCrew         = 3;
        public const int MaxTribbles     = 100000;

        // Solar systems
        public const int MaxSolarSystem  = 120;
        public const int AcamarSystem    = 0;
        public const int BaratasSystem   = 6;
        public const int DaledSystem     = 17;
        public const int DevidiasSystem  = 22;
        public const int GemulonSystem   = 32;
        public const int JaporiSystem    = 41;
        public const int KravatSystem    = 50;
        public const int MelinaSystem    = 59;
        public const int NixSystem       = 67;
        public const int OgSystem        = 70;
        public const int RegulasSystem   = 82;
        public const int SolSystem       = 92;
        public const int UtopiaSystem    = 109;
        public const int ZalkonSystem    = 118;

        // Special events
        public const long CostMoon          = 500000L;
        public const int  MaxSpecialEvent   = 37;
        public const int  EndFixed          = 7;
        public const int  MaxText           = 9;

        public const int DragonflyDestroyed     = 0;
        public const int FlyBaratas             = 1;
        public const int FlyMelina              = 2;
        public const int FlyRegulas             = 3;
        public const int MonsterKilled          = 4;
        public const int MedicineDelivery       = 5;
        public const int MoonBoughtEvent        = 6;
        public const int MoonForSale            = 7;
        public const int SkillIncrease          = 8;
        public const int Tribble                = 9;
        public const int EraseRecord            = 10;
        public const int BuyTribble             = 11;
        public const int SpaceMonsterEvent      = 12;
        public const int DragonflyEvent         = 13;
        public const int CargoForSale           = 14;
        public const int InstallLightningShield = 15;
        public const int JaporiDisease          = 16;
        public const int LotteryWinner          = 17;
        public const int ArtifactDelivery       = 18;
        public const int AlienArtifact          = 19;
        public const int AmbassadorJarek        = 20;
        public const int AlienInvasion          = 21;
        public const int GemulonInvaded         = 22;
        public const int GetFuelCompactor       = 23;
        public const int Experiment             = 24;
        public const int TransportWild          = 25;
        public const int GetReactor             = 26;
        public const int GetSpecialLaser        = 27;
        public const int Scarab                 = 28;
        public const int GetHullUpgraded        = 29;
        public const int ScarabDestroyed        = 30;
        public const int ReactorDelivered       = 31;
        public const int JarekGetsOut           = 32;
        public const int GemulonRescued         = 33;
        public const int ExperimentStopped      = 34;
        public const int ExperimentNotStopped   = 35;
        public const int WildGetsOut            = 36;

        public const int TribblesOnScreen       = 31;

        // Very rare encounters
        public const int ChanceOfVeryRareEncounter  = 5;
        public const int MaxVeryRareEncounter        = 6;
        public const int MarieCeleste               = 0;
        public const int CaptainAhab                = 1;
        public const int CaptainConrad              = 2;
        public const int CaptainHuie                = 3;
        public const int BottleOld                  = 4;
        public const int BottleGood                 = 5;

        public const int AlreadyMarie       = 1;
        public const int AlreadyAhab        = 2;
        public const int AlreadyConrad      = 4;
        public const int AlreadyHuie        = 8;
        public const int AlreadyBottleOld   = 16;
        public const int AlreadyBottleGood  = 32;

        public const int ChanceOfTradeInOrbit = 100;

        // Politics
        public const int MaxPolitics    = 17;
        public const int MaxStrength    = 8;
        public const int Anarchy        = 0;

        // Tech levels
        public const int MaxTechLevel   = 8;

        // Cargo operations
        public const int SellCargo      = 1;
        public const int DumpCargo      = 2;
        public const int JettisonCargo  = 3;

        public const int MaxSize        = 5;

        // Newspaper
        public const int MaxMastheads   = 3;
        public const int MaxStories     = 4;
        public const int MaxSpecialNewsEvents = 5;

        // News events
        public const int WildArrested           = 90;
        public const int CaughtLittering        = 91;
        public const int ExperimentPerformed    = 92;
        public const int ArrivalViaSingularity  = 93;
        public const int CaptainHuieAttacked    = 100;
        public const int CaptainConradAttacked  = 101;
        public const int CaptainAhabAttacked    = 102;
        public const int CaptainHuieDestroyed   = 110;
        public const int CaptainConradDestroyed = 111;
        public const int CaptainAhabDestroyed   = 112;

        // Police record scores
        public const int MaxPoliceRecord    = 10;
        public const int AttackPoliceScore  = -3;
        public const int KillPoliceScore    = -6;
        public const int CaughtWithWildScore= -4;
        public const int AttackTraderScore  = -2;
        public const int PlunderTraderScore = -2;
        public const int KillTraderScore    = -4;
        public const int AttackPirateScore  = 0;
        public const int KillPirateScore    = 1;
        public const int PlunderPirateScore = -1;
        public const int Trafficking        = -1;
        public const int FleeFromInspection = -2;
        public const int TakeMariaNarcotics = -4;

        public const int PsychopathScore    = -70;
        public const int VillainScore       = -30;
        public const int CriminalScore      = -10;
        public const int DubiousScore       = -5;
        public const int CleanScore         = 0;
        public const int LawfulScore        = 5;
        public const int TrustedScore       = 10;
        public const int HelperScore        = 25;
        public const int HeroScore          = 75;

        // Reputation thresholds (kill counts)
        public const int MaxReputation      = 9;
        public const int HarmlessRep        = 0;
        public const int MostlyHarmlessRep  = 10;
        public const int PoorRep            = 20;
        public const int AverageScore       = 40;
        public const int AboveAverageScore  = 80;
        public const int CompetentRep       = 150;
        public const int DangerousRep       = 300;
        public const int DeadlyRep          = 600;
        public const int EliteScore         = 1500;

        // Debt limits
        public const int DebtWarning        = 75000;
        public const int DebtTooLarge       = 100000;

        // Resources
        public const int MaxResources           = 13;
        public const int NoSpecialResources     = 0;
        public const int MineralRich            = 1;
        public const int MineralPoor            = 2;
        public const int Desert                 = 3;
        public const int LotsOfWater            = 4;
        public const int RichSoil               = 5;
        public const int PoorSoil               = 6;
        public const int RichFauna              = 7;
        public const int Lifeless               = 8;
        public const int WeirdMushrooms         = 9;
        public const int LotsOfHerbs            = 10;
        public const int Artistic               = 11;
        public const int Warlike                = 12;

        // Wormholes
        public const int MaxWormhole            = 6;

        // Galaxy dimensions
        public const int GalaxyWidth            = 150;
        public const int GalaxyHeight           = 110;
        public const int ShortRangeWidth        = 140;
        public const int ShortRangeHeight       = 140;
        public const int MinDistance            = 6;
        public const int CloseDistance          = 13;
        public const int WormholeDistance       = 3;

        // Misc
        public const int FabricRipInitialProbability = 25;
        public const int MaxHighScore           = 3;
        public const int Killed                 = 0;
        public const int Retired                = 1;
        public const int Moon                   = 2;
        public const int MaxWord                = 65535;
        public const int MaxForFutureUse        = 96;
    }
}
