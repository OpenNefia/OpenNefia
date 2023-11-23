using OpenNefia.Content.DisplayName;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Roles;
using OpenNefia.Content.Shopkeeper;
using OpenNefia.Core.Areas;
using OpenNefia.Core.EngineVariables;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static OpenNefia.Core.Prototypes.EntityPrototype;
using OpenNefia.Content.RandomGen;
using OpenNefia.Content.World;
using OpenNefia.Core.Containers;
using OpenNefia.Content.Charas;
using OpenNefia.Core.Utility;
using OpenNefia.Content.Feats;
using OpenNefia.Core.Game;
using OpenNefia.Content.Maps;
using OpenNefia.Core.UserInterface;
using OpenNefia.Core.Rendering;
using System.Runtime.Serialization;
using OpenNefia.Content.Currency;
using OpenNefia.Core.Audio;
using OpenNefia.Core.GameController;
using OpenNefia.Content.GameController;
using System.Drawing;
using OpenNefia.Content.UI;
using OpenNefia.Content.ChooseNPC;
using static OpenNefia.Content.Prototypes.Protos;
using OpenNefia.Content.Home;

namespace OpenNefia.Content.Home
{
    [DataDefinition]
    public sealed class ServantSpec
    {
        [DataField("id", required: true)]
        public PrototypeId<EntityPrototype> ID { get; }

        [DataField("weight")]
        public int Weight { get; } = 1;
    }

    [DataDefinition]
    public sealed class ServantShopkeeperSpec
    {
        [DataField("id", required: true)]
        public PrototypeId<ShopInventoryPrototype> ID { get; }

        [DataField("weight")]
        public int Weight { get; } = 1;

        [DataField("wage")]
        public int? Wage { get; }
    }

    public interface IServantSystem : IEntitySystem
    {
        /// <summary>
        /// Generates a servant.
        /// </summary>
        /// <remarks>
        /// Be sure to set the random seed before calling to prevent fluctuations in shop rank, etc. on each query.
        /// </remarks>
        /// <param name="spec"></param>
        /// <returns></returns>
        EntityUid? GenerateServant(EntityCoordinates coords, ServantSpec spec);

        IList<EntityUid> GenerateHiringCandidates(IMap map);

        int CalcHireCost(EntityUid chara, ServantComponent? servant = null);
        int CalcWageCost(EntityUid chara, ServantComponent? servant = null);

        /// <summary>
        /// Calculates the maximum number of servants for a house *per floor*.
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>
        int CalcMaxServantLimit(IMap map);

        int CalcTotalLaborExpenses(IMap map);
        void QueryHire();
    }

    public class ServantHireBehavior : DefaultChooseNPCBehavior
    {
        private readonly IServantSystem _servants;

        public ServantHireBehavior(IServantSystem servants)
        {
            _servants = servants;
        }

        public override string TopicCustom => Loc.GetString("Elona.Servant.Hire.Topic.InitCost");

        public override string FormatDetail(EntityUid entity)
        {
            var hireCost = _servants.CalcHireCost(entity);
            var wage = _servants.CalcWageCost(entity);
            return Loc.GetString("Elona.UI.ChooseNPC.GoldCounter", ("gold", $"{hireCost}({wage})"));
        }
    }

    public class ServantInfoBehavior : DefaultChooseNPCBehavior
    {
        private readonly IServantSystem _servants;

        public ServantInfoBehavior(IServantSystem servants)
        {
            _servants = servants;
        }

        public override string TopicCustom => Loc.GetString("Elona.Servant.Hire.Topic.Wage");

        public override string FormatDetail(EntityUid entity)
        {
            var wage = _servants.CalcWageCost(entity);
            return Loc.GetString("Elona.UI.ChooseNPC.GoldCounter", ("gold", wage));
        }
    }

    public sealed class ServantSystem : EntitySystem, IServantSystem
    {
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly ICharaGen _charaGen = default!;
        [Dependency] private readonly IWorldSystem _world = default!;
        [Dependency] private readonly IRoleSystem _roles = default!;
        [Dependency] private readonly ICommonFeatsSystem _commonFeats = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IMapPlacement _mapPlacement = default!;
        [Dependency] private readonly IUserInterfaceManager _uiMan = default!;
        [Dependency] private readonly IAudioManager _audio = default!;
        [Dependency] private readonly IGameController _gameController = default!;
        [Dependency] private readonly ITemporaryEntitySystem _tempEntities = default!;
        [Dependency] private readonly IHomeSystem _homes = default!;

        public override void Initialize()
        {
            SubscribeComponent<ServantShopkeeperComponent, EntityBeingGeneratedEvent>(InitServantShopkeeper, priority: EventPriorities.VeryHigh);
            SubscribeComponent<ServantShopkeeperComponent, GetDisplayNameEventArgs>(HandleGetDisplayName_ServantShopkeeper, priority: EventPriorities.Low - 5000);

            SubscribeEntity<HouseBoardQueriedEvent>(ShowServantInfo);
        }

        [EngineVariable("Elona.ServantChoices")]
        public List<ServantSpec> ServantChoices { get; } = new();

        [EngineVariable("Elona.ServantShopkeeperChoices")]
        public List<ServantShopkeeperSpec> ServantShopkeeperChoices { get; } = new();

        [EngineVariable("Elona.ServantHiringCandidatesCount")]
        public int ServantHiringCandidatesCount { get; } = 10;

        private void ShowServantInfo(EntityUid uid, HouseBoardQueriedEvent args)
        {
            // >>>>>>>> elona122/shade2/map_user.hsp:212 	if gArea=areaHome{ ...
            var map = GetMap(uid);
            if (!_homes.ActiveHomeIDs.Contains(map.Id))
                return;

            var servantCount = _lookup.EntityQueryInMap<ServantComponent>(map).Count();
            var maxServants = CalcMaxServantLimit(map);

            _mes.Display(Loc.GetString("Elona.Servant.Count", ("curServants", servantCount), ("maxServants", maxServants)));
            // <<<<<<<< elona122/shade2/map_user.hsp:218 		} ...
        }

        private void InitServantShopkeeper(EntityUid uid, ServantShopkeeperComponent component, ref EntityBeingGeneratedEvent args)
        {
            // >>>>>>>> elona122/shade2/map_user.hsp:396 	if cId(rc)=idShopKeeper{ ...
            var sampler = new WeightedSampler<ServantShopkeeperSpec>();
            foreach (var cand in ServantShopkeeperChoices)
                sampler.Add(cand, cand.Weight);
            var spec = sampler.EnsureSample(_rand);

            // TODO event before EntityBeingGeneratedEvent for creating components
            // Here the new RoleShopkeeperComponent won't have EntityBeingGeneratedEvent fired on it
            // Benchmark performance loss
            var shop = EnsureComp<RoleShopkeeperComponent>(uid);
            shop.ShopInventoryId = spec.ID;
            shop.ShopRank = _rand.Next(15) + 1;
            shop.ShowTitleInName = false;
            if (spec.Wage != null)
                EnsureComp<ServantComponent>(uid).Wage = spec.Wage.Value;
            // <<<<<<<< elona122/shade2/map_user.hsp:405 		} ...
        }

        private void HandleGetDisplayName_ServantShopkeeper(EntityUid uid, ServantShopkeeperComponent component, ref GetDisplayNameEventArgs args)
        {
            // >>>>>>>> elona122/shade2/map_user.hsp:396 	if cId(rc)=idShopKeeper{ ...
            if (TryComp<RoleShopkeeperComponent>(uid, out var shop)
                && Loc.TryGetPrototypeString(shop.ShopInventoryId, "ServantTitle", out var name, ("entity", uid)))
            {
                args.OutName = name;
            }
            // <<<<<<<< elona122/shade2/map_user.hsp:405 		} ...
        }

        public EntityUid? GenerateServant(EntityCoordinates coords, ServantSpec spec)
        {
            var chara = _charaGen.GenerateChara(coords, spec.ID);
            return chara;
        }

        private bool ServantTypeExistsHere(EntityUid chara, IMap map)
        {
            var roles = _roles.EnumerateRoles(chara).ToList();
            foreach (var ent in _lookup.EntityQueryInMap<CharaComponent>(map))
            {
                foreach (var role in roles)
                {
                    var ty = role.GetType();
                    if (EntityManager.HasComponent(ent.Owner, ty))
                        return true;
                }
            }
            return false;
        }

        public IList<EntityUid> GenerateHiringCandidates(IMap map)
        {
            // >>>>>>>> elona122/shade2/map_user.hsp:388 	repeat 10 ...
            EntityUid? Generate(int index)
            {
                var seed = (int)_world.State.GameDate.TotalDays + index;
                var candidate = _rand.WithSeed(seed, () =>
                {
                    if (_rand.OneIn(2))
                        return null;

                    ServantSpec candidate;
                    if (index == 0)
                        candidate = ServantChoices.First(); // maid
                    else
                    {
                        var sampler = new WeightedSampler<ServantSpec>();
                        foreach (var cand in ServantChoices)
                            sampler.Add(cand, cand.Weight);
                        candidate = sampler.EnsureSample(_rand);
                    }
                    return candidate;
                });

                if (candidate == null)
                    return null;

                return _rand.WithSeed(seed, () =>
                {
                    var chara = _charaGen.GenerateChara(MapCoordinates.Global, candidate.ID, args: EntityGenArgSet.Make(new EntityGenCommonArgs() { IsMapSavable = false }));
                    if (!IsAlive(chara))
                        return null;

                    EnsureComp<ServantComponent>(chara.Value);

                    if (ServantTypeExistsHere(chara.Value, map))
                    {
                        EntityManager.DeleteEntity(chara.Value);
                        return null;
                    }

                    return chara;
                });
            }

            return Enumerable.Range(0, ServantHiringCandidatesCount).Select(Generate).WhereNotNullS().ToList();
            // <<<<<<<< elona122/shade2/map_user.hsp:412 	randomize  ...
        }

        public int CalcHireCost(EntityUid chara, ServantComponent? servant = null)
        {
            // >>>>>>>> elona122/shade2/map_user.hsp:418 		if cGold(pc)<calcHireCost(tc)*20{ ...
            if (!Resolve(chara, ref servant))
                return 0;

            return servant.Wage * 20;
            // <<<<<<<< elona122/shade2/map_user.hsp:418 		if cGold(pc)<calcHireCost(tc)*20{ ...
        }

        public int CalcWageCost(EntityUid chara, ServantComponent? servant = null)
        {
            // >>>>>>>> elona122/shade2/map_user.hsp:418 		if cGold(pc)<calcHireCost(tc)*20{ ...
            if (!Resolve(chara, ref servant))
                return 0;

            return servant.Wage;
            // <<<<<<<< elona122/shade2/map_user.hsp:418 		if cGold(pc)<calcHireCost(tc)*20{ ...
        }

        public int CalcMaxServantLimit(IMap map)
        {
            if (!TryArea(map, out var area) || !TryComp<AreaHomeComponent>(area.AreaEntityUid, out var areaHome))
                return 0;

            return areaHome.HomeScale + 2;
        }

        public int CalcTotalLaborExpenses(IMap map)
        {
            var totalExpense = _lookup.EntityQueryInMap<ServantComponent>(map).Aggregate(0, (total, servant) => total + servant.Wage);

            return _commonFeats.CalcAdjustedExpenseGold(totalExpense);
        }

        public void QueryHire()
        {
            // <<<<<<<< elona122/shade2/map_user.hsp:432 	calcCostHire ...
            var player = _gameSession.Player;
            var map = GetMap(player);

            if (!HasComp<MapTypePlayerOwnedComponent>(map.MapEntityUid))
                return;

            var servantCount = _lookup.EntityQueryInMap<ServantComponent>(map).Count();
            var max = CalcMaxServantLimit(map);
            if (servantCount >= max)
            {
                _mes.Display(Loc.GetString("Elona.Servant.Hire.TooManyGuests"));
                return;
            }

            _tempEntities.ClearGlobalTemporaryEntities();
            var candidates = GenerateHiringCandidates(map);

            var args = new ChooseNPCMenu.Args(candidates)
            {
                Behavior = new ServantHireBehavior(this),
                Prompt = Loc.GetString("Elona.Servant.Hire.Prompt")
            };
            var result = _uiMan.Query<ChooseNPCMenu, ChooseNPCMenu.Args, ChooseNPCMenu.Result>(args);

            if (result.HasValue)
            {
                _mes.Newline();
                var servant = result.Value.Selected;
                var hireCost = CalcHireCost(servant);
                if (!TryComp<MoneyComponent>(player, out var money) || money.Gold < hireCost)
                {
                    _mes.Display(Loc.GetString("Elona.Servant.Hire.NotEnoughMoney"));
                }
                else
                {
                    _audio.Play(Protos.Sound.Paygold1);
                    money.Gold -= hireCost;
                    _gameController.Wait(0.25f);
                    _mes.Display(Loc.GetString("Elona.Servant.Hire.YouHire", ("entity", servant)), color: UiColors.MesGreen);
                    MetaData(servant).IsMapSavable = true;
                    _mapPlacement.TryPlaceChara(servant, Spatial(player).MapPosition);
                    _audio.Play(Protos.Sound.Pray1);
                }
            }

            _world.State.LaborExpenses = CalcTotalLaborExpenses(map);
            // >>>>>>>> elona122/shade2/map_user.hsp:380 	txtNew ...
        }
    }
}