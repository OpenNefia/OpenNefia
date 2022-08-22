using OpenNefia.Core.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.GameObjects.Components
{
    public interface IComponentRefreshable : IComponent
    {
        void Refresh();
    }
}
