using OpenNefia.Content.Activity;
using OpenNefia.Content.Factions;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Parties;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Skills;
using OpenNefia.Content.World;
using OpenNefia.Core.Areas;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using OpenNefia.Core.UserInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Content.Inventory;
using OpenNefia.Content.Identify;

namespace OpenNefia.Content.Dialog
{
    public sealed partial class VanillaDialogSystem : EntitySystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly IFactionSystem _factions = default!;
        [Dependency] private readonly IPartySystem _parties = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly ISkillsSystem _skills = default!;
        [Dependency] private readonly IDialogSystem _dialog = default!;
        [Dependency] private readonly IWorldSystem _world = default!;
        [Dependency] private readonly IUserInterfaceManager _uiManager = default!;
        [Dependency] private readonly IAudioManager _audio = default!;
        [Dependency] private readonly IActivitySystem _activities = default!;
        [Dependency] private readonly IInventorySystem _inv = default!;
        [Dependency] private readonly IStackSystem _stacks = default!;

        public override void Initialize()
        {
            Default_Initialize();
            Ally_Initialize();

            Shopkeeper_Initialize();
            Trainer_Initialize();
            Prostitute_Initialize();
            Innkeeper_Initialize();
            Guard_Initialize();
        }

        public QualifiedDialogNode? OpenTradeMenu(IDialogEngine engine, IDialogNode node)
        {
            foreach (var item in _inv.EnumerateInventoryAndEquipment(engine.Speaker!.Value))
            {
                if (TryComp<IdentifyComponent>(item, out var identify))
                    identify.IdentifyState = IdentifyState.Full;
            }

            var context = new InventoryContext(engine.Player, engine.Speaker.Value, new TradeInventoryBehavior());
            var result = _uiManager.Query<InventoryLayer, InventoryContext, InventoryLayer.Result>(context);

            if (!result.HasValue || result.Value.Data is not InventoryResult.Finished invResult || invResult.TurnResult != TurnResult.Succeeded)
            {
                return engine.GetNodeByID(Protos.Dialog.Default, "YouKidding");
            }

            return engine.GetNodeByID(Protos.Dialog.Default, "Thanks");
        }
    }
}