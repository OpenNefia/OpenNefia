using OpenNefia.XamlInjectors.CompilerExtensions.Transformers;
using System.Diagnostics;
using XamlX.Emit;
using XamlX.IL;
using XamlX.Transform;

namespace OpenNefia.XamlInjectors.CompilerExtensions
{
    /// <summary>
    /// Emitters & Transformers based on:
    /// - https://github.com/AvaloniaUI/Avalonia/blob/c85fa2b9977d251a31886c2534613b4730fbaeaf/src/Markup/Avalonia.Markup.Xaml.Loader/CompilerExtensions/Transformers/AvaloniaXamlIlRootObjectScopeTransformer.cs
    /// - https://github.com/AvaloniaUI/Avalonia/blob/c85fa2b9977d251a31886c2534613b4730fbaeaf/src/Markup/Avalonia.Markup.Xaml.Loader/CompilerExtensions/Transformers/AddNameScopeRegistration.cs
    /// - https://github.com/AvaloniaUI/Avalonia/blob/afb8ae6f3c517dae912729511483995b16cb31af/src/Markup/Avalonia.Markup.Xaml.Loader/CompilerExtensions/Transformers/IgnoredDirectivesTransformer.cs
    /// </summary>
    public class OpenNefiaXamlILCompiler : XamlILCompiler
    {
        public OpenNefiaXamlILCompiler(TransformerConfiguration configuration, XamlLanguageEmitMappings<IXamlILEmitter, XamlILNodeEmitResult> emitMappings, bool fillWithDefaults) : base(configuration, emitMappings, fillWithDefaults)
        {
            Transformers.Insert(0, new IgnoredDirectivesTransformer());

            Transformers.Add(new AddNameScopeRegistration());
            Transformers.Add(new OpenNefiaMarkRootObjectScopeNode());

            Emitters.Add(new AddNameScopeRegistration.Emitter());
            Emitters.Add(new OpenNefiaMarkRootObjectScopeNode.Emitter());
        }
    }
}
