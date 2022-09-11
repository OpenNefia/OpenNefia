using OpenNefia.Content.BaseAnim;
using OpenNefia.Content.CurseStates;
using OpenNefia.Content.Effects;
using OpenNefia.Content.EquipSlots;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Identify;
using OpenNefia.Content.Inventory;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UserInterface;

namespace OpenNefia.Content.Spells
{
    public sealed class EffectUncurse : Effect
    {
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IUserInterfaceManager _uiMan = default!;
        [Dependency] private readonly IIdentifySystem _identify = default!;
        [Dependency] private readonly IStackSystem _stacks = default!;
        [Dependency] private readonly IInventorySystem _inv = default!;
        [Dependency] private readonly ICurseStateSystem _curseStates = default!;
        [Dependency] private readonly IEquipSlotsSystem _equipSlots = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMapDrawablesManager _mapDrawables = default!;
        [Dependency] private readonly IRefreshSystem _refresh = default!;
        [Dependency] private readonly IEffectSystem _effects = default!;

        public override TurnResult Apply(EntityUid source, EntityUid target, EntityCoordinates coords, EntityUid? verb, EffectArgSet args)
        {
            switch (args.CurseState)
            {
                case CurseState.Normal:
                    _mes.Display(Loc.GetString("Elona.Effect.Uncurse.Apply.Normal", ("target", target)), entity: target);
                    break;
                case CurseState.Blessed:
                    _mes.Display(Loc.GetString("Elona.Effect.Uncurse.Apply.Blessed", ("target", target)), entity: target);
                    break;
                case CurseState.Cursed:
                case CurseState.Doomed:
                    _mes.Display(Loc.GetString("Elona.Effect.Common.CursedLaughter"), entity: target);
                    return _effects.Apply<EffectCurse>(source, target, coords, verb, args);
            }

            bool Filter(EntityUid uid)
            {
                if (!_curseStates.IsCursed(uid))
                    return false;
                
                // Only uncurse unequipped items if the scroll is blessed.
                if (args.CurseState != CurseState.Blessed && !_equipSlots.IsEquippedOnAnySlot(uid))
                    return false;

                return true;
            }

            var totalUncursed = 0;
            var totalResisted = 0;

            foreach (var item in _inv.EnumerateInventoryAndEquipment(target).Where(Filter))
            {
                var cursePower = 0;

                var curseState = EntityManager.GetComponent<CurseStateComponent>(item);
                if (curseState.CurseState == CurseState.Cursed)
                    cursePower = _rand.Next(200) + 1;
                else if (curseState.CurseState == CurseState.Doomed)
                    cursePower = _rand.Next(1000) + 1;

                if (args.CurseState == CurseState.Blessed)
                    cursePower = cursePower / 2 + 1;

                if (cursePower > 0 && args.Power >= cursePower)
                {
                    totalUncursed++;
                    curseState.CurseState = CurseState.Normal;
                    _stacks.TryStackAtSamePos(item, showMessage: true);
                }
                else
                {
                    totalResisted++;
                }
            }

            if (totalUncursed > 0)
            {
                if (args.CurseState == CurseState.Blessed)
                {
                    _mes.Display(Loc.GetString("Elona.Effect.Uncurse.Result.Items", ("target", target)));
                }
                else
                {
                    _mes.Display(Loc.GetString("Elona.Effect.Uncurse.Result.Equipment", ("target", target)));
                }
            }
            if (totalResisted > 0)
            {
                _mes.Display(Loc.GetString("Elona.Effect.Uncurse.Result.Resisted", ("target", target)));
            }

            if (totalUncursed == 0 && totalResisted == 0)
            {
                _mes.Display(Loc.GetString("Elona.Common.NothingHappens"));
                args.Ensure<EffectCommonArgs>().Obvious = false;
            }
            else
            {
                var anim = new BasicAnimMapDrawable(Protos.BasicAnim.AnimSparkle);
                _mapDrawables.Enqueue(anim, target);
            }

            _refresh.Refresh(target);
            
            return TurnResult.Succeeded;
        }
    }
}