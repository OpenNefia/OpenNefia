using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core
{
    public interface ILocalizable
    {
        bool IsLocalized { get; }
        void Localize(LocaleKey key);
    }
}
