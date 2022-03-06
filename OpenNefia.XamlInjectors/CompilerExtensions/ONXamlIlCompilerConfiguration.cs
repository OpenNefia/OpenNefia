using XamlX.Transform;
using XamlX.TypeSystem;

namespace OpenNefia.XamlInjectors.CompilerExtensions
{
    class ONXamlIlCompilerConfiguration : TransformerConfiguration
    {
        public XamlIlTrampolineBuilder TrampolineBuilder { get; }

        public ONXamlIlCompilerConfiguration(IXamlTypeSystem typeSystem,
            IXamlAssembly defaultAssembly,
            XamlLanguageTypeMappings typeMappings,
            XamlXmlnsMappings xmlnsMappings,
            XamlValueConverter customValueConverter,
            XamlIlTrampolineBuilder trampolineBuilder,
            IXamlIdentifierGenerator identifierGenerator = null)
            : base(typeSystem, defaultAssembly, typeMappings, xmlnsMappings, customValueConverter, identifierGenerator)
        {
            TrampolineBuilder = trampolineBuilder;
            AddExtra(TrampolineBuilder);
        }
    }
}
