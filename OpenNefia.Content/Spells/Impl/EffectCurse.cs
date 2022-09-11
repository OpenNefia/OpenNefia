using OpenNefia.Content.BaseAnim;
using OpenNefia.Content.CurseStates;
using OpenNefia.Content.Effects;
using OpenNefia.Content.Enchantments;
using OpenNefia.Content.Equipment;
using OpenNefia.Content.EquipSlots;
using OpenNefia.Content.Feats;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Identify;
using OpenNefia.Content.Inventory;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Parties;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Skills;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.UserInterface;

namespace OpenNefia.Content.Spells
{
    public sealed class EffectCurse : Effect
    {
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IUserInterfaceManager _uiMan = default!;
        [Dependency] private readonly IIdentifySystem _identify = default!;
        [Dependency] private readonly IStackSystem _stacks = default!;
        [Dependency] private readonly IPartySystem _parties = default!;
        [Dependency] private readonly ISkillsSystem _skills = default!;
        [Dependency] private readonly IEnchantmentSystem _enchantments = default!;
        [Dependency] private readonly IFeatsSystem _feats = default!;
        [Dependency] private readonly IInventorySystem _inv = default!;
        [Dependency] private readonly IEquipSlotsSystem _equipSlots = default!;
        [Dependency] private readonly IRefreshSystem _refresh = default!;
        [Dependency] private readonly IAudioManager _audio = default!;
        [Dependency] private readonly IMapDrawablesManager _mapDrawables = default!;
        
        /// <summary>
        /// Show "X points their finger at..."
        /// </summary>
        [DataField]
        public bool IsAction { get; set; } = false;
        
        public override TurnResult Apply(EntityUid source, EntityUid target, EntityCoordinates coords, EntityUid? verb, EffectArgSet args)
        {
            if (IsAction)
                _mes.Display(Loc.GetString("Elona.Effect.Curse.Spell", ("source", source), ("target", target)), entity: target);

            var cursePower = args.Power / 2;
            if (args.CurseState == CurseState.Cursed || args.CurseState == CurseState.Doomed)
                cursePower *= 100;

            var resistance = 75 + _skills.Level(target, Protos.Skill.AttrLuck);
            var enchantmentPower = _enchantments.GetTotalEquippedEnchantmentPower<EncResistCurseComponent>(target);
            if (enchantmentPower > 0)
                resistance += enchantmentPower / 2;

            if (_rand.Next(resistance) > cursePower)
                return TurnResult.Failed;

            if (_parties.IsInPlayerParty(target))
            {
                if (_feats.HasFeat(target, Protos.Feat.ResCurse) && _rand.OneIn(3))
                {
                    _mes.Display(Loc.GetString("Elona.Effect.Curse.NoEffect"));
                    return TurnResult.Failed;
                }
            }

            bool Filter(EntityUid uid)
            {
                if (!EntityManager.TryGetComponent<CurseStateComponent>(uid, out var curseState))
                    return false;
                if (curseState.CurseState == CurseState.Blessed && _rand.OneIn(10))
                    return false;
                return true;
            }

            var candidates = _equipSlots.EnumerateEquippedEntities(target).Where(Filter).ToList();

            if (candidates.Count == 0)
            {
                var items = _inv.EnumerateInventory(target).ToList();
                for (var i = 0; i < 200; i++)
                {
                    var item = _rand.Pick(items);
                    if (Filter(item))
                    {
                        candidates.Add(item);
                    }
                }
            }

            if (candidates.Count == 0)
            {
                _mes.Display(Loc.GetString("Elona.Common.NothingHappens"));
                args.Ensure<EffectCommonArgs>().Obvious = false;
                
                return TurnResult.Failed;
            }

            var chosen = _rand.Pick(candidates);

            _mes.Display(Loc.GetString("Elona.Effect.Curse.Apply", ("target", target), ("item", chosen)), entity: target);

            var curseStateComp = EntityManager.GetComponent<CurseStateComponent>(chosen);
            if (curseStateComp.CurseState == CurseState.Cursed)
                curseStateComp.CurseState = CurseState.Doomed;
            else
                curseStateComp.CurseState = CurseState.Cursed;

            _refresh.Refresh(target);
            _audio.Play(Protos.Sound.Curse3);
            var anim = new BasicAnimMapDrawable(Protos.BasicAnim.AnimCurse);
            _mapDrawables.Enqueue(anim, target);
            _stacks.TryStackAtSamePos(chosen, showMessage: true);

            return TurnResult.Succeeded;
        }
    }
}