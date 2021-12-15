using System.Collections.Generic;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Utility;

namespace OpenNefia.Core.GameObjects
{
    /// <summary>
    ///     Raised directed on an entity when the map is initialized.
    /// </summary>
    public struct MapInitEvent
    {
    }

    public static class MapInitExt
    {
        public static void RunMapInit(this EntityUid entity)
        {
            var entMan = IoCManager.Resolve<IEntityManager>();
            var meta = entMan.GetComponent<MetaDataComponent>(entity);
            DebugTools.Assert(meta.EntityLifeStage == EntityLifeStage.Initialized);
            meta.EntityLifeStage = EntityLifeStage.MapInitialized;

            var ev = new MapInitEvent();
            entMan.EventBus.RaiseLocalEvent(entity, ref ev, false);
        }
    }
}
