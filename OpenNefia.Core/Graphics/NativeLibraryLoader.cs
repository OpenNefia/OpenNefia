using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace NativeLibraryLoader
{
    public class NativeLibrary
    {
        /// <summary>
        /// A list of all libraries loaded so far.
        /// </summary>
        public static List<NativeLibrary> LoadedLibraries = new List<NativeLibrary>();

        /// <summary>
        /// The filename of the library.
        /// </summary>
        public string LibraryName;
        /// <summary>
        /// The pointer to the library handle.
        /// </summary>
        public IntPtr LibraryHandle;

        // Windows
        [DllImport("kernel32")]
        internal static extern IntPtr LoadLibrary(string Filename);

        [DllImport("kernel32")]
        internal static extern IntPtr GetProcAddress(IntPtr Handle, string FunctionName);

        // Linux
        [DllImport("libdl.so")]
        internal static extern IntPtr dlopen(string filename, int flags);

        [DllImport("libdl.so")]
        internal static extern IntPtr dlsym(IntPtr Handle, string FunctionName);

        public static Platform? _platform;
        public static Platform Platform
        {
            get
            {
                if (_platform != null) return (Platform)_platform;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) _platform = Platform.Windows;
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) _platform = Platform.MacOS;
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) _platform = Platform.Linux;
                else _platform = Platform.Other;
                return (Platform)_platform;
            }
        }

        /// <summary>
        /// Opens the specified library.
        /// </summary>
        /// <param name="Library">The filename to the library to load.</param>
        /// <param name="PreloadLibraries">A list of any other libraries that must be loaded before loading this library.</param>
        public static NativeLibrary Load(string Library, params string[] PreloadLibraries)
        {
            return new NativeLibrary(Library, PreloadLibraries);
        }

        /// <summary>
        /// Opens the specified library.
        /// </summary>
        /// <param name="Library">The filename to the library to load.</param>
        /// <param name="PreloadLibraries">A list of any other libraries that must be loaded before loading this library.</param>
        protected NativeLibrary(string Library, params string[] PreloadLibraries)
        {
            foreach (string PreloadLibrary in PreloadLibraries)
            {
                if (LoadedLibraries.Find(l => l.LibraryName == PreloadLibrary) != null) continue;
                LoadedLibraries.Add(new NativeLibrary(PreloadLibrary));
            }
            LibraryName = Library;
            if (Platform == Platform.Windows) LibraryHandle = LoadLibrary(Library);
            else if (Platform == Platform.Linux) LibraryHandle = dlopen(Library, 0x102);
            else if (Platform == Platform.MacOS) throw new UnsupportedPlatformException();
            else throw new UnsupportedPlatformException();
            if (LibraryHandle == IntPtr.Zero) throw new LibraryLoadException(Library);
            LoadedLibraries.Add(this);
        }

        /// <summary>
        /// Returns a delegate bound to the target method in this library.
        /// </summary>
        /// <typeparam name="TDelegate">The delegate to bind the target method with.</typeparam>
        /// <param name="FunctionName">The name of the target function.</param>
        /// <returns>A delegate bound to the target method.</returns>
        public TDelegate GetFunction<TDelegate>(string FunctionName)
        {
            IntPtr funcaddr = IntPtr.Zero;
            if (Platform == Platform.Windows) funcaddr = GetProcAddress(LibraryHandle, FunctionName);
            else if (Platform == Platform.Linux) funcaddr = dlsym(LibraryHandle, FunctionName);
            else if (Platform == Platform.MacOS) throw new UnsupportedPlatformException();
            else throw new UnsupportedPlatformException();
            if (funcaddr == IntPtr.Zero) throw new InvalidEntryPointException(LibraryName, FunctionName);
            return Marshal.GetDelegateForFunctionPointer<TDelegate>(funcaddr);
        }

        /// <summary>
        /// Returns whether the given function exists in the library.
        /// </summary>
        /// <param name="FunctionName">The name of the target function.</param>
        /// <returns>Whether the method exists.</returns>
        public bool HasFunction(string FunctionName)
        {
            IntPtr funcaddr = IntPtr.Zero;
            if (Platform == Platform.Windows) funcaddr = GetProcAddress(LibraryHandle, FunctionName);
            else if (Platform == Platform.Linux) funcaddr = dlsym(LibraryHandle, FunctionName);
            else if (Platform == Platform.MacOS) throw new UnsupportedPlatformException();
            else throw new UnsupportedPlatformException();
            return funcaddr != IntPtr.Zero;
        }

        /// <summary>
        /// Represents an error with binding to a native function.
        /// </summary>
        public class InvalidEntryPointException : Exception
        {
            public InvalidEntryPointException(string Library, string FunctionName) : base($"No entry point by the name of '{FunctionName}' could be found in '{Library}'.") { }
        }

        /// <summary>
        /// Represents an error with the current platform not being supported.
        /// </summary>
        public class UnsupportedPlatformException : Exception
        {
            public UnsupportedPlatformException() : base("This platform is not supported.") { }
        }

        /// <summary>
        /// Represents an error with loading a library.
        /// </summary>
        public class LibraryLoadException : Exception
        {
            public LibraryLoadException(string Library) : base($"Failed to load library '{Library}'") { }
        }
    }

    public enum Platform
    {
        Windows,
        Linux,
        MacOS,
        Other
    }
}