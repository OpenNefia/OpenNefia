using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Equipment;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Levels;
using OpenNefia.Content.Skills;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Prototypes;
using static OpenNefia.Content.Prototypes.Protos;

namespace OpenNefia.Content.Resists
{
    public interface IResistsSystem : IEntitySystem
    {
        int Level(EntityUid uid, ElementPrototype proto, ResistsComponent? resists = null);
        int Level(EntityUid uid, PrototypeId<ElementPrototype> id, ResistsComponent? resists = null);
        int Grade(EntityUid uid, ElementPrototype proto, ResistsComponent? resists = null);
        int Grade(EntityUid uid, PrototypeId<ElementPrototype> id, ResistsComponent? resists = null);

        bool TryGetKnown(EntityUid uid, PrototypeId<ElementPrototype> protoId, [NotNullWhen(true)] out LevelAndPotential? level, ResistsComponent? resists = null);

        bool HasResist(EntityUid uid, PrototypeId<ElementPrototype> protoId, ResistsComponent? resists = null);
        bool HasResist(EntityUid uid, ElementPrototype proto, ResistsComponent? resists = null);

        /// <summary>
        /// Enumerates all resistable elemental damage types.
        /// </summary>
        IEnumerable<ElementPrototype> EnumerateResistableElements();
    }

    public sealed partial class ResistsSystem : EntitySystem, IResistsSystem
    {
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly IRefreshSystem _refresh = default!;

        public override void Initialize()
        {
            SubscribeLocalEvent<ResistsComponent, EntityRefreshEvent>(HandleRefresh, nameof(HandleRefresh),
                before: new[] { new SubId(typeof(EquipmentSystem), "HandleRefresh") });
        }

        private void HandleRefresh(EntityUid uid, ResistsComponent resists, ref EntityRefreshEvent args)
        {
            ResetResistBuffs(resists);
        }

        private void ResetResistBuffs(ResistsComponent resists)
        {
            foreach (var (_, level) in resists.Resists)
            {
                level.Level.Reset();
            }
        }

        public bool TryGetKnown(EntityUid uid, PrototypeId<ElementPrototype> protoId, [NotNullWhen(true)] out LevelAndPotential? level, ResistsComponent? resists = null)
        {
            if (!Resolve(uid, ref resists))
            {
                level = null;
                return false;
            }

            return resists.TryGetKnown(protoId, out level);
        }

        public bool HasResist(EntityUid uid, ElementPrototype proto, ResistsComponent? resists = null)
            => HasResist(uid, proto.GetStrongID(), resists);

        public bool HasResist(EntityUid uid, PrototypeId<ElementPrototype> protoId, ResistsComponent? resists = null)
        {
            if (!Resolve(uid, ref resists))
                return false;

            return resists.TryGetKnown(protoId, out _);
        }

        public int Level(EntityUid uid, ElementPrototype proto, ResistsComponent? resists = null)
            => Level(uid, proto.GetStrongID(), resists);
        public int Level(EntityUid uid, PrototypeId<ElementPrototype> id, ResistsComponent? resists = null)
        {
            if (!Resolve(uid, ref resists))
            {
                return 0;
            }

            return resists.Level(id);
        }

        public int Grade(EntityUid uid, ElementPrototype proto, ResistsComponent? resists = null)
            => Grade(uid, proto.GetStrongID(), resists);
        public int Grade(EntityUid uid, PrototypeId<ElementPrototype> id, ResistsComponent? resists = null)
        {
            if (!Resolve(uid, ref resists))
            {
                return 0;
            }

            return resists.Grade(id);
        }
    }
}