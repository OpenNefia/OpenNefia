using OpenNefia.Content.Charas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;

namespace OpenNefia.Content.GameObjects
{
    public interface IEntityMemorySystem : IEntitySystem
    {
        void GetEntityMemory(EntityUid entity, ref MapObjectMemory memory);
        MapObjectMemory GetEntityMemory(EntityUid entity);
    }

    public class EntityMemorySystem : EntitySystem, IEntityMemorySystem
    {
        [Dependency] private readonly IPrototypeManager _protos = default!;

        public override void Initialize()
        {
            SubscribeComponent<ChipComponent, GetMapObjectMemoryEventArgs>(ProduceSpriteMemory, priority: EventPriorities.VeryHigh);
            SubscribeComponent<CharaComponent, GetMapObjectMemoryEventArgs>(HideWhenOutOfSight, priority: EventPriorities.High);
        }

        private void HideWhenOutOfSight(EntityUid uid, CharaComponent component, GetMapObjectMemoryEventArgs args)
        {
            args.Memory.HideWhenOutOfSight = true;
        }

        private void ProduceSpriteMemory(EntityUid uid, ChipComponent chip, GetMapObjectMemoryEventArgs args)
        {
            var chipProto = _protos.Index(chip.ChipID);
            var memory = args.Memory;
            memory.AtlasIndex = chipProto.Image.AtlasIndex;
            memory.Color = chip.Color;
            memory.ScreenOffset = chipProto.Offset;
            memory.IsVisible = EntityManager.IsAlive(uid);
            memory.ZOrder = chip.DrawDepth;
            memory.HideWhenOutOfSight = false;
        }

        public void GetEntityMemory(EntityUid entity, ref MapObjectMemory memory)
        {
            var ev = new GetMapObjectMemoryEventArgs(memory);
            RaiseEvent(entity, ev);
        }

        public MapObjectMemory GetEntityMemory(EntityUid entity)
        {
            var memory = new MapObjectMemory();
            GetEntityMemory(entity, ref memory);
            return memory;
        }
    }
}
