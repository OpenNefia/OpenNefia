using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Levels;
using OpenNefia.Content.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Log;
using System.Diagnostics.CodeAnalysis;
using OpenNefia.Core.Random;
using OpenNefia.Core.Directions;

namespace OpenNefia.Content.Nefia
{
    public sealed class BaseNefiaTypesGeneratorSystem : EntitySystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IRandom _rand = default!;

        public override void Initialize()
        {
            SubscribeLocalEvent<GenerateNefiaFloorParamsEvent>(SetupBaseParams, nameof(SetupBaseParams));
            SubscribeLocalEvent<NefiaGenTypeComponent, GenerateNefiaFloorAttemptEvent>(GenerateFloor, nameof(GenerateFloor));
        }

        private void SetupBaseParams(GenerateNefiaFloorParamsEvent ev)
        {
            var baseParams = ev.BaseParams;
            var (width, height) = baseParams.MapSize;
            baseParams.MapSize = new(width, height);
            baseParams.RoomCount = (width ^ 2) / 70;
            baseParams.TunnelLength = width * height;
            baseParams.MaxCharaCount = (width * height) / 2;

            var areaNefia = EntityManager.GetComponent<AreaNefiaComponent>(ev.Area.AreaEntityUid);
            baseParams.DangerLevel = AreaNefiaSystem.NefiaFloorNumberToLevel(ev.FloorNumber, areaNefia.BaseLevel);
        }

        private void GenerateFloor(EntityUid uid, NefiaGenTypeComponent component, GenerateNefiaFloorAttemptEvent args)
        {
            var map = component.Generator.Generate(args.Area, args.MapId, args.GenerationAttempt, args.FloorNumber, args.Data);

            if (map != null)
            {
                args.Handle();
            }
        }
    }
}
