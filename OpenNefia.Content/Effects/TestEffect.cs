using OpenNefia.Core.Data.Types;
using OpenNefia.Core.Effects;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Serialization;
using OpenNefia.Core.UI.Layer;

namespace OpenNefia.Content.Effects
{
    public class TestEffect : Effect, ISerializationHooks
    {       
        // TODO flyweight this
        [Dependency] private readonly IMapDrawables _mapDrawables = default!;

        void ISerializationHooks.AfterDeserialization() 
        {
            IoCManager.InjectDependencies(this);
        }

        public override EffectResult Apply(EntityUid source, MapCoordinates coords, EntityUid target, EffectArgs args)
        {
            var drawable = new BasicAnimMapDrawable(BasicAnimPrototypeOf.AnimCurse);
            _mapDrawables.Enqueue(drawable, coords);

            return EffectResult.Succeeded;
        }
    }
}
