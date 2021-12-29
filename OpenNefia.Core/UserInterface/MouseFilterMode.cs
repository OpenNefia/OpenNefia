using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.UserInterface
{
    /// <summary>
    ///     Mode that will be tested when testing controls to invoke mouse button events on.
    /// </summary>
    public enum MouseFilterMode : byte
    {
        /// <summary>
        ///     The control will be able to receive mouse buttons events.
        ///     Furthermore, if a control with this mode does get clicked,
        ///     the event automatically gets marked as handled after every other candidate has been tried,
        ///     so that the rest of the game does not receive it.
        /// </summary>
        Pass = 1,

        /// <summary>
        ///     The control will be able to receive mouse button events like <see cref="Pass" />,
        ///     but the event will be stopped and handled even if the relevant events do not handle it.
        /// </summary>
        Stop = 0,

        /// <summary>
        ///     The control will not be considered at all, and will not have any effects.
        /// </summary>
        Ignore = 2,
    }
}
