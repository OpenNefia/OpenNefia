using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.UI
{
    public interface IUiInput
    {
        List<UiKeyHint> MakeKeyHints();
    }
}
