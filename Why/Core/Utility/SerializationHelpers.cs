using OpenNefia.Core.ContentPack;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Reflection;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Markdown;
using OpenNefia.Core.Utility;
using YamlDotNet.RepresentationModel;

namespace OpenNefia.Core.Utility
{
    public static class SerializationHelpers
    {
        /// <summary>
        /// Dirty hack to deserialize something through the <see cref="DataDefinition"/> system.
        /// Remove once binary serialization is implemented.
        /// </summary>
        public static T? Deserialize<T>(ResourcePath file, ISerializationContext? context = null, bool skipHook = false) where T : class
        {
            var resources = IoCManager.Resolve<IResourceManager>();
            var reflection = IoCManager.Resolve<IReflectionManager>();
            var serialization = IoCManager.Resolve<ISerializationManager>();

            var reader = new StreamReader(resources.UserData.Open(file, FileMode.Open), EncodingHelpers.UTF8);

            var yamlStream = new YamlStream();
            yamlStream.Load(reader);
            var document = yamlStream.Documents[0];

            var node = (YamlMappingNode)document.RootNode;

            var res = serialization.Read(typeof(T), node.ToDataNode(), skipHook: true);

            return res.RawValue as T;
        }

        /// <summary>
        /// Dirty hack to serialize something through the <see cref="DataDefinition"/> system.
        /// Remove once binary serialization is implemented.
        /// </summary>
        public static void Serialize<T>(ResourcePath file, T obj, bool alwaysWrite = false, ISerializationContext? context = null) where T : class
        {
            var resources = IoCManager.Resolve<IResourceManager>();
            var serialization = IoCManager.Resolve<ISerializationManager>();

            var node = serialization.WriteValue(typeof(T), obj, alwaysWrite, context);

            var stream = resources.UserData.Open(file, FileMode.Create);
            var document = new YamlDocument(node.ToYamlNode());
            var yamlStream = new YamlStream(document);

            using (StreamWriter writer = new StreamWriter(stream, EncodingHelpers.UTF8))
            {
                yamlStream.Save(writer);
            }
        }
    }
}
