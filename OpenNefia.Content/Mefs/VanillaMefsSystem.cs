using OpenNefia.Content.Logic;
using OpenNefia.Content.Maps;
using OpenNefia.Content.Prototypes;
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
using OpenNefia.Content.Weather;
using OpenNefia.Content.World;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Game;
using OpenNefia.Content.Effects;
using OpenNefia.Content.UI;
using OpenNefia.Content.DeferredEvents;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Visibility;
using OpenNefia.Content.Factions;
using OpenNefia.Content.Damage;
using OpenNefia.Content.Resists;
using OpenNefia.Content.Charas.Impl;
using OpenNefia.Content.Skills;
using OpenNefia.Content.Weight;
using OpenNefia.Content.Combat;

namespace OpenNefia.Content.Mefs
{
    public sealed class VanillaMefsSystem : EntitySystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly IWeatherSystem _weathers = default!;
        [Dependency] private readonly IAudioManager _audio = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IMefSystem _mefs = default!;
        [Dependency] private readonly ICommonEffectsSystem _commonEffects = default!;
        [Dependency] private readonly IDeferredEventsSystem _deferred = default!;
        [Dependency] private readonly IVisibilitySystem _vis = default!;
        [Dependency] private readonly IFactionSystem _factions = default!;
        [Dependency] private readonly IDamageSystem _damages = default!;
        [Dependency] private readonly IResistsSystem _resists = default!;
        [Dependency] private readonly ISkillsSystem _skills = default!;

        public override void Initialize()
        {
            SubscribeComponent<MefFireComponent, EntityPositionChangedEvent>(Fire_RemoveIfOnWater);
            SubscribeComponent<MefFireComponent, MefBeforeMapTurnBeginEventArgs>(Fire_BeforeTurnBegin);
            SubscribeComponent<MefFireComponent, AfterEntitySteppedOnEvent>(Fire_AfterSteppedOn);

            SubscribeComponent<MefAcidGroundComponent, AfterEntitySteppedOnEvent>(AcidGround_AfterSteppedOn);

            SubscribeComponent<MefEtherGroundComponent, AfterEntitySteppedOnEvent>(EtherGround_AfterSteppedOn);

            SubscribeComponent<MefWebComponent, BeforeEntitySteppedOffEvent>(Web_BeforeSteppedOff);

            SubscribeEntity<BeforePhysicalAttackEventArgs>(MistOfDarkness_BeforeAttack, priority: EventPriorities.VeryHigh);

            SubscribeComponent<MefNuclearBombComponent, MefBeforeMapTurnBeginEventArgs>(Nuke_BeforeTurnBegin);
            SubscribeComponent<MefNuclearBombComponent, MefTimerExpiredEvent>(Nuke_TimerExpired);
        }

        private void Fire_RemoveIfOnWater(EntityUid uid, MefFireComponent component, ref EntityPositionChangedEvent args)
        {
            // >>>>>>>> elona122/shade2/module.hsp:419 	if id=mefFire:if tRole(p)=tWater:return ...
            if (!TryMap(args.NewPosition, out var map))
                return;

            var proto = map.GetTilePrototype(args.NewPosition.ToMap(EntityManager));
            if (proto == null)
                return;

            if (proto.Kind == TileKind.Water || proto.Kind2 == TileKind.Water)
                EntityManager.DeleteEntity(uid);
            // <<<<<<<< elona122/shade2/module.hsp:419 	if id=mefFire:if tRole(p)=tWater:return ...
        }

        private void Fire_BeforeTurnBegin(EntityUid uid, MefFireComponent component, MefBeforeMapTurnBeginEventArgs args)
        {
            // >>>>>>>> elona122/shade2/main.hsp:525 	if mefExist(cnt)=mefFire:if mField=mFieldOutdoor: ...
            if (args.Handled)
                return;

            if (!TryComp<MapCommonComponent>(args.Map.MapEntityUid, out var mapCommon)
                || mapCommon.IsIndoors
                || HasComp<MapTypeWorldMapComponent>(args.Map.MapEntityUid))
                return;

            if (_weathers.IsRaining())
            {
                args.Mef.TimeRemaining = GameTimeSpan.Zero;
                return;
            }

            var coords = Spatial(uid).MapPosition;
            var playerCoords = Spatial(_gameSession.Player).MapPosition;
            var newFireCount = 0;

            if (_rand.OneIn(35))
            {
                newFireCount = 3;
                if (coords.TryDistanceTiled(playerCoords, out var dist) && dist < 6)
                    _audio.Play(Protos.Sound.Fire1, uid);
            }

            for (var i = 0; i < newFireCount; i++)
            {
                var pos = coords.Position + _rand.NextVec2iInVec(2, 2) - _rand.NextVec2iInVec(2, 2);
                if (!args.Map.IsInBounds(pos))
                    continue;

                var tile = args.Map.GetTilePrototype(pos);
                if (tile == null)
                    continue;

                if (tile.IsSolid)
                {
                    args.Map.SetTile(pos, Protos.Tile.Destroyed);
                    continue;
                }

                var turns = _rand.Next(15) + 20;
                _mefs.SpawnMef(Protos.Mef.Fire, args.Map.AtPos(pos),
                    duration: GameTimeSpan.FromMinutes(turns),
                    power: 50,
                    spawnedBy: args.Mef.SpawnedBy);
                _commonEffects.DamageTileFire(coords, args.Mef.SpawnedBy);
            }
            // <<<<<<<< elona122/shade2/main.hsp:538 	} ...
        }

        private void Fire_AfterSteppedOn(EntityUid uid, MefFireComponent component, AfterEntitySteppedOnEvent args)
        {
            // >>>>>>>> elona122/shade2/main.hsp:782 		if mefExist(ef)=mefFire{ ...
            if (args.Handled)
                return;

            if (_vis.PlayerCanSeeEntity(uid))
            {
                _audio.Play(Protos.Sound.Fire1, uid);
                _mes.Display(Loc.GetString("Elona.Mefs.Fire.IsBurnt", ("stepper", args.Stepper)));
            }

            EntityUid? spawnedBy = null;

            if (TryComp<MefComponent>(uid, out var mef)
                && IsAlive(mef.SpawnedBy))
            {
                spawnedBy = mef.SpawnedBy;
                if (_gameSession.IsPlayer(spawnedBy.Value))
                    _factions.ActHostileTowards(spawnedBy.Value, args.Stepper);
            }

            var damage = _rand.Next((mef?.Power ?? 50) / 15 + 5) + 1;
            var damageType = new ElementalDamageType(Protos.Element.Fire, mef?.Power ?? 50);
            _damages.DamageHP(args.Stepper, damage, damageType: damageType);
            if (!IsAlive(args.Stepper) && IsAlive(spawnedBy)) // TODO responsible entity param to DamageHP
                _damages.RunCheckKillEvents(args.Stepper, spawnedBy.Value);
            // <<<<<<<< elona122/shade2/main.hsp:787 			} ...
        }

        private void AcidGround_AfterSteppedOn(EntityUid uid, MefAcidGroundComponent component, AfterEntitySteppedOnEvent args)
        {
            // >>>>>>>> elona122/shade2/main.hsp:776 		if mefExist(ef)=mefAcid:if (cBit(cFloat,tc)=fals ...
            if (args.Handled)
                return;

            var isFloating = CompOrNull<CommonProtectionsComponent>(args.Stepper)?.IsFloating?.Buffed ?? false;
            var acidResistance = _resists.Level(args.Stepper, Protos.Element.Acid);

            if (isFloating || acidResistance >= ResistGrades.Strong)
                return;

            if (_vis.PlayerCanSeeEntity(uid))
            {
                _audio.Play(Protos.Sound.Water, uid);
                _mes.Display(Loc.GetString("Elona.Mefs.AcidGround.Melts", ("stepper", args.Stepper)));
            }

            EntityUid? spawnedBy = null;

            if (TryComp<MefComponent>(uid, out var mef)
                && IsAlive(mef.SpawnedBy))
            {
                spawnedBy = mef.SpawnedBy;
                if (_gameSession.IsPlayer(spawnedBy.Value))
                    _factions.ActHostileTowards(spawnedBy.Value, args.Stepper);
            }

            var damage = _rand.Next((mef?.Power ?? 50) / 25 + 5) + 1;
            var damageType = new ElementalDamageType(Protos.Element.Acid, mef?.Power ?? 50);
            _damages.DamageHP(args.Stepper, damage, damageType: damageType);
            if (!IsAlive(args.Stepper) && IsAlive(spawnedBy)) // TODO responsible entity param to DamageHP
                _damages.RunCheckKillEvents(args.Stepper, spawnedBy.Value);
            // <<<<<<<< elona122/shade2/main.hsp:781 			} ...
        }

        private void EtherGround_AfterSteppedOn(EntityUid uid, MefEtherGroundComponent component, AfterEntitySteppedOnEvent args)
        {
            if (args.Handled)
                return;

            // TODO
        }

        public const int WebFreeWeight = 100;

        private void Web_BeforeSteppedOff(EntityUid uid, MefWebComponent component, BeforeEntitySteppedOffEvent args)
        {
            if (args.Handled)
                return;

            // >>>>>>>> elona122/shade2/action.hsp:621 		if mefExist(i)=mefWeb:if cnRace(cc)!"spider"{ ...
            if (HasComp<RaceSpiderComponent>(args.Stepper) || !TryComp<MefComponent>(uid, out var mef))
                return;

            var str = _skills.Level(args.Stepper, Protos.Skill.AttrStrength);
            var dex = _skills.Level(args.Stepper, Protos.Skill.AttrStrength);
            var weight = CompOrNull<WeightComponent>(args.Stepper)?.Weight?.Buffed ?? 0;
            if (_rand.Next(mef.Power + 25) < _rand.Next(str + dex + 1) || weight > WebFreeWeight)
            {
                _mes.Display(Loc.GetString("Elona.Mefs.Web.Destroys", ("stepper", args.Stepper)), entity: args.Stepper);
                EntityManager.DeleteEntity(uid);
            }
            else
            {
                mef.Power = (int)(mef.Power * 0.75f);
                _mes.Display(Loc.GetString("Elona.Mefs.Web.Caught", ("stepper", args.Stepper)), entity: args.Stepper);
                args.Handle(TurnResult.Failed);
                return;
            }
            // <<<<<<<< elona122/shade2/action.hsp:633 		} ...
        }
        private void MistOfDarkness_BeforeAttack(EntityUid attacker, BeforePhysicalAttackEventArgs args)
        {
            // >>>>>>>> elona122/shade2/action.hsp:1238 	if map(cX(tc),cY(tc),8)!0{ ...
            var isInMistOfDarkness = _lookup.EntityQueryLiveEntitiesAtCoords<MefMistOfDarknessComponent>(Spatial(attacker).MapPosition).Any();

            if (isInMistOfDarkness && _rand.OneIn(2))
            {
                _mes.Display(Loc.GetString("Elona.Mefs.MistOfDarkness.AttacksIllusion", ("attacker", attacker)), entity: attacker);
                args.Handled = true;
                return;
            }
            // <<<<<<<< elona122/shade2/action.hsp:1246 		} ...
        }

        private void Nuke_BeforeTurnBegin(EntityUid uid, MefNuclearBombComponent component, MefBeforeMapTurnBeginEventArgs args)
        {
            // >>>>>>>> elona122/shade2/main.hsp:539 	if mefExist(cnt)=mefNuke{ ...
            if (args.Handled)
                return;

            var message = Loc.Space + "*" + args.Mef.TimeRemaining.TotalMinutes + "*" + Loc.Space;
            _mes.Display(message, color: UiColors.MesRed);
            // <<<<<<<< elona122/shade2/main.hsp:541 	} ...
        }

        private void NukeExplodes(EntityCoordinates coordinates)
        {
            // >>>>>>>> elona122/shade2/main.hsp:1961 	case evNuke ...
            _mes.Display("TODO nuke", color: UiColors.MesYellow);
            // <<<<<<<< elona122/shade2/main.hsp:2024 	swbreak ...
        }

        private void Nuke_TimerExpired(EntityUid uid, MefNuclearBombComponent component, MefTimerExpiredEvent args)
        {
            // >>>>>>>> elona122/shade2/module.hsp:400 	if mefExist(id)=mefNuke :evAdd evNuke,mefX(id),me ...
            _deferred.Enqueue(() =>
            {
                NukeExplodes(Spatial(args.Mef.Owner).Coordinates);
                return TurnResult.Aborted;
            });
            // <<<<<<<< elona122/shade2/module.hsp:400 	if mefExist(id)=mefNuke :evAdd evNuke,mefX(id),me ...
        }
    }
}