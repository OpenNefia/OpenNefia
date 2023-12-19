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
using OpenNefia.Content.Maps;
using OpenNefia.Core.Audio;

namespace OpenNefia.Content.Effects.New.Unique
{
    public sealed class VanillaEffectLogicSystem : EntitySystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly IMapTilesetSystem _mapTilesets = default!;
        [Dependency] private readonly IAudioManager _audio = default!;

        public override void Initialize()
        {
            SubscribeComponent<EffectWallCreationComponent, ApplyEffectDamageEvent>(Apply_WallCreation);
        }

        private void Apply_WallCreation(EntityUid uid, EffectWallCreationComponent component, ApplyEffectDamageEvent args)
        {
            if (args.Handled)
                return;

            var map = args.SourceMap;

            if (!_mapTilesets.TryGetTile(Protos.Tile.MapgenWall, map, out var tileID)
                || !map.IsInBounds(args.TargetCoordsMap)
                || !map.CanAccess(args.TargetCoordsMap)
                || map.GetTileID(args.TargetCoordsMap) == tileID)
            {
                _mes.Display(Loc.GetString("Elona.Common.NothingHappens"));
                args.Args.Ensure<EffectCommonArgs>().OutEffectWasObvious = false;
                args.Handle(TurnResult.Failed);
                return;
            }

            _mes.Display(Loc.GetString("Elona.Effect.Spell.WallCreation.WallAppears"));
            _audio.Play(Protos.Sound.Offer1, args.TargetCoordsMap);

            map.SetTile(args.TargetCoordsMap, tileID.Value);
            map.MemorizeTile(args.TargetCoordsMap);

            args.Handle(TurnResult.Succeeded);
        }
    }
}