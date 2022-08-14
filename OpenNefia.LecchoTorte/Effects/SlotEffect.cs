using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Serialization;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.UI.Layer;
using static OpenNefia.Core.Prototypes.EntityPrototype;

namespace OpenNefia.Content.Effects
{
    public class SlotEffect : Effect, ISerializationHooks
    {
        [DataField]
        public ComponentRegistry Components { get; set; } = new();

        public override EffectResult Apply(EntityUid source, MapCoordinates coords, EntityUid target, EffectArgs args)
        {
            EntitySystem.Get<ISlotSystem>().AddSlot(target, Components);

            return EffectResult.Succeeded;
        }
    }
}
