using OpenNefia.Content.GameObjects;
using OpenNefia.Core.Audio;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Content.Prototypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Content.Logic;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Content.UI.Layer;
using OpenNefia.Content.Levels;
using OpenNefia.Content.EntityGen;

namespace OpenNefia.Content.Pot
{
    public sealed class PotSystem : EntitySystem
    {
        [Dependency] private readonly IMapManager _mapMan = default!;
        [Dependency] private readonly IAudioManager _audio = default!;
        [Dependency] private readonly IVisibilitySystem _visibliity = default!;
        [Dependency] private readonly IMessage _mes = default!;
        [Dependency] private readonly IFieldLayer _field = default!;
        [Dependency] private readonly ILevelSystem _levels = default!;
        [Dependency] private readonly IEntityGen _entityGen = default!;

        public override void Initialize()
        {
            SubscribeLocalEvent<PotComponent, WasCollidedWithEventArgs>(HandleCollidedWith, nameof(HandleCollidedWith));
        }

        private void HandleCollidedWith(EntityUid uid, PotComponent component, WasCollidedWithEventArgs args)
        {
            var spatial = EntityManager.GetComponent<SpatialComponent>(uid);
            var map = _mapMan.GetMap(spatial.MapID);

            var chip = EntityManager.GetComponent<ChipComponent>(uid);
            chip.ChipID = Protos.Chip.Default;

            // TODO
            var level = _levels.GetLevel(map.MapEntityUid);
            _entityGen.SpawnEntity(Protos.Item.SunCrystal, spatial.MapPosition);

            map.MemorizeTile(spatial.WorldPosition);
            _field.RefreshScreen();

            // TODO
            if (_visibliity.IsInWindowFov(args.Source))
            {
                _audio.Play(Protos.Sound.Bash1, uid);
                _mes.Display(Loc.GetString("Elona.Pot.Shatters", ("basher", args.Source)));
                _audio.Play(Protos.Sound.Crush1, uid);
            }

            EntityManager.DeleteEntity(uid);
        }
    }
}
