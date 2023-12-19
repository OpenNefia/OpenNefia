using OpenNefia.Content.Effects;
using OpenNefia.Content.Identify;
using OpenNefia.Content.Inventory;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Skills;
using OpenNefia.Content.StatusEffects;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Random;
using OpenNefia.Content.CurseStates;
using OpenNefia.Content.Effects.New;

namespace OpenNefia.Content.Scroll
{
    public interface IScrollSystem : IEntitySystem
    {
    }
    
    public sealed class ScrollSystem : EntitySystem, IScrollSystem
    {
        [Dependency] private readonly IStackSystem _stacks = default!;
        [Dependency] private readonly IStatusEffectSystem _statusEffects = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly ISkillsSystem _skills = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IIdentifySystem _identify = default!;
        [Dependency] private readonly IEffectSystem _effects = default!;
        [Dependency] private readonly ICurseStateSystem _curseStates = default!;
        [Dependency] private readonly INewEffectSystem _newEffects = default!;
        
        public override void Initialize()
        {
            SubscribeComponent<ScrollComponent, GetVerbsEventArgs>(Scroll_GetVerbs);
        }

        private void Scroll_GetVerbs(EntityUid uid, ScrollComponent component, GetVerbsEventArgs args)
        {
            args.OutVerbs.Add(new Verb(ReadInventoryBehavior.VerbTypeRead, "Read Scroll",
                () => ReadScroll(args.Source, args.Target)));
        }

        private TurnResult ReadScroll(EntityUid reader, EntityUid scroll, ScrollComponent? scrollComp = null)
        {
            if (!Resolve(scroll, ref scrollComp))
                return TurnResult.Aborted;

            if (_stacks.GetCount(scroll) < scrollComp.AmountConsumedOnRead)
                return TurnResult.Aborted;

            if (_statusEffects.HasEffect(reader, Protos.StatusEffect.Blindness))
            {      
                _mes.Display(Loc.GetString("Elona.Read.CannotSee", ("reader", reader)), entity: reader);
                return TurnResult.Failed;
            }

            if (_statusEffects.HasEffect(reader, Protos.StatusEffect.Dimming) 
                || _statusEffects.HasEffect(reader, Protos.StatusEffect.Confusion))
            {
                if (!_rand.OneIn(4))
                {
                    _mes.Display(Loc.GetString("Elona.Scroll.Read.DimmedOrConfused", ("reader", reader)), entity: reader);
                    return TurnResult.Failed;
                }
            }

            _mes.Display(Loc.GetString("Elona.Scroll.Read.Execute", ("reader", reader), ("scroll", scroll)), entity: reader);

            if (scrollComp.AmountConsumedOnRead > 0)
            {
                _stacks.Use(scroll, scrollComp.AmountConsumedOnRead);
                _skills.GainSkillExp(reader, Protos.Skill.Literacy, 25, 2);
            }
            
            var coords = Spatial(reader).Coordinates;

            TurnResult result = TurnResult.Failed; // At minimum, a turn should pass.
            var obvious = false;
            foreach (var spec in scrollComp.Effects.EnumerateEffectSpecs())
            {
                var args = new EffectCommonArgs()
                {
                    EffectSource = EffectSources.Scroll,
                    CurseState = _curseStates.GetCurseState(scroll),
                    Item = scroll
                };
                var newResult = _newEffects.Apply(reader, reader, coords, spec.ID, EffectArgSet.Make(args));
                result = result.Combine(newResult);
                obvious = obvious || args.OutEffectWasObvious;
            }

            if (_gameSession.IsPlayer(reader))
            {
                if (obvious && IsAlive(scroll))
                {
                    _identify.Identify(scroll, IdentifyState.Name);
                }
            }

            return result;
        }
    }
}