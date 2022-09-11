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

namespace OpenNefia.Content.Effects
{
    public interface IEffectSystem : IEntitySystem
    {
        TurnResult Apply<T>(EntityUid source, EntityUid target, EntityCoordinates coords, EntityUid? verb, EffectArgSet args)
            where T: class, IEffect, new();
    }

    public sealed class EffectSystem : EntitySystem, IEffectSystem
    {
        public override void Initialize()
        {
        }

        TurnResult IEffectSystem.Apply<T>(EntityUid source, EntityUid target, EntityCoordinates coords, EntityUid? verb, EffectArgSet args)
        {
            var effect = new T();
            EntitySystem.InjectDependencies(effect);
            return effect.Apply(source, target, coords, verb, args);
        }
    }
}