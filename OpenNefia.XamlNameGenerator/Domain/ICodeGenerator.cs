using System.Collections.Generic;
using XamlX.TypeSystem;

namespace OpenNefia.XamlNameGenerator.Domain;

internal interface ICodeGenerator
{
    string GenerateCode(string className, IList<string> generics, string nameSpace, IXamlType XamlType, IEnumerable<ResolvedName> names);
}