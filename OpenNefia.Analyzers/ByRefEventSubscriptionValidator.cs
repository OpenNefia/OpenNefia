using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace OpenNefia.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ByRefEventSubscriptionValidator : DiagnosticAnalyzer
    {
        public const string ByRefEventAttributeTypeName = "OpenNefia.Core.GameObjects.ByRefEventAttribute";
        public const string EntitySystemTypeName = "OpenNefia.Core.GameObjects.EntitySystem";
        public const string EntityEventRefHandlerTypeName = "OpenNefia.Core.GameObjects.EntityEventRefHandler`1";
        public const string ComponentEventRefHandlerTypeName = "OpenNefia.Core.GameObjects.ComponentEventRefHandler`2";
        public const string BroadcastEventRefHandlerTypeName = "OpenNefia.Core.GameObjects.BroadcastEventRefHandler`1";

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Diagnostics.InvalidEventSubscribingByValue, Diagnostics.InvalidEventSubscribingByRef);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterCompilationStartAction(compilationContext =>
            {
                var byRefEventAttributeType = compilationContext.Compilation.GetTypeByMetadataName(ByRefEventAttributeTypeName);
                if (byRefEventAttributeType == null)
                    return;

                var entitySystemType = compilationContext.Compilation.GetTypeByMetadataName(EntitySystemTypeName);
                if (entitySystemType == null)
                    return;

                var entityEventRefHandlerType = compilationContext.Compilation.GetTypeByMetadataName(EntityEventRefHandlerTypeName);
                if (entityEventRefHandlerType == null)
                    return;

                var componentEventRefHandlerType = compilationContext.Compilation.GetTypeByMetadataName(ComponentEventRefHandlerTypeName);
                if (componentEventRefHandlerType == null)
                    return;

                var broadcastEventRefHandlerType = compilationContext.Compilation.GetTypeByMetadataName(BroadcastEventRefHandlerTypeName);
                if (broadcastEventRefHandlerType == null)
                    return;

                // Initialize state in the start action.
                var analyzer = new CompilationAnalyzer(byRefEventAttributeType, entitySystemType, entityEventRefHandlerType, componentEventRefHandlerType, broadcastEventRefHandlerType);

                compilationContext.RegisterOperationAction(analyzer.AnalyzeOperation, OperationKind.Invocation);
            });
        }

        private class CompilationAnalyzer
        {
            private readonly INamedTypeSymbol _byRefEventAttributeType;
            private readonly INamedTypeSymbol _entitySystemType;
            private readonly INamedTypeSymbol _entityEventRefHandlerType;
            private readonly INamedTypeSymbol _componentEventRefHandlerType;
            private readonly INamedTypeSymbol _broadcastEventRefHandlerType;

            private static HashSet<string> SubMethodNames = new HashSet<string>()
            {
                "SubscribeComponent",
                "SubscribeEntity",
                "SubscribeBroadcast",
            };

            public CompilationAnalyzer(INamedTypeSymbol byRefEventAttributeType, INamedTypeSymbol entitySystemType, INamedTypeSymbol entityEventRefHandlerType, INamedTypeSymbol componentEventRefHandlerType, INamedTypeSymbol broadcastEventRefHandlerType)
            {
                _byRefEventAttributeType = byRefEventAttributeType;
                _entitySystemType = entitySystemType;
                _entityEventRefHandlerType = entityEventRefHandlerType;
                _componentEventRefHandlerType = componentEventRefHandlerType;
                _broadcastEventRefHandlerType = broadcastEventRefHandlerType;
            }

            public void AnalyzeOperation(OperationAnalysisContext context)
            {
                switch (context.Operation.Kind)
                {
                    case OperationKind.Invocation:
                        var invocation = (IInvocationOperation)context.Operation;
                        var method = invocation.TargetMethod;

                        if (SubMethodNames.Contains(method.Name) && SymbolEqualityComparer.Default.Equals(method.ContainingType, _entitySystemType))
                        {
                            ITypeSymbol eventType;

                            if (method.TypeArguments.Length == 2)
                            {
                                // SubscribeComponent<TComp, TEvent>(...);
                                eventType = method.TypeArguments[1];
                            }
                            else
                            {
                                // SubscribeEntity<TEvent>(...);
                                // SubscribeBroadcast<TEvent>(...);
                                eventType = method.TypeArguments[0];
                            }

                            // Ignore generic type parameters; only care about calls that specify a
                            // concrete class/struct for the event type.
                            if (eventType.TypeKind != TypeKind.Class && eventType.TypeKind != TypeKind.Struct)
                                return;

                            var handler = method.Parameters[0];

                            var isEventByRef = eventType.GetAttributes()
                                    .Any(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, _byRefEventAttributeType));

                            var isSubscribedByRef = SymbolEqualityComparer.Default.Equals(handler.Type.OriginalDefinition, _componentEventRefHandlerType) || SymbolEqualityComparer.Default.Equals(handler.Type.OriginalDefinition, _entityEventRefHandlerType) || SymbolEqualityComparer.Default.Equals(handler.Type.OriginalDefinition, _broadcastEventRefHandlerType);

                            if (isEventByRef != isSubscribedByRef)
                            {
                                if (isEventByRef)
                                {
                                    context.ReportDiagnostic(
                                        Diagnostic.Create(
                                            Diagnostics.InvalidEventSubscribingByValue,
                                            invocation.Syntax.GetLocation(),
                                            eventType.Name));
                                }
                                else
                                {
                                    context.ReportDiagnostic(
                                        Diagnostic.Create(
                                            Diagnostics.InvalidEventSubscribingByRef,
                                            invocation.Syntax.GetLocation(),
                                            eventType.Name));
                                }
                            }
                        }

                        break;
                }
            }
        }
    }
}