using OpenNefia.Core.GameObjects;
using OpenNefia.Content.Maps;

namespace OpenNefia.Content.Areas
{
    /// <summary>
    /// Used by the encounter system to calculate distance to the nearest town.
    /// </summary>
    /// <hsp>areaType(areaId)=mTypeTown</hsp>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Area)]
    public sealed class AreaTypeTownComponent : Component
    {
    }
}