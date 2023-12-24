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
using OpenNefia.Content.Fame;
using OpenNefia.Content.Weather;
using OpenNefia.Content.World;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Levels;
using OpenNefia.Content.Spells;
using OpenNefia.Content.Actions;
using OpenNefia.Content.Scroll;
using OpenNefia.Content.CurseStates;
using OpenNefia.Content.Potion;
using OpenNefia.Content.Return;

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
        [Dependency] private readonly IHomeSystem _homes = default!;
        [Dependency] private readonly IMapLoader _mapLoader = default!;
        [Dependency] private readonly IEntityFactory _entityFactory = default!;
        [Dependency] private readonly IAreaManager _areas = default!;
        [Dependency] private readonly IWeatherSystem _weathers = default!;
        [Dependency] private readonly ILevelSystem _levels = default!;
        [Dependency] private readonly ISkillsSystem _skills = default!;

        [EngineVariable("LecchoTorte.QuickstartPlayer")]
        private QuickstartChara _quickstartPlayer { get; } = new();

        [EngineVariable("LecchoTorte.QuickstartAllies")]
        private List<QuickstartChara> _quickstartAllies { get; } = new();

        private void UpdateQuickstartChara(EntityUid chara, QuickstartChara data)
        {
            _entityFactory.UpdateEntityComponents(chara, data.Components);

            GenerateQuickstartItems(chara, data);

            var level = _levels.GetLevel(chara);
            for (var i = level; i < data.Level; i++)
            {
                _levels.GainLevel(chara, showMessage: false);
            }

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
                var args = new EntityGenCommonArgs()
                {
                    LevelOverride = allyDef.Level
                };
                var ally = _charaGen.GenerateChara(coords, allyDef.ID, args: EntityGenArgSet.Make(args));
                if (ally != null)
                {
                    _parties.TryRecruitAsAlly(player, ally.Value);
                    UpdateQuickstartChara(ally.Value, allyDef);
                }
            }
        }

        private void MapInitializer(ResourcePath mapFile, P_ScenarioOnGameStartEvent ev, Action<EntityUid, IMap> cb)
        {
            var map = _mapLoader.LoadBlueprint(mapFile);
            map.MemorizeAllTiles();

            var playerSpatial = Spatial(ev.Player);
            playerSpatial.Coordinates = map.AtPosEntity(2, 2);

            _mapMan.SetActiveMap(map.Id);

            PrototypeId<EntityPrototype> areaId = new("LecchoTorte.QuickstartArea");
            GlobalAreaId globalAreaId = new("LecchoTorte.QuickstartArea");
            AreaFloorId floorId = AreaFloorId.Default;

            var area = _areas.CreateArea(areaId, globalAreaId);
            _areas.RegisterAreaFloor(area, floorId, map);

            _homes.SetHome(map);

            var player = ev.Player;

            UpdateQuickstartChara(player, _quickstartPlayer);
            GenerateAllies(player);

            EnsureComp<FameComponent>(player).Fame.Base = 50000;

            foreach (var spell in _protos.EnumeratePrototypes<SpellPrototype>())
                _skills.GainSkill(player, spell.SkillID, new LevelAndPotential() { Level = new(50) });
            foreach (var action in _protos.EnumeratePrototypes<ActionPrototype>())
                _skills.GainSkill(player, action.SkillID);

            // Generate an artifact for oracle
            _itemGen.GenerateItem(map.AtPos(3, 3), Protos.Item.BloodMoon);

            _weathers.TryChangeWeather(Protos.Weather.Rain, GameTimeSpan.FromHours(6));

            cb(player, map);

            foreach (var identify in _entityLookup.EntityQueryInMap<IdentifyComponent>(map))
            {
                identify.IdentifyState = IdentifyState.Full;
            }

            foreach (var childArea in _areas.EnumerateRootAreas(recursive: true))
            {
                if (TryComp<AreaReturnDestinationComponent>(childArea.AreaEntityUid, out var areaDest))
                {
                    areaDest.HasEverBeenVisited = true;
                }
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

                    if (proto.Components.HasComponent<ScrollComponent>()
                      || proto.Components.HasComponent<PotionComponent>())
                    {
                        foreach (var curseState in EnumHelpers.EnumerateValues<CurseState>())
                        {
                            var item = _itemGen.GenerateItem(map.AtPos((2, 4)), proto.GetStrongID(), amount: 99);
                            if (IsAlive(item))
                            {
                                EnsureComp<CurseStateComponent>(item.Value).CurseState = curseState;
                            }
                        }
                    }
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
