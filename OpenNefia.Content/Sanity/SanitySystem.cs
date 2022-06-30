using OpenNefia.Core.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Sanity
{
    public interface ISanitySystem : IEntitySystem
    {
        /// <hsp>#deffunc healSAN int tc,int dmg</hsp>
        void HealInsanity(EntityUid uid, int healAmount, SanityComponent? sanity = null);
    }

    public sealed class SanitySystem : EntitySystem, ISanitySystem
    {
        /// <inheritdoc/>
        public void HealInsanity(EntityUid uid, int healAmount, SanityComponent? sanity = null)
        {
            if (!Resolve(uid, ref sanity))
                return;

            healAmount = Math.Max(healAmount, 0);
            sanity.Insanity = Math.Max(sanity.Insanity - healAmount, 0);
        }
    }
}
