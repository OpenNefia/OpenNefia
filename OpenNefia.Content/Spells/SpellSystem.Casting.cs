using OpenNefia.Content.CurseStates;
using OpenNefia.Content.Effects;
using OpenNefia.Content.Effects.New;
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
        
        private int CalcAdjustedSpellPower(int power, EffectAlignment alignment, CurseState curseState)
        {
            if (alignment == EffectAlignment.Negative)
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
        
        // TODO remove
        public TurnResult Cast(PrototypeId<SpellPrototype> spellID, EntityUid target, int power = 0, 
            EntityUid? source = null, EntityUid? item = null, 
            EntityCoordinates? coords = null, CurseState? curseState = null, string effectSource = EffectSources.Default, EffectArgSet? args = null)
        {
            return TurnResult.Aborted;
        }
    }
}