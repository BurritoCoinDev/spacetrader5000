// Space Trader 5000 – Android/Unity Port
// Encounter screen: combat and non-combat encounter resolution.

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SpaceTrader.Persistence;
using static SpaceTrader.GameConstants;

namespace SpaceTrader.UI.Screens
{
    public class EncounterUI : MonoBehaviour, IScreenUI
    {
        TextMeshProUGUI _titleText, _descText;
        TextMeshProUGUI _playerHullText, _oppHullText;
        TextMeshProUGUI _playerShieldText, _oppShieldText;
        Transform _btnContainer;
        readonly List<Button> _actionBtns = new();

        // Combat state
        bool _playerFleeing;
        bool _oppFleeing;

        public void Initialize(GameObject panel)
        {
            UIFactory.Stretch(panel.GetComponent<RectTransform>());

            // Title bar (no back button during encounter)
            var titleBar = UIFactory.Panel(panel.transform, "TitleBar", ColorTheme.HeaderBg);
            UIFactory.SetAnchored(titleBar.GetComponent<RectTransform>(),
                new Vector2(0, 0.90f), Vector2.one, Vector2.zero, Vector2.zero);

            _titleText = UIFactory.Label(titleBar.transform, "Title", "ENCOUNTER",
                ColorTheme.FontHeader, ColorTheme.TextAccent, TextAlignmentOptions.Center);
            UIFactory.Stretch(_titleText.rectTransform, 8, 8, 4, 4);

            // Opponent status
            var oppPanel = UIFactory.Panel(panel.transform, "OppPanel", ColorTheme.PanelBg);
            UIFactory.SetAnchored(oppPanel.GetComponent<RectTransform>(),
                new Vector2(0, 0.74f), new Vector2(1, 0.90f), new Vector2(8, 4), new Vector2(-8, -4));

            _oppHullText = UIFactory.Label(oppPanel.transform, "OppHull", "",
                ColorTheme.FontSmall, ColorTheme.TextNegative, TextAlignmentOptions.Left);
            UIFactory.SetAnchored(_oppHullText.rectTransform,
                new Vector2(0, 0), new Vector2(0.5f, 0.5f), new Vector2(8, 4), new Vector2(-4, -4));

            _oppShieldText = UIFactory.Label(oppPanel.transform, "OppShield", "",
                ColorTheme.FontSmall, ColorTheme.TextSecondary, TextAlignmentOptions.Right);
            UIFactory.SetAnchored(_oppShieldText.rectTransform,
                new Vector2(0.5f, 0), new Vector2(1, 0.5f), new Vector2(4, 4), new Vector2(-8, -4));

            // Description
            _descText = UIFactory.LabelWrap(panel.transform, "Desc", "",
                ColorTheme.FontSmall, ColorTheme.TextPrimary, TextAlignmentOptions.Left);
            UIFactory.SetAnchored(_descText.rectTransform,
                new Vector2(0.02f, 0.40f), new Vector2(0.98f, 0.74f), new Vector2(4, 4), new Vector2(-4, -4));

            // Player status
            var plyPanel = UIFactory.Panel(panel.transform, "PlyPanel", ColorTheme.PanelBg);
            UIFactory.SetAnchored(plyPanel.GetComponent<RectTransform>(),
                new Vector2(0, 0.30f), new Vector2(1, 0.40f), new Vector2(8, 4), new Vector2(-8, -4));

            _playerHullText = UIFactory.Label(plyPanel.transform, "PlyHull", "",
                ColorTheme.FontSmall, ColorTheme.HullFill, TextAlignmentOptions.Left);
            UIFactory.SetAnchored(_playerHullText.rectTransform,
                new Vector2(0, 0), new Vector2(0.5f, 1), new Vector2(8, 4), new Vector2(-4, -4));

            _playerShieldText = UIFactory.Label(plyPanel.transform, "PlyShield", "",
                ColorTheme.FontSmall, ColorTheme.TextSecondary, TextAlignmentOptions.Right);
            UIFactory.SetAnchored(_playerShieldText.rectTransform,
                new Vector2(0.5f, 0), new Vector2(1, 1), new Vector2(4, 4), new Vector2(-8, -4));

            // Action buttons
            _btnContainer = UIFactory.TransparentPanel(panel.transform, "Btns").transform;
            UIFactory.SetAnchored(_btnContainer.GetComponent<RectTransform>(),
                new Vector2(0, 0.01f), new Vector2(1, 0.30f), new Vector2(8, 4), new Vector2(-8, -4));
            var glg = _btnContainer.gameObject.AddComponent<GridLayoutGroup>();
            glg.cellSize        = new Vector2(490, 80);
            glg.spacing         = new Vector2(10, 10);
            glg.padding         = new RectOffset(4, 4, 4, 4);
            glg.constraint      = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = 2;
        }

        public void OnShow()
        {
            _playerFleeing = false;
            _oppFleeing    = false;
            BuildActions();
            RefreshStatus();
        }

        void RefreshStatus()
        {
            var G   = GameState.Instance;
            var opp = G.Opponent;
            var st  = GameData.Shiptypes[opp.Type];

            long oppHull    = opp.Hull;
            long oppMaxHull = st.HullStrength;
            long oppShield  = EncounterSystem.TotalShieldStrength(opp);

            _oppHullText.text    = $"Opp Hull: {oppHull}/{oppMaxHull}";
            _oppShieldText.text  = oppShield > 0 ? $"Shield: {oppShield}" : "";

            long plyHull    = G.Ship.Hull;
            long plyMaxHull = ShipyardSystem.GetHullStrength();
            long plyShield  = EncounterSystem.TotalShieldStrength(G.Ship);

            _playerHullText.text   = $"Your Hull: {plyHull}/{plyMaxHull}";
            _playerShieldText.text = plyShield > 0 ? $"Shield: {plyShield}" : "";
            _playerHullText.color  = plyHull < plyMaxHull / 4 ? ColorTheme.TextNegative : ColorTheme.HullFill;
        }

        void BuildActions()
        {
            foreach (Transform child in _btnContainer) Destroy(child.gameObject);
            _actionBtns.Clear();

            var G   = GameState.Instance;
            int enc = G.EncounterType;
            string oppName = GameData.Shiptypes[G.Opponent.Type].Name;

            _titleText.text  = EncounterTitle(enc);
            _descText.text   = EncounterDescription(enc);

            bool isCombatStart = IsCombatEncounter(enc);
            bool canFight      = EncounterSystem.TotalWeapons(G.Ship, 0, MaxWeaponType + ExtraWeapons - 1) > 0;
            bool hasPod        = G.EscapePod;

            if (isCombatStart)
            {
                AddBtn("ATTACK",  OnAttack,  ColorTheme.ButtonDanger);
                AddBtn("FLEE",    OnFlee,    ColorTheme.ButtonNormal);
                if (hasPod) AddBtn("ESCAPE POD", OnEscapePod, ColorTheme.ButtonWarning);
                AddBtn("YIELD",   OnYield,   ColorTheme.ButtonNormal);
            }
            else if (enc == PoliceInspection)
            {
                AddBtn("SUBMIT",      OnSubmitInspection, ColorTheme.ButtonNormal);
                AddBtn("FLEE",        OnFlee,             ColorTheme.ButtonNormal);
                AddBtn("BRIBE",       OnBribe,            ColorTheme.ButtonNormal);
                if (canFight) AddBtn("ATTACK", OnAttack,  ColorTheme.ButtonDanger);
            }
            else if (EncounterSystem.IsTrader(enc))
            {
                AddBtn("IGNORE",    OnIgnore,    ColorTheme.ButtonNormal);
                if (canFight) AddBtn("ATTACK", OnAttack, ColorTheme.ButtonDanger);
            }
            else if (enc == MarieCelesteEncounter)
            {
                AddBtn("IGNORE",     OnIgnore,            ColorTheme.ButtonNormal);
                AddBtn("TAKE CARGO", OnTakeMarieCargo,    ColorTheme.ButtonSuccess);
            }
            else if (enc == CaptainAhabEncounter || enc == CaptainConradEncounter || enc == CaptainHuieEncounter)
            {
                AddBtn("ACCEPT",  OnFamousCaptainAccept,  ColorTheme.ButtonSuccess);
                AddBtn("IGNORE",  OnIgnore,               ColorTheme.ButtonNormal);
                if (canFight) AddBtn("ATTACK", OnAttack,  ColorTheme.ButtonDanger);
            }
            else if (enc == BottleOldEncounter || enc == BottleGoodEncounter)
            {
                AddBtn("DRINK",  OnBottle,  ColorTheme.ButtonSuccess);
                AddBtn("IGNORE", OnIgnore,  ColorTheme.ButtonNormal);
            }
            else
            {
                AddBtn("CONTINUE", OnIgnore, ColorTheme.ButtonNormal);
            }
        }

        void AddBtn(string label, UnityEngine.Events.UnityAction action, Color bgColor)
        {
            var btn = UIFactory.Btn(_btnContainer, label, label, action, bgColor);
            _actionBtns.Add(btn);
        }

        bool IsCombatEncounter(int enc)
            => enc == PoliceAttack || EncounterSystem.IsPirate(enc)
            || EncounterSystem.IsSpaceMonster(enc) || EncounterSystem.IsDragonfly(enc)
            || EncounterSystem.IsScarab(enc) || enc == FamousCaptAttack
            || enc == TraderAttack;

        void OnAttack()
        {
            var G   = GameState.Instance;
            var opp = G.Opponent;

            // Player attacks opponent
            bool oppDestroyed = EncounterSystem.ExecuteAttack(G.Ship, opp, _oppFleeing, false);
            if (oppDestroyed)
            {
                OnOpponentDestroyed();
                return;
            }

            // Opponent attacks player
            bool playerDestroyed = EncounterSystem.ExecuteAttack(opp, G.Ship, _playerFleeing, true);
            if (playerDestroyed)
            {
                OnPlayerDestroyed();
                return;
            }

            _playerFleeing = false;
            _oppFleeing    = false;
            RefreshStatus();
        }

        void OnFlee()
        {
            var G   = GameState.Instance;
            int pil = SkillSystem.PilotSkill(G.Ship);
            int oppPil = SkillSystem.PilotSkill(G.Opponent);

            if (GameMath.GetRandom(pil + oppPil) >= oppPil)
            {
                // Escaped
                ShowResult("You escaped!", () => ReturnToTravel());
                return;
            }

            // Failed to flee — opponent gets a free shot
            _playerFleeing = true;
            bool destroyed = EncounterSystem.ExecuteAttack(G.Opponent, G.Ship, false, true);
            if (destroyed) { OnPlayerDestroyed(); return; }
            _playerFleeing = false;
            if (EncounterSystem.IsPolice(G.EncounterType))
                G.PoliceRecordScore += FleeFromInspection;
            RefreshStatus();
        }

        void OnYield()
        {
            var G = GameState.Instance;
            if (EncounterSystem.IsPirate(G.EncounterType))
            {
                // Pirates take all cargo when they have enough space (original behavior).
                // BuyingPrice must also be zeroed — it tracks purchase price per item.
                for (int i = 0; i < MaxTradeItem; i++)
                {
                    G.Ship.Cargo[i]     = 0;
                    G.BuyingPrice[i]    = 0;
                }
                G.PoliceRecordScore += PlunderPirateScore;
                ShowResult("Pirates took your cargo.", () => ReturnToTravel());
            }
            else
            {
                ShowResult("You yielded.", () => ReturnToTravel());
            }
        }

        void OnEscapePod()
        {
            EncounterSystem.EscapeWithPod();
            ShowResult("Escape pod launched! You survived.", () => UIManager.Instance.Navigate(GameScreen.Docked));
        }

        void OnSubmitInspection()
        {
            var G = GameState.Instance;
            bool hasCont = false;
            for (int i = 0; i < MaxTradeItem; i++)
                if (G.Ship.Cargo[i] > 0 && (i == Narcotics || i == Firearms)) { hasCont = true; break; }

            G.Inspected = true;
            if (hasCont)
            {
                if (G.Ship.Cargo[Narcotics] > 0) G.PoliceRecordScore += Trafficking;
                if (G.Ship.Cargo[Firearms]  > 0) G.PoliceRecordScore += Trafficking;
                // Confiscate contraband
                G.Ship.Cargo[Narcotics] = 0;
                G.Ship.Cargo[Firearms]  = 0;
                ShowResult("Police confiscated contraband. Your record suffers.", () => ReturnToTravel());
            }
            else
            {
                ShowResult("Inspection passed. Nothing found.", () => ReturnToTravel());
            }
        }

        void OnBribe()
        {
            var G     = GameState.Instance;
            long bribe = G.Credits / 10;
            if (bribe <= 0) { ShowResult("You have nothing to offer.", () => { BuildActions(); }); return; }
            G.Credits -= bribe;
            ShowResult($"You bribed the police with {UIFactory.Cr(bribe)}.", () => ReturnToTravel());
        }

        void OnIgnore()
        {
            ReturnToTravel();
        }

        void OnTakeMarieCargo()
        {
            var G = GameState.Instance;
            G.VeryRareEncounter |= AlreadyMarie;
            // Fill hold with narcotics (Marie Celeste is full of them)
            int space = CargoSystem.FreeCargoBays();
            if (space > 0)
            {
                int qty = GameMath.Min(space, 5);
                G.Ship.Cargo[Narcotics] += qty;
                G.PoliceRecordScore     += TakeMariaNarcotics;
            }
            ShowResult("You took cargo from the Marie Celeste.", () => ReturnToTravel());
        }

        void OnFamousCaptainAccept()
        {
            var G   = GameState.Instance;
            int enc = G.EncounterType;
            if (enc == CaptainAhabEncounter)
            {
                G.VeryRareEncounter |= AlreadyAhab;
                SkillSystem.TonicTweakRandomSkill();
                ShowResult("Captain Ahab shared a skill tonic with you!", () => ReturnToTravel());
            }
            else if (enc == CaptainConradEncounter)
            {
                G.VeryRareEncounter |= AlreadyConrad;
                SkillSystem.IncreaseRandomSkill();
                ShowResult("Captain Conrad improved one of your skills!", () => ReturnToTravel());
            }
            else if (enc == CaptainHuieEncounter)
            {
                G.VeryRareEncounter |= AlreadyHuie;
                SkillSystem.IncreaseRandomSkill();
                ShowResult("Captain Huie improved one of your skills!", () => ReturnToTravel());
            }
        }

        void OnBottle()
        {
            var G   = GameState.Instance;
            int enc = G.EncounterType;
            if (enc == BottleOldEncounter)
            {
                G.VeryRareEncounter |= AlreadyBottleOld;
                SkillSystem.DecreaseRandomSkill(1);
                ShowResult("The old bottle tasted terrible. A skill decreased.", () => ReturnToTravel());
            }
            else
            {
                G.VeryRareEncounter |= AlreadyBottleGood;
                SkillSystem.IncreaseRandomSkill();
                ShowResult("The bottle of Eden improved a skill!", () => ReturnToTravel());
            }
        }

        void OnOpponentDestroyed()
        {
            var G   = GameState.Instance;
            int enc = G.EncounterType;

            long bounty = EncounterSystem.GetBounty(G.Opponent);
            G.Credits   += bounty;
            G.ReputationScore++;

            if (EncounterSystem.IsPolice(enc))   G.PoliceRecordScore += KillPoliceScore;
            if (EncounterSystem.IsPirate(enc))   G.PoliceRecordScore += KillPirateScore;
            if (EncounterSystem.IsTrader(enc))   G.PoliceRecordScore += KillTraderScore;

            ShowResult($"You destroyed the {GameData.Shiptypes[G.Opponent.Type].Name}!\nBounty: {UIFactory.Cr(bounty)}",
                () => ReturnToTravel());
        }

        void OnPlayerDestroyed()
        {
            var G = GameState.Instance;
            if (G.EscapePod)
            {
                EncounterSystem.EscapeWithPod();
                ShowResult("Your ship was destroyed!\nEscape pod saved you.", () => UIManager.Instance.Navigate(GameScreen.Docked));
            }
            else
            {
                TravelerSystem.TryRecordHighScore(Killed);
                ShowResult("Your ship was DESTROYED!\nGame over.", () => {
                    SaveSystem.DeleteSave();
                    UIManager.Instance.Navigate(GameScreen.HighScores);
                });
            }
        }

        void ShowResult(string message, System.Action onOK)
        {
            foreach (Transform child in _btnContainer) Destroy(child.gameObject);
            _actionBtns.Clear();
            _descText.text = message;

            var okBtn = UIFactory.Btn(_btnContainer, "OK", "OK", () => onOK(), ColorTheme.ButtonNormal);
            _actionBtns.Add(okBtn);
        }

        void ReturnToTravel()
        {
            UIManager.Instance.Navigate(GameScreen.Travel);
        }

        string EncounterTitle(int enc)
        {
            if (EncounterSystem.IsPolice(enc))       return "POLICE";
            if (EncounterSystem.IsPirate(enc))        return "PIRATES";
            if (EncounterSystem.IsTrader(enc))        return "TRADER";
            if (EncounterSystem.IsSpaceMonster(enc)) return "SPACE MONSTER";
            if (EncounterSystem.IsDragonfly(enc))    return "THE DRAGONFLY";
            if (EncounterSystem.IsScarab(enc))       return "THE SCARAB";
            if (EncounterSystem.IsFamousCaptain(enc)) return "FAMOUS CAPTAIN";
            if (enc == MarieCelesteEncounter)         return "MARIE CELESTE";
            if (enc == BottleOldEncounter || enc == BottleGoodEncounter) return "DRIFTING BOTTLE";
            return "ENCOUNTER";
        }

        string EncounterDescription(int enc)
        {
            var opp = GameData.Shiptypes[GameState.Instance.Opponent.Type].Name;
            if (enc == PoliceInspection)          return $"A {opp} requests an inspection.";
            if (enc == PoliceAttack)              return $"A {opp} attacks you!";
            if (EncounterSystem.IsPirate(enc))    return $"A {opp} attacks — pirates!";
            if (EncounterSystem.IsTrader(enc))    return $"A {opp} passes by.";
            if (EncounterSystem.IsSpaceMonster(enc)) return "A massive Space Monster blocks your path!";
            if (EncounterSystem.IsDragonfly(enc)) return "The Dragonfly cuts you off!";
            if (EncounterSystem.IsScarab(enc))    return "The Scarab swoops to attack!";
            if (enc == CaptainAhabEncounter)      return "Captain Ahab hails you and offers a skill tonic.";
            if (enc == CaptainConradEncounter)    return "Captain Conrad hails you and offers to train you.";
            if (enc == CaptainHuieEncounter)      return "Captain Huie hails you and offers to train you.";
            if (enc == MarieCelesteEncounter)     return "The Marie Celeste drifts silently. Its hold looks full.";
            if (enc == BottleOldEncounter)        return "A sealed bottle drifts past. It smells questionable.";
            if (enc == BottleGoodEncounter)       return "A sealed bottle drifts past. It smells wonderful.";
            return "Something unusual happens.";
        }
    }
}
