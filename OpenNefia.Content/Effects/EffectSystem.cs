using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.Serialization.Manager;

namespace OpenNefia.Content.Effects
{
    public interface IEffectSystem : IEntitySystem
    {
        TurnResult Apply<T>(EntityUid source, EntityUid target, EntityCoordinates coords, EntityUid? verb, EffectArgSet args)
            where T: class, IEffect, new();
        TurnResult Apply(IEffect effect, EntityUid source, EntityUid target, EntityCoordinates coords, EntityUid? verb,
            EffectArgSet args);
    }

    public sealed class EffectSystem : EntitySystem, IEffectSystem
    {
        public override void Initialize()
        {
        }

        TurnResult IEffectSystem.Apply<T>(EntityUid source, EntityUid target, EntityCoordinates coords, EntityUid? verb, EffectArgSet args)
        {
            var effect = new T();
            // TODO: hack until auto-injection is supported by serialization
            EntitySystem.InjectDependencies(effect);
            return effect.Apply(source, target, coords, verb, args);
        }

        public TurnResult Apply(IEffect effect, EntityUid source, EntityUid target, EntityCoordinates coords, EntityUid? verb,
            EffectArgSet args)
        {
            // TODO: hack until auto-injection is supported by serialization
            EntitySystem.InjectDependencies(effect);
            return effect.Apply(source, target, coords, verb, args);
        }
    }
}