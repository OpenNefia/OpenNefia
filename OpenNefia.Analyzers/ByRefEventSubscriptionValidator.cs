﻿using Microsoft.CodeAnalysis;
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

                // Initialize state in the start action.
                var analyzer = new CompilationAnalyzer(byRefEventAttributeType, entitySystemType, entityEventRefHandlerType, componentEventRefHandlerType);

                compilationContext.RegisterOperationAction(analyzer.AnalyzeOperation, OperationKind.Invocation);
            });
        }

        private class CompilationAnalyzer
        {
            private readonly INamedTypeSymbol _byRefEventAttributeType;
            private readonly INamedTypeSymbol _entitySystemType;
            private readonly INamedTypeSymbol _entityEventRefHandlerType;
            private readonly INamedTypeSymbol _componentEventRefHandlerType;

            public CompilationAnalyzer(INamedTypeSymbol byRefEventAttributeType, INamedTypeSymbol entitySystemType, INamedTypeSymbol entityEventRefHandlerType, INamedTypeSymbol componentEventRefHandlerType)
            {
                _byRefEventAttributeType = byRefEventAttributeType;
                _entitySystemType = entitySystemType;
                _entityEventRefHandlerType = entityEventRefHandlerType;
                _componentEventRefHandlerType = componentEventRefHandlerType;
            }

            public void AnalyzeOperation(OperationAnalysisContext context)
            {
                switch (context.Operation.Kind)
                {
                    case OperationKind.Invocation:
                        var invocation = (IInvocationOperation)context.Operation;
                        var method = invocation.TargetMethod;
                        
                        if (method.Name == "SubscribeLocalEvent" && SymbolEqualityComparer.Default.Equals(method.ContainingType, _entitySystemType))
                        {
                            ITypeSymbol eventType;

                            if (method.TypeArguments.Length == 2)
                            {
                                // SubscribeLocalEvent<TComp, TEvent>(...);
                                eventType = method.TypeArguments[1];
                            }
                            else
                            {
                                // SubscribeLocalEvent<TEvent>(...);
                                eventType = method.TypeArguments[0];
                            }

                            var handler = method.Parameters[0];

                            var isEventByRef = eventType.GetAttributes()
                                    .Any(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, _byRefEventAttributeType));
                            
                            var isSubscribedByRef = SymbolEqualityComparer.Default.Equals(handler.Type.OriginalDefinition, _componentEventRefHandlerType) || SymbolEqualityComparer.Default.Equals(handler.Type.OriginalDefinition, _entityEventRefHandlerType);

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