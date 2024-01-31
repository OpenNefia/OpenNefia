using OpenNefia.Content.Charas;
using OpenNefia.Content.Inventory;
using OpenNefia.Content.Items;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.VisualAI.Engine;

namespace OpenNefia.VisualAI.Block
{
    [ImplicitDataDefinitionForInheritors]
    public interface IVisualAITargetSource
    {
        IEnumerable<IVisualAITargetValue> GetTargets(VisualAIState state);
    }

    public sealed class AllEntitiesTargetSource : IVisualAITargetSource
    {
        [Dependency] private readonly IEntityLookup _lookup = default!;

        public IEnumerable<IVisualAITargetValue> GetTargets(VisualAIState state)
        {
            return _lookup.EntityQueryInMap<SpatialComponent>(state.Map)
                .Select(s => new VisualAIEntityTarget(s.Owner));
        }
    }

    public sealed class CharactersTargetSource : IVisualAITargetSource
    {
        [Dependency] private readonly IEntityLookup _lookup = default!;

        public IEnumerable<IVisualAITargetValue> GetTargets(VisualAIState state)
        {
            return _lookup.EntityQueryInMap<CharaComponent>(state.Map)
                .Select(s => new VisualAIEntityTarget(s.Owner));
        }
    }

    public sealed class ItemsOnGroundTargetSource : IVisualAITargetSource
    {
        [Dependency] private readonly IEntityLookup _lookup = default!;

        public IEnumerable<IVisualAITargetValue> GetTargets(VisualAIState state)
        {
            return _lookup.EntityQueryInMap<ItemComponent>(state.Map)
                .Select(s => new VisualAIEntityTarget(s.Owner));
        }
    }

    public sealed class ItemsInInventoryTargetSource : IVisualAITargetSource
    {
        [Dependency] private readonly IInventorySystem _inv = default!;

        public IEnumerable<IVisualAITargetValue> GetTargets(VisualAIState state)
        {
            return _inv.EnumerateInventory(state.AIEntity)
                .Select(ent => new VisualAIEntityTarget(ent));
        }
    }

    public sealed class MapPositionsTargetSource : IVisualAITargetSource
    {
        public IEnumerable<IVisualAITargetValue> GetTargets(VisualAIState state)
        {
            return state.Map.AllTiles.Select(t => new VisualAIPositionTarget(t.MapPosition));
        }
    }
}