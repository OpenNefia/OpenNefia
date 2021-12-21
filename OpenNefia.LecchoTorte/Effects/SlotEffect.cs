using OpenNefia.Core.Data.Types;
using OpenNefia.Core.Effects;
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
        // TODO flyweight this
        [Dependency] private readonly ISlotSystem _slotSystem = default!;

        [DataField]
        public ComponentRegistry Components { get; set; } = new();

        void ISerializationHooks.AfterDeserialization() 
        {
            IoCManager.InjectDependencies(this);
        }

        public override EffectResult Apply(EntityUid source, MapCoordinates coords, EntityUid target, EffectArgs args)
        {

            return EffectResult.Succeeded;
        }
    }
}
