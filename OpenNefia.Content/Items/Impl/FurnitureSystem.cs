using OpenNefia.Content.EntityGen;
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

namespace OpenNefia.Content.Items
{
    public interface IFurnitureSystem : IEntitySystem
    {
    }

    public sealed class FurnitureSystem : EntitySystem, IFurnitureSystem
    {
        [Dependency] private readonly IRandom _rand = default!;

        public override void Initialize()
        {
            SubscribeComponent<FurnitureComponent, EntityBeingGeneratedEvent>(HandleBeingGenerated);
        }

        private void HandleBeingGenerated(EntityUid uid, FurnitureComponent component, ref EntityBeingGeneratedEvent args)
        {
            if (component.FurnitureQuality == 0 && _rand.OneIn(3))
                component.FurnitureQuality = _rand.Next(_rand.Next(12) + 1);
        }
    }
}