using OpenNefia.Content.GameObjects.Components;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Stats;

namespace OpenNefia.Content.Qualities
{
    [RegisterComponent]
    public class QualityComponent : Component, IComponentRefreshable
    {
        [DataField]
        public Stat<Quality> Quality { get; set; } = new(Qualities.Quality.Bad);

        public void Refresh()
        {
            Quality.Reset();
        }
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
