using OpenNefia.Content.CharaMake;
using OpenNefia.Content.Parties;
using OpenNefia.Content.RandomGen;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.TitleScreen;
using OpenNefia.Core.Game;
using OpenNefia.Content.Skills;
using OpenNefia.Core.Rendering;
using OpenNefia.Content.VanillaAI;
using OpenNefia.Content.Resists;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.Damage;
using OpenNefia.Content.Food;
using OpenNefia.Core.Maps;
using OpenNefia.Content.Currency;
using OpenNefia.Core.Audio;
using OpenNefia.Content.Identify;
using OpenNefia.Content.Chests;
using OpenNefia.Content.Inventory;
using OpenNefia.Content.EquipSlots;
using OpenNefia.Content.Items.Impl;
using OpenNefia.Content.Enchantments;
using OpenNefia.Content.Materials;
using OpenNefia.Content.LivingWeapon;
using OpenNefia.Content.Qualities;
using OpenNefia.Content.GameObjects;
using OpenNefia.Core.Utility;
using OpenNefia.Content.Scenarios;
using static OpenNefia.Content.Prototypes.Protos;
using OpenNefia.Content.Home;
using OpenNefia.Core.EngineVariables;
using static OpenNefia.Core.Prototypes.EntityPrototype;
using OpenNefia.Content.Equipment;
using OpenNefia.Core.Areas;

namespace OpenNefia.LecchoTorte.QuickStart
{
    public sealed class QuickStartScenariosSystem : EntitySystem
    {
        [Dependency] private readonly IPartySystem _parties = default!;
        [Dependency] private readonly ICharaGen _charaGen = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IRefreshSystem _refresh = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly IDamageSystem _damage = default!;
        [Dependency] private readonly IItemGen _itemGen = default!;
        [Dependency] private readonly IMapManager _mapMan = default!;
        [Dependency] private readonly IEntityLookup _entityLookup = default!;
        [Dependency] private readonly IEquipSlotsSystem _equipSlots = default!;
        [Dependency] private readonly IEnchantmentSystem _enchantments = default!;
        [Dependency] private readonly IEquipmentSystem _equip = default!;
        [Dependency] private readonly IMaterialSystem _materials = default!;
        [Dependency] private readonly IHomeSystem _homes = default!;
        [Dependency] private readonly IMapLoader _mapLoader = default!;
        [Dependency] private readonly IEntityFactory _entityFactory = default!;
        [Dependency] private readonly IAreaManager _areas = default!;

        [EngineVariable("LecchoTorte.QuickstartPlayer")]
        private QuickstartChara _quickstartPlayer { get; } = new();

        [EngineVariable("LecchoTorte.QuickstartAllies")]
        private List<QuickstartChara> _quickstartAllies { get; } = new();

        private void UpdateQuickstartChara(EntityUid chara, QuickstartChara data)
        {
            _entityFactory.UpdateEntityComponents(chara, data.Components);

            GenerateQuickstartItems(chara, data);

            var skills = EnsureComp<SkillsComponent>(chara);

            foreach (var entry in data.Skills)
            {
                skills.Skills[entry.Key] = entry.Value;
            }

            var resists = EnsureComp<ResistsComponent>(chara);

            foreach (var entry in data.Resists)
            {
                resists.Resists[entry.Key] = entry.Value;
            }

            _refresh.Refresh(chara);
            _damage.HealToMax(chara);
        }

        private void GenerateQuickstartItems(EntityUid chara, QuickstartChara data)
        {
            var inv = EntityManager.GetComponent<InventoryComponent>(chara);
            var pos = Spatial(chara).MapPosition;

            foreach (var def in data.Items)
            {
                switch (def.Location)
                {
                    case QuickstartEntityLocation.Inventory:
                        _itemGen.GenerateItem(inv.Container, def.ID, amount: def.Amount);
                        break;
                    case QuickstartEntityLocation.Equipment:
                        var item = _itemGen.GenerateItem(pos, def.ID, amount: def.Amount);
                        if (IsAlive(item))
                        {
                            _equip.EquipIfHigherValueInSlotForNPC(chara, item.Value);
                        }
                        break;
                    case QuickstartEntityLocation.Ground:
                        _itemGen.GenerateItem(pos, def.ID, amount: def.Amount);
                        break;
                }
            }
        }

        private void GenerateAllies(EntityUid player)
        {
            var coords = EntityManager.GetComponent<SpatialComponent>(player).MapPosition;

            foreach (var allyDef in _quickstartAllies)
            {
                var ally = _charaGen.GenerateChara(coords, allyDef.ID);
                if (ally != null)
                {
                    _parties.RecruitAsAlly(player, ally.Value);
                    UpdateQuickstartChara(ally.Value, allyDef);
                }
            }
        }

        private void MapInitializer(ResourcePath mapFile, P_ScenarioOnGameStartEvent ev, Action<EntityUid, IMap> cb)
        {
            var map = _mapLoader.LoadBlueprint(mapFile);
            map.MemorizeAllTiles();

            _homes.SetHome(map);

            var playerSpatial = Spatial(ev.Player);
            playerSpatial.Coordinates = map.AtPosEntity(2, 2);

            ev.OutActiveMap = map;

            PrototypeId<EntityPrototype> areaId = new("LecchoTorte.QuickstartArea");
            GlobalAreaId globalAreaId = new("LecchoTorte.Quickstart");
            AreaFloorId floorId = new("Default", 1);

            var area = _areas.CreateArea(areaId, globalAreaId);
            _areas.RegisterAreaFloor(area, floorId, map);

            var player = ev.Player;

            UpdateQuickstartChara(player, _quickstartPlayer);
            GenerateAllies(player);

            cb(player, map);

            foreach (var identify in _entityLookup.EntityQueryInMap<IdentifyComponent>(map))
            {
                identify.IdentifyState = IdentifyState.Full;
            }
        }

        public void Quickstart_OnGameStart(ScenarioPrototype _proto, P_ScenarioOnGameStartEvent ev)
        {
            MapInitializer(new("/Maps/LecchoTorte/Quickstart.yml"), ev, (player, map) =>
            {
                foreach (var proto in _protos.EnumeratePrototypes<EntityPrototype>())
                {
                    if (proto.Components.HasComponent<FoodComponent>())
                        _itemGen.GenerateItem(map.AtPos((2, 2)), proto.GetStrongID(), amount: 99);
                }

                foreach (var proto in _protos.EnumeratePrototypes<MusicPrototype>())
                {
                    var item = _itemGen.GenerateItem(map.AtPos((2, 3)), Protos.Item.Disc);
                    if (IsAlive(item))
                        Comp<MusicDiscComponent>(item.Value).MusicID = proto.GetStrongID();
                }

                foreach (var proto in _protos.EnumeratePrototypes<EntityPrototype>().Where(p => p.Components.HasComponent<ChestComponent>()))
                {
                    _itemGen.GenerateItem(map.AtPos(3, 2), proto.GetStrongID(), amount: 99);
                }
            });
        }

        private const int UnloadEntityCount = 2500;

        public void EntityUnload_OnGameStart(ScenarioPrototype _proto, P_ScenarioOnGameStartEvent ev)
        {
            MapInitializer(new("/Maps/LecchoTorte/EntityUnload.yml"), ev, (player, map) =>
            {
                for (var i = 0; i < UnloadEntityCount; i++)
                {
                    var chara = _charaGen.GenerateChara(map.AtPos(5, 5), Protos.Chara.FireDrake);
                    if (IsAlive(chara))
                    {
                        _damage.Kill(chara.Value);
                    }
                }
            });
        }
    }
}
