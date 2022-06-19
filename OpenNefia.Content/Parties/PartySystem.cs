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

namespace OpenNefia.Content.Parties
{
    public interface IPartySystem : IEntitySystem
    {
        /// <summary>
        /// Gets the entity that is the highest up the chain of party leaders.
        /// </summary>
        /// <remarks>
        /// If the entity is not in a party, returns null.
        /// </remarks>
        public PartyComponent? GetSupremeCommander(EntityUid entity);

        /// <summary>
        /// Returns true if this entity is in the player's party.
        /// </summary>
        bool IsInPlayerParty(EntityUid entity, PartyComponent? party = null);

        bool IsDirectPartyLeaderOf(EntityUid leader, EntityUid target);

        /// <summary>
        /// Creates a <see cref="PartyComponent"/> on the given entity if it doesn't exist,
        /// sets the leader to the given entity, and ensures they are a member of the party.
        /// </summary>
        PartyComponent EnsurePartyAndSetLeader(EntityUid entity);

        bool CanRecruitMoreMembers(EntityUid entity, PartyComponent? party = null);
        bool RecruitAsAlly(EntityUid leader, EntityUid ally, PartyComponent? partyLeader = null, PartyComponent? partyAlly = null, bool noMessage = false);
    }

    public class PartySystem : EntitySystem, IPartySystem
    {
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IRefreshSystem _refresh = default!;
        [Dependency] private readonly IAudioManager _audio = default!;

        public PartyComponent? GetSupremeCommander(EntityUid entity)
        {
            while (EntityManager.TryGetComponent(entity, out PartyComponent party))
            {
                if (party.Leader == null || party.Leader == entity)
                    return party;

                entity = party.Leader.Value;
            }

            return null;
        }

        public bool IsDirectPartyLeaderOf(EntityUid leader, EntityUid target)
        {
            throw new NotImplementedException();
        }

        public bool IsInPlayerParty(EntityUid entity, PartyComponent? party = null)
        {
            if (_gameSession.IsPlayer(entity))
                return true;

            if (!Resolve(entity, ref party, logMissing: false))
                return false;

            return party.Leader != null && party.Leader == _gameSession.Player;
        }

        public PartyComponent EnsurePartyAndSetLeader(EntityUid entity)
        {
            var parties = EntityManager.EnsureComponent<PartyComponent>(entity);

            parties.Leader = entity;
            parties.Members.Add(entity);

            return parties;
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

            if (leader == ally)
                return false;

            if (partyLeader.Members.Contains(ally))
            {
                Logger.WarningS("party", $"Ally already in this party");
                return false;
            }

            if (partyAlly.Leader != null)
            {
                Logger.WarningS("party", $"Ally already had another party");
                return false;
            }

            if (!CanRecruitMoreMembers(leader, partyLeader))
            {
                if (!noMessage)
                    _mes.DisplayL("Party.AllyJoins.PartyFull");
                return false;
            }

            // TODO
            partyAlly.Leader = leader;
            partyLeader.Members.Add(ally);

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
