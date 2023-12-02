using OpenNefia.Core.Areas;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.Scenarios
{
    /// <summary>
    /// Defines a set of start/win conditions, selected during character creation. In previous
    /// variants of Elona there was only ever one scenario with one or more acts. This system will
    /// allow for greater customization of game progression.
    /// </summary>
    [Prototype("Elona.Scenario")]
    public class ScenarioPrototype : IPrototype
    {
        [IdDataField]
        public string ID { get; } = default!;

        /// <summary>
        /// List of global areas to initialize on a new game. They will be initialized in the order given.
        /// 
        /// Typical usage: First initialize the global map(s), such that the positions of entrances
        /// into each town map can be saved. Then initialize each town so that quests can be generated
        /// between them. This way delivery quests can be generated (as they rely on the positions
        /// of more than one town on a global map).
        /// </summary>
        [DataField("initGlobalAreas")]
        private List<GlobalAreaId> _initGlobalAreas { get; } = new();
        public IReadOnlyList<GlobalAreaId> InitGlobalAreas => _initGlobalAreas;
    }
}