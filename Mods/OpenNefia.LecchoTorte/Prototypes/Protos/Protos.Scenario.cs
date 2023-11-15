using ScenarioPrototypeId = OpenNefia.Core.Prototypes.PrototypeId<OpenNefia.Content.Scenarios.ScenarioPrototype>;

namespace OpenNefia.LecchoTorte.Prototypes
{
    public static partial class Protos
    {
        public static class Scenario
        {
            #pragma warning disable format

            public static readonly ScenarioPrototypeId Quickstart = new($"LecchoTorte.{nameof(Quickstart)}");

            #pragma warning restore format
        }
    }
}
