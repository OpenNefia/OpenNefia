using OpenNefia.Content.Nefia.Layout;
using OpenNefia.Core.Areas;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Utility;

namespace OpenNefia.Content.Nefia
{
    /// <summary>
    /// Algorithm that picks a <see cref="INefiaFloorTemplate"/> randomly. Can also
    /// optionally setup data for generation.
    /// </summary>
    [ImplicitDataDefinitionForInheritors]
    public interface IVanillaNefiaTemplate
    {
        /// <summary>
        /// Given the nefia floor number being generated, selects an appropriate floor layout.
        /// </summary>
        public IVanillaNefiaLayout GetLayout(int floorNumber, Blackboard<NefiaGenParams> data);

        /// <summary>
        /// Runs extra setup on the generated map.
        /// </summary>
        public void AfterGenerateMap(IArea area, IMap map, int floorNumber, Blackboard<NefiaGenParams> data) { }
    }
}