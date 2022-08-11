using OpenNefia.Content.EntityGen;
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
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.Skills;
using OpenNefia.Content.DisplayName;
using OpenNefia.Content.Identify;
using OpenNefia.Content.Inventory;
using OpenNefia.Core.Game;
using OpenNefia.Content.Activity;

namespace OpenNefia.Content.Book
{
    public interface ITextbookSystem : IEntitySystem
    {
    }

    public sealed class TextbookSystem : EntitySystem, ITextbookSystem
    {
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly ISkillsSystem _skills = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IPlayerQuery _playerQuery = default!;
        [Dependency] private readonly IActivitySystem _activities = default!;

        public override void Initialize()
        {
            SubscribeComponent<TextbookComponent, LocalizeItemNameExtraEvent>(LocalizeExtra_Textbook);
            SubscribeComponent<TextbookComponent, EntityBeingGeneratedEvent>(EntityBeingGenerated_Textbook);
            SubscribeComponent<TextbookComponent, GetVerbsEventArgs>(GetVerbs_Textbook);
        }

        private void LocalizeExtra_Textbook(EntityUid uid, TextbookComponent component, ref LocalizeItemNameExtraEvent args)
        {
            var identify = CompOrNull<IdentifyComponent>(uid)?.IdentifyState ?? IdentifyState.None;
            if (identify >= IdentifyState.Full)
            {
                var skillName = Loc.GetPrototypeString(component.SkillID, "Name");
                var s = Loc.GetString("Elona.Read.Textbook.ItemName.Title", ("name", args.OutFullName.ToString()), ("skillName", skillName));
                args.OutFullName.Clear().Append(s);
            }
        }

        private void EntityBeingGenerated_Textbook(EntityUid uid, TextbookComponent component, ref EntityBeingGeneratedEvent args)
        {
            if (!_protos.HasIndex(component.SkillID))
            {
                component.SkillID = _skills.PickRandomRegularSkill().GetStrongID();
            }
        }

        private void GetVerbs_Textbook(EntityUid uid, TextbookComponent component, GetVerbsEventArgs args)
        {
            args.OutVerbs.Add(new Verb(ReadInventoryBehavior.VerbTypeRead, "Read Textbook", () => ReadTextbook(args.Source, args.Target, component)));
        }

        private TurnResult ReadTextbook(EntityUid source, EntityUid target, TextbookComponent? component = null)
        {
            if (!Resolve(target, ref component))
                return TurnResult.Aborted;

            if (_gameSession.IsPlayer(source) && !_skills.HasSkill(source, component.SkillID))
            {
                if (!_playerQuery.YesOrNo(Loc.GetString("Elona.Read.Textbook.PromptNotInterested")))
                    return TurnResult.Aborted;
            }

            var activity = EntityManager.SpawnEntity(Protos.Activity.Training, MapCoordinates.Global);
            var activityComp = Comp<ActivityTrainingComponent>(activity);
            activityComp.SkillID = component.SkillID;
            activityComp.Item = target;
            _activities.StartActivity(source, activity);
            return TurnResult.Succeeded;
        }
    }
}