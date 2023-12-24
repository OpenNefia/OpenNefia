using OpenNefia.Content.DisplayName;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Equipment;
using OpenNefia.Content.Identify;
using OpenNefia.Content.Items;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Memory;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Qualities;
using OpenNefia.Content.Skills;
using OpenNefia.Content.StatusEffects;
using OpenNefia.Content.World;
using OpenNefia.Core.Areas;
using OpenNefia.Core.Configuration;
using OpenNefia.Core.Game;
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

namespace OpenNefia.Content.SenseQuality
{
    public interface ISenseQualitySystem : IEntitySystem
    {
        void SenseQuality(EntityUid ent);
    }

    public sealed class SenseQualitySystem : EntitySystem, ISenseQualitySystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly IWorldSystem _world = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IStatusEffectSystem _effects = default!;
        [Dependency] private readonly ISkillsSystem _skills = default!;
        [Dependency] private readonly IDisplayNameSystem _names = default!;
        [Dependency] private readonly IEntityGenMemorySystem _entityGenMemory = default!;
        [Dependency] private readonly IConfigurationManager _config = default!;
        [Dependency] private readonly IIdentifySystem _identify = default!;

        public override void Initialize()
        {
            SubscribeBroadcast<MapOnTimePassedEvent>(ProcSenseQuality);
            SubscribeComponent<IdentifyComponent, EntityBeingGeneratedEvent>(HandleEntityBeingGenerated, priority: EventPriorities.Default);
        }

        private void HandleEntityBeingGenerated(EntityUid uid, IdentifyComponent component, ref EntityBeingGeneratedEvent args)
        {
            var isShop = args.GenArgs.GetOrNull<ItemGenArgs>()?.IsShop ?? false;
            if (IsAlive(_gameSession.Player) && !isShop && HasComp<EquipmentComponent>(uid))
            {
                if (_rand.Next(_skills.Level(_gameSession.Player, Protos.Skill.SenseQuality) + 1) > 5)
                {
                    component.IdentifyState = IdentifyState.Quality;
                }
            }
        }

        private void ProcSenseQuality(ref MapOnTimePassedEvent ev)
        {
            // >>>>>>>> shade2 / main.hsp:546        if gTurn¥10 = 1 : call item_senseQuality ...
            if (ev.MinutesPassed == 0)
                return;

            if (_world.State.PlayTurns % 10 == 0)
                SenseQuality(_gameSession.Player);
            // <<<<<<<< shade2/main.hsp:546 		if gTurn¥10=1 : call item_senseQuality ..
        }

        private bool CanSenseQuality(EntityUid ent, StatusEffectsComponent? statusEffects = null)
        {
            if (!Resolve(ent, ref statusEffects))
                return true;

            return !(_effects.HasEffect(ent, Protos.StatusEffect.Confusion)
                  || _effects.HasEffect(ent, Protos.StatusEffect.Sleep)
                  || _effects.HasEffect(ent, Protos.StatusEffect.Paralysis)
                  || _effects.HasEffect(ent, Protos.StatusEffect.Choking));
        }

        public void SenseQuality(EntityUid ent)
        {
            if (!CanSenseQuality(ent))
                return;

            var power = _skills.Level(ent, Protos.Skill.AttrPerception) + _skills.Level(ent, Protos.Skill.SenseQuality) * 5;

            foreach (var (item, identify, equipment) in _lookup.EntityQueryDirectlyIn<ItemComponent, IdentifyComponent, EquipmentComponent>(ent))
            {
                if (identify.IdentifyState >= IdentifyState.Full)
                    continue;

                var proc = 1500 + identify.IdentifyDifficulty;

                if (power > _rand.Next(proc * 5))
                {
                    var unidentifiedName = _names.GetDisplayName(item.Owner);
                    _identify.IdentifyItem(item.Owner, IdentifyState.Full, identify);

                    if (TryProtoID(ent, out var protoID))
                        _entityGenMemory.SetIdentified(protoID.Value, true);

                    if (_config.GetCVar(CCVars.GameHideAutoidentify) != AutoIdentifyType.All)
                    {
                        _mes.Display(Loc.GetString("Elona.SenseQuality.Identify.FullyIdentified", ("unidentifiedName", unidentifiedName), ("identifiedName", _names.GetDisplayName(item.Owner))));
                    }

                    _skills.GainSkillExp(ent, Protos.Skill.SenseQuality, 50);
                }

                if (identify.IdentifyState < IdentifyState.Quality && power > _rand.Next(proc))
                {
                    if (_config.GetCVar(CCVars.GameHideAutoidentify) == AutoIdentifyType.None)
                    {
                        if (TryComp<QualityComponent>(item.Owner, out var quality))
                        {
                            _mes.Display(Loc.GetString("Elona.SenseQuality.Identify.AlmostIdentified", ("unidentifiedName", _names.GetDisplayName(item.Owner)), ("quality", quality.Quality.Base.GetLocalizedName())));
                        }

                        _identify.IdentifyItem(item.Owner, IdentifyState.Quality, identify);
                        _skills.GainSkillExp(ent, Protos.Skill.SenseQuality, 50);
                    }
                }
            }
        }
    }
}
