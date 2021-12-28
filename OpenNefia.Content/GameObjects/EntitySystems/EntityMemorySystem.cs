using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;

namespace OpenNefia.Content.GameObjects
{
    public interface IEntityMemorySystem : IEntitySystem
    {
        MapObjectMemory GetEntityMemory(EntityUid entity);
    }

    public class EntityMemorySystem : EntitySystem, IEntityMemorySystem
    {
        public override void Initialize()
        {
            SubscribeLocalEvent<ChipComponent, GetMapObjectMemoryEventArgs>(ProduceSpriteMemory, nameof(ProduceSpriteMemory));
            SubscribeLocalEvent<CharaComponent, GetMapObjectMemoryEventArgs>(HideWhenOutOfSight, nameof(HideWhenOutOfSight));
        }

        private void HideWhenOutOfSight(EntityUid uid, CharaComponent component, GetMapObjectMemoryEventArgs args)
        {
            args.Memory.HideWhenOutOfSight = true;
        }

        private void ProduceSpriteMemory(EntityUid uid, ChipComponent chip, GetMapObjectMemoryEventArgs args)
        {
            var memory = args.Memory;
            memory.AtlasIndex = chip.ChipID.ResolvePrototype().Image.AtlasIndex;
            memory.Color = chip.Color;
            memory.IsVisible = true;
            memory.ScreenOffset = Vector2i.Zero;
            memory.IsVisible = EntityManager.IsAlive(uid);
            memory.HideWhenOutOfSight = false;
        }

        public MapObjectMemory GetEntityMemory(EntityUid entity)
        {
            var memory = new MapObjectMemory();
            var ev = new GetMapObjectMemoryEventArgs(memory);
            RaiseLocalEvent(entity, ev);
            return memory;
        }
    }
}
