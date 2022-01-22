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

    [Flags]
    public enum DialogChoiceFlag
    {
        None            = 0,
        Branch          = 1 << 0,
        More            = 1 << 1,
    }


    [DataDefinition]
    public class DialogChoice
    {
        [DataField]
        public string LocKey { get; set; }

        [DataField]
        public int Priority { get; set; }

        [DataField(required: true)]
        public IDialogNode? Node { get; set; }

        [DataField]
        public string ExtraText { get; set; }

        public DialogChoiceFlag Flags { get; set; }

        public DialogChoice() 
        {
            LocKey = string.Empty;
            Node = null;
            ExtraText = string.Empty;
        }
        public DialogChoice(int priority = 0, PrototypeId<DialogNodePrototype>? id = null, string? locKey = null, string extraText = "", DialogChoiceFlag flags = default)
        {
            var proto = id?.ResolvePrototype();
            Priority = priority;
            if (proto != null)
            {
                Node = proto.Node;
                if (Node.LocKey == null)
                {
                    Node.LocKey = proto.LocKey;
                }
                if (locKey == null)
                {
                    LocKey = proto.LocKey;
                }
            }

            if (locKey != null)
            {
                LocKey = locKey;
            }
            LocKey ??= "";

            ExtraText = extraText;
            Flags = flags;
        }
    }

    public class GetDialogueOptionsEventArgs : EntityEventArgs
    {
        public DialogLayer Layer { get; set; } = default!;
        public List<DialogChoice> Choices = new();
    }
}
