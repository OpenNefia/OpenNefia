using Microsoft.CodeAnalysis;

namespace OpenNefia.Analyzers
{
    public static class Diagnostics
    {
        public static SuppressionDescriptor MeansImplicitAssignment =>
            new SuppressionDescriptor("ONAS1000", "CS0649", "Marked as implicitly assigned.");

        public const string DiagnosticCategory = "OpenNefia";

        public static DiagnosticDescriptor InvalidEventSubscribingByValue =
            new DiagnosticDescriptor("ONAD1000", "Mismatch on by-refness for entity system event subscription",
                "Type '{0}' is annotated as [ByRefEvent], but it was subscribed to by value",
                DiagnosticCategory,
                DiagnosticSeverity.Error,
                isEnabledByDefault: true,
                description: "Events must be subscribed to with matching by-refness.");

        public static DiagnosticDescriptor InvalidEventSubscribingByRef =
            new DiagnosticDescriptor("ONAD1001", "Mismatch on by-refness for entity system event subscription",
                "Type '{0}' is not annotated as [ByRefEvent], but it was subscribed to by ref",
                DiagnosticCategory,
                DiagnosticSeverity.Error,
                isEnabledByDefault: true,
                description: "Events must be subscribed to with matching by-refness.");
    }
}
