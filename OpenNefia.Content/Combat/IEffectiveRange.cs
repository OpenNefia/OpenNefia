using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Combat
{
    [ImplicitDataDefinitionForInheritors]
    public interface IRangedAccuracy
    {
        /// <returns>Accuracy modifier as a percentage (1.0 as 100%)</returns>
        public float GetAccuracyModifier(float distance);
    }

    public sealed class RangedAccuracyTable : IRangedAccuracy
    {
        [DataField]
        public List<float> Table { get; set; } = new();

        public float GetAccuracyModifier(float distance)
        {
            if (Table.Count == 0)
                return 1f;

            var index = Math.Clamp((int)distance, 0, Table.Count - 1);
            return Table[index];
        }
    }
}
