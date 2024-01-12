using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Patches
{
    public interface IPatchManager
    {
    }

    internal interface IPatchManagerInternal : IPatchManager
    {
        void Initialize();
        void Shutdown();
    }
}
