using OpenNefia.Core.Audio;
using OpenNefia.Core.IoC;
using OpenNefia.Content.Prototypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Content.UI.Hud;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.Input;
using OpenNefia.Content.Input;

namespace OpenNefia.Content.Journal
{
    public class BacklogUiLayer : LogGroupUiLayer
    {
        [Dependency] private readonly IHudLayer _hud = default!;

        public override void OnQuery()
        {
            _hud.Backlog.ToggleBacklog(visible: true);
            Sounds.Play(Protos.Sound.Log);
        }

        public override void OnQueryFinish()
        {
            _hud.Backlog.ToggleBacklog(visible: false);
        }

        protected override void OnKeyDown(GUIBoundKeyEventArgs args)
        {
            base.OnKeyDown(args);
            if (args.Function == ContentKeyFunctions.Backlog)
                Cancel();
        }
    }
}
