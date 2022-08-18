using OpenNefia.Content.Maps;
using OpenNefia.Content.RandomGen;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;

namespace OpenNefia.Content.Nefia
{
    public sealed class DungeonCharaFilterGen : IMapCharaFilterGen
    {
        [Dependency] private readonly IRandomGenSystem _randomGen = default!;

        public CharaFilter GenerateFilter(IMap map)
        {
            return new CharaFilter()
            {
                MinLevel = _randomGen.CalcObjectLevel(map),
                Quality = _randomGen.CalcObjectQuality(Qualities.Quality.Normal)
            };
        }
    }
}