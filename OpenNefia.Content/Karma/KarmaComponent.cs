﻿using OpenNefia.Content.GameObjects.Components;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Stats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Karma
{
    [RegisterComponent]
    public sealed class KarmaComponent : Component, IComponentRefreshable
    {
        [DataField]
        public Stat<int> Karma { get; set; } = new(0);

        [DataField]
        public Stat<bool> IsIncognito { get; set; } = new(false);

        public void Refresh()
        {
            Karma.Reset();
            IsIncognito.Reset();
        }
    }

    public static class KarmaLevels
    {
        public const int Bad = -30;

        /// <summary>
        /// Threshold for the redemption wish to take effect.
        /// </summary>
        public const int Neutral = 0;
        
        public const int Good = 20;
    }
}
