using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.ContentPack;

namespace OpenNefia.Core.SaveGames
{
    /// <summary>
    /// Manages the filesystem local to a single saved game.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The files in a saved game are tracked using a caching mechanism
    /// to allow resetting to the state of the last save. When a file in the cache is
    /// written to for the first time, the new data is written to the cache first
    /// under the same filename. When the player requests to save the game, all files
    /// in the cache are moved into the save, replacing the original copies there.
    /// </para>
    /// <para>
    /// Conversely, when a game is loaded, the cache is wiped first before the loading
    /// logic takes place.
    /// </para>
    /// <para>
    /// The API for this interface closely follows that of <see cref="IWritableDirProvider"/>.
    /// </para>
    /// </remarks>
    public interface ISaveGameDirProvider : IWritableDirProvider
    {
        /// <summary>
        /// Copies all files from the cache to the committed file location.
        /// </summary>
        void Commit();

        /// <summary>
        /// Clears all temporary files.
        /// </summary>
        void ClearTemp();
    }
}
