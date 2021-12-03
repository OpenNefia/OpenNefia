using OpenNefia.Core.Utility;

namespace OpenNefia.Core.GameObjects
{
    /// <summary>
    ///     Raised directed on an entity when the map is initialized.
    /// </summary>
    public class MapInitEvent : EntityEventArgs
    {
    }

    public static class MapInitExt
    {
        private static readonly MapInitEvent MapInit = new MapInitEvent();

        public static void RunMapInit(this Entity entity)
        {
            DebugTools.Assert(entity.LifeStage == EntityLifeStage.Initialized);
            entity.LifeStage = EntityLifeStage.MapInitialized;

            entity.EntityManager.EventBus.RaiseLocalEvent(entity.Uid, MapInit, false);
        }
    }
}
