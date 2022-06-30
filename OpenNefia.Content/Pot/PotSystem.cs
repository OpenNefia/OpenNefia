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
using OpenNefia.Core.Rendering;
using Love;
using OpenNefia.Content.Rendering;
using OpenNefia.Content.RandomGen;
using OpenNefia.Content.Qualities;
using OpenNefia.Core.Random;
using OpenNefia.Content.Visibility;

namespace OpenNefia.Content.Pot
{
    public sealed class PotSystem : EntitySystem
    {
        [Dependency] private readonly IMapManager _mapMan = default!;
        [Dependency] private readonly IAudioManager _audio = default!;
        [Dependency] private readonly IVisibilitySystem _visibliity = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IFieldLayer _field = default!;
        [Dependency] private readonly IItemGen _itemGen = default!;
        [Dependency] private readonly IRandomGenSystem _randomGen = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly ILevelSystem _levels = default!;
        [Dependency] private readonly IMapDrawables _mapDrawables = default!;

        public override void Initialize()
        {
            SubscribeComponent<PotComponent, WasCollidedWithEventArgs>(HandleCollidedWith);
        }

        private void HandleCollidedWith(EntityUid uid, PotComponent component, WasCollidedWithEventArgs args)
        {
            var spatial = EntityManager.GetComponent<SpatialComponent>(uid);
            var map = _mapMan.GetMap(spatial.MapID);

            var chip = EntityManager.GetComponent<ChipComponent>(uid);
            chip.ChipID = Protos.Chip.Default;

            // TODO shelter
            var level = _levels.GetLevel(map.MapEntityUid);

            _itemGen.GenerateItem(spatial.MapPosition, 
                minLevel: level, 
                quality: _randomGen.CalcObjectQuality(Quality.Good),
                tags: new[] { _rand.Pick(RandomGenConsts.FilterSets.Barrel) });

            map.MemorizeTile(spatial.WorldPosition);
            _field.RefreshScreen();
            
            if (_visibliity.IsInWindowFov(args.Source))
            {
                _audio.Play(Protos.Sound.Bash1, uid);
                _mes.Display(Loc.GetString("Elona.Pot.Shatters", ("basher", args.Source)));
                _audio.Play(Protos.Sound.Crush1, uid);
                var drawable = new BreakingFragmentsMapDrawable();
                _mapDrawables.Enqueue(drawable, spatial.MapPosition);
            }

            EntityManager.DeleteEntity(uid);
        }
    }
}
