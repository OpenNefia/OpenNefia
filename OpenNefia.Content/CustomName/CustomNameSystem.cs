using OpenNefia.Content.DisplayName;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.GameObjects.Components;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.RandomText;
using OpenNefia.Content.UI.Layer;
using OpenNefia.Content.UI;
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
using OpenNefia.Core.UserInterface;

namespace OpenNefia.Content.CustomName
{
    public interface ICustomNameSystem : IEntitySystem
    {
        void PromptForNewName(EntityUid target);
    }

    public sealed class CustomNameSystem : EntitySystem, ICustomNameSystem
    {
        [Dependency] private readonly IRandomNameGenerator _randomNames = default!;
        [Dependency] private readonly IUserInterfaceManager _uiManager = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;

        public override void Initialize()
        {
            SubscribeComponent<CharaNameGenComponent, EntityBeingGeneratedEvent>(GenerateRandomName, priority: EventPriorities.VeryHigh);
            SubscribeComponent<CustomNameComponent, GetDisplayNameEventArgs>(GetCustomName, priority: EventPriorities.VeryHigh);
            SubscribeComponent<CustomNameComponent, GetBaseNameEventArgs>(GetCustomBaseName, priority: EventPriorities.VeryHigh);
        }

        public void PromptForNewName(EntityUid target)
        {
            var args = new TextPrompt.Args(12, isCancellable: true, queryText: Loc.GetString("Elona.CustomName.Interact.ChangeName.Prompt", ("entity", target)));
            var result = _uiManager.Query<TextPrompt, TextPrompt.Args, TextPrompt.Result>(args);

            if (!result.HasValue || string.IsNullOrWhiteSpace(result.Value!.Text))
            {
                _mes.Display(Loc.GetString("Elona.CustomName.Interact.ChangeName.Cancel"));
                return;
            }

            var newName = result.Value.Text;

            EnsureComp<CustomNameComponent>(target).CustomName = newName;
            _mes.Display(Loc.GetString("Elona.CustomName.Interact.ChangeName.YouNamed",
                ("entity", target),
                ("newName", newName)));
        }

        private void GenerateRandomName(EntityUid uid, CharaNameGenComponent component, ref EntityBeingGeneratedEvent args)
        {
            if (!component.HasRandomName)
                return;

            var customName = EnsureComp<CustomNameComponent>(uid);
            customName.CustomName = _randomNames.GenerateRandomName();
            customName.ShowDisplayName = true;
        }

        private void GetCustomName(EntityUid uid, CustomNameComponent component, ref GetDisplayNameEventArgs args)
        {
            if (component.CustomName != null)
            {
                if (component.ShowDisplayName && TryComp<MetaDataComponent>(uid, out var meta) && meta.DisplayName != null)
                {
                    // "Arnord the putit"
                    args.OutName = Loc.GetString("Elona.DisplayName.WithMetaDataName", ("metaDataName", meta.DisplayName), ("customName", component.CustomName));
                }
                else
                {
                    // "Arnord"
                    args.OutName = component.CustomName;
                }
                args.OutAddArticle = false;
            }
        }

        private void GetCustomBaseName(EntityUid uid, CustomNameComponent component, ref GetBaseNameEventArgs args)
        {
            if (component.CustomName != null)
            {
                args.OutBaseName = component.CustomName;
            }
        }
    }
}