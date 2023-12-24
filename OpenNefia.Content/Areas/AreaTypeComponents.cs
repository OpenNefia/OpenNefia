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

    /// <summary>
    /// Area type that can hold maps that can be the target of the Return spell.
    /// The topmost area that has this component is considered the "global area"
    /// and is tracked by <see cref="IAreaEntranceSystem.CurrentGlobalAreaID"/>.
    /// </summary>
    /// <remarks>
    /// NOTE: It is important to have a top-level area with this component as the root
    /// of all areas, or else the player won't be able to cast Return.
    /// </remarks>
    /// <hsp>areaType(areaId)=mTypeWorld</hsp>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Area)]
    public sealed class AreaTypeGlobalComponent : Component
    {
    }
}