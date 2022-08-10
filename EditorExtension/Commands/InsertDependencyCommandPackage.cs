using EnvDTE;
using EnvDTE80;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Design;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Document = Microsoft.CodeAnalysis.Document;
using Task = System.Threading.Tasks.Task;

namespace OpenNefia.EditorExtension
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(InsertDependencyCommandPackage.PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    public sealed class InsertDependencyCommandPackage : AsyncPackage
    {
        /// <summary>
        /// InsertDependencyCommandPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "7998893b-36dc-43f7-89b1-f1cac24256cd";

        /// <summary>
        /// Initializes a new instance of the <see cref="InsertDependencyCommandPackage"/> class.
        /// </summary>
        public InsertDependencyCommandPackage()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
        }

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
        /// <param name="progress">A provider for progress updates.</param>
        /// <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            Logger.Initialize(this, "InsertDependency");
            // When initialized asynchronously, the current thread may be a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            await InsertDependencyCommand.InitializeAsync(this);
        }

        public struct TypeName : IComparable<TypeName>
        {
            public TypeName(string fullName, string shortName)
            {
                FullName = fullName;
                ShortName = shortName;
            }

            public string FullName { get; }
            public string ShortName { get; }

            public override int GetHashCode()
            {
                return FullName.GetHashCode();
            }

            public int CompareTo(TypeName other)
            {
                return FullName.CompareTo(other.FullName);
            }
        }

        private List<TypeName> _insertableTypes = null;

        public async Task<List<TypeName>> GetInsertableTypes()
        {
            if (_insertableTypes != null)
                return _insertableTypes;

            var componentModel = (IComponentModel)Package.GetGlobalService(typeof(SComponentModel));
            var workspace = (Workspace)componentModel.GetService<VisualStudioWorkspace>();

            var result = await GetAllTypeSymbolsAsync();

            _insertableTypes = result.Select(e =>
            {
                var symbolDisplayFormat = new SymbolDisplayFormat(typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);
                return new TypeName(e.ToDisplayString(symbolDisplayFormat), e.Name);
            }).ToHashSet().ToList();

            _insertableTypes.Sort();
            return _insertableTypes;
        }

        private static async Task<List<INamedTypeSymbol>> GetAllTypeSymbolsAsync()
        {
            var componentModel = (IComponentModel)Package.GetGlobalService(typeof(SComponentModel));
            var workspace = (Workspace)componentModel.GetService<VisualStudioWorkspace>();

            var coreProject = workspace.CurrentSolution.Projects.First(p => p.Name == "OpenNefia.Core");
            var coreCompilation = await coreProject.GetCompilationAsync();
            // var iEntitySystemDef = coreCompilation.GetTypeByMetadataName("OpenNefia.Core.GameObjects.IEntitySystem");

            var result = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);

            foreach (var proj in workspace.CurrentSolution.Projects)
            {
                var comp = await proj.GetCompilationAsync();
                foreach (var type in GetAllTypes(comp))
                {
                    if (type.TypeKind == TypeKind.Interface && type.ContainingNamespace.ToString().StartsWith("OpenNefia.") /* && type.DerivesFromOrImplementsAnyConstructionOf(iEntitySystemDef) */)
                        result.Add(type);
                }
            }

            return result.ToList();
        }

        private static IEnumerable<INamedTypeSymbol> GetAllTypes(Compilation compilation) =>
            GetAllTypes(compilation.GlobalNamespace);

        private static IEnumerable<INamedTypeSymbol> GetAllTypes(INamespaceSymbol @namespace)
        {
            foreach (var type in @namespace.GetTypeMembers())
                foreach (var nestedType in GetNestedTypes(type))
                    yield return nestedType;

            foreach (var nestedNamespace in @namespace.GetNamespaceMembers())
                foreach (var type in GetAllTypes(nestedNamespace))
                    yield return type;
        }

        private static IEnumerable<INamedTypeSymbol> GetNestedTypes(INamedTypeSymbol type)
        {
            yield return type;
            foreach (var nestedType in type.GetTypeMembers()
                .SelectMany(nestedType => GetNestedTypes(nestedType)))
                yield return nestedType;
        }

        public T GetService<T>()
        {
            return (T)GetService(typeof(T));
        }

        public async Task<T> GetServiceAsync<T>()
        {
            var service = await GetServiceAsync(typeof(T));
            return (T)service;
        }

        #endregion
    }
}
