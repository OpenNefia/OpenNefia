using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
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

        /// <summary>
        /// Creates a <see cref="PartyComponent"/> on the given entity if it doesn't exist,
        /// sets the leader to the given entity, and ensures they are a member of the party.
        /// </summary>
        PartyComponent EnsurePartyAndSetLeader(EntityUid entity);
    }

    public class PartySystem : EntitySystem, IPartySystem
    {
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        
        public PartyComponent? GetSupremeCommander(EntityUid entity)
        {
            while (EntityManager.TryGetComponent(entity, out PartyComponent party))
            {
                if (party.Leader == null)
                    return party;

                entity = party.Leader.Value;
            }

            return null;
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
    }
}
