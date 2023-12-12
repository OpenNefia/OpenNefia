using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Stats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.GameObjects.Components
{
    /// <summary>
    /// All components with a <see cref="Stat{T}"/> property should implement this interface,
    /// and call <see cref="Stat{T}.Reset"/> in their <see cref="IComponentRefreshable.Refresh"/> method.
    /// </summary>
    public interface IComponentRefreshable : IComponent
    {
        void Refresh();
    }
}
