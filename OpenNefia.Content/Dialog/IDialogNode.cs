using OpenNefia.Content.GameObjects;
using OpenNefia.Content.GameObjects.EntitySystems;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Dialog
{
    [ImplicitDataDefinitionForInheritors]
    public interface IDialogNode
    {
        public string LocKey { get; set; }
        IEnumerable<DialogMessage> GetResults(EntityUid entity);
    }

    public interface IDialogBranchNode
    {
        public DialogChoice? SelectedChoice { get; set; }
    }

    public abstract class DialogNode : IDialogNode
    {
        [Dependency] protected readonly IEntityManager _entMan = default!;


        [DataField]
        public string LocKey { get; set; } = default!;

        public abstract IEnumerable<DialogMessage> GetResults(EntityUid entity);
    }

    public sealed class EmptyDialog : DialogNode
    {
        public EmptyDialog(string locKey)
        {
            LocKey = locKey;
        }

        public override IEnumerable<DialogMessage> GetResults(EntityUid entity)
        {
            yield return new DialogMessage.Complete();
        }
    }

    public sealed class CloseDialog : DialogNode
    {
        public override IEnumerable<DialogMessage> GetResults(EntityUid entity)
        {
            yield return new DialogMessage.Complete();
        }
    }

    public sealed class ReturnToRoot : DialogNode
    {
        public override IEnumerable<DialogMessage> GetResults(EntityUid entity)
        {
            yield return new DialogMessage.Return();
        }
    }

    public sealed class MoreDialog : DialogNode
    {
        public override IEnumerable<DialogMessage> GetResults(EntityUid entity)
        {
            yield return new DialogMessage.Continue();
        }
    }

    public sealed class DialogBranch : DialogNode, IDialogBranchNode
    {
        [DataField(required: true)]
        public List<DialogChoice> Choices { get; set; } = new();

        public DialogChoice? SelectedChoice { get; set; }

        public override IEnumerable<DialogMessage> GetResults(EntityUid entity)
        {
            foreach (var choice in Choices)
            {
                choice.Flags = DialogChoiceFlag.Branch;
            }

            yield return new DialogMessage.DialogChoices(Loc.GetString(LocKey), Choices.OrderByDescending(x => x.Priority), this);

            if (SelectedChoice == null || SelectedChoice.Node == null)
                yield break;

            foreach(var node in SelectedChoice.Node.GetResults(entity))
            {
                yield return node;
            }
        }
    }

    public sealed class SpeakerSwap : DialogNode
    {
        [DataField(required: true)]
        public string NewSpeaker { get; } = default!;
        public override IEnumerable<DialogMessage> GetResults(EntityUid entity)
        {
            yield return new DialogMessage.SpeakerSwap(NewSpeaker);
        }
    }

    public sealed class DefaultDialog : DialogNode
    {
        public override IEnumerable<DialogMessage> GetResults(EntityUid entity)
        {
            EntitySystem.InjectDependencies(this);

            if (!_entMan.TryGetComponent(entity, out ToneComponent tone))
                yield return new DialogMessage.Complete();

            yield return new DialogMessage.DialogSingle("asdasd");
        }
    }

    public sealed class NodeSequence : DialogNode
    {
        public override IEnumerable<DialogMessage> GetResults(EntityUid entity)
        {
            foreach(var node in Nodes)
                foreach (var res in node.GetResults(entity))
                    yield return res;
        }

        [DataField(required: true)]
        public List<IDialogNode> Nodes { get; } = default!;
    }

    public sealed class DialogSequence : DialogNode
    {
        public override IEnumerable<DialogMessage> GetResults(EntityUid entity)
        {
            var luaMan = IoCManager.Resolve<ILocalizationManager>();
            if (!luaMan.TryGetTable(LocKey, out var table))
            {
                yield return new DialogMessage.Cancelled();
                yield break;
            }

            foreach(KeyValuePair<object, object> item in table)
            {
                switch(item.Value)
                {
                    case string str:
                        yield return new DialogMessage.DialogText(str);
                        break;
                }
            }
        }
    }

    public sealed class DialogText : DialogNode
    {
        public override IEnumerable<DialogMessage> GetResults(EntityUid entity)
        {
            yield return new DialogMessage.DialogText(Loc.GetString(LocKey ?? ""));
        }
    }

    public sealed class PrototypeDialog : DialogNode
    {
        public override IEnumerable<DialogMessage> GetResults(EntityUid entity)
        {
            var proto = Id.ResolvePrototype();
            return proto.Node!.GetResults(entity);
        }

        [DataField(required: true)]
        public PrototypeId<DialogNodePrototype> Id { get; } = default!;
    }
}
