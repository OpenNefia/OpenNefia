using OpenNefia.Core.IoC;

namespace OpenNefia.Core.GameObjects
{
    public class StackableSystem : EntitySystem
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;

        public override void Initialize()
        {
            SubscribeLocalEvent<StackableComponent, MapInitEvent>(HandleEntityInitialized, nameof(HandleEntityInitialized));
        }

        /// <summary>
        /// Ensure that the liveness based on stackable amount is properly initialized.
        /// It has a dependency on <see cref="MetaDataComponent"/>.
        /// </summary>
        private void HandleEntityInitialized(EntityUid uid, StackableComponent stackable, ref MapInitEvent args)
        {
            MetaDataComponent? metaData = null;

            if (!Resolve(uid, ref metaData))
                return;

            if (stackable.Amount <= 0)
            {
                stackable.Amount = 0;
                metaData.Liveness = EntityGameLiveness.DeadAndBuried;
            }
        }
    }
}
