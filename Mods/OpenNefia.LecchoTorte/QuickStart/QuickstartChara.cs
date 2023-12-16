using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Resists;
using OpenNefia.Content.Skills;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenNefia.Core.Prototypes.EntityPrototype;

namespace OpenNefia.LecchoTorte.QuickStart
{
    /// <summary>
    /// Status of the quickstart player
    /// </summary>
    /// <remarks>
    /// TODO It would be nice to be able to merge in skills/resists, but the current serializer doesn't handle nested field merging yet.
    /// </remarks>
    [DataDefinition]
    public sealed class QuickstartChara
    {
        [DataField]
        public PrototypeId<EntityPrototype> ID { get; } = Protos.Chara.Putit;

        [DataField]
        public int Level { get; set; } = 1;

        [DataField]
        public Dictionary<PrototypeId<SkillPrototype>, LevelAndPotential> Skills { get; } = new();

        [DataField]
        public Dictionary<PrototypeId<ElementPrototype>, LevelAndPotential> Resists { get; } = new();

        [DataField]
        public List<QuickstartItem> Items { get; } = new();

        [DataField]
        public ComponentRegistry Components { get; } = new();
    }
}
