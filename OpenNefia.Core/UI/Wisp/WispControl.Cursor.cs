using System;
using OpenNefia.Core.Graphics;

namespace OpenNefia.Core.UI.Wisp
{
    public partial class WispControl
    {
        private CursorShape _cursorShape;

        /// <summary>
        ///     The shape the cursor will get when being over this control.
        /// </summary>
        public CursorShape DefaultCursorShape
        {
            get => _cursorShape;
            set
            {
                _cursorShape = value;
                WispManager.CursorChanged(this);
            }
        }
    }
}
