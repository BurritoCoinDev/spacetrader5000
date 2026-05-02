// Space Trader 5000 – Android/Unity Port
// Special Event screen: quest/event dialogs at a system.

using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static SpaceTrader.GameConstants;

namespace SpaceTrader.UI.Screens
{
    public class SpecialEventUI : MonoBehaviour, IScreenUI
    {
        TextMeshProUGUI _titleText, _descText;
        Button _acceptBtn, _declineBtn;

        public void Initialize(GameObject panel)
        {
            UIFactory.Stretch(panel.GetComponent<RectTransform>());
            UIFactory.Header(panel.transform, "SPECIAL EVENT",
                () => UIManager.Instance.NavigateBack());

            var body = UIFactory.Panel(panel.transform, "Body", ColorTheme.PanelBg);
            UIFactory.SetAnchored(body.GetComponent<RectTransform>(),
                new Vector2(0.04f, 0.40f), new Vector2(0.96f, 0.87f),
                Vector2.zero, Vector2.zero);

            _titleText = UIFactory.Label(body.transform, "Title", "",
                ColorTheme.FontBody, ColorTheme.TextAccent, TextAlignmentOptions.Center);
            UIFactory.SetAnchored(_titleText.rectTransform,
                new Vector2(0, 0.82f), Vector2.one, new Vector2(8, 0), new Vector2(-8, 0));

            _descText = UIFactory.LabelWrap(body.transform, "Desc", "",
                ColorTheme.FontSmall, ColorTheme.TextPrimary, TextAlignmentOptions.Left);
            UIFactory.SetAnchored(_descText.rectTransform,
                new Vector2(0, 0), new Vector2(1, 0.80f), new Vector2(8, 8), new Vector2(-8, -8));

            _acceptBtn = UIFactory.Btn(panel.transform, "Accept", "ACCEPT",
                OnAccept, ColorTheme.ButtonSuccess, ColorTheme.FontButton);
            UIFactory.SetAnchored(_acceptBtn.GetComponent<RectTransform>(),
                new Vector2(0.05f, 0.22f), new Vector2(0.48f, 0.38f), Vector2.zero, Vector2.zero);

            _declineBtn = UIFactory.Btn(panel.transform, "Decline", "DECLINE",
                OnDecline, ColorTheme.ButtonNormal, ColorTheme.FontButton);
            UIFactory.SetAnchored(_declineBtn.GetComponent<RectTransform>(),
                new Vector2(0.52f, 0.22f), new Vector2(0.95f, 0.38f), Vector2.zero, Vector2.zero);
        }

        public void OnShow()
        {
            var G   = GameState.Instance;
            int evt = G.CurrentSystem.Special;

            // Gate events on quest prerequisites; treat gated events as absent
            if (!IsEventAvailable(G, evt)) evt = -1;

            if (evt < 0)
            {
                _titleText.text = "No Event";
                _descText.text  = "Nothing special here.";
                _acceptBtn.gameObject.SetActive(false);
                _declineBtn.GetComponentInChildren<TextMeshProUGUI>().text = "LEAVE";
                return;
            }

            _acceptBtn.gameObject.SetActive(true);
            _declineBtn.GetComponentInChildren<TextMeshProUGUI>().text = "DECLINE";
            PopulateEvent(evt);
        }

        static bool IsEventAvailable(GameState G, int evt)
        {
            switch (evt)
            {
                case DragonflyDestroyed:    return G.DragonflyStatus >= 5;
                case FlyBaratas:            return G.DragonflyStatus == 1;
                case FlyMelina:             return G.DragonflyStatus == 2;
                case FlyRegulas:            return G.DragonflyStatus == 3;
                case MonsterKilled:         return G.MonsterStatus == 2;
                case ScarabDestroyed:       return G.ScarabStatus == 2;
                case GetHullUpgraded:       return G.ScarabStatus == 2 && !G.HullUpgraded;
                case AmbassadorJarek:       return G.JarekStatus == 0;
                case JarekGetsOut:          return G.JarekStatus == 2;
                case TransportWild:         return G.WildStatus == 0;
                case GetReactor:            return G.ReactorStatus == 0;
                // Player must be carrying an active reactor (1..20) to deliver
                case ReactorDelivered:      return G.ReactorStatus > 0 && G.ReactorStatus < 21;
                case GetSpecialLaser:       return G.ReactorStatus == 21;
                case GetFuelCompactor:      return true;
                case InstallLightningShield:return true;
                case EraseRecord:           return true;
                case WildGetsOut:           return G.WildStatus == 1;
                case JaporiDisease:         return G.JaporiDiseaseStatus == 0
                                                && CargoSystem.FreeCargoBays() >= 10;
                case AlienArtifact:         return !G.ArtifactOnBoard;
                case ArtifactDelivery:      return G.ArtifactOnBoard;
                case MedicineDelivery:      return G.JaporiDiseaseStatus == 1;
                default:                    return true;
            }
        }

        void PopulateEvent(int evt)
        {
            switch (evt)
            {
                case DragonflyDestroyed:
                    _titleText.text = "Dragonfly Destroyed";
                    _descText.text  = "The Dragonfly has been eliminated. A grateful government rewards you.";
                    break;
                case FlyBaratas:
                    _titleText.text = "Unusual Ship";
                    _descText.text  = "A small, fast ship called the Dragonfly has been spotted near Baratas. It carries a special shield. Will you hunt it?";
                    break;
                case FlyMelina:
                    _titleText.text = "The Dragonfly";
                    _descText.text  = "The Dragonfly was last seen heading toward Melina. Continue pursuit?";
                    break;
                case FlyRegulas:
                    _titleText.text = "The Dragonfly";
                    _descText.text  = "The Dragonfly was sighted near Regulas. Will you intercept?";
                    break;
                case MonsterKilled:
                    _titleText.text = "Monster Killed";
                    _descText.text  = "You have slain the Space Monster! The system is safe again.";
                    break;
                case MedicineDelivery:
                    _titleText.text = "Antidote Delivered";
                    _descText.text  = "The antidote arrives just in time. The Japori government rewards you with two skill increases.";
                    break;
                case JaporiDisease:
                    _titleText.text = "Plague on Japori";
                    _descText.text  = "Doctors plead for an antidote to be carried to Japori. It will fill 10 cargo bays. Accept?";
                    break;
                case AlienArtifact:
                    _titleText.text = "Alien Artifact";
                    _descText.text  = "A trader offers you a strange alien artifact. It may attract Mantis attention. Accept?";
                    break;
                case ArtifactDelivery:
                    _titleText.text = "Artifact Delivered";
                    _descText.text  = "Researchers gratefully accept the alien artifact. Mantis attacks will cease.";
                    break;
                case GetSpecialLaser:
                    _titleText.text = "Morgan's Laser";
                    _descText.text  = "Doctor Morgan presents you with his unique laser as thanks for delivering the reactor.";
                    break;
                case InstallLightningShield:
                    _titleText.text = "Lightning Shield";
                    _descText.text  = "An engineer offers to install the lightning shield recovered from the Dragonfly. Accept?";
                    break;
                case EraseRecord:
                    _titleText.text = "Erase Record";
                    _descText.text  = "A skilled hacker offers to erase your criminal record. Accept the offer?";
                    break;
                case WildGetsOut:
                    _titleText.text = "Wild Departs";
                    _descText.text  = "Jonathan Wild leaves your ship at Kravat. He sends Zeethibal as thanks — find him here.";
                    break;
                case MoonBoughtEvent:
                    _titleText.text = "Moon For Sale";
                    _descText.text  = "A small moon is for sale for 500,000 credits. Buy it and retire in style!";
                    break;
                case AmbassadorJarek:
                    _titleText.text = "Ambassador Jarek";
                    _descText.text  = "Ambassador Jarek needs passage. Will you transport him?";
                    break;
                case GetFuelCompactor:
                    _titleText.text = "Fuel Compactor";
                    _descText.text  = "A retired engineer offers you a fuel compactor gadget. Accept it?";
                    break;
                case TransportWild:
                    _titleText.text = "Jonathan Wild";
                    _descText.text  = "Jonathan Wild is a wanted fugitive. Will you smuggle him past the police?";
                    break;
                case GetReactor:
                    _titleText.text = "Unstable Reactor";
                    _descText.text  = "A huge, unstable reactor is available. It will damage your cargo capacity. Accept it?";
                    break;
                case SpaceMonsterEvent:
                    _titleText.text = "Space Monster";
                    _descText.text  = "A gigantic space monster has been terrorizing the Acamar system. Will you hunt it?";
                    break;
                case DragonflyEvent:
                    _titleText.text = "The Dragonfly";
                    _descText.text  = "A rogue ship called the Dragonfly, armed with a unique lightning shield, has been spotted. Will you hunt it down?";
                    break;
                case Scarab:
                    _titleText.text = "The Scarab";
                    _descText.text  = "A specially-hulled ship called the Scarab is causing trouble. Will you deal with it?";
                    break;
                case ScarabDestroyed:
                    _titleText.text = "Scarab Destroyed";
                    _descText.text  = "You have destroyed the Scarab! An engineer will now upgrade your hull plating.";
                    break;
                case GetHullUpgraded:
                    _titleText.text = "Hull Upgrade";
                    _descText.text  = "An engineer can upgrade your hull for free. Accept the upgrade?";
                    break;
                case JarekGetsOut:
                    _titleText.text = "Jarek Gets Out";
                    _descText.text  = "Ambassador Jarek thanks you for the safe passage to Daled. His contacts improve your trading reputation.";
                    break;
                case ReactorDelivered:
                    _titleText.text = "Reactor Delivered";
                    _descText.text  = "The Nix government thanks you for delivering the reactor. They offer a reward.";
                    break;
                default:
                    _titleText.text = "Special Event";
                    _descText.text  = $"A special event (#{evt}) occurs at this system.";
                    break;
            }
        }

        void OnAccept()
        {
            var G   = GameState.Instance;
            int evt = G.CurrentSystem.Special;
            ApplyEvent(evt);
            // Only consume the event if the handler didn't already set a new
            // Special on this system (e.g. ReactorDelivered queues
            // GetSpecialLaser as a follow-up). Without this guard, a quest
            // that no-ops due to unmet preconditions (insufficient credits /
            // no free slot / no cargo space) would be permanently lost.
            if (G.CurrentSystem.Special == evt) G.CurrentSystem.Special = -1;
            UIManager.Instance.NavigateBack();
        }

        void OnDecline()
        {
            UIManager.Instance.NavigateBack();
        }

        void ApplyEvent(int evt)
        {
            var G = GameState.Instance;
            switch (evt)
            {
                case MoonBoughtEvent:
                    if (G.Credits >= 500000)
                    {
                        G.Credits -= 500000;
                        G.MoonBought = true;
                        TravelerSystem.TryRecordHighScore(Moon);
                    }
                    break;

                case SpaceMonsterEvent:
                    G.MonsterStatus = 1;
                    break;

                case DragonflyEvent:
                    G.DragonflyStatus = 1;
                    break;

                case FlyBaratas:
                    if (G.DragonflyStatus == 1) G.DragonflyStatus = 2;
                    break;

                case FlyMelina:
                    if (G.DragonflyStatus == 2) G.DragonflyStatus = 3;
                    break;

                case FlyRegulas:
                    if (G.DragonflyStatus == 3) G.DragonflyStatus = 4;
                    break;

                case MonsterKilled:
                    G.Credits += 2000L * (G.Difficulty + 1);
                    G.PoliceRecordScore += 3;
                    break;

                case DragonflyDestroyed:
                    // Reward isn't a Military Laser — original re-points the
                    // system to InstallLightningShield so the player can pick
                    // up the Lightning Shield on a follow-up visit.
                    G.CurrentSystem.Special = InstallLightningShield;
                    break;

                case Scarab:
                    G.ScarabStatus = 1;
                    break;

                case ScarabDestroyed:
                    // Hull upgrade event placed somewhere random by TravelerSystem; nothing more to do here
                    break;

                case AmbassadorJarek:
                    G.JarekStatus = 1;
                    break;

                case GetFuelCompactor:
                {
                    // Free reward gadget — bypass InstallGadget which charges credits
                    var gs = ShipyardSystem.GetFirstEmptySlot(
                        GameData.Shiptypes[G.Ship.Type].GadgetSlots, G.Ship.Gadget);
                    if (gs >= 0) G.Ship.Gadget[gs] = FuelCompactor;
                    break;
                }

                case TransportWild:
                    G.WildStatus = 1;
                    break;

                case GetReactor:
                    G.ReactorStatus = 1;
                    break;

                case GetHullUpgraded:
                    G.Ship.Hull = GameMath.Min(G.Ship.Hull + UpgradedHull, ShipyardSystem.GetHullStrength() + UpgradedHull);
                    G.HullUpgraded = true;
                    G.ScarabStatus = 3;   // matches original — gates EscapeWithPod cleanup and Scarab hull cap
                    break;

                case JarekGetsOut:
                    G.JarekStatus = 2;
                    // Trader-skill cap bump changes the price ceiling for
                    // goods at this system, so prices must be recalculated.
                    SkillSystem.RecalculateBuyPrices(G.Commander.CurSystem);
                    break;

                case WildGetsOut:
                    {
                        // Place Zeethibal (the famous-captain slot) at Kravat,
                        // record completion, and clear the player's record.
                        G.WildStatus = 2;
                        G.Mercenary[MaxCrewMember].CurSystem = KravatSystem;
                        if (G.PoliceRecordScore < CleanScore) G.PoliceRecordScore = CleanScore;
                    }
                    break;

                case JaporiDisease:
                    {
                        // Pick up the antidote — occupies 10 cargo bays until
                        // delivery (TotalCargoBays handles the deduction).
                        G.JaporiDiseaseStatus = 1;
                    }
                    break;

                case AlienArtifact:
                    G.ArtifactOnBoard = true;
                    break;

                case ArtifactDelivery:
                    G.ArtifactOnBoard = false;
                    // Modest credit reward + reputation bump for completing
                    // the artifact-delivery side quest.
                    G.Credits += 20000;
                    G.ReputationScore += 5;
                    break;

                case ReactorDelivered:
                    // Mark delivery and queue the Morgan Laser pickup at this system.
                    G.ReactorStatus = 21;
                    G.CurrentSystem.Special = GetSpecialLaser;
                    break;

                case GetSpecialLaser:
                    {
                        // Install Morgan's Laser into the first empty weapon slot.
                        for (int i = 0; i < MaxWeapon; i++)
                        {
                            if (G.Ship.Weapon[i] < 0)
                            {
                                G.Ship.Weapon[i] = MorganLaserWeapon;
                                break;
                            }
                        }
                        G.ReactorStatus = -1;   // fully complete
                    }
                    break;

                case InstallLightningShield:
                    {
                        for (int i = 0; i < MaxShield; i++)
                        {
                            if (G.Ship.Shield[i] < 0)
                            {
                                G.Ship.Shield[i] = LightningShield;
                                G.Ship.ShieldStrength[i] = GameData.Shieldtypes[LightningShield].Power;
                                break;
                            }
                        }
                    }
                    break;

                case EraseRecord:
                    G.PoliceRecordScore = CleanScore;
                    break;

                case MedicineDelivery:
                    // Frees the 10 antidote bays and grants two skill bumps
                    // (the reward for completing the Japori quest).
                    G.JaporiDiseaseStatus = 2;
                    SkillSystem.IncreaseRandomSkill();
                    SkillSystem.IncreaseRandomSkill();
                    break;
            }
        }
    }
}
