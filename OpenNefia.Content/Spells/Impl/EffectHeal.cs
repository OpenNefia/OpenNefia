using NuGet.Protocol.Plugins;
using OpenNefia.Content.Combat;
using OpenNefia.Content.CurseStates;
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
using OpenNefia.Content.Rendering;
using OpenNefia.Content.Skills;
using OpenNefia.Content.UI;
using OpenNefia.Core;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.Spells
{
    public class EffectHeal : Effect
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
        [Dependency] private readonly ISkillsSystem _skills = default!;
        [Dependency] private readonly CommonEffectsSystem _commonEffects = default!;
        [Dependency] private readonly IStatusEffectSystem _statusEffects = default!;
        [Dependency] private readonly IMapDrawablesManager _mapDrawables = default!;
        
        // TODO dice
        
        [DataField]
        public LocaleKey MessageKey { get; set; } = "Elona.Effect.Heal.Apply.Slightly";

        [DataField]
        public IEffectDice Dice { get; set; } = new EffectDice();
        
        public override void GetDice(EntityUid source, EntityUid target, EntityCoordinates coords, EntityUid? verb, EffectArgSet args,
            ref Dictionary<string, IDice> result)
        {
            result.Add(nameof(EffectHeal), Dice.GetDice(source, target, coords, verb, args));
        }

        public override TurnResult Apply(EntityUid source, EntityUid target, EntityCoordinates coords, EntityUid? verb, EffectArgSet args)
        {
            _mes.Display(Loc.GetString(MessageKey, ("target", target)), entity: target);

            var dice = Dice.GetDice(source, target, coords, verb, args);
            _commonEffects.Heal(target, dice);

            if (args.CurseState == CurseState.Blessed)
            {
                _statusEffects.Heal(target, Protos.StatusEffect.Sick, 5 + _rand.Next(5));
            }
            else
            {
                if (_rand.OneIn(3))
                {
                    _commonEffects.MakeSickIfCursed(target, args.CurseState);
                }
            }

            var anim = new ParticleMapDrawable(Protos.Asset.HealEffect, Protos.Sound.Heal1);
            _mapDrawables.Enqueue(anim, target);
            
            return TurnResult.Succeeded;
        }
    }
}