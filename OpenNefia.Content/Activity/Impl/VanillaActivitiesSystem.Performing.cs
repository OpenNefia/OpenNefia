using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Random;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Maps;
using OpenNefia.Content.UI;
using OpenNefia.Content.Currency;
using OpenNefia.Content.Charas;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Dialog;
using OpenNefia.Content.VanillaAI;
using OpenNefia.Content.World;
using OpenNefia.Content.Damage;
using OpenNefia.Content.RandomGen;
using OpenNefia.Content.Rendering;
using OpenNefia.Content.Shopkeeper;

namespace OpenNefia.Content.Activity
{
    public sealed partial class VanillaActivitiesSystem
    {
        private void Initialize_Performing()
        {
            SubscribeComponent<ActivityPerformingComponent, OnActivityStartEvent>(Performing_OnStart);
            SubscribeComponent<ActivityPerformingComponent, OnActivityPassTurnEvent>(Performing_OnPassTurn);
            SubscribeComponent<ActivityPerformingComponent, OnActivityFinishEvent>(Performing_OnFinish);
        }

        private void Performing_OnStart(EntityUid activity, ActivityPerformingComponent component, OnActivityStartEvent args)
        {
            if (!IsAlive(component.Instrument))
            {
                args.Cancel();
                return;
            }

            _mes.Display(Loc.GetString("Elona.Activity.Performing.Start", ("actor", args.Activity.Actor), ("instrument", component.Instrument)), entity: args.Activity.Actor);
        }

        private int ApplyPerformance(EntityUid actor, ActivityPerformingComponent component, EntityUid audience)
        {
            if (!IsAlive(audience) || actor == audience || !TryComp<DialogComponent>(audience, out var dialog))
                return 0;

            if (_world.State.GameDate >= dialog.InterestRenewDate)
            {
                dialog.Interest = 100;
            }
            else if (dialog.Interest <= 0)
            {
                return 0;
            }

            if (_vis.IsInWindowFov(actor))
            {
                if (!_vis.IsInWindowFov(audience))
                    return 0;
            }
            else if (!Spatial(actor).MapPosition.TryDistanceTiled(Spatial(audience).MapPosition, out var dist) || dist > 3)
                return 0;

            if (_effects.HasEffect(audience, Protos.StatusEffect.Sleep))
                return 0;

            if (_factions.GetRelationTowards(audience, actor) <= Factions.Relation.Enemy)
            {
                if (TryComp<VanillaAIComponent>(audience, out var vai))
                {
                    if (vai.Aggro <= 0)
                        _mes.Display(Loc.GetString("Elona.Activity.Performing.GetsAngry", ("audience", audience)), entity: audience);
                    _vanillaAI.SetTarget(audience, actor, 30);
                }
                return 0;
            }

            if (_gameSession.IsPlayer(actor))
            {
                dialog.Interest -= _rand.Next(15);
                dialog.InterestRenewDate = _world.State.GameDate + GameTimeSpan.FromHours(12);
            }

            if (dialog.Interest <= 0)
            {
                _mes.Display(Loc.GetString("Elona.Activity.Performing.Dialog.Disinterest"), UiColors.MesSkyBlue);
                dialog.Interest = 0;
                return 0;
            }

            if (_skills.Level(actor, Protos.Skill.Performer) < _levels.GetLevel(audience))
            {
                if (_rand.OneIn(3))
                {
                    ApplyPerformanceBad(actor, component, audience);
                    return 0;
                }
            }

            var goldEarned = 0;

            if (_rand.OneIn(3))
            {
                if (TryComp<MoneyComponent>(actor, out var actorWallet) && TryComp<MoneyComponent>(audience, out var audienceWallet))
                {
                    goldEarned = CalcPerformanceEarnedGold(actor, component, audienceWallet);
                    audienceWallet.Gold -= goldEarned;
                    actorWallet.Gold += goldEarned;
                }
            }

            if (_levels.GetLevel(audience) > _skills.Level(actor, Protos.Skill.Performer))
                return goldEarned;

            ApplyPerformanceGood(actor, component, audience);

            return goldEarned;
        }

        private int CalcPerformanceEarnedGold(EntityUid actor, ActivityPerformingComponent component, MoneyComponent audienceWallet)
        {
            var instrumentQuality = CompOrNull<InstrumentComponent>(component.Instrument)?.PerformanceQuality ?? 0;

            var gold = component.PerformanceQuality * component.PerformanceQuality * (100 + (instrumentQuality / 5) / 100 / 1000 + _rand.Next(10));
            gold = Math.Clamp(gold, 1, 100);
            gold = Math.Clamp(audienceWallet.Gold * gold / 125, 0, _skills.Level(actor, Protos.Skill.Performer));
            if (_parties.IsPartyLeaderOf(actor, audienceWallet.Owner))
                gold = _rand.Next(Math.Clamp(gold, 1, 100)) + 1;
            if (HasComp<RoleShopkeeperComponent>(audienceWallet.Owner))
                gold /= 5;

            return Math.Clamp(gold, 0, audienceWallet.Gold);
        }

        private void ApplyPerformanceBad(EntityUid actor, ActivityPerformingComponent component, EntityUid audience)
        {
            component.PerformanceQuality -= _levels.GetLevel(audience) / 2;

            _mes.Display(Loc.GetString("Elona.Activity.Performing.Dialog.Angry"), UiColors.MesSkyBlue, entity: audience);
            _mes.Display(Loc.GetString("Elona.Activity.Performing.ThrowsRock", ("audience", audience)), entity: audience);

            var damage = _rand.Next(_levels.GetLevel(audience) + 1) + 1;
            _damage.DamageHP(actor, damage, damageType: new GenericDamageType("Elona.DamageType.Performance"));
        }

        private MapCoordinates? PositionNearby(MapCoordinates coords)
        {
            var map = _mapManager.GetMap(coords.MapId);
            var nearby = new MapCoordinates(coords.MapId, (coords.Position + _rand.NextVec2iInRadius(1)).BoundWithin(map.Bounds));

            if (!map.CanAccess(nearby))
                return null;

            return nearby;
        }

        private ItemFilter CalcPerformanceThrownItemFilter(EntityUid actor, ActivityPerformingComponent component, EntityUid audience)
        {
            var map = GetMap(actor);
            var filter = new ItemFilter();

            if (TryComp<SpecialInstrumentsComponent>(component.Instrument, out var inst) && inst.IsStradivarius.Buffed)
            {
                filter.MinLevel = _randomGen.CalcObjectLevel(component.PerformanceQuality / 8);
                filter.Quality = Qualities.Quality.Good;
                if (_rand.OneIn(4))
                    filter.Quality = Qualities.Quality.Great;
            }
            else
            {
                filter.MinLevel = _randomGen.CalcObjectLevel(component.PerformanceQuality / 8);
                filter.Quality = _randomGen.CalcObjectQuality(Qualities.Quality.Good);
            }

            filter.Tags = new[] { _rand.Pick(RandomGenConsts.FilterSets.Perform) };

            // TODO quest
            var isPartyQuest = false;
            if (isPartyQuest)
            {
                if (_rand.OneIn(150))
                {
                    filter.Id = Protos.Item.Safe;
                }
                if (_rand.OneIn(150))
                {
                    filter.Id = Protos.Item.SmallMedal;
                }
                if (_levels.GetLevel(audience) > 15 && _rand.OneIn(1000))
                {
                    filter.Id = Protos.Item.KillKillPiano;
                }
                if (_levels.GetLevel(audience) > 10 && _rand.OneIn(800))
                {
                    filter.Id = Protos.Item.Alud;
                }
            }
            else
            {
                if (_rand.OneIn(10))
                {
                    filter.Id = Protos.Item.MusicTicket;
                }
                if (_rand.OneIn(250))
                {
                    filter.Id = Protos.Item.PlatinumCoin;
                }
            }

            return filter;
        }

        private void AudienceThrowItem(EntityUid actor, ActivityPerformingComponent component, EntityUid audience, MapCoordinates targetCoords)
        {
            if (!targetCoords.TryToEntity(_mapManager, out var entityCoords))
                return;

            var map = GetMap(targetCoords);
            var filter = CalcPerformanceThrownItemFilter(actor, component, audience);
            var item = _itemGen.GenerateItem(MapCoordinates.Global, filter);

            if (IsAlive(item))
            {
                if (TryComp<ChipComponent>(item.Value, out var chip))
                {
                    var sourceCoords = Spatial(audience).MapPosition;
                    var anim = new RangedAttackMapDrawable(sourceCoords, targetCoords, chip.ChipID, chip.Color);
                    _mapDrawables.Enqueue(anim, sourceCoords);
                }
                Spatial(item.Value).Coordinates = entityCoords;
                component.TotalNumberOfTips++;
            }
        }

        private void UpdateQuestScore(EntityUid audience)
        {
            // TODO
        }

        private void ApplyPerformanceGood(EntityUid actor, ActivityPerformingComponent component, EntityUid audience)
        {
            var level = _levels.GetLevel(actor);
            var qualityDelta = _rand.Next(level + 1) + 1;
            if (_rand.Next(_skills.Level(actor, Protos.Skill.Performer) + 1) > _rand.Next(level * 2 + 1))
            {
                UpdateQuestScore(audience);
                if (_rand.OneIn(2))
                    component.PerformanceQuality += qualityDelta;
                else
                    component.PerformanceQuality -= qualityDelta;
            }

            if (TryComp<SpecialInstrumentsComponent>(component.Instrument, out var inst) && inst.IsGouldsPiano.Buffed)
            {
                _effects.Apply(audience, Protos.StatusEffect.Drunk, 500);
            }

            if (_rand.Next(_skills.Level(actor, Protos.Skill.Performer) + 1) > _rand.Next(level * 5 + 1))
            {
                if (_rand.OneIn(3))
                {
                    _mes.Display(Loc.GetString("Elona.Activity.Performing.Dialog.Interest", ("audience", audience), ("actor", actor)), UiColors.MesSkyBlue, entity: audience);
                    component.PerformanceQuality += level + 5;

                    var receiveGoods = _gameSession.IsPlayer(actor);

                    if (receiveGoods && !_parties.IsPartyLeaderOf(actor, audience))
                    {
                        var willGetGoods = _rand.OneIn(component.TotalNumberOfTips * 2 + 2);
                        if (willGetGoods)
                        {
                            var pos = PositionNearby(Spatial(actor).MapPosition);
                            if (pos != null && _vis.HasLineOfSight(audience, pos.Value))
                            {
                                AudienceThrowItem(actor, component, audience, pos.Value);
                            }
                        }
                    }
                }
            }
        }

        private void Performing_OnPassTurn(EntityUid activity, ActivityPerformingComponent component, OnActivityPassTurnEvent args)
        {
            var actor = args.Activity.Actor;

            if (args.Activity.TurnsRemaining % 10 == 0)
            {
                if (_rand.OneIn(10))
                {
                    _mes.Display(Loc.GetString("Elona.Activity.Performing.Sound.Random"), UiColors.MesBlue, entity: actor);
                }
                _mes.Display(Loc.GetString("Elona.Activity.Performing.Sound.Cha"), UiColors.MesBlue);
            }

            if (args.Activity.TurnsRemaining % 20 == 0)
            {
                var spatial = Spatial(actor);
                _commonEffects.MakeSound(actor, spatial.MapPosition, 5, 0.125f);

                var goldEarned = 0;

                foreach (var audience in _lookup.EntityQueryInMap<CharaComponent>(spatial.MapID))
                {
                    goldEarned += ApplyPerformance(actor, component, audience.Owner);
                    if (!IsAlive(actor))
                        return;
                }

                if (goldEarned > 0)
                {
                    component.TotalTipGold += goldEarned;
                    if (_vis.IsInWindowFov(actor))
                    {
                        _audio.Play(Protos.Sound.Getgold1, actor);
                    }
                }
            }
        }

        private void Performing_OnFinish(EntityUid activity, ActivityPerformingComponent component, OnActivityFinishEvent args)
        {
            var actor = args.Activity.Actor;
            var quality = component.PerformanceQuality;

            if (_gameSession.IsPlayer(actor))
            {
                int qualityRank;
                
                if (quality < 0)
                    qualityRank = 0;
                else if (quality < 20)
                    qualityRank = 1;
                else if (quality < 40)
                    qualityRank = 2;
                else if (quality == 40)
                    qualityRank = 3;
                else if (quality < 60)
                    qualityRank = 4;
                else if (quality < 80)
                    qualityRank = 5;
                else if (quality < 100)
                    qualityRank = 6;
                else if (quality < 120)
                    qualityRank = 7;
                else if (quality < 150)
                    qualityRank = 8;
                else
                    qualityRank = 9;

                _mes.Display(Loc.GetString($"Elona.Activity.Performing.Quality.{qualityRank}"));
            }

            if (quality > 40)
            {
                var instrumentQuality = CompOrNull<InstrumentComponent>(component.Instrument)?.PerformanceQuality ?? 0;
                quality = quality * (100 + (instrumentQuality / 5) / 100);
            }

            if (component.TotalTipGold != 0)
            {
                _mes.Display(Loc.GetString("Elona.Activity.Performing.Tip", ("actor", actor), ("tipGold", component.TotalTipGold)), entity: actor);
            }

            var skillExp = quality - _skills.Level(actor, Protos.Skill.Performer) + 50;
            if (skillExp > 0)
                _skills.GainSkillExp(actor, Protos.Skill.Performer, skillExp);
        }
    }
}