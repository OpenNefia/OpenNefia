using System;
using System.Reflection;
using NUnit.Framework;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Reflection;
using OpenNefia.Core.Serialization.Manager;

namespace OpenNefia.Tests.Core.Serialization
{
    public abstract class SerializationTest : OpenNefiaUnitTest
    {
        protected IReflectionManager Reflection => IoCManager.Resolve<IReflectionManager>();
        protected ISerializationManager Serialization => IoCManager.Resolve<ISerializationManager>();

        protected virtual Assembly[] Assemblies => Array.Empty<Assembly>();

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            Reflection.LoadAssemblies(Assemblies);
            Serialization.Initialize();
        }
    }
}
