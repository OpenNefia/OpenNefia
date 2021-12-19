using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Maps
{
    /// <summary>
    /// Interface for loading and saving an entire instantiated map instance
    /// along with its entities.
    /// </summary>
    /// <remarks>
    /// This is used for game save purposes, not map archetype purposes.
    /// </remarks>
    public interface ISerialMapLoader
    {
        void LoadMap(MapId mapId, ResourcePath filepath);
        void SaveMap(MapId mapId, ResourcePath filepath);
    }
}
