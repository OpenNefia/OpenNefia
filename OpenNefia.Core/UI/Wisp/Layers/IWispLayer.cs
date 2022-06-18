using OpenNefia.Core.UI.Layer;
using OpenNefia.Core.UI.Wisp.Controls;
using OpenNefia.Core.UI.Wisp.CustomControls;

namespace OpenNefia.Core.UI.Wisp
{
    public interface IWispLayer : IUiLayer
    {
        /// <summary>
        /// Wisp root control, for general-purpose use.
        /// </summary>
        WispRoot WispRoot { get; }

        /// <summary>
        /// <para>
        /// Container that holds controls inheriting from <see cref="BaseWindow"/>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// <para>
        /// NOTE: If you try to attach the windows to <see cref="WispRoot"/>,
        /// they won't be laid out correctly since its layouting behavior differs
        /// from <see cref="LayoutContainer"/>. As such, you *must* use this control
        /// for displaying new <see cref="BaseWindow"/>-derived controls.
        /// </para>
        /// </remarks>
        LayoutContainer WindowRoot { get; }

        bool Debug { get; set; }
    }
}