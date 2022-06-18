using System.Reflection;
using static OpenNefia.XamlInjectors.XamlCompiler;

namespace OpenNefia.Core.UserInterface.XAML.HotReload
{
    internal class XamlResource : IResource
    {
        private Type _classType;
        private Assembly _assembly;
        private string _xamlPath;

        public string Uri => $"resm:{Name}?assembly={_assembly.GetName().Name}";
        public string Name => $"{_classType.FullName}.xaml";
        public string FilePath => _xamlPath;
        public byte[] FileContents => File.ReadAllBytes(_xamlPath);

        public XamlResource(Type classType, string xamlPath)
        {
            _classType = classType;
            _assembly = classType.Assembly;
            _xamlPath = xamlPath;
        }

        public void Remove()
        {
        }
    }
}
