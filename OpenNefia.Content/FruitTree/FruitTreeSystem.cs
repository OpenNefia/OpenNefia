using OpenNefia.Content.EntityGen;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Maps;
using OpenNefia.Content.Pickable;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.RandomGen;
using OpenNefia.Core.Areas;
using OpenNefia.Core.Audio;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.FruitTree
{
    public sealed class FruitTreeSystem : EntitySystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly IStackSystem _stacks = default!;
        [Dependency] private readonly IAudioManager _audio = default!;
        [Dependency] private readonly IItemGen _itemGen = default!;

        public override void Initialize()
        {
            SubscribeComponent<FruitTreeComponent, EntityBeingGeneratedEvent>(InitializeFruitTree);
            SubscribeComponent<FruitTreeComponent, OnItemRenewMajorEvent>(HandleRenew);
            SubscribeComponent<FruitTreeComponent, EntityBashedEventArgs>(HandleBashed);
        }

        // TODO
        private readonly PrototypeId<EntityPrototype>[] FruitTreeFruits = new[]
        {
            Protos.Item.Apple,
            Protos.Item.Grape,
            Protos.Item.Orange,
            Protos.Item.Lemon,
            Protos.Item.Strawberry,
            Protos.Item.Cherry
        };

        private void InitializeFruitTree(EntityUid uid, FruitTreeComponent fruitTree, ref EntityBeingGeneratedEvent args)
        {
            fruitTree.FruitAmount = _rand.Next(2) + 3;
            fruitTree.FruitItemID = _rand.Pick(FruitTreeFruits);
        }

        private void HandleRenew(EntityUid uid, FruitTreeComponent fruitTree, OnItemRenewMajorEvent args)
        {
            if (fruitTree.FruitAmount < 10)
            {
                fruitTree.FruitAmount++;
                if (TryComp<ChipComponent>(uid, out var chip))
                {
                    chip.ChipID = Protos.Chip.ItemTreeOfFruits;
                }
            }
        }

        private void HandleBashed(EntityUid uid, FruitTreeComponent fruitTree, EntityBashedEventArgs args)
        {
            args.Handle(TurnResult.Succeeded);
            
            if (!_stacks.TrySplit(uid, 1, out var split))
                return;

            uid = split;
            fruitTree = Comp<FruitTreeComponent>(split);

            _audio.Play(Protos.Sound.Bash1, uid);
            _mes.Display(Loc.GetString("Elona.FruitTree.Bash", ("tree", uid)));

            if (CompOrNull<PickableComponent>(uid)?.OwnState == OwnState.Special || fruitTree.FruitAmount <= 0)
            {
                _mes.Display(Loc.GetString("Elona.FruitTree.NoFruits"));
                return;
            }

            fruitTree.FruitAmount--;
            if (fruitTree.FruitAmount <= 0 && TryComp<ChipComponent>(uid, out var chip))
            {
                chip.ChipID = Protos.Chip.ItemTreeOfFruitless;
            }

            var spawnPos = Spatial(uid).MapPosition;
            var spawnPosOneDown = new MapCoordinates(spawnPos.MapId, spawnPos.Position + (0, 1));

            if (!TryMap(spawnPos, out var map))
                return;

            if (map.IsFloor(spawnPosOneDown))
                spawnPos = spawnPosOneDown;

            var fruit = _itemGen.GenerateItem(spawnPos, fruitTree.FruitItemID);
            if (IsAlive(fruit))
                _mes.Display(Loc.GetString("Elona.FruitTree.FallsDown", ("fruit", fruit.Value)));
        }
    }
}