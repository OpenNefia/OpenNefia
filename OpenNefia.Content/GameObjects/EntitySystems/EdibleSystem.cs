using OpenNefia.Content.Logic;
using OpenNefia.Core.Audio;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Effects;
using OpenNefia.Core.IoC;
using OpenNefia.Content.DisplayName;
using OpenNefia.Core.Locale;
using OpenNefia.Content.Activity;
using OpenNefia.Core.Maps;

namespace OpenNefia.Content.GameObjects
{
    public class EdibleSystem : EntitySystem
    {
        public const string VerbTypeEat = "Elona.Eat";

        [Dependency] private readonly IAudioManager _sounds = default!;
        [Dependency] private readonly IStackSystem _stackSystem = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IActivitySystem _activities = default!;

        public override void Initialize()
        {
            SubscribeComponent<EdibleComponent, GetVerbsEventArgs>(HandleGetVerbs);
        }

        private void HandleGetVerbs(EntityUid uid, EdibleComponent edibleComp, GetVerbsEventArgs args)
        {
            args.Verbs.Add(new Verb(VerbTypeEat, "Eat Item", () => Eat(args.Source, args.Target)));
        }

        private TurnResult Eat(EntityUid eater, EntityUid target,
            EdibleComponent? edible = null)
        {
            if (!Resolve(target, ref edible))
                return TurnResult.Failed;

            if (!_stackSystem.TrySplit(target, 1, out var split))
                return TurnResult.Failed;

            var activity = EntityManager.SpawnEntity(Protos.Activity.Eating, MapCoordinates.Global);
            Comp<ActivityEatingComponent>(activity).Food = split;
            _activities.StartActivity(eater, activity);

            return TurnResult.Succeeded;
        }
    }
}
