using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Content.GameObjects.Components;
using OpenNefia.Content.RandomGen;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Stats;

namespace OpenNefia.Content.Pregnancy
{
    [RegisterComponent]
    public class PregnancyComponent : Component, IComponentRefreshable
    {
        [DataField]
        public bool IsPregnant { get; set; }

        /// <summary>
        /// Determines the type of child to birth.
        /// </summary>
        [DataField]
        public CharaFilter ChildFilter { get; set; } = new();

        [DataField]
        public Stat<bool> IsProtectedFromPregnancy { get; set; } = new(false);

        public void Refresh()
        {
            IsProtectedFromPregnancy.Reset();
        }
    }
}