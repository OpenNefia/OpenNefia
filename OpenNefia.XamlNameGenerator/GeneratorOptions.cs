using System;
using Microsoft.CodeAnalysis;
using OpenNefia.Core.UserInterface;

namespace OpenNefia.XamlNameGenerator;

public enum BuildProperties
{
    OpenNefiaNameGeneratorBehavior = 0,
    OpenNefiaNameGeneratorDefaultAccessLevel = 1,
    OpenNefiaNameGeneratorFilterByPath = 2,
    OpenNefiaNameGeneratorFilterByNamespace = 3,
    OpenNefiaNameGeneratorDebuggerLaunch = 4,
}

public enum DefaultFieldModifier
{
    Public = 0,
    Private = 1,
    Internal = 2,
    Protected = 3,
}

public enum Behavior
{
    OnlyProperties = 0,
    InitializeComponent = 1,
}

public class GeneratorOptions
{
    private readonly GeneratorExecutionContext _context;

    public GeneratorOptions(GeneratorExecutionContext context) => _context = context;

    public Behavior OpenNefiaNameGeneratorBehavior
    {
        get
        {
            const Behavior defaultBehavior = Behavior.OnlyProperties;
            var propertyValue = _context
                .GetMsBuildProperty(
                    nameof(BuildProperties.OpenNefiaNameGeneratorBehavior),
                    defaultBehavior.ToString());

            if (!Enum.TryParse(propertyValue, true, out Behavior behavior))
                return defaultBehavior;
            return behavior;
        }
    }

    public AccessLevel OpenNefiaNameGeneratorDefaultAccessLevel
    {
        get
        {
            const AccessLevel defaultFieldModifier = AccessLevel.Internal;
            var propertyValue = _context
                .GetMsBuildProperty(
                    nameof(BuildProperties.OpenNefiaNameGeneratorDefaultAccessLevel),
                    defaultFieldModifier.ToString());

            if (!Enum.TryParse(propertyValue, true, out AccessLevel modifier))
                return defaultFieldModifier;
            return modifier;
        }
    }

    public string[] OpenNefiaNameGeneratorFilterByPath
    {
        get
        {
            var propertyValue = _context.GetMsBuildProperty(
                nameof(BuildProperties.OpenNefiaNameGeneratorFilterByPath),
                "*");

            if (propertyValue.Contains(";"))
                return propertyValue.Split(';');
            return new[] {propertyValue};
        }
    }

    public string[] OpenNefiaNameGeneratorFilterByNamespace
    {
        get
        {
            var propertyValue = _context.GetMsBuildProperty(
                nameof(BuildProperties.OpenNefiaNameGeneratorFilterByNamespace),
                "*");

            if (propertyValue.Contains(";"))
                return propertyValue.Split(';');
            return new[] {propertyValue};
        }
    }

    public bool OpenNefiaNameGeneratorDebuggerLaunch
    {
        get
        {
            var propertyValue = _context.GetMsBuildProperty(
                nameof(BuildProperties.OpenNefiaNameGeneratorDebuggerLaunch),
                "false");

            return propertyValue == "true";
        }
    }
}