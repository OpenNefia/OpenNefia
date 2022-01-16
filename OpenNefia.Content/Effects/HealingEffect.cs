using OpenNefia.Content.Logic;
using OpenNefia.Content.Rendering;
using OpenNefia.Content.UI;
using OpenNefia.Core.Audio;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Serialization;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.DisplayName;

namespace OpenNefia.Content.Effects
{
    public class HealingEffect : Effect, ISerializationHooks
    {
        // TODO flyweight this
        [Dependency] private readonly IMapDrawables _mapDrawables = default!;
        [Dependency] private readonly IDisplayNameSystem _displayNames = default!;

        void ISerializationHooks.AfterDeserialization()
        {
            IoCManager.InjectDependencies(this);
        }

        public override EffectResult Apply(EntityUid source, MapCoordinates coords, EntityUid target, EffectArgs args)
        {
            Mes.Display($"{_displayNames.GetDisplayName(target)} is (supposed to be) healed.", UiColors.MesGreen);

            var drawable = new ParticleMapDrawable(Protos.Asset.HealEffect, Protos.Sound.Heal1, 5f);
            _mapDrawables.Enqueue(drawable, coords);

            return EffectResult.Succeeded;
        }
    }
}
