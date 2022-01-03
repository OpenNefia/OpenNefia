using OpenNefia.Core.ContentPack;
using OpenNefia.Tests.Core.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Tests
{
    public abstract class ContentSerializationTest : SerializationTest
    {
        protected override Assembly[] Assemblies => new[] { AppDomain.CurrentDomain.GetAssemblyByName("OpenNefia.Content") };
    }
}
