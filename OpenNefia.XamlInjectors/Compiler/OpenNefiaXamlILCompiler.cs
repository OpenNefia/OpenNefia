using System.Diagnostics;
using System.Linq;
using System.Net.Mime;
using XamlX.Ast;
using XamlX.Emit;
using XamlX.IL;
using XamlX.Transform;
using XamlX.TypeSystem;

namespace OpenNefia.XamlInjectors.Compiler
{
    /// <summary>
    /// Emitters & Transformers based on:
    /// - https://github.com/AvaloniaUI/Avalonia/blob/c85fa2b9977d251a31886c2534613b4730fbaeaf/src/Markup/Avalonia.Markup.Xaml.Loader/CompilerExtensions/Transformers/AvaloniaXamlIlRootObjectScopeTransformer.cs
    /// - https://github.com/AvaloniaUI/Avalonia/blob/c85fa2b9977d251a31886c2534613b4730fbaeaf/src/Markup/Avalonia.Markup.Xaml.Loader/CompilerExtensions/Transformers/AddNameScopeRegistration.cs
    /// - https://github.com/AvaloniaUI/Avalonia/blob/afb8ae6f3c517dae912729511483995b16cb31af/src/Markup/Avalonia.Markup.Xaml.Loader/CompilerExtensions/Transformers/IgnoredDirectivesTransformer.cs
    /// </summary>
    public partial class OpenNefiaXamlILCompiler : XamlILCompiler
    {
        private readonly IXamlType _contextType;

        private OpenNefiaXamlILCompiler(TransformerConfiguration configuration, XamlLanguageEmitMappings<IXamlILEmitter, XamlILNodeEmitResult> emitMappings)
           : base(configuration, emitMappings, true)
        {
            Transformers.Insert(0, new IgnoredDirectivesTransformer());

            Transformers.Add(new AddNameScopeRegistration());
            Transformers.Add(new OpenNefiaMarkRootObjectScopeNode());

            Emitters.Add(new AddNameScopeRegistration.Emitter());
            Emitters.Add(new OpenNefiaMarkRootObjectScopeNode.Emitter());
        }

        public OpenNefiaXamlILCompiler(TransformerConfiguration configuration,
            XamlLanguageEmitMappings<IXamlILEmitter, XamlILNodeEmitResult> emitMappings,
            IXamlTypeBuilder<IXamlILEmitter> contextTypeBuilder)
    : this(configuration, emitMappings)
        {
            _contextType = CreateContextType(contextTypeBuilder);
        }


        public OpenNefiaXamlILCompiler(TransformerConfiguration configuration,
            XamlLanguageEmitMappings<IXamlILEmitter, XamlILNodeEmitResult> emitMappings,
            IXamlType contextType) : this(configuration, emitMappings)
        {
            _contextType = contextType;
        }

        public void OverrideRootType(XamlDocument doc, IXamlAstTypeReference newType)
        {
            var root = (XamlAstObjectNode)doc.Root;
            var oldType = root.Type;
            if (oldType.Equals(newType))
                return;

            root.Type = newType;
            foreach (var child in root.Children.OfType<XamlAstXamlPropertyValueNode>())
            {
                if (child.Property is XamlAstNamePropertyReference prop)
                {
                    if (prop.DeclaringType.Equals(oldType))
                        prop.DeclaringType = newType;
                    if (prop.TargetType.Equals(oldType))
                        prop.TargetType = newType;
                }
            }
        }
    }
}
