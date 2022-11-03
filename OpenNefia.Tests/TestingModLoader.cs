using System;
using System.IO;
using System.Reflection;
using OpenNefia.Core.ContentPack;
using OpenNefia.Core.Utility;

namespace OpenNefia.Tests
{
    internal sealed class TestingModLoader : BaseModLoader, IModLoaderInternal
    {
        public Assembly[] Assemblies { get; set; } = Array.Empty<Assembly>();

        public bool TryLoadModulesFrom(ResourcePath mountPath, string filterPrefix)
        {
            var rootID = 0;
            foreach (var assembly in Assemblies)
            {
                var manifest = new ModManifest(new ContentRootID(rootID++), assembly.GetName()!.Name!, assembly.GetName()!.Version!, new List<ModDependency>()); // TODO
                InitMod(manifest, assembly);
            }

            return true;
        }

        public void LoadGameAssembly(ModManifest manifest, Stream assembly, Stream? symbols = null, bool skipVerify = false)
        {
            throw new NotSupportedException();
        }

        public void LoadGameAssembly(ModManifest manifest, string diskPath, bool skipVerify = false)
        {
            throw new NotSupportedException();
        }

        public bool TryLoadAssembly(string assemblyName)
        {
            throw new NotSupportedException();
        }

        public void SetUseLoadContext(bool useLoadContext)
        {
            // Nada.
        }

        public void SetEnableSandboxing(bool sandboxing)
        {
            // Nada.
        }

        public Func<string, Stream?>? VerifierExtraLoadHandler { get; set; }

        public event ExtraModuleLoad? ExtraModuleLoaders;
    }
}
