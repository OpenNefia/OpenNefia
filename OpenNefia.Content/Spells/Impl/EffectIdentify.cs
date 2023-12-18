using OpenNefia.Content.Effects;
using OpenNefia.Content.Identify;
using OpenNefia.Content.Inventory;
using OpenNefia.Content.Logic;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.UserInterface;

namespace OpenNefia.Content.Spells
{
    public sealed class EffectIdentify : Effect
    {
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IUserInterfaceManager _uiMan = default!;
        [Dependency] private readonly IIdentifySystem _identify = default!;
        [Dependency] private readonly IStackSystem _stacks = default!;
        
        // TODO dice
        
        public override TurnResult Apply(EntityUid source, EntityUid target, EntityCoordinates coords, EntityUid? verb, EffectArgSet args)
        {
            if (!_gameSession.IsPlayer(source))
            {
                _mes.Display(Loc.GetString("Elona.Common.NothingHappens"));
                args.Ensure<EffectCommonArgs>().OutEffectWasObvious = false;
                
                return TurnResult.Succeeded;
            }

            var context = new InventoryContext(source, target, new IdentifyInventoryBehavior());
            var result = _uiMan.Query<InventoryLayer, InventoryContext, InventoryLayer.Result>(context);

            if (!result.HasValue || !EntityManager.IsAlive(result.Value.SelectedItem))
                return TurnResult.Succeeded;

            var item = result.Value.SelectedItem.Value;
            var identify = EntityManager.EnsureComponent<IdentifyComponent>(item);
            var level = IdentifyState.None;
            if (args.Power >= identify.IdentifyDifficulty)
                level = IdentifyState.Full;

            var identifyResult = _identify.Identify(item, level);
            if (identifyResult.ChangedState)
            {
                if (identifyResult.NewState != IdentifyState.Full)
                {
                    _mes.Display(Loc.GetString("Elona.Effect.Identify.Partially", ("item", item)));
                }
                else
                {
                    _mes.Display(Loc.GetString("Elona.Effect.Identify.Fully", ("item", item)));
                }
            }
            else
            {
                _mes.Display(Loc.GetString("Elona.Effect.Identify.NeedMorePower", ("item", item)));
            }

            _stacks.TryStackAtSamePos(item, showMessage: true);
            return TurnResult.Succeeded;
        }
    }
}