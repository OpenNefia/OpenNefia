#nullable enable
#r "System.Runtime"
#r "System.Collections"
#r "NLua, Version=1.6.0.0, Culture=neutral, PublicKeyToken=6a194c04b9c89217"
#r "C:/users/yuno/build/OpenNefia.NET/OpenNefia.EntryPoint/bin/Debug/net8.0/OpenNefia.Core.dll"
#r "C:/Users/yuno/build/OpenNefia.NET/OpenNefia.Content/Resources/Assemblies/OpenNefia.Content.dll"

using NLua;
using OpenNefia.Core;
using OpenNefia.Content;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maths;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Areas;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Game;
using OpenNefia.Core.Utility;
using OpenNefia.Core.GameObjects;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.RandomText;
using OpenNefia.Content.Nefia;
using OpenNefia.Content.Maps;
using OpenNefia.Content.GameObjects.EntitySystems.Tag;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.RandomGen;
using OpenNefia.Content.TitleScreen;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Directions;
using OpenNefia.Content.Rendering;
using OpenNefia.Core.Random;
using OpenNefia.Content.RandomEvent;
using OpenNefia.Content.World;
using OpenNefia.Core.DebugServer;
using OpenNefia.Content.Activity;
using OpenNefia.Content.Logic;
using OpenNefia.Core.UserInterface;
using OpenNefia.Content.UI.Layer;
using OpenNefia.Content.Skills;
using OpenNefia.Content.Inventory;
using OpenNefia.Content.Spells;
using OpenNefia.Content.StatusEffects;
using OpenNefia.Content.Factions;
using OpenNefia.Content.Damage;
using OpenNefia.Content.Charas;
using OpenNefia.Content.EntityGen;
using OpenNefia.Core.Configuration;
using OpenNefia.Content.Levels;
using OpenNefia.Content.Effects;
using OpenNefia.Content.CurseStates;
using OpenNefia.Core.ViewVariables;
using OpenNefia.Content.DebugView;
using OpenNefia.Content.Spells;
using OpenNefia.Content.Identify;
using OpenNefia.Content.Quests;
using OpenNefia.Content.Food;
using OpenNefia.Content.Hunger;
using OpenNefia.Content.Weather;
using OpenNefia.Content.Items;
using OpenNefia.Content.Scene;
using OpenNefia.Content.Effects;
using OpenNefia.Content.Wishes;

var _entityMan = IoCManager.Resolve<IEntityManager>();
var _mapMan = IoCManager.Resolve<IMapManager>();
var _areaMan = IoCManager.Resolve<IAreaManager>();
var _entityGen = EntitySystem.Get<IEntityGen>();
var _itemGen = EntitySystem.Get<IItemGen>();
var _charaGen = EntitySystem.Get<ICharaGen>();
var _gameSession = IoCManager.Resolve<IGameSessionManager>();
var _rand = IoCManager.Resolve<IRandom>();
var _randEvents = EntitySystem.Get<IRandomEventSystem>();
var _protos = IoCManager.Resolve<IPrototypeManager>();
var _world = EntitySystem.Get<IWorldSystem>();
var _lookup = EntitySystem.Get<IEntityLookup>();
var _activities = EntitySystem.Get<IActivitySystem>();
var _playerQuery = IoCManager.Resolve<IPlayerQuery>();
var _uiMan = IoCManager.Resolve<IUserInterfaceManager>();
var _loc = IoCManager.Resolve<ILocalizationManager>();
var _inv = EntitySystem.Get<IInventorySystem>();
var _mapEntrance = EntitySystem.Get<IMapEntranceSystem>();
var _statusEffects = EntitySystem.Get<IStatusEffectSystem>();
var _tags = EntitySystem.Get<ITagSystem>();
var _factions = EntitySystem.Get<IFactionSystem>();
var _damage = EntitySystem.Get<IDamageSystem>();
var _skills = EntitySystem.Get<ISkillsSystem>();
var _spells = EntitySystem.Get<ISpellSystem>();
var _config = IoCManager.Resolve<IConfigurationManager>();
var _refresh = EntitySystem.Get<IRefreshSystem>();
var _levels = EntitySystem.Get<ILevelSystem>();
var _effects = EntitySystem.Get<IEffectSystem>();
var _quests = EntitySystem.Get<IQuestSystem>();
var _weather = EntitySystem.Get<IWeatherSystem>();
var _scenes = EntitySystem.Get<ISceneSystem>();
var _wishes = EntitySystem.Get<IWishSystem>();

public EntityUid player() => _gameSession.Player;
public SpatialComponent playerS() => _entityMan.GetComponent<SpatialComponent>(_gameSession.Player);
public IMap curMap() => _mapMan.ActiveMap!;
public EntityUid curMapEnt() => _mapMan.ActiveMap!.MapEntityUid;

public MapCoordinates promptPos()
{
    var args = new PositionPrompt.Args(playerS().MapPosition);
    var result = _uiMan.Query<PositionPrompt, PositionPrompt.Args, PositionPrompt.Result>(args);
    return result.Value.Coords;
}

public T res<T>() => IoCManager.Resolve<T>();
public T sys<T>() where T : IEntitySystem => EntitySystem.Get<T>();

public T comp<T>(EntityUid? uid = null) where T : class, IComponent
{
    return _entityMan.GetComponent<T>(uid ?? player());
}

public SpatialComponent entityAt()
{
    var coords = promptPos();
    return _lookup.GetLiveEntitiesAtCoords(coords).First();
}

public SpatialComponent spatial(EntityUid uid)
{
    return _entityMan.GetComponent<SpatialComponent>(uid);
}

public void gotoArea(string id)
{
    var entrance = new MapEntrance()
    {
        MapIdSpecifier = new GlobalAreaMapIdSpecifier(new GlobalAreaId(id)),
        StartLocation = new MapEdgesLocation()
    };
    _mapEntrance.UseMapEntrance(player(), entrance);
}

public LevelAndPotential skill(PrototypeId<SkillPrototype> id)
{
    return _entityMan.GetComponent<SkillsComponent>(player()).Ensure(id);
}

public void gainSkill(PrototypeId<SkillPrototype> id)
{
    _skills.GainSkill(player(), id);
}

public void skillLv(PrototypeId<SkillPrototype> id, int level)
{
    var sk = skill(id);
    sk.Level.Base = level;
}

public EntityUid? spawn(PrototypeId<EntityPrototype> id, int? amount = null)
{
    var proto = _protos.Index(id);

    if (proto.Components.HasComponent<CharaComponent>())
    {
        var chara = _charaGen.GenerateChara(player(), id);
        if (!_entityMan.IsAlive(chara))
            return null;

        return chara.Value;
    }
    else if (proto.Components.HasComponent<ItemComponent>())
    {
        var item = _itemGen.GenerateItem(player(), id, amount: amount);
        if (!_entityMan.IsAlive(item))
            return null;

        return item.Value;
    }
    else
    {
        var ent = _entityGen.SpawnEntity(id, player());
        if (!_entityMan.IsAlive(ent))
            return null;

        return ent.Value;
    }
}

public EntityUid? give(PrototypeId<EntityPrototype> id, int? amount = null)
{
    if (!_inv.TryGetInventoryContainer(player(), out var inv))
        return null;

    var item = _itemGen.GenerateItem(inv, id, amount: amount);
    if (!_entityMan.IsAlive(item))
        return null;

    _entityMan.EnsureComponent<IdentifyComponent>(item.Value).IdentifyState = IdentifyState.Full;
    return item;
}

public void clearEffects()
{
    _statusEffects.RemoveAll(player());
}

public void gotoDownStairs()
{
    var delving = _tags.EntityWithTagInMap(curMap().Id, Protos.Tag.DungeonStairsDelving);
    if (delving != null)
    {
        playerS().Coordinates = spatial(delving.Owner).Coordinates;
    }
}

public void gotoUpStairs()
{
    var surfacing = _tags.EntityWithTagInMap(curMap().Id, Protos.Tag.DungeonStairsSurfacing);
    if (surfacing != null)
    {
        playerS().Coordinates = spatial(surfacing.Owner).Coordinates;
    }
}

public void gotoWithComp<T>() where T: IComponent
{
    var comp = _lookup.EntityQueryInMap<T>(curMap().Id).FirstOrDefault();
    if (comp != null)
    {
        playerS().Coordinates = spatial(comp.Owner).Coordinates;
    }
}

public void killAllFoes()
{
    foreach (var foe in _lookup.EntityQueryInMap<CharaComponent>(curMap()).Where(c => _factions.GetRelationToPlayer(c.Owner) <= Relation.Enemy))
    {
        _damage.Kill(foe.Owner, player());
    }
}

public void refresh(EntityUid? uid = null)
{
    uid ??= player();
    _refresh.Refresh(uid.Value);
}

public void setcvar<T>(CVarDef<T> cvar, T val) where T: notnull
{
    _config.SetCVar(cvar, val);
    _config.SaveToFile();
}

public T getcvar<T>(CVarDef<T> cvar) where T: notnull
    => _config.GetCVar(cvar);

public void addcon(PrototypeId<StatusEffectPrototype> id, int turns = 100)
    => _statusEffects.Apply(player(), id, turns);

public void healcon(PrototypeId<StatusEffectPrototype> id, int? turns = null)
{
    if (turns == null)
        _statusEffects.HealFully(player(), id);
    else
        _statusEffects.Heal(player(), id, turns.Value);
}

public void setlog(LogLevel level)
{
    _config.SetCVar(CCVars.LogLevel, level);
    IoCManager.Resolve<ILogManager>().RootSawmill.Level = level;
}

public void setlog(string sawmill, LogLevel level)
{
    Logger.GetSawmill(sawmill).Level = level;
}

public void applyEffect<T>(int power = 100,
                           CurseState curseState = CurseState.Normal,
                           EntityUid? target = null,
                           EntityUid? source = null,
                           EntityCoordinates? coords = null,
                           EffectArgSet? args = null)
    where T: class, IEffect, new()
{
    target ??= player();
    source ??= target.Value;
    coords ??= spatial(target.Value).Coordinates;
    args ??= new EffectArgSet();
    args.Power = power;
    args.CurseState = curseState;
    _effects.Apply<T>(source.Value, target.Value, coords.Value, null, args);
}

public void sandbag(EntityUid? ent = null)
{
    ent ??= entityAt().Owner;
    _entityMan.EnsureComponent<OpenNefia.Content.Combat.SandBaggedComponent>(ent.Value);
}

public void vv(object obj)
{
    var debugView = IoCManager.Resolve<IDebugViewLayer>();
    var vv = IoCManager.Resolve<IViewVariablesManager>();
    vv.OpenVV(obj, debugView);

    _uiMan.Query(debugView);
}

public bool warpTo(GlobalAreaId areaId, AreaFloorId? floorId = null)
{
    var entrance = new MapEntrance()
    {
        MapIdSpecifier = new GlobalAreaMapIdSpecifier(areaId, floorId)
    };

    return _mapEntrance.UseMapEntrance(player(), entrance);
}

public void passDays(int days)
{
    _world.PassTime(GameTimeSpan.FromDays(days));
}

public void changeWeather(PrototypeId<EntityPrototype> id, GameTimeSpan? duration = null)
{
    _weather.TryChangeWeather(id, duration);
}

public void wish(string? wish = null)
{
    if (wish == null)
        _wishes.PromptForWish();
    else
        _wishes.GrantWish(wish);
}
