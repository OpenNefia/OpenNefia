using OpenNefia.Content.GameObjects.Components;
using OpenNefia.Content.Skills;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Resists
{
    [RegisterComponent]
    public sealed class ResistsComponent : Component, IComponentRefreshable
    {
        public override string Name => "Resists";

        /// <summary>
        /// Level, potential and experience for resistances.
        /// </summary>
        [DataField]
        public Dictionary<PrototypeId<ElementPrototype>, LevelAndPotential> Resists { get; } = new();

        [DataField]
        public bool IsImmuneToElementalDamage { get; set; } = false;

        public LevelAndPotential Ensure(PrototypeId<ElementPrototype> protoId)
        {
            if (Resists.TryGetValue(protoId, out var level))
                return level;

            return new LevelAndPotential()
            {
                Level = new(0)
            };
        }

        public LevelAndPotential Ensure(ElementPrototype proto) => Ensure(proto.GetStrongID());

        public bool TryGetKnown(ElementPrototype proto, [NotNullWhen(true)] out LevelAndPotential? level)
            => TryGetKnown(proto.GetStrongID(), out level);
        public bool TryGetKnown(PrototypeId<ElementPrototype> protoId, [NotNullWhen(true)] out LevelAndPotential? level)
        {
            if (!Resists.TryGetValue(protoId, out level))
            {
                return false;
            }

            if (level.Level.Base <= 0)
            {
                return false;
            }

            return true;
        }

        public int Level(ElementPrototype proto) => Level(proto.GetStrongID());
        public int Level(PrototypeId<ElementPrototype> id)
        {
            if (!TryGetKnown(id, out var level))
                return 0;

            return level.Level.Buffed;
        }

        public int BaseLevel(ElementPrototype proto) => BaseLevel(proto.GetStrongID());
        public int BaseLevel(PrototypeId<ElementPrototype> id)
        {
            if (!TryGetKnown(id, out var level))
                return 0;

            return level.Level.Base;
        }

        public int Grade(ElementPrototype proto) => Grade(proto.GetStrongID());
        public int Grade(PrototypeId<ElementPrototype> id)
        {
            return ResistHelpers.CalculateGrade(Level(id));
        }

        public int BaseGrade(ElementPrototype proto) => BaseGrade(proto.GetStrongID());
        public int BaseGrade(PrototypeId<ElementPrototype> id)
        {
            return ResistHelpers.CalculateGrade(BaseLevel(id));
        }

        public void Refresh()
        {
            foreach (var level in Resists.Values)
            {
                level.Level.Reset();
            }
        }
    }
}
