using ICSharpCode.Decompiler.CSharp.Syntax;
using OpenNefia.Content.CurseStates;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Equipment;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Memory;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Core.Serialization.Manager.Attributes;
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
        [Dependency] private readonly IPrototypeManager _protos = default!;

        public override void Initialize()
        {
            SubscribeComponent<IdentifyComponent, EntityBeingGeneratedEvent>(TryToAutoIdentifyItem);
        }

        private void TryToAutoIdentifyItem(EntityUid uid, IdentifyComponent component, ref EntityBeingGeneratedEvent args)
        {
            if (args.GenArgs.TryGet<ItemGenArgs>(out var itemArgs) && itemArgs.IsShop)
                component.IdentifyState = IdentifyState.Full;

            if (TryComp<TagComponent>(uid, out var tags))
            {
                foreach (var tag in tags.Tags)
                {
                    if (_protos.TryGetExtendedData<TagPrototype, ExtDefaultIdentifyState>(tag, out var def))
                    {
                        component.IdentifyState = def.IdentifyState;

                        if (def.AutoMemorize && TryProtoID(uid, out var id))
                        {
                            // TODO this API is weird...
                            // identified items should be split off into global save data for this
                            // entity system instead.
                            _entityGenMemory.Set(id.Value, EntityMemoryKinds.Identified, 1);
                        }
                    }
                }
            }
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

    /// <summary>
    /// When attached to a tag prototype, indicates that items with this tag should
    /// be inititialized with a default identify state instead of <see cref="IdentifyState.None"/>.
    /// </summary>
    public sealed class ExtDefaultIdentifyState : IPrototypeExtendedData<TagPrototype>
    {
        [DataField]
        public IdentifyState IdentifyState { get; }

        /// <summary>
        /// If true, add entities with this tag to the list of identified entities in <see
        /// cref="IEntityGenMemorySystem"/> list when they are first generated.
        /// </summary>
        [DataField]
        public bool AutoMemorize { get; set; }
    }
}