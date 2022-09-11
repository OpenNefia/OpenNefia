using OpenNefia.Core.IoC;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Content.Sidequests;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;

namespace OpenNefia.Content.Dialog
{
    [ImplicitDataDefinitionForInheritors]
    public interface IDialogAction
    {
        void Invoke(IDialogEngine engine, IDialogNode node);
    }
    
    public sealed class DialogCallbackAction : IDialogAction
    {
        [DataField]
        public DialogActionDelegate Callback { get; } = default!;

        public void Invoke(IDialogEngine engine, IDialogNode node)
        {
            Callback.Invoke(engine, node);
        }
    }

    public sealed class SetSidequestStateDialogAction : IDialogAction
    {
        [Dependency] private readonly ISidequestSystem _sidequests = default!;

        [DataField]
        public PrototypeId<SidequestPrototype> SidequestID { get; }

        [DataField]
        public int State { get; }

        public void Invoke(IDialogEngine engine, IDialogNode node)
        {
            EntitySystem.InjectDependencies(this);
            
            _sidequests.SetState(SidequestID, State);
        }
    }
}
