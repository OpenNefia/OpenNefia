using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Xml.Linq;
using System.Linq;

namespace OpenNefia.XamlInjectors.Utilities
{
    static class OpenNefiaResourcesIndexReaderWriter
    {
        private const int LastKnownVersion = 1;
        public static List<OpenNefiaResourcesIndexEntry> Read(Stream stream)
        {
            var ver = new BinaryReader(stream).ReadInt32();
            if (ver > LastKnownVersion)
                throw new Exception("Resources index format version is not known");

            var assetDoc = XDocument.Load(stream);
            XNamespace assetNs = assetDoc.Root!.Attribute("xmlns")!.Value;
            List<OpenNefiaResourcesIndexEntry> entries =
                (from entry in assetDoc.Root.Element(assetNs + "Entries")!.Elements(assetNs + "AvaloniaResourcesIndexEntry")
                 select new OpenNefiaResourcesIndexEntry
                 {
                     Path = entry.Element(assetNs + "Path")!.Value,
                     Offset = int.Parse(entry.Element(assetNs + "Offset")!.Value),
                     Size = int.Parse(entry.Element(assetNs + "Size")!.Value)
                 }).ToList();

            return entries;
        }

        public static void Write(Stream stream, List<OpenNefiaResourcesIndexEntry> entries)
        {
            new BinaryWriter(stream).Write(LastKnownVersion);
            new DataContractSerializer(typeof(OpenNefiaResourcesIndex)).WriteObject(stream,
                new OpenNefiaResourcesIndex()
                {
                    Entries = entries
                });
        }

        public static byte[] Create(Dictionary<string, byte[]> data)
        {
            var sources = data.ToList();
            var offsets = new Dictionary<string, int>();
            var coffset = 0;
            foreach (var s in sources)
            {
                offsets[s.Key] = coffset;
                coffset += s.Value.Length;
            }
            var index = sources.Select(s => new OpenNefiaResourcesIndexEntry
            {
                Path = s.Key,
                Size = s.Value.Length,
                Offset = offsets[s.Key]
            }).ToList();
            var output = new MemoryStream();
            var ms = new MemoryStream();
            Write(ms, index);
            new BinaryWriter(output).Write((int)ms.Length);
            ms.Position = 0;
            ms.CopyTo(output);
            foreach (var s in sources)
            {
                output.Write(s.Value, 0, s.Value.Length);
            }

            return output.ToArray();
        }
    }

    [DataContract]
    class OpenNefiaResourcesIndex
    {
        [DataMember]
        public List<OpenNefiaResourcesIndexEntry> Entries { get; set; } = new List<OpenNefiaResourcesIndexEntry>();
    }

    [DataContract]
    class OpenNefiaResourcesIndexEntry
    {
        [DataMember]
        public string Path { get; set; }

        [DataMember]
        public int Offset { get; set; }

        [DataMember]
        public int Size { get; set; }
    }
}
