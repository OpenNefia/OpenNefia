using OpenNefia.Content.DisplayName;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.GameObjects.EntitySystems;
using OpenNefia.Core;
using OpenNefia.Core.Game;
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
        public LocaleKey LocKey { get; set; }
        IEnumerable<IDialogMessage> GetResults(EntityUid entity, DialogContextData context);
    }

    public interface IDialogBranchNode
    {
        public DialogChoice? SelectedChoice { get; set; }
    }

    public abstract class DialogNode : IDialogNode
    {
        [Dependency] protected readonly IEntityManager _entMan = default!;


        [DataField]
        public LocaleKey LocKey { get; set; } = LocaleKey.Empty;

        public abstract IEnumerable<IDialogMessage> GetResults(EntityUid entity, DialogContextData context);
    }

    public sealed class EmptyDialog : DialogNode
    {
        public EmptyDialog(string locKey)
        {
            LocKey = locKey;
        }

        public override IEnumerable<IDialogMessage> GetResults(EntityUid entity, DialogContextData context)
        {
            yield return new DialogCompleteMessage();
        }
    }

    public sealed class CloseDialog : DialogNode
    {
        public override IEnumerable<IDialogMessage> GetResults(EntityUid entity, DialogContextData context)
        {
            yield return new DialogCompleteMessage();
        }
    }

    public sealed class ReturnToRoot : DialogNode
    {
        public override IEnumerable<IDialogMessage> GetResults(EntityUid entity, DialogContextData context)
        {
            yield return new DialogResetMessage();
        }
    }

    public sealed class MoreDialog : DialogNode
    {
        public override IEnumerable<IDialogMessage> GetResults(EntityUid entity, DialogContextData context)
        {
            yield return new DialogContinueMessage();
        }
    }

    public sealed class DialogBranch : DialogNode, IDialogBranchNode
    {
        [DataField(required: true)]
        public List<DialogChoice> Choices { get; set; } = new();

        public DialogChoice? SelectedChoice { get; set; }

        public override IEnumerable<IDialogMessage> GetResults(EntityUid entity, DialogContextData context)
        {
            yield return new DialogChoicesMessage(Loc.GetString(LocKey), Choices.OrderByDescending(x => x.Priority));

            if (SelectedChoice == null || SelectedChoice.Node == null)
                yield break;

            foreach(var node in SelectedChoice.Node.GetResults(entity, context))
            {
                yield return node;
            }
        }
    }

    public sealed class SpeakerSwap : DialogNode
    {
        [Dependency] private readonly IEntityLookup _lookup = default!;

        [DataField("id", required: true)]
        public string ID { get; } = default!;
        public override IEnumerable<IDialogMessage> GetResults(EntityUid entity, DialogContextData context)
        {
            EntitySystem.InjectDependencies(this);
            var speaker = _lookup.EntityQueryInMap<TagComponent>(GameSession.ActiveMap!.Id)
                .FirstOrDefault(x => x.HasTag(new(ID)));

            if (speaker != null)
                yield return new DialogSpeakerSwapMessage(speaker.Owner);
            else
                yield return new DialogCancelMessage($"Unable to find entity with tag {ID}.");
        }
    }

    public sealed class DefaultDialog : DialogNode
    {
        public override IEnumerable<IDialogMessage> GetResults(EntityUid entity, DialogContextData context)
        {
            EntitySystem.InjectDependencies(this);

            if (!_entMan.TryGetComponent(entity, out ToneComponent tone))
                yield return new DialogCancelMessage("No ToneComponent present on entity");

            yield return new DialogSingleMessage("Normal dialog");
        }
    }

    public sealed class NodeSequence : DialogNode
    {
        public override IEnumerable<IDialogMessage> GetResults(EntityUid entity, DialogContextData context)
        {
            foreach(var node in Nodes)
                foreach (var res in node.GetResults(entity, context))
                    yield return res;
        }

        [DataField(required: true)]
        public List<IDialogNode> Nodes { get; } = default!;
    }

    public sealed class DialogSequence : DialogNode
    {
        public override IEnumerable<IDialogMessage> GetResults(EntityUid entity, DialogContextData context)
        {
            var luaMan = IoCManager.Resolve<ILocalizationManager>();
            if (!luaMan.TryGetTable(LocKey, out var table))
            {
                yield return new DialogCancelMessage($"Locale table for {LocKey} not found.");
                yield break;
            }

            foreach(KeyValuePair<object, object> item in table)
            {
                switch(item.Value)
                {
                    case string str:
                        yield return new DialogTextMessage(str);
                        break;
                }
            }
        }
    }

    public sealed class DialogText : DialogNode
    {
        public override IEnumerable<IDialogMessage> GetResults(EntityUid entity, DialogContextData context)
        {
            yield return new DialogTextMessage(Loc.GetString(LocKey));
        }
    }

    public class FormattedText : DialogNode
    {
        [DataField]
        public List<DialogFormatData> FormatData { get; } = new();
        public override IEnumerable<IDialogMessage> GetResults(EntityUid entity, DialogContextData context)
        {
            var res = Loc.GetString(LocKey, FormatData.Select((x, ind) => new LocaleArg($"_{ind}", x.GetFormatText(context))).ToArray());
            yield return new DialogTextMessage(res);
        }
    }

    public sealed class PrototypeDialog : DialogNode
    {
        public override IEnumerable<IDialogMessage> GetResults(EntityUid entity, DialogContextData context)
        {
            var proto = ID.ResolvePrototype();
            return proto.Node!.GetResults(entity, context);
        }

        [DataField("id", required: true)]
        public PrototypeId<DialogNodePrototype> ID { get; } = default!;
    }
}
