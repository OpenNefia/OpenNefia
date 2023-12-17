using OpenNefia.Content.CurseStates;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Effects.New
{
    /// <summary>
    /// Handles spell casting checks.
    /// </summary>
    public sealed class EffectTypeSpellSystem : EntitySystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;

        public override void Initialize()
        {
            SubscribeComponent<EffectTypeSpellComponent, CastEffectEvent>(CastMagic);
        }

        private int CalcAdjustedPower(SpellAlignment alignment, int power, CurseState curseState)
        {
            switch (alignment)
            {
                case SpellAlignment.Neutral:
                default:
                    return power;
                case SpellAlignment.Positive:
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
                case SpellAlignment.Negative:
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
        }

        private void CastMagic(EntityUid effectUid, EffectTypeSpellComponent component, CastEffectEvent args)
        {
            if (args.Handled)
                return;

            EntityUid target;
            EntityCoordinates? targetCoords = null;
            if (args.Target == null)
            {
                var ev = new GetEffectTargetEvent(args.Source, args.Args);
                RaiseEvent(effectUid, ev);
                if (ev.Cancelled)
                {
                    args.Handle(TurnResult.Aborted);
                    return;
                }

                if (ev.OutTarget == null)
                {
                    // Set target to the same as source as a fallback.
                    target = args.Source;
                }
                else
                {
                    target = ev.OutTarget.Value;
                }
                targetCoords = ev.OutCoords;
            }
            else
            {
                target = args.Target.Value;
            }

            var sourceCoords = Spatial(args.Source).Coordinates;
            targetCoords ??= Spatial(target).Coordinates;

            var common = args.Args.Ensure<EffectCommonArgs>();
            if (!common.NoInheritCurseState)
            {
                args.Args.CurseState = CurseStates.CurseState.Normal;

                if (IsAlive(common.Item) && TryComp<CurseStateComponent>(common.Item, out var curseStateComp))
                    args.Args.CurseState = curseStateComp.CurseState;
            }

            args.Args.Power = CalcAdjustedPower(component.Alignment, args.Args.Power, args.Args.CurseState);

            var ev2 = new ApplyEffectAreaEvent(args.Source, target, sourceCoords, targetCoords.Value, args.Args);
            Raise(effectUid, ev2);

            args.Handle(ev2.TurnResult);
        }
    }
}