// Space Trader 5000 - Android/Unity Port
// Ported from Space Trader 1.2.2 by Pieter Spronck (GPL)
// Original: Global.c (mutable globals) and DataTypes.h (SAVEGAMETYPE)

using UnityEngine;
using static SpaceTrader.GameConstants;

namespace SpaceTrader
{
    // Central singleton holding all mutable game state.
    // Mirrors the SAVEGAMETYPE struct and the loose globals from Global.c.
    public class GameState : MonoBehaviour
    {
        public static GameState Instance { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Reset();
        }

        // ── Persistent state (saved / loaded) ────────────────────────────────

        public long  Credits            = 1000;
        public long  Debt               = 0;
        public int   Days               = 0;
        public int   WarpSystem         = 0;
        public int   SelectedShipType   = 0;

        public long[] BuyPrice          = new long[MaxTradeItem];
        public long[] SellPrice         = new long[MaxTradeItem];
        public long[] BuyingPrice       = new long[MaxTradeItem];
        public long[] ShipPrice         = new long[MaxShipType];

        public int   GalacticChartSystem    = 0;
        public long  PoliceKills            = 0;
        public long  TraderKills            = 0;
        public long  PirateKills            = 0;
        public long  PoliceRecordScore      = 0;
        public long  ReputationScore        = 0;

        public bool  AutoFuel               = false;
        public bool  AutoRepair             = false;
        public int   Clicks                 = 0;   // clicks remaining until arrival (0 = arrived)
        public int   EncounterType          = 0;
        public bool  Raided                 = false;

        public int   MonsterStatus          = 0;
        public int   DragonflyStatus        = 0;
        public int   JaporiDiseaseStatus    = 0;
        public bool  MoonBought             = false;
        public long  MonsterHull            = 500;

        public string NameCommander         = "Jameson";
        public int   CurForm                = 0;

        public Ship        Ship             = new Ship();
        public Ship        Opponent         = new Ship();
        public CrewMember[]Mercenary        = new CrewMember[MaxCrewMember + 1];
        public SolarSystem[]SolarSystem     = new SolarSystem[MaxSolarSystem];

        public bool  EscapePod              = false;
        public bool  Insurance              = false;
        public int   NoClaim                = 0;
        public bool  Inspected              = false;
        public bool  AlwaysIgnoreTraders    = false;
        public int[] Wormhole               = new int[MaxWormhole];
        public int   Difficulty             = Normal;

        public bool  ArtifactOnBoard        = false;
        public bool  ReserveMoney           = false;
        public bool  PriceDifferences       = false;
        public int   LeaveEmpty             = 0;
        public bool  TribbleMessage         = false;
        public bool  AlwaysInfo             = false;
        public bool  AlwaysIgnorePolice     = true;
        public bool  AlwaysIgnorePirates    = false;
        public bool  TextualEncounters      = false;
        public int   JarekStatus            = 0;
        public int   InvasionStatus         = 0;
        public bool  Continuous             = false;
        public bool  AttackFleeing          = false;
        public int   ExperimentStatus       = 0;
        public int   FabricRipProbability   = 0;
        public int   VeryRareEncounter      = 0;
        public int   ReactorStatus          = 0;  // 0=none, 1-20=active, -1=delivered
        public int   TrackedSystem          = -1;
        public int   ScarabStatus           = 0;
        public bool  AlwaysIgnoreTradeInOrbit = false;
        public bool  AlreadyPaidForNewspaper  = false;
        public bool  GameLoaded             = false;
        public bool  LitterWarning          = false;
        public bool  SharePreferences       = true;
        public int   WildStatus             = 0;
        public bool  HullUpgraded           = false;

        public int   Shortcut1              = 0;
        public int   Shortcut2              = 1;
        public int   Shortcut3              = 2;
        public int   Shortcut4              = 10;

        // ── Runtime-only state ────────────────────────────────────────────────

        public int   CheatCounter           = 0;
        public int   NewsSpecialEventCount  = 0;
        public int   ChanceOfVeryRareEncounter  = GameConstants.ChanceOfVeryRareEncounter;
        public int   ChanceOfTradeInOrbit       = GameConstants.ChanceOfTradeInOrbit;

        public bool  AutoAttack             = false;
        public bool  AutoFlee               = false;
        public bool  AttackIconStatus       = false;
        public bool  PossibleToGoThroughRip = false;
        public bool  NewsAutoPay            = false;
        public bool  ShowTrackedRange       = true;
        public bool  JustLootedMarie        = false;
        public bool  ArrivedViaWormhole     = false;
        public bool  TrackAutoOff           = true;
        public bool  RemindLoans            = true;
        public bool  CanSuperWarp           = false;

        public HighScore[] Hscores          = new HighScore[MaxHighScore];

        // Special enemy ships (initialized by Reset / StartNewGame)
        public Ship  SpaceMonster           = new Ship();
        public Ship  Scarab_Ship            = new Ship();
        public Ship  Dragonfly_Ship         = new Ship();

        // ── Convenience accessors ─────────────────────────────────────────────

        public CrewMember Commander => Mercenary[0];
        public SolarSystem CurrentSystem => SolarSystem[Commander.CurSystem];

        // ── Initialization ────────────────────────────────────────────────────

        public void Reset()
        {
            Credits             = 1000;
            Debt                = 0;
            Days                = 0;
            PoliceKills         = 0;
            TraderKills         = 0;
            PirateKills         = 0;
            PoliceRecordScore   = 0;
            ReputationScore     = 0;
            MonsterHull         = 500;
            NameCommander       = "Jameson";
            Difficulty          = Normal;
            GameLoaded          = false;

            // Quest / story flags
            MonsterStatus       = 0;
            DragonflyStatus     = 0;
            JaporiDiseaseStatus = 0;
            MoonBought          = false;
            JarekStatus         = 0;
            InvasionStatus      = 0;
            ExperimentStatus    = 0;
            FabricRipProbability= 0;
            VeryRareEncounter   = 0;
            ReactorStatus       = 0;
            ScarabStatus        = 0;
            WildStatus          = 0;
            HullUpgraded        = false;
            ArtifactOnBoard     = false;

            // Ship / bank state
            EscapePod           = false;
            Insurance           = false;
            NoClaim             = 0;

            // Travel state
            Clicks              = 0;
            WarpSystem          = 0;
            TrackedSystem       = -1;
            Raided              = false;
            Inspected           = false;
            LitterWarning       = false;
            AlreadyPaidForNewspaper = false;
            ArrivedViaWormhole  = false;
            PossibleToGoThroughRip = false;

            // Runtime flags that could leak between games
            EncounterType       = 0;
            JustLootedMarie     = false;
            CanSuperWarp        = false;
            CheatCounter        = 0;
            NewsSpecialEventCount = 0;

            for (int i = 0; i < MaxTradeItem; i++)
            {
                BuyPrice[i]     = 0;
                SellPrice[i]    = 0;
                BuyingPrice[i]  = 0;
            }
            for (int i = 0; i < MaxShipType; i++)
                ShipPrice[i]    = 0;

            for (int i = 0; i < Hscores.Length; i++)
                Hscores[i]      = new HighScore();

            InitMercenaries();
            InitSolarSystems();
            InitPlayerShip();
            InitSpecialShips();
        }

        void InitMercenaries()
        {
            Mercenary = new CrewMember[MaxCrewMember + 1];
            for (int i = 0; i <= MaxCrewMember; i++)
                Mercenary[i] = new CrewMember { NameIndex = i };
        }

        void InitSolarSystems()
        {
            SolarSystem = new SolarSystem[MaxSolarSystem];
            for (int i = 0; i < MaxSolarSystem; i++)
                SolarSystem[i] = new SolarSystem { NameIndex = i };
        }

        void InitPlayerShip()
        {
            Ship = new Ship
            {
                Type = 1,    // Gnat
                Fuel = 14,
                Hull = 100,
                Tribbles = 0,
            };
            for (int i = 0; i < MaxWeapon; i++)  Ship.Weapon[i]  = -1;
            for (int i = 0; i < MaxShield; i++)  Ship.Shield[i]  = -1;
            for (int i = 0; i < MaxGadget; i++)  Ship.Gadget[i]  = -1;
            for (int i = 0; i < MaxCrew; i++)    Ship.Crew[i]    = -1;
            Ship.Weapon[0] = 0;   // pulse laser
            Ship.Crew[0]   = 0;   // commander aboard
        }

        void InitSpecialShips()
        {
            SpaceMonster = new Ship { Type = MaxShipType, Fuel = 1, Hull = 500, Tribbles = 0 };
            SpaceMonster.Weapon[0] = SpaceMonster.Weapon[1] = SpaceMonster.Weapon[2] = MilitaryLaserWeapon;
            for (int i = 0; i < MaxShield; i++) SpaceMonster.Shield[i] = -1;
            for (int i = 0; i < MaxGadget; i++) SpaceMonster.Gadget[i] = -1;
            SpaceMonster.Crew[0] = MaxCrewMember;
            for (int i = 1; i < MaxCrew; i++) SpaceMonster.Crew[i] = -1;

            Dragonfly_Ship = new Ship { Type = MaxShipType + 1, Fuel = 1, Hull = 10, Tribbles = 0 };
            Dragonfly_Ship.Weapon[0] = MilitaryLaserWeapon;
            Dragonfly_Ship.Weapon[1] = PulseLaserWeapon;
            Dragonfly_Ship.Weapon[2] = -1;
            for (int i = 0; i < MaxShield; i++) { Dragonfly_Ship.Shield[i] = LightningShield; Dragonfly_Ship.ShieldStrength[i] = LShieldPower; }
            Dragonfly_Ship.Gadget[0] = AutoRepairSystem;
            Dragonfly_Ship.Gadget[1] = TargetingSystem;
            Dragonfly_Ship.Gadget[2] = -1;
            Dragonfly_Ship.Crew[0] = MaxCrewMember;
            for (int i = 1; i < MaxCrew; i++) Dragonfly_Ship.Crew[i] = -1;

            Scarab_Ship = new Ship { Type = MaxShipType + 3, Fuel = 1, Hull = 400, Tribbles = 0 };
            Scarab_Ship.Weapon[0] = Scarab_Ship.Weapon[1] = MilitaryLaserWeapon;
            Scarab_Ship.Weapon[2] = -1;
            for (int i = 0; i < MaxShield; i++) Scarab_Ship.Shield[i] = -1;
            for (int i = 0; i < MaxGadget; i++) Scarab_Ship.Gadget[i] = -1;
            Scarab_Ship.Crew[0] = MaxCrewMember;
            for (int i = 1; i < MaxCrew; i++) Scarab_Ship.Crew[i] = -1;
        }
    }
}
