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

namespace OpenNefia.Content.Journal
{
    public class BacklogUiLayer : JournalUiLayer
    {
        [Dependency] private readonly IHudLayer _hud = default!;

        public override void TabEnter()
        {
            base.TabEnter();
            _hud.ToggleBacklog(true);
            Sounds.Play(Protos.Sound.Log);
        }

        public override void TabExit()
        {
            base.TabExit();
            _hud.ToggleBacklog(false);
        }

        protected override void OnKeyDown(GUIBoundKeyEventArgs args)
        {
            base.OnKeyDown(args);
            if (args.Function == EngineKeyFunctions.UIBacklog)
                Cancel();
        }
    }
}
