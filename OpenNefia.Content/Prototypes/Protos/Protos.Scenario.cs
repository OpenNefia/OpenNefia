using ScenarioPrototypeId = OpenNefia.Core.Prototypes.PrototypeId<OpenNefia.Content.Scenarios.ScenarioPrototype>;

namespace OpenNefia.Content.Prototypes
{
    public static partial class Protos
    {
        public static class Scenario
        {
            #pragma warning disable format

            public static readonly ScenarioPrototypeId Default    = new($"Elona.{nameof(Default)}");

            #pragma warning restore format
        }
    }
}
