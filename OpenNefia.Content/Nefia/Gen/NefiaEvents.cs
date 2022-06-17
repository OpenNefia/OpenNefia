using OpenNefia.Content.Maps;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Utility;
using OpenNefia.Content.Prototypes;

namespace OpenNefia.Content.Nefia
{
    public class NefiaGenParams {}

    /// <summary>
    /// Parameters general to all nefia geneation algorithms.
    /// </summary>
    public sealed class BaseNefiaGenParams : NefiaGenParams
    {
        public BaseNefiaGenParams() : this(10, 10) {}

        public BaseNefiaGenParams(int width, int height)
        {
            MapSize = Vector2i.ComponentMax(new(width, height), Vector2i.One);
        }

        public Vector2i MapSize { get; set; } = Vector2i.One;
        public int DangerLevel { get; set; } = 1;
        public int RoomCount { get; set; } = 1;
        public int MinRoomSize { get; set; } = 3;
        public int MaxRoomSize { get; set; } = 4;
        public int ExtraRoomCount { get; set; } = 10;
        public int RoomEntranceCount { get; set; } = 1;
        public int TunnelLength { get; set; } = 1;
        public float HiddenPathChance { get; set; } = 0.05f;
        public int CreaturePacks { get; set; } = 1;
        public bool CanHaveMultipleMonsterHouses { get; set; } = false;
        public int MaxCharaCount { get; set; } = 1;
    }

    /// <summary>
    /// Parameters only used by <see cref="VanillaNefiaGenSystem"/>.
    /// </summary>
    public sealed class StandardNefiaGenParams : NefiaGenParams
    {
        public StandardNefiaGenParams(IVanillaNefiaTemplate template, IVanillaNefiaLayout layout)
        {
            Template = template;
            Layout = layout;
        }

        public IVanillaNefiaTemplate Template { get; set; }
        public IVanillaNefiaLayout Layout { get; set; }
    }

    internal class GenerateNefiaFloorParamsEvent
    {
        public IArea Area { get; }
        public MapId MapId { get; }
        public int GenerationAttempt { get; }
        public int FloorNumber { get; }
        public Blackboard<NefiaGenParams> Data { get; set; }
        public BaseNefiaGenParams BaseParams => Data.Get<BaseNefiaGenParams>();

        public GenerateNefiaFloorParamsEvent(IArea area, MapId mapId, Blackboard<NefiaGenParams> data, int generationAttempt, int floorNumber)
        {
            Area = area;
            MapId = mapId;
            Data = data;
            GenerationAttempt = generationAttempt;
            FloorNumber = floorNumber;
        }
    }

    /// <summary>
    /// Event for attempting to generate a nefia floor. If the attempt succeeds,
    /// the handler should set <c>Handled</c> to true.
    /// </summary>
    public sealed class GenerateNefiaFloorAttemptEvent : HandledEntityEventArgs
    {
        public IArea Area { get; }
        public MapId MapId { get; }
        public int GenerationAttempt { get; }
        public int FloorNumber { get; }
        public Blackboard<NefiaGenParams> Data { get; set; }
        public BaseNefiaGenParams BaseParams => Data.Get<BaseNefiaGenParams>();

        public GenerateNefiaFloorAttemptEvent(IArea area, MapId mapId, Blackboard<NefiaGenParams> data, int generationAttempt, int floorNumber)
        {
            Area = area;
            MapId = mapId;
            Data = data;
            GenerationAttempt = generationAttempt;
            FloorNumber = floorNumber;
        }

        public void Handle()
        {
            Handled = true;
        }
    }

    public sealed class AfterGenerateNefiaFloorEvent : EntityEventArgs
    {
        public IArea Area { get; }
        public IMap Map { get; }
        public int FloorNumber { get; }
        public Blackboard<NefiaGenParams> Data { get; set; }
        public BaseNefiaGenParams BaseParams => Data.Get<BaseNefiaGenParams>();

        public AfterGenerateNefiaFloorEvent(IArea area, IMap map, Blackboard<NefiaGenParams> data, int floorNumber)
        {
            Area = area;
            Map = map;
            Data = data;
            FloorNumber = floorNumber;
        }
    }
}
