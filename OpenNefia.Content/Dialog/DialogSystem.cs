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

namespace OpenNefia.Content.Dialog
{
    public interface IDialogSystem : IEntitySystem
    {
    }

    public sealed class DialogSystem : EntitySystem, IDialogSystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;

        public override void Initialize()
        {
            SubscribeComponent<DialogComponent, EntityBeingGeneratedEvent>(InitializePersonality);
        }

        private void InitializePersonality(EntityUid uid, DialogComponent component, ref EntityBeingGeneratedEvent args)
        {
            // TODO: replace with custom talk
            component.Personality = _rand.Next(4);
        }
    }
}