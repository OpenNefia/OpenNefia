using OpenNefia.Content.Sleep;
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

        void DamageSanity(EntityUid uid, int damageAmount, SanityComponent? sanity = null);
    }

    public sealed class SanitySystem : EntitySystem, ISanitySystem
    {
        public override void Initialize()
        {
            SubscribeComponent<SanityComponent, OnCharaSleepEvent>(HandleCharaSleep);
        }

        private void HandleCharaSleep(EntityUid uid, SanityComponent component, OnCharaSleepEvent args)
        {
            HealInsanity(uid, 10, component);
        }

        /// <inheritdoc/>
        public void HealInsanity(EntityUid uid, int healAmount, SanityComponent? sanity = null)
        {
            if (!Resolve(uid, ref sanity))
                return;

            healAmount = Math.Max(healAmount, 0);
            sanity.Insanity = Math.Max(sanity.Insanity - healAmount, 0);
        }

        public void DamageSanity(EntityUid uid, int damageAmount, SanityComponent? sanity = null)
        {
            // TODO
        }
    }
}
