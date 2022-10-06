using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Reflection;
using OpenNefia.Core.Timing;
using OpenNefia.Core.Utility;

namespace OpenNefia.Core.ContentPack
{
    public abstract class BaseModLoader
    {
        [Dependency] protected readonly IReflectionManager ReflectionManager = default!;

        private readonly List<ModuleTestingCallbacks> _testingCallbacks = new();

        /// <summary>
        ///     Loaded assemblies.
        /// </summary>
        protected readonly List<ModInfo> Mods = new();

        public IEnumerable<ModInfo> LoadedMods => Mods;
        public IEnumerable<Assembly> LoadedModAssemblies => Mods.Select(p => p.GameAssembly).WhereNotNull();

        public Assembly GetAssembly(string name)
        {
            return LoadedModAssemblies.Single(p => p.GetName().Name == name);
        }

        protected void InitMod(ModManifest manifest)
        {
            var mod = new ModInfo(manifest, null);
            Mods.Add(mod);
        }

        protected void InitMod(ModManifest manifest, Assembly assembly)
        {
            var assemblyVersion = assembly.GetName()?.Version;
            if (assemblyVersion == null)
            {
                throw new ArgumentException($"Content assembly {assembly} must have a version declared.", nameof(assembly));
            }

            if (manifest.Version != assemblyVersion)
            {
                throw new ArgumentException($"Content assembly {assembly} version {assemblyVersion} must match mod manifest version {manifest.Version}", nameof(assembly));
            }

            var mod = new ModInfo(manifest, assembly);

            ReflectionManager.LoadAssemblies(mod.GameAssembly!);

            var entryPoints = mod.GameAssembly!.GetTypes().Where(t => typeof(ModEntryPoint).IsAssignableFrom(t));

            foreach (var entryPoint in entryPoints)
            {
                var entryPointInstance = (ModEntryPoint) Activator.CreateInstance(entryPoint)!;
                if (_testingCallbacks != null)
                {
                    entryPointInstance.SetTestingCallbacks(_testingCallbacks);
                }

                mod.EntryPoints.Add(entryPointInstance);
            }

            Mods.Add(mod);
        }

        public bool IsContentAssembly(Assembly typeAssembly)
        {
            foreach (var mod in Mods)
            {
                if (mod.GameAssembly == typeAssembly)
                {
                    return true;
                }
            }

            return false;
        }

        public void BroadcastRunLevel(ModRunLevel level)
        {
            foreach (var mod in Mods)
            {
                foreach (var entry in mod.EntryPoints)
                {
                    switch (level)
                    {
                        case ModRunLevel.PreInit:
                            entry.PreInit();
                            break;
                        case ModRunLevel.Init:
                            entry.Init();
                            break;
                        case ModRunLevel.PostInit:
                            entry.PostInit();
                            break;
                        default:
                            Logger.ErrorS("res.mod", $"Unknown RunLevel: {level}");
                            break;
                    }
                }
            }
        }

        public void BroadcastUpdate(ModUpdateLevel level, FrameEventArgs frameEventArgs)
        {
            foreach (var module in Mods)
            {
                foreach (var entryPoint in module.EntryPoints)
                {
                    entryPoint.Update(level, frameEventArgs);
                }
            }
        }

        public void SetModuleBaseCallbacks(ModuleTestingCallbacks testingCallbacks)
        {
            _testingCallbacks.Add(testingCallbacks);
        }

        public void Shutdown()
        {
            foreach (var module in Mods)
            {
                foreach (var entryPoint in module.EntryPoints)
                {
                    entryPoint.Dispose();
                }
            }
        }
    }

    /// <summary>
    ///     Holds info about a loaded mod.
    /// </summary>
    public sealed class ModInfo
    {
        public ModInfo(ModManifest manifest, Assembly? gameAssembly)
        {
            Manifest = manifest;
            GameAssembly = gameAssembly;
            EntryPoints = new List<ModEntryPoint>();
            ExtraAssemblies = new List<Assembly>();
        }

        public ModManifest Manifest { get; }
        public Assembly? GameAssembly { get; }
        internal List<Assembly> ExtraAssemblies { get; }
        internal List<ModEntryPoint> EntryPoints { get; }
    }
}
