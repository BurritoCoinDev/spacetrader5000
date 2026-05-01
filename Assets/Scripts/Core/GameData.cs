// Space Trader 5000 - Android/Unity Port
// Ported from Space Trader 1.2.2 by Pieter Spronck (GPL)
// Original: Global.c (const arrays)

using static SpaceTrader.GameConstants;

namespace SpaceTrader
{
    public static class GameData
    {
        public static readonly PoliceRecord[] PoliceRecords =
        {
            new PoliceRecord { Name = "Psycho",   MinScore = -100 },
            new PoliceRecord { Name = "Villain",  MinScore = PsychopathScore },
            new PoliceRecord { Name = "Criminal", MinScore = VillainScore },
            new PoliceRecord { Name = "Crook",    MinScore = CriminalScore },
            new PoliceRecord { Name = "Dubious",  MinScore = DubiousScore },
            new PoliceRecord { Name = "Clean",    MinScore = CleanScore },
            new PoliceRecord { Name = "Lawful",   MinScore = LawfulScore },
            new PoliceRecord { Name = "Trusted",  MinScore = TrustedScore },
            new PoliceRecord { Name = "Liked",    MinScore = HelperScore },
            new PoliceRecord { Name = "Hero",     MinScore = HeroScore },
        };

        public static readonly Reputation[] Reputations =
        {
            new Reputation { Name = "Harmless",        MinScore = HarmlessRep },
            new Reputation { Name = "Mostly harmless", MinScore = MostlyHarmlessRep },
            new Reputation { Name = "Poor",            MinScore = PoorRep },
            new Reputation { Name = "Average",         MinScore = AverageScore },
            new Reputation { Name = "Above average",   MinScore = AboveAverageScore },
            new Reputation { Name = "Competent",       MinScore = CompetentRep },
            new Reputation { Name = "Dangerous",       MinScore = DangerousRep },
            new Reputation { Name = "Deadly",          MinScore = DeadlyRep },
            new Reputation { Name = "Elite",           MinScore = EliteScore },
        };

        public static readonly string[] DifficultyLevel =
        {
            "Beginner", "Easy", "Normal", "Hard", "Impossible"
        };

        public static readonly string[] SpecialResources =
        {
            "Nothing special",
            "Mineral rich",
            "Mineral poor",
            "Desert",
            "Sweetwater oceans",
            "Rich soil",
            "Poor soil",
            "Rich fauna",
            "Lifeless",
            "Weird mushrooms",
            "Special herbs",
            "Artistic populace",
            "Warlike populace",
        };

        public static readonly string[] StatusDescriptions =
        {
            "under no particular pressure",
            "at war",
            "ravaged by a plague",
            "suffering from a drought",
            "suffering from extreme boredom",
            "suffering from a cold spell",
            "suffering from a crop failure",
            "lacking enough workers",
        };

        public static readonly string[] ActivityDescriptions =
        {
            "Absent", "Minimal", "Few", "Some", "Moderate", "Many", "Abundant", "Swarms"
        };

        public static readonly TradeItem[] Tradeitems =
        {
            new TradeItem { Name = "Water",     TechProduction = 0, TechUsage = 0, TechTopProduction = 2,   PriceLowTech =   30, PriceInc =   +3,  Variance =   4, DoublePriceStatus = Drought,       CheapResource = LotsOfWater,    ExpensiveResource = Desert,       MinTradePrice =   30, MaxTradePrice =   50, RoundOff =   1 },
            new TradeItem { Name = "Furs",      TechProduction = 0, TechUsage = 0, TechTopProduction = 0,   PriceLowTech =  250, PriceInc =  +10,  Variance =  10, DoublePriceStatus = Cold,          CheapResource = RichFauna,      ExpensiveResource = Lifeless,      MinTradePrice =  230, MaxTradePrice =  280, RoundOff =   5 },
            new TradeItem { Name = "Food",      TechProduction = 1, TechUsage = 0, TechTopProduction = 1,   PriceLowTech =  100, PriceInc =   +5,  Variance =   5, DoublePriceStatus = CropFailure,   CheapResource = RichSoil,       ExpensiveResource = PoorSoil,      MinTradePrice =   90, MaxTradePrice =  160, RoundOff =   5 },
            new TradeItem { Name = "Ore",       TechProduction = 2, TechUsage = 2, TechTopProduction = 3,   PriceLowTech =  350, PriceInc =  +20,  Variance =  10, DoublePriceStatus = War,           CheapResource = MineralRich,    ExpensiveResource = MineralPoor,   MinTradePrice =  350, MaxTradePrice =  420, RoundOff =  10 },
            new TradeItem { Name = "Games",     TechProduction = 3, TechUsage = 1, TechTopProduction = 6,   PriceLowTech =  250, PriceInc =  -10,  Variance =   5, DoublePriceStatus = Boredom,       CheapResource = Artistic,       ExpensiveResource = -1,            MinTradePrice =  160, MaxTradePrice =  270, RoundOff =   5 },
            new TradeItem { Name = "Firearms",  TechProduction = 3, TechUsage = 1, TechTopProduction = 5,   PriceLowTech = 1250, PriceInc =  -75,  Variance = 100, DoublePriceStatus = War,           CheapResource = Warlike,        ExpensiveResource = -1,            MinTradePrice =  600, MaxTradePrice = 1100, RoundOff =  25 },
            new TradeItem { Name = "Medicine",  TechProduction = 4, TechUsage = 1, TechTopProduction = 6,   PriceLowTech =  650, PriceInc =  -20,  Variance =  10, DoublePriceStatus = Plague,        CheapResource = LotsOfHerbs,    ExpensiveResource = -1,            MinTradePrice =  400, MaxTradePrice =  700, RoundOff =  25 },
            new TradeItem { Name = "Machines",  TechProduction = 4, TechUsage = 3, TechTopProduction = 5,   PriceLowTech =  900, PriceInc =  -30,  Variance =   5, DoublePriceStatus = LackOfWorkers, CheapResource = -1,             ExpensiveResource = -1,            MinTradePrice =  600, MaxTradePrice =  800, RoundOff =  25 },
            new TradeItem { Name = "Narcotics", TechProduction = 5, TechUsage = 0, TechTopProduction = 5,   PriceLowTech = 3500, PriceInc = -125,  Variance = 150, DoublePriceStatus = Boredom,       CheapResource = WeirdMushrooms, ExpensiveResource = -1,            MinTradePrice = 2000, MaxTradePrice = 3000, RoundOff =  50 },
            new TradeItem { Name = "Robots",    TechProduction = 6, TechUsage = 4, TechTopProduction = 7,   PriceLowTech = 5000, PriceInc = -150,  Variance = 100, DoublePriceStatus = LackOfWorkers, CheapResource = -1,             ExpensiveResource = -1,            MinTradePrice = 3500, MaxTradePrice = 5000, RoundOff = 100 },
        };

        public static readonly string[] MercenaryNames =
        {
            "Jameson",   // index 0 = commander (name overridden by player)
            "Alyssa", "Armatur", "Bentos",    "C2U2",    "Chi'Ti",
            "Crystal",   "Dane",   "Deirdre",  "Doc",     "Draco",
            "Iranda",  "Jeremiah", "Jujubal",  "Krydon",  "Luis",
            "Mercedez",  "Milete",  "Muri-L",   "Mystyc",  "Nandi",
            "Orestes",   "Pancho",  "PS37",     "Quarck",  "Sosumi",
            "Uma",       "Wesley",  "Wonton",   "Yorvick", "Zeethibal",
            "FamousCaptain", // index 31 = MaxCrewMember — reserved for famous captain NPCs
        };

        // Ships indices 0-9 are buyable; 10-14 are special/enemy only
        public static readonly ShipType[] Shiptypes =
        {
            new ShipType { Name = "Flea",          CargoBays = 10, WeaponSlots = 0, ShieldSlots = 0, GadgetSlots = 0, CrewQuarters = 1, FuelTanks = MaxRange, MinTechLevel = 4, CostOfFuel =  1, Price =   2000, Bounty =   5, Occurrence =  2, HullStrength =  25, Police = -1, Pirates = -1, Traders =  0, RepairCosts = 1, Size = 0 },
            new ShipType { Name = "Gnat",          CargoBays = 15, WeaponSlots = 1, ShieldSlots = 0, GadgetSlots = 1, CrewQuarters = 1, FuelTanks = 14,       MinTechLevel = 5, CostOfFuel =  2, Price =  10000, Bounty =  50, Occurrence = 28, HullStrength = 100, Police =  0, Pirates =  0, Traders =  0, RepairCosts = 1, Size = 1 },
            new ShipType { Name = "Firefly",       CargoBays = 20, WeaponSlots = 1, ShieldSlots = 1, GadgetSlots = 1, CrewQuarters = 1, FuelTanks = 17,       MinTechLevel = 5, CostOfFuel =  3, Price =  25000, Bounty =  75, Occurrence = 20, HullStrength = 100, Police =  0, Pirates =  0, Traders =  0, RepairCosts = 1, Size = 1 },
            new ShipType { Name = "Mosquito",      CargoBays = 15, WeaponSlots = 2, ShieldSlots = 1, GadgetSlots = 1, CrewQuarters = 1, FuelTanks = 13,       MinTechLevel = 5, CostOfFuel =  5, Price =  30000, Bounty = 100, Occurrence = 20, HullStrength = 100, Police =  0, Pirates =  1, Traders =  0, RepairCosts = 1, Size = 1 },
            new ShipType { Name = "Bumblebee",     CargoBays = 25, WeaponSlots = 1, ShieldSlots = 2, GadgetSlots = 2, CrewQuarters = 2, FuelTanks = 15,       MinTechLevel = 5, CostOfFuel =  7, Price =  60000, Bounty = 125, Occurrence = 15, HullStrength = 100, Police =  1, Pirates =  1, Traders =  0, RepairCosts = 1, Size = 2 },
            new ShipType { Name = "Beetle",        CargoBays = 50, WeaponSlots = 0, ShieldSlots = 1, GadgetSlots = 1, CrewQuarters = 3, FuelTanks = 14,       MinTechLevel = 5, CostOfFuel = 10, Price =  80000, Bounty =  50, Occurrence =  3, HullStrength =  50, Police = -1, Pirates = -1, Traders =  0, RepairCosts = 1, Size = 2 },
            new ShipType { Name = "Hornet",        CargoBays = 20, WeaponSlots = 3, ShieldSlots = 2, GadgetSlots = 1, CrewQuarters = 2, FuelTanks = 16,       MinTechLevel = 6, CostOfFuel = 15, Price = 100000, Bounty = 200, Occurrence =  6, HullStrength = 150, Police =  2, Pirates =  3, Traders =  1, RepairCosts = 2, Size = 3 },
            new ShipType { Name = "Grasshopper",   CargoBays = 30, WeaponSlots = 2, ShieldSlots = 2, GadgetSlots = 3, CrewQuarters = 3, FuelTanks = 15,       MinTechLevel = 6, CostOfFuel = 15, Price = 150000, Bounty = 300, Occurrence =  2, HullStrength = 150, Police =  3, Pirates =  4, Traders =  2, RepairCosts = 3, Size = 3 },
            new ShipType { Name = "Termite",       CargoBays = 60, WeaponSlots = 1, ShieldSlots = 3, GadgetSlots = 2, CrewQuarters = 3, FuelTanks = 13,       MinTechLevel = 7, CostOfFuel = 20, Price = 225000, Bounty = 300, Occurrence =  2, HullStrength = 200, Police =  4, Pirates =  5, Traders =  3, RepairCosts = 4, Size = 4 },
            new ShipType { Name = "Wasp",          CargoBays = 35, WeaponSlots = 3, ShieldSlots = 2, GadgetSlots = 2, CrewQuarters = 3, FuelTanks = 14,       MinTechLevel = 7, CostOfFuel = 20, Price = 300000, Bounty = 500, Occurrence =  2, HullStrength = 200, Police =  5, Pirates =  6, Traders =  4, RepairCosts = 5, Size = 4 },
            // Special / enemy ships (not buyable)
            new ShipType { Name = "Space monster", CargoBays =  0, WeaponSlots = 3, ShieldSlots = 0, GadgetSlots = 0, CrewQuarters = 1, FuelTanks =  1,       MinTechLevel = 8, CostOfFuel =  1, Price = 500000, Bounty =   0, Occurrence =  0, HullStrength = 500, Police =  8, Pirates =  8, Traders =  8, RepairCosts = 1, Size = 4 },
            new ShipType { Name = "Dragonfly",     CargoBays =  0, WeaponSlots = 2, ShieldSlots = 3, GadgetSlots = 2, CrewQuarters = 1, FuelTanks =  1,       MinTechLevel = 8, CostOfFuel =  1, Price = 500000, Bounty =   0, Occurrence =  0, HullStrength =  10, Police =  8, Pirates =  8, Traders =  8, RepairCosts = 1, Size = 1 },
            new ShipType { Name = "Mantis",        CargoBays =  0, WeaponSlots = 3, ShieldSlots = 1, GadgetSlots = 3, CrewQuarters = 3, FuelTanks =  1,       MinTechLevel = 8, CostOfFuel =  1, Price = 500000, Bounty =   0, Occurrence =  0, HullStrength = 300, Police =  8, Pirates =  8, Traders =  8, RepairCosts = 1, Size = 2 },
            new ShipType { Name = "Scarab",        CargoBays = 20, WeaponSlots = 2, ShieldSlots = 0, GadgetSlots = 0, CrewQuarters = 2, FuelTanks =  1,       MinTechLevel = 8, CostOfFuel =  1, Price = 500000, Bounty =   0, Occurrence =  0, HullStrength = 400, Police =  8, Pirates =  8, Traders =  8, RepairCosts = 1, Size = 3 },
            new ShipType { Name = "Bottle",        CargoBays =  0, WeaponSlots = 0, ShieldSlots = 0, GadgetSlots = 0, CrewQuarters = 0, FuelTanks =  1,       MinTechLevel = 8, CostOfFuel =  1, Price =    100, Bounty =   0, Occurrence =  0, HullStrength =  10, Police =  8, Pirates =  8, Traders =  8, RepairCosts = 1, Size = 1 },
        };

        public static readonly Weapon[] Weapontypes =
        {
            new Weapon { Name = "Pulse laser",    Power = PulseLaserPower,    Price =  2000, TechLevel = 5, Chance = 50 },
            new Weapon { Name = "Beam laser",     Power = BeamLaserPower,     Price = 12500, TechLevel = 6, Chance = 35 },
            new Weapon { Name = "Military laser", Power = MilitaryLaserPower, Price = 35000, TechLevel = 7, Chance = 15 },
            new Weapon { Name = "Morgan's laser", Power = MorganLaserPower,   Price = 50000, TechLevel = 8, Chance =  0 },
        };

        public static readonly Shield[] Shieldtypes =
        {
            new Shield { Name = "Energy shield",     Power = EShieldPower,  Price =  5000, TechLevel = 5, Chance = 70 },
            new Shield { Name = "Reflective shield", Power = RShieldPower,  Price = 20000, TechLevel = 6, Chance = 30 },
            new Shield { Name = "Lightning shield",  Power = LShieldPower,  Price = 45000, TechLevel = 8, Chance =  0 },
        };

        public static readonly Gadget[] Gadgettypes =
        {
            new Gadget { Name = "5 extra cargo bays",   Price =   2500, TechLevel = 4, Chance = 35 },
            new Gadget { Name = "Auto-repair system",   Price =   7500, TechLevel = 5, Chance = 20 },
            new Gadget { Name = "Navigating system",    Price =  15000, TechLevel = 6, Chance = 20 },
            new Gadget { Name = "Targeting system",     Price =  25000, TechLevel = 6, Chance = 20 },
            new Gadget { Name = "Cloaking device",      Price = 100000, TechLevel = 7, Chance =  5 },
            new Gadget { Name = "Fuel compactor",       Price =  30000, TechLevel = 8, Chance =  0 },
        };

        public static readonly string[] SystemSize =
        {
            "Tiny", "Small", "Medium", "Large", "Huge"
        };

        public static readonly string[] TechLevelNames =
        {
            "Pre-agricultural", "Agricultural", "Medieval", "Renaissance",
            "Early Industrial", "Industrial",   "Post-industrial", "Hi-tech"
        };

        public static readonly Politics[] PoliticsTypes =
        {
            new Politics { Name = "Anarchy",          ReactionIllegal = 0, StrengthPolice = 0, StrengthPirates = 7, StrengthTraders = 1, MinTechLevel = 0, MaxTechLevel = 5, BribeLevel = 7, DrugsOK = true,  FirearmsOK = true,  Wanted = Food     },
            new Politics { Name = "Capitalist State", ReactionIllegal = 2, StrengthPolice = 3, StrengthPirates = 2, StrengthTraders = 7, MinTechLevel = 4, MaxTechLevel = 7, BribeLevel = 1, DrugsOK = true,  FirearmsOK = true,  Wanted = Ore      },
            new Politics { Name = "Communist State",  ReactionIllegal = 6, StrengthPolice = 6, StrengthPirates = 4, StrengthTraders = 4, MinTechLevel = 1, MaxTechLevel = 5, BribeLevel = 5, DrugsOK = true,  FirearmsOK = true,  Wanted = -1       },
            new Politics { Name = "Confederacy",      ReactionIllegal = 5, StrengthPolice = 4, StrengthPirates = 3, StrengthTraders = 5, MinTechLevel = 1, MaxTechLevel = 6, BribeLevel = 3, DrugsOK = true,  FirearmsOK = true,  Wanted = Games    },
            new Politics { Name = "Corporate State",  ReactionIllegal = 2, StrengthPolice = 6, StrengthPirates = 2, StrengthTraders = 7, MinTechLevel = 4, MaxTechLevel = 7, BribeLevel = 2, DrugsOK = true,  FirearmsOK = true,  Wanted = Robots   },
            new Politics { Name = "Cybernetic State", ReactionIllegal = 0, StrengthPolice = 7, StrengthPirates = 7, StrengthTraders = 5, MinTechLevel = 6, MaxTechLevel = 7, BribeLevel = 0, DrugsOK = false, FirearmsOK = false, Wanted = Ore      },
            new Politics { Name = "Democracy",        ReactionIllegal = 4, StrengthPolice = 3, StrengthPirates = 2, StrengthTraders = 5, MinTechLevel = 3, MaxTechLevel = 7, BribeLevel = 2, DrugsOK = true,  FirearmsOK = true,  Wanted = Games    },
            new Politics { Name = "Dictatorship",     ReactionIllegal = 3, StrengthPolice = 4, StrengthPirates = 5, StrengthTraders = 3, MinTechLevel = 0, MaxTechLevel = 7, BribeLevel = 2, DrugsOK = true,  FirearmsOK = true,  Wanted = -1       },
            new Politics { Name = "Fascist State",    ReactionIllegal = 7, StrengthPolice = 7, StrengthPirates = 7, StrengthTraders = 1, MinTechLevel = 4, MaxTechLevel = 7, BribeLevel = 0, DrugsOK = false, FirearmsOK = true,  Wanted = Machinery},
            new Politics { Name = "Feudal State",     ReactionIllegal = 1, StrengthPolice = 1, StrengthPirates = 6, StrengthTraders = 2, MinTechLevel = 0, MaxTechLevel = 3, BribeLevel = 6, DrugsOK = true,  FirearmsOK = true,  Wanted = Firearms },
            new Politics { Name = "Military State",   ReactionIllegal = 7, StrengthPolice = 7, StrengthPirates = 0, StrengthTraders = 6, MinTechLevel = 2, MaxTechLevel = 7, BribeLevel = 0, DrugsOK = false, FirearmsOK = true,  Wanted = Robots   },
            new Politics { Name = "Monarchy",         ReactionIllegal = 3, StrengthPolice = 4, StrengthPirates = 3, StrengthTraders = 4, MinTechLevel = 0, MaxTechLevel = 5, BribeLevel = 4, DrugsOK = true,  FirearmsOK = true,  Wanted = Medicine },
            new Politics { Name = "Pacifist State",   ReactionIllegal = 7, StrengthPolice = 2, StrengthPirates = 1, StrengthTraders = 5, MinTechLevel = 0, MaxTechLevel = 3, BribeLevel = 1, DrugsOK = true,  FirearmsOK = false, Wanted = -1       },
            new Politics { Name = "Socialist State",  ReactionIllegal = 4, StrengthPolice = 2, StrengthPirates = 5, StrengthTraders = 3, MinTechLevel = 0, MaxTechLevel = 5, BribeLevel = 6, DrugsOK = true,  FirearmsOK = true,  Wanted = -1       },
            new Politics { Name = "State of Satori",  ReactionIllegal = 0, StrengthPolice = 1, StrengthPirates = 1, StrengthTraders = 1, MinTechLevel = 0, MaxTechLevel = 1, BribeLevel = 0, DrugsOK = false, FirearmsOK = false, Wanted = -1       },
            new Politics { Name = "Technocracy",      ReactionIllegal = 1, StrengthPolice = 6, StrengthPirates = 3, StrengthTraders = 6, MinTechLevel = 4, MaxTechLevel = 7, BribeLevel = 2, DrugsOK = true,  FirearmsOK = true,  Wanted = Water    },
            new Politics { Name = "Theocracy",        ReactionIllegal = 5, StrengthPolice = 6, StrengthPirates = 1, StrengthTraders = 4, MinTechLevel = 0, MaxTechLevel = 4, BribeLevel = 0, DrugsOK = true,  FirearmsOK = true,  Wanted = Narcotics},
        };

        public static readonly string[] SolarSystemNames =
        {
            "Acamar",   "Adahn",      "Aldea",    "Andevian", "Antedi",     "Balosnee",
            "Baratas",  "Brax",       "Bretel",   "Calondia", "Campor",     "Capelle",
            "Carzon",   "Castor",     "Cestus",   "Cheron",   "Courteney",  "Daled",
            "Damast",   "Davlos",     "Deneb",    "Deneva",   "Devidia",    "Draylon",
            "Drema",    "Endor",      "Esmee",    "Exo",      "Ferris",     "Festen",
            "Fourmi",   "Frolix",     "Gemulon",  "Guinifer", "Hades",      "Hamlet",
            "Helena",   "Hulst",      "Iodine",   "Iralius",  "Janus",      "Japori",
            "Jarada",   "Jason",      "Kaylon",   "Khefka",   "Kira",       "Klaatu",
            "Klaestron","Korma",      "Kravat",   "Krios",    "Laertes",    "Largo",
            "Lave",     "Ligon",      "Lowry",    "Magrat",   "Malcoria",   "Melina",
            "Mentar",   "Merik",      "Mintaka",  "Montor",   "Mordan",     "Myrthe",
            "Nelvana",  "Nix",        "Nyle",     "Odet",     "Og",         "Omega",
            "Omphalos", "Orias",      "Othello",  "Parade",   "Penthara",   "Picard",
            "Pollux",   "Quator",     "Rakhar",   "Ran",      "Regulas",    "Relva",
            "Rhymus",   "Rochani",    "Rubicum",  "Rutia",    "Sarpeidon",  "Sefalla",
            "Seltrice", "Sigma",      "Sol",      "Somari",   "Stakoron",   "Styris",
            "Talani",   "Tamus",      "Tantalos", "Tanuga",   "Tarchannen", "Terosa",
            "Thera",    "Titan",      "Torin",    "Triacus",  "Turkana",    "Tyrus",
            "Umberlee", "Utopia",     "Vadera",   "Vagra",    "Vandor",     "Ventax",
            "Xenon",    "Xerxes",     "Yew",      "Yojimbo",  "Zalkon",     "Zuul",
        };
    }
}
