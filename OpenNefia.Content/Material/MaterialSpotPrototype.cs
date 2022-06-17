using OpenNefia.Content.PCCs;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Utility;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using OpenNefia.Core.IoC;

namespace OpenNefia.Content.Material
{
    [Prototype("Elona.MaterialSpot")]
    public class MaterialSpotPrototype : IPrototype
    {
        [DataField("id", required: true)]
        public string ID { get; } = default!;

        [DataField(required: true)]
        private readonly List<PrototypeId<MaterialPrototype>> _materials = new();

        public IReadOnlyList<PrototypeId<MaterialPrototype>> Materials => _materials;
    }
}