using System;
using System.Collections.Generic;
using System.Linq;
using OpenNefia.XamlNameGenerator.Compiler;
using OpenNefia.XamlNameGenerator.Domain;
using XamlX;
using XamlX.Ast;
using XamlX.Parsers;
using XamlX.TypeSystem;

namespace OpenNefia.XamlNameGenerator.Generator;

internal class XamlXViewResolver : IViewResolver, IXamlAstVisitor
{
    private readonly RoslynTypeSystem _typeSystem;
    private readonly MiniCompiler _compiler;
    private readonly bool _checkTypeValidity;
    private readonly Action<string> _onTypeInvalid;

    private ResolvedView _resolvedClass;
    private XamlDocument _xaml;

    public XamlXViewResolver(
        RoslynTypeSystem typeSystem,
        MiniCompiler compiler,
        bool checkTypeValidity = false,
        Action<string> onTypeInvalid = null)
    {
        _checkTypeValidity = checkTypeValidity;
        _onTypeInvalid = onTypeInvalid;
        _typeSystem = typeSystem;
        _compiler = compiler;
    }

    public ResolvedView ResolveView(string xaml)
    {
        _resolvedClass = null;
        _xaml = XDocumentXamlParser.Parse(xaml, new Dictionary<string, string>
        {
            {XamlNamespaces.Blend2008, XamlNamespaces.Blend2008}
        });

        _compiler.Transform(_xaml);
        _xaml.Root.Visit(this);
        _xaml.Root.VisitChildren(this);
        return _resolvedClass;
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
            if (child is XamlAstXamlPropertyValueNode property &&
                property.Property is XamlAstNamePropertyReference namedProperty &&
                namedProperty.Name == "Class" &&
                property.Values[0] is XamlAstTextNode text)
            {
                if (_checkTypeValidity)
                {
                    var existingType = _typeSystem.FindType(text.Text);
                    if (existingType == null)
                    {
                        _onTypeInvalid?.Invoke(text.Text);
                        return node;
                    }
                }

                var split = text.Text.Split('.');
                var nameSpace = string.Join(".", split.Take(split.Length - 1));
                var className = split.Last();

                _resolvedClass = new ResolvedView(className, clrType, nameSpace, _xaml);
                return node;
            }
        }

        return node;
    }

    void IXamlAstVisitor.Push(IXamlAstNode node) { }

    void IXamlAstVisitor.Pop() { }
}