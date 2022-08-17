using OpenNefia.Content.Activity;
using OpenNefia.Content.Inventory;
using OpenNefia.Content.Levels;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.RandomGen;
using OpenNefia.Content.SaveLoad;
using OpenNefia.Content.UI;
using OpenNefia.Core.Areas;
using OpenNefia.Core.Audio;
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

namespace OpenNefia.Content.TreasureMap
{
    public sealed class TreasureMapSystem : EntitySystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly IInventorySystem _inv = default!;
        [Dependency] private readonly IAudioManager _audio = default!;
        [Dependency] private readonly IPlayerQuery _playerQuery = default!;
        [Dependency] private readonly IItemGen _itemGen = default!;
        [Dependency] private readonly IRandomGenSystem _randomGen = default!;
        [Dependency] private readonly ILevelSystem _levels = default!;
        [Dependency] private readonly IStackSystem _stacks = default!;
        [Dependency] private readonly ISaveLoadSystem _saveLoad = default!;

        public override void Initialize()
        {
            SubscribeEntity<EntityFinishedDiggingSpotEvent>(HandleFinishedDiggingSpot);
        }

        private void HandleFinishedDiggingSpot(EntityUid uid, EntityFinishedDiggingSpotEvent args)
        {
            var treasureMap = _inv.EnumerateItems(uid).Select(i => CompOrNull<TreasureMapComponent>(i)).FirstOrDefault();
            if (treasureMap == null)
                return;

            _audio.Play(Protos.Sound.Chest1, uid);
            _mes.Display(Loc.GetString("Elona.Dig.Spot.SomethingIsThere"), UiColors.MesYellow);
            _playerQuery.PromptMore();
            _audio.Play(Protos.Sound.Ding2);

            _itemGen.GenerateItem(uid, Protos.Item.SmallMedal, amount: 2 + _rand.Next(2));
            _itemGen.GenerateItem(uid, Protos.Item.PlatinumCoin, amount: 1 + _rand.Next(3));
            _itemGen.GenerateItem(uid, Protos.Item.GoldPiece, amount: _rand.Next(10000) + 20001);

            for (var i = 0; i < 4; i++)
            {
                var filter = new ItemFilter()
                {
                    MinLevel = _randomGen.CalcObjectLevel(_levels.GetLevel(uid) + 10),
                    Quality = i == 0 ? Qualities.Quality.God : _randomGen.CalcObjectQuality(Qualities.Quality.Good),
                    Tags = new[] { _rand.Pick(RandomGenConsts.FilterSets.Chest) }
                };
                _itemGen.GenerateItem(uid, filter);
            }

            _mes.Display(Loc.GetString("Elona.Common.SomethingIsPut"));
            _stacks.Use(treasureMap.Owner, 1);
            _saveLoad.QueueAutosave();
        }
    }
}