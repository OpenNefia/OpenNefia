using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.Audio;
using OpenNefia.Core.HotReload;

namespace OpenNefia.LecchoTorte
{
    public sealed class NotifyOnHotReloadSystem : EntitySystem
    {
        [Dependency] private readonly IAudioManager _audio = default!;
        [Dependency] private readonly IHotReloadWatcher _hotReloadWatcher = default!;

        public override void Initialize()
        {
            _hotReloadWatcher.OnUpdateApplication += HandleUpdateApplication;
        }

        private void HandleUpdateApplication(HotReloadEventArgs args)
        {
            _audio.Play(Protos.Sound.Chest1);
        }
    }
}