using System;
using System.Collections.Generic;
using System.Linq;
using OpenNefia.Core.UserInterface;
using OpenNefia.XamlNameGenerator.Domain;
using XamlX;
using XamlX.Ast;
using XamlX.TypeSystem;

namespace OpenNefia.XamlNameGenerator.Generator;

internal class XamlXNameResolver : INameResolver, IXamlAstVisitor
{
    private readonly List<ResolvedName> _items = new();
    private readonly string _defaultFieldModifier;

    public XamlXNameResolver(string defaultFieldModifier = "internal")
    {
        _defaultFieldModifier = defaultFieldModifier;
    }

    public IReadOnlyList<ResolvedName> ResolveNames(XamlDocument xaml)
    {
        _items.Clear();
        xaml.Root.Visit(this);
        xaml.Root.VisitChildren(this);
        return _items;
    }

    private static bool IsControl(IXamlType type) => type.FullName != "System.Object" &&
                                          (type.FullName == "OpenNefia.Core.UI.Wisp.WispControl" ||
                                           IsControl(type.BaseType));

    IXamlAstNode IXamlAstVisitor.Visit(IXamlAstNode node)
    {
        if (node is not XamlAstObjectNode objectNode)
            return node;

        var clrType = objectNode.Type.GetClrType();
        if (!IsControl(clrType))
            return node;

        foreach (var child in objectNode.Children)
        {
            if (child is XamlAstXamlPropertyValueNode propertyValueNode &&
                propertyValueNode.Property is XamlAstNamePropertyReference namedProperty &&
                namedProperty.Name == "Name" &&
                propertyValueNode.Values.Count > 0 &&
                propertyValueNode.Values[0] is XamlAstTextNode text)
            {
                var fieldModifier = TryGetAccess(objectNode);
                var typeName = $@"{clrType.Namespace}.{clrType.Name}";
                var resolvedName = new ResolvedName(typeName, text.Text, fieldModifier);
                if (_items.Contains(resolvedName))
                    continue;
                _items.Add(resolvedName);
            }
        }

        return node;
    }

    void IXamlAstVisitor.Push(IXamlAstNode node) { }

    void IXamlAstVisitor.Pop() { }

    private AccessLevel TryGetAccess(XamlAstObjectNode objectNode)
    {
        var accessText = objectNode
            .Children
            .OfType<XamlAstXamlPropertyValueNode>()
            .Where(prop => prop.Property is XamlAstNamePropertyReference propRef && propRef.Name == "Access")
            .Select(prop => prop.Values[0])
            .OfType<XamlAstTextNode>()
            .Select(txt => txt.Text)
            .FirstOrDefault();

        return accessText != null ? (AccessLevel)Enum.Parse(typeof(AccessLevel), accessText) : AccessLevel.Protected;
    }
}