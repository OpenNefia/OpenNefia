using OpenNefia.Content.Prototypes;
using OpenNefia.Core;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Items
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class JapaneseCounterComponent : Component, IComponentLocalizable
    {
        public override string Name => "JapaneseCounter";

        [Localize]
        public string? CounterText { get; set; }

        void IComponentLocalizable.LocalizeFromLua(NLua.LuaTable table)
        {
            CounterText = table.GetStringOrNull(nameof(CounterText));
        }
    }
}