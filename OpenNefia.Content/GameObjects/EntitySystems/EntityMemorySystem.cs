using OpenNefia.Content.Charas;
using OpenNefia.Content.Items;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;
using OpenNefia.Content.Visibility;
using OpenNefia.Core.Game;

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
        [Dependency] private readonly IVisibilitySystem _vis = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;

        public override void Initialize()
        {
            SubscribeComponent<ChipComponent, GetMapObjectMemoryEventArgs>(ProduceSpriteMemory, priority: EventPriorities.VeryHigh);
            SubscribeComponent<CharaComponent, GetMapObjectMemoryEventArgs>(SetCharaMemory, priority: EventPriorities.High);
            SubscribeComponent<ItemComponent, GetMapObjectMemoryEventArgs>(SetItemMemory, priority: EventPriorities.High);
        }

        private void SetCharaMemory(EntityUid uid, CharaComponent component, GetMapObjectMemoryEventArgs args)
        {
            args.OutMemory.HideWhenOutOfSight = true;
            args.OutMemory.ShadowType = ShadowType.Normal;
        }

        private void SetItemMemory(EntityUid uid, ItemComponent component, GetMapObjectMemoryEventArgs args)
        {
            args.OutMemory.ShadowType = ShadowType.DropShadow;
        }

        private void ProduceSpriteMemory(EntityUid uid, ChipComponent chip, GetMapObjectMemoryEventArgs args)
        {
            // NOTE: All fields need to be reinitialized here as the memory is pooled!
            var chipProto = _protos.Index(chip.ChipID);
            var memory = args.OutMemory;
            memory.AtlasIndex = chipProto.Image.AtlasIndex;
            memory.Color = chip.Color;
            memory.ScreenOffset = chipProto.Offset;
            memory.ShadowRotationRads = (float)Angle.FromDegrees(chipProto.ShadowRotation).Theta / 2;
            memory.IsVisible = _vis.CanSeeEntity(_gameSession.Player, uid, ignoreLos: true);
            memory.ZOrder = chip.DrawDepth;
            memory.HideWhenOutOfSight = false;
            memory.ShadowType = ShadowType.None;
            memory.ShadowRotationRads = .15f;
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
