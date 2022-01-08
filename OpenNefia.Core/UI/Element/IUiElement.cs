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
        void GetPreferredSize(out Vector2i size);
        void SetPreferredSize();
    }
}
