using OpenNefia.Content.Dialog;
using OpenNefia.Content.Factions;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.GameObjects.EntitySystems
{
    public interface IDialogSystem : IEntitySystem
    {
        GetDialogueOptionsEventArgs HandleTalk(EntityUid target);
    }
    public class DialogSystem : EntitySystem, IDialogSystem
    {
        [Dependency] private readonly IFactionSystem _factions = default!;
        [Dependency] private readonly IMessage _mes = default!;

        private const int TalkDefaultPrio = 20000;
        private const int TalkByePrio = -20000;

        public override void Initialize()
        {
            SubscribeLocalEvent<DialogComponent, WasCollidedWithEventArgs>(HandleCollision, nameof(HandleCollision));

            SubscribeLocalEvent<DialogComponent, GetDialogueOptionsEventArgs>(AddDefaultDialog, nameof(AddDefaultDialog));
        }

        private void AddDefaultDialog(EntityUid uid, DialogComponent component, GetDialogueOptionsEventArgs args)
        {
            args.Layer = new DefaultDialogLayer();
            args.Choices.Add(new(TalkDefaultPrio, Protos.Dialog.TalkDefault));
            args.Choices.Add(new(TalkByePrio, Protos.Dialog.TalkBye));
        }

        private void HandleCollision(EntityUid uid, DialogComponent component, WasCollidedWithEventArgs args)
        {
            if (args.Handled)
                return;
            var relation = _factions.GetRelationTowards(uid, component.Owner);
            if (relation < Relation.Dislike)
                return;

            var model = new DialogModel();
            model.Inititialize(component.Owner);
        }

        public GetDialogueOptionsEventArgs HandleTalk(EntityUid target)
        {
            var diaArgs = new GetDialogueOptionsEventArgs();
            RaiseLocalEvent(target, diaArgs);

            return diaArgs;
        }
    }

    public class GetDialogueOptionsEventArgs : EntityEventArgs
    {
        public DialogLayer Layer { get; set; } = default!;
        public List<DialogChoice> Choices = new();
    }
}
