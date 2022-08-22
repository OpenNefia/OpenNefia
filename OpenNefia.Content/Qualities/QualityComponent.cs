using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Stats;

namespace OpenNefia.Content.Qualities
{
    [RegisterComponent]
    public class QualityComponent : Component
    {
        public override string Name => "Quality";

        [DataField(required: true)]
        public Stat<Quality> Quality { get; set; } = new(Qualities.Quality.Bad);
    }

    public enum Quality
    {
        Bad = 1,
        Normal = 2,
        Good = 3,
        Great = 4,
        God = 5,
        Unique = 6
    }

    public static class QualityExtensions
    {
        public static string GetLocalizedName(this Quality quality)
        {
            return Loc.GetString("Elona.Quality.Names." + quality.ToString());
        }
    }
}
