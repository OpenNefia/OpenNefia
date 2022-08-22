using OpenNefia.Content.GameObjects;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Random;

namespace OpenNefia.Content.Fame
{
    public interface IFameSystem : IEntitySystem
    {
        /// <hsp>#deffunc decFame int c, int per</hsp>
        int DecrementFame(EntityUid ent, int fraction, FameComponent? fame = null);

        /// <hsp>#deffunc calcFame int c, int per</hsp>
        int CalcFameGained(EntityUid ent, int baseAmount, FameComponent? fame = null);
    }

    public sealed class FameSystem : EntitySystem, IFameSystem
    {
        [Dependency] private readonly IRandom _rand = default!;

        /// <inheritdoc/>
        public int DecrementFame(EntityUid ent, int fraction, FameComponent? fame = null)
        {
            if (!Resolve(ent, ref fame))
                return 0;
            
            var delta = fame.Fame.Base / fraction + 5;
            delta += _rand.Next(delta / 2) - _rand.Next(delta / 2);
            fame.Fame.Base = Math.Max(fame.Fame.Base - delta, 0);
            return delta;
        }

        /// <inheritdoc/>
        public int CalcFameGained(EntityUid ent, int baseAmount, FameComponent? fame = null)
        {
            if (!Resolve(ent, ref fame))
                return 0;
            
            var ret = baseAmount * 100 / (100 + fame.Fame.Base / 100 * (fame.Fame.Base / 100) / 2500);
            if (ret < 5)
                ret = _rand.Next(5) + 1;

            return ret;
        }
    }
}