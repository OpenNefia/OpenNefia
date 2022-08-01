using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.UserInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Dialog
{
    public sealed partial class DialogSystem
    {
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly IUserInterfaceManager _uiManager = default!;
        [Dependency] private readonly IDynamicTypeFactory _dynTypeFac = default!;

        public TurnResult StartDialog(EntityUid target, PrototypeId<DialogPrototype> dialogID)
        {
            if (!IsAlive(target))
                return TurnResult.Aborted;

            var args = new DialogArgs()
            {
            };
            IDialogLayer dialogLayer = _uiManager.CreateLayer<DialogLayer, DialogArgs, DialogResult>(args);

            var dialogProto = _protos.Index(dialogID);
            var engine = (DialogEngine)_dynTypeFac.CreateInstance(typeof(DialogEngine), new object[] { target, dialogProto, dialogLayer });

            return engine.StartDialog();
        }
    }
}
