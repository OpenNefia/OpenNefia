using OpenNefia.Content.PCCs;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Utility;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using OpenNefia.Core.IoC;

namespace OpenNefia.Content.MaterialSpot
{
    [Prototype("Elona.MaterialSpot")]
    public class MaterialSpotPrototype : IPrototype
    {
        [DataField("id", required: true)]
        public string ID { get; } = default!;

        [DataField("materials", required: true)]
        private readonly List<PrototypeId<BlendMaterialPrototype>> _materials = new();

        public IReadOnlyList<PrototypeId<BlendMaterialPrototype>> Materials => _materials;
    }
}