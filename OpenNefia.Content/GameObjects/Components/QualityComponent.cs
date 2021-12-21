﻿using OpenNefia.Content.Logic;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.GameObjects
{
    [RegisterComponent]
    public class QualityComponent : Component
    {
        public override string Name => "Quality";

        [DataField(required: true)]
        public Quality Quality { get; set; }
    }
}
