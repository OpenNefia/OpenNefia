using OpenNefia.Analyzers;
using OpenNefia.Content.Charas;
using OpenNefia.Content.EmotionIcon;
using OpenNefia.Content.Fame;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Parties;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Talk;
using OpenNefia.Content.UI;
using OpenNefia.Content.VanillaAI;
using OpenNefia.Content.World;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Random;
using OpenNefia.Core.Utility;

namespace OpenNefia.Content.Factions
{
    public interface IFactionSystem : IEntitySystem
    {
        Relation GetRelationTowards(EntityUid us, EntityUid them);
        Relation GetOriginalRelationTowards(EntityUid us, EntityUid them);
        Relation GetRelationToPlayer(EntityUid target, FactionComponent? faction = null);

        void SetPersonalRelationTowards(EntityUid us, EntityUid them, Relation relation, FactionComponent? ourFaction = null);
        bool ClearPersonalRelationTowards(EntityUid us, EntityUid them, FactionComponent? ourFaction = null);
        void ClearAllPersonalRelations(EntityUid entity, FactionComponent? faction = null);

        void ActHostileTowards(EntityUid us, EntityUid them);
    }

    public class FactionSystem : EntitySystem, IFactionSystem
    {
        [Dependency] private readonly IPartySystem _partySystem = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IEmotionIconSystem _emoIcons = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IKarmaSystem _karma = default!;
        [Dependency] private readonly IWorldSystem _world = default!;
        [Dependency] private readonly IVanillaAISystem _vanillaAI = default!;
        [Dependency] private readonly ITalkSystem _talk = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;

        public override void Initialize()
        {
            SubscribeComponent<FactionComponent, CalculateRelationEventArgs>(HandleCalculateRelation, priority: EventPriorities.VeryHigh);
        }

        /// <inheritdoc/>
        public Relation GetRelationTowards(EntityUid us, EntityUid them)
        {
            var ev = new CalculateRelationEventArgs(them, ignorePersonal: false);
            RaiseEvent(us, ref ev);
            return ev.Relation;
        }
        
        /// <inheritdoc/>
        public Relation GetOriginalRelationTowards(EntityUid us, EntityUid them)
        {
            var ev = new CalculateRelationEventArgs(them, ignorePersonal: true);
            RaiseEvent(us, ref ev);
            return ev.Relation;
        }

        private void HandleCalculateRelation(EntityUid us, FactionComponent ourFaction, ref CalculateRelationEventArgs args)
        {
            args.Relation = CalculateRelationDefault(us, args.Target, args.IgnorePersonal);
        }

        public Relation CalculateRelationDefault(EntityUid us, EntityUid them, bool ignorePersonal)
        {
            if (us == them)
            {
                // Love thyself.
                return Relation.Ally;
            }

            Relation? ourUnderlingRelation = null;
            Relation? theirUnderlingRelation = null;
            if (!ignorePersonal)
            {
                if (TryComp<FactionComponent>(us, out var ourUnderlingFaction)
                    && ourUnderlingFaction.PersonalRelations.TryGetValue(them, out var ourRel))
                    ourUnderlingRelation = ourRel;
                if (TryComp<FactionComponent>(them, out var theirUnderlingFaction)
                    && theirUnderlingFaction.PersonalRelations.TryGetValue(us, out var theirRel))
                    theirUnderlingRelation = theirRel;
            }
            
            if (_partySystem.TryGetLeader(us, out var leader))
                us = leader.Value;
            if (_partySystem.TryGetLeader(them, out leader))
                them = leader.Value;

            // If either entity lacks a faction component, then they should be treated as neutral.
            // This prevents the AI from targeting inanimate things like doors.
            if (!EntityManager.TryGetComponent<FactionComponent>(us, out var ourFaction)
                || !EntityManager.TryGetComponent<FactionComponent>(them, out var theirFaction))
            {
                return Relation.Neutral;
            }

            var ourRelation = GetBaseRelation(us, ourFaction);
            var theirRelation = GetBaseRelation(them, theirFaction);

            if (!ignorePersonal)
            {
                if (ourFaction.PersonalRelations.TryGetValue(them, out var ourPersonal))
                    ourRelation = ourPersonal;
                if (theirFaction.PersonalRelations.TryGetValue(them, out var theirPersonal))
                    ourRelation = theirPersonal;

                // If our allies have a beef with each other, the leaders also share that beef towards the
                // entire opposing party.
                if (ourUnderlingRelation != null)
                    ourRelation = (Relation)Math.Min((int)ourRelation, (int)ourUnderlingRelation.Value);
                if (theirUnderlingRelation != null)
                    theirRelation = (Relation)Math.Min((int)theirRelation, (int)theirUnderlingRelation.Value);
            }

            return CompareRelations(ourRelation, theirRelation);
        }

        public Relation CompareRelations(Relation ourRelation, Relation theirRelation)
        {
            // Allies like each other...
            if (ourRelation == Relation.Ally && theirRelation == Relation.Ally)
                return Relation.Ally;

            // ...and so do enemies.
            if (ourRelation == Relation.Enemy && theirRelation == Relation.Enemy)
                return Relation.Ally;

            // Non-enemies dislike enemies...
            if (ourRelation >= Relation.Hate)
            {
                if (theirRelation <= Relation.Enemy)
                {
                    return Relation.Enemy;
                }
            }
            // ...and vice-versa.
            else
            {
                if (theirRelation >= Relation.Hate)
                {
                    return Relation.Enemy;
                }
            }

            // Whichever relation is more hostile wins out.
            return (Relation)Math.Min((int)ourRelation, (int)theirRelation);
        }

        private Relation GetBaseRelation(EntityUid entity, FactionComponent faction)
        {
            if (_gameSession.IsPlayer(entity))
            {
                return Relation.Ally;
            }
            else
            {
                return faction.RelationToPlayer;
            }
        }

        public Relation GetRelationToPlayer(EntityUid target, FactionComponent? faction = null)
        {
            if (!Resolve(target, ref faction))
                return Relation.Neutral;

            return faction.RelationToPlayer;
        }

        public void SetPersonalRelationTowards(EntityUid us, EntityUid them, Relation relation, FactionComponent? ourFaction = null)
        {
            if (!Resolve(us, ref ourFaction))
                return;

            ourFaction.PersonalRelations[them] = relation;
        }

        public bool ClearPersonalRelationTowards(EntityUid us, EntityUid them, FactionComponent? ourFaction = null)
        {
            if (!Resolve(us, ref ourFaction))
                return false;

            return ourFaction.PersonalRelations.Remove(them);
        }

        public void ClearAllPersonalRelations(EntityUid entity, FactionComponent? faction = null)
        {
            if (!Resolve(entity, ref faction))
                return;

            faction.PersonalRelations.Clear();
        }

        public void ActHostileTowards(EntityUid us, EntityUid them)
        {
            if (!_partySystem.IsInPlayerParty(us) || _gameSession.IsPlayer(them))
                return;

            var theirRelation = GetRelationTowards(them, us);

            if (theirRelation > Relation.Enemy)
                _emoIcons.SetEmotionIcon(them, EmotionIcons.Angry, 4);

            var glares = () =>
                    _mes.Display(Loc.GetString("Elona.Faction.HostileAction.GlaresAt", ("us", us), ("them", them)), UiColors.MesPurple);

            if (theirRelation >= Relation.Ally)
            {
                glares();
            }
            else
            {
                if (theirRelation > Relation.Neutral)
                    _karma.ModifyKarma(us, -2);

                // TODO
                if (ProtoID(them) == Protos.Chara.Ebon && !_world.State.IsFireGiantReleased)
                {
                    glares();
                    return;
                }

                if (theirRelation > Relation.Hate)
                {
                    glares();
                    SetPersonalRelationTowards(them, us, Relation.Hate);
                }
                else
                {
                    if (theirRelation > Relation.Enemy)
                        _mes.Display(Loc.GetString("Elona.Faction.HostileAction.GetsFurious", ("us", us), ("them", them)), UiColors.MesPurple);
                    SetPersonalRelationTowards(them, us, Relation.Enemy);
                    _vanillaAI.SetTarget(them, us, 80);
                }

                _talk.Say(them, "Elona.Aggro", new Dictionary<string, object>() { { "target", us } });
            }

            if (HasComp<LivestockComponent>(them) && _rand.OneIn(50) && TryMap(them, out var map))
            {
                _mes.Display(Loc.GetString("Elona.Faction.HostileAction.AnimalsGetExcited"), UiColors.MesRed);

                foreach (var livestock in _lookup.EntityQueryInMap<LivestockComponent>(map))
                {
                    SetPersonalRelationTowards(livestock.Owner, us, Relation.Enemy);
                    _vanillaAI.SetTarget(livestock.Owner, us, 20);
                    _emoIcons.SetEmotionIcon(livestock.Owner, EmotionIcons.Angry, 3);
                }
            }
        }
    }

    [ByRefEvent]
    public struct CalculateRelationEventArgs
    {
        public EntityUid Target { get; }
        public bool IgnorePersonal { get; }

        public Relation Relation { get; set; } = Relation.Neutral;

        public CalculateRelationEventArgs(EntityUid target, bool ignorePersonal)
        {
            Target = target;
            IgnorePersonal = ignorePersonal;
        }
    }
}