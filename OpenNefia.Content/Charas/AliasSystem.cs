using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Qualities;
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
using OpenNefia.Content.RandomText;

namespace OpenNefia.Content.Charas
{
    public interface IAliasSystem : IEntitySystem
    {
    }

    public sealed class AliasSystem : EntitySystem, IAliasSystem
    {
        [Dependency] private readonly IRandomAliasGenerator _randomAliases = default!;

        public override void Initialize()
        {
            SubscribeComponent<QualityComponent, EntityBeingGeneratedEvent>(Quality_AddAliasBasedOnQuality, priority: EventPriorities.VeryLow);
        }

        private void Quality_AddAliasBasedOnQuality(EntityUid uid, QualityComponent component, ref EntityBeingGeneratedEvent args)
        {
            if (component.Quality != Quality.Great && component.Quality != Quality.God)
                return;

            var alias = EnsureComp<AliasComponent>(uid);
            alias.Alias = _randomAliases.GenerateRandomAlias(AliasType.Item);
        }
    }
}