using Melanchall.DryWetMidi.MusicTheory;
using OpenNefia.Content.EquipSlots;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Rendering;
using OpenNefia.Content.Prototypes;

namespace OpenNefia.Content.Inventory
{
    public class EntityEquipmentSource : IInventorySource
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IEquipSlotsSystem _equipSlots = default!;

        public EntityUid Entity { get; }

        public EntityEquipmentSource(EntityUid entity)
        {
            Entity = entity;
        }

        public IEnumerable<EntityUid> GetEntities()
        {
            if (!_entityManager.TryGetComponent(Entity, out EquipSlotsComponent? equipSlots))
                return Enumerable.Empty<EntityUid>();

            return _equipSlots.EnumerateEquippedEntities(Entity, equipSlots);
        }

        public void OnDraw(float uiScale, float x, float y)
        {
            Assets.Get(Protos.Asset.EquippedIcon).Draw(uiScale, x - 12, y + 14);
        }
    }
}
