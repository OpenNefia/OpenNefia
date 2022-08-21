using OpenNefia.Content.EntityGen;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Logic;
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
using OpenNefia.Core.Audio;
using OpenNefia.Content.Charas;
using OpenNefia.Core.Configuration;
using OpenNefia.Content.Parties;
using OpenNefia.Content.Factions;
using OpenNefia.Content.Roles;
using OpenNefia.Content.Qualities;
using OpenNefia.Content.Levels;
using OpenNefia.Content.Skills;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Metadata;
using OpenNefia.Core.Log;
using OpenNefia.Content.Weight;
using OpenNefia.Content.Inventory;
using OpenNefia.Content.RandomGen;
using OpenNefia.Content.DisplayName;
using OpenNefia.Core.Rendering;
using OpenNefia.Content.BaseAnim;
using OpenNefia.Content.UI;

namespace OpenNefia.Content.Items.Impl
{
    public interface IMonsterBallSystem : IEntitySystem
    {
        bool CanCaptureEntity(EntityUid monsterBall, EntityUid target, [NotNullWhen(false)] out string? reason, MonsterBallComponent? monsterBallComp);
    }

    public sealed class MonsterBallSystem : EntitySystem, IMonsterBallSystem
    {
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IAudioManager _audio = default!;
        [Dependency] private readonly IConfigurationManager _config = default!;
        [Dependency] private readonly IFactionSystem _factions = default!;
        [Dependency] private readonly IRoleSystem _roles = default!;
        [Dependency] private readonly ILevelSystem _levels = default!;
        [Dependency] private readonly IPartySystem _parties = default!;
        [Dependency] private readonly IStackSystem _stacks = default!;
        [Dependency] private readonly ICharaGen _charaGen = default!;
        [Dependency] private readonly IMapDrawablesManager _mapDrawables = default!;
        [Dependency] private readonly IActionThrowSystem _throwables = default!;

        public override void Initialize()
        {
            SubscribeComponent<MonsterBallComponent, LocalizeItemNameExtraEvent>(LocalizeExtra_MonsterBall);
            SubscribeComponent<MonsterBallComponent, EntityBeingGeneratedEvent>(BeingGenerated_MonsterBall);
            SubscribeComponent<MonsterBallComponent, BeforeEntityThrownEventArgs>(BeforeThrown_MonsterBall, priority: EventPriorities.VeryHigh);
            SubscribeComponent<MonsterBallComponent, ThrownEntityImpactedOtherEvent>(ThrownImpactedOther_MonsterBall);
            SubscribeComponent<MonsterBallComponent, GetVerbsEventArgs>(GetVerbs_MonsterBall);
        }

        private void LocalizeExtra_MonsterBall(EntityUid uid, MonsterBallComponent component, ref LocalizeItemNameExtraEvent args)
        {
            if (component.CapturedEntityID != null)
            {
                var s = Loc.GetString("Elona.MonsterBall.ItemName.Full",
                    ("name", args.OutFullName.ToString()),
                    ("charaName", Loc.GetPrototypeString(component.CapturedEntityID.Value, "MetaData.Name")));
                args.OutFullName.Clear().Append(s);
            }
            else
            {
                var s = Loc.GetString("Elona.MonsterBall.ItemName.Empty",
                    ("name", args.OutFullName.ToString()),
                    ("lv", component.MaxLevel));
                args.OutFullName.Clear().Append(s);
            }
        }

        private void BeingGenerated_MonsterBall(EntityUid uid, MonsterBallComponent component, ref EntityBeingGeneratedEvent args)
        {
            if (component.MaxLevel <= 0)
            {
                component.MaxLevel = _rand.Next(args.CommonArgs.MinLevel + 1) + 1;
            }

            if (TryComp<ValueComponent>(uid, out var value))
                value.Value.Base = 200 + component.MaxLevel * component.MaxLevel + component.MaxLevel * 100;
        }

        private void BeforeThrown_MonsterBall(EntityUid uid, MonsterBallComponent component, BeforeEntityThrownEventArgs args)
        {
            if (args.Handled)
                return;

            _audio.Play(Protos.Sound.Throw2, args.Thrower);
        }

        public bool CanCaptureEntity(EntityUid monsterBall, EntityUid target, [NotNullWhen(false)] out string? reason, MonsterBallComponent? monsterBallComp)
        {
            if (!Resolve(monsterBall, ref monsterBallComp) || !TryComp<CharaComponent>(target, out var targetChara))
            {
                reason = "";
                return false;
            }

            if (_config.GetCVar(CCVars.DebugDevelopmentMode))
            {
                reason = null;
                return true;
            }

            if (_factions.GetRelationToPlayer(target) >= Relation.Ally
                || _roles.HasAnyRoles(target)
                || CompOrNull<QualityComponent>(target)?.Quality == Quality.Unique
                || targetChara.IsPrecious
                || MetaData(target).EntityPrototype == null)
            {
                reason = Loc.GetString("Elona.MonsterBall.Throw.CannotBeCaptured");
                return false;
            }

            if (_levels.GetLevel(target) > monsterBallComp.MaxLevel)
            {
                reason = Loc.GetString("Elona.MonsterBall.Throw.NotEnoughPower");
                return false;
            }

            if (TryComp<SkillsComponent>(target, out var targetSkills))
            {
                if (targetSkills.HP > targetSkills.MaxHP / 10)
                {
                    reason = Loc.GetString("Elona.MonsterBall.Throw.NotWeakEnough");
                    return false;
                }
            }

            reason = null;
            return true;
        }

        private void CaptureEntity(EntityUid monsterBall, EntityUid target, MonsterBallComponent component)
        {
            if (!TryProtoID(target, out var id))
            {
                Logger.ErrorS("item.monsterBall", $"Entity {target} does not have a prototype ID!");
                return;
            }

            component.CapturedEntityID = id;
            component.CapturedEntityLevel = _levels.GetLevel(target);

            if (TryComp<WeightComponent>(monsterBall, out var weight)
                && TryComp<WeightComponent>(target, out var targetWeight))
            {
                weight.Weight.Base = Math.Clamp(targetWeight.Weight.Base, 10000, 100000);
            }

            if (TryComp<ValueComponent>(monsterBall, out var value))
                value.Value.Base = 1000;

            EntityManager.DeleteEntity(target);
        }

        private void ThrownImpactedOther_MonsterBall(EntityUid uid, MonsterBallComponent component, ThrownEntityImpactedOtherEvent args)
        {
            if (args.Handled)
                return;

            args.Handled = true;

            _mes.Display(Loc.GetString("Elona.Throw.Hits", ("entity", args.ImpactedWith)));

            if (!CanCaptureEntity(uid, args.ImpactedWith, out var reason, component))
            {
                _mes.Display(reason);
                return;
            }

            _mes.Display(Loc.GetString("Elona.MonsterBall.Throw.YouCapture", ("user", args.Thrower), ("target", args.ImpactedWith)), UiColors.MesGreen);
            var anim = new BasicAnimMapDrawable(Protos.BasicAnim.AnimSmoke);
            _mapDrawables.Enqueue(anim, uid);

            CaptureEntity(uid, args.ImpactedWith, component);
        }

        private void GetVerbs_MonsterBall(EntityUid uid, MonsterBallComponent component, GetVerbsEventArgs args)
        {
            if (component.CapturedEntityID == null)
            {
                args.OutVerbs.Add(new Verb(ActionThrowSystem.VerbTypeThrow, "Throw Entity", () => _throwables.PromptThrow(args.Source, args.Target)));
            }
            args.OutVerbs.Add(new Verb(UseInventoryBehavior.VerbTypeUse, "Use Monster Ball", () => UseMonsterBall(args.Source, args.Target)));
        }

        public TurnResult UseMonsterBall(EntityUid user, EntityUid monsterBall, MonsterBallComponent? monsterBallComp = null)
        {
            if (!Resolve(monsterBall, ref monsterBallComp))
                return TurnResult.Aborted;

            if (monsterBallComp.CapturedEntityID == null)
            {
                _mes.Display(Loc.GetString("Elona.MonsterBall.Use.Empty"));
                return TurnResult.Aborted;
            }

            if (!_parties.CanRecruitMoreMembers(user))
            {
                _mes.Display(Loc.GetString("Elona.MonsterBall.Use.PartyIsFull"));
                return TurnResult.Aborted;
            }

            _mes.Display(Loc.GetString("Elona.MonsterBall.Use.YouUse", ("user", user), ("monsterBall", monsterBall)));
            _stacks.Use(monsterBall, 1);

            var args = EntityGenArgSet.Make(new EntityGenCommonArgs()
            {
                // Don't apply Void level modifiers.
                NoRandomModify = true,
                LevelOverride = monsterBallComp.CapturedEntityLevel,
            });

            var chara = _charaGen.GenerateChara(user, id: monsterBallComp.CapturedEntityID.Value, args: args);
            if (IsAlive(chara))
                _parties.RecruitAsAlly(user, chara.Value);

            return TurnResult.Succeeded;
        }
    }
}
