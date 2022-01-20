using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Rendering
{
    public interface IFontManager
    {
        void Initialize();
        Love.Font GetFont(FontSpec spec);
        Love.Font GetFont(FontSpec spec, float uiScale);
        void Clear();
    }
}
