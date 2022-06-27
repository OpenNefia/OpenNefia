using OpenNefia.Content.Factions;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Logic;
using OpenNefia.Content.UI;
using OpenNefia.Content.VanillaAI;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Log;
using OpenNefia.Content.Prototypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;
using OpenNefia.Core.SaveGames;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.Parties
{
    [DataDefinition]
    internal sealed class Party
    {
        public Party() {}

        public Party(EntityUid leader)
        {
            Leader = leader;
            Members.Add(leader);
        }

        [DataField(required: true)]
        public EntityUid Leader { get; set; }

        [DataField(required: true)]
        public SortedSet<EntityUid> Members { get; set; } = new();
    }

    [DataDefinition]
    internal sealed class PartyCollection
    {
        [DataField]
        private Dictionary<int, Party> Parties { get; set; } = new();

        [DataField]
        private Dictionary<EntityUid, int> EntityUidToPartyId { get; set; } = new();

        [DataField]
        private int NextPartyId { get; set; } = 0;

        public void AddMember(int partyId, EntityUid uid)
        {
            if (!Parties.TryGetValue(partyId, out var party))
                throw new ArgumentException($"Party {partyId} does not exist.");

            if (EntityUidToPartyId.TryGetValue(uid, out var existing))
                throw new ArgumentException($"Entity {uid} is already in party {existing}, but tried adding them to {partyId}.");

            party.Members.Add(uid);
            EntityUidToPartyId[uid] = partyId;
        }

        public bool HasMember(int partyId, EntityUid uid)
        {
            if (!Parties.TryGetValue(partyId, out var party))
                return false;

            return party.Members.Contains(uid);
        }

        public void SetLeader(int partyId, EntityUid uid)
        {
            if (!Parties.TryGetValue(partyId, out var party))
                throw new ArgumentException($"Party {partyId} does not exist.");

            if (!party.Members.Contains(uid))
                throw new ArgumentException($"Party {partyId} does not have entity {uid}.");

            party.Leader = uid;
        }

        public bool TryGetLeader(int partyId, [NotNullWhen(true)] out EntityUid? leader)
        {
            if (!Parties.TryGetValue(partyId, out var party))
            {
                leader = null;
                return false;
            }

            leader = party.Leader;
            return true;
        }

        public void RemoveMember(int partyId, EntityUid uid)
        {
            var party = Parties[partyId];
            party.Members.Remove(uid);
            EntityUidToPartyId.Remove(uid);

            if (party.Leader == uid)
            {
                if (party.Members.Count > 0)
                {
                    var newLeader = party.Members.First();
                    Logger.WarningS("party", $"Auto-setting new leader for party {partyId}: {newLeader}");
                    party.Leader = newLeader;
                }
                else
                {
                    Parties.Remove(partyId);
                }
            }
        }

        public int AddParty(EntityUid leader)
        {
            var id = NextPartyId;
            NextPartyId++;
            Parties[id] = new Party(leader);
            EntityUidToPartyId[leader] = id;
            return id;
        }

        public bool TryGetPartyId(EntityUid entityUid, [NotNullWhen(true)] out int partyId)
        {
            return EntityUidToPartyId.TryGetValue(entityUid, out partyId);
        }

        public bool TryGetParty(EntityUid entityUid, [NotNullWhen(true)] out Party? party)
        {
            if (!TryGetPartyId(entityUid, out var partyId))
            {
                party = null;
                return false;
            }

            return TryGetParty(partyId, out party);
        }

        public bool TryGetParty(int partyId, [NotNullWhen(true)] out Party? party)
        {
            return Parties.TryGetValue(partyId, out party);
        }

        public int GetPartyId(EntityUid leader)
        {
            if (!TryGetPartyId(leader, out var partyId))
                throw new KeyNotFoundException($"{leader} had no party.");

            return partyId;
        }

        public Party GetParty(EntityUid leader)
        {
            if (!TryGetParty(leader, out var party))
                throw new KeyNotFoundException($"{leader} had no party.");

            return party;
        }
    }
    public interface IPartySystem : IEntitySystem
    {
        /// <summary>
        /// Tries to get this entity's leader.
        /// Also returns successfully if the entity is a leader themselves.
        /// </summary>
        bool TryGetLeader(EntityUid target, [NotNullWhen(true)] out EntityUid? leader, PartyComponent? party = null);

        /// <summary>
        /// Enumerates all members in the party of this entity, including the leader.
        /// </summary>
        IEnumerable<EntityUid> EnumerateMembers(EntityUid member, PartyComponent? partyComp = null);

        /// <summary>
        /// Enumerates all members in the party of this entity, excluding the leader.
        /// </summary>
        IEnumerable<EntityUid> EnumerateUnderlings(EntityUid member, PartyComponent? partyComp = null);

        /// <summary>
        /// Returns true if <paramref name="leader"/> is the leader of <paramref name="target"/>.
        /// Also returns true if <paramref name="target"/> is the leader of the party.
        /// </summary>
        bool IsPartyLeaderOf(EntityUid leader, EntityUid target, PartyComponent? partyLeader = null);

        /// <summary>
        /// Returns true if <paramref name="leader"/> is the leader of <paramref name="target"/>.
        /// Does *not* return true if <paramref name="target"/> is the leader of the party.
        /// </summary>
        bool IsDirectAllyOf(EntityUid leader, EntityUid target, PartyComponent? partyLeader = null);

        EntityUid? GetLeader(EntityUid target, PartyComponent? party = null);

        /// <summary>
        /// Returns true if this entity is in the player's party.
        /// Includes the player themselves.
        /// </summary>
        bool IsInPlayerParty(EntityUid entity, PartyComponent? party = null);

        /// <summary>
        /// Returns true if this entity is in the player's party, excluding the player themselves.
        /// </summary>
        bool IsDirectAllyOfPlayer(EntityUid entity, PartyComponent? party = null);

        bool CanRecruitMoreMembers(EntityUid entity, PartyComponent? party = null);
        bool RecruitAsAlly(EntityUid leader, EntityUid ally, PartyComponent? partyLeader = null, PartyComponent? partyAlly = null, bool noMessage = false);
        bool TryLeaveParty(EntityUid ally, PartyComponent? party = null);
    }

    public class PartySystem : EntitySystem, IPartySystem
    {
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IAudioManager _audio = default!;

        [RegisterSaveData("Elona.PartySystem.Parties")]
        private PartyCollection Parties { get; set; } = new();

        public override void Initialize()
        {
            SubscribeLocalEvent<EntityDeletedEvent>(HandleEntityDeleted, nameof(HandleEntityDeleted));
        }

        private void HandleEntityDeleted(EntityDeletedEvent ev)
        {
            if (Parties.TryGetPartyId(ev.EntityUid, out var partyId))
            {
                Parties.RemoveMember(partyId, ev.EntityUid);
            }
        }

        public bool TryGetLeader(EntityUid target, [NotNullWhen(true)] out EntityUid? leader, PartyComponent? party = null)
        {
            if (!Resolve(target, ref party, logMissing: false) || party.PartyID == null)
            {
                leader = null;
                return false;
            }

            // Also counts an entity leading themselves.
            return Parties.TryGetLeader(party.PartyID.Value, out leader);
        }

        public EntityUid? GetLeader(EntityUid target, PartyComponent? party = null)
        {
            TryGetLeader(target, out var leader, party);
            return leader;
        }

        public bool TryGetMembers(EntityUid leader, [NotNullWhen(true)] out IReadOnlySet<EntityUid>? members, PartyComponent? partyComp = null)
        {
            if (!Resolve(leader, ref partyComp, logMissing: false) 
                || partyComp.PartyID == null 
                || !Parties.TryGetParty(partyComp.PartyID.Value, out var party))
            {
                members = null;
                return false;
            }

            members = party.Members;
            return true;
        }

        public IEnumerable<EntityUid> EnumerateMembers(EntityUid member, PartyComponent? partyComp = null)
        {
            if (!Resolve(member, ref partyComp) || !Parties.TryGetParty(member, out var party))
                return Enumerable.Empty<EntityUid>();

            return party.Members;
        }

        public IEnumerable<EntityUid> EnumerateUnderlings(EntityUid member, PartyComponent? partyComp = null)
        {
            if (!Resolve(member, ref partyComp) || !Parties.TryGetParty(member, out var party))
                return Enumerable.Empty<EntityUid>();


            return party.Members.Where(m => m != party.Leader);
        }

        public bool IsPartyLeaderOf(EntityUid leader, EntityUid target, PartyComponent? partyLeader = null)
        {
            if (!Resolve(leader, ref partyLeader, logMissing: false)
                || partyLeader.PartyID == null
                || !Parties.TryGetParty(partyLeader.PartyID.Value, out var party))
                return false;

            return party.Members.Contains(target) && party.Leader == leader;
        }

        public bool IsInPlayerParty(EntityUid entity, PartyComponent? party = null)
        {
            if (_gameSession.IsPlayer(entity))
                return true;

            return IsPartyLeaderOf(_gameSession.Player!, entity, party);
        }

        public bool IsDirectAllyOf(EntityUid leader, EntityUid target, PartyComponent? partyLeader = null)
        {
            if (target == leader)
                return false;

            return IsPartyLeaderOf(leader, target, partyLeader);
        }

        public bool IsDirectAllyOfPlayer(EntityUid entity, PartyComponent? party = null)
        {
            return IsDirectAllyOf(_gameSession.Player, entity);
        }

        public bool CanRecruitMoreMembers(EntityUid entity, PartyComponent? party = null)
        {
            if (!Resolve(entity, ref party, logMissing: false))
                return false;

            // TODO
            return true;
        }

        public bool RecruitAsAlly(EntityUid leader, EntityUid ally, PartyComponent? partyLeader = null, PartyComponent? partyAlly = null, bool noMessage = false)
        {
            if (!Resolve(leader, ref partyLeader) || !Resolve(ally, ref partyAlly))
                return false;

            if (!EntityManager.IsAlive(leader) || !EntityManager.IsAlive(ally))
                return false;

            if (leader == ally)
            {
                Logger.WarningS("party", $"Leader {leader} tried to recruit themselves");
                return false;
            }

            if (IsPartyLeaderOf(leader, ally, partyLeader))
            {
                Logger.WarningS("party", $"Ally {ally} already in this party ({leader})");
                return false;
            }

            if (Parties.TryGetPartyId(ally, out var otherParty))
            {
                Logger.WarningS("party", $"Ally is already in another party: {otherParty}");
                return false;
            }

            if (!CanRecruitMoreMembers(leader, partyLeader))
            {
                if (!noMessage)
                    _mes.DisplayL("Party.AllyJoins.PartyFull");
                return false;
            }

            if (!Parties.TryGetPartyId(leader, out var partyId))
            {
                partyId = Parties.AddParty(leader);
                partyLeader.PartyID = partyId;
            }

            Parties.AddMember(partyId, ally);
            partyAlly.PartyID = partyId;

            if (EntityManager.TryGetComponent<FactionComponent>(leader, out var factionLeader) && EntityManager.TryGetComponent<FactionComponent>(ally, out var factionAlly))
            {
                factionAlly.RelationToPlayer = factionLeader.RelationToPlayer;
            }

            if (EntityManager.TryGetComponent<VanillaAIComponent>(leader, out var aiLeader)
                && aiLeader.CurrentTarget == ally)
            {
                aiLeader.CurrentTarget = null;
            }

            if (_gameSession.IsPlayer(leader) && !noMessage)
            {
                _mes.Display(Loc.GetString("Party.AllyJoins.Success", ("ally", ally)), color: UiColors.MesYellow);
                _audio.Play(Protos.Sound.Pray1);
            }

            var ev = new CharaRecruitedAsAllyEvent(leader, noMessage);
            RaiseLocalEvent(ally, ev);

            return true;
        }

        public bool TryLeaveParty(EntityUid ally, PartyComponent? party = null)
        {
            if (!Resolve(ally, ref party) || party.PartyID == null)
                return false;

            if (_gameSession.IsPlayer(ally))
            {
                Logger.ErrorS("party", "Player tried to leave their own party!");
                return false;
            }

            Parties.RemoveMember(party.PartyID.Value, ally);
            party.PartyID = null;
            return true;
        }
    }

    public sealed class CharaRecruitedAsAllyEvent : EntityEventArgs
    {
        public EntityUid Leader { get; }
        public bool NoMessage { get; }

        public CharaRecruitedAsAllyEvent(EntityUid leader, bool noMessage)
        {
            Leader = leader;
            NoMessage = noMessage;
        }
    }
}
