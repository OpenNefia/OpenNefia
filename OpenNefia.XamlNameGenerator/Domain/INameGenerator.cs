using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace OpenNefia.XamlNameGenerator.Domain;

internal interface INameGenerator
{
    IReadOnlyList<GeneratedPartialClass> GenerateNameReferences(IEnumerable<AdditionalText> additionalFiles);
}

internal record GeneratedPartialClass(string FileName, string Content);