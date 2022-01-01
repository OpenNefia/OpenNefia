using System.Collections.Generic;
using OpenNefia.Analyzers;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Utility;

namespace OpenNefia.Core.GameObjects
{
    /// <summary>
    ///     Raised directed on an entity when the map is initialized.
    /// </summary>
    [EventArgsUsage(EventArgsTargets.ByRef)]
    public struct EntityMapInitEvent
    {
    }

    public static class MapInitExt
    {
        public static void RunMapInit(this EntityUid entity)
        {
            var entMan = IoCManager.Resolve<IEntityManager>();
            var meta = entMan.GetComponent<MetaDataComponent>(entity);

            if (meta.EntityLifeStage == EntityLifeStage.MapInitialized)
                return; // Already map initialized, do nothing.

            meta.EntityLifeStage = EntityLifeStage.MapInitialized;

            var ev = new EntityMapInitEvent();
            entMan.EventBus.RaiseLocalEvent(entity, ref ev, false);
        }
    }
}
