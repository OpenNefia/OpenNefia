using OpenNefia.Content.CurseStates;
using OpenNefia.Content.Effects;
using OpenNefia.Content.Logic;
using OpenNefia.Content.UI;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;

namespace OpenNefia.Content.Spells
{
    public sealed partial class SpellSystem
    {
        [Dependency] private readonly IEffectSystem _effects = default!;
        [Dependency] private readonly ICurseStateSystem _curseStates = default!;
        
        private int CalcAdjustedSpellPower(int power, PrototypeId<SpellPrototype> spellId, CurseState curseState)
        {
            var spell = _protos.Index(spellId);

            if (spell.Alignment == SpellAlignment.Negative)
            {
                switch (curseState)
                {
                    case CurseState.Blessed:
                        return 50;
                    case CurseState.Cursed:
                        case CurseState.Doomed:
                        return (int)(power * 1.5);
                    default:
                        return power;
                }
            }
            else
            {
                switch (curseState)
                {
                    case CurseState.Blessed:
                        return (int)(power * 1.5);
                    case CurseState.Cursed:
                    case CurseState.Doomed:
                        return 50;
                    default:
                        return power;
                }
            }
        }
        
        public TurnResult Cast(PrototypeId<SpellPrototype> spellID, EntityUid target, int power = 0, 
            EntityUid? source = null, EntityUid? item = null, 
            EntityCoordinates? coords = null, CurseState? curseState = null, string effectSource = EffectSources.Default, EffectArgSet? args = null)
        {
            if (curseState == null)
            {
                if (IsAlive(item) && TryComp<CurseStateComponent>(item.Value, out var curseStateComp))
                    curseState = curseStateComp.CurseState;
                else
                    curseState = CurseState.Normal;
            }

            if (coords == null)
            {
                coords = Spatial(target).Coordinates;
            }

            power = CalcAdjustedSpellPower(power, spellID, curseState.Value);
            
            var spell = _protos.Index(spellID);
            
            args ??= new EffectArgSet();
            args.Power = power;
            args.CurseState = curseState.Value;
            
            var commonArgs = args.Ensure<EffectCommonArgs>();
            commonArgs.EffectSource = effectSource;

            source ??= target;

            return _effects.Apply(spell.Effect, source.Value, target, coords.Value, item, args);
        }
    }
}