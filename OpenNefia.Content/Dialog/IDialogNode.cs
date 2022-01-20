using OpenNefia.Content.GameObjects;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
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
        DialogMessage GetResult(EntityUid entity);
    }

    public abstract class DialogNode : IDialogNode
    {
        [Dependency] protected readonly IEntityManager _entMan = default!;

        public DialogNode()
        {
            EntitySystem.InjectDependencies(this);
        }

        public abstract DialogMessage GetResult(EntityUid entity);
    }

    public sealed class DefaultDialog : DialogNode
    {
        public override DialogMessage GetResult(EntityUid entity)
        {
            if (!_entMan.TryGetComponent(entity, out ToneComponent tone))
                return new DialogMessage.Complete();

            return new DialogMessage();
        }
    }
}
