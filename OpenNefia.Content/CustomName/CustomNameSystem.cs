using OpenNefia.Content.DisplayName;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.RandomText;
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

namespace OpenNefia.Content.CustomName
{
    public interface ICustomNameSystem : IEntitySystem
    {
    }

    public sealed class CustomNameSystem : EntitySystem, ICustomNameSystem
    {
        [Dependency] private readonly IRandomNameGenerator _randomNames = default!;

        public override void Initialize()
        {
            SubscribeComponent<CharaNameGenComponent, EntityBeingGeneratedEvent>(GenerateRandomName, priority: EventPriorities.VeryHigh);
            SubscribeComponent<CustomNameComponent, GetDisplayNameEventArgs>(GetCustomName, priority: EventPriorities.VeryHigh);
        }

        private void GenerateRandomName(EntityUid uid, CharaNameGenComponent component, ref EntityBeingGeneratedEvent args)
        {
            if (!component.HasRandomName)
                return;

            var customName = EnsureComp<CustomNameComponent>(uid);
            customName.CustomName = _randomNames.GenerateRandomName();
            customName.ShowBaseName = true;
        }

        private void GetCustomName(EntityUid uid, CustomNameComponent component, ref GetDisplayNameEventArgs args)
        {
            if (component.ShowBaseName)
            {
                // "Arnord the putit"
                args.OutName = Loc.GetString("Elona.DisplayName.WithBaseName", ("baseName", args.BaseName), ("customName", component.CustomName));
            }
            else
            {
                // "Arnord"
                args.OutName = component.CustomName;
            }
        }
    }
}