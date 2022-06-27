using OpenNefia.Core.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Buffs
{
    public interface IBuffsSystem : IEntitySystem
    {
        void RemoveAllBuffs(EntityUid entity);
    }

    public sealed class BuffsSystem : EntitySystem, IBuffsSystem
    {
        public void RemoveAllBuffs(EntityUid entity)
        {
            // TODO
        }
    }
}
