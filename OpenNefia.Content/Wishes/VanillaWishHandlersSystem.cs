using OpenNefia.Content.StatusEffects;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Game;
using OpenNefia.Content.Effects;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.RandomGen;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Logic;
using OpenNefia.Core.UserInterface;
using OpenNefia.Content.CharaMake;
using OpenNefia.Content.RandomText;
using OpenNefia.Content.Charas;
using OpenNefia.Content.DeferredEvents;
using OpenNefia.Content.Scenarios;
using OpenNefia.Content.UI;
using OpenNefia.Content.Damage;
using OpenNefia.Content.Skills;
using OpenNefia.Content.Karma;
using OpenNefia.Content.Fame;
using OpenNefia.Content.Weight;
using OpenNefia.Content.World;
using OpenNefia.Core.Utility;
using OpenNefia.Content.Religion;
using Love;
using System.Text.RegularExpressions;
using OpenNefia.Content.Items;
using ICSharpCode.Decompiler.Util;
using OpenNefia.Content.Levels;
using OpenNefia.Content.Qualities;
using OpenNefia.Content.Identify;
using OpenNefia.Content.Potion;
using OpenNefia.Content.Scroll;
using OpenNefia.Content.GameObjects;
using OpenNefia.Core.Formulae;
using OpenNefia.Content.Items.Impl;
using System.Net.Mail;
using System.Xml.Linq;

namespace OpenNefia.Content.Wishes
{
    public sealed class VanillaWishHandlersSystem : EntitySystem
    {
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IEntityLookup _entityLookup = default!;
        [Dependency] private readonly IEntityGen _entityGen = default!;
        [Dependency] private readonly IRandomGenSystem _randomGen = default!;
        [Dependency] private readonly ICharaGen _charaGen = default!;
        [Dependency] private readonly IItemGen _itemGen = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IAudioManager _audio = default!;
        [Dependency] private readonly IUserInterfaceManager _uiMan = default!;
        [Dependency] private readonly IDeferredEventsSystem _deferredEvents = default!;
        [Dependency] private readonly IVanillaScenariosSystem _vanillaScenarios = default!;
        [Dependency] private readonly IDamageSystem _damages = default!;
        [Dependency] private readonly IKarmaSystem _karmas = default!;
        [Dependency] private readonly IWorldSystem _world = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly ISkillsSystem _skills = default!;
        [Dependency] private readonly IItemNameSystem _itemNames = default!;
        [Dependency] private readonly ILevelSystem _levels = default!;
        [Dependency] private readonly IQualitySystem _qualities = default!;
        [Dependency] private readonly IIdentifySystem _identify = default!;
        [Dependency] private readonly IStackSystem _stacks = default!;
        [Dependency] private readonly IFormulaEngine _formulaEngine = default!;

        public override void Initialize()
        {
            SubscribeComponent<PotionComponent, AfterItemCreatedFromWishEvent>(AfterWished_Potion, priority: EventPriorities.VeryHigh);
            SubscribeComponent<ScrollComponent, AfterItemCreatedFromWishEvent>(AfterWished_Scroll, priority: EventPriorities.VeryHigh);
            SubscribeComponent<PotionComponent, AfterItemCreatedFromWishEvent>(AfterWished_Potion_Value, priority: EventPriorities.Low);
            SubscribeComponent<ScrollComponent, AfterItemCreatedFromWishEvent>(AfterWished_Scroll_Value, priority: EventPriorities.Low);
            SubscribeComponent<HolyWellComponent, AfterItemCreatedFromWishEvent>(AfterWished_HolyWell);
            SubscribeComponent<ItemComponent, AfterItemCreatedFromWishEvent>(Item_AdjustWishedForAmount, priority: EventPriorities.High);
        }

        #region Elona.Alias

        public void Alias_OnWish(WishHandlerPrototype proto, ref P_WishHandlerOnWishEvent ev)
        {
            if (ev.Handled)
                return;

            ev.Handled = true;

            // TODO wizard mode

            _mes.Display(Loc.GetPrototypeString(proto, "Prompt", ("wisher", ev.Wisher)));

            var args = new PickRandomAliasPrompt.Args()
            {
                AliasType = AliasType.Chara
            };
            var result = _uiMan.Query<PickRandomAliasPrompt, PickRandomAliasPrompt.Args, PickRandomAliasPrompt.Result>(args);

            if (result.HasValue && !string.IsNullOrWhiteSpace(result.Value.Alias))
            {
                var newAlias = result.Value.Alias;
                EnsureComp<AliasComponent>(ev.Wisher).Alias = newAlias;
                _mes.Display(Loc.GetPrototypeString(proto, "NewAlias", ("wisher", ev.Wisher), ("newAlias", newAlias)));
            }
            else
            {
                _mes.Display(Loc.GetPrototypeString(proto, "NoChange", ("wisher", ev.Wisher)));
            }
        }

        #endregion

        #region Elona.SmallMedal

        public void SmallMedal_OnWish(WishHandlerPrototype proto, ref P_WishHandlerOnWishEvent ev)
        {
            if (ev.Handled)
                return;

            ev.Handled = true;

            _mes.Display(Loc.GetPrototypeString(proto, "Result", ("wisher", ev.Wisher)), color: UiColors.MesYellow);
            var amount = 3 + _rand.Next(3);
            _itemGen.GenerateItem(ev.Wisher, Protos.Item.SmallMedal, amount: amount);
        }

        #endregion

        #region Elona.Sex

        public void Sex_OnWish(WishHandlerPrototype proto, ref P_WishHandlerOnWishEvent ev)
        {
            if (ev.Handled)
                return;

            ev.Handled = true;

            if (!TryComp<CharaComponent>(ev.Wisher, out var chara))
            {
                ev.OutDidSomething = false;
                return;
            }

            // TODO arbitrary gender
            chara.Gender = chara.Gender == Gender.Male ? Gender.Female : Gender.Male;
            var genderName = Loc.GetString($"Elona.Gender.Names.{chara.Gender}.Normal");
            _mes.Display(Loc.GetPrototypeString(proto, "Result", ("wisher", ev.Wisher), ("newGender", genderName)));
        }

        #endregion

        #region Elona.Youth

        public void Youth_OnWish(WishHandlerPrototype proto, ref P_WishHandlerOnWishEvent ev)
        {
            if (ev.Handled)
                return;

            ev.Handled = true;

            if (!TryComp<WeightComponent>(ev.Wisher, out var weight))
            {
                ev.OutDidSomething = false;
                return;
            }

            _mes.Display(Loc.GetPrototypeString(proto, "Result", ("wisher", ev.Wisher)));
            weight.Age = int.Min(weight.Age + 20, _world.State.GameDate.Year - 12); // TODO check
        }

        #endregion

        #region Elona.Redemption

        public void Redemption_OnWish(WishHandlerPrototype proto, ref P_WishHandlerOnWishEvent ev)
        {
            if (ev.Handled)
                return;

            ev.Handled = true;

            if (!TryComp<KarmaComponent>(ev.Wisher, out var karma) || karma.Karma.Base >= KarmaLevels.Neutral)
            {
                _mes.Display(Loc.GetPrototypeString(proto, "NotASinner", ("wisher", ev.Wisher)));
                return;
            }

            _karmas.ModifyKarma(ev.Wisher, -(karma.Karma.Base / 2), karma);
            karma.Karma.Reset();
            _mes.Display(Loc.GetPrototypeString(proto, "Result", ("wisher", ev.Wisher)));
        }

        #endregion

        #region Elona.ManInside

        public void ManInside_OnWish(WishHandlerPrototype proto, ref P_WishHandlerOnWishEvent ev)
        {
            if (ev.Handled)
                return;

            ev.Handled = true;

            _mes.Display(Loc.GetPrototypeString(proto, "Result", ("wisher", ev.Wisher)));
        }

        #endregion

        #region Elona.Ally

        public void Ally_OnWish(WishHandlerPrototype proto, ref P_WishHandlerOnWishEvent ev)
        {
            if (ev.Handled)
                return;

            ev.Handled = true;

            _deferredEvents.Enqueue(() => _vanillaScenarios.MeetFirstAlly(canCancel: true));
        }

        #endregion

        #region Elona.Platinum

        public void Platinum_OnWish(WishHandlerPrototype proto, ref P_WishHandlerOnWishEvent ev)
        {
            if (ev.Handled)
                return;

            ev.Handled = true;

            _mes.Display(Loc.GetPrototypeString(proto, "Result", ("wisher", ev.Wisher)), color: UiColors.MesYellow);
            var amount = 5;
            _itemGen.GenerateItem(ev.Wisher, Protos.Item.PlatinumCoin, amount: amount);
        }

        #endregion

        #region Elona.Death

        public void Death_OnWish(WishHandlerPrototype proto, ref P_WishHandlerOnWishEvent ev)
        {
            if (ev.Handled)
                return;

            ev.Handled = true;

            if (!TryComp<SkillsComponent>(ev.Wisher, out var skills))
            {
                ev.OutDidSomething = false;
                return;
            }

            _mes.Display(Loc.GetPrototypeString(proto, "Result", ("wisher", ev.Wisher)));
            _damages.DamageHP(ev.Wisher, skills.MaxHP + 1, damageType: new GenericDamageType("Elona.DamageType.UnseenHand"));
        }

        #endregion

        #region Elona.Gold

        public void Gold_OnWish(WishHandlerPrototype proto, ref P_WishHandlerOnWishEvent ev)
        {
            if (ev.Handled)
                return;

            ev.Handled = true;

            _mes.Display(Loc.GetPrototypeString(proto, "Result", ("wisher", ev.Wisher)), color: UiColors.MesYellow);
            var amount = 5;
            _itemGen.GenerateItem(ev.Wisher, Protos.Item.GoldPiece, amount: amount);
        }

        #endregion

        #region Elona.GodInside

        public void GodInside_OnWish(WishHandlerPrototype proto, ref P_WishHandlerOnWishEvent ev)
        {
            if (ev.Handled)
                return;

            ev.Handled = true;

            _mes.Display(Loc.GetPrototypeString(proto, "Result", ("wisher", ev.Wisher)));
        }

        #endregion

        #region Elona.God

        public void God_OnWish(WishHandlerPrototype proto, ref P_WishHandlerOnWishEvent ev)
        {
            if (ev.Handled || !TryMap(ev.Wisher, out var map))
                return;

            GodPrototype? found = null;

            foreach (var god in _protos.EnumeratePrototypes<GodPrototype>())
            {
                if (!Loc.TryGetPrototypeString(god, "Name", out var name)
                    || !Loc.TryGetPrototypeString(god, "ShortName", out var shortName))
                    continue;

                if (ev.Wish.Equals(name, StringComparison.InvariantCultureIgnoreCase)
                    || ev.Wish.Equals(shortName, StringComparison.InvariantCultureIgnoreCase))
                {
                    found = god;
                    break;
                }
            }

            if (found != null && found.Summon != null)
            {
                ev.Handled = true;

                var alreadyInMap = _entityLookup.EntityQueryInMap<MetaDataComponent>(map)
                    .Any(metadata => metadata.EntityPrototype?.GetStrongID() == found.Summon);

                if (alreadyInMap)
                {
                    ev.OutDidSomething = false;
                    return;
                }

                if (Loc.TryGetPrototypeString(found, "Talk.WishSummon", out var talkSummon))
                {
                    _mes.Display(talkSummon, color: UiColors.MesTalk);
                }
                _charaGen.GenerateChara(ev.Wisher, found.Summon.Value);
            }
        }

        #endregion

        private string NormalizeWishText(string wish)
        {
            // >>>>>>>> shade2/command.hsp:1386 *wish_fix ..
            // Remove all spaces if the language doesn't need them.
            if (Loc.IsFullwidth())
                wish = Regex.Replace(wish, @"\s+", "");

            wish = wish.Replace(Loc.GetString("Elona.Wishes.General.Item.Keyword"), "");
            wish = wish.Replace(Loc.GetString("Elona.Wishes.General.Skill.Keyword"), "");
            // <<<<<<<< shade2/command.hsp:1401 	return ..

            // >>>>>>>> shade2/module.hsp:2063 	cnv_str s,"card ","" ..
            wish = wish.Replace(Loc.GetString("Elona.Wishes.General.Card.Keyword"), "");
            wish = wish.Replace(Loc.GetString("Elona.Wishes.General.Figure.Keyword"), "");
            // >>>>>>>> shade2/module.hsp:2063 	cnv_str s,"card ","" ..

            wish = wish.Trim();
            wish = wish.ToLowerInvariant();

            return wish;
        }

        private int FuzzyMatch(string wish, string textToMatch)
        {
            if (wish.Equals(textToMatch, StringComparison.InvariantCultureIgnoreCase))
                return int.MaxValue;
            if (wish == string.Empty)
                return 0;

            wish = wish.ToLowerInvariant();
            textToMatch = textToMatch.ToLowerInvariant();

            var wishRunes = wish.EnumerateRunes();
            var textRunes = textToMatch.EnumerateRunes();

            var priority = 0;
            var i = 1;
            foreach (var (wishRune, textRune) in wishRunes.Zip(textRunes))
            {
                if (wishRune == textRune)
                    priority += 50 * i + _rand.Next(15);
                i++;
            }

            return priority;
        }

        #region Elona.Skill

        private bool CanWishForSkill(SkillPrototype skill)
        {
            // TODO replace with flag
            return skill.SkillType != SkillType.AttributeSpecial;
        }

        public void Skill_OnWish(WishHandlerPrototype proto, ref P_WishHandlerOnWishEvent ev)
        {
            if (ev.Handled)
                return;

            var wish = NormalizeWishText(ev.Wish);

            var maxPriority = 0;
            SkillPrototype? found = null;

            foreach (var skillProto in _protos.EnumeratePrototypes<SkillPrototype>().Where(CanWishForSkill))
            {
                if (!Loc.TryGetPrototypeString(skillProto, "Name", out var name))
                    continue;

                var priority = FuzzyMatch(wish, name);
                if (priority > maxPriority)
                {
                    found = skillProto;
                    maxPriority = priority;
                }
            }

            if (found != null)
            {
                ev.Handled = true;
                var skillName = Loc.GetPrototypeString(found, "Name");

                if (_skills.HasSkill(ev.Wisher, found))
                {
                    _mes.Display(Loc.GetString("Elona.Wishes.General.Skill.Improve", ("wisher", ev.Wisher), ("skillName", skillName)), color: UiColors.MesYellow);
                    _skills.GainFixedSkillExp(ev.Wisher, found.GetStrongID(), 1000);
                    _skills.ModifyPotential(ev.Wisher, found.GetStrongID(), 25);
                }
                else
                {
                    _mes.Display(Loc.GetString("Elona.Wishes.General.Skill.Gain", ("wisher", ev.Wisher), ("skillName", skillName)), color: UiColors.MesYellow);
                    _skills.GainSkill(ev.Wisher, found.GetStrongID());

                }
            }
        }

        #endregion

        #region Elona.Item

        private bool IsWishableItem(EntityPrototype prototype)
        {
            return prototype.Components.TryGetComponent<ItemComponent>(out var item)
                && item.CanWishFor
                && !item.IsPrecious
                && (!prototype.Components.TryGetComponent<QualityComponent>(out var quality)
                  || quality.Quality != Quality.Unique);
        }

        private void AfterWished_Potion(EntityUid uid, PotionComponent component, AfterItemCreatedFromWishEvent args)
        {
            // >>>>>>>> elona122/shade2/command.hsp:1598 		if (iType(ci)=fltPotion)or(iType(ci)=fltScroll){ ...
            var amount = 3 + _rand.Next(2);
            _stacks.SetCount(uid, amount);
            // <<<<<<<< elona122/shade2/command.hsp:1608 			} ...
        }

        private void AfterWished_Scroll(EntityUid uid, ScrollComponent component, AfterItemCreatedFromWishEvent args)
        {
            // >>>>>>>> elona122/shade2/command.hsp:1598 		if (iType(ci)=fltPotion)or(iType(ci)=fltScroll){ ...
            var amount = 3 + _rand.Next(2);
            _stacks.SetCount(uid, amount);
            // <<<<<<<< elona122/shade2/command.hsp:1608 			} ...
        }

        private void AfterWished_Potion_Value(EntityUid uid, PotionComponent component, AfterItemCreatedFromWishEvent args)
        {
            // >>>>>>>> elona122/shade2/command.hsp:1598 		if (iType(ci)=fltPotion)or(iType(ci)=fltScroll){ ...
            AdjustPotionAndScrollCountFromValue(uid);
            // <<<<<<<< elona122/shade2/command.hsp:1608 			} ...
        }

        private void AfterWished_Scroll_Value(EntityUid uid, ScrollComponent component, AfterItemCreatedFromWishEvent args)
        {
            // >>>>>>>> elona122/shade2/command.hsp:1598 		if (iType(ci)=fltPotion)or(iType(ci)=fltScroll){ ...
            AdjustPotionAndScrollCountFromValue(uid);
            // <<<<<<<< elona122/shade2/command.hsp:1608 			} ...
        }

        private void AfterWished_HolyWell(EntityUid uid, HolyWellComponent component, AfterItemCreatedFromWishEvent args)
        {
            // >>>>>>>> shade2/command.hsp:1597 		if iId(ci)=idHolyWell:iNum(ci)=0:flt:item_create ..
            args.OutItem = _itemGen.GenerateItem(uid, Protos.Item.BottleOfWater, amount: 3);
            EntityManager.DeleteEntity(uid);
            _mes.Display(Loc.GetString("Elona.Wishes.ItIsSoldOut"));
            // <<<<<<<< shade2/command.hsp:1597 		if iId(ci)=idHolyWell:iNum(ci)=0:flt:item_create ..
        }

        private void Item_AdjustWishedForAmount(EntityUid uid, ItemComponent component, AfterItemCreatedFromWishEvent args)
        {
            // >>>>>>>> elona122/shade2/command.hsp:1595 		if iId(ci)=idGold:iNum(ci)=cLevel(pc)*cLevel(pc) ...
            if (!IsAlive(uid) || component.WishAmount == null)
                return;

            var vars = new Dictionary<string, double>();
            vars["sourceLevel"] = _levels.GetLevel(args.Wisher);

            var curAmount = _stacks.GetCount(uid);
            var amount = int.Max((int)_formulaEngine.Calculate(component.WishAmount.Value, vars, curAmount), 1);

            _stacks.SetCount(uid, amount);
            // <<<<<<<< elona122/shade2/command.hsp:1608 			} ...
        }

        private void AdjustPotionAndScrollCountFromValue(EntityUid uid)
        {
            // >>>>>>>> elona122/shade2/command.hsp:1599 			iNum(ci)=3+rnd(2) ...
            var value = CompOrNull<ValueComponent>(uid)?.Value?.Buffed ?? 0;
            var amount = _stacks.GetCount(uid);
            if (value >= 5000)
                amount = int.Min(amount, 3);
            if (value >= 10000)
                amount = int.Min(amount, 2);
            if (value >= 20000)
                amount = int.Min(amount, 1);
            _stacks.SetCount(uid, amount);
            // <<<<<<<< elona122/shade2/command.hsp:1607 			if iValue(ci)>=20000			:iNum(ci)=1 ...
        }

        public void Item_OnWish(WishHandlerPrototype proto, ref P_WishHandlerOnWishEvent ev)
        {
            // >>>>>>>> shade2/command.hsp:1557 *wish_item ..
            if (ev.Handled)
                return;

            var wish = NormalizeWishText(ev.Wish);

            var candidates = new List<(EntityPrototype, int)>();

            foreach (var item in _protos.EnumeratePrototypes<EntityPrototype>().Where(IsWishableItem))
            {
                var name = _itemNames.QualifyNameWithItemType(item.GetStrongID());
                var priority = FuzzyMatch(wish, name);
                if (priority > 0)
                {
                    candidates.Add((item, priority));
                }
            }

            candidates = candidates.OrderBy(p => p.Item2).ToList();

            while (candidates.Count > 0)
            {
                var (itemProto, priority) = candidates.Pop();
                var filter = new ItemFilter()
                {
                    Id = itemProto.GetStrongID(),
                    MinLevel = _levels.GetLevel(ev.Wisher) + 10,
                    Quality = Qualities.Quality.Great,
                    Args = EntityGenArgSet.Make(new EntityGenCommonArgs()
                    {
                        NoStack = true,
                    }, new ItemGenArgs()
                    {
                        NoOracle = true,
                    })
                };

                // TODO quality for aurora ring/seven league boots/forest cloak/ring of speed
                // if (p=idRingAurora)or(p=idBootsSeven)or(p=idCloackForest)or(p=idRingSpeed):fixLv=calcObjLv(fixGood)

                // TODO level for material kit
                // if p=idMaterialKit:objFix=2

                var item = _itemGen.GenerateItem(ev.Wisher, filter);
                if (!IsAlive(item))
                    continue;

                var itemEv = new AfterItemCreatedFromWishEvent(ev.Wisher, item.Value, ev.Wish);
                RaiseEvent(item.Value, itemEv);
                item = itemEv.OutItem;
                if (!IsAlive(item))
                    continue;
                
                _identify.IdentifyItem(item.Value, IdentifyState.Full);
                _mes.Display(Loc.GetString("Elona.Wishes.SomethingAppears.Normal", ("wisher", ev.Wisher), ("item", item.Value)));
                ev.Handled = true;
                return;
            }
            // <<<<<<<< shade2/command.hsp:1614 		} ..
        }

        #endregion

        #region Elona.Common

        private EntityPrototype ExtractCharaPrototype(string wish)
        {
            wish = NormalizeWishText(wish);

            EntityPrototype? found = null;
            var maxPriority = 0;

            foreach (var ent in _protos.EnumeratePrototypes<EntityPrototype>())
            {
                var charaName = Loc.GetPrototypeString(ent, "MetaData.Name");
                var priority = FuzzyMatch(wish, charaName);
                if (priority > maxPriority)
                {
                    found = ent;
                    maxPriority = priority;
                }
            }

            return found ?? _protos.Index(Protos.Chara.At);
        }

        private bool SpawnCard(EntityUid wisher, string wish)
        {
            var charaID = ExtractCharaPrototype(wish);
            var card = _itemGen.GenerateItem(wisher, Protos.Item.Card);
            if (IsAlive(card))
            {
                // TODO card, figure
                EnsureComp<EntityProtoSourceComponent>(card.Value).EntityID = charaID.GetStrongID();
                _mes.Display(Loc.GetString("Elona.Wishes.SomethingAppears.FromNowhere", ("wisher", wisher), ("item", card.Value)));
                return true;
            }
            return false;
        }

        private bool SpawnFigure(EntityUid wisher, string wish)
        {
            var charaID = ExtractCharaPrototype(wish);
            var card = _itemGen.GenerateItem(wisher, Protos.Item.Figurine);
            if (IsAlive(card))
            {
                // TODO card, figure
                EnsureComp<EntityProtoSourceComponent>(card.Value).EntityID = charaID.GetStrongID();
                _mes.Display(Loc.GetString("Elona.Wishes.SomethingAppears.FromNowhere", ("wisher", wisher), ("item", card.Value)));
                return true;
            }
            return false;
        }

        public void Common_OnWish(WishHandlerPrototype proto, ref P_WishHandlerOnWishEvent ev)
        {
            if (ev.Handled)
                return;

            if (ev.Wish.Contains(Loc.GetString("Elona.Wishes.General.Skill.Keyword"), StringComparison.InvariantCultureIgnoreCase))
            {
                Skill_OnWish(proto, ref ev);
            }
            else if (ev.Wish.Contains(Loc.GetString("Elona.Wishes.General.Item.Keyword"), StringComparison.InvariantCultureIgnoreCase))
            {
                Item_OnWish(proto, ref ev);
            }
            else if (ev.Wish.Contains(Loc.GetString("Elona.Wishes.General.Card.Keyword"), StringComparison.InvariantCultureIgnoreCase))
            {
                ev.Handled = SpawnCard(ev.Wisher, ev.Wish);
            }
            else if (ev.Wish.Contains(Loc.GetString("Elona.Wishes.General.Figure.Keyword"), StringComparison.InvariantCultureIgnoreCase))
            {
                ev.Handled = SpawnFigure(ev.Wisher, ev.Wish);
            }
        }

        #endregion
    }

    public sealed class AfterItemCreatedFromWishEvent : EntityEventArgs
    {
        public AfterItemCreatedFromWishEvent(EntityUid wisher, EntityUid item, string wish)
        {
            Wisher = wisher;
            OutItem = item;
            Wish = wish;
        }

        public EntityUid Wisher { get; }
        public string Wish { get; }

        public EntityUid? OutItem { get; set; }
    }
}