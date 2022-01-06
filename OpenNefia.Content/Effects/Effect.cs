using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;

namespace OpenNefia.Content.Effects
{
    public abstract class Effect : IEffect
    {
        private IEntityManager? _entityManager;
        protected IEntityManager EntityManager
        {
            get => _entityManager ??= IoCManager.Resolve<IEntityManager>();
        }

        public abstract EffectResult Apply(EntityUid source, MapCoordinates coords, EntityUid target, EffectArgs args);
    }
}
