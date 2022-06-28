using JetBrains.Annotations;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;

namespace OpenNefia.Core.Areas
{
    internal sealed class AreaSystem : EntitySystem
    {
        [Dependency] private readonly IAreaManager _areaManager = default!;

        public override void Initialize()
        {
            base.Initialize();

            _areaManager.OnActiveAreaChanged += OnActiveAreaChanged;

            SubscribeComponent<AreaComponent, ComponentAdd>(OnAreaAdd);
            SubscribeComponent<AreaComponent, ComponentInit>(OnAreaInit);
            SubscribeComponent<AreaComponent, ComponentStartup>(OnAreaStartup);
        }

        private void OnActiveAreaChanged(IArea? newArea, IArea? oldArea)
        {
            if (oldArea != null)
            {
                var ev = new AreaExitedEvent(newArea, oldArea);
                RaiseLocalEvent(oldArea.AreaEntityUid, ev);
            }
            if (newArea != null)
            {
                var ev = new AreaEnteredEvent(newArea, oldArea);
                RaiseLocalEvent(newArea.AreaEntityUid, ev);
            }
        }

        private void OnAreaAdd(EntityUid uid, AreaComponent component, ComponentAdd args)
        {
            var msg = new AreaAddEvent(uid, component.AreaId);
            EntityManager.EventBus.RaiseEvent(uid, msg);
        }

        private void OnAreaInit(EntityUid uid, AreaComponent component, ComponentInit args)
        {
            var msg = new AreaInitializeEvent(uid, component.AreaId);
            EntityManager.EventBus.RaiseEvent(uid, msg);
        }

        private void OnAreaStartup(EntityUid uid, AreaComponent component, ComponentStartup args)
        {
            var msg = new AreaStartupEvent(uid, component.AreaId);
            EntityManager.EventBus.RaiseEvent(uid, msg);
        }
    }

    /// <summary>
    /// Raised whenever an area component starts up.
    /// </summary>
    public sealed class AreaStartupEvent : EntityEventArgs
    {
        public EntityUid EntityUid { get; }
        public AreaId AreaId { get; }

        public AreaStartupEvent(EntityUid uid, AreaId areaId)
        {
            EntityUid = uid;
            AreaId = areaId;
        }
    }

    /// <summary>
    /// Raised whenever an area is being initialized.
    /// </summary>
    public sealed class AreaInitializeEvent : EntityEventArgs
    {
        public EntityUid EntityUid { get; }
        public AreaId AreaId { get; }

        public AreaInitializeEvent(EntityUid uid, AreaId areaId)
        {
            EntityUid = uid;
            AreaId = areaId;
        }
    }

    /// <summary>
    /// Raised whenever an area is added.
    /// </summary>
    public sealed class AreaAddEvent : EntityEventArgs
    {
        public EntityUid EntityUid { get; }
        public AreaId AreaId { get; }

        public AreaAddEvent(EntityUid uid, AreaId areaId)
        {
            EntityUid = uid;
            AreaId = areaId;
        }
    }

    /// <summary>
    /// Raised whenever the current area changes.
    /// </summary>
    public sealed class AreaExitedEvent : EntityEventArgs
    {
        public IArea? NewArea { get; }
        public IArea OldArea { get; }

        public AreaExitedEvent(IArea? newArea, IArea oldArea)
        {
            NewArea = newArea;
            OldArea = oldArea;
        }
    }

    /// <summary>
    /// Raised whenever the current area changes.
    /// </summary>
    public sealed class AreaEnteredEvent : EntityEventArgs
    {
        public IArea NewArea { get; }
        public IArea? OldArea { get; }

        public AreaEnteredEvent(IArea newArea, IArea? oldArea = null)
        {
            NewArea = newArea;
            OldArea = oldArea;
        }
    }
}
