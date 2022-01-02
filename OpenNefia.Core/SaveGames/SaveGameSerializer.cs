using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.SaveGames
{
    /// <summary>
    /// Handles converting the serializable data of the current game state
    /// into a loadable format. This wraps <see cref="ISaveGameManager"/> by adding
    /// game save format support.
    /// </summary>
    public interface ISaveGameSerializer
    {
    }

    public class SaveGameSerializer : ISaveGameSerializer
    {
    }
}
