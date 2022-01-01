using OpenNefia.Core.Containers;
using OpenNefia.Core.Maps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.GameObjects
{
    internal partial class ComponentFactory
    {
        /// <summary>
        /// Register components that must always be available to the engine.
        /// </summary>
        public void DoDefaultRegistrations()
        {
            RegisterClass<MetaDataComponent>();
            RegisterClass<SpatialComponent>();
            RegisterClass<MapComponent>();
            RegisterClass<MapSaveIdComponent>();
            RegisterClass<ContainerManagerComponent>();
            RegisterClass<StackComponent>();
            RegisterClass<MapEntityLookupComponent>();
        }
    }
}
