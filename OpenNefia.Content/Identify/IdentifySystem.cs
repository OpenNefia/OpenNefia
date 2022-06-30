using OpenNefia.Content.Equipment;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Memory;
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

namespace OpenNefia.Content.Identify
{
    public interface IIdentifySystem : IEntitySystem
    {
        IdentifyResult Identify(EntityUid ent, IdentifyState state, IdentifyComponent? identify = null);
        IdentifyResult TryToIdentify(EntityUid ent, int identifyPower, IdentifyComponent? identify = null);
    }

    public sealed record IdentifyResult(bool ChangedState, IdentifyState NewState);

    public sealed class IdentifySystem : EntitySystem, IIdentifySystem
    {
        [Dependency] private readonly IEntityGenMemorySystem _entityGenMemory = default!;

        public override void Initialize()
        {
        }

        public IdentifyResult GetNewIdentifyState(EntityUid ent, IdentifyState newState, IdentifyComponent? identify = null)
        {
            if (!Resolve(ent, ref identify))
                return new(false, IdentifyState.None);

            if (identify.IdentifyState == IdentifyState.Quality && !HasComp<EquipmentComponent>(ent))
                newState = IdentifyState.Full;

            if (identify.IdentifyState >= newState)
                return new(false, identify.IdentifyState);

            return new(true, newState);
        }

        public IdentifyResult Identify(EntityUid ent, IdentifyState state, IdentifyComponent? identify = null)
        {
            if (!Resolve(ent, ref identify))
                return new(false, IdentifyState.None);

            var result = GetNewIdentifyState(ent, state, identify);

            if (result.ChangedState)
            {
                identify.IdentifyState = result.NewState;
                
                if (result.NewState >= IdentifyState.Name && TryProtoID(ent, out var protoID))
                {
                    _entityGenMemory.SetIdentified(protoID.Value, true);
                }
            }

            return result;
        }

        public IdentifyResult TryToIdentify(EntityUid ent, int identifyPower, IdentifyComponent? identify = null)
        {
            if (!Resolve(ent, ref identify))
                return new(false, IdentifyState.None);

            IdentifyState newState;

            if (identifyPower >= identify.IdentifyDifficulty)
                newState = IdentifyState.Full;
            else
                newState = IdentifyState.None;

            return Identify(ent, newState);
        }
    }
}