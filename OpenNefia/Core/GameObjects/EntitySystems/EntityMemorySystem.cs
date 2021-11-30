using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;

namespace OpenNefia.Core.GameObjects.EntitySystems
{
    public class EntityMemorySystem : EntitySystem
    {
        public override void Initialize()
        {
            SubscribeLocalEvent<ChipComponent, GetMapObjectMemoryEventArgs>(ProduceSpriteMemory);
        }

        private void ProduceSpriteMemory(EntityUid uid, ChipComponent chip, GetMapObjectMemoryEventArgs args)
        {
            var memory = args.Memory;
            memory.AtlasIndex = chip.ID.ResolvePrototype().Image.AtlasIndex;
            memory.Color = chip.Color;
            memory.IsVisible = true;
            memory.ScreenOffset = Vector2i.Zero;
        }
    }
}
