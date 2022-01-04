using OpenNefia.Content.Logic;
using OpenNefia.Core.Audio;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Effects;
using OpenNefia.Core.IoC;
using OpenNefia.Content.DisplayName;
using OpenNefia.Core.Locale;

namespace OpenNefia.Content.GameObjects
{
    public class EdibleSystem : EntitySystem
    {
        public const string VerbIDEat = "Elona.Eat";

        [Dependency] private readonly IAudioSystem _sounds = default!;
        [Dependency] private readonly IStackSystem _stackSystem = default!;

        public override void Initialize()
        {
            SubscribeLocalEvent<EdibleComponent, GetVerbsEventArgs>(HandleGetVerbs, nameof(HandleGetVerbs));
            SubscribeLocalEvent<ExecuteVerbEventArgs>(HandleExecuteVerb, nameof(HandleExecuteVerb));
            SubscribeLocalEvent<EdibleComponent, DoEatEventArgs>(HandleDoEat, nameof(HandleDoEat));
            SubscribeLocalEvent<EdibleComponent, ThrownEntityImpactedOtherEvent>(HandleImpactOther, nameof(HandleImpactOther));
        }

        private void HandleGetVerbs(EntityUid potion, EdibleComponent edibleComp, GetVerbsEventArgs args)
        {
            args.Verbs.Add(new Verb(VerbIDEat));
        }

        private void HandleExecuteVerb(ExecuteVerbEventArgs args)
        {
            if (args.Handled)
                return;

            switch (args.Verb.ID)
            {
                case VerbIDEat:
                    Raise(args.Target, new DoEatEventArgs(args.Source), args);
                    break;
            }
        }

        private void HandleDoEat(EntityUid target, EdibleComponent edible, DoEatEventArgs args)
        {
            args.Handle(Eat(target, args.Eater, edible));
        }

        private TurnResult Eat(EntityUid target, EntityUid eater,
            EdibleComponent? edible = null)
        {
            if (!Resolve(target, ref edible))
                return TurnResult.Failed;

            if (!EntityManager.TryGetComponent(eater, out SpatialComponent sourceSpatial))
                return TurnResult.Failed;

            if (!_stackSystem.TrySplit(target, 1, out var split))
                return TurnResult.Failed;

            Mes.Display(Loc.GetString("Elona.Edible.Eats", ("entity", eater), ("edible", split)));

            _sounds.Play(Protos.Sound.Eat1, sourceSpatial.MapPosition);

            var result = edible.Effect?.Apply(eater, sourceSpatial.MapPosition, eater, edible.Args)
                ?? EffectResult.Succeeded;

            EntityManager.DeleteEntity(split);

            return result.ToTurnResult();
        }

        private void HandleImpactOther(EntityUid thrown, EdibleComponent edibleComp, ThrownEntityImpactedOtherEvent args)
        {
            Mes.Display(Loc.GetString("Elona.Throwable.Hits", ("entity", args.ImpactedWith)));
            _sounds.Play(Protos.Sound.Eat1, args.Coords);

            edibleComp.Effect?.Apply(args.Thrower, args.Coords, args.ImpactedWith, edibleComp.Args);

            EntityManager.DeleteEntity(thrown);
        }
    }

    public class DoEatEventArgs : TurnResultEntityEventArgs
    {
        public readonly EntityUid Eater;

        public DoEatEventArgs(EntityUid eater)
        {
            Eater = eater;
        }
    }
}
