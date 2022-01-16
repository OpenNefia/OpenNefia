using OpenNefia.Core.GameObjects;
using OpenNefia.Core.UI.Element;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Rendering
{
    /// <summary>
    /// <para>
    /// Manages entity drawables.
    /// </para>
    /// <para>
    /// "Entity drawables" are pieces of state that need special rendering logic, like PCCs,
    /// which can't be rendered in a separate <see cref="TileDrawLayers.ITileLayer"/>
    /// because they need proper Z-ordering and occlusion in <see cref="TileDrawLayers.TileAndChipTileLayer"/>.
    /// </para>
    /// </summary>
    public sealed class EntityDrawablesComponent : Component
    {
        public override string Name => "EntityDrawables";

        /// <summary>
        /// List of drawables. Not serialized; rebuilt when the entity is instantiated/deserialized. 
        /// Entity systems should modify this collection within <see cref="ComponentStartup"/> and 
        /// <see cref="ComponentShutdown"/> event handlers.
        /// </summary>
        public Dictionary<string, EntityDrawableEntry> EntityDrawables { get; } = new();
    }
}
