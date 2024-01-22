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
    /// Indicates this component can be reset when its parent entity is refreshed
    /// via <see cref="IRefreshSystem.Refresh(EntityUid)"/>. Refreshing an entity clears the
    /// effects of temporary buffs.
    /// 
    /// All components with a <see cref="Stat{T}"/> property must implement this interface for consistency,
    /// and call <see cref="Stat{T}.Reset"/> on all stat properties in their <see cref="Refresh"/> method.
    /// </summary>
    public interface IComponentRefreshable : IComponent
    {
        /// <summary>
        /// Resets all <see cref="Stat{T}"/> properties on this entity, and anything
        /// else that can be effected by temporary buffs.
        /// </summary>
        void Refresh();
    }
}
