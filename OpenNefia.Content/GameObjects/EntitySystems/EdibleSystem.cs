using OpenNefia.Content.Logic;
using OpenNefia.Core.Audio;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Effects;
using OpenNefia.Core.IoC;
using OpenNefia.Content.DisplayName;
using OpenNefia.Core.Locale;
using OpenNefia.Content.Effects;

namespace OpenNefia.Content.GameObjects
{
    public class EdibleSystem : EntitySystem
    {
        public const string VerbIDEat = "Elona.Eat";

        [Dependency] private readonly IAudioManager _sounds = default!;
        [Dependency] private readonly IStackSystem _stackSystem = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;

        public override void Initialize()
        {
            SubscribeComponent<EdibleComponent, GetVerbsEventArgs>(HandleGetVerbs);
            SubscribeComponent<EdibleComponent, ThrownEntityImpactedOtherEvent>(HandleImpactOther);
        }

        private void HandleGetVerbs(EntityUid potion, EdibleComponent edibleComp, GetVerbsEventArgs args)
        {
            args.Verbs.Add(new Verb(VerbIDEat, "Eat Item", () => Eat(args.Source, args.Target)));
        }

        private TurnResult Eat(EntityUid eater, EntityUid target,
            EdibleComponent? edible = null)
        {
            if (!Resolve(target, ref edible))
                return TurnResult.Failed;

            if (!EntityManager.TryGetComponent(eater, out SpatialComponent sourceSpatial))
                return TurnResult.Failed;

            if (!_stackSystem.TrySplit(target, 1, out var split))
                return TurnResult.Failed;

            _mes.Display(Loc.GetString("Elona.Edible.Starts", ("entity", eater), ("edible", split)));
            _mes.Display(Loc.GetString("Elona.Edible.Finishes", ("entity", eater), ("edible", split)));

            _sounds.Play(Protos.Sound.Eat1, sourceSpatial.MapPosition);

            var result = edible.Effect?.Apply(eater, sourceSpatial.MapPosition, eater, edible.Args)
                ?? EffectResult.Succeeded;

            EntityManager.DeleteEntity(split);

            return result.ToTurnResult();
        }

        private void HandleImpactOther(EntityUid thrown, EdibleComponent edibleComp, ThrownEntityImpactedOtherEvent args)
        {
            _mes.Display(Loc.GetString("Elona.Throwable.Hits", ("entity", args.ImpactedWith)));
            _sounds.Play(Protos.Sound.Eat1, args.Coords);

            edibleComp.Effect?.Apply(args.Thrower, args.Coords, args.ImpactedWith, edibleComp.Args);

            EntityManager.DeleteEntity(thrown);
        }
    }
}
