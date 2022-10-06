using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using OpenNefia.Core.Timing;
using OpenNefia.Core.Utility;

namespace OpenNefia.Core.ContentPack
{
    /// <summary>
    ///     The mod loader is in charge of loading content assemblies and managing them.
    /// </summary>
    public interface IModLoader
    {
        /// <summary>
        ///     All directly loaded mods.
        /// </summary>
        IEnumerable<ModInfo> LoadedMods { get; }

        /// <summary>
        ///     All directly loaded content assemblies.
        /// </summary>
        IEnumerable<Assembly> LoadedModAssemblies { get; }

        Assembly GetAssembly(string name);

        /// <summary>
        ///     Adds a testing callbacks that will be passed to <see cref="ModEntryPoint.SetTestingCallbacks"/>.
        /// </summary>
        void SetModuleBaseCallbacks(ModuleTestingCallbacks testingCallbacks);

        bool IsContentAssembly(Assembly typeAssembly);
    }

    internal delegate Assembly? ExtraModuleLoad(AssemblyName name);

    internal interface IModLoaderInternal : IModLoader
    {
        /// <summary>
        ///     Loads all content assemblies from the specified resource directory and filename prefix.
        /// </summary>
        /// <param name="mountPath">The directory in which to look for assemblies.</param>
        /// <param name="filterPrefix">The prefix files need to have to be considered. e.g. <c>Content.</c></param>
        /// <returns>True if all modules loaded successfully. False if there were load errors.</returns>
        bool TryLoadModulesFrom(ResourcePath mountPath, string filterPrefix);

        /// <summary>
        ///     Loads an assembly into the current AppDomain.
        /// </summary>
        /// <param name="assembly">Byte array of the assembly.</param>
        /// <param name="symbols">Optional byte array of the debug symbols.</param>
        /// <param name="skipVerify">Whether to skip checking the loaded assembly for sandboxing.</param>
        void LoadGameAssembly(ModManifest manifest, Stream assembly, Stream? symbols = null, bool skipVerify = false);

        /// <summary>
        ///     Loads an assembly into the current AppDomain.
        /// </summary>
        void LoadGameAssembly(ModManifest manifest, string diskPath, bool skipVerify = false);

        /// <summary>
        ///     Broadcasts a run level change to all loaded entry point.
        /// </summary>
        /// <param name="level">New level</param>
        void BroadcastRunLevel(ModRunLevel level);

        void BroadcastUpdate(ModUpdateLevel level, FrameEventArgs frameEventArgs);

        void SetUseLoadContext(bool useLoadContext);

        Func<string, Stream?>? VerifierExtraLoadHandler { get; set; }

        void Shutdown();
        event ExtraModuleLoad ExtraModuleLoaders;
    }
}
