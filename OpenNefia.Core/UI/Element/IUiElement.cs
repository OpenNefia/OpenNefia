using OpenNefia.Core.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.UI.Element
{
    public interface IUiElement : IDrawable
    {
        Vector2 ExactSize { get; set; }
        Vector2 MinSize { get; set; }
        bool Visible { get; set; }

        void GetPreferredSize(out Vector2 size);
        void SetPreferredSize();
    }
}
