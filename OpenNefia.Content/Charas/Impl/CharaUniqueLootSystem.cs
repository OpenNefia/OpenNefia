using OpenNefia.Content.Logic;
using OpenNefia.Content.Loot;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.RandomGen;
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
using OpenNefia.Content.Levels;
using OpenNefia.Core.Game;
using OpenNefia.Content.GameObjects;

namespace OpenNefia.Content.Charas.Impl
{

    public sealed class CharaUniqueLootSystem : EntitySystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly ILevelSystem _levels = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IStackSystem _stacks = default!;

        public override void Initialize()
        {
            SubscribeComponent<RogueBossLootComponent, OnGenerateLootDropsEvent>(RogueBoss_GenerateLoot);
        }

        private void RogueBoss_GenerateLoot(EntityUid uid, RogueBossLootComponent component, OnGenerateLootDropsEvent args)
        {
            // >>>>>>>> elona122/shade2/item.hsp:379 	if cId(rc)=302{ ...
            var count = 2 + _rand.Next(4);
            for (var i = 0; i < count; i++)
            {
                var itemFilter = new ItemFilter()
                {
                    MinLevel = _levels.GetLevel(_gameSession.Player),
                    Quality = Qualities.Quality.Normal,
                    Tags = new[] { Protos.Tag.ItemCatCargo }
                };
                OnGenerateLootItemDelegate onGenerate = (EntityUid item, EntityUid victim, EntityUid? attacker) =>
                {
                    var baseValue = CompOrNull<ValueComponent>(item)?.Value.Base ?? 0;
                    if (baseValue < 800)
                        _stacks.SetCount(item, _rand.Next(5) + 1);
                };
                var drop = new LootDrop(itemFilter, onGenerate);
                args.OutLootDrops.Add(drop);
            }
            // <<<<<<<< elona122/shade2/item.hsp:383 	} ...
        }
    }
}