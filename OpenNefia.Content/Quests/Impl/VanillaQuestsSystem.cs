using OpenNefia.Core.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Random;
using OpenNefia.Content.Levels;
using OpenNefia.Core.Game;
using OpenNefia.Content.Items;
using OpenNefia.Content.Quests;
using OpenNefia.Content.RandomGen;
using OpenNefia.Core.Areas;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Inventory;
using OpenNefia.Core.Containers;

namespace OpenNefia.Content.Quests
{
    public sealed partial class VanillaQuestsSystem : EntitySystem
    {
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly ILevelSystem _levels = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IItemNameSystem _itemName = default!;
        [Dependency] private readonly IQuestSystem _quests = default!;
        [Dependency] private readonly IRandomGenSystem _randomGen = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IItemGen _itemGen = default!;
        [Dependency] private readonly ICharaGen _charaGen = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IInventorySystem _inv = default!;
        [Dependency] private readonly IContainerSystem _containers = default!;
        [Dependency] private readonly IStackSystem _stacks = default!;

        public override void Initialize()
        {
            Initialize_Supply();
        }
    }
}
