using System;
using System.Runtime.CompilerServices;
using OpenNefia.XamlNameGenerator.Compiler;
using OpenNefia.XamlNameGenerator.Domain;
using OpenNefia.XamlNameGenerator.Generator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Diagnostics;

[assembly: InternalsVisibleTo("OpenNefia.XamlNameGenerator.Tests")]

namespace OpenNefia.XamlNameGenerator;

/// <summary>
/// All code is derived from https://github.com/AvaloniaUI/Avalonia.NameGenerator/commit/b16a6ba80b90833de88a1f8c4dc97eceb24e05ff.
/// </summary>
[Generator]
public class OpenNefiaNameSourceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context) { }

    public void Execute(GeneratorExecutionContext context)
    {
        try
        {
            var generator = CreateNameGenerator(context);
            var partials = generator.GenerateNameReferences(context.AdditionalFiles);
            foreach (var partial in partials) context.AddSource(partial.FileName, partial.Content);
        }
        catch (Exception exception)
        {
            ReportUnhandledError(context, exception);
        }
    }

    private static INameGenerator CreateNameGenerator(GeneratorExecutionContext context)
    {
        var options = new GeneratorOptions(context);
        var types = new RoslynTypeSystem((CSharpCompilation)context.Compilation);
        var defaultAccess = options.OpenNefiaNameGeneratorDefaultAccessLevel.ToString().ToLowerInvariant();
        ICodeGenerator generator = options.OpenNefiaNameGeneratorBehavior switch {
            Behavior.OnlyProperties => new OnlyPropertiesCodeGenerator(),
            Behavior.InitializeComponent => new InitializeComponentCodeGenerator(types),
            _ => throw new ArgumentOutOfRangeException()
        };

        var compiler = MiniCompiler.CreateDefault(types, MiniCompiler.OpenNefiaXmlnsDefinitionAttribute);
        return new OpenNefiaNameGenerator(
            new GlobPatternGroup(options.OpenNefiaNameGeneratorFilterByPath),
            new GlobPatternGroup(options.OpenNefiaNameGeneratorFilterByNamespace),
            new XamlXViewResolver(types, compiler, true, type => ReportInvalidType(context, type)),
            new XamlXNameResolver(defaultAccess),
            generator);
    }

    private static void ReportUnhandledError(GeneratorExecutionContext context, Exception error)
    {
        const string message =
            "Unhandled exception occured while generating typed Name references. " +
            "Please file an issue: https://github.com/avaloniaui/avalonia.namegenerator";
        context.ReportDiagnostic(
            Diagnostic.Create(
                new DiagnosticDescriptor(
                    "AXN0002",
                    message,
                    error.ToString(),
                    "Usage",
                    DiagnosticSeverity.Error,
                    true),
                Location.None));
    }

    private static void ReportInvalidType(GeneratorExecutionContext context, string typeName)
    {
        var message =
            $"OpenNefia x:Name generator was unable to generate names for type '{typeName}'. " +
            $"The type '{typeName}' does not exist in the assembly.";
        context.ReportDiagnostic(
            Diagnostic.Create(
                new DiagnosticDescriptor(
                    "AXN0001",
                    message,
                    message,
                    "Usage",
                    DiagnosticSeverity.Error,
                    true),
                Location.None));
    }
}