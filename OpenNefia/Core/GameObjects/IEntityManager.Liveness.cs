using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.GameObjects
{
    public partial interface IEntityManager
    {
        /// <summary>
        /// Tests if this entity is active in the game map.
        /// </summary>
        bool IsAlive(EntityUid uid);

        /// <summary>
        /// Tests if this entity is invalid and can be removed.
        /// </summary>
        bool IsDeadAndBuried(EntityUid uid);
    }
}
