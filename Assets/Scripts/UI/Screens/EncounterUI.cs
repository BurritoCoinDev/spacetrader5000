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
        TextMeshProUGUI _playerNameText, _oppNameText;
        TextMeshProUGUI _playerHullText, _oppHullText;
        TextMeshProUGUI _playerShieldText, _oppShieldText;
        Image _playerShipImg, _oppShipImg;
        Transform _btnContainer;
        readonly List<Button> _actionBtns = new();

        // Placeholder colors per ship type (indexed by Shiptypes index).
        // Quest ships and out-of-range fall back to a neutral purple.
        // To be replaced when real ship sprites are imported.
        static readonly Color[] ShipColors =
        {
            new Color(0.65f, 0.65f, 0.70f),  // 0  Flea
            new Color(0.30f, 0.75f, 0.30f),  // 1  Gnat
            new Color(0.95f, 0.85f, 0.20f),  // 2  Firefly
            new Color(0.85f, 0.30f, 0.30f),  // 3  Mosquito
            new Color(0.95f, 0.55f, 0.20f),  // 4  Bumblebee
            new Color(0.55f, 0.40f, 0.25f),  // 5  Beetle
            new Color(0.85f, 0.65f, 0.20f),  // 6  Hornet
            new Color(0.50f, 0.85f, 0.55f),  // 7  Grasshopper
            new Color(0.65f, 0.20f, 0.20f),  // 8  Termite
            new Color(1.00f, 0.95f, 0.40f),  // 9  Wasp
        };

        static Color ShipColor(int type)
            => (type >= 0 && type < ShipColors.Length)
               ? ShipColors[type]
               : new Color(0.55f, 0.30f, 0.85f);

        // Sprite cache so each combat round doesn't re-create Sprites.
        static readonly Dictionary<int, Sprite> _spriteCache = new();

        // Loads ship sprite at Assets/Resources/Ships/ship{type}.png. Tries
        // Sprite first; if the asset was imported as plain Texture2D (the
        // default when .meta files aren't checked in), creates a Sprite at
        // runtime from the texture.
        static Sprite LoadShipSprite(int type)
        {
            if (_spriteCache.TryGetValue(type, out var cached)) return cached;
            Sprite sp = Resources.Load<Sprite>($"Ships/ship{type}");
            if (sp == null)
            {
                var tex = Resources.Load<Texture2D>($"Ships/ship{type}");
                if (tex != null)
                {
                    tex.filterMode = FilterMode.Point;  // crisp pixel art
                    sp = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                                       new Vector2(0.5f, 0.5f), 100f);
                }
            }
            _spriteCache[type] = sp;
            return sp;
        }

        static void ApplyShipImage(Image img, int type)
        {
            var sp = LoadShipSprite(type);
            if (sp != null)
            {
                img.sprite         = sp;
                img.color          = Color.white;
                img.preserveAspect = true;
            }
            else
            {
                img.sprite = null;
                img.color  = ShipColor(type);
            }
        }

        // Combat state
        bool _playerFleeing;
        bool _oppFleeing;
        bool _hasStruckFirst;   // tracks whether the player has already taken
                                // an offensive action this encounter (so the
                                // peaceful-attack record penalty applies once).

        // Trader buy overlay
        GameObject _traderDialog;
        Transform  _traderContent;

        public void Initialize(GameObject panel)
        {
            UIFactory.Stretch(panel.GetComponent<RectTransform>());

            // Title bar (no back button during encounter)
            var titleBar = UIFactory.Panel(panel.transform, "TitleBar", ColorTheme.HeaderBg);
            UIFactory.SetAnchored(titleBar.GetComponent<RectTransform>(),
                new Vector2(0, 0.92f), Vector2.one, Vector2.zero, Vector2.zero);

            _titleText = UIFactory.Label(titleBar.transform, "Title", "ENCOUNTER",
                ColorTheme.FontHeader, ColorTheme.TextAccent, TextAlignmentOptions.Center);
            UIFactory.Stretch(_titleText.rectTransform, 8, 8, 4, 4);

            // Two ship cards side by side: player on the left, opponent on the right
            BuildShipCard(panel.transform, "PlyCard",
                new Vector2(0.02f, 0.55f), new Vector2(0.49f, 0.91f),
                out _playerShipImg, out _playerNameText, out _playerHullText, out _playerShieldText);
            BuildShipCard(panel.transform, "OppCard",
                new Vector2(0.51f, 0.55f), new Vector2(0.98f, 0.91f),
                out _oppShipImg, out _oppNameText, out _oppHullText, out _oppShieldText);

            // Description text below the ships
            _descText = UIFactory.LabelWrap(panel.transform, "Desc", "",
                ColorTheme.FontBody, ColorTheme.TextPrimary, TextAlignmentOptions.Center);
            UIFactory.SetAnchored(_descText.rectTransform,
                new Vector2(0.04f, 0.20f), new Vector2(0.96f, 0.54f), new Vector2(4, 4), new Vector2(-4, -4));

            // Action buttons row
            _btnContainer = UIFactory.TransparentPanel(panel.transform, "Btns").transform;
            UIFactory.SetAnchored(_btnContainer.GetComponent<RectTransform>(),
                new Vector2(0, 0.01f), new Vector2(1, 0.19f), new Vector2(8, 4), new Vector2(-8, -4));
            var glg = _btnContainer.gameObject.AddComponent<GridLayoutGroup>();
            glg.cellSize        = new Vector2(490, 80);
            glg.spacing         = new Vector2(10, 10);
            glg.padding         = new RectOffset(4, 4, 4, 4);
            glg.constraint      = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = 2;

            BuildTraderDialog(panel);
        }

        void BuildTraderDialog(GameObject panel)
        {
            _traderDialog = UIFactory.Panel(panel.transform, "TraderDialog",
                new Color(0.08f, 0.08f, 0.25f, 0.97f));
            UIFactory.SetAnchored(_traderDialog.GetComponent<RectTransform>(),
                new Vector2(0.02f, 0.18f), new Vector2(0.98f, 0.92f), Vector2.zero, Vector2.zero);

            var titleLbl = UIFactory.Label(_traderDialog.transform, "TraderTitle", "TRADER CARGO",
                ColorTheme.FontHeader, ColorTheme.TextAccent, TextAlignmentOptions.Center);
            UIFactory.SetAnchored(titleLbl.rectTransform,
                new Vector2(0, 0.85f), Vector2.one, new Vector2(8, 0), new Vector2(-8, 0));

            var contentGo = UIFactory.TransparentPanel(_traderDialog.transform, "Content");
            UIFactory.SetAnchored(contentGo.GetComponent<RectTransform>(),
                new Vector2(0, 0.18f), new Vector2(1, 0.85f), new Vector2(4, 4), new Vector2(-4, -4));
            var vlg = contentGo.AddComponent<VerticalLayoutGroup>();
            vlg.spacing            = 6;
            vlg.padding            = new RectOffset(6, 6, 4, 4);
            vlg.childForceExpandWidth  = true;
            vlg.childForceExpandHeight = false;
            _traderContent = contentGo.transform;

            var doneBtn = UIFactory.Btn(_traderDialog.transform, "Done", "DONE",
                () => _traderDialog.SetActive(false),
                ColorTheme.ButtonNormal, ColorTheme.FontButton);
            UIFactory.SetAnchored(doneBtn.GetComponent<RectTransform>(),
                new Vector2(0.25f, 0.02f), new Vector2(0.75f, 0.16f), Vector2.zero, Vector2.zero);

            _traderDialog.SetActive(false);

        // Builds a "ship card": placeholder colored ship icon up top, then the
        // ship name, hull, and shield stacked underneath. Real sprites can be
        // dropped in later by replacing the Image's sprite/color.
        void BuildShipCard(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax,
            out Image shipImg, out TextMeshProUGUI nameText,
            out TextMeshProUGUI hullText, out TextMeshProUGUI shieldText)
        {
            var card = UIFactory.Panel(parent, name, ColorTheme.PanelBg);
            UIFactory.SetAnchored(card.GetComponent<RectTransform>(),
                anchorMin, anchorMax, new Vector2(4, 4), new Vector2(-4, -4));

            // Ship icon — currently a flat colored rectangle as a placeholder.
            var imgGo = new GameObject("ShipImg");
            imgGo.transform.SetParent(card.transform, false);
            shipImg = imgGo.AddComponent<Image>();
            shipImg.color = Color.gray;
            var imgRt = imgGo.GetComponent<RectTransform>();
            imgRt.anchorMin = new Vector2(0.18f, 0.42f);
            imgRt.anchorMax = new Vector2(0.82f, 0.96f);
            imgRt.offsetMin = Vector2.zero;
            imgRt.offsetMax = Vector2.zero;

            nameText = UIFactory.Label(card.transform, "Name", "",
                ColorTheme.FontBody, ColorTheme.TextAccent, TextAlignmentOptions.Center);
            UIFactory.SetAnchored(nameText.rectTransform,
                new Vector2(0, 0.28f), new Vector2(1, 0.42f), new Vector2(4, 0), new Vector2(-4, 0));

            hullText = UIFactory.Label(card.transform, "Hull", "",
                ColorTheme.FontSmall, ColorTheme.HullFill, TextAlignmentOptions.Center);
            UIFactory.SetAnchored(hullText.rectTransform,
                new Vector2(0, 0.15f), new Vector2(1, 0.28f), new Vector2(4, 0), new Vector2(-4, 0));

            shieldText = UIFactory.Label(card.transform, "Shield", "",
                ColorTheme.FontSmall, ColorTheme.TextSecondary, TextAlignmentOptions.Center);
            UIFactory.SetAnchored(shieldText.rectTransform,
                new Vector2(0, 0.02f), new Vector2(1, 0.15f), new Vector2(4, 0), new Vector2(-4, 0));
        }

        public void OnShow()
        {
            _playerFleeing  = false;
            _oppFleeing     = false;
            _hasStruckFirst = false;
            BuildActions();
            RefreshStatus();
        }

        void RefreshStatus()
        {
            var G   = GameState.Instance;
            var opp = G.Opponent;
            var oppSt = GameData.Shiptypes[opp.Type];
            var plySt = GameData.Shiptypes[G.Ship.Type];

            // Opponent card
            _oppNameText.text    = oppSt.Name;
            ApplyShipImage(_oppShipImg, opp.Type);
            long oppHull    = opp.Hull;
            long oppMaxHull = oppSt.HullStrength;
            long oppShield  = EncounterSystem.TotalShieldStrength(opp);
            _oppHullText.text    = $"Hull: {oppHull}/{oppMaxHull}";
            _oppShieldText.text  = oppShield > 0 ? $"Shield: {oppShield}" : "";

            // Player card
            _playerNameText.text  = plySt.Name;
            ApplyShipImage(_playerShipImg, G.Ship.Type);
            long plyHull    = G.Ship.Hull;
            long plyMaxHull = ShipyardSystem.GetHullStrength();
            long plyShield  = EncounterSystem.TotalShieldStrength(G.Ship);
            _playerHullText.text   = $"Hull: {plyHull}/{plyMaxHull}";
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
            else if (enc == TraderSell)
            {
                AddBtn("BUY",    OpenTraderDialog, ColorTheme.ButtonSuccess);
                AddBtn("IGNORE", OnIgnore,         ColorTheme.ButtonNormal);
                if (canFight) AddBtn("ATTACK", OnAttack, ColorTheme.ButtonDanger);
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
            || enc == TraderAttack || enc == Mantis;

        void OnAttack()
        {
            var G   = GameState.Instance;
            var opp = G.Opponent;

            // First strike against a peaceful encounter incurs a record penalty:
            // attacking police that haven't fired (inspection/ignore) or a trader
            // that wasn't already attacking is treated as the aggressor.
            if (!_hasStruckFirst)
            {
                int enc = G.EncounterType;
                bool peacefulPolice = enc == PoliceInspection || enc == PoliceIgnore || enc == PoliceFlee;
                bool peacefulTrader = EncounterSystem.IsTrader(enc) && enc != TraderAttack;
                if (peacefulPolice) G.PoliceRecordScore += AttackPoliceScore;
                else if (peacefulTrader) G.PoliceRecordScore += AttackTraderScore;
                _hasStruckFirst = true;
            }

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

            // Attempting to flee from police always hurts your record (successful or not)
            if (EncounterSystem.IsPolice(G.EncounterType))
                G.PoliceRecordScore += FleeFromInspection;

            // Guard against pil + oppPil == 0 → GetRandom(0) divides by zero.
            int pilSum = pil + oppPil;
            if (pilSum <= 0 || GameMath.GetRandom(pilSum) >= oppPil)
            {
                ShowResult("You escaped!", () => ReturnToTravel());
                return;
            }

            // Failed to flee — opponent fires a free shot while player evades
            string oppName = GameData.Shiptypes[G.Opponent.Type].Name;
            long hullBefore   = G.Ship.Hull;
            long shieldBefore = EncounterSystem.TotalShieldStrength(G.Ship);

            _playerFleeing = true;
            bool destroyed = EncounterSystem.ExecuteAttack(G.Opponent, G.Ship, false, true);
            _playerFleeing = false;

            if (destroyed) { OnPlayerDestroyed(); return; }

            bool wasHit = G.Ship.Hull < hullBefore
                       || EncounterSystem.TotalShieldStrength(G.Ship) < shieldBefore;
            string hitLine = wasHit
                ? $"The {oppName} hits you."
                : $"The {oppName} missed you.";

            RefreshStatus();
            ShowResult(
                $"{hitLine}\nThe {oppName} is still following you.\nYour opponent attacks.",
                () => { BuildActions(); RefreshStatus(); });
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
            else if (EncounterSystem.IsPolice(G.EncounterType))
            {
                // Yielding to attacking police triggers the same confiscation as submitting
                // to an inspection — the player surrenders and the police board the ship.
                G.Inspected = true;
                bool hasCont = G.Ship.Cargo[Narcotics] > 0 || G.Ship.Cargo[Firearms] > 0;
                if (hasCont)
                {
                    if (G.Ship.Cargo[Narcotics] > 0) G.PoliceRecordScore += Trafficking;
                    if (G.Ship.Cargo[Firearms]  > 0) G.PoliceRecordScore += Trafficking;
                    G.Ship.Cargo[Narcotics]  = 0;
                    G.BuyingPrice[Narcotics] = 0;
                    G.Ship.Cargo[Firearms]   = 0;
                    G.BuyingPrice[Firearms]  = 0;
                    ShowResult("You surrendered. Police confiscated your contraband.", () => ReturnToTravel());
                }
                else
                {
                    ShowResult("You surrendered. Police found nothing and let you go.", () => ReturnToTravel());
                }
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
                G.Ship.Cargo[Narcotics]    = 0;
                G.BuyingPrice[Narcotics]   = 0;
                G.Ship.Cargo[Firearms]     = 0;
                G.BuyingPrice[Firearms]    = 0;
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

        void OpenTraderDialog()
        {
            var G = GameState.Instance;
            foreach (Transform child in _traderContent) Destroy(child.gameObject);

            bool hasGoods = false;
            for (int i = 0; i < MaxTradeItem; i++)
            {
                if (G.Opponent.Cargo[i] <= 0) continue;
                long price = TraderPriceFor(i);
                if (price <= 0) continue;
                hasGoods = true;

                int idx = i; // capture for closure

                var row = UIFactory.Panel(_traderContent, $"Row{i}",
                    i % 2 == 0 ? ColorTheme.RowBg : ColorTheme.RowAlt);
                var le = row.AddComponent<LayoutElement>();
                le.preferredHeight = 80;

                var nameLbl = UIFactory.Label(row.transform, "Name",
                    GameData.Tradeitems[i].Name,
                    ColorTheme.FontBody, ColorTheme.TextPrimary, TextAlignmentOptions.Left);
                UIFactory.SetAnchored(nameLbl.rectTransform,
                    new Vector2(0, 0), new Vector2(0.38f, 1), new Vector2(8, 2), new Vector2(-2, -2));

                var infoLbl = UIFactory.Label(row.transform, "Info",
                    $"{G.Opponent.Cargo[i]} @ {UIFactory.Cr(price)}",
                    ColorTheme.FontSmall, ColorTheme.TextSecondary, TextAlignmentOptions.Center);
                UIFactory.SetAnchored(infoLbl.rectTransform,
                    new Vector2(0.38f, 0), new Vector2(0.72f, 1), new Vector2(2, 2), new Vector2(-2, -2));

                var buyBtn = UIFactory.SmallBtn(row.transform, "Buy", "BUY",
                    () => { BuyFromTrader(idx, G.Opponent.Cargo[idx]); OpenTraderDialog(); });
                UIFactory.SetAnchored(buyBtn.GetComponent<RectTransform>(),
                    new Vector2(0.73f, 0.08f), new Vector2(0.98f, 0.92f), Vector2.zero, Vector2.zero);
            }

            if (!hasGoods)
            {
                var noGoodsLbl = UIFactory.Label(_traderContent, "NoGoods",
                    "Trader has no goods available.",
                    ColorTheme.FontBody, ColorTheme.TextSecondary, TextAlignmentOptions.Center);
                var le = noGoodsLbl.gameObject.AddComponent<LayoutElement>();
                le.preferredHeight = 80;
            }

            _traderDialog.SetActive(true);
        }

        long TraderPriceFor(int item)
        {
            var G = GameState.Instance;
            if (G.BuyPrice[item] > 0) return G.BuyPrice[item];
            var sys = G.CurrentSystem;
            return TravelerSystem.StandardPrice(item, sys.Size, sys.TechLevel, sys.Politics, sys.SpecialResources);
        }

        void BuyFromTrader(int item, int amount)
        {
            var G     = GameState.Instance;
            long price = TraderPriceFor(item);
            if (price <= 0 || amount <= 0) return;

            int maxByMoney = price > 0 ? (int)(MoneySystem.ToSpend() / price) : 0;
            int maxBySpace = CargoSystem.FreeCargoBays();
            int maxByStock = G.Opponent.Cargo[item];
            int qty = GameMath.Min(GameMath.Min(maxByMoney, maxBySpace), GameMath.Min(amount, maxByStock));
            if (qty <= 0) return;

            long cost = price * qty;
            G.Credits          -= cost;
            G.Ship.Cargo[item] += qty;
            G.Opponent.Cargo[item] -= qty;
            // Maintain running-average purchase price for profit tracking
            int newQty = G.Ship.Cargo[item];
            if (newQty > 0)
                G.BuyingPrice[item] = (G.BuyingPrice[item] * (newQty - qty) + price * qty) / newQty;
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
                // Easy/Normal get a second skill bump (Hard/Impossible only one).
                if (G.Difficulty < Hard) SkillSystem.IncreaseRandomSkill();
                ShowResult("The bottle of Eden improved your skills!", () => ReturnToTravel());
            }
        }

        void OnOpponentDestroyed()
        {
            var G   = GameState.Instance;
            int enc = G.EncounterType;

            long bounty = EncounterSystem.GetBounty(G.Opponent);
            G.Credits   += bounty;
            G.ReputationScore++;

            if (EncounterSystem.IsPolice(enc))  { G.PoliceRecordScore += KillPoliceScore; G.PoliceKills++; }
            if (EncounterSystem.IsPirate(enc))  { G.PoliceRecordScore += KillPirateScore; G.PirateKills++; }
            if (EncounterSystem.IsTrader(enc))  { G.PoliceRecordScore += KillTraderScore; G.TraderKills++; }

            string extraMsg = "";
            if (EncounterSystem.IsSpaceMonster(enc))
            {
                G.MonsterStatus = 2;
                extraMsg = "\nThe Acamar system is safe!";
            }
            else if (EncounterSystem.IsDragonfly(enc))
            {
                G.DragonflyStatus = 5;
                extraMsg = "\nCheck Zalkon for your reward.";
            }
            else if (EncounterSystem.IsScarab(enc))
            {
                G.ScarabStatus = 2;
                extraMsg = "\nA hull upgrade awaits you somewhere.";
            }

            ShowResult($"You destroyed the {GameData.Shiptypes[G.Opponent.Type].Name}!\nBounty: {UIFactory.Cr(bounty)}{extraMsg}",
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
                    // Replace the stack so back-from-HighScores goes to Title,
                    // not back into the destroyed encounter.
                    UIManager.Instance.NavigateReplaceStack(
                        GameScreen.Title, GameScreen.HighScores);
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
            var G       = GameState.Instance;
            var oppName = GameData.Shiptypes[G.Opponent.Type].Name.ToLower();
            string dest = (G.WarpSystem >= 0 && G.WarpSystem < MaxSolarSystem)
                ? GameData.SolarSystemNames[G.SolarSystem[G.WarpSystem].NameIndex]
                : "your destination";
            string prefix = $"At {G.Clicks} clicks from {dest}, ";

            if (enc == PoliceInspection)          return prefix + $"a police {oppName} requests an inspection.";
            if (enc == PoliceAttack)              return prefix + $"a police {oppName} attacks you!";
            if (EncounterSystem.IsPirate(enc))    return prefix + $"you encounter a pirate {oppName}.\nYour opponent attacks.";
            if (enc == TraderAttack)              return prefix + $"a trader {oppName} attacks!";
            if (enc == TraderSell)                return prefix + $"a trader {oppName} hails you and offers goods for sale.";
            if (EncounterSystem.IsTrader(enc))    return prefix + $"a trader {oppName} passes by.";
            if (EncounterSystem.IsSpaceMonster(enc)) return prefix + "a massive Space Monster blocks your path!";
            if (EncounterSystem.IsDragonfly(enc)) return prefix + "the Dragonfly cuts you off!";
            if (EncounterSystem.IsScarab(enc))    return prefix + "the Scarab swoops to attack!";
            if (enc == CaptainAhabEncounter)      return prefix + "Captain Ahab hails you and offers a skill tonic.";
            if (enc == CaptainConradEncounter)    return prefix + "Captain Conrad hails you and offers to train you.";
            if (enc == CaptainHuieEncounter)      return prefix + "Captain Huie hails you and offers to train you.";
            if (enc == MarieCelesteEncounter)     return prefix + "the Marie Celeste drifts silently. Its hold looks full.";
            if (enc == BottleOldEncounter)        return prefix + "a sealed bottle drifts past. It smells questionable.";
            if (enc == BottleGoodEncounter)       return prefix + "a sealed bottle drifts past. It smells wonderful.";
            return prefix + "something unusual happens.";
        }
    }
}
