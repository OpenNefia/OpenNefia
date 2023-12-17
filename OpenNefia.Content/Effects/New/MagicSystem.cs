using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Effects.New
{
    /// <summary>
    /// Wraps <see cref="INewEffectSystem"/> with MP checks and casting style logic.
    /// </summary>
    public interface IMagicSystem : IEntitySystem
    {
        TurnResult Cast(EntityUid source, EntityUid? target, PrototypeId<EntityPrototype> effectID, EffectArgSet args);
    }

    public sealed class MagicSystem : EntitySystem, IMagicSystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;

        public TurnResult Cast(EntityUid source, EntityUid? target, PrototypeId<EntityPrototype> effectID, EffectArgSet args)
        {
            return TurnResult.Failed;
        }

        public override void Initialize()
        {
        }
    }
}