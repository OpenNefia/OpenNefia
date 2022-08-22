using OpenNefia.Content.GameObjects.Components;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Stats;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Enchantments
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class HasCursedEnchantmentComponent : Component, IComponentRefreshable
    {
        public override string Name => "HasCursedEnchantment";

        /// <summary>
        /// If true, does random teleports/other cursed actions.
        /// </summary>
        [DataField]
        public Stat<bool> HasCursedEnchantment { get; set; } = new(true);

        public void Refresh()
        {
            HasCursedEnchantment.Reset();
        }
    }
}