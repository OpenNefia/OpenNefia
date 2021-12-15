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
        public static void RunMapInit(this Entity entity)
        {
            DebugTools.Assert(entity.LifeStage == EntityLifeStage.Initialized);
            entity.LifeStage = EntityLifeStage.MapInitialized;

            var ev = new MapInitEvent();
            entity.EntityManager.EventBus.RaiseLocalEvent(entity.Uid, ref ev, false);
        }
    }
}
