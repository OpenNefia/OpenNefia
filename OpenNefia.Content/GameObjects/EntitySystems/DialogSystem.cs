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
        GetDialogOptionsEventArgs HandleTalk(EntityUid target);
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

            SubscribeLocalEvent<DialogComponent, GetDialogOptionsEventArgs>(AddDefaultDialog, nameof(AddDefaultDialog));
        }

        private void AddDefaultDialog(EntityUid uid, DialogComponent component, GetDialogOptionsEventArgs args)
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

            var logic = new DialogLogic();
            logic.Initialize(component.Owner);
        }

        public GetDialogOptionsEventArgs HandleTalk(EntityUid target)
        {
            var diaArgs = new GetDialogOptionsEventArgs();
            RaiseLocalEvent(target, diaArgs);

            return diaArgs;
        }
    }

    public class GetDialogOptionsEventArgs : EntityEventArgs
    {
        public DialogLayer Layer { get; set; } = default!;
        public List<DialogChoice> Choices = new();
    }
}
