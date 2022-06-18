using OpenNefia.Core.UserInterface;
using System.Collections.Generic;
using XamlX.Ast;

namespace OpenNefia.XamlNameGenerator.Domain;

internal interface INameResolver
{
    IReadOnlyList<ResolvedName> ResolveNames(XamlDocument xaml);
}

internal record ResolvedName(string TypeName, string Name, AccessLevel Access);